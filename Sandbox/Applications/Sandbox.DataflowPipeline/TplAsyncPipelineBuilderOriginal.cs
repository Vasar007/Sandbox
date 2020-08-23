using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Sandbox.DataflowPipeline
{
    public sealed class TplAsyncPipelineBuilderOriginal<TInput> :
        TplAsyncPipelineBuilderOriginal<TInput, TInput>
    {
        public TplAsyncPipelineBuilderOriginal()
        {
        }
    }

    public class TplAsyncPipelineBuilderOriginal<TFirstInput, TCurrentInput>
    {
        private readonly IDataflowBlock? _firstStep;

        private readonly IDataflowBlock? _lastStep;


        public TplAsyncPipelineBuilderOriginal()
            : this(null, null)
        {
        }

        private TplAsyncPipelineBuilderOriginal(IDataflowBlock? firstStep, IDataflowBlock? lastStep)
        {
            _firstStep = firstStep;
            _lastStep = lastStep;
        }

        public TplAsyncPipelineBuilderOriginal<TFirstInput, TOutput> AddStep<TOutput>(
            Func<TCurrentInput, Task<TOutput>> transformAsync)
        {
            IDataflowBlock lastStep = _lastStep switch
            {
                null => new TransformBlock<TCurrentInput, Task<TOutput>>(
                            input => transformAsync(input)
                        ),

                ISourceBlock<Task<TCurrentInput>> asyncSourceBlock =>
                    LinkAsyncBlock(asyncSourceBlock, transformAsync),

                ISourceBlock<TCurrentInput> sourceBlock =>
                    LinkSyncBlock(sourceBlock, transformAsync),

                _ => throw new InvalidOperationException(
                        "Cannot find proper matching for the last step block."
                     )
            };

            IDataflowBlock firstStep = _firstStep ?? lastStep;

            return new TplAsyncPipelineBuilderOriginal<TFirstInput, TOutput>(
                firstStep, lastStep
            );
        }

        public TplAsyncPipelineBuilderOriginal<TFirstInput, TOutput> AddStep<TOutput>(
            Func<TCurrentInput, TOutput> transform)
        {
            IDataflowBlock lastStep = _lastStep switch
            {
                null => new TransformBlock<TCurrentInput, TOutput>(
                            input => transform(input)
                        ),

                ISourceBlock<Task<TCurrentInput>> asyncSourceBlock =>
                    LinkAsyncBlock(asyncSourceBlock, transform),

                ISourceBlock<TCurrentInput> sourceBlock =>
                    LinkSyncBlock(sourceBlock, transform),

                _ => throw new InvalidOperationException(
                         "Cannot find proper matching for the last step block."
                     )
            };

            IDataflowBlock firstStep = _firstStep ?? lastStep;

            return new TplAsyncPipelineBuilderOriginal<TFirstInput, TOutput>(
                firstStep, lastStep
            );
        }

        public TplAsyncPipeline<TFirstInput, TCurrentInput> Build()
        {
            if (!(_firstStep is ITargetBlock<TFirstInput> firstStep))
            {
                throw new InvalidOperationException("The first step block is not initialized.");
            }

            var completionTask = new TaskCompletionSource<TCurrentInput>();
            AddStep(input =>
            {
                completionTask.SetResult(input);
                return input;
            });

            return new TplAsyncPipeline<TFirstInput, TCurrentInput>(
                firstStep, completionTask.Task
            );
        }

        private static IDataflowBlock LinkAsyncBlock<TInput, TOutput>(
           ISourceBlock<Task<TInput>> asyncSourceBlock,
           Func<TInput, TOutput> transform)
        {
            var newAsyncBlock = new TransformBlock<Task<TInput>, Task<TOutput>>(
                async input => transform(await input)
            );

            asyncSourceBlock.LinkTo(newAsyncBlock);

            return newAsyncBlock;
        }

        private static IDataflowBlock LinkAsyncBlock<TInput, TOutput>(
            ISourceBlock<Task<TInput>> asyncSourceBlock,
            Func<TInput, Task<TOutput>> transform)
        {
            var newAsyncBlock = new TransformBlock<Task<TInput>, Task<TOutput>>(
                async input => await transform(await input)
            );

            asyncSourceBlock.LinkTo(newAsyncBlock);

            return newAsyncBlock;
        }

        private static IDataflowBlock LinkSyncBlock<TInput, TOutput>(
            ISourceBlock<TInput> sourceBlock,
            Func<TInput, TOutput> transform)
        {
            var newAsyncBlock = new TransformBlock<TInput, TOutput>(
                input => transform(input)
            );

            sourceBlock.LinkTo(newAsyncBlock);

            return newAsyncBlock;
        }

        public static IDataflowBlock LinkSyncBlock<TInput, TOutput>(
            ISourceBlock<TInput> sourceBlock,
            Func<TInput, Task<TOutput>> transform)
        {
            var newAsyncBlock = new TransformBlock<TInput, Task<TOutput>>(
                input => transform(input)
            );

            sourceBlock.LinkTo(newAsyncBlock);

            return newAsyncBlock;
        }
    }
}
