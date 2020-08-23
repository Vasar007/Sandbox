using System;
using System.Collections.Generic;
using System.Linq;
using Veeam.CloudBackup.Common;
using Veeam.CloudBackup.Model.Amazon;

namespace Veeam.CloudBackup.Amazon.Jobs.Tests.MountPointFinderTests
{
    internal static class SEc2ApplianceGenerator
    {
        private static readonly Random RandomInstance = new Random();

        internal static CEc2MachineInfo CreateEc2MachineInfoWithHvmType()
        {
            return CreateEc2MachineInfo(CEc2MachineVirtualizationType.Hvm.Type);
        }

        internal static CEc2MachineInfo CreateEc2MachineInfoWithParavirtualType()
        {
            return CreateEc2MachineInfo(CEc2MachineVirtualizationType.Paravirtual.Type);
        }

        internal static CEc2MachineInfo CreateEc2MachineInfo(EEc2VirtualizationType virtualizationType)
        {
            var convertedVirtualizationType = new CEc2MachineVirtualizationType(virtualizationType);

            return new CEc2MachineInfo(
                id: "i-075a3674980442427",
                imageId: "ami-077a5b1762a2dde35",
                vpcId: "vpc-3e109b57",
                instanceType: CEc2MachineType.T2Medium.Value,
                reservationId: null,
                state: EEc2MachineState.Stopped,
                subnetId: "subnet-ff962e84",
                launchTime: SManagedDateTime.UtcNow,
                tags: new[] { new CEc2TagInfo(CEc2TagInfo.VmNameTagKey, "InstanceName") },
                virtualizationType: convertedVirtualizationType.Value,
                tenancy: "default",
                rootDeviceName: "/dev/sda1",
                keyName: "key-name",
                kernelId: null,
                enaSupport: true,
                architecture: "x86_64",
                platform: CEc2PlatformType.Linux.Value,
                securityGroups: new String[0],
                launchTemplateInfo: new CEc2MachineLaunchTemplateInfo(
                    shutdownBehavior: null,
                    disableApiTermination: null,
                    kernelId: null,
                    ramDiskId: null,
                    monitoringEnabled: null,
                    ebsOptimized: null,
                    userData: null
                )
            );
        }

        internal static IReadOnlyList<CEc2VolumeInfo> CreateEc2VolumeInfos(String instanceId, Int32 count, params String[] deviceNames)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "Count must be positive.");

            String deviceName = String.Empty;
            var ec2Volumes = new List<CEc2VolumeInfo>(count);
            for (Int32 i = 0; i < count; ++i)
            {
                if (i == 0)
                    deviceName = "/dev/sda1";
                else if (!deviceNames.IsNullOrEmpty() && i - 1 < deviceNames.Length)
                    deviceName = deviceNames[i - 1];
                else
                    deviceName = SMountPointFinder.DefaultDeviceNamePrefix + Convert.ToChar(deviceName.Last() + 1);


                var ec2Volume = new CEc2VolumeInfo(
                    id: $"vol-{SStringHelper.CreateRandomString(length: 17, RandomInstance)}",
                    creationTime: SManagedDateTime.UtcNow,
                    isEncrypted: false,
                    size: 8,
                    volumeType: EEc2VolumeType.Gp2,
                    iops: 100,
                    availabilityZone: "eu-west-2a",
                    state: EEc2VolumeState.Available,
                    device: deviceName,
                    instanceId: instanceId,
                    tags: new[] { new CEc2TagInfo(CEc2TagInfo.VmNameTagKey, $"VolumeName-{i.ToString()}") }
                );

                ec2Volumes.Add(ec2Volume);
            }

            return ec2Volumes;
        }
    }
}
