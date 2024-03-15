using System.Collections.Generic;
using System.Data;
using System.Text;
using edpesp_db.Data.EdpespDatabases.EdpespFep;
using OSII.ConversionToolkit;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespScada
{
    class EdpespStation
    {
        private readonly EdpespParser _parser;  // Local reference of the parser
        private readonly SCADA _scadaDb;  // Local reference of the Scada Database
        private readonly Dictionary<string, int> _stationDict;  // Local reference of the station Dict. 
        private readonly Dictionary<string, string> _bayDict;  // Local reference of the Bay Dict. 
        private readonly Dictionary<string, int> _areaDict;  // Local reference of the Area Dict. 
        private readonly Dictionary<string, int> _aorDict;  // Local reference of the AORGROUP Dict. 
        private readonly Dictionary<string, string> _RtuaorDict;  // Local reference of the AORGROUP Dict. 
        private const int MAX_LENGTH = 3;  // Max length of key based on mapping
        private readonly Dictionary<string, string> _stnNameDict;  // Local reference of the AORGROUP Dict. 
        private  Dictionary<string, int> _OldRTURecDict;  // Local reference of the AORGROUP Dict. 
        private List<string> _RTUNotFoundInOld ;
        private List<string> _OldRTUNotFoundInNew;
        // Local reference of the AORGROUP Dict. 
        /// <summary>
        /// Default Constructor.
        /// Assigns local references of important variables.
        /// </summary>
        /// <param name="par">Current Parser.</param>
        /// <param name="scadaDb">Current Scada database.</param>
        /// <param name="stationDict">Station dictionary object.</param>
        public EdpespStation(EdpespParser par, SCADA scadaDb, Dictionary<string, int> stationDict, Dictionary<string, int> AorDict, Dictionary<string, string> StnNameDict, Dictionary<string, int> AreaDict, Dictionary<string, string> RtuIrnDivDict, Dictionary<string, int> OldRtuIrnRecDict, List<string> RTUNotFoundInOld,List<string> OldRTUNotFoundInNew)
        {
            this._parser = par;
            this._scadaDb = scadaDb;
            this._stationDict = stationDict;
            this._aorDict = AorDict;
            this._stnNameDict = StnNameDict;
            this._areaDict = AreaDict;
            this._RtuaorDict = RtuIrnDivDict;
            this._OldRTURecDict = OldRtuIrnRecDict;
            this._RTUNotFoundInOld = RTUNotFoundInOld;
            this._OldRTUNotFoundInNew = OldRTUNotFoundInNew;
            //this._bayDict = BayDict;
        }

        /// <summary>
        /// Converts all station objects.
        /// </summary>
        public void ConvertStation()
        {
            Logger.OpenXMLLog();

            DbObject stationObj = this._scadaDb.GetDbObject("STATION");
            int stationRec = 0;
            int AOR_GROUP = 1;  // Value based on mapping

            foreach (DataRow stationRow in this._parser.StationTbl.Rows)
            {
                
                string fullName = stationRow["STATION_ID_TEXT"].ToString();
                
                //string abbr = FindAbbr(fullName); //SA:20211124 //SA:Cole
                //AOR_GROUP = EdpespScadaExtensions.GetpAorGroup(stationRow["SUBSYSTEM_IRN"].ToString(), this._aorDict);
                if (AOR_GROUP.Equals(-1))
                {
                    AOR_GROUP = 1;
                }
                //AOR_GROUP = FindAor(stationRow["SUBSYSTEM_IRN"]);
                string abbr = stationRow["STATION_ACRONYM"].ToString();
                // Skip empty strings and don't convert.
                if (!string.IsNullOrEmpty(abbr))
                {
                    // Set Record, Name, Key, and pAORGroup
                    //stationObj.CurrentRecordNo = ++stationRec;
                    //stationObj.SetValue("Name", fullName);
                    //stationObj.SetValue("Key", abbr);
                    //stationObj.SetValue("pAORGroup", AOR_GROUP);

                    // Add to station dictionary using Station IRN and Record.
                    //string stationIRN = stationRow["IRN"].ToString();
                    //if (!this._stationDict.ContainsKey(stationIRN))
                    //{
                    //    this._stationDict.Add(stationIRN, stationRec);
                    //    //this._stnNameDict[stationRow["IRN"].ToString()] = fullName;
                    //}
                    
                }
                else
                {
                    // Add to list with -1 to indicate skip
                    //string stationIRN = stationRow["IRN"].ToString();
                    //if (!this._stationDict.ContainsKey(stationIRN))
                    //{
                    //    this._stationDict.Add(stationIRN, -1);
                    //}
                }
                if (!this._stnNameDict.ContainsKey(stationRow["IRN"].ToString()))
                {
                    
                    this._stnNameDict[stationRow["IRN"].ToString()] = fullName;
                }
            }
            int oldRec = 1;
            //foreach (DataRow stationRow in this._parser.RtuTblOld.Rows) //RTURecConstant: BD
            //{
            //    if (stationRow["EXTERNAL_IDENTITY"].ToString().Contains("DMS TRIANGLE GATEWAY")) continue;
            //    this._OldRTURecDict[stationRow["IRN"].ToString()] = oldRec;
            //    oldRec++;

            //}
            

            //foreach (DataRow stationRow in this._parser.RtuTbl.Rows)
            this._parser.RtuTbl.Sort("IRN");
            //foreach (var OldRTU in this._OldRTURecDict) //RTURecConstant: BD
            //{
            //    DataRow stationRow = this._parser.RtuTbl.GetInfo(new object[] { OldRTU.Key });
            //    if (stationRow == null)// skip the points that are present in the new file but that RTU is absent in the old file
            //    {
            //        this._RTUNotFoundInOld.Add(OldRTU.Key);   

            //        continue;
            //    }
            //    string fullName = EdpespFepExtensions.GetNameWithoutRTU(stationRow["IDENTIFICATION_TEXT"].ToString()); //stationRow["EXTERNAL_IDENTITY"].ToString();

            //    //string abbr = FindAbbr(fullName); //SA:20211124 //SA:Cole
            //    if (stationRow["IDENTIFICATION_TEXT"].ToString() == "RTU CAMA_CZ" || stationRow["IRN"].ToString() == "172450201")
            //    {
            //        int t = 0;
            //    }
            //    AOR_GROUP = EdpespScadaExtensions.GetpAorGroup(stationRow["IRN"].ToString(), this._aorDict, this._RtuaorDict );
            //    if (AOR_GROUP.Equals(-1))
            //    {
            //        Logger.Log("AORNotFound", LoggerLevel.INFO, $"STATION: AOR Not found: {stationRow["IRN"].ToString()}\t ");
            //        AOR_GROUP = 1;
            //    }

            //    //AOR_GROUP = FindAor(stationRow["SUBSYSTEM_IRN"]);
            //    string abbr = EdpespFepExtensions.GetNameWithoutRTU(stationRow["IDENTIFICATION_TEXT"].ToString());
            //    //string abbr = stationRow["EXTERNAL_IDENTITY"].ToString();
            //    //"138093201","206805261","1798949251","1795852251","1972835251","1992663251","1995038251","2009856251","2016324251","2016325251","2020391251","2026310251","2042422251","2043217251","2076307251","222452251","2073195251","2089811251"
            //    //if (stationRow["EXTERNAL_IDENTITY"].ToString().Contains("DMS TRIANGLE GATEWAY")) continue; //BD commented 030423
            //    if (new List<string> { "138093201", "206805261", "1798949251", "1795852251", "1972835251", "1992663251", "1995038251", "2009856251", "2016324251", "2016325251", "2020391251", "2026310251", "2042422251", "2043217251", "2076307251", "222452251", "2073195251", "2089811251" }.Contains(stationRow["IRN"].ToString()))
            //    {
            //        Logger.Log("StationSkippedPoints", LoggerLevel.INFO, $"STATION: Belongs to Triangle gateway. ExID: {stationRow["EXTERNAL_IDENTITY"].ToString()}\t ");
            //        continue;

            //    }
            //    int area = EdpespScadaExtensions.GetArea(stationRow["IRN"].ToString(), this._areaDict);
            //    // Skip empty strings and don't convert.
            //    if (!string.IsNullOrEmpty(abbr))
            //    {
            //        // Set Record, Name, Key, and pAORGroup
            //        stationObj.CurrentRecordNo = stationObj.NextAvailableRecord; ++stationRec;
            //        stationObj.SetValue("Name", fullName);
            //        stationObj.SetValue("Key", abbr);
            //        stationObj.SetValue("pAORGroup", AOR_GROUP);
            //        stationObj.SetValue("pParent", area);

            //        // Add to station dictionary using Station IRN and Record.
            //        string stationIRN = stationRow["IRN"].ToString();
            //        if (!this._stationDict.ContainsKey(stationIRN))
            //        {
            //            this._stationDict.Add(stationIRN, stationObj.CurrentRecordNo);
            //            //this._stnNameDict[stationRow["IRN"].ToString()] = fullName;
            //        }

            //    }
            //    else
            //    {
            //        // Add to list with -1 to indicate skip
            //        string stationIRN = stationRow["IRN"].ToString();
            //        if (!this._stationDict.ContainsKey(stationIRN))
            //        {
            //            this._stationDict.Add(stationIRN, -1);
            //        }
            //    }
            //    //if (!this._stnNameDict.ContainsKey(stationRow["IRN"].ToString()))
            //    //{

            //    //    this._stnNameDict[stationRow["IRN"].ToString()] = fullName;
            //    //}
            //}
            //foreach (var NewRTUIRN in this._RTUNotFoundInOld)
            foreach (DataRow stationRow in this._parser.RtuTbl.Rows)
            {
                //if(this._OldRTURecDict.ContainsKey(stationRow["IRN"].ToString())) continue; //RTURecConstant: BD
                //stationRow = this._parser.RtuTbl.GetInfo(new object[] { NewRTUIRN });
                //if (stationRow == null)
                //{
                //     continue;
                //}
                if (stationRow["IRN"].ToString() == "223512261" || stationRow["IRN"].ToString() == "2152677546")
                {
                    int t = 0;
                }
                if (new List<string> { "138093201", "222452251", "1798949251", "2089811251", "1972835251", "1995038251", "2016324251", "2016325251", "1795852251", "2009856251", "2073195251", "1992663251", "2026310251", "206805261", "2020391251", "2043217251", "2042422251", "2076307251" }.Contains(stationRow["IRN"].ToString()))
                {
                    Logger.Log("StationSkippedPoints", LoggerLevel.INFO, $"STATION: Belongs to Triangle gateway. ExID: {stationRow["EXTERNAL_IDENTITY"].ToString()}\t ");
                    continue;

                }
                string fullName = EdpespFepExtensions.GetNameWithoutRTU(stationRow["IDENTIFICATION_TEXT"].ToString()); //stationRow["EXTERNAL_IDENTITY"].ToString();

                string division = stationRow["DIVISION"].ToString();
                AOR_GROUP = EdpespScadaExtensions.GetpAorGroup(stationRow["IRN"].ToString(), this._aorDict, this._RtuaorDict);
                if (AOR_GROUP.Equals(-1))
                {
                    Logger.Log("AORNotFound", LoggerLevel.INFO, $"STATION: AOR Not found: {stationRow["IRN"].ToString()}\t ");
                    AOR_GROUP = 1;
                }

                //AOR_GROUP = FindAor(stationRow["SUBSYSTEM_IRN"]);
                string abbr = EdpespFepExtensions.GetNameWithoutRTU(stationRow["IDENTIFICATION_TEXT"].ToString());
                //DataRow stationkey = this._parser.StationKeysTbl.GetInfo(new object[] { stationRow["IRN"].ToString() });
                //if(stationkey != null)  
                //    abbr = stationkey["STATION"].ToString();
                //string abbr = stationRow["EXTERNAL_IDENTITY"].ToString();
                //if (stationRow["EXTERNAL_IDENTITY"].ToString().Contains("DMS TRIANGLE GATEWAY")) continue;
                int area = EdpespScadaExtensions.GetArea(stationRow["IRN"].ToString(), this._areaDict);
                // Skip empty strings and don't convert.
                if (!string.IsNullOrEmpty(abbr))
                {
                    // Set Record, Name, Key, and pAORGroup
                    stationObj.CurrentRecordNo = stationObj.NextAvailableRecord;// ++stationRec;
                    abbr = division +"_"+ stationObj.CurrentRecordNo;
                    stationObj.SetValue("Name", fullName);
                    stationObj.SetValue("Key", abbr);
                    stationObj.SetValue("pAORGroup", AOR_GROUP);
                    stationObj.SetValue("pParent", area);

                    // Add to station dictionary using Station IRN and Record.
                    string stationIRN = stationRow["IRN"].ToString();
                    if (!this._stationDict.ContainsKey(stationIRN))
                    {
                        this._stationDict.Add(stationIRN, stationObj.CurrentRecordNo);
                        //this._stnNameDict[stationRow["IRN"].ToString()] = fullName;
                    }

                }
                else
                {
                    // Add to list with -1 to indicate skip
                    string stationIRN = stationRow["IRN"].ToString();
                    if (!this._stationDict.ContainsKey(stationIRN))
                    {
                        this._stationDict.Add(stationIRN, -1);
                    }
                }
            }

            // //eMap_DPF_SG creation for adding all the C_IND type points: BD 21/11/22
            stationObj.CurrentRecordNo = stationObj.NextAvailableRecord;// oldRec;
            stationObj.SetValue("Name", "eMap DPF Source Group");
            stationObj.SetValue("Key", "eMap_DPF_SG");
            stationObj.SetValue("pAORGroup", 1);
            stationObj.SetValue("pParent", 2);
            // Add to station dictionary using Station IRN and Record.
            this._stationDict.Add("eMap_DPF_SG", stationObj.CurrentRecordNo);
            //System_Calc creation for adding all the C type points: BD 14/07/22
            // Set Record, Name, Key, and pAORGroup
            stationObj.CurrentRecordNo = stationObj.NextAvailableRecord;//++stationRec;
            stationObj.SetValue("Name", "System_Calc");
            stationObj.SetValue("Key", "SysCalc");
            stationObj.SetValue("pAORGroup", 9);
            stationObj.SetValue("pParent", 2);
            this._stationDict.Add("System_Calc", stationObj.CurrentRecordNo);
            //Add ICCP Station
            stationObj.CurrentRecordNo = stationObj.NextAvailableRecord;//++stationRec;
            stationObj.SetValue("Name", "ICCP_Stat");
            stationObj.SetValue("Key", "ICCP_Stat");
            stationObj.SetValue("pAORGroup", 1);
            stationObj.SetValue("pParent", 2);
            this._stationDict.Add("ICCP_Stat", stationObj.CurrentRecordNo);
            //// Add to station dictionary using Station IRN and Record.

            //this._stationDict.Add("System_Calc", stationRec);


            //this._stnNameDict[stationRow["IRN"].ToString()] = fullName;


            Logger.CloseXMLLog();

        }

        /// <summary>
        /// Helper function to find abbreviation
        /// </summary>
        /// <param name="stationName">Station name from input file.</param>
        /// <returns>Returns abbreviation if found, else empty string.</returns>
        public string FindAbbr(string stationName)
        {
            if (this._parser.StationAbbrTbl.TryGetRow(new[] { stationName }, out DataRow stationRow))
            {
                if (stationRow["Conversion"].Equals("YES"))
                {
                    return stationRow["New Abbreviation_EDP"].ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                Logger.Log("UNMATCHED STATION NAME", LoggerLevel.INFO, $"Could not match Station name {stationName}");
                return string.Empty;
            }
        }
        //public int FindAor(string stationName)
        //{
        //    if (this._parser.StationAbbrTbl.TryGetRow(new[] { stationName }, out DataRow stationRow))
        //    {
        //        if (stationRow["Conversion"].Equals("YES"))
        //        {
        //            return stationRow["New Abbreviation_EDP"].ToString();
        //        }
        //        else
        //        {
        //            return string.Empty;
        //        }
        //    }
        //    else
        //    {
        //        Logger.Log("UNMATCHED STATION NAME", LoggerLevel.INFO, $"Could not match Station name {stationName}");
        //        return string.Empty;
        //    }
        //}
        /// <summary>
        /// Helper function to count Consonants in the station acronym.
        /// </summary>
        /// <param name="keyName">STATION_ACRONYM from the input.</param>
        /// <returns>Number of consonants.</returns>
        public int HowManyConsonants(string keyName)
        {
            int consonants = 0;
            keyName = keyName.ToUpper();
            foreach (char c in keyName)
            {
                switch (c)
                {
                    case 'A':
                    case 'E':
                    case 'I':
                    case 'O':
                    case 'U':
                    case '_':
                        break;
                    default:
                        consonants++;
                        break;
                }
            }
            return consonants;
        }

        /// <summary>
        /// Helper function to build a string of consonants.
        /// </summary>
        /// <param name="keyName">STATION_ACRONYM from the input.</param>
        /// <returns>A string of the first 3 consonants.</returns>
        public string GetFirstThreeCons(string keyName)
        {
            StringBuilder keyString = new StringBuilder();
            foreach (char c in keyName)
            {
                switch (c)
                {
                    case 'A':
                    case 'E':
                    case 'I':
                    case 'O':
                    case 'U':
                    case '_':
                        break;
                    default:
                        keyString.Append(c);
                        break;
                }
            }
            return keyString.ToString().Substring(0, MAX_LENGTH);
        }
    }
}
