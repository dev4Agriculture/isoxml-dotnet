using Dev4Agriculture.ISO11783.ISOXML.TimeLog;

namespace Dev4Agriculture.ISO11783.ISOXML.TaskFile
{
    public partial class ISOTimeLog
    {
        /// <summary>
        /// Create a <TLG>-Element from the corresponding TLG00001.xml/.bin File
        /// </summary>
        /// <param name="tlgFile"></param>
        /// <returns></returns>
        public static ISOTimeLog FromISOTLG(ISOTLG tlgFile)
        {
            var isoTimeLog = new ISOTimeLog()
            {
                Filename = tlgFile.Name,
                TimeLogType = ISOTimeLogType.Binarytimelogfiletype1
            };


            return isoTimeLog;



        }

    }
}
