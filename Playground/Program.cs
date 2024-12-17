// See https://aka.ms/new-console-template for more information

using System.Collections.Concurrent;
using System.Text;
using Playground;

const uint SimulationSize = 1000000;

var simulationSets = StackExtensions.SovereignEnhancements.Where(x => x.Level == 7)
                                    .SelectMany(x =>
                                     {
                                         var startStack = 100u;
                                         var freeStack = 15u;
                                         var parameters = new List<SimulationParameters>();
                                         for (uint origins = 0; origins < 44u; origins++)
                                         {
                                             var additionalDetails = new AdditionalDetails(startStack, freeStack, origins);
                                             var stackSize = additionalDetails.GetFinalStack();
                                             var parameter = new SimulationParameters($"{x.Level} {x.Name} - {stackSize}", stackSize, x, additionalDetails);
                                             parameters.Add(parameter);
                                         }

                                         return parameters;
                                     });

var simulator = new SimulatorV2(SimulationSize);
var results = await simulator.Start(simulationSets, true);
Console.WriteLine(results.BuildReport());

//
// public record Parameters(
//     string Name,
//     Func<double, double> InitialSuccessPercentFunc,
//     double OnFailureSuccessPercentDelta,
//     uint AutoSuccessAfterCount,
//     uint CronStonesConsumed,
//     TargetStack TargetStack,
//     bool Disabled = false)
// {
//     private (uint OriginsConsumed, uint Final) Stack { get; } = GetOriginsConsumed(TargetStack);
//     public uint StartingStack => Stack.Final;
//     public uint OriginsConsumed => Stack.OriginsConsumed;
//
//     private static (uint OriginsConsumed, uint Final) GetOriginsConsumed(TargetStack target)
//     {
//         if (target is null or { Start: < 100 })
//         {
//             return (0, target?.Start ?? 0);
//         }
//
//         var current = target.Start;
//         var consumed = 0u;
//         while (current + target.Additional < target.Target)
//         {
//             var increase = current.OriginIncreasesStackBy();
//
//             if (increase == 0)
//             {
//                 break;
//             }
//
//             current += increase;
//             consumed++;
//         }
//
//         return (consumed, current + target.Additional);
//     }
//
//
// }
//
// public record TargetStack(uint Start, uint Target, uint Additional);
//
// public class Simulator(uint size, double cronStoneCost, double originCost)
// {
//     private const double SepSovBase = 0.16;
//     private const double OctSovBase = 0.1075;
//     private const double NovSovBase = 0.0485;
//     private const double DecSovBase = 0.0242;
//
//     // private static readonly Parameters[] SimulationSets =
//     // [
//     //     new Parameters("IX Acc, 272 Stack", 9.024, 0.032, 25, 2850, false, new TargetStack(100, 272, 15)),
//     //     new Parameters("X Acc, 316 Stack", 5.607, 0.017, 30, 3650, false, new TargetStack(100, 315, 15)),
//     //     new Parameters("VII Sov, 240 Stack", SepSovBase + 240 * SepSovBase * .1, SepSovBase * .1, 50, 1550, false, new TargetStack(100, 240, 15)),
//     //     new Parameters("VIII Sov, 270 Stack", OctSovBase + 240 * OctSovBase * .1, OctSovBase * .1, 75, 2250, false, new TargetStack(100, 240, 15)),
//     //     new Parameters("IX Sov, 300 Stack", NovSovBase + 300 * NovSovBase * .1, NovSovBase * .1, 165, 2760, false, new TargetStack(100, 285, 15)),
//     //     new Parameters("IX Sov, 330 Stack", NovSovBase + 315 * NovSovBase * .1, NovSovBase * .1, 165, 2760, false, new TargetStack(100, 300, 15)),
//     //     new Parameters("X Sov, 300 Stack", DecSovBase + 300 * DecSovBase * .1, DecSovBase * .1, 330, 3920, false, new TargetStack(100, 300, 15)),
//     //     new Parameters("X Sov, 350 Stack", DecSovBase + 350 * DecSovBase * .1, DecSovBase * .1, 330, 3920, false, new TargetStack(100, 350, 15)),
//     //     new Parameters("X Sov, 380 Stack", DecSovBase + 380 * DecSovBase * .1, DecSovBase * .1, 330, 3920, false, new TargetStack(100, 380, 15)),
//     //     new Parameters("X Sov, 400 Stack", DecSovBase + 400 * DecSovBase * .1, DecSovBase * .1, 330, 3920, false, new TargetStack(100, 400, 15)),
//     // ];
//
//     public async Task Start()
//     {
//         var summaries = new List<Summary>();
//         // var sets = SimulationSets;
//         var sets = DecAccessoryParameters();
//         var simulations = await Task.WhenAll(sets.Where(x => !x.Disabled).GroupBy(x => x.StartingStack).Select(x => x.OrderBy(y => y.OriginsConsumed).First())
//                               .Select(x => Task.Run(() => Simulate(x))));
//         
//         foreach (var result in simulations)
//         {
//             var summary = new Summary(size, result, cronStoneCost, originCost);
//             summaries.Add(summary);
//         }
//
//         var stringBuilder = new StringBuilder("Name");
//         stringBuilder.Append(", Starting Stack, Rulupee + Valks, Origins Consumed, Final Stack");
//         stringBuilder.Append(", Initial Success Percent, Average Attempts, Median Attempts, Crons Consumed, Crons Consumed Cost");
//         stringBuilder.Append(", Origins Consumed Cost");
//
//         stringBuilder.Append(", Total Cost");
//         foreach (var summary in summaries.OrderBy(x => x.TotalCost))
//         {
//             stringBuilder.AppendLine();
//             stringBuilder.Append($"{summary.Result.Parameters.Name.Split(',').First()}");
//
//             stringBuilder.Append($", {summary.Result.Parameters.TargetStack.Start}");
//             stringBuilder.Append($", {summary.Result.Parameters.TargetStack.Additional}");
//             stringBuilder.Append($", {summary.Result.Parameters.OriginsConsumed:N0}");
//             stringBuilder.Append($", {summary.Result.Parameters.StartingStack}");
//
//             stringBuilder.Append($", {summary.Result.InitialSuccessRate:P4}");
//             stringBuilder.Append($", {summary.Average}");
//             stringBuilder.Append($", {summary.Median}");
//             stringBuilder.Append($", {summary.AverageCronsConsumed:F0}");
//             stringBuilder.Append($", {summary.AverageCronsCost:F0}");
//             stringBuilder.Append($", {summary.OriginConst:F0}");
//
//             stringBuilder.Append($", {summary.TotalCost:F0}");
//         }
//
//         Console.WriteLine(stringBuilder.ToString());
//     }
//
//     private static List<Parameters> DecAccessoryParameters()
//     {
//         var sets = new List<Parameters>();
//         var start = 250u;
//         var stop = 300u;
//         var additional = 15u;
//         var stackIncrease = 0.0172;
//         for (var target = start; target <= stop; target++)
//         {
//             sets.Add(new Parameters($"X Kharazad, {target + additional} Stack",
//                                     (stackSize) => (stackSize) * stackIncrease + 10*stackIncrease,
//                                     stackIncrease,
//                                     30,
//                                     3650,
//                                     new TargetStack(250, target + additional, additional)));
//         }
//
//         return sets;
//     }
//
//     private Result Simulate(Parameters parameters)
//     {
//         Console.WriteLine($"Starting simulation: {parameters.Name} => {parameters.StartingStack}");
//
//         var counts = new ConcurrentDictionary<uint, uint>();
//         var initialSuccessRate = parameters.InitialSuccessPercentFunc(parameters.StartingStack) * 0.01;
//         for (var index = 0; index < size; index++)
//         {
//             var result = SimulateOnce(parameters, initialSuccessRate);
//             counts.AddOrUpdate(result, k => 1, (k, v) => v + 1);
//         }
//         var results = new Result(parameters, counts, initialSuccessRate);
//
//         return results;
//     }
//
//     private static uint SimulateOnce(Parameters parameters, double initialSuccessRate)
//     {
//         var currentCount = 0u;
//         var currentPercent = initialSuccessRate;
//         var onFailureSuccessPercentDelta = parameters.OnFailureSuccessPercentDelta * 0.01;
//         while (currentCount++ < parameters.AutoSuccessAfterCount)
//         {
//             var roll = Random.Shared.NextDouble();
//             if (roll < currentPercent)
//             {
//                 break;
//             }
//
//             currentPercent += onFailureSuccessPercentDelta;
//         }
//
//         return currentCount;
//     }
// }
//
// public record Result(
//     Parameters Parameters,
//     ConcurrentDictionary<uint, uint> Counts,
//     double InitialSuccessRate);
//
// public class Summary(
//     uint size,
//     Result result,
//     double cronStoneCost,
//     double originCost)
// {
//     public Result Result { get; } = result;
//     public double Average { get; } = result.Counts.Aggregate(0d, (avg, setCount) => avg + (setCount.Key * setCount.Value / (double)size));
//     public uint Median { get; } = result.Counts.Aggregate(new KeyValuePair<uint, uint>(), (acc, setCount) => acc.Value > size * 0.5 ? acc : new KeyValuePair<uint, uint>(setCount.Key, acc.Value + setCount.Value)).Key;
//     public double AverageCronsConsumed => Math.Ceiling(Average) * Result.Parameters.CronStonesConsumed;
//     public double AverageCronsCost => AverageCronsConsumed * cronStoneCost;
//     public double OriginConst { get; } = result.Parameters.OriginsConsumed * originCost;
//     public double TotalCost => AverageCronsCost + OriginConst;
// }