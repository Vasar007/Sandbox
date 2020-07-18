using System.Threading.Tasks.Dataflow;

namespace DataflowPipeline
{
    public static class PipelineBlockFactory
    {
        public static PipelineBlock CreateCommonBlock<TInput, TOutput>(string name,
           IDataflowBlock dataflowBlock)
        {
            return PipelineBlock.CreateCommonBlock(name, dataflowBlock);
        }

        public static PipelineBlock<TKey> CreateKeyBlock<TKey>(string name, TKey key,
            IDataflowBlock dataflowBlock)
        {
            return PipelineBlock<TKey>.CreateKeyBlock(name, key, dataflowBlock);
        }
    }
}
