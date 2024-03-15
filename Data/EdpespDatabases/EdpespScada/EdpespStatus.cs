using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using OSII.ConversionToolkit;
using OSII.ConversionToolkit.Extensions;
using OSII.ConversionToolkit.Generic;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespScada
{
    class EdpespStatus
    {
        private readonly EdpespParser _parser;  // Local reference of the parser
        private readonly SCADA _scadaDb;  // Local reference of the Scada Database
        private readonly FEP _fepDb;  // Local reference of the Fep Database
        private readonly Dictionary<string, int> _stationDict;  // Local reference of the station Dict. 
        private readonly Dictionary<string, string> _BayDict;  // Local reference of the station Dict. 
        private readonly Dictionary<string, int> _aorDict;  // Local reference of the AORGroup Dict. 
        private readonly Dictionary<string, string> _RtuIrnaorDict;  // Local reference of the AORGroup Dict. 
        private readonly Dictionary<string, int> _alarmsDict;  // Local reference of the alarm Dict. 
        private readonly Dictionary<string, int> _class2Dict;  // Local reference of the Class2 Dict. 
        private readonly Dictionary<(string, string), int> _oldStateDict;  // Local reference of the old state dictionary.
        private readonly Dictionary<(string, string), int> _newStateDict;  // Local reference of the new state dictionary.
        private readonly Dictionary<string, int> _rtuDataDict;  // Local reference to the RTU_DATA Dictionary.
        private readonly Dictionary<(int, int), int> _rtuIoaDict;  // Local reference to the RTU, subtype to IOA dictionary.
        private readonly GenericTable _scadaXref;  // Local reference to the Scada Xref
        private readonly GenericTable _scadaToFepXref;  // Local reference to the Scada to Fep Xref
        private readonly Dictionary<int, string> statedict;
        private Dictionary<string, List<string>> _StnRtuStatus = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> _StnPoints = new Dictionary<string, List<string>>();
        private Dictionary<string, string> _StnNameDict;
        string sPath = Path.Combine(@"D:\Source\Repos\AEML\Database Conversion\Input\Database_Conversion\Input_Files\Database_CSV\CSV", "StnRtuStatus.txt");

        //private object statedict;

        /// <summary>
        /// Default Constructor.
        /// Assigns local references of important variables.
        /// </summary>
        /// <param name="par">Current Parser.</param>
        /// <param name="scadaDb">Current Scada database.</param>
        /// <param name="fepDb">Current Fep database object.</param>
        /// <param name="stationDict">Station dictionary object.</param>
        /// <param name="oldStateDict">Old state dictionary.</param>
        /// <param name="newStateDict">New state dictionary object.</param>
        /// <param name="alarmsDict">Alarm dictionary object.</param>
        /// <param name="class2Dict">Class2 dictionary object.</param>
        /// <param name="rtuDataDict">RTU_DATA dictionary object.</param>
        /// <param name="rtuIoaDict">RTU to IOA dictionary.</param>
        /// <param name="scadaXref">Current Scada XREF object.</param>
        /// <param name="statedict">Current Scada XREF object.</param>
        /// <param name="AorDict">Aorgroup dictionary object.</param>
        public EdpespStatus(EdpespParser par, SCADA scadaDb, FEP fepDb, Dictionary<string, int> stationDict, Dictionary<string, string> BayDict, Dictionary<(string, string), int> oldStateDict,
            Dictionary<(string, string), int> newStateDict, Dictionary<string, int> alarmsDict, Dictionary<string, int> class2Dict, Dictionary<string, int> rtuDataDict,
                Dictionary<(int, int), int> rtuIoaDict, GenericTable scadaXref, GenericTable scadaToFepXref,Dictionary<int,string> statedict, Dictionary<string, int> AorDict, Dictionary<string, string> StnNameDict, Dictionary<string, string> RtuIrnAorDict)
        {
            this._parser = par;
            this._scadaDb = scadaDb;
            this._fepDb = fepDb;
            this._stationDict = stationDict;
            this._oldStateDict = oldStateDict;
            this._newStateDict = newStateDict;
            this._scadaXref = scadaXref;
            this._alarmsDict = alarmsDict;
            this._class2Dict = class2Dict;
            this._rtuDataDict = rtuDataDict;
            this._rtuIoaDict = rtuIoaDict;
            this._scadaToFepXref = scadaToFepXref;
            this.statedict = statedict;
            this._aorDict = AorDict;
            this._StnNameDict = StnNameDict;
            this._BayDict = BayDict;
            this._RtuIrnaorDict = RtuIrnAorDict;
        }

        /// <summary>
        /// Function to convert all STATUS objects.
        /// </summary>
        public void ConvertStatus()
        {
            Logger.OpenXMLLog();

            DbObject statusObj = this._scadaDb.GetDbObject("STATUS");
            int statusRec = 0;

            // Sort for later
            this._parser.RtuTbl.Sort("IRN");
            this._parser.IndicationTbl.Sort("BAY_IRN");

            // Constant values from mapping
            const int STATE_CALC = 0;
            const int QUAD_STATE_CALC = 5;
            foreach (DataRow statusRow in this._parser.IndicationTbl.Rows)
            {
                string dictkey = statusRow["STATION_IRN"].ToString();

                dictkey = (this._StnNameDict.ContainsKey(dictkey)) ? this._StnNameDict[dictkey]: dictkey;
                //if (aa != null) dictkey = aa["STATION_ID_TEXT"].ToString();
                string dictvalue = statusRow["RTU_IRN"].ToString();
                if(dictkey == "0")
                {
                    int t = 0;
                }
                List<string> Rtulist = new List<string>();
                if (this._StnRtuStatus.ContainsKey(dictkey))
                    Rtulist = this._StnRtuStatus[dictkey];

                //if(termlist == null ) termlist.Add(dictvalue);
                if (!Rtulist.Contains(dictvalue) && !string.IsNullOrEmpty(dictvalue))
                {
                    Rtulist.Add(dictvalue);
                }
                this._StnRtuStatus[dictkey] = Rtulist;
                
            }
            
            var NoRtu = this._StnRtuStatus.Where(x=>x.Value.Count == 0).ToList();
            var MultipleRtus = this._StnRtuStatus.Where(y=>y.Value.Count > 1).ToList();
            foreach (var Rtus in NoRtu)
            {
                using (StreamWriter sw = (File.Exists(sPath)) ? File.AppendText(sPath) : File.CreateText(sPath))
                {
                    //sw.WriteLine("No RTU Station: {0}, ", Rtus.Key);
                }
            }
            foreach (var Rtus in MultipleRtus)
            {
                using (StreamWriter sw = (File.Exists(sPath)) ? File.AppendText(sPath) : File.CreateText(sPath))
                {
                    //sw.WriteLine("Multiple RTU Stations: {0} ", Rtus.Key);
                }
            }
            
            foreach (DataRow statusRow in this._parser.IndicationTbl.Rows)
            {
                if(statusRow["EXTERNAL_IDENTITY"].ToString() == "ARY22033 B4SW33SERVPOS" || statusRow["EXTERNAL_IDENTITY"].ToString() == "VSV22033 B7SW76ISO")
                {
                    int t = 0;
                }
                DataRow Statusomit = null;
                Statusomit = EdpespScadaExtensions.MeasurandRtuPointCheck(this._parser.MeasOmitTbl, statusRow["EXTERNAL_IDENTITY"].ToString());
                if (Statusomit != null)
                {
                    Logger.Log("StatusSkippedPoints", LoggerLevel.INFO, $"Status: Point to be omitted. ExID:{statusRow["EXTERNAL_IDENTITY"].ToString()}\t ");
                    continue;
                }
                bool Notfoundinold = true;
                //Skip the points with description as OSI_NA: Given by custonmer 20220328
                if (statusRow["DESCRIPTION"].ToString() == "OSI_NA" )//|| statusRow["EXTERNAL_IDENTITY"].ToString().EndsWith("PKUP")|| statusRow["EXTERNAL_IDENTITY"].ToString().EndsWith("TRIP"))
                {
                    Logger.Log("StatusSkippedPoints", LoggerLevel.INFO, $"STATUS: Description is OSI_NA  ExID:{statusRow["EXTERNAL_IDENTITY"].ToString()}\t ");
                    continue;
                    
                }
                
                bool isControl = false;
                // Check for pStation first to know which points to skip.
                //Skip the points which are telemetry and there is no RTU linked
                if (string.IsNullOrEmpty(statusRow["RTU_IRN"].ToString()) && (!(statusRow["INDICATION_TYPE_CODE"].ToString() == "M" || statusRow["INDICATION_TYPE_CODE"].ToString() == "C")))
                {
                    Logger.Log("StatusSkippedPoints", LoggerLevel.INFO, $"STATUS: RTU_IRN is empty. ExID:{statusRow["EXTERNAL_IDENTITY"].ToString()}\t ");
                    continue;
                    
                }
            
                if(/*statusRow["RTU_IRN"].ToString() == "2026310251" || */statusRow["IDENTIFICATION_TEXT"].ToString().Contains("MAROL MAROSHI ROAD 1_SCZ_213_5_T")) 
                {
                    int tt = 0;
                }
                if (string.IsNullOrEmpty(statusRow["PXINT1"].ToString()) && (statusRow["EXTERNAL_IDENTITY"].ToString().EndsWith("PKUP") || statusRow["EXTERNAL_IDENTITY"].ToString().EndsWith("TRIP"))) //BD new file update 230428
                {
                    Logger.Log("StatusSkippedPoints", LoggerLevel.INFO, $"STATUS: IOA is empty for TripPKUP. ExID:{statusRow["EXTERNAL_IDENTITY"].ToString()}\t ");
                    continue;
                }
                
                bool skipstatus = true;
                DataRow statusrow2 = null;
                // Check RTU_Irn missing points
                if (string.IsNullOrEmpty(statusRow["PXINT1"].ToString()) && string.IsNullOrEmpty(statusRow["PXINT1CMD"].ToString()) && !(statusRow["EXTERNAL_IDENTITY"].ToString().EndsWith("PKUP") || statusRow["EXTERNAL_IDENTITY"].ToString().EndsWith("TRIP"))) //BD new file update 230428
                {
                    statusrow2 = EdpespScadaExtensions.IndIoaPointCheck(this._parser.IndCMDTbl, statusRow["EXTERNAL_IDENTITY"].ToString());
                    if (statusrow2 != null) skipstatus = false;
                }
                
                //Skip if IOA is empty
                //if ((statusRow["INDICATION_TYPE_CODE"].ToString() == "D"|| statusRow["INDICATION_TYPE_CODE"].ToString() =="S" || statusRow["INDICATION_TYPE_CODE"].ToString() == "N") && ( string.IsNullOrEmpty(statusRow["PXINT1"].ToString()) &&  string.IsNullOrEmpty(statusRow["PXINT1CMD"].ToString())) && skipstatus)
                if ((statusRow["INDICATION_TYPE_CODE"].ToString() == "D" || statusRow["INDICATION_TYPE_CODE"].ToString() == "S" || statusRow["INDICATION_TYPE_CODE"].ToString() == "N") && (string.IsNullOrEmpty(statusRow["PXINT1"].ToString()) && string.IsNullOrEmpty(statusRow["PXINT1CMD"].ToString())) && skipstatus)
                {
                    Logger.Log("StatusSkippedPoints", LoggerLevel.INFO, $"STATUS: IOA is empty. ExID:{statusRow["EXTERNAL_IDENTITY"].ToString()}\t ");
                    continue ;
                    
                }
                string splitRTU = "";
                //skip the points belonging to trianagle gateway Stations:BD

                //if (new List<string> { "222452251", "1798949251", "1795852251", "2042422251", "2073195251", "2089811251", "1995038251", "2016324251", "2016325251", "1992663251", "2009856251", "2026310251", "1972835251", "2076307251", "2020391251", "2043217251" }.Contains(statusRow["RTU_IRN"].ToString()))
                //if (new List<string> { "138093201", "206805261", "1798949251", "1795852251", "1972835251", "1992663251", "1995038251", "2009856251", "2016324251", "2016325251", "2020391251", "2026310251", "2042422251", "2043217251", "2076307251", "222452251", "2073195251", "2089811251" }.Contains(statusRow["RTU_IRN"].ToString()))
                if (new List<string> { "138093201", "222452251", "1798949251", "2089811251", "1972835251", "1995038251", "2016324251", "2016325251", "1795852251", "2009856251", "2073195251", "1992663251", "2026310251", "206805261", "2020391251", "2043217251", "2042422251", "2076307251" }.Contains(statusRow["RTU_IRN"].ToString()))
                {
                    Logger.Log("StatusSkippedPoints", LoggerLevel.INFO, $"STATUS: Belongs to Triangle gateway. ExID:{statusRow["EXTERNAL_IDENTITY"].ToString()}\t ");
                    continue;
                    
                }
                int pStation = string.IsNullOrEmpty(statusRow["RTU_IRN"].ToString()) ? 5000 : EdpespScadaExtensions.GetpStation(statusRow["RTU_IRN"].ToString(), this._stationDict);
                //int pStation = EdpespScadaExtensions.GetpStation(statusRow["STATION_IRN"].ToString(), this._stationDict);
                //int pStation = EdpespScadaExtensions.GetpAorGroup(statusRow["SUBSYSTEM_IRN"].ToString(), this._stationDict);
                if(statusRow["IDENTIFICATION_TEXT"].ToString().ToUpper().StartsWith("AAREY220  33KV") && statusRow["RTU_IRN"].ToString() == "138096201") //138096201
                {
                    splitRTU = "400001";
                    //pStation = EdpespScadaExtensions.GetpStation(splitRTU, this._stationDict);
                }
                if (statusRow["IDENTIFICATION_TEXT"].ToString().ToUpper().StartsWith("VERSO220  33KV") && statusRow["RTU_IRN"].ToString() == "138097201")//138097201
                {
                    splitRTU = "400002";
                    //pStation = EdpespScadaExtensions.GetpStation(splitRTU, this._stationDict);
                }
                if (pStation.Equals(-1))
                {
                    Logger.Log("StatusSkippedPoints", LoggerLevel.INFO, $"STATUS: pStation is empty. ExID:{statusRow["EXTERNAL_IDENTITY"].ToString()}\t ");
                    continue;
                    
                }
                // Check points in display to know which to skip.
                //SA:20111124
                //if (EdpespScadaExtensions.ToSkip(this._parser.StationsToSkip, this._parser.PointsFromDisplay, statusRow["STATION_IRN"].ToString(), statusRow["EXTERNAL_IDENTITY"].ToString()))
                //{
                //    continue;
                //}
                // Check type and external identity to know which points to skip.
                
                int type = GetType(statusRow);
                if (!skipstatus && !string.IsNullOrEmpty(statusrow2["PXINT1"].ToString()))  //BD new file update 230428
                {
                    type = (int)StatusType.T_IND;
                }
                if (!skipstatus && string.IsNullOrEmpty(statusrow2["PXINT1"].ToString()))  //BD new file update 230428
                {
                    Logger.Log("StatusSkippedPoints", LoggerLevel.INFO, $"STATUS: IOA is empty for updated point. ExID:{statusRow["EXTERNAL_IDENTITY"].ToString()}\t ");
                    continue;
                }
                if ((statusRow["EXTERNAL_IDENTITY"].ToString().EndsWith("PKUP") || statusRow["EXTERNAL_IDENTITY"].ToString().EndsWith("TRIP")) && !string.IsNullOrEmpty(statusRow["PXINT1"].ToString())) //BD new file update 230428
                {
                    type = (int)StatusType.T_IND;
                }
                if (type.Equals(-1) || statusRow["EXTERNAL_IDENTITY"].ToString().Contains("PROTT"))
                {
                    Logger.Log("StatusSkippedPoints", LoggerLevel.INFO, $"STATUS: Type is invalid. ExID:{statusRow["EXTERNAL_IDENTITY"].ToString()}\t ");
                    continue;
                }
                // Check field 'PXINT1CMD' for non-0, if true it is a control. 
                if (!string.IsNullOrEmpty(statusRow["PXINT1CMD"].ToString()) && !(statusRow["EXTERNAL_IDENTITY"].ToString().EndsWith("PKUP") || statusRow["EXTERNAL_IDENTITY"].ToString().EndsWith("TRIP")))
                {
                    isControl = true;
                }
                
                // Check if IDENTIFICATION_TEXT contains Carripont
                if (statusRow["IDENTIFICATION_TEXT"].ToString().Contains("Carripont"))
                {
                    continue;
                }
                // Check if RTU_IRN is non empty, if so: check to skip.
                if (!string.IsNullOrEmpty(statusRow["RTU_IRN"].ToString()) && EdpespScadaExtensions.SkipRtuPoint(this._parser.RtuTbl, statusRow["RTU_IRN"].ToString() ) )
                {
                    Logger.Log("StatusSkippedPoints", LoggerLevel.INFO, $"STATUS: No match for RTU_IRN in RTUTable. ExID: {statusRow["EXTERNAL_IDENTITY"].ToString()}\t ");
                    continue;
                }
                // Check if STATION_IRN equals 84525201 and RTU_TYPE is ICCP, if so skip.
                //if (statusRow["STATION_IRN"].ToString().Equals("84525201") && statusRow["RTU_TYPE_ASC"].ToString().Equals("ICCP"))
                //{
                //    continue;
                //}
                // Check if it needs a duplicate points.
                //SA:20111124
                //bool needsDuplicate = EdpespScadaExtensions.CheckForDuplicate(statusRow["EXTERNAL_IDENTITY"].ToString(), this._parser.IddValTbl);


                // Set Record
                statusObj.CurrentRecordNo = ++statusRec;

                // Set Type, Name, pDeviceInstance, and pClass2
                statusObj.SetValue("Type", type);
                if(type == (int)StatusType.T_IC || type == (int)StatusType.T_CTL)
                {
                    statusObj.SetValue("Timer", 20);
                }
                
                //int myKey = this.statedict.FirstOrDefault(x => x.Value == stateName).Key;
                //if (statusRow["IDENTIFICATION_TEXT"].ToString().Contains("Circuit Breaker") && stateName.Contains("OPEN") || statusRow["IDENTIFICATION_TEXT"].ToString().Contains("CB") && stateName.Contains("OPEN")) { myKey += 1; }
                //if (statusRow["IDENTIFICATION_TEXT"].ToString().Contains("Isolator") && stateName.Contains("OPEN")) { myKey += 2; }
                //statusObj.SetValue("pStates", myKey);
                string name = EdpespScadaExtensions.GetName(statusRow["IDENTIFICATION_TEXT"].ToString(), this._parser.StationAbbrTbl);
                //SA:20211207
                //string name = "";
                int pDeviceInstance = 0;
                
                if (type.Equals((int)StatusType.T_IND) || type.Equals((int)StatusType.T_IC))
                {
                    name = EdpespScadaExtensions.GetpDeviceInstance(name, statusRow["RTU_TYPE_ASC"].ToString().Equals("ICCP"), out pDeviceInstance);
                    //SA:20211125
                    //statusObj.SetValue("pDeviceInstance", pDeviceInstance);
                }
                
                    string voltage = EdpespScadaExtensions.SetpClass2(statusObj, statusRow["IDENTIFICATION_TEXT"].ToString(), this._class2Dict, this._parser.StationAbbrTbl);
                if (!string.IsNullOrEmpty(voltage)) name = name.Replace(voltage, "");
                //SA:20111124
                //if (needsDuplicate) name += "_CECOEL";
                name = statusRow["Full_Name"].ToString(); //To include Full Name
                statusObj.SetValue("Name", name.Trim());
                if (name == "B10S2 Auto-Seq" || name == "B10S2 Auto-Seq Enable Disable")
                {
                    int t = 0;

                }
                if ( name == "33KV   33115       7UT61       Ph L1 Trip")
                {
                    int t = 0;
                }
                // Set pStation and pAORGroup, and Archive Group
                statusObj.SetValue("pStation", pStation);
                SetpAor(statusObj, pStation);
                //EdpespScadaExtensions.SetArchiveGroup(statusObj, ScadaType.STATUS, statusRow["HIS_TYPE_IRN"].ToString());
                EdpespScadaExtensions.SetArchiveGroup2(statusObj, ScadaType.STATUS, statusRow["HIS_TYPE_IRN"].ToString());//BD: Updated as per v3 mapping file

                // Set Scada Key and pALARM_GROUP
                int firstTwo = type;
                if (statusRow["RTU_TYPE_ASC"].ToString().Equals("ICCP"))
                {
                    firstTwo = 11;
                }
                if (name == "220KV  TR3CB                   89E Interlock Bypass" && firstTwo==2 )
                {
                    int t = 0;
                }
                //string key = EdpespScadaExtensions.SetScadaKey(statusObj, firstTwo, pStation, this._scadaDb, ScadaType.STATUS,
                //    this._parser.ScadaKeyTbl, this._parser.LockedKeys, statusRow["EXTERNAL_IDENTITY"].ToString());
                string AlarmName = "";
                DataRow AlarmRow = this._parser.AlarmIndicationTbl.GetInfo(new object[] { statusRow["IRN"].ToString() });
                if (AlarmRow != null)
                {
                    AlarmName = AlarmRow["OSI_Alarm_Group"].ToString();
                }
                EdpespScadaExtensions.SetpAlarmGroup(statusObj, AlarmName, this._alarmsDict, "STATUS", statusRow["IRN"].ToString(), name);
                //EdpespScadaExtensions.SetpAlarmGroup(statusObj, statusRow["ALARM_GROUP_IND_IRN"].ToString(), this._alarmsDict);

                
                bool isQuad = SetpStates(statusObj, statusRow);
                
                string state1 = statusRow["STATUS_TEXT_01"].ToString();
                string state2 = statusRow["STATUS_TEXT_10"].ToString();
                string stateName = state1 + "/" + state2;
                stateName = stateName.Replace(":STATUS_", "");
                stateName = (isQuad) ? "Quad_" + stateName : stateName;
                int myKey = this.statedict.FirstOrDefault(x => x.Value == stateName).Key;
                //if (statusRow["IDENTIFICATION_TEXT"].ToString().Contains("Circuit Breaker") && stateName.Contains("OPEN") || statusRow["IDENTIFICATION_TEXT"].ToString().Contains("CB") && stateName.Contains("OPEN")) { myKey += 1; }// check why was it added???
                //if (statusRow["IDENTIFICATION_TEXT"].ToString().Contains("Isolator") && stateName.Contains("OPEN")) { myKey += 2; }
                
                statusObj.SetValue("pStates", myKey);
                
                // Set ConfigNormalState and pStates
                //:Invert the StateCalc to get 
                //SetConfigNormalState2(statusObj, statusRow, myKey, stateName);// BD: Using logic as per the v3 mapping file
                SetConfigNormalState(statusObj, statusRow);// BD: Using logic as per the v3 mapping file

                // Set ConfigBits and add to quad table if point is quad, set State Calc as well.
                if (isQuad)
                {
                    statusObj.SetValue("ConfigBits", 8);
                    //this._fepDb.AddToQuadTable(key, type);
                    //Commenting following part as per updated Mapping file
                    //if (pDeviceInstance.Equals(3))  // if quad point is also an ICCP point, set state calc to 0
                    //{
                    //    statusObj.SetValue("StateCalc", STATE_CALC);//TODO: for Inverting the StateCalc change here
                    //}
                    //else
                    //{
                    //    statusObj.SetValue("StateCalc", QUAD_STATE_CALC);
                    //}
                    //BD: Updated asper v3 mapping file
                    if (statusRow["INVERTED_STATUS"].ToString() == "1")
                    {
                        statusObj.SetValue("StateCalc", QUAD_STATE_CALC);
                    }
                    else 
                        statusObj.SetValue("StateCalc", STATE_CALC);
                }
                else
                {
                    //statusObj.SetValue("StateCalc", STATE_CALC);  // set state calc to 0 if non quad point.
                    //BD: Updated asper v3 mapping file
                    if (statusRow["INVERTED_STATUS"].ToString() == "1")
                    {
                        statusObj.SetValue("StateCalc", 1);
                    }
                    else 
                        statusObj.SetValue("StateCalc", STATE_CALC);
                }
                
                // 
                // Add to protocol count and set pRtu
                int pRtu = EdpespScadaExtensions.FindpRtu(statusRow, this._rtuDataDict);

                //Key creation
                int mid = pStation+ 5000;
                if (!(type.Equals((int)StatusType.C_IND) || type.Equals((int)StatusType.M_IND)) && !pRtu.Equals(-1)) mid = pRtu;
                string key = EdpespScadaExtensions.SetScadaKey(statusObj, firstTwo, mid, this._scadaDb, ScadaType.STATUS,
                this._parser.ScadaKeyTbl, this._parser.LockedKeys, name, isQuad);
                if (key == "01001317" || key == "01001318"|| key =="01001319")
                {
                    int tt = 0;
                }
                if (isQuad && !pRtu.Equals(-1))
                {
                    //key = "02" + key.Substring(2);
                    //statusObj.SetValue("Key", key);
                }
                if (string.IsNullOrEmpty(key))
                {
                    Logger.Log("EMPTY KEY", LoggerLevel.INFO, $"STATUS: Key was empty. Name: {name}\t pStation: {pStation}\t Type: {type}");
                }

                statusObj.SetValue("State",0,1);
                if (isQuad && myKey == 260)
                {
                    statusObj.SetValue("ConfigBits2", 16);
                    //statusObj.SetValue("StateCalc", 5);
                    statusObj.SetValue("State", 0, 2);
                }
                EdpespScadaExtensions.I104PointTypes fepPointType = EdpespScadaExtensions.I104PointTypes.None;
                int address = 0;
                string ioa = !string.IsNullOrEmpty(statusRow["PXINT1"].ToString()) ? statusRow["PXINT1"].ToString() : statusRow["PXINT1CMD"].ToString();
                if((statusRow["EXTERNAL_IDENTITY"].ToString().EndsWith("PKUP") || statusRow["EXTERNAL_IDENTITY"].ToString().EndsWith("TRIP") ))
                {
                    ioa = statusRow["PXINT1"].ToString();
                }
                if (!skipstatus)//BD new file update 230428
                {
                    ioa = statusrow2["PXINT1"].ToString();
                }
                if(key == "01002713" || key == "01002730" || key == "01004441")
                {
                    int t = 0;
                }
                if (!pRtu.Equals(-1) && !string.IsNullOrEmpty(ioa) && type != 3)
                {
                    fepPointType = isQuad ? EdpespScadaExtensions.I104PointTypes.DOUBLE_PT : EdpespScadaExtensions.I104PointTypes.SINGLE_PT;
                    address = EdpespScadaExtensions.GetNextRtuDefnAddress(pRtu, fepPointType);
                    statusObj.SetValue("pPoint", address);
                    statusObj.SetValue("AppID", 2);
                    if (isQuad) statusObj.SetValue("SourceType", 2);
                    else statusObj.SetValue("SourceType", 1);
                    if (address == 318)
                    {
                        int t = 0;
                    }
                    //if (!isControl || !string.IsNullOrEmpty(statusRow["PXINT1"].ToString()) || !skipstatus)
                    {
                        //int index = this._fepDb.AddToRtuCount(pRtu, address, isQuad, (int)fepPointType, true);
                        this._scadaToFepXref.AddRecordSetValues(new string[] { "STATUS", statusRec.ToString(), pRtu.ToString(), ((int)fepPointType).ToString(), key, ioa, isQuad.ToString(), address.Equals(0) ? string.Empty : address.ToString(), statusRow["EXTERNAL_IDENTITY"].ToString() });
                    }
                    statusObj.SetValue("pRTU", pRtu);
                }
                

                if (isQuad)
                {
                    
                    this._fepDb.AddToQuadTable(key, type);
                }
                    // Add to XREF
                    string[] scadaXrefFields = { "STATUS", statusRow["IDENTIFICATION_TEXT"].ToString(), key, pStation.ToString(), statusRec.ToString(),
                    statusRow["EXTERNAL_IDENTITY"].ToString(), pRtu.Equals(-1)? string.Empty : pRtu.ToString(),
                        ((int)fepPointType).Equals(0)? string.Empty : ((int)fepPointType).ToString(), address.Equals(0)? string.Empty : address.ToString(), ioa,
                            statusRow["IRN"].ToString(), isQuad.ToString(), Regex.Replace(statusRow["EXTERNAL_IDENTITY"].ToString(), @"\s+", ""), type.ToString()};
                this._scadaXref.AddRecordSetValues(scadaXrefFields);

                //Add list of points to dictionary with key as pStation and list of points as values
                string dictkey = statusObj.GetValue("pStation", 0);
                string dictvalue = statusObj.GetValue("Name", 0);
                
                List<string> pointslist = new List<string>();
                if (this._StnPoints.ContainsKey(dictkey))
                    pointslist = this._StnPoints[dictkey];

                //if(termlist == null ) termlist.Add(dictvalue);
                if (!pointslist.Contains(dictvalue) && !string.IsNullOrEmpty(dictvalue))
                {
                    pointslist.Add(dictvalue);
                }
                this._StnPoints[dictkey] = pointslist;

                // Duplicate if needed
                //SA:20111124
                //if (needsDuplicate)
                //{
                //    ++statusRec;
                //    if (statusObj.CopyRecord(statusObj.CurrentRecordNo, statusRec))
                //    {
                //        DbObject statusCopy = this._scadaDb.GetDbObject("STATUS");
                //        statusCopy.CurrentRecordNo = statusRec;

                //        // Change name and assign new key
                //        string newName = name.Replace("_CECOEL", "_CECORE");
                //        statusCopy.SetValue("Name", newName.Trim());

                //        string copyKey = EdpespScadaExtensions.SetScadaKey(statusCopy, firstTwo, pStation, this._scadaDb, ScadaType.STATUS,
                //            this._parser.ScadaKeyTbl, this._parser.LockedKeys, statusRow["EXTERNAL_IDENTITY"].ToString() + " - Copy");
                //        if (string.IsNullOrEmpty(copyKey))
                //        {
                //            Logger.Log("EMPTY KEY", LoggerLevel.INFO, $"STATUS: Key was empty. Name: {newName}\t pStation: {pStation}\t Type: {type}");
                //        }

                //        // Add to XREF
                //        string[] copyXrefFields = { "STATUS", statusRow["IDENTIFICATION_TEXT"].ToString() + " - Copy", copyKey, pStation.ToString(), statusRec.ToString(),
                //            statusRow["EXTERNAL_IDENTITY"].ToString() + " - Copy", pRtu.Equals(-1)? string.Empty : pRtu.ToString(),
                //                ((int)fepPointType).Equals(0)? string.Empty : ((int)fepPointType).ToString(), address.Equals(0)? string.Empty : address.ToString(), ioa,
                //                    statusRow["IRN"].ToString() + "_Copy", isQuad.ToString()};
                //        this._scadaXref.AddRecordSetValues(copyXrefFields);

                //        // Set original status's value
                //        statusObj.CurrentRecordNo--;
                //        statusObj.SetValue("AltDataKey", copyKey);
                //    }
                //    else
                //    {
                //        --statusRec;
                //    }

                //}
            }
            //Calc points
            foreach (DataRow statusRow in this._parser.CalcPointsTbl.Rows)
            {


                int type = (int)StatusType.C_IND;


                statusObj.CurrentRecordNo = ++statusRec;

                // Set Type, Name, pDeviceInstance, and pClass2
                statusObj.SetValue("Type", type);

                string name = statusRow["Signal"].ToString();// EdpespScadaExtensions.GetName(statusRow["IDENTIFICATION_TEXT"].ToString(), this._parser.StationAbbrTbl);


                statusObj.SetValue("Name", name.Trim());
                int pStation = EdpespScadaExtensions.GetpStation("eMap_DPF_SG", this._stationDict);
                // Set pStation and pAORGroup, and Archive Group
                statusObj.SetValue("pStation", pStation);
                SetpAor(statusObj, pStation);
                statusObj.SetValue("pAORGroup", 1);
                statusObj.SetValue("pALARM_GROUP", 1);

                //EdpespScadaExtensions.SetArchiveGroup2(statusObj, ScadaType.STATUS, statusRow["HIS_TYPE_IRN"].ToString());//BD: Updated as per v3 mapping file
                statusObj.SetValue("Archive_group", 192); // 10000000 || 01000000 == 11000000 (8th and 7th bit on)
                //statusObj.SetValue("Archive_group", ArchiveGroup.Index7);
                // Set Scada Key and pALARM_GROUP
                int firstTwo = type;
                statusObj.SetValue("pStates", 201);

                statusObj.SetValue("StateCalc", STATE_CALC);

                // 
                // Add to protocol count and set pRtu
                int pRtu = 0;// EdpespScadaExtensions.FindpRtu(statusRow, this._rtuDataDict);
                bool isQuad = false;
                //Key creation
                int mid = pStation + 5000;
                //if (!(type.Equals((int)StatusType.C_IND) || type.Equals((int)StatusType.M_IND)) && !pRtu.Equals(-1)) mid = pRtu;
                string key = EdpespScadaExtensions.SetScadaKey(statusObj, firstTwo, mid, this._scadaDb, ScadaType.STATUS,
                this._parser.ScadaKeyTbl, this._parser.LockedKeys, name, isQuad);

                if (string.IsNullOrEmpty(key))
                {
                    Logger.Log("EMPTY KEY", LoggerLevel.INFO, $"STATUS: Key was empty. Name: {name}\t pStation: {pStation}\t Type: {type}");
                }

                statusObj.SetValue("State", 0, 1);
                EdpespScadaExtensions.I104PointTypes fepPointType = EdpespScadaExtensions.I104PointTypes.None;
                int address = 0;
                // string ioa = !string.IsNullOrEmpty(statusRow["PXINT1"].ToString()) ? statusRow["PXINT1"].ToString() : statusRow["PXINT1CMD"].ToString();

                statusObj.SetValue("pRTU", pRtu);


                if (isQuad)
                {

                    this._fepDb.AddToQuadTable(key, type);
                }
                // Add to XREF
                string[] scadaXrefFields = { "STATUS", statusRow["Signal"].ToString(), key, pStation.ToString(), statusRec.ToString(),
                    statusRow["Signal"].ToString(), pRtu.Equals(-1)? string.Empty : pRtu.ToString(),
                        ((int)fepPointType).Equals(0)? string.Empty : ((int)fepPointType).ToString(), address.Equals(0)? string.Empty : address.ToString(), "",
                            "eMap_DPF_SG", isQuad.ToString(), Regex.Replace(statusRow["Signal"].ToString(), @"\s+", ""), type.ToString()};
                this._scadaXref.AddRecordSetValues(scadaXrefFields);


            }
            //Manual Points
            foreach (DataRow statusRow in this._parser.BayTbl.Rows)
            {


                int type = (int)StatusType.M_IND;


                
                if(statusObj.GetValue("Name",0) == "")
                {
                    int t = 0;
                }
                // Set pStation and pAORGroup, and Archive Group
                string pStationIRN = GetStationIRN(statusRow["IRN"].ToString(), this._BayDict);
                if (string.IsNullOrEmpty(pStationIRN))
                {
                    Logger.Log("BAY has empty RTUIRN", LoggerLevel.INFO, $"The RTU IRN is empty in indication file for given  BAY IRN: {statusRow["IRN"].ToString()}\tSetting pStation to 0");
                    continue;
                }
                int pStation = EdpespScadaExtensions.GetpStation(pStationIRN, this._stationDict);
                if(pStation == 0)
                {
                    Logger.Log("Station not created for this BAY", LoggerLevel.INFO, $"Station not created for the BAY IRN: {statusRow["IRN"].ToString()}\tSetting pStation to 0");
                    continue;

                }

                statusObj.CurrentRecordNo = ++statusRec;

                // Set Type, Name, pDeviceInstance, and pClass2
                statusObj.SetValue("Type", type);

                string name = statusRow["IDENTIFICATION_TEXT"].ToString();// EdpespScadaExtensions.GetName(statusRow["IDENTIFICATION_TEXT"].ToString(), this._parser.StationAbbrTbl);


                statusObj.SetValue("Name", name.Trim());
                statusObj.SetValue("pStation", pStation);
                SetpAor(statusObj, pStation);
                statusObj.SetValue("pAORGroup", 1);
                statusObj.SetValue("pALARM_GROUP", 1);

                //EdpespScadaExtensions.SetArchiveGroup2(statusObj, ScadaType.STATUS, statusRow["HIS_TYPE_IRN"].ToString());//BD: Updated as per v3 mapping file
                statusObj.SetValue("Archive_group", 192); // 10000000 || 01000000 == 11000000 (8th and 7th bit on)
                // Set Scada Key and pALARM_GROUP
                int firstTwo = type;
                statusObj.SetValue("pStates", 201);

                statusObj.SetValue("StateCalc", STATE_CALC);

                // 
                // Add to protocol count and set pRtu
                int pRtu = 0;// EdpespScadaExtensions.FindpRtu(statusRow, this._rtuDataDict);
                bool isQuad = false;
                //Key creation
                int mid = pStation + 5000;
                //if (!(type.Equals((int)StatusType.C_IND) || type.Equals((int)StatusType.M_IND)) && !pRtu.Equals(-1)) mid = pRtu;
                string key = EdpespScadaExtensions.SetScadaKey(statusObj, firstTwo, mid, this._scadaDb, ScadaType.STATUS,
                this._parser.ScadaKeyTbl, this._parser.LockedKeys, name, isQuad);

                if (string.IsNullOrEmpty(key))
                {
                    Logger.Log("EMPTY KEY", LoggerLevel.INFO, $"STATUS: Key was empty. Name: {name}\t pStation: {pStation}\t Type: {type}");
                }
                
                statusObj.SetValue("State", 0, 1);
                EdpespScadaExtensions.I104PointTypes fepPointType = EdpespScadaExtensions.I104PointTypes.None;
                int address = 0;
                // string ioa = !string.IsNullOrEmpty(statusRow["PXINT1"].ToString()) ? statusRow["PXINT1"].ToString() : statusRow["PXINT1CMD"].ToString();

                statusObj.SetValue("pRTU", pRtu);


                if (isQuad)
                {

                    this._fepDb.AddToQuadTable(key, type);
                }
                string ioa = "";
                // Add to XREF
                string[] scadaXrefFields = { "STATUS", statusRow["IDENTIFICATION_TEXT"].ToString(), key, pStation.ToString(), statusRec.ToString(),
                    statusRow["EXTERNAL_IDENTITY"].ToString(), pRtu.Equals(-1)? string.Empty : pRtu.ToString(),
                        ((int)fepPointType).Equals(0)? string.Empty : ((int)fepPointType).ToString(), address.Equals(0)? string.Empty : address.ToString(), ioa,
                            statusRow["IRN"].ToString(), isQuad.ToString(), Regex.Replace(statusRow["EXTERNAL_IDENTITY"].ToString(), @"\s+", ""), type.ToString()};
                this._scadaXref.AddRecordSetValues(scadaXrefFields);


            }
            //Manual points for transformer bay points
            for (int i = 1; i < 3513; i++)  
            {
                int pStation = i;
                // int.TryParse(i,out pStation);
                if (pStation == 0) continue;
                string fullname = "";
                if (!this._StnPoints.ContainsKey(i.ToString()))
                {
                    Logger.Log("No_points_under_station", LoggerLevel.INFO, $"No points under the pStation: {i.ToString()}\tskipping this station");
                    continue;
                }
                List<string> points = this._StnPoints[pStation.ToString()];
                
                foreach (DataRow statusRow in this._parser.TrsfmrTbl.Rows)
                {
                    bool foundname = false;
                    string trsfrmrname = statusRow["SignalName"].ToString().Replace("                ", " ").Replace("               "," ").Replace("     "," ");
                    trsfrmrname = trsfrmrname.Split(' ')[1];
                    foreach(string pointname in points)
                    {
                        if (pointname.Contains(trsfrmrname))
                        {
                            foundname = true;
                            break;

                        }
                        
                    }
                    if (foundname)
                    {
                        int type = (int)StatusType.M_IND;

                        //if (pStation == 0)
                        //{
                        //    Logger.Log("Station not created for this BAY", LoggerLevel.INFO, $"Station not created for the BAY IRN: {statusRow["IRN"].ToString()}\tSetting pStation to 0");
                        //    continue;

                        //}

                        statusObj.CurrentRecordNo = ++statusRec;

                        
                        statusObj.SetValue("Type", type);

                        string name = statusRow["SignalName"].ToString();
                        statusObj.SetValue("Name", name.Trim());
                        statusObj.SetValue("pStation", pStation);
                        SetpAor(statusObj, pStation);
                        statusObj.SetValue("pAORGroup", 1);
                        statusObj.SetValue("pALARM_GROUP", 1);

                        //EdpespScadaExtensions.SetArchiveGroup2(statusObj, ScadaType.STATUS, statusRow["HIS_TYPE_IRN"].ToString());//BD: Updated as per v3 mapping file
                        statusObj.SetValue("Archive_group", 192); // 10000000 || 01000000 == 11000000 (8th and 7th bit on)
                        // Set Scada Key and pALARM_GROUP
                        int firstTwo = type;
                        statusObj.SetValue("pStates", 201);

                        statusObj.SetValue("StateCalc", STATE_CALC);

                        // 
                        // Add to protocol count and set pRtu
                        int pRtu = 0;// EdpespScadaExtensions.FindpRtu(statusRow, this._rtuDataDict);
                        bool isQuad = false;
                        //Key creation
                        int mid = pStation + 5000;
                        //if (!(type.Equals((int)StatusType.C_IND) || type.Equals((int)StatusType.M_IND)) && !pRtu.Equals(-1)) mid = pRtu;
                        string key = EdpespScadaExtensions.SetScadaKey(statusObj, firstTwo, mid, this._scadaDb, ScadaType.STATUS,
                        this._parser.ScadaKeyTbl, this._parser.LockedKeys, name, isQuad);

                        if (string.IsNullOrEmpty(key))
                        {
                            Logger.Log("EMPTY KEY", LoggerLevel.INFO, $"STATUS: Key was empty. Name: {name}\t pStation: {pStation}\t Type: {type}");
                        }

                        statusObj.SetValue("State", 0, 1);
                        EdpespScadaExtensions.I104PointTypes fepPointType = EdpespScadaExtensions.I104PointTypes.None;
                        int address = 0;
                        // string ioa = !string.IsNullOrEmpty(statusRow["PXINT1"].ToString()) ? statusRow["PXINT1"].ToString() : statusRow["PXINT1CMD"].ToString();

                        statusObj.SetValue("pRTU", pRtu);


                        if (isQuad)
                        {

                            this._fepDb.AddToQuadTable(key, type);
                        }
                        string ioa = "";
                        // Add to XREF
                        string[] scadaXrefFields = { "STATUS", statusRow["SignalName"].ToString(), key, pStation.ToString(), statusRec.ToString(),
                    statusRow["SignalName"].ToString(), pRtu.Equals(-1)? string.Empty : pRtu.ToString(),
                        ((int)fepPointType).Equals(0)? string.Empty : ((int)fepPointType).ToString(), address.Equals(0)? string.Empty : address.ToString(), ioa,
                            "00000", isQuad.ToString(), Regex.Replace(statusRow["SignalName"].ToString(), @"\s+", ""), type.ToString()};
                        this._scadaXref.AddRecordSetValues(scadaXrefFields);

                    }
                }
            }
            Logger.CloseXMLLog();
        }
        public  string GetStationIRN(string BayIRN, Dictionary<string, string> stationDict)
        {
            if (stationDict.ContainsKey(BayIRN))
            {
                return stationDict[BayIRN];
            }
            else
            {
                Logger.Log("Bay_ref_Not_Matched", LoggerLevel.INFO, $"Provided Bay IRN was not found in Indication file: {BayIRN}");
                return "0";
            }
        }
        //{
        //    var row = this._parser.IndicationTbl.GetInfo(new object[] { BayIRN });
        //    if (row!= null)
        //    {
        //       if (string.IsNullOrEmpty(row["RTU_IRN"].ToString()))
        //       {
        //            Logger.Log("Bay_ref_RTU_IRN_IS_0", LoggerLevel.INFO, $"RTU_IRN for the provided BAY IRN is empty: {BayIRN}");
        //            return "0";
        //       }
        //        else return row["RTU_IRN"].ToString();// stationDict[stationIRN];
        //    }
        //    else
        //    {
        //        Logger.Log("Bay_ref_Not_Matched", LoggerLevel.INFO, $"Provided Bay IRN was not found in Indication file: {BayIRN}");
        //        return "0";
        //    }
        //}
        /// <summary>
        /// Helper function to set Type.
        /// </summary>
        /// <param name="statusRow">Current DataRow row.</param>
        /// <returns>Int value of the Status Type.</returns>
        public int GetType(DataRow statusRow)
        {
            string controllablePoint = statusRow["CONTROLABLE_POINT"].ToString();
            string indicationTypeCode = statusRow["INDICATION_TYPE_CODE"].ToString();
            string rtuTypeAsc = statusRow["RTU_TYPE_ASC"].ToString();
            string PXINT1CMD = statusRow["PXINT1CMD"].ToString();
            string PXINT1 = statusRow["PXINT1"].ToString();

            // check controls first
            if (!string.IsNullOrEmpty(PXINT1CMD) && !string.IsNullOrEmpty(PXINT1))
            {
                return (int)StatusType.T_IC;
            }
            else if (!string.IsNullOrEmpty(PXINT1CMD) && string.IsNullOrEmpty(PXINT1))
            {
                return (int)StatusType.T_CTL;
            }

            switch (indicationTypeCode)
            {
                case "D":
                case "S":
                    if (controllablePoint.Equals("0"))
                    {
                        return (int)StatusType.T_IND;
                    }
                    else if (controllablePoint.Equals("1"))
                    {
                        if (!string.IsNullOrEmpty(PXINT1CMD) && !string.IsNullOrEmpty(PXINT1))
                        {
                            return (int)StatusType.T_IC;
                        }
                        else if (!string.IsNullOrEmpty(PXINT1CMD) && string.IsNullOrEmpty(PXINT1))
                        {
                            return (int)StatusType.T_CTL;
                        }
                        else return -1;
                        //return (int)StatusType.T_IC;//BD
                    }
                    else
                    {
                        Logger.Log("UHANDLED CONTROLLABLE POINT", LoggerLevel.INFO, $"Provided Controlable Point unmapped: {controllablePoint}\tIndicationTypeCode:{indicationTypeCode}\tSetting Type to M_IND");
                        return (int)StatusType.M_IND;
                    }
                case "N":
                    if (controllablePoint.Equals("1"))
                    {
                        if (!string.IsNullOrEmpty(PXINT1CMD) && !string.IsNullOrEmpty(PXINT1))
                        {
                            return (int)StatusType.T_IC;
                        }
                        else if (!string.IsNullOrEmpty(PXINT1CMD) && string.IsNullOrEmpty(PXINT1))
                        {
                            return (int)StatusType.T_CTL;
                        }
                        else return -1;
                        //return (int)StatusType.T_CTL;//BD
                    }
                    else
                    {
                        Logger.Log("UHANDLED CONTROLLABLE POINT", LoggerLevel.INFO, $"Provided Controlable Point unmapped: {controllablePoint}\tIndicationTypeCode:{indicationTypeCode}\tSetting Type to M_IND");
                        return (int)StatusType.M_IND;
                    }
                case "C":
                    if (rtuTypeAsc.Equals("CAL"))
                    {
                        return (int)StatusType.C_IND;
                    }
                    else if (rtuTypeAsc.Equals("ICCP"))
                    {
                        return (int)StatusType.T_IND;
                    }
                    else
                    {
                        Logger.Log("UHANDLED RTU TYPE ASC", LoggerLevel.INFO, $"Provided RtuTypeASC unmapped: {rtuTypeAsc}\tIndicationTypeCode:{indicationTypeCode}\tSetting Type to M_IND");
                        return (int)StatusType.M_IND;
                    }
                case "M":
                    return (int)StatusType.M_IND;
                default:
                    Logger.Log("UHANDLED INDICATION TYPE CODE", LoggerLevel.INFO, $"Provided Indication Type Code unmapped: {indicationTypeCode}\tSetting Type to M_IND");
                    return (int)StatusType.M_IND;
            }
        }

        /// <summary>
        /// Helper function to get pAOR group.
        /// Gets the same pAOR group as its station.
        /// </summary>
        /// <param name="statusObj">Current STATUS object.</param>
        /// <param name="pStation"></param>
        public void SetpAor(DbObject statusObj, int pStation)
        {
            DbObject stationObj = this._scadaDb.GetDbObject("STATION");
            stationObj.CurrentRecordNo = pStation;

            string AORstring = stationObj.GetValue("pAORGroup", 0);
            if (AORstring.TryParseInt(out int pAOR))
            {
                statusObj.SetValue("pAORGroup", pAOR);//BD
                //statusObj.SetValue("pAORGroup", 1);
                //statusObj.SetValue("pConfiguredAORGroup", 1);

            }
            else
            {
                //statusObj.SetValue("pAORGroup", 0);//SA20211214
                statusObj.SetValue("pAORGroup", 1);
                //statusObj.SetValue("pConfiguredAORGroup", 1);
                Logger.Log("INVALID AORGROUP", LoggerLevel.INFO, $"Provided Aor GROUP was not found for station: {stationObj.GetValue("Name", 0)}\tSetting pAorGroup to 1");
            }
        }

        /// <summary>
        /// Helper function to set ConfigNormalState.
        /// </summary>
        /// <param name="statusObj">Current STATUS object.</param>
        /// <param name="statusRow">Current DataRow row.</param>
        public void SetConfigNormalState(DbObject statusObj, DataRow statusRow)
        {
            string typeCode = statusRow["INDICATION_TYPE_CODE"].ToString();
            string normalStatus = statusRow["NORMAL_STATUS"].ToString();
            if (typeCode.Equals("D")) //QUAD
            {
                switch (normalStatus)
                {
                    case "0":
                    case "1":
                        statusObj.SetValue("ConfigNormalState", 1);
                        break;
                    case "2":
                        statusObj.SetValue("ConfigNormalState", 2);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (normalStatus)
                {
                    case "0":
                    case "1":
                        statusObj.SetValue("ConfigNormalState", 0);
                        break;
                    //case "1":
                    case "2":
                        statusObj.SetValue("ConfigNormalState", 1);
                        break;
                    default:
                        break;
                }
            }
        }
        public void SetConfigNormalState2(DbObject statusObj, DataRow statusRow, int pStates, string statename) //BD: Updated as per the v3 mapping file
        {
            //string typeCode = statusRow["INDICATION_TYPE_CODE"].ToString();
            string normalStatus = statusRow["NORMAL_STATUS"].ToString();
            int nrmlState = 1;

            if(normalStatus == "1")
            {
                string x= statusRow["STATUS_TEXT_01"].ToString().Split('_')[1];
                statusObj.SetValue("ConfigNormalState", 1);
                string ggf = this.statedict.FirstOrDefault(y => y.Key == pStates).Value;
            }
            if (normalStatus == "2")
            {
                string x = statusRow["STATUS_TEXT_10"].ToString().Split('_')[1];
                statusObj.SetValue("ConfigNormalState", 2);
            }
        }

        /// <summary>
        /// Helper function to set pState.
        /// </summary>
        /// <param name="statusObj">Current STATUS object.</param>
        /// <param name="statusRow">Current DataRow row.</param>
        /// <returns>Returns true if quad, else false.</returns>
        public bool SetpStates(DbObject statusObj, DataRow statusRow)
        {
            string indication = statusRow["INDICATION_TYPE_CODE"].ToString();
            string state1 = statusRow["EVENT_PRINTOUT_TEXT1"].ToString();
            string state2 = statusRow["EVENT_PRINTOUT_TEXT2"].ToString();
            string normalStatus = statusRow["NORMAL_STATUS"].ToString();
            bool isQuad = false;
            if (indication.Equals("D"))
            {
                state1 += state2;
                state2 = "NULL";
                isQuad = true;
            }
            
            if ((!normalStatus.Equals("1") || isQuad) && this._oldStateDict.TryGetValue((state1, state2), out int pState))
            {
                //statusObj.SetValue("pStates", pState);
            }
            else if (normalStatus.Equals("1") && this._newStateDict.TryGetValue((state2, state1), out pState))
            {
                //statusObj.SetValue("pStates", pState);
            }
            else
            {
               // statusObj.SetValue("pStates", 201);
                Logger.Log("MISSING pSTATE", LoggerLevel.INFO, $"pState not found for states: {state1} and {state2}\t Setting pStates to 201");
            }
            return isQuad;
        }
    }
}
