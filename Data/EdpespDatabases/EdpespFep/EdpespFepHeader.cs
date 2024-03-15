using OSII.ConversionToolkit;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespFep
{
    class EdpespFepHeader
    {
        private readonly FEP _fepDb;  // Local reference of the Fep Database

        /// <summary>
        /// Default constructor. Sets important local references.
        /// </summary>
        /// <param name="fepDb"></param>
        public EdpespFepHeader(FEP fepDb)
        {
            this._fepDb = fepDb;
        }

        /// <summary>
        /// Function to convert the Fep Header object.
        /// </summary>
        public void ConvertFepHeader()
        {
            Logger.OpenXMLLog();

            DbObject fepHeaderObj = this._fepDb.GetDbObject("FEP_HEADER");
            int fepHeaderRec = 0;
            const int pAORGROUP = 1;
            const int ALARMSTN = 1;

            // Set Record, pAORGroup, and AlarmtStn
            fepHeaderObj.CurrentRecordNo = ++fepHeaderRec;
            fepHeaderObj.SetValue("pAORGroup", pAORGROUP);
            fepHeaderObj.SetValue("AlarmStn", ALARMSTN);

            Logger.CloseXMLLog();
        }
    }
}
