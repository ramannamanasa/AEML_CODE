using System.Collections.Generic;
using OSII.ConversionToolkit;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespOpenCalc
{
    class EdpespTimer
    {
        private readonly OpenCalc _openCalcDb;  // Local reference of the OpenCalc database object.
        private readonly Dictionary<string, int> _timerDict;  // Local reference of the timer dictionary.

        /// <summary>
        /// Enum for period units.
        /// </summary>
        public enum TimerPeriodUnit
        {
            NONE = 0,
            SECS = 1,
            MIN = 2,
            HOUR = 3,
            DAY = 4
        }

        /// <summary>
        /// Default constructor.
        /// Sets important local references.
        /// </summary>
        /// <param name="openCalcDb">Current OpenCalc database.</param>
        /// <param name="timerDict">Current Timer dictionary.</param>
        public EdpespTimer(OpenCalc openCalcDb, Dictionary<string, int> timerDict)
        {
            this._openCalcDb = openCalcDb;
            this._timerDict = timerDict;
        }

        /// <summary>
        /// Function to convert all TIMER objects.
        /// </summary>
        public void ConvertTimer()
        {
            Logger.OpenXMLLog();

            DbObject timerObj = this._openCalcDb.GetDbObject("TIMER");
            int timerRec = 0;

            // Values from mapping
            const int PERIOD_10 = 10;
            const int PERIOD_1 = 1;

            // Create TIMER objects based on values from mapping
            timerObj.CurrentRecordNo = ++timerRec;
            timerObj.SetValue("Name", "10secs");
            timerObj.SetValue("Period", PERIOD_10);
            timerObj.SetValue("PeriodUnit", (int)TimerPeriodUnit.SECS);
            this._timerDict.Add("10secs", timerRec);

            timerObj.CurrentRecordNo = ++timerRec;
            timerObj.SetValue("Name", "1min");
            timerObj.SetValue("Period", PERIOD_1);
            timerObj.SetValue("PeriodUnit", (int)TimerPeriodUnit.MIN);
            this._timerDict.Add("1min", timerRec);

            timerObj.CurrentRecordNo = ++timerRec;
            timerObj.SetValue("Name", "10min");
            timerObj.SetValue("Period", PERIOD_10);
            timerObj.SetValue("PeriodUnit", (int)TimerPeriodUnit.MIN);
            this._timerDict.Add("10min", timerRec);

            timerObj.CurrentRecordNo = ++timerRec;
            timerObj.SetValue("Name", "1h");
            timerObj.SetValue("Period", PERIOD_1);
            timerObj.SetValue("PeriodUnit", (int)TimerPeriodUnit.HOUR);
            this._timerDict.Add("1h", timerRec);

            Logger.CloseXMLLog();
        }
    }
}
