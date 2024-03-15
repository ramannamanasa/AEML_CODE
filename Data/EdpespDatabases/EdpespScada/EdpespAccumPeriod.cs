using OSII.ConversionToolkit;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespScada
{
    class EdpespAccumPeriod
    {
        private readonly SCADA _scadaDb;  // Local reference of the Scada Database

        /// <summary>
        /// Default constructor.
        /// Sets local references.
        /// </summary>
        /// <param name="scadaDB">Current SCADA database object.</param>
        public EdpespAccumPeriod(SCADA scadaDb)
        {
            this._scadaDb = scadaDb;
        }

        /// <summary>
        /// Function to convert all ACCUM_PERIOD objects.
        /// </summary>
        public void ConvertAccumPeriod()
        {
            Logger.OpenXMLLog();

            DbObject accumPeriodObj = this._scadaDb.GetDbObject("ACCUM_PERIOD");
            int accumPeriodRec = 0;

            // Constants from mapping
            const int MINUTE_VALUE = 10;
            const int HOUR_VALUE = 1;

            const int MINUTE_UNIT = 2;
            const int HOUR_UNIT = 3;

            accumPeriodObj.CurrentRecordNo = ++accumPeriodRec;
            accumPeriodObj.SetValue("Period", MINUTE_VALUE);
            accumPeriodObj.SetValue("PeriodUnit", MINUTE_UNIT);
            accumPeriodObj.SetValue("Name", "ACCUMULATOR PERIOD 1");

            accumPeriodObj.CurrentRecordNo = ++accumPeriodRec;
            accumPeriodObj.SetValue("Period", HOUR_VALUE);
            accumPeriodObj.SetValue("PeriodUnit", HOUR_UNIT);
            accumPeriodObj.SetValue("Name", "ACCUMULATOR PERIOD 2");

            Logger.CloseXMLLog();
        }
    }
}
