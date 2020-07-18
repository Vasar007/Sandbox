using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Veeam.CloudBackup.Model.Amazon;

namespace Veeam.CloudBackup.Amazon.Jobs.Tests.MountPointFinderTests
{
    [TestFixture]
    public sealed class CMountPointFinderTests
    {

        public CMountPointFinderTests()
        {
        }

        #region Common Tests

        [Test]
        [TestCase(EEc2VirtualizationType.Hvm)]
        [TestCase(EEc2VirtualizationType.Paravirtual)]
        public void NullTests(EEc2VirtualizationType virtualizationType)
        {
            CEc2MachineInfo machine = SEc2ApplianceGenerator.CreateEc2MachineInfo(virtualizationType);
            IReadOnlyList<CEc2VolumeInfo> ec2Volumes = new List<CEc2VolumeInfo>();

            String actualMountPoint = null;
            Assert.Throws<ArgumentNullException>(() => actualMountPoint = SMountPointFinder.GetMountPoint(machine, null));
            Assert.Throws<ArgumentNullException>(() => actualMountPoint = SMountPointFinder.GetMountPoint(null, ec2Volumes));
            Assert.Throws<ArgumentNullException>(() => actualMountPoint = SMountPointFinder.GetMountPoint(null, null));

            Assert.IsNull(actualMountPoint);
        }

        [Test]
        [TestCase(EEc2VirtualizationType.Hvm)]
        [TestCase(EEc2VirtualizationType.Paravirtual)]
        public void GetMountPointForInstanceWithoutVolumesTest(EEc2VirtualizationType virtualizationType)
        {
            CEc2MachineInfo machine = SEc2ApplianceGenerator.CreateEc2MachineInfo(virtualizationType);
            IReadOnlyList<CEc2VolumeInfo> ec2Volumes = new List<CEc2VolumeInfo>();

            String actualMountPoint = SMountPointFinder.GetMountPoint(machine, ec2Volumes);

            String expectedMountPoint = SMountPointFinder.DefaultDeviceName;

            Assert.IsNotNull(actualMountPoint);
            Assert.AreEqual(expectedMountPoint, actualMountPoint);
        }

        [Test]
        [TestCase(EEc2VirtualizationType.Hvm)]
        [TestCase(EEc2VirtualizationType.Paravirtual)]
        public void GetMountPointForInstanceWithRootVolumeTest(EEc2VirtualizationType virtualizationType)
        {
            CEc2MachineInfo machine = SEc2ApplianceGenerator.CreateEc2MachineInfo(virtualizationType);
            IReadOnlyList<CEc2VolumeInfo> ec2Volumes = SEc2ApplianceGenerator.CreateEc2VolumeInfos(machine.Id, 1);

            String actualMountPoint = SMountPointFinder.GetMountPoint(machine, ec2Volumes);

            String expectedMountPoint = SMountPointFinder.DefaultDeviceName;

            Assert.IsNotNull(actualMountPoint);
            Assert.AreEqual(expectedMountPoint, actualMountPoint);
        }

        [Test]
        [TestCase(EEc2VirtualizationType.Hvm)]
        [TestCase(EEc2VirtualizationType.Paravirtual)]
        public void GetMountPointForInstanceWithRootSdbVolumesTest(EEc2VirtualizationType virtualizationType)
        {
            CEc2MachineInfo machine = SEc2ApplianceGenerator.CreateEc2MachineInfo(virtualizationType);
            IReadOnlyList<CEc2VolumeInfo> ec2Volumes = SEc2ApplianceGenerator.CreateEc2VolumeInfos(machine.Id, 2, "/dev/sdb");

            String actualMountPoint = SMountPointFinder.GetMountPoint(machine, ec2Volumes);

            String expectedMountPoint = SMountPointFinder.DefaultDeviceName;

            Assert.IsNotNull(actualMountPoint);
            Assert.AreEqual(expectedMountPoint, actualMountPoint);
        }

        [Test]
        [TestCase(EEc2VirtualizationType.Hvm)]
        [TestCase(EEc2VirtualizationType.Paravirtual)]
        public void GetMountPointForInstanceWithRootSdfVolumesTest(EEc2VirtualizationType virtualizationType)
        {
            CEc2MachineInfo machine = SEc2ApplianceGenerator.CreateEc2MachineInfo(virtualizationType);
            IReadOnlyList<CEc2VolumeInfo> ec2Volumes = SEc2ApplianceGenerator.CreateEc2VolumeInfos(machine.Id, 2, "/dev/sdf");

            String actualMountPoint = SMountPointFinder.GetMountPoint(machine, ec2Volumes);

            const String expectedMountPoint = "/dev/sdg";

            Assert.IsNotNull(actualMountPoint);
            Assert.AreEqual(expectedMountPoint, actualMountPoint);
        }

        [Test]
        [TestCase(EEc2VirtualizationType.Hvm)]
        [TestCase(EEc2VirtualizationType.Paravirtual)]
        public void GetMountPointForInstanceWithRootSdbSdfVolumesTest(EEc2VirtualizationType virtualizationType)
        {
            CEc2MachineInfo machine = SEc2ApplianceGenerator.CreateEc2MachineInfo(virtualizationType);
            IReadOnlyList<CEc2VolumeInfo> ec2Volumes = SEc2ApplianceGenerator.CreateEc2VolumeInfos(machine.Id, 3, "/dev/sdb", "/dev/sdf");

            String actualMountPoint = SMountPointFinder.GetMountPoint(machine, ec2Volumes);

            const String expectedMountPoint = "/dev/sdg";

            Assert.IsNotNull(actualMountPoint);
            Assert.AreEqual(expectedMountPoint, actualMountPoint);
        }

        [Test]
        [TestCase(EEc2VirtualizationType.Hvm)]
        [TestCase(EEc2VirtualizationType.Paravirtual)]
        public void GetMountPointForInstanceWithRootSdfSdgVolumesTest(EEc2VirtualizationType virtualizationType)
        {
            CEc2MachineInfo machine = SEc2ApplianceGenerator.CreateEc2MachineInfo(virtualizationType);
            IReadOnlyList<CEc2VolumeInfo> ec2Volumes = SEc2ApplianceGenerator.CreateEc2VolumeInfos(machine.Id, 3, "/dev/sdf", "/dev/sdg");

            String actualMountPoint = SMountPointFinder.GetMountPoint(machine, ec2Volumes);

            const String expectedMountPoint = "/dev/sdh";

            Assert.IsNotNull(actualMountPoint);
            Assert.AreEqual(expectedMountPoint, actualMountPoint);
        }

        [Test]
        [TestCase(EEc2VirtualizationType.Hvm)]
        [TestCase(EEc2VirtualizationType.Paravirtual)]
        public void GetMountPointForInstanceWithRootSdbSdfSdgVolumesTest(EEc2VirtualizationType virtualizationType)
        {
            CEc2MachineInfo machine = SEc2ApplianceGenerator.CreateEc2MachineInfo(virtualizationType);
            IReadOnlyList<CEc2VolumeInfo> ec2Volumes = SEc2ApplianceGenerator.CreateEc2VolumeInfos(machine.Id, 4, "/dev/sdb", "/dev/sdf", "/dev/sdg");

            String actualMountPoint = SMountPointFinder.GetMountPoint(machine, ec2Volumes);

            const String expectedMountPoint = "/dev/sdh";

            Assert.IsNotNull(actualMountPoint);
            Assert.AreEqual(expectedMountPoint, actualMountPoint);
        }

        [Test]
        [TestCase(EEc2VirtualizationType.Hvm)]
        [TestCase(EEc2VirtualizationType.Paravirtual)]
        public void GetMountPointForInstanceWithAllVolumesExceptOneFirstTest(EEc2VirtualizationType virtualizationType)
        {
            CEc2MachineInfo machine = SEc2ApplianceGenerator.CreateEc2MachineInfo(virtualizationType);

            String[] deviceNames = SMountPointFinder.DeviceNameSuffixes
                .Skip(1)
                .Select(suffix => SMountPointFinder.DefaultDeviceNamePrefix + suffix)
                .ToArray();

            IReadOnlyList<CEc2VolumeInfo> ec2Volumes = SEc2ApplianceGenerator.CreateEc2VolumeInfos(machine.Id, deviceNames.Length + 1, deviceNames);

            String actualMountPoint = SMountPointFinder.GetMountPoint(machine, ec2Volumes);

            const String expectedMountPoint = "/dev/sdf";

            Assert.IsNotNull(actualMountPoint);
            Assert.AreEqual(expectedMountPoint, actualMountPoint);
        }

        [Test]
        [TestCase(EEc2VirtualizationType.Hvm)]
        [TestCase(EEc2VirtualizationType.Paravirtual)]
        public void GetMountPointForInstanceWithAllVolumesExceptOneLastTest(EEc2VirtualizationType virtualizationType)
        {
            CEc2MachineInfo machine = SEc2ApplianceGenerator.CreateEc2MachineInfo(virtualizationType);

            String[] deviceNames = SMountPointFinder.DeviceNameSuffixes
                .SkipLast(1)
                .Select(suffix => SMountPointFinder.DefaultDeviceNamePrefix + suffix)
                .ToArray();

            IReadOnlyList<CEc2VolumeInfo> ec2Volumes = SEc2ApplianceGenerator.CreateEc2VolumeInfos(machine.Id, deviceNames.Length + 1, deviceNames);

            String actualMountPoint = SMountPointFinder.GetMountPoint(machine, ec2Volumes);

            const String expectedMountPoint = "/dev/sdp";

            Assert.IsNotNull(actualMountPoint);
            Assert.AreEqual(expectedMountPoint, actualMountPoint);
        }

        #endregion

        #region Paravirtual Virtualization Type Tests

        [Test]
        public void CannotGetMountVolumeForHvmTypeTest()
        {
            CEc2MachineInfo machine = SEc2ApplianceGenerator.CreateEc2MachineInfoWithHvmType();

            String[] deviceNames = SMountPointFinder.DeviceNameSuffixes
                .Select(suffix => SMountPointFinder.DefaultDeviceNamePrefix + suffix)
                .ToArray();

            IReadOnlyList<CEc2VolumeInfo> ec2Volumes = SEc2ApplianceGenerator.CreateEc2VolumeInfos(machine.Id, deviceNames.Length + 1, deviceNames);

            String actualMountPoint = null;
            Assert.Throws<InvalidOperationException>(() => actualMountPoint = SMountPointFinder.GetMountPoint(machine, ec2Volumes));

            Assert.IsNull(actualMountPoint);
        }

        #endregion

        #region Paravirtual Virtualization Type Tests

        [Test]
        public void GetMountPointForInstanceWithAllVolumesExceptAdditionalTest()
        {
            CEc2MachineInfo machine = SEc2ApplianceGenerator.CreateEc2MachineInfoWithParavirtualType();

            String[] deviceNames = SMountPointFinder.DeviceNameSuffixes
                .Select(suffix => SMountPointFinder.DefaultDeviceNamePrefix + suffix)
                .ToArray();

            IReadOnlyList<CEc2VolumeInfo> ec2Volumes = SEc2ApplianceGenerator.CreateEc2VolumeInfos(machine.Id, deviceNames.Length + 1, deviceNames);

            String actualMountPoint = SMountPointFinder.GetMountPoint(machine, ec2Volumes);

            const String expectedMountPoint = "/dev/sdf1";

            Assert.IsNotNull(actualMountPoint);
            Assert.AreEqual(expectedMountPoint, actualMountPoint);
        }

        [Test]
        public void GetMountPointForInstanceWithAllVolumesExceptSomeAdditionalFirstCaseTest()
        {
            CEc2MachineInfo machine = SEc2ApplianceGenerator.CreateEc2MachineInfoWithParavirtualType();

            String[] deviceNames = SMountPointFinder.DeviceNameSuffixes
                .Select(suffix => SMountPointFinder.DefaultDeviceNamePrefix + suffix)
                .Append(SMountPointFinder.DefaultDeviceNamePrefix + SMountPointFinder.DeviceNameSuffixes.First() + SMountPointFinder.AdditionalDeviceNameSuffixes.First())
                .ToArray();

            IReadOnlyList<CEc2VolumeInfo> ec2Volumes = SEc2ApplianceGenerator.CreateEc2VolumeInfos(machine.Id, deviceNames.Length + 1, deviceNames);

            String actualMountPoint = SMountPointFinder.GetMountPoint(machine, ec2Volumes);

            const String expectedMountPoint = "/dev/sdf2";

            Assert.IsNotNull(actualMountPoint);
            Assert.AreEqual(expectedMountPoint, actualMountPoint);
        }

        [Test]
        public void GetMountPointForInstanceWithAllVolumesExceptSomeAdditionalSecondCaseTest()
        {
            CEc2MachineInfo machine = SEc2ApplianceGenerator.CreateEc2MachineInfoWithParavirtualType();

            String[] deviceNames = SMountPointFinder.DeviceNameSuffixes
                .SelectMany(suffix => SMountPointFinder.AdditionalDeviceNameSuffixes.SkipLast(1), (suffix, additionalSuffix) => SMountPointFinder.DefaultDeviceNamePrefix + suffix + additionalSuffix)
                .Concat(SMountPointFinder.DeviceNameSuffixes.Select(suffix => SMountPointFinder.DefaultDeviceNamePrefix + suffix))
                .ToArray();

            IReadOnlyList<CEc2VolumeInfo> ec2Volumes = SEc2ApplianceGenerator.CreateEc2VolumeInfos(machine.Id, deviceNames.Length + 1, deviceNames);

            String actualMountPoint = SMountPointFinder.GetMountPoint(machine, ec2Volumes);

            const String expectedMountPoint = "/dev/sdf6";

            Assert.IsNotNull(actualMountPoint);
            Assert.AreEqual(expectedMountPoint, actualMountPoint);
        }

        [Test]
        public void GetMountPointForInstanceWithAllVolumesExceptLastAdditionalTest()
        {
            CEc2MachineInfo machine = SEc2ApplianceGenerator.CreateEc2MachineInfoWithParavirtualType();

            String[] deviceNames = SMountPointFinder.DeviceNameSuffixes
                .SelectMany(suffix => SMountPointFinder.AdditionalDeviceNameSuffixes, (suffix, additionalSuffix) => SMountPointFinder.DefaultDeviceNamePrefix + suffix + additionalSuffix)
                .SkipLast(1)
                .Concat(SMountPointFinder.DeviceNameSuffixes.Select(suffix => SMountPointFinder.DefaultDeviceNamePrefix + suffix))
                .ToArray();

            IReadOnlyList<CEc2VolumeInfo> ec2Volumes = SEc2ApplianceGenerator.CreateEc2VolumeInfos(machine.Id, deviceNames.Length + 1, deviceNames);

            String actualMountPoint = SMountPointFinder.GetMountPoint(machine, ec2Volumes);

            const String expectedMountPoint = "/dev/sdp6";

            Assert.IsNotNull(actualMountPoint);
            Assert.AreEqual(expectedMountPoint, actualMountPoint);
        }

        [Test]
        public void CannotGetMountVolumeForParavirtualTypeTest()
        {
            CEc2MachineInfo machine = SEc2ApplianceGenerator.CreateEc2MachineInfoWithParavirtualType();

            String[] deviceNames = SMountPointFinder.DeviceNameSuffixes
                .SelectMany(suffix => SMountPointFinder.AdditionalDeviceNameSuffixes, (suffix, additionalSuffix) => SMountPointFinder.DefaultDeviceNamePrefix + suffix + additionalSuffix)
                .Concat(SMountPointFinder.DeviceNameSuffixes.Select(suffix => SMountPointFinder.DefaultDeviceNamePrefix + suffix))
                .ToArray();

            IReadOnlyList<CEc2VolumeInfo> ec2Volumes = SEc2ApplianceGenerator.CreateEc2VolumeInfos(machine.Id, deviceNames.Length + 1, deviceNames);

            String actualMountPoint = null;
            Assert.Throws<InvalidOperationException>(() => actualMountPoint = SMountPointFinder.GetMountPoint(machine, ec2Volumes));

            Assert.IsNull(actualMountPoint);
        }

        #endregion
    }
}
