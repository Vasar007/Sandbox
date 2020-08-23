using System;
using System.Threading.Tasks.Dataflow;

namespace Sandbox.DataflowPipeline
{
    public sealed class PipelineBlock<TKey>
    {
        private readonly IdentifierData<TKey> _identifierData;

        private readonly IDataflowBlock _dataflowBlock;

        public Guid Id => _identifierData.Id;

        public string Name => _identifierData.Name;

        public TKey Key => _identifierData.Key;


        private PipelineBlock(IdentifierData<TKey> identifierData, IDataflowBlock dataflowBlock)
        {
            _identifierData = identifierData
                ?? throw new ArgumentNullException(nameof(identifierData));

            _dataflowBlock = dataflowBlock
                ?? throw new ArgumentNullException(nameof(dataflowBlock));
        }

        public static PipelineBlock<TKey> CreateKeyBlock(string name, TKey key,
            IDataflowBlock dataflowBlock)
        {
            IdentifierData<TKey> identifierData = IdentifierDataFactory.CreateKeyData(name, key);
            return new PipelineBlock<TKey>(identifierData, dataflowBlock);
        }
    }
}
