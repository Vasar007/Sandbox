using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using Gridsum.DataflowEx;

namespace DataflowPipeline
{
    public sealed class CrawlersFlow : Dataflow<IReadOnlyList<string>, IReadOnlyList<IData>>
    {
        private readonly Dataflow<IReadOnlyList<string>, IReadOnlyList<string>> _inputBroadcaster;

        private readonly Dataflow<IReadOnlyList<IData>, IReadOnlyList<IData>> _resultConsumer;

        public override ITargetBlock<IReadOnlyList<string>> InputBlock =>
            _inputBroadcaster.InputBlock;

        public override ISourceBlock<IReadOnlyList<IData>> OutputBlock =>
            _resultConsumer.OutputBlock;


        public CrawlersFlow(IEnumerable<Func<IReadOnlyList<string>, IReadOnlyList<IData>>> crawlers)
            : base(DataflowOptions.Default)
        {
            if (crawlers is null)
            {
                throw new ArgumentNullException(nameof(crawlers));
            }

            _inputBroadcaster = new DataBroadcaster<IReadOnlyList<string>>(filteredData =>
            {
                Console.WriteLine($"Broadcasts all filtered inputters data. {filteredData.Count.ToString()}");
                return filteredData;
            }, DataflowOptions.Default);

            _resultConsumer = new TransformBlock<IReadOnlyList<IData>, IReadOnlyList<IData>>(
                crawlersData => crawlersData
            ).ToDataflow(DataflowOptions.Default);

            var crawlerFlows = crawlers
                .Select(crawler => DataflowUtils.FromDelegate(crawler, DataflowOptions.Default));
            foreach (var crawlerFlow in crawlerFlows)
            {
                _inputBroadcaster.LinkTo(crawlerFlow);
                crawlerFlow.LinkTo(_resultConsumer);

                _resultConsumer.RegisterDependency(crawlerFlow);
                RegisterChild(crawlerFlow);
            }

            RegisterChild(_inputBroadcaster);
            RegisterChild(_resultConsumer);
        }
    }
}
