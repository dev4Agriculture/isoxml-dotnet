using Dev4Agriculture.ISO11783.ISOXML.TimeLog;

namespace Dev4Agriculture.ISO11783.ISOXML.TaskFile
{
    public partial class ISOTimeLog
    {
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
