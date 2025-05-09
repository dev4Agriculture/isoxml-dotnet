using System;
using System.Collections.Generic;
using Dev4Agriculture.ISO11783.ISOXML.DDI.DDIRegistry;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;

namespace Dev4Agriculture.ISO11783.ISOXML.DDI
{

    public class ManufacturerSettings
    {
        public Dictionary<ushort, DDIRegisterEntry> DDIs = new Dictionary<ushort, DDIRegisterEntry>();
    }

    public static class DDIRegister
    {
        public static Dictionary<ushort, ManufacturerSettings> ManufacturersList = new Dictionary<ushort, ManufacturerSettings>();

        /// <summary>
        /// Add a new, manufacturer specific proprietary DDI. Each RegisterEntry describes how such DDI is handled during processing; e.g. when generating Totals
        /// </summary>
        /// <param name="ddi"></param>
        /// <param name="manufacturer"></param>
        /// <param name="registerEntry"></param>
        public static void RegisterProprietaryDDI(
            ushort ddi,
            ushort manufacturer,
            DDIRegisterEntry registerEntry)
        {
            if (!ManufacturersList.TryGetValue(manufacturer, out var manufacturerEntry))
            {
                manufacturerEntry = new ManufacturerSettings();
                ManufacturersList.Add(manufacturer, manufacturerEntry);
            }
            manufacturerEntry.DDIs.Add(ddi, registerEntry);
        }


        public static bool TryGetManufacturerSpecificDDI(ushort ddi, ISODevice device, out DDIRegisterEntry entry)
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


        public static void Clear()
        {
            ManufacturersList.Clear();
        }

    }



}

