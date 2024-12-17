using System.Collections.Concurrent;
using System.Diagnostics;

namespace BDOEnhancementSimulator;

public class Simulator(uint size)
{
    public async Task<IEnumerable<SimulationSummary>> Start(IEnumerable<SimulationParameters> parameters, bool useParallelization = true)
    {
        var summaries = new ConcurrentBag<SimulationSummary>();
        if (useParallelization)
        {
            await Parallel.ForEachAsync(parameters, async (simulation, cancellationToken) =>
            {
                var result = await Task.Run(() => Simulate(simulation), cancellationToken);
                var summary = new SimulationSummary(size, result);
                summaries.Add(summary);
            });
        }
        else
        {
            foreach (var simulation in parameters)
            {
                var result = Simulate(simulation);
                var summary = new SimulationSummary(size, result);
                summaries.Add(summary);
            }
        }

        return summaries;
    }

    private SimulationResult Simulate(SimulationParameters parameters)
    {
        var simulationTitle = $"{parameters.Name} => {parameters.StackSize}";
        var counts = new ConcurrentDictionary<uint, uint>();

        Console.WriteLine($"[{Environment.CurrentManagedThreadId}] Starting simulation: {simulationTitle}");
        var timer = Stopwatch.StartNew();
        for (var index = 0; index < size; index++)
        {
            var result = SimulateOnce(parameters);
            counts.AddOrUpdate(result, k => 1, (k, v) => v + 1);
        }

        timer.Stop();
        Console.WriteLine($"[{Environment.CurrentManagedThreadId}] Completed simulation: {simulationTitle} after {timer.ElapsedMilliseconds} ms");
        return new SimulationResult(parameters, counts);
    }

    private static uint SimulateOnce(SimulationParameters parameters)
    {
        var attempt = 0u;
        var currentStack = parameters.StackSize;
        var successRate = parameters.EnhancementDetails.SuccessRate(currentStack);
        while (attempt++ < parameters.EnhancementDetails.Agris)
        {
            var roll = Random.Shared.NextDouble();
            if (roll < successRate)
            {
                break;
            }

            successRate = parameters.EnhancementDetails.SuccessRate(currentStack + attempt);
        }

        return attempt;
    }
}