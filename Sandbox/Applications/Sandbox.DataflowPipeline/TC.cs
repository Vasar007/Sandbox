using System.Threading.Tasks;

namespace Sandbox.DataflowPipeline
{
    public sealed class TC<TInput, TOutput>
    {
        public TInput Input { get; set; }

        public TaskCompletionSource<TOutput> TaskCompletionSource { get; set; }


        public TC(TInput input, TaskCompletionSource<TOutput> tcs)
        {
            Input = input;
            TaskCompletionSource = tcs ?? throw new System.ArgumentNullException(nameof(tcs));
        }
    }
}
