using System;
using System.Collections.Generic;
using System.Linq;
using Veeam.CloudBackup.Common;
using Veeam.CloudBackup.Model.Amazon;
using Veeam.CloudBackup.Utils;

namespace Veeam.CloudBackup.Amazon.Jobs
{
    internal static class SMountPointFinder
    {
        private static readonly CPrefixLogger Logger = CPrefixLogger.Create(nameof(SMountPointFinder));

        // https://docs.aws.amazon.com/AWSEC2/latest/UserGuide/device_naming.html
        // Can end with: f, g, h, i, j, k, l, m, n, o, p, i.e. "/dev/sd[f-p]" for both HVM and paravirtual virtualization type.
        // For paravirtual virtualization type also can end with "/dev/sd[f-p][1-6]".
        internal const String DefaultDeviceNamePrefix = "/dev/sd";

        internal static readonly IReadOnlyList<String> DeviceNameSuffixes = new List<String> { "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p" };
        internal static readonly IReadOnlyList<String> AdditionalDeviceNameSuffixes = new List<String> { "1", "2", "3", "4", "5", "6" };

        internal static readonly String DefaultDeviceName = DefaultDeviceNamePrefix + DeviceNameSuffixes.First();

        internal static readonly String MountPointAlreadyInUseErrorCode = "InvalidParameterValue";

        public static String GetMountPoint(CEc2MachineInfo ec2Machine, IReadOnlyList<CEc2VolumeInfo> attachedEc2Volumes)
        {
            SExceptions.CheckArgumentNull(ec2Machine, nameof(ec2Machine));
            SExceptions.CheckArgumentNull(attachedEc2Volumes, nameof(attachedEc2Volumes));

            String volumesMessageInfo = attachedEc2Volumes.Select(volume => $"ID: {volume.Id}, Device: {volume.Device}").EnumerableToLogString();
            Logger.Message($"Trying to get mount point for instance '{ec2Machine.Id}' with attached volumes [{volumesMessageInfo}]");

            if (ec2Machine.Platform.Type == EEc2PlatformType.Windows)
                Logger.Warning("There are attempt to get mount point for Windows platform. This may be the cause of further errors during volume attaching.");

            if (attachedEc2Volumes.Count == 0)
                return DefaultDeviceName;

            HashSet<String> attachedDevices = attachedEc2Volumes.Select(attachedEc2Volume => attachedEc2Volume.Device).ToHashSet();
            foreach (String deviceNameSuffix in DeviceNameSuffixes)
            {
                String newDeviceName = DefaultDeviceNamePrefix + deviceNameSuffix;

                if (!attachedDevices.Contains(newDeviceName))
                    return newDeviceName;
            }

            if (ec2Machine.VirtualizationType.Type == EEc2VirtualizationType.Paravirtual)
            {
                HashSet<String> deviceNamesWithNumbers = DeviceNameSuffixes
                    .SelectMany(suffix => AdditionalDeviceNameSuffixes, (suffix, additionalSuffix) => DefaultDeviceNamePrefix + suffix + additionalSuffix)
                    .ToHashSet();

                foreach (String deviceName in deviceNamesWithNumbers)
                {
                    if (!attachedDevices.Contains(deviceName))
                        return deviceName;
                }
            }

            String message = $"Free device name to volume attach could not be found for instance '{ec2Machine.Id}' with attached volumes [{volumesMessageInfo}].";
            throw new InvalidOperationException(message);
        }
    }
}
