using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using OSII.ConversionToolkit;
using OSII.ConversionToolkit.Generic;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespScada
{
    class EdpespSetpoint
    {
        private readonly EdpespParser _parser;  // Local reference of the parser
        private readonly SCADA _scadaDb;  // Local reference of the Scada Database
        private readonly Dictionary<string, int> _stationDict;  // Local reference of the station Dict.
        private readonly Dictionary<string, int> _aorDict;  // Local reference of the Aor Dict.
        private readonly Dictionary<string, int> _unitDict;  // Local reference of the unit dictionary.
        private readonly Dictionary<string, int> _class2Dict;  // Local reference of the Class2 Dict.
        private readonly Dictionary<string, int> _rtuDataDict;  // Local reference to the RTU_DATA Dictionary.
        private readonly Dictionary<string, int> _scaleDict;  // Local reference of the scale dictionary
        private readonly GenericTable _scadaXref;  // Local reference to the Scada Xref
        private Dictionary<string, List<string>> _StnRTUSetPoint = new Dictionary<string, List<string>>();
        private readonly Dictionary<string, string> _StnNameDict;
        string sPath = Path.Combine(@"D:\Source\Repos\AEML\Database Conversion\Input\Database_Conversion\Input_Files\Database_CSV\CSV", "StnRtuSetpoint.txt");
        /// <summary>
        /// Default Constructor.
        /// Assigns local references of important variables.
        /// </summary>
        /// <param name="par">Current Parser.</param>
        /// <param name="scadaDb">Current Scada database.</param>
        /// <param name="stationDict">Station dictionary object.</param>
        /// <param name="unitDict">Unit dictionary object.</param>
        /// <param name="class2Dict">Class2 dictionary object.</param>
        /// <param name="_rtuDataDict">RTU_DATA dictionary object.</param>
        /// <param name="scadaXref">Current Scada XREF object.</param>
        /// <param name="AorDict">Station dictionary object.</param>
        public EdpespSetpoint(EdpespParser par, SCADA scadaDb, Dictionary<string, int> stationDict, Dictionary<string, int> unitDict,
            Dictionary<string, int> class2Dict, Dictionary<string, int> rtuDataDict, Dictionary<string, int> scaleDict, GenericTable scadaXref, Dictionary<string, int> AorDict, Dictionary<string, string> StnNameDict)
        {
            this._parser = par;
            this._scadaDb = scadaDb;
            this._stationDict = stationDict;
            this._unitDict = unitDict;
            this._scadaXref = scadaXref;
            this._class2Dict = class2Dict;
            this._rtuDataDict = rtuDataDict;
            this._scaleDict = scaleDict;
            this._aorDict = AorDict;
            this._StnNameDict = StnNameDict;
        }

        /// <summary>
        /// Function to convert all SETPOINT objects.
        /// </summary>
        public void ConvertSetpoint()
        {
            Logger.OpenXMLLog();

            DbObject setpointObj = this._scadaDb.GetDbObject("SETPOINT");
            int setpointRec = 0;
            // Const values from mapping.
            int AOR_GROUP = 1;
            const int pALARM_GROUP = 1;
            foreach (DataRow statusRow in this._parser.SetpointTbl.Rows)
            {
                string dictkey = statusRow["STATION_IRN"].ToString();

                dictkey = (this._StnNameDict.ContainsKey(dictkey)) ? this._StnNameDict[dictkey] : dictkey;
                //if (aa != null) dictkey = aa["STATION_ID_TEXT"].ToString();
                string dictvalue = statusRow["RTU_IRN"].ToString();

                List<string> Rtulist = new List<string>();
                if (this._StnRTUSetPoint.ContainsKey(dictkey))
                    Rtulist = this._StnRTUSetPoint[dictkey];

                //if(termlist == null ) termlist.Add(dictvalue);
                if (!Rtulist.Contains(dictvalue) && !string.IsNullOrEmpty(dictvalue))
                {
                    Rtulist.Add(dictvalue);
                }
                this._StnRTUSetPoint[dictkey] = Rtulist;

            }
            var NoRtu = this._StnRTUSetPoint.Where(x => x.Value.Count == 0).ToList();
            var MultipleRtus = this._StnRTUSetPoint.Where(y => y.Value.Count > 1).ToList();
            foreach (var Rtus in NoRtu)
            {
                using (StreamWriter sw = (File.Exists(sPath)) ? File.AppendText(sPath) : File.CreateText(sPath))
                {
                    //sw.WriteLine("No RTU Stations: {0}", Rtus.Key);
                }
            }
            foreach (var Rtus in MultipleRtus)
            {
                using (StreamWriter sw = (File.Exists(sPath)) ? File.AppendText(sPath) : File.CreateText(sPath))
                {
                    //sw.WriteLine("Multiple RTU Stations: {0} ", Rtus.Key);
                }
            }

            foreach (DataRow setpointRow in this._parser.SetpointTbl.Rows)
            {
                if (setpointRow["DESCRIPTION"].ToString() == "OSI_NA")
                {
                    Logger.Log("SetpointSkippedPoints", LoggerLevel.INFO, $"Setpoint: Description is OSI_NA. ExID: {setpointRow["EXTERNAL_IDENTITY"].ToString()}\t ");
                    continue;
                }
                if (string.IsNullOrEmpty(setpointRow["RTU_IRN"].ToString()))// && (!(setpointRow["SETPOINT_TYPE_CODE"].ToString() == "M" || setpointRow["SETPOINT_TYPE_CODE"].ToString() == "C")))
                {
                    Logger.Log("SetpointSkippedPoints", LoggerLevel.INFO, $"Setpoint: RTU_IRN is empty. ExID: {setpointRow["EXTERNAL_IDENTITY"].ToString()}\t ");
                    continue;
                }
                //skip the points belonging to trianagle gateway Stations:BD
                //if (new List<string> { "222452251", "1798949251", "1795852251", "2042422251", "2073195251", "2089811251", "1995038251", "2016324251", "2016325251", "1992663251", "2009856251", "2026310251", "1972835251", "2076307251", "2020391251", "2043217251" }.Contains(setpointRow["RTU_IRN"].ToString()))
                //{
                //if (new List<string> { "138093201", "206805261", "1798949251", "1795852251", "1972835251", "1992663251", "1995038251", "2009856251", "2016324251", "2016325251", "2020391251", "2026310251", "2042422251", "2043217251", "2076307251", "222452251", "2073195251", "2089811251" }.Contains(setpointRow["RTU_IRN"].ToString()))
                if (new List<string> { "138093201", "222452251", "1798949251", "2089811251", "1972835251", "1995038251", "2016324251", "2016325251", "1795852251", "2009856251", "2073195251", "1992663251", "2026310251", "206805261", "2020391251", "2043217251", "2042422251", "2076307251" }.Contains(setpointRow["RTU_IRN"].ToString()))
                {
                    Logger.Log("SetpointSkippedPoints", LoggerLevel.INFO, $"Setpoint: Belongs to Trinagle gateway RTU. ExID: {setpointRow["EXTERNAL_IDENTITY"].ToString()}\t ");
                    continue;
                }
                // Check for pStation first to know which points to skip.
                //Skip the points which are telemetry and there is no RTU linked
                //int pStation = EdpespScadaExtensions.GetpStation(setpointRow["STATION_IRN"].ToString(), this._stationDict);
                int pStation = string.IsNullOrEmpty(setpointRow["RTU_IRN"].ToString()) ? 5000 : EdpespScadaExtensions.GetpStation(setpointRow["RTU_IRN"].ToString(), this._stationDict);
                if (pStation.Equals(-1))
                {
                    Logger.Log("SetpointSkippedPoints", LoggerLevel.INFO, $"Setpoint: pStation is empty. ExID: {setpointRow["EXTERNAL_IDENTITY"].ToString()}\t ");
                    continue; 
                }
                AOR_GROUP = EdpespScadaExtensions.GetpAorGroup2(setpointRow["SUBSYSTEM_IRN"].ToString(), this._aorDict);
                

                // Check points in display to know which to skip.
                //SA:20211124
                //if (EdpespScadaExtensions.ToSkip(this._parser.StationsToSkip, this._parser.PointsFromDisplay, setpointRow["STATION_IRN"].ToString(), setpointRow["EXTERNAL_IDENTITY"].ToString()))
                //{
                //    continue;
                //}

                // Set Record and Name. Skip if name appears in tbl of setpoints to skip.
                //SA:20211125 setpointstoskip is a custom meathod so skipping it for now
                //if (this._parser.SetPointsToSkipTbl.TryGetRow(new[] { setpointRow["IDENTIFICATION_TEXT"].ToString() }, out DataRow _))
                //{
                //    continue;
                //}
                // Check external identity to and skip those containing PROTT
                if (setpointRow["EXTERNAL_IDENTITY"].ToString().Contains("PROTT"))
                {
                    Logger.Log("SetpointSkippedPoints", LoggerLevel.INFO, $"Setpoint: Contains PROTT in exID . ExID: {setpointRow["EXTERNAL_IDENTITY"].ToString()}\t ");
                    continue;
                }
                // Check if RTU_IRN is non empty, if so: check to skip.
                if (!string.IsNullOrEmpty(setpointRow["RTU_IRN"].ToString()) && EdpespScadaExtensions.SkipRtuPoint(this._parser.RtuTbl, setpointRow["RTU_IRN"].ToString()))
                {
                    Logger.Log("SetpointSkippedPoints", LoggerLevel.INFO, $"Setpoint: RTU_IRN match not found in RTU table . ExID: {setpointRow["EXTERNAL_IDENTITY"].ToString()}\t ");
                    continue;
                }
                //SA:20111124
                string name = EdpespScadaExtensions.GetName(setpointRow["IDENTIFICATION_TEXT"].ToString(), this._parser.StationAbbrTbl);
                //string name = "";
                setpointObj.CurrentRecordNo = ++setpointRec;
                string voltage = EdpespScadaExtensions.SetpClass2(setpointObj, setpointRow["IDENTIFICATION_TEXT"].ToString(), this._class2Dict, this._parser.StationAbbrTbl);
                if (!string.IsNullOrEmpty(voltage)) name = name.Replace(voltage, "");
                setpointObj.SetValue("Name", name.Trim());

                // Set Type
                int type = GetType(setpointRow);
                setpointObj.SetValue("Type", type);

                // Set pStation, pAORGroup, and pALARMGROUP
                setpointObj.SetValue("pStation", pStation);
                setpointObj.SetValue("pAORGroup", AOR_GROUP);
                setpointObj.SetValue("pALARM_GROUP", pALARM_GROUP);
                setpointObj.SetValue("Archive_group", 192); // 10000000 || 01000000 == 11000000 (8th and 7th bit on)
                //setpointObj.SetValue("Archive_group", 8, 1);

                // Set pUnit, pScale, and pRtu
                EdpespScadaExtensions.SetpUnit(setpointObj, setpointRow, this._unitDict);
                SetpScale(setpointObj, setpointRow);
                int pRtu = EdpespScadaExtensions.FindpRtu(setpointRow, this._rtuDataDict);
                if (!pRtu.Equals(-1))
                {
                    setpointObj.SetValue("pRTU", pRtu);
                }
                // Set Scada Key
                int mid = pStation+5000;
                if(!(type.Equals((int)SetpointType.M_STPNT) || type.Equals((int)SetpointType.C_STPNT)) && !pRtu.Equals(-1)) mid = pRtu;
                string key = EdpespScadaExtensions.SetScadaKey(setpointObj, type, mid, this._scadaDb, ScadaType.SETPOINT,
                    this._parser.ScadaKeyTbl, this._parser.LockedKeys, setpointRow["EXTERNAL_IDENTITY"].ToString(),false);
                if (string.IsNullOrEmpty(key))
                {
                    Logger.Log("EMPTY KEY", LoggerLevel.INFO, $"SETPOINT: Key was empty. Name: {name}\t pStation: {pStation}\t Type: {type}");
                }

                // Set Limits
                setpointObj.SetValue("HiLimit", setpointRow["MAXIMUM_VALUE"]);
                setpointObj.SetValue("LoLimit", GetMinVal(setpointRow["MINIMUM_VALUE"].ToString()));
                setpointObj.SetValue("CtrlROCLimit", setpointRow["MAXIMUM_VALUE"]);

                // Add to XREF
                string[] scadaXrefFields = { "SETPOINT", setpointRow["IDENTIFICATION_TEXT"].ToString(), key, pStation.ToString(), setpointRec.ToString(), setpointRow["EXTERNAL_IDENTITY"].ToString(),
                    string.Empty, string.Empty, string.Empty, string.Empty, setpointRow["IRN"].ToString(),"False", Regex.Replace(setpointRow["EXTERNAL_IDENTITY"].ToString(), @"\s+", ""), type.ToString()};
                this._scadaXref.AddRecordSetValues(scadaXrefFields);
            }

            Logger.CloseXMLLog();
        }

        /// <summary>
        /// Helper function to get get Type.
        /// </summary>
        /// <param name="setpointRow">Current DataRow row.</param>
        /// <returns></returns>
        public int GetType(DataRow setpointRow)
        {
            string setpointTypeCode = setpointRow["SETPOINT_TYPE_CODE"].ToString();
            if(setpointTypeCode == "A"|| setpointTypeCode == "D")
            {
                int ttt = 0;
            }
            switch (setpointTypeCode)
            {
                case "A":
                case "D":
                    return (int)SetpointType.T_STPNT;
                case "C":
                    return (int)SetpointType.C_STPNT;
                default:
                    Logger.Log("UHANDLED SETPOINT TYPE CODE", LoggerLevel.INFO, $"Provided Setpoint Type Code unmapped: {setpointTypeCode}\tSetting Type to M_STPNT");
                    return (int)SetpointType.M_STPNT;
            }
        }

        /// <summary>
        /// Helper function to get min val.
        /// </summary>
        /// <param name="rawMinVal">Raw min val from input file.</param>
        /// <returns>Desired min value based on raw value.</returns>
        public string GetMinVal(string rawMinVal)
        {
            if (rawMinVal.Equals("-4") || rawMinVal.Equals("-1"))
            {
                return rawMinVal;
            }
            else
            {
                return "0";
            }
        }

        /// <summary>
        /// Helper function to set pScale based on minimum value.
        /// </summary>
        /// <param name="setpointObj">Current setpoint object.</param>
        /// <param name="setpointRow">Current setpoint data row.</param>
        public void SetpScale(DbObject setpointObj, DataRow setpointRow)
        {
            switch (setpointRow["MINIMUM_VALUE"].ToString())
            {
                case "-4":
                    setpointObj.SetValue("pScale", this._scaleDict["Setpoint -4 +4"]);
                    break;
                case "-320":
                    setpointObj.SetValue("pScale", this._scaleDict["Setpoint -320 +320"]);
                    break;
                case "0":
                    setpointObj.SetValue("pScale", this._scaleDict["Setpoint 0-600"]);
                    break;
                default:
                    Logger.Log("NO pSCALE", LoggerLevel.INFO, $"pScale from input not found {setpointRow["MINIMUM_VALUE"]}\t setting pScale to 1 (default).");
                    setpointObj.SetValue("pScale", 1);
                    break;
            }
        }
    }
}
