using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.EC2;
using Amazon.EC2.Model;
using Veeam.CloudBackup.Common;
using Veeam.CloudBackup.Configuration;
using Veeam.CloudBackup.Logging;
using Veeam.CloudBackup.Model.Amazon;

namespace Veeam.CloudBackup.AmazonApi
{
    internal static class SEc2VolumesRequestFactory
    {
        public static DescribeVolumesRequest FindVolume(String volumeId)
        {
            return new DescribeVolumesRequest(new List<String> { volumeId });
        }

        public static DescribeVolumesRequest GetVolumesByInstanceId(String instanceId)
        {
            var request = new DescribeVolumesRequest
            {
                Filters = new List<Filter> { new Filter("attachment.instance-id", new List<String> { instanceId }) }
            };

            return request;
        }

        public static DeleteVolumeRequest DeleteVolume(String volumeId)
        {
            return new DeleteVolumeRequest(volumeId);
        }

        public static DescribeSnapshotsRequest FindSnapshot(String snapshotId)
        {
            return new DescribeSnapshotsRequest { SnapshotIds = new List<String> { snapshotId } };
        }

        public static DescribeSnapshotsRequest FindSnapshotByVolumeIdAndState(String volumeId, EEc2SnapshotState snapshotState)
        {
            const String filterByVolumdIdName = "volume-id";
            const String filterByStatusName = "status";

            return new DescribeSnapshotsRequest
            {
                Filters = new List<Filter>
                {
                    new Filter(filterByVolumdIdName, new List<String> { volumeId }),
                    new Filter(filterByStatusName, new List<String> { snapshotState.ToString().ToLowerInvariant() })
                }
            };
        }

        public static DescribeSnapshotsRequest FindSnapshotByDescription(String snapshotDescription)
        {
            const String filterByDescriptionName = "description";

            return new DescribeSnapshotsRequest { Filters = new List<Filter>{ new Filter(filterByDescriptionName, new List<String>{ snapshotDescription}) } };
        }

        public static ModifySnapshotAttributeRequest ShareSnapshotRequest(String snapshotId, String accountId)
        {
            var request = new ModifySnapshotAttributeRequest(snapshotId, SnapshotAttributeName.CreateVolumePermission, OperationType.Add)
            {
                UserIds = new List<String> { accountId }
            };

            return request;
        }

        public static CreateSnapshotRequest CreateSnapshot(String volumeId, String snapshotDescription)
        {
            return CreateSnapshotWithTags(volumeId, snapshotDescription, new List<CEc2TagInfo>());
        }

        public static CreateSnapshotRequest CreateSnapshotWithTags(String volumeId, String snapshotDescription, IReadOnlyCollection<CEc2TagInfo> tags)
        {
            if (String.IsNullOrEmpty(snapshotDescription))
                snapshotDescription = "[TBD] Veeam snapshot";

            var request = new CreateSnapshotRequest(volumeId, snapshotDescription);

            if (tags.Any())
            {
                request.TagSpecifications = new List<TagSpecification>
                {
                    new TagSpecification
                    {
                        ResourceType = ResourceType.Snapshot,
                        Tags = tags.TransformToAwsEc2Tags()
                    }
                };
            }

            return request;
        }

        public static CreateSnapshotsRequest CreateMultiVolumeSnapshot(String instanceId, String snapshotDescription)
        {
            return CreateMultiVolumeSnapshotWithTags(instanceId, snapshotDescription, new List<CEc2TagInfo>());
        }

        public static CreateSnapshotsRequest CreateMultiVolumeSnapshotWithTags(String instanceId, String snapshotDescription, IReadOnlyCollection<CEc2TagInfo> tags)
        {
            if (String.IsNullOrEmpty(snapshotDescription))
                snapshotDescription = "[TBD] Veeam multi-volume snapshot";

            var request = new CreateSnapshotsRequest
            {
                Description = snapshotDescription,
                InstanceSpecification = new InstanceSpecification { InstanceId = instanceId }
            };

            if (tags.Any())
            {
                request.TagSpecifications = new List<TagSpecification>
                {
                    new TagSpecification
                    {
                        ResourceType = ResourceType.Snapshot,
                        Tags = tags.TransformToAwsEc2Tags()
                    }
                };
            }

            return request;
        }

        public static CopySnapshotRequest CopySnapshot(String destinationRegion, String sourceRegion, String sourceSnapshotId, String snapshotDescription)
        {
            if (String.IsNullOrEmpty(snapshotDescription))
                snapshotDescription = "[TBD] Veeam snapshot";

            return new CopySnapshotRequest
            {
                DestinationRegion = destinationRegion,
                SourceRegion = sourceRegion,
                SourceSnapshotId = sourceSnapshotId,
                Description = snapshotDescription,
            };
        }

        public static DeleteSnapshotRequest DeleteSnapshot(String snapshotId)
        {
            return new DeleteSnapshotRequest(snapshotId);
        }

        public static ModifyVolumeRequest ModifyVolume(String volumeId, EEc2VolumeType type, Int32 iops)
        {
            var request = new ModifyVolumeRequest
            {
                VolumeId = volumeId,
                VolumeType = type.ToString()
            };

            if (type == EEc2VolumeType.Io1)
            {
                if (iops <= 0)
                    throw ExceptionFactory.Create($"[TBD] Invalid IOPS value: {iops.ToString()}.");

                request.Iops = iops;
            }

            return request;
        }

        public static DetachVolumeRequest DetachVolume(String volumeId, Boolean force)
        {
            var request = new DetachVolumeRequest(volumeId)
            {
                Force = force
            };

            return request;
        }

        public static ImportVolumeRequest ImportVolume(CEc2VolumeImportSpec spec)
        {
            var request = new ImportVolumeRequest
            {
                AvailabilityZone = spec.AvailabilityZone,
                Description = "[TBD] Veeam import disk"
            };

            var diskDetail = new DiskImageDetail
            {
                Bytes = spec.DiskImageSize,
                Format = spec.FileType,
                ImportManifestUrl = spec.ImportManifestUrl
            };

            var volDetail = new VolumeDetail
            {
                Size = CheckAwsVolumeSize(spec.VolumeSize)
            };

            request.Image = diskDetail;
            request.Volume = volDetail;

            return request;
        }

        public static CreateVolumeRequest CreateVolume(CEc2VolumeCreationSpec spec)
        {
            return CreateVolumeWithTags(spec, new List<CEc2TagInfo>());
        }

        public static CreateVolumeRequest CreateVolumeWithTags(CEc2VolumeCreationSpec spec, IReadOnlyCollection<CEc2TagInfo> tags)
        {
            Int32 size = CheckAwsVolumeSize(spec.VolumeType, spec.Size);

            var request = new CreateVolumeRequest
            {
                AvailabilityZone = spec.AvailabilityZone,
                Size = size,
                VolumeType = spec.VolumeType,
                SnapshotId = spec.SnapshotId
            };

            if (tags.Any())
            {
                request.TagSpecifications = new List<TagSpecification>
                {
                    new TagSpecification
                    {
                        ResourceType = ResourceType.Volume,
                        Tags = tags.TransformToAwsEc2Tags()
                    }
                };
            }

            if (StringComparer.InvariantCultureIgnoreCase.Equals(spec.VolumeType, "io1"))
            {
                Int32 iops = CheckAwsVolumeIops(spec.VolumeType, spec.Iops);
                Int32 maxRatio = SOptions.GetOptions<CAmazonApiOptions>().AwsMaxIopsRatio;

                Double iopsRatio = (Double)iops / size;

                if (iopsRatio > maxRatio)
                {
                    iops = size * maxRatio;

                    Log.Warning($"Current IOPS ratio ({iopsRatio.ToString()}) is greater than maximum ({maxRatio.ToString()}). IOPS value was decreased to {iops.ToString()}.");
                }

                request.Iops = iops;
            }

            return request;
        }

        public static AttachVolumeRequest AttachVolume(String device, String volumeId, String instanceId)
        {
            return new AttachVolumeRequest(volumeId, instanceId, device);
        }

        public static DescribeVolumesModificationsRequest DescribeVolumeModification(params String[] volumeIds)
        {
            if (volumeIds.Length == 0)
                return null;

            String[] volIds = volumeIds.Where(id => SAmazonIdHelper.ResolveConversionTaskType(id) == EAmazonConversionTaskType.ModifyVolumeTask).ToArray();

            if (volIds.Length == 0)
                return null;

            var request = new DescribeVolumesModificationsRequest
            {
                VolumeIds = new List<String>(volIds)
            };

            return request;
        }

        public static DescribeSnapshotsRequest DescribeSnapshotTasks(params String[] snapshotIds)
        {
            if (snapshotIds.IsEmpty())
                return null;

            List<String> snapIds = snapshotIds.Where(id => SAmazonIdHelper.ResolveConversionTaskType(id) == EAmazonConversionTaskType.CreateSnapshotTask).ToList();

            if (snapIds.IsEmpty())
                return null;

            var request = new DescribeSnapshotsRequest
            {
                SnapshotIds = snapIds
            };

            return request;
        }

        private static Int32 CheckAwsVolumeSize(Int32 size)
        {
            const String defaultAwsVolumeType = "gp2";
            return CheckAwsVolumeSize(defaultAwsVolumeType, size);
        }

        private static Int32 CheckAwsVolumeSize(String volumeType, Int32 size)
        {
            GetVolumeSizeRange(volumeType, out Int32 minSize, out Int32 maxSize);

            if (size < minSize)
            {
                Log.Warning($"Volume size is less than minimal ({minSize.ToString()} Gb). Minimal size was set.");
                return minSize;
            }

            if (size > maxSize)
            {
                Log.Warning($"Volume size is greater than maximal ({maxSize.ToString()} Gb). Maximal size was set.");
                return maxSize;
            }

            return size;
        }

        private static void GetVolumeSizeRange(String volumeType, out Int32 minimumSize, out Int32 maximumSize)
        {
            switch (volumeType.ToLowerInvariant())
            {
                case "gp2":
                {
                    minimumSize = 1;
                    maximumSize = (Int32) SUnitHelper.ConvertSize(16, ESizeUnit.TB, ESizeUnit.GB);
                    return;
                }

                case "io1":
                {
                    minimumSize = 4;
                    maximumSize = (Int32) SUnitHelper.ConvertSize(16, ESizeUnit.TB, ESizeUnit.GB);
                    return;
                }

                case "st1":
                {
                    minimumSize = 500;
                    maximumSize = (Int32) SUnitHelper.ConvertSize(16, ESizeUnit.TB, ESizeUnit.GB);
                    return;
                }

                case "sc1":
                {
                    minimumSize = 500;
                    maximumSize = (Int32) SUnitHelper.ConvertSize(16, ESizeUnit.TB, ESizeUnit.GB);
                    return;
                }

                case "standard":
                {
                    minimumSize = 1;
                    maximumSize = (Int32) SUnitHelper.ConvertSize(1, ESizeUnit.TB, ESizeUnit.GB);
                    return;
                }

                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(volumeType), volumeType, $"[TBD] Unknown volume type: '{volumeType}'.");
                }
            }
        }

        private static Int32 CheckAwsVolumeIops(String volumeType, Int32 iops)
        {
            GetVolumeIopsRange(volumeType, out Int32 minimalIops, out Int32 maximalIops);

            if (iops < minimalIops)
            {
                Log.Warning($"Volume IOPS value ({iops.ToString()}) is less than minimum allowed ({minimalIops.ToString()}), setting the minimum allowed value.");
                return minimalIops;
            }

            if (iops > maximalIops)
            {
                Log.Warning($"Volume IOPS value ({iops.ToString()}) is greater than maximum allowed ({maximalIops.ToString()}), setting the maximum allowed value.");
                return maximalIops;
            }

            return iops;
        }

        private static void GetVolumeIopsRange(String volumeType, out Int32 minimalIops, out Int32 maximalIops)
        {
            switch (volumeType.ToLowerInvariant())
            {
                case "gp2":
                {
                    minimalIops = 100;
                    maximalIops = 3000;
                    return;
                }

                case "io1":
                {
                    minimalIops = 100;
                    maximalIops = 32000;
                    return;
                }

                case "st1":
                case "sc1":
                case "standard":
                {
                    minimalIops = 100;
                    maximalIops = 20000;
                    return;
                }

                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(volumeType), volumeType, $"[TBD] Unknown volume type: '{volumeType}'.");
                }
            }
        }
    }
}
