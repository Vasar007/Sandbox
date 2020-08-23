using System;
using System.Threading.Tasks;

namespace Sandbox.DataflowPipeline
{
    internal static class FuncHelper
    {
        public static TOutput WrapFuncWithCatch<TInput, TOutput, TSource>(
            Func<TInput, TOutput> transformSync,
            TInput input,
            TaskCompletionSource<TSource> taskCompletionSource)
        {
            try
            {
                return transformSync(input);
            }
            catch (Exception ex)
            {
                taskCompletionSource.SetException(ex);
                return default!;
            }
        }

        public static async Task<TOutput> WrapFuncWithCatch<TInput, TOutput, TSource>(
            Func<TInput, TOutput> transformSync,
            Task<TInput> asyncInput,
            TaskCompletionSource<TSource> taskCompletionSource)
        {
            try
            {
                return transformSync(await asyncInput);
            }
            catch (Exception ex)
            {
                taskCompletionSource.SetException(ex);
                return default!;
            }
        }

        public static async Task<TOutput> WrapFuncWithCatch<TInput, TOutput, TSource>(
            Func<TInput, Task<TOutput>> transformAsync,
            TInput input,
            TaskCompletionSource<TSource> taskCompletionSource)
        {
            try
            {
                return await transformAsync(input);
            }
            catch (Exception ex)
            {
                taskCompletionSource.SetException(ex);
                return default!;
            }
        }

        public static async Task<TOutput> WrapFuncWithCatch<TInput, TOutput, TSource>(
            Func<TInput, Task<TOutput>> transformAsync,
            Task<TInput> asyncInput,
            TaskCompletionSource<TSource> taskCompletionSource)
        {
            try
            {
                return await transformAsync(await asyncInput);
            }
            catch (Exception ex)
            {
                taskCompletionSource.SetException(ex);
                return default!;
            }
        }
    }
}
