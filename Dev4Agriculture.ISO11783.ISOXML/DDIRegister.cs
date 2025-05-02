using System;
using System.Collections.Generic;
using System.Text;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.TimeLog;

namespace Dev4Agriculture.ISO11783.ISOXML
{
    public class SingleTimelogDDICalculationCallbackParameters
    {
        public ISOTLG Tlg;
        public ISODevice Device;
    }

    public class GroupedTimelogDDICalculationCallbackParameters
    {
        public List<ISODataLogValue> LastDataLogValues = new List<ISODataLogValue>();
        public List<ISODataLogValue> CurrentDataLogValues = new List<ISODataLogValue>();
        public ushort DDI;
        public ushort DET;
    }

    public class DDIRegisterEntry
    {
        public ushort DDI;
        public ushort Manufacturer;
        public Func<SingleTimelogDDICalculationCallbackParameters, uint> SingleTimelogCalculationCallback;
        public Func<GroupedTimelogDDICalculationCallbackParameters, uint> GroupTimelogCalculationCallback;
    }


    public class ManufacturerSettings
    {
        public Dictionary<ushort, DDIRegisterEntry> DDIs = new Dictionary<ushort, DDIRegisterEntry>();
    }




    public static class DDIRegister
    {
        public static Dictionary<ushort, ManufacturerSettings> ManufacturersList = new Dictionary<ushort, ManufacturerSettings>();

        public static void RegisterProprietaryDDI(
            ushort ddi,
            ushort manufacturer,
            Func<SingleTimelogDDICalculationCallbackParameters, uint> calculateSingleTimeLogTotal,
            Func<GroupedTimelogDDICalculationCallbackParameters, uint> calculateNextTimeLogTotal
            )
        {
            if (!ManufacturersList.TryGetValue(manufacturer, out var manufacturerEntry))
            {
                manufacturerEntry = new ManufacturerSettings();
                ManufacturersList.Add(manufacturer, manufacturerEntry);
            }
            manufacturerEntry.DDIs.Add(ddi, new DDIRegisterEntry()
            {
                DDI = ddi,
                Manufacturer = manufacturer,
                SingleTimelogCalculationCallback = calculateSingleTimeLogTotal,
                GroupTimelogCalculationCallback = calculateNextTimeLogTotal
            });
        }


        private static bool FindEntry(ushort ddi, ISODevice device, out DDIRegisterEntry entry)
        {
            if (device == null || device.ClientNameParsed == null)
            {
                entry = null;
                return false;
            }
            if (!ManufacturersList.TryGetValue((ushort)device.ClientNameParsed.ManufacturerCode, out var mfSettings))
            {
                entry = null;
                return false;
            }

            if (!mfSettings.DDIs.TryGetValue(ddi, out entry))
            {
                entry = null;
                return false;
            }

            return true;
        }

        public static bool TryGetManufacturerSpecificSingleTotalCallback(ushort ddi, ISODevice device, out Func<SingleTimelogDDICalculationCallbackParameters, uint> callback)
        {
            if (!FindEntry(ddi,device, out var registerEntry))
            {
                callback = null;
                return false;
            }
            callback = registerEntry.SingleTimelogCalculationCallback;
            return true;
        }

        public static bool TryGetManufacturerSpecificGroupedTotalCallback(ushort ddi, ISODevice device, out Func<GroupedTimelogDDICalculationCallbackParameters, uint> callback)
        {
            if (!FindEntry(ddi, device, out var registerEntry))
            {
                callback = null;
                return false;
            }
            callback = registerEntry.GroupTimelogCalculationCallback;
            return true;
        }
    }



}

