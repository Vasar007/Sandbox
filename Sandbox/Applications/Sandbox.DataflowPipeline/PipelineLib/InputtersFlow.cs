using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using Gridsum.DataflowEx;

namespace Sandbox.DataflowPipeline
{
    public sealed class InputtersFlow : Dataflow<string, IReadOnlyList<string>>
    {
        private readonly ConcurrentDictionary<string, byte> _filteringSet;

        private readonly Dataflow<string, string> _inputBroadcaster;

        private readonly Dataflow<IReadOnlyList<string>, IReadOnlyList<string>> _resultTransformer;

        public override ITargetBlock<string> InputBlock => _inputBroadcaster.InputBlock;

        public override ISourceBlock<IReadOnlyList<string>> OutputBlock =>
            _resultTransformer.OutputBlock;


        public InputtersFlow(IEnumerable<Func<string, IReadOnlyList<string>>> inputters)
            : base(DataflowOptions.Default)
        {
            if (inputters is null)
            {
                throw new ArgumentNullException(nameof(inputters));
            }

            _filteringSet = new ConcurrentDictionary<string, byte>();

            _inputBroadcaster = new DataBroadcaster<string>(input =>
            {
                Console.WriteLine("Broadcasts input to further blocks.");
                return input;
            }, DataflowOptions.Default);

            _resultTransformer =
                DataflowUtils.FromDelegate<IReadOnlyList<string>, IReadOnlyList<string>>(
                    FilterInputData, DataflowOptions.Default
                );

            var inputFlows = inputters
                .Select(inputter => DataflowUtils.FromDelegate(inputter, DataflowOptions.Default));
            foreach (var inputFlow in inputFlows)
            {
                _inputBroadcaster.LinkTo(inputFlow);
                inputFlow.LinkTo(_resultTransformer);

                _resultTransformer.RegisterDependency(inputFlow);
                RegisterChild(inputFlow);
            }

            RegisterChild(_inputBroadcaster);
            RegisterChild(_resultTransformer);
        }

        protected override void CleanUp(Exception dataflowException)
        {
            _filteringSet.Clear();

            base.CleanUp(dataflowException);
        }

        private IReadOnlyList<string> FilterInputData(IReadOnlyList<string> inputtersData)
        {
            Console.WriteLine($"Filtering all inputters data. {inputtersData.Count.ToString()}");
            var result = new List<string>();
            foreach (string datum in inputtersData)
            {
                if (datum.Length > 2 && _filteringSet.TryAdd(datum, default))
                {
                    result.Add(datum);
                }
            }

            return result;
        }
    }
}
