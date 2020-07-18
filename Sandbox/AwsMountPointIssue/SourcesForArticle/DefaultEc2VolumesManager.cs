using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.EC2;
using Amazon.EC2.Model;
using Veeam.CloudBackup.Common;
using Veeam.CloudBackup.Logging;
using Veeam.CloudBackup.Model.Amazon;

namespace Veeam.CloudBackup.AmazonApi
{
    internal sealed class CDefaultEc2VolumesManager : IEc2VolumesManager
    {
        private static readonly String LogTitle = $"[{nameof(CDefaultEc2VolumesManager)}]";

        private readonly CAmazonClientFactory _clientFactory;

        internal CDefaultEc2VolumesManager(CAmazonClientFactory clientFactory)
        {
            Log.Message(LogLevels.HighDetailed, $"{LogTitle} Creating {nameof(CDefaultEc2VolumesManager)} for account '{clientFactory.ProfileName}'.");
            _clientFactory = clientFactory;
        }

        #region Implementation of IEc2VolumesManager

        public async Task<CEc2VolumeInfo> FindVolumeAsync(String volumeId)
        {
            Log.Message($"{LogTitle} Getting volumes by ID: '{volumeId}'.");

            DescribeVolumesRequest request = SEc2VolumesRequestFactory.FindVolume(volumeId);

            return (await InternalGetVolumes(request)).FirstOrDefault(x => x.VolumeId.EqualsNoCase(volumeId))?.CreateVolumeInfo();
        }

        public async Task<CEc2VolumeInfo> GetVolumeAsync(String volumeId)
        {
            CEc2VolumeInfo info = await FindVolumeAsync(volumeId);
            if (info == null)
                throw new CNotFoundException($"[TBD] Cannot find volume: '{volumeId}'.");

            return info;
        }

        public async Task<IReadOnlyList<CEc2VolumeInfo>> GetVolumesByInstanceIdAsync(String instanceId)
        {
            Log.Message($"{LogTitle} Getting volumes by instance ID: '{instanceId}'.");

            DescribeVolumesRequest request = SEc2VolumesRequestFactory.GetVolumesByInstanceId(instanceId);

            return (await InternalGetVolumes(request)).Select(x => x.CreateVolumeInfo()).ToList();
        }

        public async Task<CEc2VolumeInfo> CreateVolumeAsync(CEc2VolumeCreationSpec spec)
        {
            Log.Message($"{LogTitle} Creating volume. Spec: {spec}");

            using (IAmazonEC2 client = _clientFactory.CreateEc2Client())
            {
                CreateVolumeRequest request = SEc2VolumesRequestFactory.CreateVolume(spec);

                Func<Task<CreateVolumeResponse>> func = async () => await client.CreateVolumeAsync(request);

                CreateVolumeResponse response = await func.TryInvokeAws();

                Log.Message(LogLevels.HighDetailed, $"{LogTitle} Volume '{response.Volume.VolumeId}' has been created at {response.Volume.CreateTime}.");

                return response.Volume.CreateVolumeInfo();
            }
        }

        public async Task<CEc2VolumeInfo> CreateVolumeWithTagsAsync(CEc2VolumeCreationSpec spec, IReadOnlyCollection<CEc2TagInfo> tags)
        {
            Log.Message($"{LogTitle} Creating volume with {tags.Count.ToString()} tags. Spec: {spec}");
            Log.TabMessage(1, $"Tags: [{String.Join(", ", tags)}].");

            using (IAmazonEC2 client = _clientFactory.CreateEc2Client())
            {
                CreateVolumeRequest request = SEc2VolumesRequestFactory.CreateVolumeWithTags(spec, tags);

                Func<Task<CreateVolumeResponse>> func = async () => await client.CreateVolumeAsync(request);

                CreateVolumeResponse response = await func.TryInvokeAws();

                Log.Message(LogLevels.HighDetailed, $"{LogTitle} Volume '{response.Volume.VolumeId}' has been created at {response.Volume.CreateTime}.");

                return response.Volume.CreateVolumeInfo();
            }
        }

        public async Task DeleteVolumeAsync(String volumeId)
        {
            Log.Message($"{LogTitle} Deleting volume '{volumeId}'.");

            using (IAmazonEC2 client = _clientFactory.CreateEc2Client())
            {
                DeleteVolumeRequest request = SEc2VolumesRequestFactory.DeleteVolume(volumeId);

                Func<Task<DeleteVolumeResponse>> func = async () => await client.DeleteVolumeAsync(request);

                await func.TryInvokeAws();

                Log.Message($"{LogTitle} Volume '{volumeId}' has been deleted.");
            }
        }

        public async Task<CEc2SnapshotInfo> FindSnapshotAsync(String snapshotId)
        {
            Log.Message($"{LogTitle} Getting EBS snapshot by ID: '{snapshotId}'.");

            using (IAmazonEC2 client = _clientFactory.CreateEc2Client())
            {
                DescribeSnapshotsRequest request = SEc2VolumesRequestFactory.FindSnapshot(snapshotId);
                
                String next = null;

                do
                {
                    if (next.IsSpecified())
                        request.NextToken = next;

                    Func<Task<DescribeSnapshotsResponse>> func = async () => await client.DescribeSnapshotsAsync(request);

                    DescribeSnapshotsResponse response = await func.TryGetEc2Obj();

                    if (response == null)
                        return null;

                    CEc2SnapshotInfo snap = response.Snapshots.FirstOrDefault(x => x.SnapshotId.EqualsNoCase(snapshotId))?.CreateSnapshotInfo();

                    if (snap != null)
                        return snap;

                    next = response.NextToken;

                } while (next.IsSpecified());

                return null;
            }
        }

        public async Task<CEc2SnapshotInfo> GetSnapshotAsync(String snapshotId)
        {
            CEc2SnapshotInfo info = await FindSnapshotAsync(snapshotId);
            if (info == null)
                throw new CNotFoundException($"[TBD] Cannot find snapshot: '{snapshotId}'.");

            return info;
        }

        public async Task<CEc2SnapshotInfo> CreateSnapshotAsync(String volumeId, String snapshotDescription)
        {
            Log.Message($"{LogTitle} Creating snapshot of EBS volume '{volumeId}'.");

            using (IAmazonEC2 client = _clientFactory.CreateEc2Client())
            {
                CreateSnapshotRequest request = SEc2VolumesRequestFactory.CreateSnapshot(volumeId, snapshotDescription);

                Func<Task<CreateSnapshotResponse>> func = async () => await client.CreateSnapshotAsync(request);

                CreateSnapshotResponse response = await func.TryInvokeAws();
                
                return response.Snapshot.CreateSnapshotInfo();
            }
        }

        public async Task<CEc2SnapshotInfo> CreateSnapshotWithTagsAsync(String volumeId, String snapshotDescription, IReadOnlyCollection<CEc2TagInfo> tags)
        {
            Log.Message($"{LogTitle} Creating snapshot of EBS volume '{volumeId}' with {tags.Count.ToString()} tags.");
            Log.TabMessage(1, $"Tags: [{String.Join(", ", tags)}].");

            using (IAmazonEC2 client = _clientFactory.CreateEc2Client())
            {
                CreateSnapshotRequest request = SEc2VolumesRequestFactory.CreateSnapshotWithTags(volumeId, snapshotDescription, tags);

                Func<Task<CreateSnapshotResponse>> func = async () => await client.CreateSnapshotAsync(request);

                CreateSnapshotResponse response = await func.TryInvokeAws();

                return response.Snapshot.CreateSnapshotInfo();
            }
        }

        public async Task<IReadOnlyList<CEc2SnapshotInfo>> CreateMultiVolumeSnapshotAsync(String instanceId, String snapshotDescription)
        {
            Log.Message($"{LogTitle} Creating multi-volume snapshot of EBS instance '{instanceId}'.");

            using (IAmazonEC2 client = _clientFactory.CreateEc2Client())
            {
                CreateSnapshotsRequest request = SEc2VolumesRequestFactory.CreateMultiVolumeSnapshot(instanceId, snapshotDescription);

                Func<Task<CreateSnapshotsResponse>> func = async () => await client.CreateSnapshotsAsync(request);

                CreateSnapshotsResponse response = await func.TryInvokeAws();

                return response.Snapshots.Select(snapshot => snapshot.CreateSnapshotInfo()).ToList();
            }
        }

        public async Task<IReadOnlyList<CEc2SnapshotInfo>> CreateMultiVolumeSnapshotWithTagsAsync(String instanceId, String snapshotDescription, IReadOnlyCollection<CEc2TagInfo> tags)
        {
            Log.Message($"{LogTitle} Creating multi-volume snapshot of EBS instance '{instanceId}' with {tags.Count.ToString()} tags.");
            Log.TabMessage(1, $"Tags: [{String.Join(", ", tags)}].");

            using (IAmazonEC2 client = _clientFactory.CreateEc2Client())
            {
                CreateSnapshotsRequest request = SEc2VolumesRequestFactory.CreateMultiVolumeSnapshotWithTags(instanceId, snapshotDescription, tags);

                Func<Task<CreateSnapshotsResponse>> func = async () => await client.CreateSnapshotsAsync(request);

                CreateSnapshotsResponse response = await func.TryInvokeAws();

                return response.Snapshots.Select(snapshot => snapshot.CreateSnapshotInfo()).ToList();
            }
        }

        public async Task<String> CopySnapshotAsync(String destinationRegion, String sourceRegion, String sourceSnapshotId, String snapshotDescription)
        {
            Log.Message($"{LogTitle} Copying EBS snapshot '{sourceSnapshotId}' from '{sourceRegion}' to '{destinationRegion}'.");

            using (IAmazonEC2 client = _clientFactory.CreateEc2Client())
            {
                CopySnapshotRequest request = SEc2VolumesRequestFactory.CopySnapshot(destinationRegion, sourceRegion, sourceSnapshotId, snapshotDescription);

                Func<Task<CopySnapshotResponse>> func = async () => await client.CopySnapshotAsync(request);

                CopySnapshotResponse response = await func.TryInvokeAws();

                Log.Message($"{LogTitle} Snapshot '{sourceSnapshotId}' has been copied.");

                return response.SnapshotId;
            }
        }

        public async Task DeleteSnapshotAsync(String snapshotId)
        {
            Log.Message($"{LogTitle} Deleting EBS snapshot '{snapshotId}'.");

            using (IAmazonEC2 client = _clientFactory.CreateEc2Client())
            {
                DeleteSnapshotRequest request = SEc2VolumesRequestFactory.DeleteSnapshot(snapshotId);

                Func<Task<DeleteSnapshotResponse>> func = async () => await client.DeleteSnapshotAsync(request);

                await func.TryInvokeAws();

                Log.Message($"{LogTitle} Snapshot '{snapshotId}' has been deleted.");
            }
        }

        public async Task<IReadOnlyList<CEc2VolumeInfo>> GetAllVolumesAsync()
        {
            Log.Message($"{LogTitle} Getting all volumes.");

            String token = null;

            var result = new List<CEc2VolumeInfo>();
            
            using (IAmazonEC2 client = _clientFactory.CreateEc2Client())
            {
                DescribeVolumesRequest request = SEc2MachineRequestFactory.GetAllVolumes(SEc2MachineRequestFactory.MaxResults);

                do
                {
                    if (token.IsSpecified())
                        request.NextToken = token;

                    Func<Task<DescribeVolumesResponse>> func = async () => await client.DescribeVolumesAsync(request);

                    DescribeVolumesResponse response = await func.TryGetEc2Obj();

                    if (response == null)
                        break;

                    token = response.NextToken;

                    result.AddRange(response.Volumes.Select(vol => vol.CreateVolumeInfo()));

                } while (token.IsSpecified());
            }

            return result;
        }

        public async Task<IReadOnlyList<CEc2SnapshotInfo>> GetAllSnapshotsAsync()
        {
            Log.Message($"{LogTitle} Getting all snapshots.");
            
            var result = new List<CEc2SnapshotInfo>();
            
            using (IAmazonEC2 client = _clientFactory.CreateEc2Client())
            {
                String token = null;

                do
                {
                    DescribeSnapshotsRequest request = SEc2MachineRequestFactory.GetAllSnapshots(SEc2MachineRequestFactory.MaxResults);

                    if (token.IsSpecified())
                        request.NextToken = token;

                    Func<Task<DescribeSnapshotsResponse>> func = async () => await client.DescribeSnapshotsAsync(request);

                    DescribeSnapshotsResponse response = await func.TryGetEc2Obj();

                    if (response == null)
                        break;

                    token = response.NextToken;

                    result.AddRange(response.Snapshots.Select(snap => snap.CreateSnapshotInfo()));

                } while (token.IsSpecified());
            }

            return result;
        }

        public async Task ModifyVolumeAsync(String volumeId, EEc2VolumeType type, Int32 iops)
        {
            Log.Message($"{LogTitle} Modifying volume '{volumeId}'. Type: '{type.ToString()}'. IOPS: {iops.ToString()}.");

            using (IAmazonEC2 client = _clientFactory.CreateEc2Client())
            {
                ModifyVolumeRequest request = SEc2VolumesRequestFactory.ModifyVolume(volumeId, type, iops);

                Func<Task<ModifyVolumeResponse>> func = async () => await client.ModifyVolumeAsync(request);

                await func.TryInvokeAws();
            }
        }

        public async Task<IReadOnlyList<CEc2VolumeInfo>> GetVolumesAsync(IReadOnlyList<String> volumeIds)
        {
            Log.Message($"{LogTitle} Getting volumes by ID: {volumeIds.EnumerableToLogString()}.");

            using (IAmazonEC2 client = _clientFactory.CreateEc2Client())
            {
                DescribeVolumesRequest request = SEc2MachineRequestFactory.GetVolumes(volumeIds);

                var result = new List<CEc2VolumeInfo>();
                String next = null;

                do
                {
                    if (next.IsSpecified())
                        request.NextToken = next;

                    Func<Task<DescribeVolumesResponse>> func = async () => await client.DescribeVolumesAsync(request);

                    DescribeVolumesResponse response = await func.TryGetEc2Obj();

                    if (response == null)
                        break;

                    result.AddRange(response.Volumes.Select(x => x.CreateVolumeInfo()));
                    next = response.NextToken;

                } while (next.IsSpecified());

                return result;
            }
        }

        public async Task<IReadOnlyList<CEc2SnapshotInfo>> GetAllSnapshotsByDescriptionAsync(String snapshotDescription)
        {
            Log.Message($"{LogTitle} Getting EBS snapshots by description: '{snapshotDescription}'.");

            var snapshots = new List<CEc2SnapshotInfo>();

            using (IAmazonEC2 client = _clientFactory.CreateEc2Client())
            {
                DescribeSnapshotsRequest request = SEc2VolumesRequestFactory.FindSnapshotByDescription(snapshotDescription);

                String next = null;

                do
                {
                    if (next.IsSpecified())
                        request.NextToken = next;

                    Func<Task<DescribeSnapshotsResponse>> func = async () => await client.DescribeSnapshotsAsync(request);

                    DescribeSnapshotsResponse response = await func.TryGetEc2Obj();

                    if (response == null)
                        return new CEc2SnapshotInfo[0];

                    snapshots.AddRange(response.Snapshots.Select(snap => snap.CreateSnapshotInfo()));

                    next = response.NextToken;

                } while (next.IsSpecified());

                return snapshots;
            }
        }

        public async Task<IReadOnlyList<CEc2SnapshotInfo>> GetAllSnapshotsByVolumeIdAndStateAsync(String volumeId, EEc2SnapshotState snapshotState)
        {
            Log.Message($"{LogTitle} Getting EBS snapshots by state: '{snapshotState.ToString()}'.");

            var snapshots = new List<CEc2SnapshotInfo>();

            using (IAmazonEC2 client = _clientFactory.CreateEc2Client())
            {
                DescribeSnapshotsRequest request = SEc2VolumesRequestFactory.FindSnapshotByVolumeIdAndState(volumeId, snapshotState);
                
                String next = null;

                do
                {
                    if (next.IsSpecified())
                        request.NextToken = next;

                    Func<Task<DescribeSnapshotsResponse>> func = async () => await client.DescribeSnapshotsAsync(request);

                    DescribeSnapshotsResponse response = await func.TryGetEc2Obj();

                    if (response == null)
                        return new CEc2SnapshotInfo[0];

                    snapshots.AddRange(response.Snapshots.Select(snap => snap.CreateSnapshotInfo()));
                    
                    next = response.NextToken;

                } while (next.IsSpecified());

                return snapshots;
            }
        }

        public async Task ShareSnapshotAsync(String snapshotId, String awsAccountId)
        {
            Log.Message($"{LogTitle} Sharing EBS snapshot with account: '{awsAccountId}'.");

            using (IAmazonEC2 client = _clientFactory.CreateEc2Client())
            {
                ModifySnapshotAttributeRequest request = SEc2VolumesRequestFactory.ShareSnapshotRequest(snapshotId, awsAccountId);
                Func<Task<ModifySnapshotAttributeResponse>> func = async () => await client.ModifySnapshotAttributeAsync(request);

                await func.TryInvokeAws();
            }
        }

        public async Task<CEc2ConversionTaskInfo> ImportVolumeAsync(CEc2VolumeImportSpec spec)
        {
            Log.Message($"{LogTitle} Importing volume. Spec: {spec}");

            using (IAmazonEC2 client = _clientFactory.CreateEc2Client())
            {
                ImportVolumeRequest request = SEc2VolumesRequestFactory.ImportVolume(spec);

                Func<Task<ImportVolumeResponse>> func = async () => await client.ImportVolumeAsync(request);

                ImportVolumeResponse response = await func.TryInvokeAws();

                Log.Message($"{LogTitle} Conversion task has been created. Task ID: '{response.ConversionTask.ConversionTaskId}'.");

                return response.ConversionTask.ToEc2ConversionTaskInfo();
            }
        }

        #endregion

        private async Task<IReadOnlyList<Volume>> InternalGetVolumes(DescribeVolumesRequest request)
        {
            using (IAmazonEC2 client = _clientFactory.CreateEc2Client())
            {
                var result = new List<Volume>();

                String next = null;

                do
                {
                    if (next.IsSpecified())
                        request.NextToken = next;

                    Func<Task<DescribeVolumesResponse>> func = async () => await client.DescribeVolumesAsync(request);

                    DescribeVolumesResponse response = await func.TryGetEc2Obj();

                    if (response == null)
                        break;

                    result.AddRange(response.Volumes);
                    next = response.NextToken;

                } while (next.IsSpecified());

                return result;
            }
        }
    }
}
