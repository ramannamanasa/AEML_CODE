using OSII.ConversionToolkit;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespIccp
{
    class EdpespIccpScanClass
    {
        private readonly ICCP _iccpDb;  // Locacl reference to the ICCP db.

        /// <summary>
        /// Default Constructor. Sets local references of important objects.
        /// </summary>
        /// <param name="iccpDb">Current Iccp Db object.</param>
        public EdpespIccpScanClass(ICCP iccpDb)
        {
            this._iccpDb = iccpDb;
        }

        /// <summary>
        /// Function to convert ICCP_SCAN_CLASS objects.
        /// </summary>
        public void ConvertIccpScanClass()
        {
            Logger.OpenXMLLog();

            DbObject scanClassObj = this._iccpDb.GetDbObject("ICCP_SCAN_CLASS");
            int scanClassRec = 0;

            // Constant values from mapping
            const int POLL_RATE = 2000;
            const int POLL_OFFSET = 0;

            // Set Record and Name
            scanClassObj.CurrentRecordNo = ++scanClassRec;
            scanClassObj.SetValue("Name", "2 Seconds");

            // Set PollRate and PollOffset
            scanClassObj.SetValue("PollRate", POLL_RATE);
            scanClassObj.SetValue("PollOffset", POLL_OFFSET);

            Logger.CloseXMLLog();
        }
    }
}
