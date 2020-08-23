using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Sandbox.DataflowPipeline
{
    public sealed class TplAsyncPipeline<TInput, TOutput>
    {
        private readonly ITargetBlock<TInput> _firstStep;

        private readonly Task<TOutput> _completionTask;


        public TplAsyncPipeline(ITargetBlock<TInput> firstStep, Task<TOutput> completionTask)
        {
            _firstStep = firstStep ?? throw new ArgumentNullException(nameof(firstStep));

            _completionTask =
                completionTask ?? throw new ArgumentNullException(nameof(completionTask));
        }

        public async Task<TOutput> Execute(TInput input)
        {
            bool hasSent = await _firstStep.SendAsync(input);
            if (!hasSent)
            {
                throw new InvalidOperationException("Pipeline cannot send initial data.");
            }

            TOutput result = await _completionTask;
            return result;
        }
    }
}
