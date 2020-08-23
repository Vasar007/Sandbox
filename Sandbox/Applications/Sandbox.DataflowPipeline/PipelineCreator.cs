using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Sandbox.DataflowPipeline
{
    public static class PipelineCreator
    {
        public static TransformBlock<string, string> CreateSimplePipeline(
            Action<bool> resultCallback)
        {
            var step1 = new TransformBlock<string, string>(
                sentence => Utils.FindMostCommon(sentence),
                new ExecutionDataflowBlockOptions()
                {
                    MaxDegreeOfParallelism = 3,
                    BoundedCapacity = 5,
                }
            );
            
            var step2 = new TransformBlock<string, int>(
                word => Utils.CountChars(word),
                new ExecutionDataflowBlockOptions()
                {
                    MaxDegreeOfParallelism = 1,
                    BoundedCapacity = 13,
                }
            );

            var step3 = new TransformBlock<int, bool>(
                length => Utils.IsOdd(length),
                new ExecutionDataflowBlockOptions()
                {
                    MaxDegreeOfParallelism = 11,
                    BoundedCapacity = 6,
                }
            );

            var callbackStep = new ActionBlock<bool>(resultCallback);

            step1.LinkTo(step2, new DataflowLinkOptions());
            step2.LinkTo(step3, new DataflowLinkOptions());
            step3.LinkTo(callbackStep);
            
            return step1;
        }

        public static TransformBlock<TC<string, bool>, TC<string, bool>> CreateTcPipeline()
        {
            var step1 = new TransformBlock<TC<string, bool>, TC<string, bool>>(
                tc => new TC<string, bool>(
                    Utils.FindMostCommon(tc.Input),
                    tc.TaskCompletionSource
                )
            );

            var step2 = new TransformBlock<TC<string, bool>, TC<int, bool>>(
                tc => new TC<int, bool>(
                    tc.Input.Length,
                    tc.TaskCompletionSource
                )
            );

            var step3 = new TransformBlock<TC<int, bool>, TC<bool, bool>>(tc =>
                new TC<bool, bool>(
                    Utils.IsOdd(tc.Input),
                    tc.TaskCompletionSource
                )
            );

            var setResultStep = new ActionBlock<TC<bool, bool>>(
                tc => tc.TaskCompletionSource.SetResult(tc.Input)
            );

            step1.LinkTo(step2, new DataflowLinkOptions());
            step2.LinkTo(step3, new DataflowLinkOptions());
            step3.LinkTo(setResultStep, new DataflowLinkOptions());

            return step1;
        }

        public static async Task CreateComplexPipelineAndExecuteAsync()
        {
            // Input point.
            var inputPoint = new BroadcastBlock<string>(input =>
            {
                Console.WriteLine("Broadcasts input to further blocks.");
                return input;
            });

            // Inputters data.
            var inputter1 = new TransformBlock<string, string[]>(input =>
            {
                Console.WriteLine("Inputter 1 transforms inputs.");
                return input.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            });
            var inputter2 = new TransformBlock<string, string[]>(input =>
            {
                Console.WriteLine("Inputter 2 transforms inputs.");
                return input.Split("_", StringSplitOptions.RemoveEmptyEntries);
            });
            var inputter3 = new TransformBlock<string, string[]>(input =>
            {
                Console.WriteLine("Inputter 3 transforms inputs.");
                return input.Split("=", StringSplitOptions.RemoveEmptyEntries);
            });

            // Filtering inputters data.
            var filteringSet = new ConcurrentDictionary<string, byte>();
            var filtering = new TransformBlock<string[], string[]>(inputtersData =>
            {
                Console.WriteLine($"Filtering all inputters data. {inputtersData.Length.ToString()}");
                var result = new List<string>();
                foreach (string datum in inputtersData)
                {
                    if (datum.Length > 2 && filteringSet.TryAdd(datum, default))
                    {
                        result.Add(datum);
                    }
                }

                return result.ToArray();
            });

            // Broadcasting inputters data.
            var broadcastInput = new BroadcastBlock<string[]>(filteredInputtersData =>
            {
                Console.WriteLine($"\nBroadcasts all filtered inputters data. {filteredInputtersData.Length.ToString()}");
                return filteredInputtersData;
            });

            // Crawlers data.
            var crawler1 = new TransformBlock<string[], IData[]>(filteredInputtersData =>
            {
                Console.WriteLine($"Crawler 1 transforms filtered inputters data. {filteredInputtersData.Length.ToString()}");
                return filteredInputtersData
                    .Select(datum => new Data1(datum.Length))
                    .ToArray();
            });
            var crawler2 = new TransformBlock<string[], IData[]>(filteredInputtersData =>
            {
                Console.WriteLine($"Crawler 2 transforms filtered inputters data. {filteredInputtersData.Length.ToString()}");
                return filteredInputtersData
                    .Select(datum => new Data2(datum))
                    .ToArray();
            });
            var crawler3 = new TransformBlock<string[], IData[]>(filteredInputtersData =>
            {
                Console.WriteLine($"Crawler 3 transforms filtered inputters data. {filteredInputtersData.Length.ToString()}");
                return filteredInputtersData
                    .Select(datum => new Data3(datum.Length + 42.5))
                    .ToArray();
            });

            // Broadcasting crawlers data.
            var broadcastCrawler1 = new BroadcastBlock<IData[]>(crawlersData =>
            {
                Console.WriteLine($"Broadcasts crawler's 1 data. {crawlersData.Length.ToString()}\n");
                return crawlersData;
            });
            var broadcastCrawler2 = new BroadcastBlock<IData[]>(crawlersData =>
            {
                Console.WriteLine($"Broadcasts crawler's 2 data. {crawlersData.Length.ToString()}\n");
                return crawlersData;
            });
            var broadcastCrawler3 = new BroadcastBlock<IData[]>(crawlersData =>
            {
                Console.WriteLine($"Broadcasts crawler's 3 data. {crawlersData.Length.ToString()}\n");
                return crawlersData;
            });

            // Appraisers.
            var appraiser11 = new TransformBlock<IData[], string[]>(crawlersData =>
            {
                Console.WriteLine($"Appraiser 1-1 transforms crawler's 1 data. {crawlersData.Length.ToString()}");
                return crawlersData
                    .Cast<Data1>()
                    .Select(datum => datum.Value.ToString() + "|11|")
                    .ToArray();
            });
            var appraiser12 = new TransformBlock<IData[], string[]>(crawlersData =>
            {
                Console.WriteLine($"Appraiser 1-2 transforms crawler's 1 data. {crawlersData.Length.ToString()}");
                return crawlersData
                    .Cast<Data1>()
                    .Select(datum => datum.Value.ToString() + "|12|")
                    .ToArray();
            });

            var appraiser21 = new TransformBlock<IData[], string[]>(crawlersData =>
            {
                Console.WriteLine($"Appraiser 2-1 transforms filtered crawler's 2 data. {crawlersData.Length.ToString()}");
                return crawlersData
                    .Cast<Data2>()
                    .Select(datum => datum.Value + "|21|")
                    .ToArray();
            });
            var appraiser22 = new TransformBlock<IData[], string[]>(crawlersData =>
            {
                Console.WriteLine($"Appraiser 2-2 transforms filtered crawler's 2 data. {crawlersData.Length.ToString()}");
                return crawlersData
                    .Cast<Data2>()
                    .Select(datum => datum.Value + "|22|")
                    .ToArray();
            });

            var appraiser31 = new TransformBlock<IData[], string[]>(crawlersData =>
            {
                Console.WriteLine($"Appraiser 3-1 transforms filtered crawler's 3 data. {crawlersData.Length.ToString()}");
                return crawlersData
                    .Cast<Data3>()
                    .Select(datum => datum.Value.ToString() + "|31|")
                    .ToArray();
            });
            var appraiser32 = new TransformBlock<IData[], string[]>(crawlersData =>
            {
                Console.WriteLine($"Appraiser 3-2 transforms filtered crawler's 3 data. {crawlersData.Length.ToString()}");
                return crawlersData
                    .Cast<Data3>()
                    .Select(datum => datum.Value.ToString() + "|32|")
                    .ToArray();
            });

            // Consuming appraisers data.
            var resultList = new ConcurrentBag<string>();
            var consumeAllData = new ActionBlock<string[]>(appraisersData =>
            {
                Console.WriteLine($"Consuming all appraisers data. {appraisersData.Length.ToString()}\n");
                foreach (string datum in appraisersData)
                {
                    resultList.Add(datum);
                }
            });

            // Outputters.
            Action<IEnumerable<string>> outputter1 = appraisersData =>
            {
                Console.WriteLine($"\nOutputter 1 perfrom action on all appraisers data.");
                foreach (string datum in appraisersData)
                {
                    Console.WriteLine("Outputter 1: " + datum);
                }
                Console.WriteLine("Outputter 1 finished.\n");
            };
            Action<IEnumerable<string>> outputter2 = appraisersData =>
            {
                Console.WriteLine($"\nOutputter 2 perfrom action on all appraisers data.");
                foreach (string datum in appraisersData)
                {
                    Console.WriteLine("Outputter 2: " + datum);
                }
                Console.WriteLine("Outputter 2 finished.\n");
            };
            Action<IEnumerable<string>> outputter3 = appraisersData =>
            {
                Console.WriteLine($"\nOutputter 3 perfrom action on all appraisers data.");
                foreach (string datum in appraisersData)
                {
                    Console.WriteLine("Outputter 3: " + datum);
                }
                Console.WriteLine("Outputter 3 finished.\n");
            };

            // Constructing pipeline.
            inputPoint.LinkTo(inputter1, new DataflowLinkOptions { PropagateCompletion = true });
            inputPoint.LinkTo(inputter2, new DataflowLinkOptions { PropagateCompletion = true });
            inputPoint.LinkTo(inputter3, new DataflowLinkOptions { PropagateCompletion = true });

            inputter1.LinkTo(filtering, new DataflowLinkOptions { PropagateCompletion = false });
            inputter2.LinkTo(filtering, new DataflowLinkOptions { PropagateCompletion = false });
            inputter3.LinkTo(filtering, new DataflowLinkOptions { PropagateCompletion = false });

            filtering.LinkTo(broadcastInput, new DataflowLinkOptions { PropagateCompletion = true });

            broadcastInput.LinkTo(crawler1, new DataflowLinkOptions { PropagateCompletion = true });
            broadcastInput.LinkTo(crawler2, new DataflowLinkOptions { PropagateCompletion = true });
            broadcastInput.LinkTo(crawler3, new DataflowLinkOptions { PropagateCompletion = true });

            crawler1.LinkTo(broadcastCrawler1, new DataflowLinkOptions { PropagateCompletion = true });
            crawler2.LinkTo(broadcastCrawler2, new DataflowLinkOptions { PropagateCompletion = true });
            crawler3.LinkTo(broadcastCrawler3, new DataflowLinkOptions { PropagateCompletion = true });

            broadcastCrawler1.LinkTo(appraiser11, new DataflowLinkOptions { PropagateCompletion = true });
            broadcastCrawler1.LinkTo(appraiser12, new DataflowLinkOptions { PropagateCompletion = true });

            broadcastCrawler2.LinkTo(appraiser21, new DataflowLinkOptions { PropagateCompletion = true });
            broadcastCrawler2.LinkTo(appraiser22, new DataflowLinkOptions { PropagateCompletion = true });

            broadcastCrawler3.LinkTo(appraiser31, new DataflowLinkOptions { PropagateCompletion = true });
            broadcastCrawler3.LinkTo(appraiser32, new DataflowLinkOptions { PropagateCompletion = true });

            appraiser11.LinkTo(consumeAllData, new DataflowLinkOptions { PropagateCompletion = false });
            appraiser12.LinkTo(consumeAllData, new DataflowLinkOptions { PropagateCompletion = false });
            appraiser21.LinkTo(consumeAllData, new DataflowLinkOptions { PropagateCompletion = false });
            appraiser22.LinkTo(consumeAllData, new DataflowLinkOptions { PropagateCompletion = false });
            appraiser31.LinkTo(consumeAllData, new DataflowLinkOptions { PropagateCompletion = false });
            appraiser32.LinkTo(consumeAllData, new DataflowLinkOptions { PropagateCompletion = false });

            const string inputData = "TPL Dataflow example is really_hard_to_set_up=really=hard=to=use";
            Console.WriteLine($"Pipeline executed with: {inputData}");

            // Start processing data.
            await inputPoint.SendAsync(inputData);

            // Mark the head of the pipeline as complete.
            inputPoint.Complete();

            await Task.WhenAll(inputter1.Completion, inputter2.Completion, inputter3.Completion);
            filtering.Complete();

            await Task.WhenAll(
                appraiser11.Completion, appraiser12.Completion,
                appraiser21.Completion, appraiser22.Completion,
                appraiser31.Completion, appraiser32.Completion
            );
            consumeAllData.Complete();

            // Wait for the last block in the pipeline to process all messages.
            await consumeAllData.Completion;

            outputter1(resultList);
            //outputter2(resultList);
            //outputter3(resultList);

            Console.WriteLine($"Pipeline finished. {resultList.Count.ToString()}");
        }
    }
}
