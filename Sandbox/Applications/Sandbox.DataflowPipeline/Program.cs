using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Gridsum.DataflowEx;

namespace Sandbox.DataflowPipeline
{
    public static class Program
    {
        private static async Task FirstSimplePipeline()
        {
            var pipeline = PipelineCreator.CreateSimplePipeline(
                result => Console.WriteLine($"Most common word is odd? {result.ToString()}.")
            );

            Console.WriteLine("Pipeline 1 executed.");
            await pipeline.SendAsync("The pipeline pattern is the best pattern");
        }

        private static async Task SecondTcPipeline()
        {
            var tcs = new TaskCompletionSource<bool>();
            var tc = new TC<string, bool>("The pipeline patter is the best patter", tcs);
            Task<bool> task = tcs.Task;

            Console.WriteLine("Pipeline 2 executed.");
            var pipeline = PipelineCreator.CreateTcPipeline();

            await pipeline.SendAsync(tc);
            bool result = await task;

            Console.WriteLine($"Most common word is odd? {result.ToString()}.");
        }

        private static async Task ThirdTcPipeline()
        {
            var pipeline = new PipelineCreator<string, bool>()
                .AddStep<string, string>(sentence => Utils.FindMostCommon(sentence))
                .AddStep<string, int>(word => Utils.CountChars(word))
                .AddStep<int, bool>(length => Utils.IsOdd(length))
                .CreatePipeline();

            Console.WriteLine("Pipeline 3 executed.");
            bool result = await pipeline.Execute("The pipeline pattern is the best pattern");
            Console.WriteLine($"Most common word is odd? {result.ToString()}.");
        }

        private static async Task FourthTplPipeline()
        {
            var pipeline = new TplAsyncPipelineBuilderOriginal<string>()
                .AddStep(str => Utils.FindMostCommon(str))
                .AddStep(word => Utils.CountChars(word))
                .AddStep(length => Utils.IsOdd(length))
                .Build();

            Console.WriteLine("Pipeline 4 executed.");
            var result = await pipeline.Execute("The pipeline pattern is the best pattern");

            Console.WriteLine($"Most common word is odd? {result.ToString()}.");
        }

        private static async Task FifthTplPipeline()
        {
            var pipeline = new TplAsyncPipelineBuilder<string, bool>()
                .AddStep(str => Utils.FindMostCommon(str))
                .AddStep(word => Utils.CountChars(word))
                .AddStep(length => Utils.IsOdd(length))
                .Build();

            Console.WriteLine("Pipeline 5 executed.");
            var result = await pipeline.Execute("The pipeline pattern is the best pattern");

            Console.WriteLine($"Most common word is odd? {result.ToString()}.");
        }

        private static async Task SixthTplPipeline()
        {
            // Inputters data.
            var inputter1 = new Func<string, IReadOnlyList<string>>(input =>
            {
                Console.WriteLine("Inputter 1 transforms inputs.");
                return input.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            });
            var inputter2 = new Func<string, IReadOnlyList<string>>(input =>
            {
                Console.WriteLine("Inputter 2 transforms inputs.");
                return input.Split("_", StringSplitOptions.RemoveEmptyEntries);
            });
            var inputter3 = new Func<string, IReadOnlyList<string>>(input =>
            {
                Console.WriteLine("Inputter 3 transforms inputs.");
                return input.Split("=", StringSplitOptions.RemoveEmptyEntries);
            });

            // Input point.
            // Filtering inputters data.
            // Broadcasting inputters data.
            var inputtersFlow = new InputtersFlow(new[] { inputter1, inputter2, inputter3 });

            // Crawlers data.
            var crawler1 = new Func<IReadOnlyList<string>, IReadOnlyList<IData>>(filteredInputtersData =>
            {
                Console.WriteLine($"Crawler 1 transforms filtered inputters data. {filteredInputtersData.Count.ToString()}");
                return filteredInputtersData
                    .Select(datum => new Data1(datum.Length))
                    .ToArray();
            });
            var crawler2 = new Func<IReadOnlyList<string>, IReadOnlyList<IData>>(filteredInputtersData =>
            {
                Console.WriteLine($"Crawler 2 transforms filtered inputters data. {filteredInputtersData.Count.ToString()}");
                return filteredInputtersData
                    .Select(datum => new Data2(datum))
                    .ToArray();
            });
            var crawler3 = new Func<IReadOnlyList<string>, IReadOnlyList<IData>>(filteredInputtersData =>
            {
                Console.WriteLine($"Crawler 3 transforms filtered inputters data. {filteredInputtersData.Count.ToString()}");
                return filteredInputtersData
                    .Select(datum => new Data3(datum.Length + 42.5))
                    .ToArray();
            });

            // Broadcasting crawlers data.
            var crawlersFlow = new CrawlersFlow(new[] { crawler1, crawler2, crawler3 });

            // Appraisers.
            var appraiser11 = new Func<IReadOnlyList<IData>, IReadOnlyList<string>>(crawlersData =>
            {
                Console.WriteLine($"Appraiser 1-1 transforms crawler's 1 data. {crawlersData.Count.ToString()}");
                return crawlersData
                    .Cast<Data1>()
                    .Select(datum => datum.Value.ToString() + "|11|")
                    .ToArray();
            });
            var appraiser12 = new Func<IReadOnlyList<IData>, IReadOnlyList<string>>(crawlersData =>
            {
                Console.WriteLine($"Appraiser 1-2 transforms crawler's 1 data. {crawlersData.Count.ToString()}");
                return crawlersData
                    .Cast<Data1>()
                    .Select(datum => datum.Value.ToString() + "|12|")
                    .ToArray();
            });

            var appraiser21 = new Func<IReadOnlyList<IData>, IReadOnlyList<string>>(crawlersData =>
            {
                Console.WriteLine($"Appraiser 2-1 transforms filtered crawler's 2 data. {crawlersData.Count.ToString()}");
                return crawlersData
                    .Cast<Data2>()
                    .Select(datum => datum.Value + "|21|")
                    .ToArray();
            });
            var appraiser22 = new Func<IReadOnlyList<IData>, IReadOnlyList<string>>(crawlersData =>
            {
                Console.WriteLine($"Appraiser 2-2 transforms filtered crawler's 2 data. {crawlersData.Count.ToString()}");
                return crawlersData
                    .Cast<Data2>()
                    .Select(datum => datum.Value + "|22|")
                    .ToArray();
            });

            var appraiser31 = new Func<IReadOnlyList<IData>, IReadOnlyList<string>>(crawlersData =>
            {
                Console.WriteLine($"Appraiser 3-1 transforms filtered crawler's 3 data. {crawlersData.Count.ToString()}");
                return crawlersData
                    .Cast<Data3>()
                    .Select(datum => datum.Value.ToString() + "|31|")
                    .ToArray();
            });
            var appraiser32 = new Func<IReadOnlyList<IData>, IReadOnlyList<string>>(crawlersData =>
            {
                Console.WriteLine($"Appraiser 3-2 transforms filtered crawler's 3 data. {crawlersData.Count.ToString()}");
                return crawlersData
                    .Cast<Data3>()
                    .Select(datum => datum.Value.ToString() + "|32|")
                    .ToArray();
            });

            var appraisersFlow = new AppraisersFlow(new[] {
                new Appraiser(appraiser11, typeof(Data1)), new Appraiser(appraiser12, typeof(Data1)),
                new Appraiser(appraiser21, typeof(Data2)), new Appraiser(appraiser22, typeof(Data2)),
                new Appraiser(appraiser31, typeof(Data3)), new Appraiser(appraiser32, typeof(Data3))
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

            // Consuming appraisers data.
            var outputtersFlow = new OutputtersFlow(new[] { outputter1, outputter2, outputter3 });

            // Constructing pipeline.
            inputtersFlow.LinkTo(crawlersFlow);
            crawlersFlow.LinkTo(appraisersFlow);
            appraisersFlow.LinkTo(outputtersFlow);

            const string inputData = "TPL Dataflow example is really_hard_to_set_up=really=hard=to=use";
            Console.WriteLine($"Pipeline executed with: {inputData}");

            // Start processing data.
            await inputtersFlow.ProcessAsync(new[] { inputData });

            // Wait for the last block in the pipeline to process all messages.
            await outputtersFlow.CompletionTask;

            //outputter1(resultList);
            //outputter2(resultList);
            //outputter3(resultList);

            Console.WriteLine($"Pipeline finished. {outputtersFlow.Results.Count().ToString()}");
        }

        private static async Task Main()
        {
            Console.WriteLine("App started.");

            try
            {
                //await FirstSimplePipeline();
                //await SecondTcPipeline();
                //await ThirdTcPipeline();
                //await FourthTplPipeline();
                //await FifthTplPipeline();
                await SixthTplPipeline();
                //await PipelineCreator.CreateComplexPipelineAndExecuteAsync();

                //Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured:{Environment.NewLine}{ex}");
            }
            finally
            {
                Console.WriteLine("App finished.");
            }
        }
    }
}
