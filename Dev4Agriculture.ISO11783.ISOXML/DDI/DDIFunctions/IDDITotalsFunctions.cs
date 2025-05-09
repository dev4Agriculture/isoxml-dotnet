using System;
using System.Collections.Generic;
using System.Text;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.TimeLog;
using static Dev4Agriculture.ISO11783.ISOXML.DDI.DDIAlgorithms;

namespace Dev4Agriculture.ISO11783.ISOXML.DDI.DDIFunctions
{
    public class LatestTLGEntry
    {
        public ushort DDI = 0;
        public int DeviceElement = 0;
        public DateTime TimeStamp;
        public long? Value;
    }


    public interface IDDITotalsFunctions
    {
        //=================GENERAL=======================

        /// <summary>
        /// This is used to define if we're working with an Average, Sum or Lifetime.
        /// It's relevant as we sort the Functions to be applied by their impact to other functions.
        /// E.g.
        ///  - Lifetime does not impact it's own value: 0
        ///  - A Total just is impacted only by its previous values: 1
        ///  - An Average Total is impacted by its previous values and a Weight Value: 2
        /// </summary>
        /// <returns></returns>
        TotalDDIAlgorithmEnum GetTotalType();


        //=====================TIMELOGS================================

        /// <summary>
        /// This function is called within the Enqueueing of multiple TimeLog Objects. It is called before iterating over the Single TimeLogRows.
        /// It is either called directly after its creation or before a TimeLog is iterated over.
        /// </summary>
        /// <param name="ddis"></param>
        void UpdateTimeLogEnqueuerWithHeaderLine(List<TLGDataLogDDI> ddis, List<ISODevice> devices);

        /// <summary>
        /// This function is called before a Line in a TimeLog is iterated. It provides the NON-adjusted data and can be used e.g. to update the latest value for a Relevant Weight DDI or such.
        /// </summary>
        /// <param name="line"></param>
        void UpdateTimeLogEnqueuerWithDataLine(TLGDataLogLine line);


        /// <summary>
        /// This function is called when a corresponding value is found in the TimeLog. The InputValue is the current value read from the Timelog; the return is the expected updated Value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        int EnqueueUpdatedValueInTimeLog(int value);


        //========================ISOTime and ISODataLogValue=====================

        /// <summary>
        /// This is the initialization to prepare for Singulating a DDI Value in all Rows of a TimeLog
        /// It is called for each TimeLogFile before iterating over the Rows
        /// </summary>
        /// <param name="ddis"></param>
        /// <param name="devices"></param>
        void StartSingulateValueInTimeLog(List<TLGDataLogDDI> ddis, List<ISODevice> devices);

        /// <summary>
        /// This is the actual function called whenever a DDI value corresponding to this function is found in a row of a TimeLog
        /// It outputs the new value and can update internal values; e.g. a Weight Counter
        /// </summary>
        /// <param name="currentValue"></param>
        /// <param name="currentTime"></param>
        /// <param name="latestTLGEntries"></param>
        /// <returns></returns>
        long SingulateValueInTimeLog(long currentValue, DateTime currentTime, List<LatestTLGEntry> latestTLGEntries);

        /// <summary>
        /// This function is called when singluating a Value in a Single ISOTime Element.
        /// As there is no StartFunction before iterating over multiple TIM-Elements, this function might be called right after the first value was found
        /// IMPORTANT: The SingulationLoop iterates over the TIM Elements in reverse order, so, Descending by StartTime.
        /// </summary>
        /// <param name="currentValue"></param>
        /// <param name="previousValue"></param>
        /// <param name="currentTime"></param>
        /// <param name="previousTime"></param>
        /// <param name="devices"></param>
        /// <returns></returns>
        long SingulateValueInISOTime(long currentValue, long previousValue, ISOTime currentTime, ISOTime previousTime, List<ISODevice> devices);


        /// <summary>
        /// This function is used to update the corresponding DataLogValue in a (currently possibly just being generated) TIM Element.
        /// It returns the Value and prepares itself for call in the next entry.
        /// These functions are called TIM Element by TIM Element; Asceding in StartTime
        /// </summary>
        /// <param name="currentValue"></param>
        /// <param name="currentTimeEntry"></param>
        /// <param name="det"></param>
        /// <param name="devices"></param>
        /// <returns></returns>
        long EnqueueValueAsDataLogValueInTime(long currentValue, ISOTime currentTimeEntry, int det, List<ISODevice> devices);

        /// <summary>
        /// This returns a cleaned (start to end of this timelog) TotalValue for the specific DDI
        /// </summary>
        /// <param name="iSOTLG"></param>
        /// <param name="totalValue"></param>
        /// <returns></returns>
        bool GetCleanedTotalForTimeLog(ISOTLG iSOTLG, out int totalValue);


        /// <summary>
        /// This merges all cleaned TotalValues for TimeLogs for 1 specific DDI
        /// </summary>
        /// <param name="task"></param>
        /// <param name="totalValue"></param>
        /// <returns></returns>
        bool GetCleanedTotalForTask(ISOTask task, out int totalValue);
    }
}
