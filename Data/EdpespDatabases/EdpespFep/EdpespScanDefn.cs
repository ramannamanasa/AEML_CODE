using OSII.ConversionToolkit;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespFep
{
    class EdpespScanDefn //Added BD
    {
        private readonly FEP _fepDb;  // Local reference of the Fep Database

        /// <summary>
        /// Default constructor. Set important local references.
        /// </summary>
        /// <param name="fepDb">Current FEP database object.</param>
        public EdpespScanDefn(FEP fepDb)
        {
            this._fepDb = fepDb;
        }

        /// <summary>
        /// Function to convert all INIT_SCAN_DEFN objects.
        /// </summary>
        public void ConvertScanDefn()
        {
            Logger.OpenXMLLog();

            DbObject rtuObj = this._fepDb.GetDbObject("RTU_DATA");
            DbObject ScanDefnObj = this._fepDb.GetDbObject("SCAN_DEFN");
            int ScanDefnRec;
            int rtuRec = rtuObj.RecordCount;
            const int SCAN_MODE = 1;
            const int SCAN_TYPE = 29;
            const int SCAN_STATE = 1;

            for (ScanDefnRec = 1; ScanDefnRec <= rtuRec; ScanDefnRec++)
            {
                ScanDefnObj.CurrentRecordNo = ScanDefnRec;
                //if (ScanDefnObj.CurrentRecordNo > 10) return; //BD: FEPCHECK
                ScanDefnObj.SetValue("Mode", 0, SCAN_MODE);
                ScanDefnObj.SetValue("State", 0, SCAN_STATE);
                ScanDefnObj.SetValue("ScanType", 0, SCAN_TYPE);
                ScanDefnObj.SetValue("Mode", 1, SCAN_MODE);
                ScanDefnObj.SetValue("State", 1, SCAN_STATE);
                ScanDefnObj.SetValue("ScanType", 1, 30);

            }

            Logger.CloseXMLLog();
        }
    }
}
