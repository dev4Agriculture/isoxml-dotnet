using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using de.dev4Agriculture.ISOXML.DDI;
using Dev4Agriculture.ISO11783.ISOXML.DDI.DDIFunctions;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;

namespace Dev4Agriculture.ISO11783.ISOXML.DDI
{

    public class DDIAlgorithms
    {
        public enum TotalDDIAlgorithmEnum
        {
            Sum = 0,
            Average = 1,
            Lifetime = 2,
            Special = 3
        }



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

        internal static IDDITotalsFunctions FindTotalDDIHandler(ushort ddi, int deviceElement, ISODevice device)
        {
            if (DDIRegister.TryGetManufacturerSpecificDDI(ddi, device, out var ddiRegistry))
            {
                return ddiRegistry.GetInstance(deviceElement);
            }
            else if (device.IsLifetimeTotal(ddi))
            {
                return new LifetimeTotalDDIFunctions();
            }
            else if (DDIAlgorithms.AveragesDDIWeightedDdiMap.TryGetValue(ddi, out var dvi))
            {
                return new WeightedAverageDDIFunctions(ddi, deviceElement, dvi.ToList());
            }
            else
            {
                return new SumTotalDDIFunctions();
            }

        }
    }
}
