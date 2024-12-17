// See https://aka.ms/new-console-template for more information

using BDOEnhancementSimulator;

const uint SimulationSize = 100000;

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
