using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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
    class EdpespAnalog
    {
        private readonly EdpespParser _parser;  // Local reference of the parser
        private readonly SCADA _scadaDb;  // Local reference of the Scada Database
        private readonly FEP _fepDb;  // Local reference of the Fep Database
        private readonly Dictionary<string, int> _stationDict;  // Local reference of the station Dict. 
        private readonly Dictionary<string, int> _scaleDict;  // Local reference of the Scale Dict. 
        private readonly Dictionary<string, int> _aorDict;  // Local reference of the station Dict. 
        private readonly Dictionary<string, string> _RtuIrnaorDict;  // Local reference of the AORGroup Dict. 
        private readonly Dictionary<string, int> _alarmsDict;  // Local reference of the alarm Dict. 
        private readonly Dictionary<string, int> _unitDict;  // Local reference of the unit dictionary.
        private readonly Dictionary<string, int> _rtuDataDict;  // Local reference to the RTU_DATA Dictionary.
        private readonly Dictionary<(int, int), int> _rtuIoaDict;  // Local reference to the RTU, subtype to IOA dictionary.
        private readonly GenericTable _scadaXref;  // Local reference to the Scada Xref
        private readonly Dictionary<string, int> primarySourceDict;  // Dictionary for primary source points. <IRN, Rec>
        private readonly Dictionary<int, string> secondarySourceDict;  // Dictionary for secondary source points. <Rec, PAIR IRN>
        private readonly Dictionary<string, int> _deviceRecDict;  // Local reference to the deviceRecDict
        private readonly Dictionary<(string, string), int> _measScales;  // Local reference to the scale dictionary
        private readonly GenericTable _scadaToFepXref;  // Local reference to the Scada to Fep Xref
        private Dictionary<string, List<string>> _StnRtuAnalog =  new Dictionary<string, List<string>>();
        private readonly Dictionary<string, string> _StnNameDict;
        private readonly Dictionary<string, int> _switchDict;  // Local reference of the switch Dict. 
        private readonly Dictionary<string, string> _transEquipGorai;
        private readonly Dictionary<string, string> _transEquipAarey;
        private readonly Dictionary<string, string> _transEquipChemb;
        private readonly Dictionary<string, string> _transEquipSaki;
        private readonly Dictionary<string, string> _transEquipGhod;
        private readonly Dictionary<string, string> _transEquipVers;
        string sPath = Path.Combine(@"D:\Source\Repos\AEML\Database Conversion\Input\Database_Conversion\Input_Files\Database_CSV\CSV", "StnRtuAnalog.txt");
        public object ScadaDb { get; private set; }

        /// <summary>
        /// Default Constructor.
        /// Assigns local references of important variables.
        /// </summary>
        /// <param name="par">Current Parser.</param>
        /// <param name="scadaDb">Current Scada database.</param>
        /// <param name="fepDb">Current Fep Database object.</param>
        /// <param name="stationDict">Station dictionary object.</param>
        /// <param name="unitDict">Unit dictionary object.</param>
        /// <param name="alarmsDict">Alarm dictionary object.</param>
        /// <param name="rtuDataDict">RTU_DATA dictionary object.</param>
        /// <param name="rtuIoaDict">RTU to IOA dictionary.</param>
        /// <param name="deviceRecDict">pDeviceInstance dictionary.</param>
        /// <param name="measScales">pScale dictionary object.</param>
        /// <param name="scadaXref">Current Scada XREF object.</param>
        /// <param name="scadaToFepXref">Xref for SCADA to FEP.</param>
        /// <param name="AorDict">AorGroup dictionary object.</param>
        public EdpespAnalog(EdpespParser par, SCADA scadaDb, FEP fepDb, Dictionary<string, int> stationDict, Dictionary<string, int> unitDict,
            Dictionary<string, int> alarmsDict, Dictionary<string, int> rtuDataDict, Dictionary<(int, int), int> rtuIoaDict, 
                Dictionary<string, int> deviceRecDict, Dictionary<(string, string), int> measScales, GenericTable scadaXref,
                GenericTable scadaToFepXref, Dictionary<string, int> AorDict, Dictionary<string, int> ScaleDict, 
                Dictionary<string, string> StnNameDict, Dictionary<string, string> RtuIrnAorDict, Dictionary<string, int> switchDict, Dictionary<string, string> transEquipGorai,
                Dictionary<string, string> transEquipAarey, Dictionary<string, string> transEquipChemb, Dictionary<string, string> transEquipSaki, Dictionary<string, string> transEquipGhod,
                Dictionary<string, string> transEquipVers)
        {
            this._parser = par;
            this._scadaDb = scadaDb;
            this._fepDb = fepDb;
            this._stationDict = stationDict;
            this._unitDict = unitDict;
            this._scadaXref = scadaXref;
            this._alarmsDict = alarmsDict;
            this._rtuDataDict = rtuDataDict;
            this._rtuIoaDict = rtuIoaDict;
            this._deviceRecDict = deviceRecDict;
            this._measScales = measScales;
            this._scadaToFepXref = scadaToFepXref;
            this._aorDict = AorDict;
            this._scaleDict = ScaleDict;
            this._StnNameDict = StnNameDict;
            primarySourceDict = new Dictionary<string, int>();
            secondarySourceDict = new Dictionary<int, string>();
            this._RtuIrnaorDict = RtuIrnAorDict;
            this._switchDict = switchDict;
            this._transEquipGorai = transEquipGorai;
            this._transEquipAarey = transEquipAarey;
            this._transEquipChemb = transEquipChemb;
            this._transEquipSaki = transEquipSaki;
            this._transEquipGhod = transEquipGhod;
            this._transEquipVers = transEquipVers;
        }

        /// <summary>
        /// Function to convert all ANALOG objects.
        /// </summary>
        public void ConvertAnalog()
        {
            Logger.OpenXMLLog();

            DbObject analogObj = this._scadaDb.GetDbObject("ANALOG");
            DbObject stationObj = this._scadaDb.GetDbObject("STATION");


            int analogRec = 0;
            // Const value from mapping.
            int AOR_GROUP = 1;
            const int pSCALE = 1;
            const int RAW_COUNT_FORMAT = 5;
            const int telemetryMaximum = 32767;
            int telemetryMinimum = -32768;
            //var result = this._parser.MeasurandTbl.Rows.OrderByDescending(itemArray => itemArray[1]);
            foreach (DataRow statusRow in this._parser.MeasurandTbl.Rows)
            {
                string dictkey = statusRow["STATION_IRN"].ToString();

                dictkey = (this._StnNameDict.ContainsKey(dictkey)) ? this._StnNameDict[dictkey] : dictkey;
                //if (aa != null) dictkey = aa["STATION_ID_TEXT"].ToString();
                string dictvalue = statusRow["RTU_IRN"].ToString();

                List<string> Rtulist = new List<string>();
                if (this._StnRtuAnalog.ContainsKey(dictkey))
                    Rtulist = this._StnRtuAnalog[dictkey];

                //if(termlist == null ) termlist.Add(dictvalue);
                if (!Rtulist.Contains(dictvalue) && !string.IsNullOrEmpty(dictvalue))
                {
                    Rtulist.Add(dictvalue);
                }
                this._StnRtuAnalog[dictkey] = Rtulist;

            }
            var NoRtu = this._StnRtuAnalog.Where(x => x.Value.Count == 0).ToList();
            var MultipleRtus = this._StnRtuAnalog.Where(y => y.Value.Count > 1).ToList();
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
            foreach (DataRow analogRow in this._parser.MeasurandTbl.Rows)
            {
                if(analogRow["EXTERNAL_IDENTITY"].ToString() == "MGHWDI33 33037 REF620U3VOLT" || analogRow["EXTERNAL_IDENTITY"].ToString() == "D_VIHALRSN N TOUBRO B1 42042KVAR")
                {
                    int t = 0;
                }
                DataRow analogromit = null;
                analogromit = EdpespScadaExtensions.MeasurandRtuPointCheck(this._parser.MeasOmitTbl, analogRow["EXTERNAL_IDENTITY"].ToString());
                if (analogromit != null)
                {
                    Logger.Log("AnalogSkippedPoints", LoggerLevel.INFO, $"Analog: Point to be omitted. ExID:{analogRow["EXTERNAL_IDENTITY"].ToString()}\t ");
                    continue;
                }
                bool foundinold = false;
                //Skip the points with description as OSI_NA: Given by custonmer 20220328
                if (analogRow["DESCRIPTION"].ToString() == "OSI_NA")
                {
                    Logger.Log("AnalogSkippedPoints", LoggerLevel.INFO, $"Analog: Description is OSI_NA. ExID: {analogRow["EXTERNAL_IDENTITY"].ToString()}\t ");
                    continue;
                }
                // Check for old output point and pStation first to know which points to skip.
                if (!string.IsNullOrEmpty(analogRow["MEA_SRC1_IRN"].ToString()) && !string.IsNullOrEmpty(analogRow["MEA_SRC2_IRN"].ToString()))
                {
                    Logger.Log("AnalogSkippedPoints", LoggerLevel.INFO, $"Analog: Old point found MEA_SRC1_IRN. ExID: {analogRow["EXTERNAL_IDENTITY"].ToString()}\t ");
                    continue;
                }
                bool skipanalog = true;
                DataRow analogrow2 = null;
                
                // Check RTU_Irn missing points
                if (string.IsNullOrEmpty(analogRow["RTU_IRN"].ToString()) && analogRow["MEASURAND_TYPE_CODE"].ToString() == "A") //BD new file update 230428
                {
                    analogrow2 = EdpespScadaExtensions.MeasurandRtuPointCheck(this._parser.Measurand2Tbl, analogRow["EXTERNAL_IDENTITY"].ToString());
                    if (analogrow2 != null) skipanalog = false;
                }
                
                
                //Skip the points which are telemetry and there is no RTU linked
                if (string.IsNullOrEmpty(analogRow["RTU_IRN"].ToString()) &&  (!( analogRow["MEASURAND_TYPE_CODE"].ToString() == "C" || analogRow["MEASURAND_TYPE_CODE"].ToString() == "M")) && skipanalog)
                {
                    Logger.Log("AnalogSkippedPoints", LoggerLevel.INFO, $"Analog: RTU_IRN is empty. ExID: {analogRow["EXTERNAL_IDENTITY"].ToString()}\t ");
                    continue;
                }
                //skip the points with IOA is empty
                if(string.IsNullOrEmpty(analogRow["PXINT1"].ToString()) && (analogRow["MEASURAND_TYPE_CODE"].ToString() =="A" || analogRow["MEASURAND_TYPE_CODE"].ToString() == "D") && skipanalog)
                {
                    Logger.Log("AnalogSkippedPoints", LoggerLevel.INFO, $"Analog: IOA is empty. ExID: {analogRow["EXTERNAL_IDENTITY"].ToString()}\t ");
                    continue ;
                }
                string splitRTU = "";
                //skip the points belonging to trianagle gateway Stations
                //if (new List<string> { "222452251", "1798949251", "1795852251", "2042422251", "2073195251", "2089811251", "1995038251", "2016324251", "2016325251", "1992663251", "2009856251", "2026310251", "1972835251", "2076307251", "2020391251", "2043217251" }.Contains(analogRow["RTU_IRN"].ToString()))
                                       
                //if (new List<string> { "138093201", "206805261", "1798949251", "1795852251", "1972835251", "1992663251", "1995038251", "2009856251", "2016324251", "2016325251", "2020391251", "2026310251", "2042422251", "2043217251", "2076307251", "222452251", "2073195251", "2089811251" }.Contains(analogRow["RTU_IRN"].ToString()))
                if (new List<string> { "138093201","222452251","1798949251","2089811251","1972835251","1995038251","2016324251","2016325251","1795852251","2009856251","2073195251","1992663251","2026310251","206805261","2020391251","2043217251","2042422251","2076307251" }.Contains(analogRow["RTU_IRN"].ToString()))
                {
                    Logger.Log("AnalogSkippedPoints", LoggerLevel.INFO, $"Analog:Belongs to Triangle Gateway RTU. ExID: {analogRow["EXTERNAL_IDENTITY"].ToString()}\t ");
                    continue;
                }
                int pStation = string.IsNullOrEmpty(analogRow["RTU_IRN"].ToString()) ? 5000: EdpespScadaExtensions.GetpStation(analogRow["RTU_IRN"].ToString(), this._stationDict);
                if (analogRow["IDENTIFICATION_TEXT"].ToString().ToUpper().StartsWith("AAREY220  33KV") && analogRow["RTU_IRN"].ToString() == "138096201")
                {
                    splitRTU = "400001";
                    //pStation = EdpespScadaExtensions.GetpStation(splitRTU, this._stationDict);
                }
                if (analogRow["IDENTIFICATION_TEXT"].ToString().ToUpper().StartsWith("VERSO220  33KV") && analogRow["RTU_IRN"].ToString() == "138097201")
                {
                    splitRTU = "400002";
                    //pStation = EdpespScadaExtensions.GetpStation(splitRTU, this._stationDict);
                }
                if (pStation.Equals(-1))
                {
                    Logger.Log("AnalogSkippedPoints", LoggerLevel.INFO, $"Analog: pStation is empty. ExID: {analogRow["EXTERNAL_IDENTITY"].ToString()}\t ");
                    continue;
                }
                //AOR_GROUP =EdpespScadaExtensions.GetpAorGroup(analogRow["SUBSYSTEM_IRN"].ToString(), this._aorDict);

                AOR_GROUP = 1;
                
                // Check points in display to know which to skip.
                //SA:20111124
                //if (EdpespScadaExtensions.ToSkip(this._parser.StationsToSkip, this._parser.PointsFromDisplay, analogRow["STATION_IRN"].ToString(), analogRow["EXTERNAL_IDENTITY"].ToString()))
                //{
                //    continue;
                //}
                // Check external identity to and skip those containing PROTT
                if (analogRow["EXTERNAL_IDENTITY"].ToString().Contains("PROTT"))
                {
                    Logger.Log("AnalogSkippedPoints", LoggerLevel.INFO, $"Analog: EXId has Prott in it. ExID: {analogRow["EXTERNAL_IDENTITY"].ToString()}\t ");
                    continue;
                }
                // Check if IDENTIFICATION_TEXT contains Carripont
                if (analogRow["IDENTIFICATION_TEXT"].ToString().Contains("Carripont"))
                {
                    Logger.Log("AnalogSkippedPoints", LoggerLevel.INFO, $"Analog: IdentificationText has carriport in it. Iden: {analogRow["EXTERNAL_IDENTITY"].ToString()}\t ");
                    continue;
                }
                // Check if RTU_IRN is non empty, if so: check to skip.
                if (!string.IsNullOrEmpty(analogRow["RTU_IRN"].ToString()) && EdpespScadaExtensions.SkipRtuPoint(this._parser.RtuTbl, analogRow["RTU_IRN"].ToString()))
                {
                    Logger.Log("AnalogSkippedPoints", LoggerLevel.INFO, $"Analog: Rtu_IRN match not found in RTU table. Iden: {analogRow["EXTERNAL_IDENTITY"].ToString()}\t ");

                    continue;
                }
                // Check if STATION_IRN equals 84525201 and RTU_TYPE is ICCP, if so skip.
                if (analogRow["STATION_IRN"].ToString().Equals("84525201") && analogRow["RTU_TYPE_ASC"].ToString().Equals("ICCP"))
                {
                    Logger.Log("AnalogSkippedPoints", LoggerLevel.INFO, $"Analog: station_irn is84525201 and is ICCP type . Iden: {analogRow["EXTERNAL_IDENTITY"].ToString()}\t ");

                    continue;
                }
                int type = GetType(analogRow);

                bool ICCPcheck = false;
                if (!skipanalog)//BD new file update 230428
                {
                    if (analogrow2["Type_Updated"].ToString() == "Calc")
                        type = (int)AnalogType.C_ANLG;
                    else if (analogrow2["Type_Updated"].ToString() == "ICCP")
                    {
                        ICCPcheck = true;
                       // Logger.Log("AnalogSkippedPoints", LoggerLevel.INFO, $"Analog:  ICCP type . Iden: {analogRow["EXTERNAL_IDENTITY"].ToString()}\t ");

                        //continue;
                        //pStation = EdpespScadaExtensions.GetpStation("ICCP_Stat", this._stationDict);

                    }

                }
                // Check if it needs a duplicate points.
                //SA:20111124
                //bool needsDuplicate = EdpespScadaExtensions.CheckForDuplicate(analogRow["EXTERNAL_IDENTITY"].ToString(), this._parser.IddValTbl);

                // Set Record
                analogObj.CurrentRecordNo = ++analogRec;
                SetpAor(analogObj, pStation);
                // Set Type and Name
                
                
                
                if (type.Equals((int)AnalogType.C_ANLG))
                {
                    pStation = EdpespScadaExtensions.GetpStation("System_Calc", this._stationDict);
                    analogObj.SetValue("pAORGroup", 9);
                }
                analogObj.SetValue("Type", type);
                //string name = "";
                //SA:20111124
                string name = EdpespScadaExtensions.GetName(analogRow["IDENTIFICATION_TEXT"].ToString(), this._parser.StationAbbrTbl);
                
                //string[] words = name.Split(' ');
               // voltageObj.SetValue("");
                if (type.Equals((int)AnalogType.T_ANLG))
                {
                    name = EdpespScadaExtensions.GetpDeviceInstance(name, analogRow["RTU_TYPE_ASC"].ToString().Equals("ICCP"), out int pDeviceInstance);
                    this._deviceRecDict.Add(analogRow["IDENTIFICATION_TEXT"].ToString(), pDeviceInstance);
                }
                //SA:20111124
                //if (needsDuplicate) name += "_CECOEL";
                name = analogRow["Full_Name"].ToString();// BD: To include long names
                analogObj.SetValue("Name", name);
                if(name == "11kV   15451                   Ir")
                {
                    int t = 0;
                }
                // Set pStation and pAORGroup and Archive Group
                analogObj.SetValue("pStation", pStation);
                string pconfigaor = analogObj.GetValue("pAORGroup",0);
                analogObj.SetValue("pConfiguredAORGroup", pconfigaor);
                //EdpespScadaExtensions.SetArchiveGroup(analogObj, ScadaType.ANALOG, analogRow["HIS_TYPE_IRN"].ToString());
                EdpespScadaExtensions.SetArchiveGroup2(analogObj, ScadaType.ANALOG, analogRow["HIS_TYPE_IRN"].ToString()); //BD: Updated as per v3 mapping file
                analogObj.SetValue("Archive_group", 192); // 10000000 || 01000000 == 11000000 (8th and 7th bit on)
                //analogObj.SetValue("Archive_group", 8, 1);
                // Set Scada Key
                int firstTwo = type;
                if (analogRow["RTU_TYPE_ASC"].ToString().Equals("ICCP"))
                {
                    firstTwo = 13;
                }
                //Set pEquip
                SetpEquip(analogObj, analogRow["EXTERNAL_IDENTITY"].ToString(), analogRow["IDENTIFICATION_TEXT"].ToString(), analogRow["RTU_IRN"].ToString(), this._switchDict,this._transEquipGorai,
                   this._transEquipAarey, this._transEquipChemb, this._transEquipSaki, this._transEquipGhod, this._transEquipVers );
                
                //if endwswith {
                // Set pALARM_GROUP, pUnit, and pScale
                string AlarmName = "";
                DataRow AlarmRow = this._parser.AlarmMeasurandTbl.GetInfo(new object[] { analogRow["IRN"].ToString() });
                if(AlarmRow != null)
                {
                   AlarmName = AlarmRow["OSI_Alarm_Group"].ToString();
                }
                EdpespScadaExtensions.SetpAlarmGroup(analogObj, AlarmName, this._alarmsDict, "ANALOG", analogRow["IRN"].ToString(), name);
                //EdpespScadaExtensions.SetpAlarmGroup(analogObj, analogRow["ALARM_GROUP_MEAS_IRN"].ToString(), this._alarmsDict);
                EdpespScadaExtensions.SetpUnit(analogObj, analogRow, this._unitDict);
                if (!string.IsNullOrEmpty(analogRow["RTU_IRN"].ToString()) && !analogRow["STATION_IRN"].ToString().Equals("84525201") 
                        && !analogRow["STATION_IRN"].ToString().Equals("115926201"))  // These Station IRN's are assigned pScale 1, so skip here.
                {
                   //SetpScale(analogObj, analogRow);
                   var sensorMinimum = analogRow["MIN_VALUE"].ToDouble();
                   var sensorMaximum = analogRow["MAX_VALUE"].ToDouble();
                    //telemetryMinimum = sensorMinimum < 0 ? telemetryMinimum : 0;
                    //telemetryMaximum - telemetryMinimum
                    var calcScale = (sensorMaximum - sensorMinimum) / (telemetryMaximum - telemetryMinimum); //65535
                    var offset = 0;// sensorMaximum - (calcScale * telemetryMaximum);
                    if((analogRow["Full_Name"].ToString() =="33kV   33211       7UT61       Ph L2 I on HV"))
                    {
                        int ttt = 0;
                    }
                    //For below given values use multiplying factor as 1.
                    //50000, 999999999, 1000000000, 9999999999, 10000000000

                    if (sensorMaximum == 50000 || sensorMaximum == 999999999 || sensorMaximum == 1000000000 || sensorMaximum == 9999999999 || sensorMaximum == 10000000000)
                    {
                        calcScale = 1;
                    }
                    int pScale = GetScaleRecord(calcScale, offset, calcScale.ToString());
                    //analogObj.SetValue("pScale", pScale);//commneted for now: //20220119
                    analogObj.SetValue("pScale", pSCALE);
                }
                else
                {
                    analogObj.SetValue("pScale", pSCALE);
                }
                
                //SetNominalLimits(analogObj, analogRow);

                //Limits logic as given on 20221206
                string area = stationObj.GetValue("pParent", pStation, 0);
                string areaname = "";
                if (area == "1") areaname = "CSS";
                else if (area == "2") areaname = "DSS";
                else if (area == "3") areaname = "EHV";
                if (analogRow["Full_Name"].ToString() == "11KV   14625                   Iby" || analogRow["Full_Name"].ToString() == "11KV   29206                   Iy" || analogRow["Full_Name"].ToString() == "11KV   32872                   Iy")
                {
                    int tt = 0;
                }
                SetNominalLimits2(analogObj, analogRow, areaname);
                analogObj.SetValue("RawCountFormat", RAW_COUNT_FORMAT);

                // Check for Primary and Secondary
                //SA:20111124
                //if (analogRow["PSSFLAG"].ToString().Equals("P") && !needsDuplicate)
                //{
                //    primarySourceDict.Add(analogRow["IRN"].ToString(), analogRec);
                //}
                //else if (analogRow["PSSFLAG"].ToString().Equals("S") && !needsDuplicate)
                //{
                //    secondarySourceDict.Add(analogRec, analogRow["MEA_PAIR_IRN"].ToString());
                //}

                // Add to protocol count and set pRtu
                int pRtu = EdpespScadaExtensions.FindpRtu(analogRow, this._rtuDataDict);
                //analogObj.SetValue("pRTU", pRtu);
                EdpespScadaExtensions.I104PointTypes fepPointType = EdpespScadaExtensions.I104PointTypes.None;
                int address = 0;
                string ioa = analogRow["PXINT1"].ToString();
                if (!pRtu.Equals(-1) && !string.IsNullOrEmpty(ioa) && !ICCPcheck)
                {
                    fepPointType = EdpespScadaExtensions.I104PointTypes.MEAS_VALUE;
                    //address = EdpespScadaExtensions.GetNextRtuDefnAddress(pRtu, fepPointType);
                    //int index = this._fepDb.AddToRtuCount(pRtu, address, false, (int)fepPointType, true);
                    //this._scadaToFepXref.AddRecordSetValues(new string[] { "ANALOG", analogRec.ToString(), pRtu.ToString(), ((int)fepPointType).ToString(), key, ioa, "False" });
                    analogObj.SetValue("pRTU", pRtu);
                }
                //analogObj.SetValue("State", 0, 1);
                //Key creation
                int mid = pStation+5000;
                if(!(type.Equals((int)AnalogType.C_ANLG) || type.Equals((int)AnalogType.M_ANLG)) && !pRtu.Equals(-1)) mid = pRtu;
                string key = EdpespScadaExtensions.SetScadaKey(analogObj, firstTwo, mid, this._scadaDb, ScadaType.ANALOG,
                    this._parser.ScadaKeyTbl, this._parser.LockedKeys, analogRow["EXTERNAL_IDENTITY"].ToString(), false);
                if (string.IsNullOrEmpty(key))
                {
                    Logger.Log("EMPTY KEY", LoggerLevel.INFO, $"ANALOG: Key was empty. Name: {name}\t pStation: {pStation}\t Type: {type}");
                }
                if (!pRtu.Equals(-1) && !string.IsNullOrEmpty(ioa) && !ICCPcheck)
                {
                    fepPointType = EdpespScadaExtensions.I104PointTypes.MEAS_VALUE;
                    address = EdpespScadaExtensions.GetNextRtuDefnAddress(pRtu, fepPointType);
                    analogObj.SetValue("pPoint", address);
                    analogObj.SetValue("AppID", 2);
                    analogObj.SetValue("SourceType", 3);
                    this._scadaToFepXref.AddRecordSetValues(new string[] { "ANALOG", analogRec.ToString(), pRtu.ToString(), ((int)fepPointType).ToString(), key, ioa, "False", address.Equals(0) ? string.Empty : address.ToString(), analogRow["EXTERNAL_IDENTITY"].ToString() });
                    //analogObj.SetValue("pRTU", pRtu);
                }
                if (key == "1400P004" || key == "1400A001")
                {
                    int aaa = 0;
                }
                // Add to XREF
                string[] scadaXrefFields = { "ANALOG", analogRow["IDENTIFICATION_TEXT"].ToString(), key, pStation.ToString(), analogRec.ToString(),
                    analogRow["EXTERNAL_IDENTITY"].ToString(), pRtu.Equals(-1)? string.Empty : pRtu.ToString(),
                        ((int)fepPointType).Equals(0)? string.Empty : ((int)fepPointType).ToString(), address.Equals(0)? string.Empty : address.ToString(), ioa,
                            analogRow["IRN"].ToString(),"FALSE", Regex.Replace(analogRow["EXTERNAL_IDENTITY"].ToString(), @"\s+", ""), type.ToString()};
                this._scadaXref.AddRecordSetValues(scadaXrefFields);

                // Duplicate if needed
                //SA:20111124
                //    if (needsDuplicate)
                //    {
                //        ++analogRec;
                //        if (analogObj.CopyRecord(analogObj.CurrentRecordNo, analogRec))
                //        {
                //            DbObject analogCopy = this._scadaDb.GetDbObject("ANALOG");
                //            analogCopy.CurrentRecordNo = analogRec;

                //            // Change name and assign new key
                //            string newName = name.Replace("_CECOEL", "_CECORE");
                //            analogCopy.SetValue("Name", newName.Trim());

                //            string copyKey = EdpespScadaExtensions.SetScadaKey(analogCopy, firstTwo, pStation, this._scadaDb, ScadaType.ANALOG,
                //                this._parser.ScadaKeyTbl, this._parser.LockedKeys, analogRow["EXTERNAL_IDENTITY"].ToString() + " - Copy");
                //            if (string.IsNullOrEmpty(copyKey))
                //            {
                //                Logger.Log("EMPTY KEY", LoggerLevel.INFO, $"STATUS: Key was empty. Name: {newName}\t pStation: {pStation}\t Type: {type}");
                //            }

                //            // Add to XREF
                //            string[] copyXrefFields = { "ANALOG", analogRow["IDENTIFICATION_TEXT"].ToString() + " - Copy", copyKey, pStation.ToString(), analogRec.ToString(),
                //                analogRow["EXTERNAL_IDENTITY"].ToString() + " - Copy", pRtu.Equals(-1)? string.Empty : pRtu.ToString(),
                //                    ((int)fepPointType).Equals(0)? string.Empty : ((int)fepPointType).ToString(), address.Equals(0)? string.Empty : address.ToString(), ioa,
                //                        analogRow["IRN"].ToString() + "_Copy"};
                //            this._scadaXref.AddRecordSetValues(copyXrefFields);

                //            // Set original analog's value
                //            analogObj.CurrentRecordNo--;
                //            analogObj.SetValue("AltDataKey", copyKey);
                //        }
                //        else
                //        {
                //            --analogRec;
                //        }

                //    }
                }

                AddAltDataKeys();

            Logger.CloseXMLLog();
        }
        /// <summary>
        /// Helper function to get get scale.
        /// </summary>
        /// <param name="analogRow">Current DataRow row.</param>
        /// <returns></returns>
        private int GetScaleRecord(double scale, double offset, string prefix = "", string separator = "")
        {
            DbObject scaleObj = this._scadaDb.GetDbObject("SCALE");

            string name = $"Scl: {scale} Off: {offset}";

            if (_scaleDict.ContainsKey(name))
            {
                return _scaleDict[name];
            }
            else
            {
                //int scaleRec = _scaleMapping.Count + 1;
                int scaleRec = scaleObj.NextAvailableRecord;
                scaleObj.CurrentRecordNo = scaleRec;

                scaleObj.SetValue("Name", name);
                scaleObj.SetValue("Scale", scale);
                scaleObj.SetValue("Offset", offset);

                _scaleDict.Add(name, scaleRec);

                return scaleRec;
            }
        }

        /// <summary>
        /// Helper function to get get Type.
        /// </summary>
        /// <param name="analogRow">Current DataRow row.</param>
        /// <returns></returns>
        public int GetType(DataRow analogRow)
        {
            string measurandTypeCode = analogRow["MEASURAND_TYPE_CODE"].ToString();
            string rtuTypeAsc = analogRow["RTU_TYPE_ASC"].ToString();

            switch (measurandTypeCode)
            {
                case "A":
                case "D":
                    return (int)AnalogType.T_ANLG;
                case "C":
                    if (rtuTypeAsc.Equals("CAL"))
                    {
                        return (int)AnalogType.C_ANLG;
                    }
                    else
                    {
                        return (int)AnalogType.T_ANLG;
                    }
                case "M":
                    return (int)AnalogType.M_ANLG;
                default:
                    Logger.Log("UHANDLED MEASURAND TYPE CODE", LoggerLevel.INFO, $"Provided Measurand Type Code unmapped: {measurandTypeCode}\tSetting Type to M_ANLG");
                    return (int)AnalogType.M_ANLG;
            }
        }
        private void SetpEquip(DbObject obj, string exid,string eid,string tid, Dictionary<string, int> switchdict, Dictionary<string, string> transEquipGorai,
            Dictionary<string, string> transEquipAarey, Dictionary<string, string> transEquipChemb, Dictionary<string, string> transEquipSaki, Dictionary<string, string> transEquipGhod, 
            Dictionary<string, string> transEquipVers)
        {
            if(exid == "ANIK  11 32862 KVAR"|| exid == "VPARLE11 11429 IB")
            {
                int t = 0;
            }
            DataRow switchrow = null;
            string[] exidList = exid.Split(' ');
            string exidupdated = "";
            if (exidList.Length == 5) exidupdated = exidList[0] + " " + exidList[1]+ " " + exidList[2] + " " + exidList[3];
            if (exidList.Length == 4) exidupdated = exidList[0] + " " + exidList[1] + " " + exidList[2];
            if (exidList.Length == 3) exidupdated = exidList[0] + " " + exidList[1] ;
            // Check RTU_Irn missing points
            switchrow = this._parser.SwitchTbl.GetInfo(new[] { exidupdated });
            if (switchrow != null )// TryGetRow(new[] { exidupdated }, out switchrow))
            {
                string name = switchrow["DESCRIPTIVE_LOC"].ToString() + "_" + switchrow["TAGNAM"].ToString();
                if (switchdict.ContainsKey(name))
                {
                    obj.SetValue("pEQUIP", switchdict[name]);
                }
            }
            else
            {
                if (exidList.Length == 4) exidupdated = exidList[0]+ exidList[1] + " " + exidList[2];
                switchrow = this._parser.SwitchTbl.GetInfo(new[] { exidupdated });
                if (switchrow != null)// TryGetRow(new[] { exidupdated }, out switchrow))
                {
                    string name = switchrow["DESCRIPTIVE_LOC"].ToString() + "_" + switchrow["TAGNAM"].ToString();
                    if (switchdict.ContainsKey(name))
                    {
                        obj.SetValue("pEQUIP", switchdict[name]);
                    }
                }
            }


            //if (eid.Contains("7SJ") || eid.Contains("Fault"))
            //{

            //    obj.SetValue("pEQUIP", 0);
            //}
            //eid.EndsWith("Tap Position") ||
            if (eid.EndsWith("WTI") || eid.EndsWith("OTI") ||  eid.EndsWith("Tap Change") ||
               eid.EndsWith(" TAPPN") || eid.EndsWith("Tap Change Count")||eid.EndsWith("Tap Position"))
            {
                if (eid.Contains("MVA"))
                {
                    var newid = eid.Split('-');  //BKC       20MVA-T1   OTI
                    string mvaValue = newid[0]; //find a way to extract int part from string 
                    string ActualmvaValue = Regex.Replace(mvaValue, @"\D", "");
                    string TValue = newid[1]; //find a way to extract int part from string  
                    string ActualTValue = Regex.Replace(TValue, @"\D", "");
                    string equip = "TRAFO BREAKER_" + ActualmvaValue + "MVA" + ActualTValue;//CHECK 

                    if (switchdict.ContainsKey(equip))
                    {
                        obj.SetValue("pEQUIP", switchdict[equip]);
                    }
                }
                
          
            }

            if (tid == "1431150241" && eid.Contains("SW"))
            {
                string eidlist = eid.Replace(" ", "");

                string eidList1 = eidlist.Substring(14, 4);

                if (eidList1.Contains("SW"))
                {
                    if (transEquipGorai.ContainsKey(eidList1))
                    {
                        string newID = transEquipGorai[eidList1];

                        if (switchdict.ContainsKey(newID))
                        {
                            obj.SetValue("pEQUIP", switchdict[newID]);
                        }
                    }
                }
            }
            if (tid == "138096201" && eid.Contains("SW"))
            {
                string eidlist = eid.Replace(" ", "");

                string eidList1 = eidlist.Substring(14, 4);

                if (eidList1.Contains("SW"))
                {
                    if (transEquipAarey.ContainsKey(eidList1))
                    {
                        string newID = transEquipAarey[eidList1];

                        if (switchdict.ContainsKey(newID))
                        {
                            obj.SetValue("pEQUIP", switchdict[newID]);
                        }
                    }
                }
            }
            if (tid == "138096201" && (eid.Contains("SW100")|| eid.Contains("SW101") || eid.Contains("SW102") || eid.Contains("SW103")
                || eid.Contains("SW104") || eid.Contains("SW105") || eid.Contains("SW106") || eid.Contains("SW107") || eid.Contains("SW108")
                || eid.Contains("SW109") || eid.Contains("SW110") || eid.Contains("SW111") || eid.Contains("SW112")))
            {
                string eidlist = eid.Replace(" ", "");

                string eidList1 = eidlist.Substring(14, 5);

                //if (eidList1.Contains("SW10"))
                //{
                    if (transEquipAarey.ContainsKey(eidList1))
                    {
                        string newID = transEquipAarey[eidList1];

                        if (switchdict.ContainsKey(newID))
                        {
                            obj.SetValue("pEQUIP", switchdict[newID]);
                        }
                    }
                //}
            }
            if (tid == "1837192241" && eid.Contains("SW"))
            {
                string eidlist = eid.Replace(" ", "");

                string eidList1 = eidlist.Substring(13, 4);

                if (eidList1.Contains("SW"))
                {
                    if (transEquipChemb.ContainsKey(eidList1))
                    {
                        string newID = transEquipChemb[eidList1];

                        if (switchdict.ContainsKey(newID))
                        {
                            obj.SetValue("pEQUIP", switchdict[newID]);
                        }
                    }
                }
            }
            if (tid == "2093087241" && eid.Contains("SW"))
            {
                string eidlist = eid.Replace(" ", "");

                string eidList1 = eidlist.Substring(13, 4);

                if (eidList1.Contains("SW"))
                {
                    if (transEquipSaki.ContainsKey(eidList1))
                    {
                        string newID = transEquipSaki[eidList1];

                        if (switchdict.ContainsKey(newID))
                        {
                            obj.SetValue("pEQUIP", switchdict[newID]);
                        }
                    }
                }
            }
            if (tid == "101219201" && eid.Contains("SW"))
            {
                string eidlist = eid.Replace(" ", "");

                string eidList1 = eidlist.Substring(13, 4);

                if (eidList1.Contains("SW"))
                {
                    if (transEquipGhod.ContainsKey(eidList1))
                    {
                        string newID = transEquipGhod[eidList1];

                        if (switchdict.ContainsKey(newID))
                        {
                            obj.SetValue("pEQUIP", switchdict[newID]);
                        }
                    }
                }
            }
            if (tid == "138097201" && eid.Contains("SW"))
            {
                string eidlist = eid.Replace(" ", "");

                string eidList1 = eidlist.Substring(14, 4);

                if (eidList1.Contains("SW"))
                {
                    if (transEquipVers.ContainsKey(eidList1))
                    {
                        string newID = transEquipVers[eidList1];

                        if (switchdict.ContainsKey(newID))
                        {
                            obj.SetValue("pEQUIP", switchdict[newID]);
                        }
                    }
                }
            }
            if (tid == "138097201" && (eid.Contains("SW100") || eid.Contains("SW101")))
            {
                string eidlist = eid.Replace(" ", "");

                string eidList1 = eidlist.Substring(14, 5);

                if (eidList1.Contains("SW10"))
                {
                    if (transEquipVers.ContainsKey(eidList1))
                    {
                        string newID = transEquipVers[eidList1];

                        if (switchdict.ContainsKey(newID))
                        {
                            obj.SetValue("pEQUIP", switchdict[newID]);
                        }
                    }
                }
            }

            if (eid.Contains("MVA-T")&&(eid.StartsWith("Ghod220")|| eid.StartsWith("Aarey220")|| eid.StartsWith("Verso220")|| eid.StartsWith("Gorai220")||eid.Contains(" RTCC Oil Temp")||eid.Contains("RTCC Tap Position")
                || eid.Contains("Tap Position")))
            {
                string[] eidt1t2 = eid.Split(' ');

                string NewEid = eidt1t2[2];
                if(switchdict.ContainsKey(NewEid))
                {
                    obj.SetValue("pEQUIP", switchdict[NewEid]);
                }
            }
            if (eid.Contains("MVA-T") && (eid.StartsWith("Gore220") || eid.StartsWith("Chmb220")|| eid.StartsWith("ABVL220") || eid.StartsWith("Saki220")))
            {
                string[] eidt1t2 = eid.Split(' ');

                string NewEid = eidt1t2[3];
                if (switchdict.ContainsKey(NewEid))
                {
                    obj.SetValue("pEQUIP", switchdict[NewEid]);
                }
            }
            if (eid.Contains("MVA-T") && (eid.StartsWith("Ghod220") &&eid.Contains("Wdg Temp")))
            {
                string[] eidt1t2 = eid.Split(' ');

                string NewEid = eidt1t2[3];
                if (switchdict.ContainsKey(NewEid))
                {
                    obj.SetValue("pEQUIP", switchdict[NewEid]);
                }
            }

            if (eid.Contains("MVA-T") &&  eid.Contains(" RTCC Oil Temp") || eid.Contains("RTCC Tap Position")|| eid.Contains("Tap Position"))
            {
                string[] eidt1t2 = eid.Split(' ');
                if (eid.StartsWith("Ghod220")|| eid.StartsWith("Chmb220")|| eid.StartsWith("ABVL")|| eid.StartsWith("Saki220")|| eid.StartsWith("Gore220"))
                {
                    string NewEid = eidt1t2[3];
                    if (switchdict.ContainsKey(NewEid))
                    {
                        obj.SetValue("pEQUIP", switchdict[NewEid]);
                    }
                }
                if (eid.StartsWith("Aarey220") || eid.StartsWith("Verso220"))
                {
                    string NewEid = eidt1t2[2];
                    if (switchdict.ContainsKey(NewEid))
                    {
                        obj.SetValue("pEQUIP", switchdict[NewEid]);
                    }
                }
            }

            if (eid.Contains("TR1CB")|| eid.Contains("TR2CB")|| eid.Contains("TR3CB")|| eid.Contains("TR4CB")|| eid.Contains("TR5CB"))
            {
                string[] TRequip = eid.Split(' ');
                if (TRequip.Contains("Gorai220")|| TRequip.Contains("Aarey220"))
                {
                    string NewTRA = TRequip[2];
                    string NewTRB = TRequip[5];
                    string NewTRB1 = NewTRB.Replace("CB", "");
                    string TRAB = NewTRA + "_" + NewTRB1;
                    if (switchdict.ContainsKey(TRAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[TRAB]);
                    }
                }
                if (TRequip.Contains("Ghod220"))
                {
                    string NewTRA = TRequip[3];
                    string NewTRB=TRequip[5];
                    string NewTRB1 = NewTRB.Replace("CB","");
                    string TRAB = NewTRA + "_" + NewTRB1;
                    if (switchdict.ContainsKey(TRAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[TRAB]);
                    }
                }
                
                if(TRequip.Contains("Verso220")|| TRequip.Contains("Aarey220"))
                {
                    string VersTRA=TRequip[2];
                    string VersTRB = TRequip[4];
                    string VersTRB1=VersTRB.Replace("CB","");
                    string VersTRAB=VersTRA + "_" + VersTRB1;
                    if (switchdict.ContainsKey(VersTRAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[VersTRAB]);
                    }
                }
                if (TRequip.Contains("Chmb220")|| TRequip.Contains("ABVL220")|| TRequip.Contains("Saki220")|| TRequip.Contains("Gore220"))
                {
                    string AllTRA = TRequip[3];
                    string AllTRB = TRequip[6];
                    string AllTRB1 = AllTRB.Replace("CB","");
                    string AllTRAB = AllTRA + "_" + AllTRB1;
                    if (switchdict.ContainsKey(AllTRAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[AllTRAB]);
                    }
                }


            }

            if (eid.Contains("BUS1") || eid.Contains("BUS2") )
            {
                string[] TRequip1 = eid.Split(' ');

                if(TRequip1.Contains("Ghod220"))
                {
                    string NewBusA= TRequip1[3];
                    string NewBusB = TRequip1[5];
                    string NewBusAB = NewBusA + "_" + NewBusB;
                    if (switchdict.ContainsKey(NewBusAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[NewBusAB]);
                    }

                }
                if (TRequip1.Contains("Aarey220")|| TRequip1.Contains("Verso220"))
                {
                    string AVBusA = TRequip1[2];
                    string AVBusB = TRequip1[4];
                    string AVBusAB = AVBusA + "_" + AVBusB;
                    if (switchdict.ContainsKey(AVBusAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[AVBusAB]);
                    }

                }
                if ((TRequip1.Contains("Verso220") && TRequip1.Contains("GIS"))|| TRequip1.Contains("GH220GIS"))
                {
                    string AVBusA = TRequip1[2];
                    string AVBusB = TRequip1[5];
                    string AVBusAB = AVBusA + "_" + AVBusB;
                    if (switchdict.ContainsKey(AVBusAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[AVBusAB]);
                    }

                }

            }

            if (eid.Contains("220KV  BC")|| eid.Contains("220KV  TBC")||eid.Contains(" 220KV   TBC")||eid.Contains("220KV   BC")||eid.Contains("220KV   TBC")||eid.Contains("220KV   BC")
                )
            {
                string[] TRequip2 = eid.Split(' ');
                
                if (TRequip2.Contains("Gore220")|| TRequip2.Contains("MBVL220")|| TRequip2.Contains("MTBY220") || TRequip2.Contains("ABVL220") || TRequip2.Contains("Chmb220")|| TRequip2.Contains("Saki220"))
                {
                    string GhodBCA = TRequip2[3];
                    string GhodBCB = TRequip2[6];
                    string GhodAB = GhodBCA + "_" + GhodBCB;
                    if (switchdict.ContainsKey(GhodAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[GhodAB]);
                    }
                }
                if (TRequip2.Contains("Gorai220"))
                {
                    string GhodBCA = TRequip2[2];
                    string GhodBCB = TRequip2[5];
                    string GhodAB = GhodBCA + "_" + GhodBCB;
                    if (switchdict.ContainsKey(GhodAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[GhodAB]);
                    }
                }
                if ( TRequip2.Contains("Ghod220")  )
                {
                    string GhodBCA = TRequip2[3];
                    string GhodBCB = TRequip2[5];
                    string GhodAB = GhodBCA + "_" + GhodBCB;
                    if (switchdict.ContainsKey(GhodAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[GhodAB]);
                    }
                }
                if (TRequip2.Contains("Aarey220")|| TRequip2.Contains("Verso220"))
                {
                    string VATbcA = TRequip2[2];
                    string VATbcB = TRequip2[4];
                    string VATbcAB = VATbcA + "_" + VATbcB;
                    if (switchdict.ContainsKey(VATbcAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[VATbcAB]);
                    }
                }
                if ((TRequip2.Contains("Verso220") || TRequip2.Contains("Bus-2Vry")|| TRequip2.Contains("Bus-1Vry") || TRequip2.Contains("NETIMP")|| TRequip2.Contains("MSETCL")) || TRequip2.Contains("GH220GIS"))
                {
                    string VATbcA = TRequip2[2];
                    string VATbcB = TRequip2[5];
                    string VATbcAB = VATbcA + "_" + VATbcB;
                    if (switchdict.ContainsKey(VATbcAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[VATbcAB]);
                    }
                }
                if (TRequip2.Contains("Dahanu"))
                {
                    string DhanA = TRequip2[2];
                    string DhanB = TRequip2[4];
                    string DhanAB = DhanA + "_" + DhanB;
                    if (switchdict.ContainsKey(DhanAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[DhanAB]);
                    }
                }
                if (TRequip2.Contains("Dahanu")&& TRequip2.Contains("TBC"))
                {
                    string DhanA = TRequip2[4];
                    string DhanB = TRequip2[6];
                    string DhanAB = DhanA + "_" + DhanB;
                    if (switchdict.ContainsKey(DhanAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[DhanAB]);
                    }
                }

            }
            if (eid.Contains("GIBD"))
            {
                string[] TRequip3 = eid.Split(' ');
               
                string VerA = TRequip3[2];
                string VerB = TRequip3[5];
                string VerC = TRequip3[9];
                string VerABC= VerA + "_" + VerB+"_"+VerC;
                if (switchdict.ContainsKey(VerABC))
                {
                    obj.SetValue("pEQUIP", switchdict[VerABC]);
                }
            }
            if (eid.Contains("MMRCG"))
            {
                string[] TRequip3 = eid.Split(' ');

                string VerA = TRequip3[2];
                string VerB = TRequip3[5];
                string VerC = TRequip3[7];
                string VerABC = VerA + "_"+"LINE"+"_" + VerB + "_" + VerC;
                if (switchdict.ContainsKey(VerABC))
                {
                    obj.SetValue("pEQUIP", switchdict[VerABC]);
                }
            }

            //if (eid.Contains("220KV  DHN") || eid.Contains("220KV   DHN") || eid.Contains("220KV   GORAI") || eid.Contains("220KV   TVSV") || eid.Contains("220KV  VSV") || eid.Contains(" 220KV   VSV") || eid.Contains("220KV  NEW")
            //   || eid.Contains(" 220KV   TBVL") || eid.Contains("220KV   MMRCTB  TBVL") || eid.Contains("220KV   ARYTB   TBVL") || eid.Contains("220KV   ABVL") || eid.Contains(" 220KV   ARYAB   ABVL") || eid.Contains(" 220KV   MMRCAB  ABVL")
            //        || eid.Contains("220KV   MBVL") || eid.Contains("220KV   SAKI") || eid.Contains("220KV  ARY") || eid.Contains("220KV   ARYG2   GGN") || eid.Contains("220KV   ARYG1   GGN") || eid.Contains("220KV   GGN")
            //        || eid.Contains(" 220KV   RBVL") || eid.Contains("RBVL-MBorivali") || eid.Contains("220KV   MNRL") || eid.Contains("220KV   CMBR") | eid.Contains("220KV   GRI") || eid.Contains("220KV   MTBY")
            //        || eid.Contains(" 220KV   TPC") || eid.Contains(" 220KV   TSKI") || eid.Contains(" 220KV   MBOI") || eid.Contains(" MBoisar") || eid.Contains("Aarey-MBorivali"))
            //{
            //    string[] EquipLine = eid.Split(' ');
            //    string EquipA = EquipLine[2];
            //}

            if (eid.Contains("220KV  DHN") || eid.Contains("220KV   DHN"))
            {
                string[] EquipLineDHA = eid.Split(' ');
                if (eid.StartsWith("Ghod220"))
                {
                    string EquipGoraiA = EquipLineDHA[3];
                    string EquipGoraiB= EquipLineDHA[5];
                    string EquipGoraiAB = EquipGoraiA + "_" + "LINE" + "_" + EquipGoraiB;
                    if (switchdict.ContainsKey(EquipGoraiAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[EquipGoraiAB]);
                    }
                }
                if (eid.StartsWith("Verso220"))
                {
                    string EquipVersA = EquipLineDHA[2];
                    string EquipVersB = EquipLineDHA[4];
                    string EquipVersAB = EquipVersA + "_" + "LINE" + "_" + EquipVersB;
                    if (switchdict.ContainsKey(EquipVersAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[EquipVersAB]);
                    }
                }
            }
            if (eid.Contains("220KV   GORAI")|| eid.Contains("220KV   TVSV"))
            {
                string[] EquipLineGORAI = eid.Split(' ');
                if (eid.StartsWith("Ghod220"))
                {
                    string EquipGoraiGhodA = EquipLineGORAI[3];
                    string EquipGoraiGhodB = EquipLineGORAI[6];
                    string EquipGoraiGhodAB = EquipGoraiGhodA + "_" + "LINE" + "_" + EquipGoraiGhodB;
                    if (switchdict.ContainsKey(EquipGoraiGhodAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[EquipGoraiGhodAB]);
                    }
                }
                if (eid.StartsWith("Verso220"))
                {
                    string EquipGoraiVersA = EquipLineGORAI[2];
                    string EquipGoraiVersB = EquipLineGORAI[5];
                    string EquipGoraiVersAB = EquipGoraiVersA + "_" + "LINE" + "_" + EquipGoraiVersB;
                    if (switchdict.ContainsKey(EquipGoraiVersAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[EquipGoraiVersAB]);
                    }
                }
            }
            if (eid.Contains("220KV  VSV")||eid.Contains("220KV   VSV"))
            {
                string[] EquipLineVSV = eid.Split(' ');
                if (eid.StartsWith("Ghod220"))
                {
                    string EquipVSVGhodA = EquipLineVSV[3];
                    string EquipVSVGhodB = EquipLineVSV[5];
                    string EquipVSVGhodAB = EquipVSVGhodA + "_" + "LINE" + "_" + EquipVSVGhodB;
                    if (switchdict.ContainsKey(EquipVSVGhodAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[EquipVSVGhodAB]);
                    }
                }
                if (eid.StartsWith("Dahanu"))
                {
                    string EquipVSVDahanuA = EquipLineVSV[4];
                    string EquipVSVDahanuB = EquipLineVSV[6];
                    string EquipVSVDahanuAB = EquipVSVDahanuA + "_" + "LINE" + "_" + EquipVSVDahanuB;
                    if (switchdict.ContainsKey(EquipVSVDahanuAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[EquipVSVDahanuAB]);
                    }
                }
                if (eid.StartsWith("Dahanu")&&eid.Contains("REL"))
                {
                    string EquipVSVDahanuA = EquipLineVSV[4];
                    string EquipVSVDahanuB = EquipLineVSV[7];
                    string EquipVSVDahanuAB = EquipVSVDahanuA + "_" + "LINE" + "_" + EquipVSVDahanuB;
                    if (switchdict.ContainsKey(EquipVSVDahanuAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[EquipVSVDahanuAB]);
                    }
                }

                if (eid.StartsWith("Gore220"))
                {
                    string EquipVSVGoreA = EquipLineVSV[3];
                    string EquipVSVGoreB = EquipLineVSV[6];
                    string EquipVSVGoreAB = EquipVSVGoreA + "_" + "LINE" + "_" + EquipVSVGoreB;
                    if (switchdict.ContainsKey(EquipVSVGoreAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[EquipVSVGoreAB]);
                    }
                }
                if (eid.StartsWith("Gorai220"))
                {
                    string EquipVSVGoraiA = EquipLineVSV[2];
                    string EquipVSVGoraiB = EquipLineVSV[5];
                    string EquipVSVGoraiAB = EquipVSVGoraiA + "_" + "LINE" + "_" + EquipVSVGoraiB;
                    if (switchdict.ContainsKey(EquipVSVGoraiAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[EquipVSVGoraiAB]);
                    }
                }
            }
            if (eid.Contains("Reactr"))
            {
                string[] EquipLineNEW = eid.Split(' ');
                if (eid.StartsWith("Gorai220") || eid.StartsWith("Gore220") || eid.StartsWith("Saki220") || eid.StartsWith("ABVL220") || eid.StartsWith("Gore220"))
                {
                    string EquipNEWGhodA = EquipLineNEW[2];
                    string EquipNEWGhodB = EquipLineNEW[5];
                    string EquipNEWGhodAB = EquipNEWGhodA + "_"  + EquipNEWGhodB;
                    if (switchdict.ContainsKey(EquipNEWGhodAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[EquipNEWGhodAB]);
                    }
                }
            }

                if (eid.Contains("220KV  NEW")||eid.Contains("220KV  ARY")||eid.Contains("220KV   ARY")||eid.Contains(" 220KV  NEW"))
            {
                string[] EquipLineNEW = eid.Split(' ');
                if (eid.StartsWith("MBVL220")||eid.StartsWith("Gore220")||eid.StartsWith("Saki220")|| eid.StartsWith("ABVL220")|| eid.StartsWith("Gore220"))
                {
                    string EquipNEWGhodA = EquipLineNEW[3];
                    string EquipNEWGhodB = EquipLineNEW[6];
                    string EquipNEWGhodAB = EquipNEWGhodA + "_" + "LINE" + "_" + EquipNEWGhodB;
                    if (switchdict.ContainsKey(EquipNEWGhodAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[EquipNEWGhodAB]);
                    }
                }
                if (eid.StartsWith("Ghod220"))
                {
                    string EquipNEWGhodA = EquipLineNEW[3];
                    string EquipNEWGhodB = EquipLineNEW[5];
                    string EquipNEWGhodAB = EquipNEWGhodA + "_" + "LINE" + "_" + EquipNEWGhodB;
                    if (switchdict.ContainsKey(EquipNEWGhodAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[EquipNEWGhodAB]);
                    }
                }
                if (eid.StartsWith("Verso220")||eid.StartsWith("BorivliT"))
                {
                    string EquipNEWVersA = EquipLineNEW[2];
                    string EquipNEWVersB = EquipLineNEW[4];
                    string EquipNEWVersAB = EquipNEWVersA + "_" + "LINE" + "_" + EquipNEWVersB;
                    if (switchdict.ContainsKey(EquipNEWVersAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[EquipNEWVersAB]);
                    }
                }
                if (eid.StartsWith("Dahanu"))
                {
                    string EquipNEWDahanA = EquipLineNEW[4];
                    string EquipNEWDahanB = EquipLineNEW[6];
                    string EquipNEWDahanAB = EquipNEWDahanA + "_" + "LINE" + "_" + EquipNEWDahanB;
                    if (switchdict.ContainsKey(EquipNEWDahanAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[EquipNEWDahanAB]);
                    }
                }
            }

            if (eid.Contains("220KV   TBVL") || eid.Contains("220KV   ABVL"))
            {
                string[] EquipLineTBVL = eid.Split(' ');
                if (eid.StartsWith("Aarey220"))
                {
                    string EquipTBVLAareyA = EquipLineTBVL[2];
                    string EquipTBVLAareyB = EquipLineTBVL[5];
                    string EquipTBVLAareyAB = EquipTBVLAareyA + "_" + "LINE" + "_" + EquipTBVLAareyB;
                    if (switchdict.ContainsKey(EquipTBVLAareyAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[EquipTBVLAareyAB]);
                    }
                }
            }
            if (eid.Contains("220KV   MMRCTB  TBVL") || eid.Contains("220KV   MMRCAB  ABVL"))
            {
                string[] EquipLineMMRCTBVL = eid.Split(' ');
                if (eid.StartsWith("Aarey220"))
                {
                    string EquipMMRCTBAareyA = EquipLineMMRCTBVL[2];
                    string EquipMMRCTBAareyB = EquipLineMMRCTBVL[5];
                    string EquipMMRCTBAareyC = EquipLineMMRCTBVL[7];
                    string EquipMMRCTBAB = EquipMMRCTBAareyA + "_" + "LINE" + "_" + EquipMMRCTBAareyB + "_" + EquipMMRCTBAareyC;
                    if (switchdict.ContainsKey(EquipMMRCTBAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[EquipMMRCTBAB]);
                    }
                }
            }
            if (eid.Contains("220KV   ARYTB   TBVL") || eid.Contains("220KV   ARYAB   ABVL") || eid.Contains("220KV   ARYG2   GGN")
                || eid.Contains("220KV   ARYG1   GGN"))
            {
                string[] EquipLineARYTB = eid.Split(' ');
                if (eid.StartsWith("MMR220"))
                {
                    string EquipARYTBAareyA = EquipLineARYTB[4];
                    string EquipARYTBAareyB = EquipLineARYTB[7];
                    string EquipARYTBAareyC = EquipLineARYTB[10];
                    string EquipARYTBAB = EquipARYTBAareyA + "_" + "LINE" + "_" + EquipARYTBAareyB + "_" + EquipARYTBAareyC;
                    if (switchdict.ContainsKey(EquipARYTBAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[EquipARYTBAB]);
                    }
                }
            }

            if (eid.Contains("220KV   MBVL") || eid.Contains("220KV   SAKI")|| eid.Contains("220KV   GGN")|| eid.Contains(" 220KV   RBVL") || eid.Contains(" 220KV   MBOI"))
            {
                string[] EquipLineMBSKI = eid.Split(' ');
                if (eid.StartsWith("Aarey220")|| eid.StartsWith("Gorai220")|| eid.StartsWith("GH220GIS")|| eid.StartsWith("Verso220"))
                {
                    string EquipMBSKIA = EquipLineMBSKI[2];
                    string EquipMBSKIB = EquipLineMBSKI[5];
                    string EquipMBSKIAB = EquipMBSKIA + "_"+"LINE"+"_" + EquipMBSKIB;
                    if (switchdict.ContainsKey(EquipMBSKIAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[EquipMBSKIAB]);
                    }
                }
                if (eid.StartsWith("ABVL220")|| eid.StartsWith("MBVL220"))
                {
                    string EquipABMBA = EquipLineMBSKI[3];
                    string EquipABMBB = EquipLineMBSKI[6];
                    string EquipABMBAB = EquipABMBA + "_" + "LINE" + "_" + EquipABMBB;
                    if (switchdict.ContainsKey(EquipABMBAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[EquipABMBAB]);
                    }
                }
            }

            if (eid.Contains("220KV   MNRL") || eid.Contains("220KV   CMBR") || eid.Contains("220KV   GRI") || eid.Contains("220KV   MTBY") || eid.Contains(" 220KV   TPC") || eid.Contains(" 220KV   TSKI"))
            {
                string[] EquipLineMBSKI = eid.Split(' ');
                if (eid.StartsWith("Chmb220") || eid.StartsWith("MTBY220") || eid.StartsWith("GH220GIS") || eid.StartsWith("Verso220")||
                    eid.StartsWith("ABVL220") || eid.StartsWith("MBVL220")|| eid.StartsWith("Saki220"))
                {
                    string EquipMNRLA = EquipLineMBSKI[3];
                    string EquipMNRLB = EquipLineMBSKI[6];
                    string EquipMNRLAB = EquipMNRLA + "_" + "LINE" + "_" + EquipMNRLB;
                    if (switchdict.ContainsKey(EquipMNRLAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[EquipMNRLAB]);
                    }
                }
              
            }

            if(eid.Contains("RBVL-MBorivali") || eid.Contains("Aarey-MBorivali"))
            {
                string[] EquipLineRBVMBori = eid.Split(' ');
                if(eid.StartsWith("Aarey220"))
                {
                    string EquipRBVA = EquipLineRBVMBori[2];
                    string EquipRBVB = EquipLineRBVMBori[11];
                    string EquipRBVAB = EquipRBVA + "_" + "LINE" + "_" + EquipRBVB;
                    if (switchdict.ContainsKey(EquipRBVAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[EquipRBVAB]);
                    }
                }
            }
            if (eid.Contains(" MBoisar"))
            {
                string[] EquipLineMBoi = eid.Split(' ');
                if (eid.StartsWith("Aarey220"))
                {
                    string EquipMBoiA = EquipLineMBoi[2];
                    string EquipMBoiB = EquipLineMBoi[12];
                    string EquipMBoiAB = EquipMBoiA + "_" + "LINE" + "_" + EquipMBoiB;
                    if (switchdict.ContainsKey(EquipMBoiAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[EquipMBoiAB]);
                    }
                }
            }
            if(eid.Contains(" 220KV   BOISR")|| eid.Contains("220KV  GT")|| eid.Contains("220KV  GHD")|| eid.Contains(" 220KV  ST")||eid.Contains("220KV   GHD"))
            {
                string[] EquipLineBois = eid.Split(' ');

                if (eid.StartsWith("Dahanu")&&(eid.Contains("GT")|| eid.Contains("ST")))
                {
                    string EquipBoisrA = EquipLineBois[4];
                    string EquipBoisrB = EquipLineBois[6];
                    string AllTRB1 = EquipBoisrB.Replace("CB", "");
                    string EquipBoisrAB = EquipBoisrA + "_"  + AllTRB1;
                    if (switchdict.ContainsKey(EquipBoisrAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[EquipBoisrAB]);
                    }
                }
                if (eid.StartsWith("Verso220") && (eid.Contains("GHD")))
                {
                    string EquipBoisrA = EquipLineBois[2];
                    string EquipBoisrB = EquipLineBois[4];
                    string EquipBoisrAB = EquipBoisrA + "_" + "LINE" + "_" + EquipBoisrB;
                    if (switchdict.ContainsKey(EquipBoisrAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[EquipBoisrAB]);
                    }
                }
                if (eid.StartsWith("Dahanu") &&  (eid.Contains("GHD")))
                {
                    string EquipBoisrA = EquipLineBois[4];
                    string EquipBoisrB = EquipLineBois[6];
                    string AllTRB1 = EquipBoisrB.Replace("CB", "");
                    string EquipBoisrAB = EquipBoisrA + "_" + "LINE" + "_" + AllTRB1;
                    if (switchdict.ContainsKey(EquipBoisrAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[EquipBoisrAB]);
                    }
                }
                if ((eid.StartsWith("Dahanu") && eid.Contains("REL"))||( eid.StartsWith("Dahanu") && (eid.Contains("GHD"))))
                {
                    string EquipBoisrA = EquipLineBois[4];
                    string EquipBoisrB = EquipLineBois[7];
                    string EquipBoisrAB = EquipBoisrA + "_" + "LINE" + "_" + EquipBoisrB;
                    if (switchdict.ContainsKey(EquipBoisrAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[EquipBoisrAB]);
                    }
                }
                if (eid.StartsWith("Gorai220") ||( eid.StartsWith("Verso220")&& eid.Contains("REL")))
                {
                    string EquipVerBoiA = EquipLineBois[2];
                    string EquipVerBoiB = EquipLineBois[5];
                    string EquipVerBoiAB = EquipVerBoiA + "_" + "LINE" + "_" + EquipVerBoiB;
                    if (switchdict.ContainsKey(EquipVerBoiAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[EquipVerBoiAB]);
                    }
                }
                if (eid.StartsWith("Verso220") && (eid.Contains("BOISR") ))
                {
                    string EquipVerBoiA = EquipLineBois[2];
                    string EquipVerBoiB = EquipLineBois[5];
                    string EquipVerBoiAB = EquipVerBoiA + "_" + "LINE" + "_" + EquipVerBoiB;
                    if (switchdict.ContainsKey(EquipVerBoiAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[EquipVerBoiAB]);
                    }
                }
                if (eid.StartsWith("Dahanu") && (eid.Contains("BOISR")))
                {
                    string EquipDahaBoiA = EquipLineBois[4];
                    string EquipDahaBoiB = EquipLineBois[7];
                    string EquipDahaBoiAB = EquipDahaBoiA + "_" + "LINE" + "_" + EquipDahaBoiB;
                    if (switchdict.ContainsKey(EquipDahaBoiAB))
                    {
                        obj.SetValue("pEQUIP", switchdict[EquipDahaBoiAB]);
                    }
                }
            }

            if (eid.Contains("7SJ") || eid.Contains("Fault"))
            {

                obj.SetValue("pEQUIP", 0);
            }



        }


        /// <summary>
        /// Helper function to set nominal limits.
        /// </summary>
        /// <param name="analogObj">Current ANALOG object.</param>
        /// <param name="analogRow">Current DataRow row.</param>
        public void SetNominalLimits(DbObject analogObj, DataRow analogRow)
        {
            // Get MAX and MIN Values
            double MIN_VALUE = GetLimitValue(analogRow["MIN_VALUE"].ToString());
            double MAX_VALUE = GetLimitValue(analogRow["MAX_VALUE"].ToString());

            // Set Reasonability Limits
            analogObj.SetValue("NominalLowLimits", 4, MIN_VALUE);
            analogObj.SetValue("NominalHiLimits", 4, MAX_VALUE);
            if (analogRow["Full_Name"].ToString() == "33kV   33295                   Vyn")
            {
                int t = 0;
            }
            if(analogObj.GetValue("Key", 0)== "031QZ003")
            {
                int t = 0;
            }
            // Find the EXTERNAL_IDENTITY in the limit table and pull values from there
            if (this._parser.LimitsTbl.TryGetRow(new[] {analogRow["EXTERNAL_IDENTITY"].ToString() }, out DataRow limitRow))
            {
                // Get high and low values from input
                //double high1=  limitRow["LIM2HI_CHECK"].ToString() == "0"? 0 : GetLimitValue(limitRow["LIM2HI_VALUE"].ToString());
                //double high2 = limitRow["LIM3HI_CHECK"].ToString() == "0"? 0 : GetLimitValue(limitRow["LIM3HI_VALUE"].ToString());
                //double high3 = limitRow["LIM4HI_CHECK"].ToString() == "0"? 0 : GetLimitValue(limitRow["LIM4HI_VALUE"].ToString());
                //double high4 = limitRow["LIM5HI_CHECK"].ToString() == "0"? 0 : GetLimitValue(limitRow["LIM5HI_VALUE"].ToString());
                //double high5 = limitRow["LIM5HI_CHECK"].ToString() == "0"? 0 : GetLimitValue(limitRow["LIM5HI_VALUE"].ToString());
                //double low1 =  limitRow["LIM1LO_CHECK"].ToString() == "0"? 0 : GetLimitValue(limitRow["LIM1LO_VALUE"].ToString());
                //double low2 =  limitRow["LIM2LO_CHECK"].ToString() == "0"? 0 : GetLimitValue(limitRow["LIM2LO_VALUE"].ToString());
                //double low3 =  limitRow["LIM3LO_CHECK"].ToString() == "0"? 0 : GetLimitValue(limitRow["LIM3LO_VALUE"].ToString());
                //double low4 =  limitRow["LIM4LO_CHECK"].ToString() == "0"? 0 : GetLimitValue(limitRow["LIM4LO_VALUE"].ToString());
                //double low5 =  limitRow["LIM5LO_CHECK"].ToString() == "0"? 0 : GetLimitValue(limitRow["LIM5LO_VALUE"].ToString());

                double high1 = GetLimitValue(limitRow["LIM1HI_VALUE"].ToString());
                double high2 = GetLimitValue(limitRow["LIM2HI_VALUE"].ToString());
                double high3 = GetLimitValue(limitRow["LIM3HI_VALUE"].ToString());
                double high4 = GetLimitValue(limitRow["LIM4HI_VALUE"].ToString());
                double high5 = GetLimitValue(limitRow["LIM5HI_VALUE"].ToString());
                double low1 = GetLimitValue(limitRow["LIM1LO_VALUE"].ToString());
                double low2 = GetLimitValue(limitRow["LIM2LO_VALUE"].ToString());
                double low3 = GetLimitValue(limitRow["LIM3LO_VALUE"].ToString());
                double low4 = GetLimitValue(limitRow["LIM4LO_VALUE"].ToString());
                double low5 = GetLimitValue(limitRow["LIM5LO_VALUE"].ToString());

                // Set NominalPairInactive
                int nomPairInactive = CheckLimits(analogObj, MAX_VALUE, MIN_VALUE, high1, high2, high3, high4, high5, low1, low2, low3, low4, low5);
                analogObj.SetValue("NominalPairInactive", nomPairInactive);
            }
            else
            {
                Logger.Log("LIMITS NOT FOUND", LoggerLevel.INFO, $"EXTERNAL_ID not found in limit table, no limits set: {analogRow["EXTERNAL_IDENTITY"]}");
            }
        }
        /// <summary>
        /// Helper function to set nominal limits.
        /// </summary>
        /// <param name="analogObj">Current ANALOG object.</param>
        /// <param name="analogRow">Current DataRow row.</param>
        public void SetNominalLimits2(DbObject analogObj, DataRow analogRow, string area)
        {
            if(analogRow["Full_Name"].ToString() == "11KV   14625                   Iby" || analogRow["Full_Name"].ToString() == "11KV   29206                   Iy" || analogRow["Full_Name"].ToString() == "11KV   32872                   Iy")
            {
                int tt = 0;
            }
            DataRow limitRow = analogRow;
            NominalPairInactive nominalPairInactive = NominalPairInactive.NONE;
            
            //    // Get high and low values from input
            double high1 = limitRow["LIM2HI_CHECK"].ToString() == "0" ? 0 : GetLimitValue(limitRow["LIM2HI_VALUE"].ToString());
            double high2 = limitRow["LIM3HI_CHECK"].ToString() == "0" ? 0 : GetLimitValue(limitRow["LIM3HI_VALUE"].ToString());
            double high3 = limitRow["LIM4HI_CHECK"].ToString() == "0" ? 0 : GetLimitValue(limitRow["LIM4HI_VALUE"].ToString());
            double high4 = limitRow["LIM5HI_CHECK"].ToString() == "0" ? 0 : GetLimitValue(limitRow["LIM5HI_VALUE"].ToString());
            double high5 = GetLimitValue(analogRow["MAX_VALUE"].ToString()); // limitRow["LIM5HI_CHECK"].ToString() == "0" ? 0 : GetLimitValue(limitRow["LIM5HI_VALUE"].ToString());
            double low1 = limitRow["LIM2LO_CHECK"].ToString() == "0" ? 0 : GetLimitValue(limitRow["LIM2LO_VALUE"].ToString());
            double low2 = limitRow["LIM3LO_CHECK"].ToString() == "0" ? 0 : GetLimitValue(limitRow["LIM3LO_VALUE"].ToString());
            double low3 = limitRow["LIM4LO_CHECK"].ToString() == "0" ? 0 : GetLimitValue(limitRow["LIM4LO_VALUE"].ToString());
            double low4 = limitRow["LIM5LO_CHECK"].ToString() == "0" ? 0 : GetLimitValue(limitRow["LIM5LO_VALUE"].ToString());
            double low5 = GetLimitValue(analogRow["MIN_VALUE"].ToString());// limitRow["LIM5LO_CHECK"].ToString() == "0" ? 0 : GetLimitValue(limitRow["LIM5LO_VALUE"].ToString());
            var lowlimits = new double[] { low1, low2, low3, low4, low5 };
            var highlimits = new double[] { high1,high2,high3,high4,high5 };
            if (area == "DSS")
            {
                string fullname = analogRow["Full_Name"].ToString();
                
                fullname = fullname.Replace("                    ", " ").Replace("                   ", " ").Replace("   ", " ");
                //fullname = fullname.Replace("                    ", " ");
                var name = fullname.Split(' ');
                if (name.Length == 3)
                {
                    
                    if ((name[2].ToString() == "Ir" || name[2].ToString() == "Iy" || name[2].ToString() == "Ib" /*|| name[2].ToString() == "In"*/))
                    {

                        //Incommer:
                        if ((name[0].ToString() == "22kV" || name[0].ToString() == "33kV" || name[0].ToString() == "22KV" || name[0].ToString() == "33KV") ) //Ir, Iy and Ib
                        {
                            //high1 = limitRow["LIM2HI_CHECK"].ToString() == "0" ? 0 : GetLimitValue(limitRow["LIM2HI_VALUE"].ToString());
                            //high2 = limitRow["LIM3HI_CHECK"].ToString() == "0" ? 0 : GetLimitValue(limitRow["LIM3HI_VALUE"].ToString());
                            //high3 = limitRow["LIM4HI_CHECK"].ToString() == "0" ? 0 : GetLimitValue(limitRow["LIM4HI_VALUE"].ToString());
                            //high4 = limitRow["LIM5HI_CHECK"].ToString() == "0" ? 0 : GetLimitValue(limitRow["LIM5HI_VALUE"].ToString());
                            high5 = 900;// limitRow["LIM5HI_CHECK"].ToString() == "0" ? 0 : GetLimitValue(limitRow["LIM5HI_VALUE"].ToString());

                            low2 = -1;
                            low3 = -2;
                            low5 = -10;
                         
                        }
                        //Trafo & Feeder
                        if (name[0].ToString() == "11kV" || name[0].ToString() == "11KV")
                        {
                        //Trafo
                            if (GetLimitValue(analogRow["LIM4HI_VALUE"].ToString()) > 500)
                            {
                                //high1 = limitRow["LIM2HI_CHECK"].ToString() == "0" ? 0 : GetLimitValue(limitRow["LIM2HI_VALUE"].ToString());
                                //high2 = limitRow["LIM3HI_CHECK"].ToString() == "0" ? 0 : GetLimitValue(limitRow["LIM3HI_VALUE"].ToString());
                                //high3 = limitRow["LIM4HI_CHECK"].ToString() == "0" ? 0 : GetLimitValue(limitRow["LIM4HI_VALUE"].ToString());
                                //high4 = limitRow["LIM5HI_CHECK"].ToString() == "0" ? 0 : GetLimitValue(limitRow["LIM5HI_VALUE"].ToString());
                                high5 = 1700;// limitRow["LIM5HI_CHECK"].ToString() == "0" ? 0 : GetLimitValue(limitRow["LIM5HI_VALUE"].ToString());

                                low2 = -1;
                                low3 = -2;
                                low5 = -10;

                            }
                            else //Feeder
                            {
                                //high1 = limitRow["LIM2HI_CHECK"].ToString() == "0" ? 0 : GetLimitValue(limitRow["LIM2HI_VALUE"].ToString());
                                //high2 = limitRow["LIM3HI_CHECK"].ToString() == "0" ? 0 : GetLimitValue(limitRow["LIM3HI_VALUE"].ToString());
                                //high3 = limitRow["LIM4HI_CHECK"].ToString() == "0" ? 0 : GetLimitValue(limitRow["LIM4HI_VALUE"].ToString());
                                //high4 = limitRow["LIM5HI_CHECK"].ToString() == "0" ? 0 : GetLimitValue(limitRow["LIM5HI_VALUE"].ToString());
                                high5 = 500;// limitRow["LIM5HI_CHECK"].ToString() == "0" ? 0 : GetLimitValue(limitRow["LIM5HI_VALUE"].ToString());

                                low2 = -1;
                                low3 = -2;
                                low5 = -10;
                            }
                        }
                        var nominalPairInactive2 = 0x00;
                        if (low2 == high2 && low3 != high3 || high2 >= high3) nominalPairInactive2 = 0x1A;// 26;
                        else if (low3 == high3 && low2 != high2) nominalPairInactive2 = 0x16;// 22;
                        else if (low2 == high2 && low3 == high3) nominalPairInactive2 = 0x1E;// 30;
                        else if (low2 != high2 && low3 != high3) nominalPairInactive2 = 0x12;// 18;
                        //else if( high2 >= high3) nominalPairInactive2 = 0x12;// 18;
                        //var nominalPairInactive2 = 18;// NominalPairInactive.NONE & ~NominalPairInactive.LIMIT2 & ~NominalPairInactive.LIMIT3 & ~NominalPairInactive.REASONABILITY;
                        lowlimits = new double[] { low1, low2, low3, low4, low5 };
                        highlimits = new double[] { high1, high2, high3, high4, high5 };

                        //var nominalPairInactive2 = GetNominalpairinactive(highlimits, lowlimits);
                        analogObj.SetValue("NominalPairInactive", nominalPairInactive2);
                    }
                    else if (name[2].ToString() == "MVA" || name[2].ToString() == "kW" || name[2].ToString() == "kVAR")
                    {
                        double high = 0;
                        double dnr = 0;
                        if (name[0].ToString() == "22kV" || name[0].ToString() == "33kV")
                        {
                            high = 900;
                            
                        }
                        else
                        {
                            if (name[2].ToString() == "MVA")
                            {

                                if (this._parser.LimitsTbl.TryGetRow(new[] { analogRow["Full_Name"].ToString().Replace("MVA", "Iy") }, out DataRow limitRow2))
                                {
                                    if (GetLimitValue(limitRow2["LIM4HI_VALUE"].ToString()) > 500) high = 1700;
                                    else high = 500;
                                    if(!(name[0].ToString().Replace("kV", "").Length > 3)) 
                                         high5 = (Convert.ToDouble(name[0].ToString().Replace("kV","")) * high) / 1000;
                                }

                            }
                            if (name[2].ToString() == "kW")
                            {

                                if (this._parser.LimitsTbl.TryGetRow(new[] { analogRow["Full_Name"].ToString().Replace("kW", "Iy") }, out DataRow limitRow2))
                                {
                                    if (GetLimitValue(limitRow2["LIM4HI_VALUE"].ToString()) > 500) high = 1700;
                                    else high = 500;
                                    if (!(name[0].ToString().Replace("kV", "").Length > 3))
                                        high5 = Convert.ToDouble(name[0].ToString().Replace("kV", "")) * high * 0.95;
                                }

                            }
                            if (name[2].ToString() == "kVAR")
                            {

                                if (this._parser.LimitsTbl.TryGetRow(new[] { analogRow["Full_Name"].ToString().Replace("kVAR", "Iy") }, out DataRow limitRow2))
                                {
                                    if (GetLimitValue(limitRow2["LIM4HI_VALUE"].ToString()) > 500) high = 1700;
                                    else high = 500;
                                    if (!(name[0].ToString().Replace("kV", "").Length > 3))
                                        high5 = Convert.ToDouble(name[0].ToString().Replace("kV", "")) * high * 0.3;
                                }

                            }
                        }
                        low5 = high5 * (-1);
                        //nominalPairInactive = NominalPairInactive.NONE & ~NominalPairInactive.REASONABILITY;
                        //lowlimits = new double[] { low1, low2, low3, low4, low5 };
                        //highlimits = new double[] { high1, high2, high3, high4, high5 };

                        //var nominalPairInactive2 = GetNominalpairinactive(highlimits, lowlimits);
                        //analogObj.SetValue("NominalPairInactive", nominalPairInactive2);
                        var nominalPairInactive2 = 0x1F;// NominalPairInactive.ALL;// 30; ~NominalPairInactive.REASONABILITY;
                        analogObj.SetValue("NominalPairInactive", nominalPairInactive2);
                    }
                    else if (name[2].ToString() == "Vry" || name[2].ToString() == "Vry" || name[2].ToString() == "Vyb" || name[2].ToString() == "Vbr")//Vry, Vry and Vbr Line Voltage
                    {
                        if (name[0].ToString() == "11kV") 
                        {
                            high5 = 18;
                            low5 = -5;
                        }
                        else if(name[0].ToString() == "22kV")
                        {
                            high5 = 28;
                            low5 = -5;
                        }
                        else if (name[0].ToString() == "33kV")
                        {
                            high5 = 40;
                            low5 = -5;
                        }
                        lowlimits = new double[] { low1, low2, low3, low4, low5 };
                        highlimits = new double[] { high1, high2, high3, high4, high5 };

                        var nominalPairInactive2 = GetNominalpairinactive(highlimits, lowlimits);
                        analogObj.SetValue("NominalPairInactive", nominalPairInactive2);
                    }
                    else if (name[2].ToString() == "Vrn" || name[2].ToString() == "Vyn" || name[2].ToString() == "Vbn")//Vrn, Vyn and Vbn Phase Voltage
                    {
                        if (name[0].ToString() == "11kV")
                        {
                            high5 = 12;
                            low5 = -5;
                        }
                        else if (name[0].ToString() == "22kV")
                        {
                            high5 = 18;
                            low5 = -5;
                        }
                        else if (name[0].ToString() == "33kV")
                        {
                            high5 = 24;
                            low5 = -5;
                        }
                        var nominalPairInactive2 = 0x1F;// NominalPairInactive.ALL;// 30; ~NominalPairInactive.REASONABILITY;
                        analogObj.SetValue("NominalPairInactive", nominalPairInactive2);
                    }
                    else if (name[2].ToString() == "OTI" || name[2].ToString() == "WTI")//WTI, OTI
                    {
                        high5 = 200;
                        low5 = -200;
                        lowlimits = new double[] { low1, low2, low3, low4, low5 };
                        highlimits = new double[] { high1, high2, high3, high4, high5 };

                        var nominalPairInactive2 = GetNominalpairinactive(highlimits, lowlimits);
                        analogObj.SetValue("NominalPairInactive", nominalPairInactive2);
                    }
                    else if (name[2].ToString() == "PF" )//PF
                    {
                        high5 = 1.1;
                        low5 = -1.1;
                        var nominalPairInactive2 = 0x1F;//NominalPairInactive.ALL;//30;// ~NominalPairInactive.REASONABILITY;
                        analogObj.SetValue("NominalPairInactive", nominalPairInactive2);
                    }
                    else if(fullname.ToString().EndsWith("Tap Position"))//Tap Position
                {
                    high5 = 20;
                    low5 = -20;
                    var nominalPairInactive2 = 0x1F;//NominalPairInactive.ALL;//30;// ~NominalPairInactive.REASONABILITY;
                        analogObj.SetValue("NominalPairInactive", nominalPairInactive2);
                }
                    else
                    {
                        var nominalPairInactive2 = 0x1F;//NominalPairInactive.ALL;//30;// ~NominalPairInactive.REASONABILITY;
                        analogObj.SetValue("NominalPairInactive", nominalPairInactive2);
                        //return;
                    }
                }
                else
                {
                    var nominalPairInactive2 = 0x1F;//NominalPairInactive.ALL;//30;// ~NominalPairInactive.REASONABILITY;
                    analogObj.SetValue("NominalPairInactive", nominalPairInactive2);
                    //return;
                }
            }
            else if (area == "CSS")
            {
                //Disable all limts
                var nominalPairInactive2 = 0x1F;// NominalPairInactive.ALL;//30;// ~NominalPairInactive.REASONABILITY;
                analogObj.SetValue("NominalPairInactive", nominalPairInactive2);
            }
            else if (area == "EHV")
            {
                high5 = GetLimitValue(analogRow["MAX_VALUE"].ToString());
                low5 = GetLimitValue(analogRow["MIN_VALUE"].ToString());

                lowlimits = new double[] { low1, low2, low3, low4, low5 };
                highlimits = new double[] { high1, high2, high3, high4, high5 };

                var nominalPairInactive2 = GetNominalpairinactive(highlimits, lowlimits);
                analogObj.SetValue("NominalPairInactive", nominalPairInactive2);
            }
            
                // Get MAX and MIN Values
                //double MIN_VALUE = GetLimitValue(analogRow["MIN_VALUE"].ToString());
                //double MAX_VALUE = GetLimitValue(analogRow["MAX_VALUE"].ToString());

                // Set Reasonability Limits
                //analogObj.SetValue("NominalLowLimits", 4, MIN_VALUE);
                //analogObj.SetValue("NominalHiLimits", 4, MAX_VALUE);

                // Find the EXTERNAL_IDENTITY in the limit table and pull values from there
                //if (this._parser.LimitsTbl.TryGetRow(new[] { analogRow["EXTERNAL_IDENTITY"].ToString() }, out DataRow limitRow))
                //{

                //    // Set NominalPairInactive
                //    //int nomPairInactive = CheckLimits(analogObj, MAX_VALUE, MIN_VALUE, high1, high2, high3, high4, high5, low1, low2, low3, low4, low5);
                //    //analogObj.SetValue("NominalPairInactive", nomPairInactive);
                //}
            else
            {
                Logger.Log("LIMITS NOT FOUND", LoggerLevel.INFO, $"EXTERNAL_ID not found in limit table, no limits set: {analogRow["EXTERNAL_IDENTITY"]}");
                var nominalPairInactive2 = 0x1F;//All calc_points under System_calc 3513 station will enter this part
                analogObj.SetValue("NominalPairInactive", nominalPairInactive2);
            }
            

            analogObj.SetValue("NominalHiLimits", 0, highlimits[0]);
            analogObj.SetValue("NominalHiLimits", 1, highlimits[1]);
            analogObj.SetValue("NominalHiLimits", 2, highlimits[2]);
            analogObj.SetValue("NominalHiLimits", 3, highlimits[3]);
            analogObj.SetValue("NominalHiLimits", 4, high5);

            analogObj.SetValue("NominalLowLimits", 0, lowlimits[0]);
            analogObj.SetValue("NominalLowLimits", 1, lowlimits[1]);
            analogObj.SetValue("NominalLowLimits", 2, lowlimits[2]);
            analogObj.SetValue("NominalLowLimits", 3, lowlimits[3]);
            analogObj.SetValue("NominalLowLimits", 4, low5);


        }

        public static dynamic GetNominalpairinactive(double[] NominalHiLimits,double[] NominalLowLimits)
        {
            dynamic convertedVal;
            convertedVal = 1;
            var NominalPairInactive = 0x00;
            if ((NominalHiLimits[0] == 0 && NominalLowLimits[0] == 0 )|| (NominalHiLimits[0] <= NominalLowLimits[0]) || (NominalHiLimits[0] >= NominalHiLimits[4]) || (NominalLowLimits[0] <= NominalLowLimits[4]) || (NominalLowLimits[0] <= NominalLowLimits[1]) || (NominalHiLimits[0] >= NominalHiLimits[1])) NominalPairInactive |= 0x02;
            else NominalPairInactive &= ~0x02;
            if ((NominalHiLimits[1] == 0 && NominalLowLimits[1] == 0) || (NominalHiLimits[1] <= NominalLowLimits[1]) || (NominalHiLimits[1] >= NominalHiLimits[4]) || (NominalLowLimits[1] <= NominalLowLimits[4]) || (NominalLowLimits[1] <= NominalLowLimits[2]) || (NominalHiLimits[1] >= NominalHiLimits[2])) NominalPairInactive |= 0x04;
            else NominalPairInactive &= ~0x04;
            if ((NominalHiLimits[2] == 0 && NominalLowLimits[2] == 0) || (NominalHiLimits[2] <= NominalLowLimits[2]) || (NominalHiLimits[2] >= NominalHiLimits[4]) || (NominalLowLimits[2] <= NominalLowLimits[4]) || (NominalLowLimits[2] <= NominalLowLimits[3]) || (NominalHiLimits[3] >= NominalHiLimits[2])) NominalPairInactive |= 0x08;
            else NominalPairInactive &= ~0x08;
            if ((NominalHiLimits[3] == 0 && NominalLowLimits[3] == 0) || (NominalHiLimits[3] <= NominalLowLimits[3]) || (NominalHiLimits[3] >= NominalHiLimits[4]) || (NominalLowLimits[3] <= NominalLowLimits[4]) ) NominalPairInactive |= 0x10;
            else NominalPairInactive &= ~0x10;
            NominalPairInactive |= 0x01;
            
            return NominalPairInactive;
            return convertedVal;
        }
        /// <summary>
        /// Helper function to parse double value from input.
        /// </summary>
        /// <param name="value">Current high or low limit value to parse.</param>
        /// <returns>Returns douvle value if found, else a 0.</returns>
        public double GetLimitValue(string value)
        {
            // Replace any commas with a period
            if (value.Contains(","))
            {
                value = value.Replace(',', '.');
            }
            // Try to get double value. If not found, return 0
            if (value.TryParseDouble(out double limit))
            {
                decimal tempLimit = Convert.ToDecimal(limit);
                tempLimit = decimal.Round(tempLimit, 2);
                return tempLimit.ToDouble();
            }
            return 0;
        }

        /// <summary>
        /// Helper function to check NominalPairInactive.
        /// </summary>
        /// <param name="analogObj">Current AnalogObj.</param>
        /// <param name="max">Max value.</param>
        /// <param name="min">Min value.</param>
        /// <param name="high1">NominalHiLimit 1</param>
        /// <param name="high2">NominalHiLimit 2</param>
        /// <param name="high3">NominalHiLimit 3</param>
        /// <param name="high4">NominalHiLimit 4</param>
        /// <param name="high5">NominalHiLimit 4</param>
        /// <param name="low1">NominalLowLimit 1</param>
        /// <param name="low2">NominalLowLimit 2</param>
        /// <param name="low3">NominalLowLimit 3</param>
        /// <param name="low4">NominalLowLimit 4</param>
        /// <param name="low5">NominalLowLimit 4</param>
        /// <returns></returns>
        public int CheckLimits(DbObject analogObj, double max, double min, double high1, double high2, double high3, double high4, double high5,
            double low1, double low2, double low3, double low4, double low5)
        {
            NominalPairInactive nominalPairInactive = NominalPairInactive.ALL & ~NominalPairInactive.REASONABILITY;
            bool isHighEqual = false;
            bool isLowEqual = false;

            // Check Highs for equality
            if (max.Equals(high1) && max.Equals(high2) && max.Equals(high3) && max.Equals(high4) && max.Equals(high5))
            {
                analogObj.SetValue("NominalHiLimits", 0, 0);
                analogObj.SetValue("NominalHiLimits", 1, 0);
                analogObj.SetValue("NominalHiLimits", 2, 0);
                analogObj.SetValue("NominalHiLimits", 3, 0);
                isHighEqual = true;
            }
            // Check Lows for equality
            if (min.Equals(low1) && min.Equals(low2) && min.Equals(low3) && min.Equals(low4) && min.Equals(low5))
            {
                analogObj.SetValue("NominalLowLimits", 0, 0);
                analogObj.SetValue("NominalLowLimits", 1, 0);
                analogObj.SetValue("NominalLowLimits", 2, 0);
                analogObj.SetValue("NominalLowLimits", 3, 0);
                isLowEqual = true;
            }
            if (isHighEqual && isLowEqual)
            {
                return (int)nominalPairInactive;
            }

            // Check for high limits
            bool[] highLimits = { false, false, false, false };
            List<double> highValues = CompareLimits(true, new double[] { high1, high2, high3, high4, high5 }, max);
            highValues.Sort();
            // Set high limit to true if being used.
            int highCount = highValues.Count < 5 ? highValues.Count : 4;
            for (int i = 0; i < highCount; i++)
            {
                highLimits[i] = true;
            }

            // Add padded values of the last value in the list, if none are present, use the max
            double lastHigh = highValues.Count > 0 ? highValues[highValues.Count - 1] : max;
            for (int i = highValues.Count; i < 4; i++)
            {
                highValues.Add(lastHigh);
            }

            // Check for low lmits
            bool[] lowLimits = { false, false, false, false };
            List<double> lowValues = CompareLimits(false, new double[] { low1, low2, low3, low4, low5 }, min);
            lowValues.Sort();
            lowValues.Reverse();  // Reverse to have higher values first in the list
            // Set low limit to true if being used.
            int lowCount = lowValues.Count < 5 ? lowValues.Count : 4;
            for (int i = 0; i < lowCount; i++)
            {
                lowLimits[i] = true;
            }

            // Add padded values of the last value in the list, if none are present, use the min
            double lastLow = lowValues.Count > 0 ? lowValues[lowValues.Count - 1] : min;
            for (int i = lowValues.Count; i < 4; i++)
            {
                if (highValues[i] == 0 && lastLow > 0) lastLow = 0;//BD: Error in highlimits
                lowValues.Add(lastLow);
            }

            // Compare pairs and turn on limit for each pair being used. Also set the value
            bool[] comparedActive = CompareLowAndHighLimits(lowLimits, highLimits);
            if (comparedActive[0] && lowValues[0] != highValues[0])
            {
                analogObj.SetValue("NominalLowLimits", 0, lowValues[0]);
                analogObj.SetValue("NominalHiLimits", 0, highValues[0]);
                nominalPairInactive &= ~NominalPairInactive.LIMIT1;
            }
            if (comparedActive[1] && lowValues[1] != highValues[1])
            {
                analogObj.SetValue("NominalLowLimits", 1, lowValues[1]);
                analogObj.SetValue("NominalHiLimits", 1, highValues[1]);
                nominalPairInactive &= ~NominalPairInactive.LIMIT2;
            }
            if (comparedActive[2] && lowValues[2] != highValues[2])
            {
                analogObj.SetValue("NominalLowLimits", 2, lowValues[2]);
                analogObj.SetValue("NominalHiLimits", 2, highValues[2]);
                nominalPairInactive &= ~NominalPairInactive.LIMIT3;
            }
            if (comparedActive[3] && lowValues[3] != highValues[3])
            {
                analogObj.SetValue("NominalLowLimits", 3, lowValues[3]);
                analogObj.SetValue("NominalHiLimits", 3, highValues[3]);
                nominalPairInactive &= ~NominalPairInactive.LIMIT4;
            }

            return (int)nominalPairInactive;
        }


        /// <summary>
        /// Helper function to compare which limits to turn on.
        /// </summary>
        /// <param name="lowLimits">Array of which low limits are active.</param>
        /// <param name="highLimits">Array of which high limits are active.</param>
        /// <returns>Returns which limit pairs are active.</returns>
        public bool[] CompareLowAndHighLimits(bool[] lowLimits, bool[] highLimits)
        {
            // If one low or high have a limit active, the pair is turned on.
            bool[] comparedLimits = { false, false, false, false };
            comparedLimits[0] = lowLimits[0] || highLimits[0];
            comparedLimits[1] = lowLimits[1] || highLimits[1];
            comparedLimits[2] = lowLimits[2] || highLimits[2];
            comparedLimits[3] = lowLimits[3] || highLimits[3];

            return comparedLimits;
        }
        public void SetpAor(DbObject analogObj, int pStation)
        {
            DbObject stationObj = this._scadaDb.GetDbObject("STATION");
            stationObj.CurrentRecordNo = pStation;

            string AORstring = stationObj.GetValue("pAORGroup", 0);
            if (AORstring.TryParseInt(out int pAOR))
            {
                analogObj.SetValue("pAORGroup", pAOR);//BD
                //statusObj.SetValue("pAORGroup", 1);
                //statusObj.SetValue("pConfiguredAORGroup", 1);

            }
            else
            {
                analogObj.SetValue("pAORGroup", 1);
                //statusObj.SetValue("pConfiguredAORGroup", 1);
                Logger.Log("INVALID AORGROUP", LoggerLevel.INFO, $"Provided Aor GROUP was not found for station: {stationObj.GetValue("Name", 0)}\tSetting pAorGroup to 0");
            }
        }
        /// <summary>
        /// Helper function to compare min and max values and determine which are being used.
        /// </summary>
        /// <param name="comparingHighValues">True if comparing high values, false if low values.</param>
        /// <param name="values">Array of current high or low values.</param
        /// <param name="minOrMax">Max value if comparing high. Min value if comparing low.</param>
        /// <returns>Returns list of values being used.</returns>
        public List<double> CompareLimits(bool comparingHighValues, double[] values, double minOrMax)
        {
            List<double> returnList = new List<double>();

            for (int i = 0; i < values.Length; i++)
            {
                // If comparing high values, use <, else use >
                if (comparingHighValues)
                {
                    if (values[i] < minOrMax && !returnList.Contains(values[i]))
                    {
                        returnList.Add(values[i]);
                    }
                }
                else
                {
                    if (values[i] > minOrMax && !returnList.Contains(values[i]))
                    {
                        returnList.Add(values[i]);
                    }
                }
            }
            return returnList;
        }

        /// <summary>
        /// Helper function to assign alt data keys after all analog points are converted.
        /// </summary>
        public void AddAltDataKeys()
        {
            DbObject dataObj = this._scadaDb.GetDbObject("ANALOG");
            foreach (KeyValuePair<int, string> secondaryPair in secondarySourceDict)
            {
                if (primarySourceDict.TryGetValue(secondaryPair.Value, out int primaryRec))
                {
                    dataObj.CurrentRecordNo = secondaryPair.Key;
                    string altDataKey = dataObj.GetValue("Key", 0);
                    dataObj.CurrentRecordNo = primaryRec;
                    dataObj.SetValue("AltDataKey", altDataKey);
                }
                else
                {
                    Logger.Log("PAIR IRN NOT FOUND", LoggerLevel.INFO, $"Pair IRN not found for Analog record:{secondaryPair.Key}");
                }
            }
        }

        /// <summary>
        /// Helper function to set pScale for telemetered Analog points.
        /// </summary>
        /// <param name="analogObj">Current analog object.</param>
        /// <param name="analogRow">Current analog dataRow.</param>
        public void SetpScale(DbObject analogObj, DataRow analogRow)
        {
            string min = analogRow["MIN_VALUE"].ToString().Replace(',', '.');
            string max = analogRow["MAX_VALUE"].ToString().Replace(',', '.');

            if (this._measScales.TryGetValue((min, max), out int pScale))
            {
                analogObj.SetValue("pScale", pScale);
            }
            else
            {
                Logger.Log("UNFOUND SCALE", LoggerLevel.INFO, $"pScale not found for min, max: {min}, {max} \t For object:{analogObj.GetValue("Name", 0)} \t Setting to pScale 1");
                analogObj.SetValue("pScale", 1);
            }
        }
    }
}
