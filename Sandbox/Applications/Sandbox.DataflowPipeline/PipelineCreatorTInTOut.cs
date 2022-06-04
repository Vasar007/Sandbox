using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Sandbox.DataflowPipeline
{
    public sealed class PipelineCreator<TIn, TOut>
    {
        private readonly List<IDataflowBlock> _transformBlocks = new();


        public PipelineCreator()
        {
        }

        public PipelineCreator<TIn, TOut> AddStep<TLocalIn, TLocalOut>(
            Func<TLocalIn, TLocalOut> stepFunc)
        {
            var step = new TransformBlock<TC<TLocalIn, TOut>, TC<TLocalOut, TOut>>(tc =>
            {
                try
                {
                    return new TC<TLocalOut, TOut>(stepFunc(tc.Input), tc.TaskCompletionSource);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Have an exception, yes? {ex}");
                    tc.TaskCompletionSource.SetException(ex);
                    return new TC<TLocalOut, TOut>(default!, tc.TaskCompletionSource);
                }
            });

            if (_transformBlocks.Count > 0)
            {
                IDataflowBlock lastStep = _transformBlocks.Last();
                var targetBlock = (ISourceBlock<TC<TLocalIn, TOut>>) lastStep;

                targetBlock.LinkTo(
                    step,
                    new DataflowLinkOptions(),
                    tc => !tc.TaskCompletionSource.Task.IsFaulted
                );

                targetBlock.LinkTo(
                    DataflowBlock.NullTarget<TC<TLocalIn, TOut>>(),
                    new DataflowLinkOptions(),
                    tc => tc.TaskCompletionSource.Task.IsFaulted
                );
            }

            _transformBlocks.Add(step);
            return this;
        }

        public PipelineCreator<TIn, TOut> CreatePipeline()
        {
            var setResultStep = new ActionBlock<TC<TOut, TOut>>(
                tc => tc.TaskCompletionSource.SetResult(tc.Input)
            );

            IDataflowBlock lastStep = _transformBlocks.Last();
            var setResultBlock = (ISourceBlock<TC<TOut, TOut>>) lastStep;

            setResultBlock.LinkTo(setResultStep);

            return this;
        }

        public Task<TOut> Execute(TIn input)
        {
            var firstStep = (ITargetBlock<TC<TIn, TOut>>) _transformBlocks.First();
            var tcs = new TaskCompletionSource<TOut>();

            firstStep.SendAsync(new TC<TIn, TOut>(input, tcs));

            return tcs.Task;
        }
    }
}
