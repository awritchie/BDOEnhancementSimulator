// See https://aka.ms/new-console-template for more information

using BDOEnhancementSimulator;

const uint SimulationSize = 1000000;
const double MaxInitialRate = 0.9;
const double MinBaseRate = 0.01;
var simulator = new Simulator(SimulationSize);

//var results = await simulator.Start([new SimulationParameters("X Sov", 360, StackExtensions.SovereignEnhancements.Single(x => x.Level == 10), new AdditionalDetails(345, 15, 0))], true);
//var report = results.BuildReport();
//Console.WriteLine(report);

IEnumerable<IEnumerable<EnhancementDetails>> sets = [StackExtensions.SovereignEnhancements, StackExtensions.KharazadEnhancements];
foreach (var set in sets)
{
    foreach (var level in set)
    {
        var outPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var name = $"{level.Name}_{level.Level}";
        var outputFile = Path.Combine(outPath, "BDO_Simulations", $"{name}.csv");

        var freeStack = 15u;
        var parameters = new List<SimulationParameters>();

        var previousStackSize = 0u;
        for (uint stackSize = 0; stackSize < 100; stackSize += 10)
        {
            var additionalDetails = new AdditionalDetails(stackSize, freeStack, 0);
            var finalStack = additionalDetails.GetFinalStack();            
            if (finalStack == previousStackSize)
            {
                continue;
            } else
            {
                previousStackSize = finalStack;
            }
            var initialRate = level.SuccessRate(finalStack);
            if (initialRate >= MaxInitialRate || level.BasePercent < 100 * MinBaseRate)
            {
                continue;
            }
            var parameter = new SimulationParameters($"{name} - {finalStack}", finalStack, level, additionalDetails);
            parameters.Add(parameter);
        }

        var startStack = 100u;
        for (uint origins = 0; origins < 44u; origins++)
        {
            var additionalDetails = new AdditionalDetails(startStack, freeStack, origins);
            var finalStack = additionalDetails.GetFinalStack();
            var initialRate = level.SuccessRate(finalStack);
            if (initialRate >= MaxInitialRate)
            {
                continue;
            }
            var parameter = new SimulationParameters($"{name} - {finalStack}", finalStack, level, additionalDetails);
            parameters.Add(parameter);
        }

        var results = await simulator.Start(parameters, true);
        var report = results.BuildReport();
        Console.WriteLine(report);
        await File.WriteAllLinesAsync(outputFile, [report], CancellationToken.None);
    }
}

