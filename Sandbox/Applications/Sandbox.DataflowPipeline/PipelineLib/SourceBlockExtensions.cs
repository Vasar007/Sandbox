using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Sandbox.DataflowPipeline
{
    internal static class SourceBlockExtensions
    {
        public static IDataflowBlock LinkAsyncBlockWithSync<TInput, TOutput, TSource>(
           this ISourceBlock<Task<TInput>> asyncSourceBlock,
           Func<TInput, TOutput> transformSync,
           TaskCompletionSource<TSource> taskCompletionSource)
        {
            var newAsyncBlock = new TransformBlock<Task<TInput>, Task<TOutput>>(
                input => FuncHelper.WrapFuncWithCatch(transformSync, input, taskCompletionSource)
            );

            asyncSourceBlock.LinkDependingOnTaskState(newAsyncBlock, taskCompletionSource);

            return newAsyncBlock;
        }

        public static IDataflowBlock LinkAsyncBlockWithAsync<TInput, TOutput, TSource>(
            this ISourceBlock<Task<TInput>> asyncSourceBlock,
            Func<TInput, Task<TOutput>> transformAsync,
            TaskCompletionSource<TSource> taskCompletionSource)
        {
            var newSyncBlock = new TransformBlock<Task<TInput>, Task<TOutput>>(
                input => FuncHelper.WrapFuncWithCatch(transformAsync, input, taskCompletionSource)
            );

            asyncSourceBlock.LinkDependingOnTaskState(newSyncBlock, taskCompletionSource);

            return newSyncBlock;
        }

        public static IDataflowBlock LinkSyncBlockWithSync<TInput, TOutput, TSource>(
            this ISourceBlock<TInput> syncSourceBlock,
            Func<TInput, TOutput> transformSync,
            TaskCompletionSource<TSource> taskCompletionSource)
        {
            var newSyncBlock = new TransformBlock<TInput, TOutput>(
                input => FuncHelper.WrapFuncWithCatch(transformSync, input, taskCompletionSource)
            );

            syncSourceBlock.LinkDependingOnTaskState(newSyncBlock, taskCompletionSource);

            return newSyncBlock;
        }

        public static IDataflowBlock LinkSyncBlockWithAsync<TInput, TOutput, TSource>(
            this ISourceBlock<TInput> syncSourceBlock,
            Func<TInput, Task<TOutput>> transformAsync,
            TaskCompletionSource<TSource> taskCompletionSource)
        {
            var newAsyncBlock = new TransformBlock<TInput, Task<TOutput>>(
                input => FuncHelper.WrapFuncWithCatch(transformAsync, input, taskCompletionSource)
            );

            syncSourceBlock.LinkDependingOnTaskState(newAsyncBlock, taskCompletionSource);

            return newAsyncBlock;
        }

        private static void LinkDependingOnTaskState<TInput, TSource>(
            this ISourceBlock<TInput> sourceBlock,
            ITargetBlock<TInput> targetBlock,
            TaskCompletionSource<TSource> taskCompletionSource)
        {
            sourceBlock.LinkTo(
                targetBlock,
                new DataflowLinkOptions(),
                _ => !taskCompletionSource.Task.IsFaulted
            );

            sourceBlock.LinkTo(
                DataflowBlock.NullTarget<TInput>(),
                new DataflowLinkOptions(),
                _ => taskCompletionSource.Task.IsFaulted
            );
        }
    }
}
