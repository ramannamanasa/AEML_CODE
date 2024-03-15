using System;
using System.Collections.Generic;
using System.Data;
using edpesp_db.Data.EdpespDatabases.EdpespScada;
using OSII.ConversionToolkit;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespFep
{
    class EdpespRtuData
    {
        private readonly EdpespParser _parser;  // Local reference to the parser object.
        private readonly FEP _fepDb;  // Local reference to the FEP database object.
        private readonly Dictionary<string, int> _channelGroupDict;  // Local reference to the Channel Group Dictionary.
        private readonly Dictionary<string, int> _rtuDataDict;  // Local reference to the RTU_DATA Dictionary.
        private Dictionary<string, int> _OldRTURecDict;
        private readonly Dictionary<string, int> _aorDict;  // Local reference of the AORGROUP Dict.
        private readonly Dictionary<string, string> _RtuaorDict;  // Local reference of the AORGROUP Dict. 
        /// <summary>
        /// Default constructor.
        /// Sets important local references.
        /// </summary>
        /// <param name="par">Current Parser object.</param>
        /// <param name="fepDb">Current FEP database object.</param>
        /// <param name="channelGroupDict">Current ChannelGroup Dictionary.</param>
        /// <param name="rtuDataDict">Current RTU_DATA dict.</param>
        public EdpespRtuData(EdpespParser par, FEP fepDb, Dictionary<string, int> channelGroupDict, Dictionary<string, int> rtuDataDict, Dictionary<string, int> OldRTUIrnRecNoDict, Dictionary<string, string> RtuIrnDivDict, Dictionary<string, int> AorDict)
        {
            this._parser = par;
            this._fepDb = fepDb;
            this._channelGroupDict = channelGroupDict;
            this._rtuDataDict = rtuDataDict;
            this._OldRTURecDict = OldRTUIrnRecNoDict;
            this._RtuaorDict = RtuIrnDivDict;
            this._aorDict = AorDict;
        }

        /// <summary>
        /// Function to convert all RTU_DATA objects.
        /// </summary>
        public void ConvertRtuData()
        {
            Logger.OpenXMLLog();

            DbObject rtuDataObj = this._fepDb.GetDbObject("RTU_DATA");
            int rtuDataRec = 0;

            // Constant values from mapping
            int INDIC = 1;
            int pAORGROUP = 1;
            int SUBTYPE = 10;
            int GOODPERCENT = 75;
            int FAILPERCENT = 25;
            int NUMBER_OF_SCANS = 100;
            int DELAY_REQUEST = 0;

            this._parser.RtuTbl.Sort("IRN");
            //foreach (var OldRTU in this._OldRTURecDict) //RTURecConstant: BD
            //{
            //    DataRow rtuDataRow = this._parser.RtuTbl.GetInfo(new object[] { OldRTU.Key });
            //    if (rtuDataRow == null)// skip the points that are present in the new file but that RTU is absent in the old file
            //    {
            //       // this._RTUNotFoundInOld.Add(OldRTU.Key);

            //        continue;
            //    }
            //    if (new List<string> { "138093201", "206805261", "1798949251", "1795852251", "1972835251", "1992663251", "1995038251", "2009856251", "2016324251", "2016325251", "2020391251", "2026310251", "2042422251", "2043217251", "2076307251", "222452251", "2073195251", "2089811251" }.Contains(rtuDataRow["IRN"].ToString()))
            //    {
            //        Logger.Log("StationSkippedPoints", LoggerLevel.INFO, $"STATION: Belongs to Triangle gateway. ExID: {rtuDataRow["EXTERNAL_IDENTITY"].ToString()}\t ");
            //        continue;

            //    }
            //    // Set Record, Name, and Indic
            //    rtuDataRec = rtuDataObj.NextAvailableRecord;
            //    rtuDataObj.CurrentRecordNo = rtuDataRec;
            //    //if (rtuDataObj.CurrentRecordNo > 1) return; //BD: FEPCHECK
            //    string name = EdpespFepExtensions.GetNameWithoutRTU(rtuDataRow["IDENTIFICATION_TEXT"].ToString());
            //    rtuDataObj.SetValue("Name", name);
            //    rtuDataObj.SetValue("Indic", INDIC);

            //    // Set Protocol, Subtype, and Address
            //    rtuDataObj.SetValue("Protocol", (int)FepProtocol.IEC104);
            //    rtuDataObj.SetValue("SubType", SUBTYPE);
            //    rtuDataObj.SetValue("Address", rtuDataRow["TERMINAL_NUMBER"].ToString());

            //    // Set pChannel_Group and pAORGroup
            //    //SetpChannelGroup(rtuDataObj, name);
            //    rtuDataObj.SetValue("pAORGroup", pAORGROUP);

            //    // Set Parms
            //    rtuDataObj.SetValue("RTUParms", 0, GOODPERCENT);
            //    rtuDataObj.SetValue("RTUParms", 1, FAILPERCENT);
            //    rtuDataObj.SetValue("RTUParms", 2, NUMBER_OF_SCANS);
            //    rtuDataObj.SetValue("RTUParms", 12, DELAY_REQUEST);
            //    rtuDataObj.SetValue("pCHANNEL_GROUP", 0, rtuDataRec);
            //    // Add to Dictionary
            //    string irn = rtuDataRow["IRN"].ToString();
            //    if (!this._rtuDataDict.ContainsKey(irn))
            //    {
            //        this._rtuDataDict.Add(irn, rtuDataRec);
            //    }
            //}
            foreach (DataRow rtuDataRow in this._parser.RtuTbl.Rows)
            {
                //if (this._OldRTURecDict.ContainsKey(rtuDataRow["IRN"].ToString())) continue; //RTURecConstant: BD
                //skip if it contains trianglr gateway
                //if (rtuDataRow["EXTERNAL_IDENTITY"].ToString().Contains("DMS TRIANGLE GATEWAY")) continue;// commented by BD
                //if (new List<string> { "138093201", "206805261", "1798949251", "1795852251", "1972835251", "1992663251", "1995038251", "2009856251", "2016324251", "2016325251", "2020391251", "2026310251", "2042422251", "2043217251", "2076307251", "222452251", "2073195251", "2089811251" }.Contains(rtuDataRow["IRN"].ToString()))
                if (new List<string> { "138093201", "222452251", "1798949251", "2089811251", "1972835251", "1995038251", "2016324251", "2016325251", "1795852251", "2009856251", "2073195251", "1992663251", "2026310251", "206805261", "2020391251", "2043217251", "2042422251", "2076307251" }.Contains(rtuDataRow["IRN"].ToString()))
                {
                    Logger.Log("StationSkippedPoints", LoggerLevel.INFO, $"STATION: Belongs to Triangle gateway. ExID: {rtuDataRow["EXTERNAL_IDENTITY"].ToString()}\t ");
                    continue;

                }
                // Set Record, Name, and Indic
                rtuDataRec = rtuDataObj.NextAvailableRecord;
                rtuDataObj.CurrentRecordNo = rtuDataRec;
                
                //if(rtuDataRow["IDENTIFICATION_TEXT"].ToString().StartsWith("FRTU"))
                //    rtuDataObj.SetValue("pAORGroup", 10); //DMS AOR
               //if (rtuDataObj.CurrentRecordNo > 10) return; //BD: FEPCHECK
                string name = EdpespFepExtensions.GetNameWithoutRTU(rtuDataRow["IDENTIFICATION_TEXT"].ToString());
                rtuDataObj.SetValue("Name", name);
                rtuDataObj.SetValue("Indic", INDIC);

                // Set Protocol, Subtype, and Address
                rtuDataObj.SetValue("Protocol", (int)FepProtocol.IEC104);
                rtuDataObj.SetValue("SubType", SUBTYPE);
                rtuDataObj.SetValue("Address", rtuDataRow["TERMINAL_NUMBER"].ToString());

                // Set pChannel_Group and pAORGroup
                //SetpChannelGroup(rtuDataObj, name);

                if (rtuDataRow["IDENTIFICATION_TEXT"].ToString().StartsWith("FRTU"))
                    rtuDataObj.SetValue("pAORGroup", 10); //DMS AOR                
                else
                {
                    string division = rtuDataRow["DIVISION"].ToString();
                    pAORGROUP = EdpespScadaExtensions.GetpAorGroup(rtuDataRow["IRN"].ToString(), this._aorDict, this._RtuaorDict);
                    if (pAORGROUP.Equals(-1))
                    {
                        Logger.Log("AORNotFound", LoggerLevel.INFO, $"RTU: AOR Not found: {rtuDataRow["IRN"].ToString()}\t ");
                        pAORGROUP = 1;
                    }
                    rtuDataObj.SetValue("pAORGroup", pAORGROUP);
                }
                // Set Parms
                rtuDataObj.SetValue("RTUParms", 0, GOODPERCENT);
                rtuDataObj.SetValue("RTUParms", 1, FAILPERCENT);
                rtuDataObj.SetValue("RTUParms", 2, NUMBER_OF_SCANS);
                rtuDataObj.SetValue("RTUParms", 12, DELAY_REQUEST);
                rtuDataObj.SetValue("RTUParms", 22, 75); //BD
                rtuDataObj.SetValue("pCHANNEL_GROUP", 0, rtuDataRec);
                // Add to Dictionary
                string irn = rtuDataRow["IRN"].ToString();
                if (!this._rtuDataDict.ContainsKey(irn))
                {
                    this._rtuDataDict.Add(irn, rtuDataRec);
                }
            }

            Logger.CloseXMLLog();
        }

        /// <summary>
        /// Helper function to set pCHANNEL_GROUP.
        /// </summary>
        /// <param name="rtuDataObj">Current RTU_DATA object.</param>
        /// <param name="channelName">Name of Current RTU_DATA object.</param>
        public void SetpChannelGroup(DbObject rtuDataObj, string channelName)
        {
            if (this._channelGroupDict.TryGetValue(channelName, out int pChannel))
            {
                rtuDataObj.SetValue("pCHANNEL_GROUP", pChannel);
            }
            else
            {
                Logger.Log("NO pCHANNEL_GROUP MATCH", LoggerLevel.INFO, $"Could not match RTU_DATA to a channel group:{channelName}");
            }
        }
    }
}
