using OSII.ConversionToolkit;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespOpenCalc
{
    class EdpespExecutionGroup
    {
        private readonly OpenCalc _openCalcDb;  // Local reference to OpenCalc database object

        /// <summary>
        /// Default constructor. 
        /// Sets local references of important variables.
        /// </summary>
        /// <param name="openCalcDb">Current OpenCalc db object.</param>
        public EdpespExecutionGroup(OpenCalc openCalcDb)
        {
            this._openCalcDb = openCalcDb;
        }

        /// <summary>
        /// Function to convert all EXECUTION_GROUP objects.
        /// </summary>
        public void ConvertExecutionGroup()
        {
            Logger.OpenXMLLog();

            DbObject exeObj = this._openCalcDb.GetDbObject("EXECUTION_GROUP");
            int exeRec = 0;

            // Value from mapping
            const int TEN_SECONDS = 1;
            const int ONE_MINUTE = 2;
            const int TEN_MINUTES = 3;
            const int ONE_HOUR = 4;

            // Set EXECUTION_GROUP objects according to mapping starting with 10secs
            exeObj.CurrentRecordNo = ++exeRec;
            exeObj.SetValue("Name", "10secs");
            exeObj.SetValue("pTimerRecord", TEN_SECONDS);
            // 1min
            exeObj.CurrentRecordNo = ++exeRec;
            exeObj.SetValue("Name", "1min");
            exeObj.SetValue("pTimerRecord", ONE_MINUTE);
            // 10min
            exeObj.CurrentRecordNo = ++exeRec;
            exeObj.SetValue("Name", "10min");
            exeObj.SetValue("pTimerRecord", TEN_MINUTES);
            // 1h
            exeObj.CurrentRecordNo = ++exeRec;
            exeObj.SetValue("Name", "1h");
            exeObj.SetValue("pTimerRecord", ONE_HOUR);

            Logger.CloseXMLLog();
        }
    }
}
