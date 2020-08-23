using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using Gridsum.DataflowEx;

namespace DataflowPipeline
{
    public sealed class OutputtersFlow : Dataflow<IReadOnlyList<string>>
    {
        private readonly ConcurrentBag<string> _resultList;

        private readonly Dataflow<IReadOnlyList<string>, IReadOnlyList<string>> _inputConsumer;

        public override ITargetBlock<IReadOnlyList<string>> InputBlock =>
            _inputConsumer.InputBlock;

        public IEnumerable<string> Results => _resultList;


        public OutputtersFlow(
            IEnumerable<Action<IReadOnlyList<string>>> outputters)
            : base(DataflowOptions.Default)
        {
            if (outputters is null)
            {
                throw new ArgumentNullException(nameof(outputters));
            }

            _resultList = new ConcurrentBag<string>();

            _inputConsumer = new DataBroadcaster<IReadOnlyList<string>>(appraisersData =>
            {
                Console.WriteLine($"Consuming all appraised data. {appraisersData.Count.ToString()}\n");
                foreach (string datum in appraisersData)
                {
                    _resultList.Add(datum);
                }
                return appraisersData;
            }, DataflowOptions.Default);

            var outputterFlows = outputters
                .Select(outputters => DataflowUtils.FromDelegate(outputters, DataflowOptions.Default));
            foreach (var outputterFlow in outputterFlows)
            {
                _inputConsumer.LinkTo(outputterFlow);

                RegisterChild(outputterFlow);
            }

            RegisterChild(_inputConsumer);
        }

        public void Clear()
        {
            _resultList.Clear();
        }
    }
}
