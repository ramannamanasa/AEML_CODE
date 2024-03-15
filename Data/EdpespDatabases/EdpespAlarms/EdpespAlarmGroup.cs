using System;
using System.Collections.Generic;
using System.Data;
using OSII.ConversionToolkit;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespAlarms
{
    class EdpespAlarmGroup
    {
        private readonly EdpespParser _parser;  // Local reference of the parser.
        private readonly ALARMS _alarmsDb;  // Local reference of the alarms db.
        private readonly Dictionary<string, int> _alarmsDict;  // Local reference of the alarms dictionary. 

        /// <summary>
        /// Default constructor.
        /// Sets local references of important variables.
        /// </summary>
        /// <param name="par">Current parser object.</param>
        /// <param name="alarmsDb">Current Alarms db object.</param>
        /// <param name="alarmsDict">Alarms dictionary.</param>
        public EdpespAlarmGroup(EdpespParser par, ALARMS alarmsDb, Dictionary<string, int> alarmsDict)
        {
            this._parser = par;
            this._alarmsDb = alarmsDb;
            this._alarmsDict = alarmsDict;
        }

        /// <summary>
        /// Function to convert all Alarm Group objects.
        /// </summary>
        public void ConvertAlarmGroup()
        {
            Logger.OpenXMLLog();

            DbObject alarmGrpObj = this._alarmsDb.GetDbObject("ALARM_GROUP");

            // Since a STATIC db is being used, iterate through all the alarm group objects and match them to an IRN.
            //for (int alarmGrpRec = 1; alarmGrpRec < alarmGrpObj.RecordCount + 1; alarmGrpRec++)
            //{
            //    alarmGrpObj.CurrentRecordNo = alarmGrpRec;
            //    string currentGroupName = alarmGrpObj.GetValue("Name", 0);

            //    // Check for the alarm group name in the status and analog tables, if present: add it to the dictionary with the IRN and Rec.
            //    if (this._parser.StatusAlarmsTbl.TryGetRow(new[] { currentGroupName.Replace("Status", "") }, out DataRow statusRow))
            //    {
            //        if (!this._alarmsDict.ContainsKey(statusRow["IRN"].ToString()))
            //        {
            //            this._alarmsDict.Add(statusRow["IRN"].ToString(), alarmGrpRec);
            //        }
            //    }
            //    else if (this._parser.AnalogAlarmsTbl.TryGetRow(new[] { currentGroupName.Replace("Analog", "") }, out DataRow analogRow))
            //    {
            //        if (!this._alarmsDict.ContainsKey(analogRow["IRN"].ToString()))
            //        {
            //            this._alarmsDict.Add(analogRow["IRN"].ToString(), alarmGrpRec);
            //        }
            //    }
            //    else
            //    {
            //        Logger.Log("UNMATCHED ALARM GROUP", LoggerLevel.INFO, $"Alarm group not found in status or analog table: {currentGroupName} \t Record: {alarmGrpRec}");
            //    }
            //}
            int alarmGrpRec = 0;
            // Merg the Meas_Alarm and Ind_Alarm 

            for (alarmGrpRec = 1; alarmGrpRec <= alarmGrpObj.RecordCount; alarmGrpRec++)
            {
                alarmGrpObj.CurrentRecordNo = alarmGrpRec;
                string alrmgrpName = alarmGrpObj.GetValue("Name", alarmGrpRec, 0);
                if (string.IsNullOrEmpty(alrmgrpName)) continue;
                if (!(this._alarmsDict.ContainsKey(alrmgrpName) ))
                    this._alarmsDict.Add(alrmgrpName, alarmGrpRec);
            }
            //foreach (DataRow AlarmRow in this._parser.StatusAlarmsTbl.Rows)
            //{
            //    alarmGrpObj.CurrentRecordNo = ++alarmGrpRec; ;
            //    string currentGroupName = AlarmRow["ALARM_PROC_GROUP_NAME"].ToString();
            //    alarmGrpObj.SetValue("Name", currentGroupName);
            //    alarmGrpObj.SetValue("Mode", 1);
            //    for(int i = 0; i<= 42; i++)
            //    {
            //        alarmGrpObj.SetValue("pClass", i,i+1);
            //        alarmGrpObj.SetValue("pEvent", i, i + 1);
            //    }
            //    // Check for the alarm group name in the status and analog tables, if present: add it to the dictionary with the IRN and Rec.
            //    //if (this._parser.AnalogAlarmsTbl.TryGetRow(new[] { currentGroupName.Replace("Analog", "") }, out DataRow analogRow))
            //    {
            //        if (!this._alarmsDict.ContainsKey(AlarmRow["IRN"].ToString()))
            //        {
            //            this._alarmsDict.Add(AlarmRow["IRN"].ToString(), alarmGrpRec);
            //        }
            //    }
            //    //else
            //    //{
            //    //    Logger.Log("UNMATCHED ALARM GROUP", LoggerLevel.INFO, $"Alarm group not found in status or analog table: {currentGroupName} \t Record: {alarmGrpRec}");
            //    //}
            //}
            //foreach (DataRow AlarmRow in this._parser.AnalogAlarmsTbl.Rows)
            //{
            //    alarmGrpObj.CurrentRecordNo = ++alarmGrpRec; ;
            //    string currentGroupName = AlarmRow["ALARM_PROC_GROUP_NAME"].ToString();
            //    alarmGrpObj.SetValue("Name", currentGroupName);
            //    alarmGrpObj.SetValue("Mode", 1);
            //    for (int i = 0; i <= 42; i++)
            //    {
            //        alarmGrpObj.SetValue("pClass", i, i + 1);
            //        alarmGrpObj.SetValue("pEvent", i, i + 1);
            //    }
            //    // Check for the alarm group name in the status and analog tables, if present: add it to the dictionary with the IRN and Rec.
                
            //    //if (this._parser.AnalogAlarmsTbl.TryGetRow(new[] { currentGroupName.Replace("Analog", "") }, out DataRow analogRow))
            //    {
            //        if (!this._alarmsDict.ContainsKey(AlarmRow["IRN"].ToString()))
            //        {
            //            this._alarmsDict.Add(AlarmRow["IRN"].ToString(), alarmGrpRec);
            //        }
            //    }
                
            //}

            Logger.CloseXMLLog();
        }
    }
}
