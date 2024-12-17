using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Text;

namespace Playground;

public static class StackExtensions
{
    public static uint OriginIncreasesStackBy(this uint stack)
    {
        uint increase = stack switch
        {
            < 100 => 0,
            <= 105 => 15,
            <= 109 => 14,
            <= 116 => 13,
            <= 121 => 12,
            <= 129 => 11,
            <= 139 => 10,
            <= 151 => 9,
            <= 164 => 8,
            <= 183 => 7,
            <= 198 => 6,
            <= 218 => 5,
            <= 249 => 4,
            <= 279 => 3,
            <= 298 => 2,
            <= 299 => 1,
            _ => 0
        };

        return increase;
    }

    public static readonly IReadOnlyCollection<EnhancementDetails> KharazadEnhancements =
    [
        new("Kharazad", 1, 1, 16.3, 0, 3),
        new("Kharazad", 2, 2, 7.3, 120, 5),
        new("Kharazad", 3, 3, 4.57, 280, 7),
        new("Kharazad", 4, 4, 2.89, 540, 8),
        new("Kharazad", 5, 6, 1.911, 840, 10),
        new("Kharazad", 6, 8, 1.29, 1090, 12),
        new("Kharazad", 7, 10, 0.88, 1480, 15),
        new("Kharazad", 8, 12, 0.57, 1880, 20),
        new("Kharazad", 9, 15, 0.32, 2850, 25),
        new("Kharazad", 10, 0, 0.172, 3650, 30)
    ];

    public static readonly IReadOnlyCollection<EnhancementDetails> SovereignEnhancements =
    [
        new("Sovereign", 1, 1, 8.5500, 0, 2),
        new("Sovereign", 2, 1, 4.1200, 320, 5),
        new("Sovereign", 3, 1, 2.0000, 560, 10),
        new("Sovereign", 4, 1, 0.9100, 780, 20),
        new("Sovereign", 5, 1, 0.4690, 970, 30),
        new("Sovereign", 6, 1, 0.2730, 1350, 35),
        new("Sovereign", 7, 1, 0.1600, 1550, 50),
        new("Sovereign", 8, 1, 0.1075, 2250, 75),
        new("Sovereign", 9, 1, 0.0485, 2760, 165),
        new("Sovereign", 10, 1, 0.0242, 3920, 330),
    ];
}

public record EnhancementDetails(
    string Name,
    uint Level,
    uint EnhancementMaterials,
    double BasePercent,
    uint Crons,
    uint Agris)
{
    private readonly double _baseRate = BasePercent * 0.01d;
    public double SuccessRate(uint stack) => _baseRate + 0.1d * _baseRate * stack;
}

public record SimulationParameters(
    string Name,
    uint StackSize,
    EnhancementDetails EnhancementDetails,
    AdditionalDetails AdditionalDetails);

public record SimulationResult(SimulationParameters Parameters, ConcurrentDictionary<uint, uint> Counts);

public record AdditionalDetails(uint StartStack, uint FreeStack, uint OriginCount);

public class SimulationSummary(uint size, SimulationResult result)
{
    public SimulationResult Result => result;
    public uint Size => size;
    public double Average { get; } = result.Counts.Aggregate(0d, (avg, setCount) => avg + (setCount.Key * setCount.Value / (double)size));
    public uint Median { get; } = result.Counts.OrderBy(x => x.Key).Aggregate(new KeyValuePair<uint, uint>(), (acc, setCount) => acc.Value > size * 0.5 ? acc : new KeyValuePair<uint, uint>(setCount.Key, acc.Value + setCount.Value)).Key;
}

public static class SimulationSummaryExtensions
{
    public static uint GetFinalStack(this AdditionalDetails additionalDetails)
    {
        var currentStack = additionalDetails.StartStack;
        for (uint origins = 0; origins < additionalDetails.OriginCount; origins++)
        {
            currentStack += currentStack.OriginIncreasesStackBy();
        }
        return currentStack + additionalDetails.FreeStack;
    }
    public static double AverageCronsConsumed(this SimulationSummary result) => result.Average * result.Result.Parameters.EnhancementDetails.Crons;
    public static double AverageEnhancementMaterialCost(this SimulationSummary result) => result.Average * result.Result.Parameters.EnhancementDetails.EnhancementMaterials;
    public static double MedianCronsConsumed(this SimulationSummary result) => result.Median * result.Result.Parameters.EnhancementDetails.Crons;
    public static double MedianEnhancementMaterialCost(this SimulationSummary result) => result.Median * result.Result.Parameters.EnhancementDetails.EnhancementMaterials;
    public static double InitialStackSuccessRate(this SimulationSummary result) => result.Result.Parameters.EnhancementDetails.SuccessRate(result.Result.Parameters.StackSize);
    public static double FullPityRate(this SimulationSummary result) => 1d * result.Result.Counts[result.Result.Parameters.EnhancementDetails.Agris + 1] / result.Size;

    public static string BuildReport(this IEnumerable<SimulationSummary> results)
    {
        var stringBuilder = new StringBuilder();
        var columns = string.Join(", ", [
            "Name",
            "Initial Stack",
            "Rulupee + Valks",
            "Origins Consumed",
            "Final Stack",
            "Initial Success Percent",
            "Full Pity Percent",
            "Average Attempts",
            "Average Crons Consumed",
            "Average Enhancement Material Consumed",
            "Median Attempts",
            "Median Crons Consumed",
            "Median Enhancement Material Consumed"
        ]);
        stringBuilder.AppendLine(columns);
        foreach (var result in results.OrderBy(x => x.Result.Parameters.StackSize))
        {
            var row = string.Join(", ", [
                result.Result.Parameters.Name,
                result.Result.Parameters.AdditionalDetails.StartStack.ToString("N0"),
                result.Result.Parameters.AdditionalDetails.FreeStack.ToString("N0"),
                result.Result.Parameters.AdditionalDetails.OriginCount.ToString("N0"),
                result.Result.Parameters.StackSize.ToString("N0"),
                result.InitialStackSuccessRate().ToString("P4"),
                result.FullPityRate().ToString("P4"),
                (1 / result.InitialStackSuccessRate()).ToString("F2"),
                result.Average.ToString("F6"),
                result.AverageCronsConsumed().ToString("F6"),
                result.AverageEnhancementMaterialCost().ToString("F6"),
                result.Median.ToString("F6"),
                result.MedianCronsConsumed().ToString("F6"),
                result.MedianEnhancementMaterialCost().ToString("F6"),
            ]);

            stringBuilder.AppendLine(string.Join(", ", row));
        }

        return stringBuilder.ToString();
    }
}