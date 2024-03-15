using System;
using System.Collections.Generic;
using System.Data;
using OSII.ConversionToolkit;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespScada
{
    class EdpespAnalogConfig
    {
        private readonly EdpespParser _parser;  // Local reference of the parser
        private readonly SCADA _scadaDb;  // Local reference of the Scada Database
        private readonly Dictionary<string, int> _stationDict;  // Local reference of the station Dict. 
        private readonly Dictionary<string, int> _class2Dict;  // Local reference of the Class2 Dict.
        private readonly Dictionary<string, int> _deviceRecDict;  // Local reference to the deviceRecDict

        /// <summary>
        /// Default constructor. Sets the references of important variables.
        /// </summary>
        /// <param name="par">Current parser object.</param>
        /// <param name="scadaDb">Current SCADA db object.</param>
        /// <param name="stationDict">Station dictionary.</param>
        /// <param name="class2Dict">class2 dictionary.</param>
        /// <param name="deviceRecDict">pDeviceInstance dictionary.</param>
        public EdpespAnalogConfig(EdpespParser par, SCADA scadaDb, Dictionary<string, int> stationDict, Dictionary<string, int> class2Dict, Dictionary<string, int> deviceRecDict)
        {
            this._parser = par;
            this._scadaDb = scadaDb;
            this._stationDict = stationDict;
            this._class2Dict = class2Dict;
            this._deviceRecDict = deviceRecDict;
        }

        /// <summary>
        /// Function to convert all ANALOG_CONFIG objects.
        /// </summary>
        public void ConvertAnalogConfig()
        {
            Logger.OpenXMLLog();

            DbObject analogConfigObj = this._scadaDb.GetDbObject("ANALOG_CONFIG");
            int analogConfigRec = 0;

            DbObject analogObj = this._scadaDb.GetDbObject("ANALOG");

            foreach (DataRow analogRow in this._parser.MeasurandTbl.Rows)
            {
                if (analogRow["EXTERNAL_IDENTITY"].ToString() == "MGHWDI33 33037 REF620U3VOLT" || analogRow["EXTERNAL_IDENTITY"].ToString() == "D_VIHALRSN N TOUBRO B1 42042KVAR")
                {
                    int t = 0;
                }
                bool foundinold = false;
                //Skip the points with description as OSI_NA: Given by custonmer 20220328
                if (analogRow["DESCRIPTION"].ToString() == "OSI_NA")
                {
                    continue;
                }

                // Check for old output point and pStation first to know which points to skip.
                if (!string.IsNullOrEmpty(analogRow["MEA_SRC1_IRN"].ToString()) && !string.IsNullOrEmpty(analogRow["MEA_SRC2_IRN"].ToString()))
                {
                    continue;
                }
                //Skip the points which are telemetry and there is no RTU linked : BD
                if (string.IsNullOrEmpty(analogRow["RTU_IRN"].ToString()) && (analogRow["MEASURAND_TYPE_CODE"].ToString() != "C"))
                {
                    continue;
                }
                //skip the points with IOA is empty
                if (string.IsNullOrEmpty(analogRow["PXINT1"].ToString()) && (analogRow["MEASURAND_TYPE_CODE"].ToString() == "A" || analogRow["MEASURAND_TYPE_CODE"].ToString() == "D"))
                {
                    continue;
                }
                int pStation = string.IsNullOrEmpty(analogRow["RTU_IRN"].ToString()) ? 5000 : EdpespScadaExtensions.GetpStation(analogRow["RTU_IRN"].ToString(), this._stationDict);
                //skip the points belonging to trianagle gateway Stations:BD
                //if (new List<string> { "222452251", "1798949251", "1795852251", "2042422251", "2073195251", "2089811251", "1995038251", "2016324251", "2016325251", "1992663251", "2009856251", "2026310251", "1972835251", "2076307251", "2020391251", "2043217251" }.Contains(analogRow["RTU_IRN"].ToString())) continue;
                //if (new List<string> { "138093201", "206805261", "1798949251", "1795852251", "1972835251", "1992663251", "1995038251", "2009856251", "2016324251", "2016325251", "2020391251", "2026310251", "2042422251", "2043217251", "2076307251", "222452251", "2073195251", "2089811251" }.Contains(analogRow["RTU_IRN"].ToString()))
                if (new List<string> { "138093201", "222452251", "1798949251", "2089811251", "1972835251", "1995038251", "2016324251", "2016325251", "1795852251", "2009856251", "2073195251", "1992663251", "2026310251", "206805261", "2020391251", "2043217251", "2042422251", "2076307251" }.Contains(analogRow["RTU_IRN"].ToString()))
                    //int pStation = EdpespScadaExtensions.GetpStation(analogRow["RTU_IRN"].ToString(), this._stationDict);
                    if (pStation.Equals(-1))
                {
                    continue;
                }
                // Check points in display to know which to skip.
                //SA:20111124
                //if (EdpespScadaExtensions.ToSkip(this._parser.StationsToSkip, this._parser.PointsFromDisplay, analogRow["STATION_IRN"].ToString(), analogRow["EXTERNAL_IDENTITY"].ToString()))
                //{
                //    continue;
                //}
                // Check external identity to and skip those containing PROTT
                if (analogRow["EXTERNAL_IDENTITY"].ToString().Contains("PROTT"))
                {
                    continue;
                }
                // Check if IDENTIFICATION_TEXT contains Carripont
                if (analogRow["IDENTIFICATION_TEXT"].ToString().Contains("Carripont"))
                {
                    continue;
                }
                // Check if RTU_IRN is non empty, if so: check to skip.
                if (!string.IsNullOrEmpty(analogRow["RTU_IRN"].ToString()) && EdpespScadaExtensions.SkipRtuPoint(this._parser.RtuTbl, analogRow["RTU_IRN"].ToString()))
                {
                    continue;
                }
                // Check if STATION_IRN equals 84525201 and RTU_TYPE is ICCP, if so skip.
                if (analogRow["STATION_IRN"].ToString().Equals("84525201") && analogRow["RTU_TYPE_ASC"].ToString().Equals("ICCP"))
                {
                    continue;
                }
                // Check if it needs a duplicate points.
                //SA:20111124
                //bool needsDuplicate = EdpespScadaExtensions.CheckForDuplicate(analogRow["EXTERNAL_IDENTITY"].ToString(), this._parser.IddValTbl);

                analogConfigObj.CurrentRecordNo = ++analogConfigRec;
                string voltage = EdpespScadaExtensions.SetpClass2(analogConfigObj, analogRow["IDENTIFICATION_TEXT"].ToString(), this._class2Dict, this._parser.StationAbbrTbl);
                // Change the name of the matching ANALOG object to remove voltage level.
                analogObj.CurrentRecordNo = analogConfigRec;
                string oldName = analogObj.GetValue("Name", 0);
                if (!string.IsNullOrEmpty(voltage)) oldName = oldName.Replace(voltage, "");
                oldName = analogRow["Full_Name"].ToString();// BD: To include long names
                analogObj.SetValue("Name", oldName.Trim());
                if (this._deviceRecDict.TryGetValue(analogRow["IDENTIFICATION_TEXT"].ToString(), out int pDeviceInstance))
                {
                    //SA:20211125
                   // analogConfigObj.SetValue("pDeviceInstance", pDeviceInstance);
                }

                // Make a duplicate config object for the duplicate analog obj.
                //SA:20111124
                //if (needsDuplicate)
                //{
                //    ++analogConfigRec;
                //    if (analogConfigObj.CopyRecord(analogConfigObj.CurrentRecordNo, analogConfigRec))
                //    {
                //        analogObj.CurrentRecordNo = analogConfigRec;
                //        analogObj.SetValue("Name", oldName.Replace("_CECOEL", "_CECORE").Trim());
                //    }
                //    else
                //    {
                //        --analogConfigRec;
                //    }
                //}
            }

            Logger.CloseXMLLog();
        }
    }
}
