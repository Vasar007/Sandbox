using System;
using System.Threading.Tasks.Dataflow;

namespace Sandbox.DataflowPipeline
{
    public sealed class PipelineBlock
    {
        private readonly IdentifierData _identifierData;

        private readonly IDataflowBlock _dataflowBlock;

        public Guid Id => _identifierData.Id;

        public string Name => _identifierData.Name;


        private PipelineBlock(IdentifierData identifierData, IDataflowBlock dataflowBlock)
        {
            _identifierData = identifierData
                ?? throw new ArgumentNullException(nameof(identifierData));

            _dataflowBlock = dataflowBlock
                ?? throw new ArgumentNullException(nameof(dataflowBlock));
        }

        public static PipelineBlock CreateCommonBlock(string name,
            IDataflowBlock dataflowBlock)
        {
            IdentifierData identifierData = IdentifierDataFactory.CreateCommonData(name);
            return new PipelineBlock(identifierData, dataflowBlock);
        }
    }
}
