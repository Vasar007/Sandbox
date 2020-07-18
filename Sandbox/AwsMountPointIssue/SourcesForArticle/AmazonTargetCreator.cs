using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.EC2;
using Veeam.CloudBackup.Amazon.Common;
using Veeam.CloudBackup.AmazonApi;
using Veeam.CloudBackup.Common;
using Veeam.CloudBackup.Configuration;
using Veeam.CloudBackup.DBManager.Common;
using Veeam.CloudBackup.Jobs;
using Veeam.CloudBackup.Logging;
using Veeam.CloudBackup.Model;
using Veeam.CloudBackup.Model.Amazon;
using Veeam.CloudBackup.Utils;

namespace Veeam.CloudBackup.Amazon.Jobs
{
    internal sealed class CAmazonTargetCreator
    {
        private static readonly CPrefixLogger Logger = CPrefixLogger.Create(nameof(CAmazonTargetCreator));

        private readonly ITempResourceRegistrator _resourceRegistrator;
        private readonly CSession _session;
        private readonly IDictionary<String, CEc2MachineDeletingSpec> _deletingSpecs = new Dictionary<String, CEc2MachineDeletingSpec>();
        private readonly CAmazonInfrastructure _awsInfrastructure;

        public CAmazonTargetCreator(CAmazonInfrastructure awsInfrastructure, ITempResourceRegistrator resourceRegistrator, CSession session)
        {
            _awsInfrastructure = awsInfrastructure;
            _resourceRegistrator = resourceRegistrator;
            _session = session;
        }

        public async Task<CEc2MachineInfo> CreateTargetMachineAsync(CRestoreTaskSpec restoreTaskSpec, CAmazonTargetCreatorMachineSpec machineSpec)
        {
            Logger.Message("Creating target instance.");

            CEc2MachineInfo machineInfo;
            if (restoreTaskSpec.RestoreToOriginalLocation)
            {
                Logger.Message("Creating new instance because of restore to original location.");
                machineInfo = await CreateTargetMachine(machineSpec);
            }
            else
            {
                Logger.Message("Creating instance because of restore to different location.");
                machineInfo = await CreateMachineFromRestoreTaskSpec(restoreTaskSpec, machineSpec);
            }

            return machineInfo;
        }

        public async Task AttachDisksAsync(CAmazonTargetCreatorVolumesSpec volumesSpec)
        {
            try
            {
                await AttachDisksInternal(volumesSpec);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, "Attaching disks failed.");
                await DeleteMachineAsync(volumesSpec.Machine);
                throw;
            }
        }

        public async Task DeleteMachineAsync(CEc2MachineInfo machineInfo)
        {
            if (!_deletingSpecs.TryGetValue(machineInfo.Id, out CEc2MachineDeletingSpec deletingSpec))
                throw new ArgumentException($"[TBD] Cannot find deleting spec for specified machine '{machineInfo.Id}'.", nameof(machineInfo));

            await DeleteMachineInternal(machineInfo.Id, deletingSpec);
        }

        public async Task DeleteMachineByIdAndAttachedVolumesAsync(String instanceId)
        {
            CEc2MachineInfo machineInfo = await _awsInfrastructure.GetEc2().Machines.FindInstanceAsync(instanceId);

            if (machineInfo == null)
            {
                Logger.Message($"Instance '{instanceId}' was not found in EC2. Instance and attached volumes would not be deleted.");
                return;
            }

            Logger.Message($"Found instance '{instanceId}' in EC2. Instance would be deleted.");

            IReadOnlyList<CEc2VolumeInfo> attachedVolumes = await _awsInfrastructure.GetEc2().Volumes.GetVolumesByInstanceIdAsync(instanceId);
            IReadOnlyList<String> volumeIds = attachedVolumes.Select(volume => volume.Id).ToList();

            if (volumeIds.Count > 0)
                Logger.Message($"Found attached to instance '{instanceId}' volumes in EC2 [{volumeIds.EnumerableToLogString()}]. Attached volumes would be deleted.");

            CEc2MachineDeletingSpec deletingSpec = CEc2MachineDeletingSpec.Create(
                machine: machineInfo,
                keyPair: null,
                subnet: null,
                volumeIds: volumeIds,
                ami: null,
                instanceRole: null,
                trackedResources: ETrackedResourceFlags.Instance
            );

            await DeleteMachineInternal(instanceId, deletingSpec);

            foreach (String volumeId in volumeIds)
                await SDbManager.Instance.CloudResources.SetDeletedByResourceIAsync(volumeId);
        }

        private async Task DeleteMachineInternal(String instanceId, CEc2MachineDeletingSpec deletingSpec)
        {
            var machineCreator = new CAwsMachineCreator(_awsInfrastructure);

            await machineCreator.DeleteMachineAndEnvironmentAsync(deletingSpec);
            await SDbManager.Instance.CloudResources.SetDeletedByResourceIAsync(instanceId);
        }

        private async Task<CEc2MachineInfo> CreateMachineFromRestoreTaskSpec(CRestoreTaskSpec restoreTaskSpec, CAmazonTargetCreatorMachineSpec originalSpec)
        {
            CAmazonTargetCreatorMachineSpec targetCreatorMachineSpec = CAmazonTargetCreatorMachineSpec.Create(restoreTaskSpec, originalSpec);
            CEc2MachineInfo machineInfo = await CreateTargetMachine(targetCreatorMachineSpec);

            return machineInfo;
        }

        private async Task<CEc2MachineInfo> CreateTargetMachine(CAmazonTargetCreatorMachineSpec machineSpec)
        {
            using (CSessionLogLine logLine = _session.Logger.CreateProgress(SAmazonRestoreStrings.CreatingTargetVm))
            {
                CEc2MachineInfo machineInfo = await InternalCreateTargetMachine(machineSpec);

                logLine.Success();

                return machineInfo;
            }
        }

        private async Task<CEc2MachineInfo> InternalCreateTargetMachine(CAmazonTargetCreatorMachineSpec machineSpec)
        {
            var machineCreator = new CAwsMachineCreator(_awsInfrastructure);
            
            CEc2ImageInfo image = await _awsInfrastructure.GetEc2().Machines.FindImageAsync(machineSpec.ImageId);

            if (image == null)
            {
                IReadOnlyList<CEc2ImageInfo> images = await _awsInfrastructure.GetEc2().Machines.GetImagesByNamesAsync(new[] {machineSpec.ImageName}, new CAmazonImageSearchOptions());
                machineSpec.ImageId = images.FirstOrDefault()?.Id;
            }

            if (image == null)
            {
                CAmazonProxyImageInfo proxyImage = await SAmazonImageProvider.GetAwsProxyImageAsync(_awsInfrastructure.GetEc2());
                machineSpec.ImageId = proxyImage?.ImageId;
            }

            CEc2KeyPairIdentifier key = await _awsInfrastructure.GetEc2().Security.FindKeyPairAsync(machineSpec.KeyName);
            if (key == null)
                machineSpec.KeyName = String.Empty;
            
            var creatorSpec = new CEc2MachineSpecWithSubnet(machineSpec.ImageId, machineSpec.Name, machineSpec.Type, machineSpec.KeyName, true,null, ETrackedResourceFlags.Instance, false);
            creatorSpec.Interfaces.Add(new CEc2NetworkInterfaceContext(machineSpec.SubnetId, machineSpec.SecurityGroups, 0, true));

            foreach (CEc2TagInfo tag in machineSpec.Tags)
                creatorSpec.Tags.Add(tag);

            CEc2MachineCreationResult result = await machineCreator.CreateAndRunAsync(creatorSpec);

            if (result.IsFailed)
            {
                Logger.Exception(result.Error, "Cannot create target VM.");

                await machineCreator.DeleteMachineAndEnvironmentAsync(result.DeletingSpec);
                throw ExceptionFactory.Create($"[TBD] Cannot create target VM: {result.Error.GetFirstChanceException().Message}");
            }

            await _resourceRegistrator.RegisterAsync(result.Machine.Id, ECloudResourceType.Instance, _session.LeaseId, result.DeletingSpec.Serial());

            CEc2MachineInfo machineInfo = result.Machine;
            _deletingSpecs[machineInfo.Id] = result.DeletingSpec;

            await StopMachineAsync(machineInfo);
            return machineInfo;
        }

        private async Task StopMachineAsync(CEc2MachineInfo machineInfo)
        {
            Logger.Message("Stopping machine.");

            await _awsInfrastructure.GetEc2().Machines.StopInstanceAsync(machineInfo.Id);
            TimeSpan startStopTimeout = TimeSpan.FromMinutes(10);
            await CEc2StateAwaiter.WaitMachineOrThrowAsync(_awsInfrastructure.GetEc2(), machineInfo.Id, startStopTimeout, EEc2MachineState.Stopped);
        }

        private async Task AttachDisksInternal(CAmazonTargetCreatorVolumesSpec volumesSpec)
        {
            Logger.Message("Attaching disks.");

            IReadOnlyList<CEc2VolumeInfo> volumesToDelete = await _awsInfrastructure.GetEc2().Volumes.GetVolumesByInstanceIdAsync(volumesSpec.Machine.Id);
            if (!volumesSpec.DetachAllOriginalVolumes)
                volumesToDelete = volumesToDelete.Where(volume => volumesSpec.DisksMapping.ContainsKey(volume.Device)).ToList();

            await Task.WhenAll(volumesToDelete.Select(volume => DetachAndDeleteIfNeeded(volume.Id, volumesSpec.DeleteOriginalVolumes)));

            await Task.WhenAll(volumesSpec.DisksMapping.Select(disk => Attach(disk.Key, disk.Value, volumesSpec.Machine)));

            if (volumesSpec.StartMachineAfterAttach)
                await _awsInfrastructure.GetEc2().Machines.StartInstanceAsync(volumesSpec.Machine.Id);
        }

        private async Task Attach(String device, String volumeId, CEc2MachineInfo machine)
        {
            SLogAsyncWorkflowId.Reset();

            CEc2VolumeInfo volume = await _awsInfrastructure.GetEc2().Volumes.GetVolumeAsync(volumeId);

            if (volume.State != EEc2VolumeState.Available)
            {
                String awaitingMessage = String.Format(SAmazonRestoreStrings.AwaitingVolume, volume.Id, EEc2VolumeState.Available.ToString());
                using (CSessionLogLine awaitingLine = _session.Logger.CreateProgress(awaitingMessage))
                {
                    TimeSpan timeout = SOptions.GetOptions<CAmazonApiOptions>().AwsVolumesTimeout;
                    await CEc2StateAwaiter.WaitVolumeOrThrowAsync(_awsInfrastructure.GetEc2(), volume.Id, timeout, EEc2VolumeState.Available);

                    awaitingLine.Success();
                }
            }

            try
            {
                await _awsInfrastructure.GetEc2().Machines.AttachVolumeAsync(device, volume.Id, machine.Id);
            }
            catch (AmazonEC2Exception ex) when (ex.ErrorCode.Contains(SMountPointFinder.MountPointAlreadyInUseErrorCode, StringComparison.InvariantCultureIgnoreCase))
            {
                // Some Linux images have reserved mount points and this can cause errors during volumes attaching to instance.
                Logger.Warning(ex, $"Exception occured during volume '{volume.Id}' attaching to instance '{machine.Id}', device '{device}'. Can handle such exception.");

                Boolean result = await TrySetNewMountPoint(device, volume.Id, machine);
                if (!result)
                    throw;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, $"Exception occured during volume '{volume.Id}' attaching to instance '{machine.Id}', device '{device}'. Cannot handle such exception.");
                throw;
            }
        }

        private async Task DetachAndDeleteIfNeeded(String volumeId, Boolean deleteVolume)
        {
            SLogAsyncWorkflowId.Reset();

            await _awsInfrastructure.GetEc2().Machines.DetachVolumeAsync(volumeId, true);

            TimeSpan timeout = SOptions.GetOptions<CAmazonApiOptions>().AwsVolumesTimeout;

            await CEc2StateAwaiter.WaitVolumeOrThrowAsync(_awsInfrastructure.GetEc2(), volumeId, timeout, EEc2VolumeState.Available);

            if (deleteVolume)
            {
                await _awsInfrastructure.GetEc2().Volumes.DeleteVolumeAsync(volumeId);
                await SDbManager.Instance.CloudResources.SetDeletedByResourceIAsync(volumeId);
            }
        }

        private async Task<Boolean> TrySetNewMountPoint(String oldDevice, String volumeId, CEc2MachineInfo machine)
        {
            Logger.Warning("Trying to find other mount point to attach.");

            String newDevice = "[None]";
            try
            {
                IReadOnlyList<CEc2VolumeInfo> ec2Volumes = await _awsInfrastructure.GetEc2().Volumes.GetVolumesByInstanceIdAsync(machine.Id);
                newDevice = SMountPointFinder.GetMountPoint(machine, ec2Volumes);

                await _awsInfrastructure.GetEc2().Machines.AttachVolumeAsync(newDevice, volumeId, machine.Id);

                String message = $"[TBD] Volume '{volumeId}' was attached to '{newDevice}' instead of '{oldDevice}' for instance '{machine.Id}'.";
                _session.Logger.AddWarning(message);
                Logger.Warning(message);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, $"Cannot set new mount point. Old device: '{oldDevice}', new device: '{newDevice}', Volume: '{volumeId}', Instance: '{machine.Id}'.");
                return false;
            }
        }
    }
}
