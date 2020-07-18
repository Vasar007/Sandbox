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
    public sealed class CDefaultEc2MachineManager : IEc2MachineManager
    {
        private static readonly String LogTitle = $"[{nameof(CDefaultEc2MachineManager)}]";

        private readonly CAmazonClientFactory _clientFactory;

        internal CDefaultEc2MachineManager(CAmazonClientFactory clientFactory)
        {
            Log.Message(LogLevels.HighDetailed, $"{LogTitle} Creating {nameof(CDefaultEc2MachineManager)} for account '{clientFactory.ProfileName}'.");
            _clientFactory = clientFactory;
        }

        #region Implementation of IEc2MachineManager

        public async Task<IReadOnlyList<CEc2MachineInfo>> GetAllInstancesAsync()
        {
            Log.Message($"{LogTitle} Getting all instances.");

            var result = new List<CEc2MachineInfo>();

            using (IAmazonEC2 client = _clientFactory.CreateEc2Client())
            {
                DescribeInstancesRequest request = SEc2MachineRequestFactory.GetAllInstances(SEc2MachineRequestFactory.MaxResults);
                
                String token = null;

                do
                {
                    if (token.IsSpecified())
                        request.NextToken = token;

                    Func<Task<DescribeInstancesResponse>> func = async () => await client.DescribeInstancesAsync(request);

                    DescribeInstancesResponse response = await func.TryGetEc2Obj();

                    if (response == null)
                        break;

                    Func<Instance, Task<CEc2MachineInfo>> createFunc = async instance =>
                    {
                        GetLaunchTemplateDataRequest launchTemplateDataRequest = SEc2MachineRequestFactory.GetLaunchTemplateData(instance.InstanceId);
                        Log.Error("Get launch template data");
                        ResponseLaunchTemplateData data = null;//await InternalGetLaunchTemplateDataAsync(launchTemplateDataRequest);

                        return instance.CreateMachineInfo(data);
                    };

                    List<Task<CEc2MachineInfo>> tasks = response.Reservations.SelectMany(reservation => reservation.Instances).Select(createFunc).ToList();
                    CEc2MachineInfo[] tasksResult = await Task.WhenAll(tasks);

                    result.AddRange(tasksResult);

                    token = response.NextToken;

                } while (token.IsSpecified());
            }

            return result;
        }

        public async Task<CEc2ConversionTaskInfo> ImportImageAsync(CEc2ImportImageSpec spec)
        {
            Log.Message($"{LogTitle} Importing image. Spec: {spec}");

            using (IAmazonEC2 client = _clientFactory.CreateEc2Client())
            {
                ImportImageRequest request = SEc2MachineRequestFactory.CreateImportImageRequest(spec);

                Func<Task<ImportImageResponse>> func = async () => await client.ImportImageAsync(request);

                ImportImageResponse response = await func.TryInvokeAws();

                Log.Message($"{LogTitle} Task has been created. ID: '{response.ImportTaskId}'.");

                DescribeImportImageTasksRequest descrTaskReq = SEc2MachineRequestFactory.DescribeImportImageTasks(response.ImportTaskId);

                Func<Task<DescribeImportImageTasksResponse>> descrFunc = async () => await client.DescribeImportImageTasksAsync(descrTaskReq);

                DescribeImportImageTasksResponse descrTaskRes = await descrFunc.TryGetEc2Obj();

                if (descrTaskRes == null)
                    throw new CNotFoundException($"Cannot find ImportImageTask: '{response.ImportTaskId}'.");

                ImportImageTask task = descrTaskRes.ImportImageTasks.FirstOrDefault();

                if (task == null)
                    throw new CNotFoundException($"Cannot find ImportImageTask: '{response.ImportTaskId}'.");

                return task.ToAwsConversionTaskInfo();
            }
        }

        public async Task<IReadOnlyList<CEc2ImageInfo>> GetImagesByNamesAsync(IReadOnlyCollection<String> imageNames, CAmazonImageSearchOptions searchOptions)
        {
            Log.Message($"{LogTitle} Getting images by name: {imageNames.EnumerableToLogString()};");

            if (imageNames.IsEmpty())
                throw ExceptionFactory.Create("Cannot find AMI: name has not been passed.");

            using (IAmazonEC2 client = _clientFactory.CreateEc2Client())
            {
                DescribeImagesRequest request = SEc2MachineRequestFactory.GetImagesByName(imageNames, searchOptions);

                Func<Task<DescribeImagesResponse>> getFunc = async () => await client.DescribeImagesAsync(request);

                var result = new List<CEc2ImageInfo>();

                DescribeImagesResponse response = await getFunc.TryGetEc2Obj();

                if (response != null)
                    result.AddRange(response.Images.Select(image => image.CreateImageInfo()));

                return result.ToList();
            }
        }

        public async Task<CEc2MachineInfo> FindInstanceAsync(String instanceId)
        {
            Log.Message($"{LogTitle} Searching instances by ID: '{instanceId}'.");

            DescribeInstancesRequest request = SEc2MachineRequestFactory.FindInstance(instanceId);
            Instance instance = (await InternalGetInstancesAsync(request)).FirstOrDefault(inst => inst.InstanceId.EqualsNoCase(instanceId));

            GetLaunchTemplateDataRequest launchTemplateDataRequest = SEc2MachineRequestFactory.GetLaunchTemplateData(instanceId);
            ResponseLaunchTemplateData data = null;// await InternalGetLaunchTemplateDataAsync(launchTemplateDataRequest);

            return instance?.CreateMachineInfo(data);
        }

        public async Task<CEc2MachineInfo> GetInstanceAsync(String instanceId)
        {
            CEc2MachineInfo instance = await FindInstanceAsync(instanceId);

            if (instance == null)
                throw new CNotFoundException($"[TBD] Cannot find instance: '{instanceId}'.");

            return instance;
        }

        public async Task<IReadOnlyList<CEc2MachineInfo>> GetRunningInstancesAsync()
        {
            Log.Message($"{LogTitle} Getting running instances.");

            DescribeInstancesRequest request = SEc2MachineRequestFactory.GetRunning();

            Func<Instance, Task<CEc2MachineInfo>> func = async instance =>
            {
               GetLaunchTemplateDataRequest launchTemplateDataRequest = SEc2MachineRequestFactory.GetLaunchTemplateData(instance.InstanceId);
               ResponseLaunchTemplateData data = null;//await InternalGetLaunchTemplateDataAsync(launchTemplateDataRequest);

                return instance.CreateMachineInfo(data);
            };

            List<Task<CEc2MachineInfo>> tasks = (await InternalGetInstancesAsync(request)).Select(func).ToList();

            return await Task.WhenAll(tasks);
        }

        public async Task<CEc2MachineInfo> CreateInstanceAsync(CVpcLaunchContext context)
        {
            Log.Message($"{LogTitle} Creating instance. Context: {context}");

            using (IAmazonEC2 client = _clientFactory.CreateEc2Client())
            {
                RunInstancesRequest request = SEc2MachineRequestFactory.RunInstances(context);

                Func<Task<RunInstancesResponse>> getFunc = async () => await client.RunInstancesAsync(request);

                RunInstancesResponse response = await getFunc.TryInvokeAws();

                Func<Instance, Task<CEc2MachineInfo>> func = async instance =>
                {
                    GetLaunchTemplateDataRequest launchTemplateDataRequest = SEc2MachineRequestFactory.GetLaunchTemplateData(instance.InstanceId);
                    ResponseLaunchTemplateData data = null;// await InternalGetLaunchTemplateDataAsync(launchTemplateDataRequest);

                    return instance.CreateMachineInfo(data, response.Reservation.ReservationId);
                };

                Task<CEc2MachineInfo> machineTask = response.Reservation.Instances.Select(func).Single();

                return await machineTask;
            }
        }

        public async Task TerminateInstanceAsync(String instanceId)
        {
            Log.Message($"{LogTitle} Terminating instance by ID: '{instanceId}'.");

            using (IAmazonEC2 client = _clientFactory.CreateEc2Client())
            {
                TerminateInstancesRequest request = SEc2MachineRequestFactory.Terminate(instanceId);

                Func<Task<TerminateInstancesResponse>> func = async () => await client.TerminateInstancesAsync(request);

                TerminateInstancesResponse response = await func.TryInvokeAws();
                
                ValidateInstanceStateChanges(instanceId, response.TerminatingInstances, EEc2MachineState.ShuttingDown, EEc2MachineState.Terminated);
            }
        }

        public async Task StartInstanceAsync(String instanceId)
        {
            Log.Message($"{LogTitle} Starting instance by ID: '{instanceId}'.");

            using (IAmazonEC2 client = _clientFactory.CreateEc2Client())
            {
                StartInstancesRequest request = SEc2MachineRequestFactory.Start(instanceId);

                Func<Task<StartInstancesResponse>> func = async () => await client.StartInstancesAsync(request);

                StartInstancesResponse response = await func.TryInvokeAws();

                ValidateInstanceStateChanges(instanceId, response.StartingInstances, EEc2MachineState.Pending, EEc2MachineState.Running);
            }
        }

        public async Task StopInstanceAsync(String instanceId, Boolean force = false)
        {
            Log.Message($"{LogTitle} Stopping instance by ID: '{instanceId}'.");

            using (IAmazonEC2 client = _clientFactory.CreateEc2Client())
            {
                StopInstancesRequest request = SEc2MachineRequestFactory.Stop(instanceId, force);

                Func<Task<StopInstancesResponse>> func = async () => await client.StopInstancesAsync(request);

                StopInstancesResponse response = await func.TryInvokeAws();

                ValidateInstanceStateChanges(instanceId, response.StoppingInstances, EEc2MachineState.Stopped, EEc2MachineState.Stopping);
            }
        }

        public async Task<CEc2ImageInfo> FindImageAsync(String imageId)
        {
            Log.Message($"{LogTitle} Searching image by ID: '{imageId}'.");

            using (IAmazonEC2 client = _clientFactory.CreateEc2Client())
            {
                DescribeImagesRequest request = SEc2MachineRequestFactory.FindImage(imageId);

                Func<Task<DescribeImagesResponse>> func = async () => await client.DescribeImagesAsync(request);

                DescribeImagesResponse response = await func.TryGetEc2Obj();

                if (response == null || response.Images.IsNullOrEmpty())
                    return null;

                return response.Images.FirstOrDefault(x => x.ImageId.EqualsNoCase(imageId))?.CreateImageInfo();
            }
        }

        public async Task<CEc2ImageInfo> GetImageAsync(String imageId)
        {
            CEc2ImageInfo info = await FindImageAsync(imageId);

            if (info == null)
                throw new CNotFoundException($"[TBD] Cannot find image: '{imageId}'.");

            return info;
        }

        public async Task DeleteImageAsync(String imageId)
        {
            Log.Message($"{LogTitle} Deleting image by ID: '{imageId}'.");

            using (IAmazonEC2 client = _clientFactory.CreateEc2Client())
            {
                DeregisterImageRequest request = SEc2MachineRequestFactory.DeleteImage(imageId);

                Func<Task<DeregisterImageResponse>> func = async () => await client.DeregisterImageAsync(request);

                DeregisterImageResponse response = await func.TryInvokeAws();

                Log.Message($"{LogTitle} Image '{imageId}' has been deleted. Response: {response.HttpStatusCode}.");
            }
        }

        public async Task AttachVolumeAsync(String device, String volumeId, String instanceId)
        {
            Log.Message($"{LogTitle} Attaching volume '{volumeId}' to instance '{instanceId}' as '{device}'.");

            using (IAmazonEC2 client = _clientFactory.CreateEc2Client())
            {
                AttachVolumeRequest request = SEc2VolumesRequestFactory.AttachVolume(device, volumeId, instanceId);

                Func<Task<AttachVolumeResponse>> func = async () => await client.AttachVolumeAsync(request);

                AttachVolumeResponse response = await func.TryInvokeAws();

                Log.Message(LogLevels.HighDetailed, $"{LogTitle} Volume has been attached at {response.Attachment.AttachTime}. Attachment state: {response.Attachment.State.Value}.");
            }
        }

        public async Task DetachVolumeAsync(String volumeId, Boolean force = false)
        {
            Log.Message($"{LogTitle} Detaching volume '{volumeId}'. Force: {force}.");

            using (IAmazonEC2 client = _clientFactory.CreateEc2Client())
            {
                DetachVolumeRequest request = SEc2MachineRequestFactory.DetachVolume(volumeId, force);

                Func<Task<DetachVolumeResponse>> func = async () => await client.DetachVolumeAsync(request);

                DetachVolumeResponse response = await func.TryInvokeAws();

                Log.Message(LogLevels.HighDetailed, $"{LogTitle} Volume has been detached at {response.Attachment.AttachTime}. Attachment state: {response.Attachment.State.Value}.");
            }
        }
        
        #endregion

        private static void ValidateInstanceStateChanges(String instanceId, IEnumerable<InstanceStateChange> changes, params EEc2MachineState[] allowedStates)
        {
            InstanceStateChange stateChange = changes.FirstOrDefault(x => x.InstanceId.EqualsNoCase(instanceId));

            if (stateChange == null)
                throw ExceptionFactory.Create($"[TBD] State of instance '{instanceId}' has not been changed.");

            EEc2MachineState ec2MachineState = stateChange.CurrentState.ResolveMachineState();

            if (allowedStates.Contains(ec2MachineState))
                return;

            String expectedStates = allowedStates.Select(machineState => machineState.ToString()).Aggregate((all, machineStateString) => $"{all}, {machineStateString}");
            throw ExceptionFactory.Create($"[TBD] Unexpected state of instance '{instanceId}': {ec2MachineState} (expected: {expectedStates}).");
        }

        private async Task<IReadOnlyList<Instance>> InternalGetInstancesAsync(DescribeInstancesRequest request)
        {
            using (IAmazonEC2 client = _clientFactory.CreateEc2Client())
            {
                Func<Task<DescribeInstancesResponse>> func = async () => await client.DescribeInstancesAsync(request);

                DescribeInstancesResponse response = await func.TryGetEc2Obj();

                if (response == null || response.Reservations.IsNullOrEmpty())
                    return new Instance[0];

                return response.Reservations.SelectMany(reservation => reservation.Instances).ToList();
            }
        }

        private async Task<ResponseLaunchTemplateData> InternalGetLaunchTemplateDataAsync(GetLaunchTemplateDataRequest request)
        {
            using (IAmazonEC2 client = _clientFactory.CreateEc2Client())
            {
                Func<Task<GetLaunchTemplateDataResponse>> func = async () => await client.GetLaunchTemplateDataAsync(request);
                
                GetLaunchTemplateDataResponse response = await func.TryGetEc2Obj();

                return response?.LaunchTemplateData;
            }
        }
    }
}
