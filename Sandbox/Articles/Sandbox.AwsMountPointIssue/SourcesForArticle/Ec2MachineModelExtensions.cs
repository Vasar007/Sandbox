using System;
using System.Globalization;
using System.Linq;
using Amazon.EC2;
using Amazon.EC2.Model;
using Veeam.CloudBackup.Common;
using Veeam.CloudBackup.Logging;
using Veeam.CloudBackup.Model.Amazon;

namespace Veeam.CloudBackup.AmazonApi
{
    internal static class SEc2MachineModelExtensions
    {
        public static CEc2MachineInfo CreateMachineInfo(this Instance instance, ResponseLaunchTemplateData launchTemplateData)
        {
            return instance.CreateMachineInfo(launchTemplateData, instance.CapacityReservationId);
        }

        public static CEc2MachineInfo CreateMachineInfo(this Instance instance, ResponseLaunchTemplateData launchTemplateData, String reservationId)
        {
            var launchTemplateInfo = new CEc2MachineLaunchTemplateInfo(
                launchTemplateData?.InstanceInitiatedShutdownBehavior?.Value,
                launchTemplateData?.DisableApiTermination, launchTemplateData?.KernelId, launchTemplateData?.RamDiskId,
                launchTemplateData?.Monitoring.Enabled, launchTemplateData?.EbsOptimized, launchTemplateData?.UserData
            );

            var info = new CEc2MachineInfo(
                instance.InstanceId, instance.ImageId, instance.VpcId,
                instance.InstanceType.Value, reservationId, instance.State.ResolveMachineState(), 
                instance.SubnetId, instance.LaunchTime, instance.Tags.Select(tag => new CEc2TagInfo(tag.Key, tag.Value)).ToList(), instance.VirtualizationType.Value, instance.Placement.Tenancy.Value,
                instance.RootDeviceName, instance.KeyName, instance.KernelId, instance.EnaSupport, instance.Architecture.Value, instance.Platform?.Value,
                instance.SecurityGroups.Select(sg => sg.GroupId).ToList(), launchTemplateInfo
            );

            if (instance.PublicIpAddress.IsSpecified())
                info.SetPublicAddressInfo(instance.PublicDnsName, instance.PublicIpAddress);

            if (instance.PrivateIpAddress.IsSpecified())
                info.SetPrivateAddressInfo(instance.PrivateDnsName, instance.PrivateIpAddress);

            return info;
        }

        public static EEc2MachineState ResolveMachineState(this InstanceState state)
        {
            try
            {
                return (EEc2MachineState) state.Code;
            }
            catch (Exception ex)
            {
                Log.Exception(ex, $"Cannot resolve EC2 instance state. Code '{state.Code.ToString()}', Name '{state.Name}'");
                throw;
            }
        }

        public static CEc2ImageInfo CreateImageInfo(this Image image)
        {
            EEc2PlatformType platform = image.Platform == PlatformValues.Windows
                ? EEc2PlatformType.Windows
                : EEc2PlatformType.Linux;

            var awsImageInfo = new CEc2ImageInfo(
                image.ImageId, image.Name,
                DateTime.Parse(image.CreationDate, CultureInfo.GetCultureInfo("en-US")), image.Description,
                image.ImageType.ToEc2ImageType(), platform
            );

            return awsImageInfo;
        }

        private static EEc2ImageType ToEc2ImageType(this ImageTypeValues imageType)
        {
            if (imageType == ImageTypeValues.Kernel)
                return EEc2ImageType.Kernel;

            if (imageType == ImageTypeValues.Machine)
                return EEc2ImageType.Machine;

            if (imageType == ImageTypeValues.Ramdisk)
                return EEc2ImageType.Ramdisk;

            throw new ArgumentOutOfRangeException(nameof(imageType), imageType, $"[TBD] Unknown image type: '{imageType.Value}'.");
        }

        public static BlockDeviceMapping ToEc2BlockDeviceMapping(this CEc2BlockDeviceMapping mapping)
        {
            var blockDevice = new EbsBlockDevice
            {
                VolumeType = mapping.VolumeType.ToEc2VolumeType()

            };

            if (mapping.VolumeType == EEc2VolumeType.Io1)
                blockDevice.Iops = mapping.Iops;

            var blockDeviceMapping = new BlockDeviceMapping
            {
                DeviceName = mapping.DeviceName,
                Ebs = blockDevice
            };

            return blockDeviceMapping;
        }

        public static InstanceNetworkInterfaceSpecification ToEc2InterfaceSpecification(this CEc2NetworkInterfaceContext context)
        {
            var spec = new InstanceNetworkInterfaceSpecification
            {
                DeviceIndex = context.DeviceNumber,
                SubnetId = context.SubnetId,
                Groups = context.SecurityGroupsId.ToList(),
                AssociatePublicIpAddress = context.AttachPublicIp
            };

            return spec;
        }
    }
}
