using OSII.ConversionToolkit;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespFep
{
    class EdespDemandScanDefn
    {
        private readonly FEP _fepDb;  // Local reference of the Fep Database

        /// <summary>
        /// Default constructor. Set important local references.
        /// </summary>
        /// <param name="fepDb">Current FEP database object.</param>
        public EdespDemandScanDefn(FEP fepDb)
        {
            this._fepDb = fepDb;
        }

        /// <summary>
        /// Function to convert all INIT_SCAN_DEFN objects.
        /// </summary>
        public void ConvertDemandScanDefn()
        {
            Logger.OpenXMLLog();

            DbObject rtuObj = this._fepDb.GetDbObject("RTU_DATA");
            DbObject DemandScanDefnObj = this._fepDb.GetDbObject("DEMAND_SCAN_DEFN");
            int DemandScanDefnRec;
            int rtuRec = rtuObj.RecordCount;
            const int SCAN_MODE = 1;
            const int SCAN_TYPE = 29;
            const int SCAN_STATE = 1;

            for (DemandScanDefnRec = 1; DemandScanDefnRec <= rtuRec; DemandScanDefnRec++)
            {
                DemandScanDefnObj.CurrentRecordNo = DemandScanDefnRec;
               // if (DemandScanDefnObj.CurrentRecordNo > 10) return; //BD: FEPCHECK
                DemandScanDefnObj.SetValue("Mode", SCAN_MODE);
                DemandScanDefnObj.SetValue("State", SCAN_STATE);
                DemandScanDefnObj.SetValue("ScanType", SCAN_TYPE);
            }

            Logger.CloseXMLLog();
        }
    }
}
