using OSII.ConversionToolkit;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespFep
{
    class EdpespInitScanDefn
    {
        private readonly FEP _fepDb;  // Local reference of the Fep Database

        /// <summary>
        /// Default constructor. Set important local references.
        /// </summary>
        /// <param name="fepDb">Current FEP database object.</param>
        public EdpespInitScanDefn(FEP fepDb)
        {
            this._fepDb = fepDb;
        }

        /// <summary>
        /// Function to convert all INIT_SCAN_DEFN objects.
        /// </summary>
        public void ConvertInitScanDefn()
        {
            Logger.OpenXMLLog();

            DbObject rtuObj = this._fepDb.GetDbObject("RTU_DATA");
            DbObject initScanDefnObj = this._fepDb.GetDbObject("INIT_SCAN_DEFN");
            int initScanDefnRec;
            int rtuRec = rtuObj.RecordCount;
            const int SCAN_MODE = 1;
            const int SCAN_TYPE = 29;
            const int SCAN_STATE = 1;

            for (initScanDefnRec = 1; initScanDefnRec <= rtuRec; initScanDefnRec++)
            {
                initScanDefnObj.CurrentRecordNo = initScanDefnRec;
                //if (initScanDefnObj.CurrentRecordNo > 10) return; //BD: FEPCHECK             

                initScanDefnObj.SetValue("Mode", 0, SCAN_MODE);
                initScanDefnObj.SetValue("State", 0, SCAN_STATE);
                initScanDefnObj.SetValue("ScanType", 0, 30);
                initScanDefnObj.SetValue("Mode", 1, SCAN_MODE);
                initScanDefnObj.SetValue("State", 1, SCAN_STATE);
                initScanDefnObj.SetValue("ScanType", 1, SCAN_TYPE);
            }

            Logger.CloseXMLLog();
        }
    }
}
