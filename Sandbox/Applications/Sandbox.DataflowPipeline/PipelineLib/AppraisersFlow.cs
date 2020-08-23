using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using Gridsum.DataflowEx;

namespace Sandbox.DataflowPipeline
{
    public sealed class AppraisersFlow : Dataflow<IReadOnlyList<IData>, IReadOnlyList<string>>
    {
        private readonly Dataflow<IReadOnlyList<IData>, IReadOnlyList<IData>> _inputConsumer;

        private readonly Dataflow<IReadOnlyList<string>, IReadOnlyList<string>> _resultConsumer;

        public override ITargetBlock<IReadOnlyList<IData>> InputBlock =>
            _inputConsumer.InputBlock;

        public override ISourceBlock<IReadOnlyList<string>> OutputBlock =>
            _resultConsumer.OutputBlock;


        public AppraisersFlow(IEnumerable<Appraiser> appraisers)
            : base(DataflowOptions.Default)
        {
            if (appraisers is null)
            {
                throw new ArgumentNullException(nameof(appraisers));
            }


            _inputConsumer = new DataBroadcaster<IReadOnlyList<IData>>(crawlersData =>
            {
                Console.WriteLine($"Broadcasts all crawlers data. {crawlersData.Count.ToString()}");
                return crawlersData;
            }, DataflowOptions.Default);

            _resultConsumer = new TransformBlock<IReadOnlyList<string>, IReadOnlyList<string>>(
                appraisedData => appraisedData
            ).ToDataflow(DataflowOptions.Default);

            var usedTypes = new Dictionary<Type, DataBroadcaster<IReadOnlyList<IData>>>();
            foreach (var appraiser in appraisers)
            {
                if (!usedTypes.TryGetValue(appraiser.DataType, out var broadcaster))
                {
                    broadcaster = new DataBroadcaster<IReadOnlyList<IData>>(crawlersData =>
                    {
                        Console.WriteLine($"Broadcasts specified data of type {appraiser.DataType.Name}. {crawlersData.Count.ToString()}");
                        return crawlersData;
                    }, DataflowOptions.Default);

                    usedTypes.Add(appraiser.DataType, broadcaster);
                    _inputConsumer.TransformAndLink(
                        broadcaster,
                        l => l,
                        l => l.All(d => d.GetType().IsAssignableFrom(appraiser.DataType))
                    );
                    RegisterChild(broadcaster);
                }
                var appraiserFlow = DataflowUtils.FromDelegate(appraiser.Func, DataflowOptions.Default);
                broadcaster.LinkTo(appraiserFlow);
                appraiserFlow.LinkTo(_resultConsumer);

                _resultConsumer.RegisterDependency(appraiserFlow);
                RegisterChild(appraiserFlow);
            }

            RegisterChild(_inputConsumer);
            RegisterChild(_resultConsumer);
        }
    }
}
