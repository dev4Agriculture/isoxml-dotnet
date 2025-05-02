using System;
using System.Collections.Generic;
using System.Text;
using de.dev4Agriculture.ISOXML.DDI;

namespace Dev4Agriculture.ISO11783.ISOXML.DDI
{

    public class DDIAlgorithms
    {

        public static readonly ushort[] CustomLifetimeTotalsDdis = new ushort[]
            {
            (ushort)DDIList.LifetimeAverageFuelConsumptionPerTime,
            (ushort)DDIList.LifetimeAverageFuelConsumptionPerArea,
            (ushort)DDIList.LifetimeAverageDieselExhaustFluidConsumptionPerTime,
            (ushort)DDIList.LifetimeAverageDieselExhaustFluidConsumptionPerArea,
            };

        public static readonly Dictionary<ushort, ushort[]> AveragesDDIWeightedDdiMap = new Dictionary<ushort, ushort[]>()
        {
            [(ushort)DDIList.AverageYieldMassPerTime] = new ushort[] { 90 },
            [(ushort)DDIList.AverageCropMoisture] = new ushort[] { 90 },
            [(ushort)DDIList.AverageYieldMassPerArea] = new ushort[] { 90 },
            [(ushort)DDIList.AveragePercentageCropDryMatter] = new ushort[] { 90 },
            [(ushort)DDIList.AverageDryYieldMassPerTime] = new ushort[] { 119 },
            [(ushort)DDIList.AverageDryYieldMassPerArea] = new ushort[] { 116 },
            [(ushort)DDIList.AverageProteinContent] = new ushort[] { 90 },
            [(ushort)DDIList.AverageAppliedPreservativePerYieldMass] = new ushort[] { 90 },


            [(ushort)DDIList.AverageCropContamination] = new ushort[] { 82, 81, 80 },
            [(ushort)DDIList.AverageSeedSingulationPercentage] = new ushort[] { 82, 81, 80 },
            [(ushort)DDIList.AverageSeedSkipPercentage] = new ushort[] { 82, 81, 80 },
            [(ushort)DDIList.AverageSeedMultiplePercentage] = new ushort[] { 82, 81, 80 },
            [(ushort)DDIList.AverageSeedSpacingDeviation] = new ushort[] { 82, 81, 80 },
            [(ushort)DDIList.AverageCoefficientOfVariationOfSeedSpacingPercentage] = new ushort[] { 82, 81, 80 }
        };
    }
}
