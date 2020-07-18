using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.EC2;
using Amazon.EC2.Model;
using Veeam.CloudBackup.Common;
using Veeam.CloudBackup.Model.Amazon;

namespace Veeam.CloudBackup.AmazonApi
{
    internal static class SEc2MachineRequestFactory
    {
        public const Int32 MaxResults = 1000;

        public static DescribeInstancesRequest GetAllInstances(Int32 maxResult)
        {
            var request = new DescribeInstancesRequest
            {
                MaxResults = maxResult
            };
            return request;
        }

        public static DescribeVolumesRequest GetAllVolumes(Int32 maxResult)
        {
            var request = new DescribeVolumesRequest
            {
                MaxResults = maxResult
            };
            return request;
        }

        public static DescribeSnapshotsRequest GetAllSnapshots(Int32 maxResult)
        {
            var request = new DescribeSnapshotsRequest
            {
                MaxResults = maxResult
            };
            request.OwnerIds.Add("self");
            return request;
        }

        public static DescribeVolumesRequest GetVolumes(IReadOnlyList<String> volumeIds)
        {
            var request = new DescribeVolumesRequest();

            if (volumeIds.Count > 0)
                request.VolumeIds = volumeIds.ToList();

            return request;
        }

        public static DescribeInstancesRequest FindInstance(String instanceId)
        {
            var request = new DescribeInstancesRequest();

            if (instanceId.IsSpecified())
                request.InstanceIds = new List<String> { instanceId };

            return request;
        }

        public static GetLaunchTemplateDataRequest GetLaunchTemplateData(String instanceId)
        {
            var request = new GetLaunchTemplateDataRequest();

            if (instanceId.IsSpecified())
                request.InstanceId = instanceId;

            return request;
        }

        public static DescribeInstancesRequest GetRunning()
        {
            String stateCode = ((Int32) EEc2MachineState.Running).ToString();

            var request = new DescribeInstancesRequest
            {
                Filters = new List<Filter> { new Filter("instance-state-code", new List<String> { stateCode }) }
            };

            return request;
        }

        public static StartInstancesRequest Start(String instanceId)
        {
            return new StartInstancesRequest(new List<String> { instanceId });
        }

        public static StopInstancesRequest Stop(String instanceId, Boolean force)
        {
            return new StopInstancesRequest(new List<String> { instanceId }) { Force = force };
        }

        public static TerminateInstancesRequest Terminate(String instanceId)
        {
            return new TerminateInstancesRequest(new List<String> { instanceId });
        }

        public static DescribeImagesRequest FindImage(String imageId)
        {
            return new DescribeImagesRequest { ImageIds = new List<String> { imageId } };
        }

        public static DeregisterImageRequest DeleteImage(String imageId)
        {
            return new DeregisterImageRequest(imageId);
        }

        public static ImportImageRequest CreateImportImageRequest(CEc2ImportImageSpec spec)
        {
            List<ImageDiskContainer> disks = spec.Disks.Select(ToAwsDiskContainer).ToList();
            disks.AddRange(spec.Snapshots.Select(ToAwsDiskContainer));

            var request = new ImportImageRequest
            {
                LicenseType = "BYOL",
                DiskContainers = disks,
                Platform = spec.Platform.Value,
                Architecture = "x86_64"
            };

            if (spec.Platform == CEc2PlatformType.Windows && spec.License == EPublicCloudLicenseType.PublicCloud)
                request.LicenseType = "AWS";

            return request;
        }

        private static ImageDiskContainer ToAwsDiskContainer(CS3DiskInfo diskInfo)
        {
            return new ImageDiskContainer
            {
                Format = diskInfo.Format,
                DeviceName = diskInfo.DeviceName,
                Url = diskInfo.DiskUrl
            };
        }

        private static ImageDiskContainer ToAwsDiskContainer(KeyValuePair<String, String> snapshotMap)
        {
            return new ImageDiskContainer
            {
                SnapshotId = snapshotMap.Key,
                DeviceName = snapshotMap.Value
            };
        }

        public static DescribeImportImageTasksRequest DescribeImportImageTasks(params String[] imageTasksIds)
        {
            if (imageTasksIds.Length > 0)
            {
                IEnumerable<String> importAmiIds = imageTasksIds.Where(id => SAmazonIdHelper.ResolveConversionTaskType(id) == EAmazonConversionTaskType.ImportImageTask);

                if (!importAmiIds.Any())
                    return null;

                return new DescribeImportImageTasksRequest { ImportTaskIds = imageTasksIds.ToList() };
            }

            return new DescribeImportImageTasksRequest();
        }

        public static DetachVolumeRequest DetachVolume(String volumeId, Boolean force)
        {
            return new DetachVolumeRequest(volumeId) {Force = force};
        }

        public static RunInstancesRequest RunInstances(CVpcLaunchContext context)
        {
            var request = new RunInstancesRequest();
            context.FillRequest(request);
            return request;
        }

        public static void FillRequest(this CVpcLaunchContext context, RunInstancesRequest request)
        {
            request.ImageId = context.ImageId;
            request.MinCount = 1;
            request.MaxCount = 1;
            request.InstanceType = new InstanceType(context.MachineType);

            if (context.KeyPairName.IsSpecified())
                request.KeyName = context.KeyPairName;

            List<InstanceNetworkInterfaceSpecification> networkInterfaceSpecs = context.NetworkInterfaces.Select(interfaceContext => interfaceContext.ToEc2InterfaceSpecification()).ToList();

            request.NetworkInterfaces = networkInterfaceSpecs;

            List<BlockDeviceMapping> devMappings = context.DeviceMappings.Select(map => map.ToEc2BlockDeviceMapping()).ToList();

            if (devMappings.Count > 0)
                request.BlockDeviceMappings = devMappings;

            if (context.UserData.IsSpecified())
                request.UserData = context.UserData;

            context.AdditionalContext?.FillRequest(request);

            if(context.TerminateAtShutdown)
                request.InstanceInitiatedShutdownBehavior = ShutdownBehavior.Terminate;

            if (!String.IsNullOrEmpty(context.InstanceProfile))
                request.IamInstanceProfile = new IamInstanceProfileSpecification { Name = context.InstanceProfile };
        }

        private static void FillRequest(this CEc2MachineLaunchTemplateInfo context, RunInstancesRequest request)
        {
            context.ShutdownBehavior.Do(value => request.InstanceInitiatedShutdownBehavior = ShutdownBehavior.FindValue(value));
            context.DisableApiTermination.Do(termination => request.DisableApiTermination = termination);
            context.MonitoringEnabled.Do(monitoring => request.Monitoring = monitoring);
            context.EbsOptimized.Do(optimized => request.EbsOptimized = optimized);

            if (context.KernelId.IsSpecified())
                request.KernelId = context.KernelId;

            if (context.RamDiskId.IsSpecified())
                request.RamdiskId = context.RamDiskId;
        }

        public static DescribeImagesRequest GetImagesByName(IReadOnlyCollection<String> names, CAmazonImageSearchOptions searchOptions)
        {
            var request = new DescribeImagesRequest();

            request.Filters.Add(new Filter("name", names.ToList()));
            request.Filters.Add(new Filter("image-type", new List<String> { "machine" }));
            request.Filters.Add(new Filter("state", new List<String> { "available" }));

            if (searchOptions.UseOnlyAmazonImages)
                request.Filters.Add(new Filter("owner-alias", new List<String> { "amazon" }));

            if (searchOptions.UseOnyEbsImages)
                request.Filters.Add(new Filter("root-device-type", new List<String> { "ebs" }));

            if (searchOptions.UseOnlyHvmImages)
                request.Filters.Add(new Filter("virtualization-type", new List<String> { "hvm" }));

            return request;
        }
        
        public static DescribeConversionTasksRequest DescribeConversionTasks(params String[] conversionTaskIds)
        {
            if (conversionTaskIds.Length > 0)
            {
                IEnumerable<String> convTaskIds = conversionTaskIds.Where(taskId => SAmazonIdHelper.ResolveConversionTaskType(taskId) == EAmazonConversionTaskType.ImportVolumeTask);

                if (!convTaskIds.Any())
                    return null;

                return new DescribeConversionTasksRequest { ConversionTaskIds = conversionTaskIds.ToList() };
            }

            return new DescribeConversionTasksRequest();
        }

        public static CancelConversionTaskRequest CancelConversionTask(String conversionTaskId, String reason)
        {
            var request = new CancelConversionTaskRequest { ConversionTaskId = conversionTaskId };

            if (reason.IsSpecified())
                request.ReasonMessage = reason;

            return request;
        }

        public static CancelImportTaskRequest CancelImportTask(String importTaskId, String reason)
        {
            var request = new CancelImportTaskRequest { ImportTaskId = importTaskId };

            if (reason.IsSpecified())
                request.CancelReason = reason;

            return request;
        }
    }
}
