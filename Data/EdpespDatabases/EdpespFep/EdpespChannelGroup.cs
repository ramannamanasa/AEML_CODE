using System.Collections.Generic;
using System.Data;
using edpesp_db.Data.EdpespDatabases.EdpespScada;
using OSII.ConversionToolkit;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespFep
{
    class EdpespChannelGroup
    {
        private readonly EdpespParser _parser;  // Local reference to the parser object.
        private readonly FEP _fepDb;  // Local reference to the FEP database object.
        private readonly Dictionary<string, int> _channelGroupDict;  // Local reference to the Channel Group Dictionary.
        private readonly Dictionary<string, int> _channelDict;  // Local reference to the Channel Dictionary.
        private Dictionary<string, int> _OldRTURecDict;
        private readonly Dictionary<string, int> _aorDict;  // Local reference of the AORGROUP Dict. RM 20230616
        private readonly Dictionary<string, string> _RtuaorDict;  // Local reference of the AORGROUP Dict.RM 20230616 
        /// <summary>
        /// Default constructor.
        /// Sets important local references.
        /// </summary>
        /// <param name="par">Current Parser object.</param>
        /// <param name="fepDb">Current FEP database object.</param>
        /// <param name="channelGroupDict">Current ChannelGroup Dictionary.</param>
        /// <param name="channelDict">Current Channel Dictionary.</param>
        public EdpespChannelGroup(EdpespParser par, FEP fepDb, Dictionary<string, int> channelGroupDict, Dictionary<string, int> channelDict, Dictionary<string, int> OldRTUIrnRecNoDict , Dictionary<string, string> RtuIrnDivDict, Dictionary<string, int> AorDict)
        {
            this._parser = par;
            this._fepDb = fepDb;
            this._channelGroupDict = channelGroupDict;
            this._channelDict = channelDict;
            this._OldRTURecDict = OldRTUIrnRecNoDict;
            this._RtuaorDict = RtuIrnDivDict;
            this._aorDict = AorDict;
        }

        /// <summary>
        /// Function to convert all CHANNEL_GROUP objects.
        /// </summary>
        public void ConvertChannelGroup()
        {
            Logger.OpenXMLLog();

            DbObject channelGroupObj = this._fepDb.GetDbObject("CHANNEL_GROUP");
            int channelGroupRec = 0;

            // Constant values from mapping
            const int ACK_TIMEOUT = 10;
            const int TEST_FRAME_TIMEOUT = 600; //20 BD
            const int LATEST_ACK = 8;
            //const int pAORGROUP = 1;
            const int TYPE = 5;
            const int INDIC = 1;

            int pAORGROUP;

            this._parser.RtuTbl.Sort("IRN");
            //foreach (var OldRTU in this._OldRTURecDict) //RTURecConstant: BD
            //{
            //    DataRow channelGroupRow = this._parser.RtuTbl.GetInfo(new object[] { OldRTU.Key });
            //    if (channelGroupRow == null)// skip the points that are present in the new file but that RTU is absent in the old file
            //    {
            //        // this._RTUNotFoundInOld.Add(OldRTU.Key);

            //        continue;
            //    }
            //    if (new List<string> { "138093201", "206805261", "1798949251", "1795852251", "1972835251", "1992663251", "1995038251", "2009856251", "2016324251", "2016325251", "2020391251", "2026310251", "2042422251", "2043217251", "2076307251", "222452251", "2073195251", "2089811251" }.Contains(channelGroupRow["IRN"].ToString()))
            //    {
            //        Logger.Log("channelGroupSkippedPoints", LoggerLevel.INFO, $"STATION: Belongs to Triangle gateway. ExID: {channelGroupRow["EXTERNAL_IDENTITY"].ToString()}\t ");
            //        continue;

            //    }
            //    // Set Record and Name
            //    channelGroupRec = channelGroupObj.NextAvailableRecord;
            //    channelGroupObj.CurrentRecordNo = channelGroupRec;
            //    //if (channelGroupObj.CurrentRecordNo >1) return; //BD: FEPCHECK
            //    string name = EdpespFepExtensions.GetNameWithoutRTU(channelGroupRow["IDENTIFICATION_TEXT"].ToString()); //string name = EdpespFepExtensions.GetNameWithoutRTU(channelGroupRow["IDENTIFICATION_TEXT"].ToString());

            //    channelGroupObj.SetValue("Name", name);
            //    //if (name.Length > 15)
            //    //{
            //    //    name = channelGroupRow["COM_LINE_NO"].ToString() + "_" + channelGroupRec;
            //    //    name = channelGroupRow["EXTERNAL_IDENTITY"].ToString().StartsWith("RTU") ? "R_" + name : "F_" + name;
            //    //}
            //    // Set pChannel
            //    if (!string.IsNullOrEmpty(channelGroupRow["Channel 4 IP Details"].ToString()))
            //    {
            //        SetpChannel(channelGroupObj, name + "_C4", 3);
            //        SetpChannel(channelGroupObj, name + "_C3", 2);
            //        SetpChannel(channelGroupObj, name + "_C2", 1);
            //        SetpChannel(channelGroupObj, name + "_C1", 0);

            //    }
            //    else if (!string.IsNullOrEmpty(channelGroupRow["Channel 3 IP Details"].ToString()))
            //    {
            //        SetpChannel(channelGroupObj, name + "_C3", 2);
            //        SetpChannel(channelGroupObj, name + "_C2", 1);
            //        SetpChannel(channelGroupObj, name + "_C1", 0);

            //    }
            //    else if (!string.IsNullOrEmpty(channelGroupRow["Channel 2 IP Details"].ToString()))
            //    {
            //        SetpChannel(channelGroupObj, name + "_C2", 1);
            //        SetpChannel(channelGroupObj, name + "_C1", 0);

            //    }
            //    else
            //    {

            //        SetpChannel(channelGroupObj, name + "_C1", 0);
            //    }


            //    // Set Protocol, pAORGROUP, and Type
            //    channelGroupObj.SetValue("Protocol", (int)FepProtocol.IEC104);
            //    channelGroupObj.SetValue("pAORGroup", pAORGROUP);
            //    channelGroupObj.SetValue("Type", TYPE);

            //    // Set pRTU - this will have the same record number
            //    if (channelGroupRec == 4)
            //    {
            //        int tt = 0;
            //    }
            //    channelGroupObj.SetValue("pRTU", channelGroupRec);

            //    // Set constant values
            //    channelGroupObj.SetValue("Parameters", 3, ACK_TIMEOUT);
            //    channelGroupObj.SetValue("Parameters", 4, TEST_FRAME_TIMEOUT);
            //    channelGroupObj.SetValue("Parameters", 5, LATEST_ACK);
            //    channelGroupObj.SetValue("Parameters", 8, LATEST_ACK);
            //    channelGroupObj.SetValue("Indic", INDIC);

            //    // Add to Dictionary
            //    if (!this._channelGroupDict.ContainsKey(name))
            //    {
            //        this._channelGroupDict.Add(name, channelGroupRec);
            //    }
            //}
            foreach (DataRow channelGroupRow in this._parser.RtuTbl.Rows)
            {
                //if (this._OldRTURecDict.ContainsKey(channelGroupRow["IRN"].ToString())) continue; //RTURecConstant: BD
                //skip if it contains trianglr gateway
                //if (channelGroupRow["EXTERNAL_IDENTITY"].ToString().Contains("DMS TRIANGLE GATEWAY")) continue;// commented by BD: 030423
                //if (new List<string> { "138093201", "206805261", "1798949251", "1795852251", "1972835251", "1992663251", "1995038251", "2009856251", "2016324251", "2016325251", "2020391251", "2026310251", "2042422251", "2043217251", "2076307251", "222452251", "2073195251", "2089811251" }.Contains(channelGroupRow["IRN"].ToString()))
                if (new List<string> { "138093201", "222452251", "1798949251", "2089811251", "1972835251", "1995038251", "2016324251", "2016325251", "1795852251", "2009856251", "2073195251", "1992663251", "2026310251", "206805261", "2020391251", "2043217251", "2042422251", "2076307251" }.Contains(channelGroupRow["IRN"].ToString()))
                {
                    Logger.Log("channelGroupSkippedPoints", LoggerLevel.INFO, $"STATION: Belongs to Triangle gateway. ExID: {channelGroupRow["EXTERNAL_IDENTITY"].ToString()}\t ");
                    continue;

                }
                // Set Record and Name
                channelGroupRec = channelGroupObj.NextAvailableRecord;
                channelGroupObj.CurrentRecordNo = channelGroupRec;
                //if (channelGroupObj.CurrentRecordNo >10) return; //BD: FEPCHECK
                string name = EdpespFepExtensions.GetNameWithoutRTU(channelGroupRow["IDENTIFICATION_TEXT"].ToString()); //string name = EdpespFepExtensions.GetNameWithoutRTU(channelGroupRow["IDENTIFICATION_TEXT"].ToString());
                if(channelGroupObj.CurrentRecordNo == 3805)
                {
                    int t = 0;
                }
                channelGroupObj.SetValue("Name", name);
                //if (name.Length > 15)
                //{
                //    name = channelGroupRow["COM_LINE_NO"].ToString() + "_" + channelGroupRec;
                //    name = channelGroupRow["EXTERNAL_IDENTITY"].ToString().StartsWith("RTU") ? "R_" + name : "F_" + name;
                //}
                // Set pChannel
                if (!string.IsNullOrEmpty(channelGroupRow["Channel 4 IP Details"].ToString()))
                {
                    SetpChannel(channelGroupObj, name + "_C4", 3);
                    SetpChannel(channelGroupObj, name + "_C3", 2);
                    SetpChannel(channelGroupObj, name + "_C2", 1);
                    SetpChannel(channelGroupObj, name + "_C1", 0);

                }
                else if (!string.IsNullOrEmpty(channelGroupRow["Channel 3 IP Details"].ToString()))
                {
                    SetpChannel(channelGroupObj, name + "_C3", 2);
                    SetpChannel(channelGroupObj, name + "_C2", 1);
                    SetpChannel(channelGroupObj, name + "_C1", 0);

                }
                else if (!string.IsNullOrEmpty(channelGroupRow["Channel 2 IP Details"].ToString()))
                {
                    SetpChannel(channelGroupObj, name + "_C2", 1);
                    SetpChannel(channelGroupObj, name + "_C1", 0);

                }
                else 
                {

                    SetpChannel(channelGroupObj, name + "_C1", 0);
                }
                    

                // Set Protocol, pAORGROUP, and Type
                channelGroupObj.SetValue("Protocol", (int)FepProtocol.IEC104);
                string name1 = EdpespFepExtensions.GetNameWithoutRTU(channelGroupRow["IDENTIFICATION_TEXT"].ToString()); //RM 
                string division = channelGroupRow["DIVISION"].ToString();
                pAORGROUP = EdpespScadaExtensions.GetpAorGroup(channelGroupRow["IRN"].ToString(), this._aorDict, this._RtuaorDict);
                if (pAORGROUP.Equals(-1))
                {
                    Logger.Log("AORNotFound", LoggerLevel.INFO, $"RTU: AOR Not found: {channelGroupRow["IRN"].ToString()}\t ");
                    pAORGROUP = 1;
                }
                channelGroupObj.SetValue("pAORGroup", pAORGROUP);
                //channelGroupObj.SetValue("pAORGroup", pAORGROUP);
                channelGroupObj.SetValue("Type", TYPE);

                // Set pRTU - this will have the same record number
                if (channelGroupRec == 4)
                {
                    int tt = 0;
                }
                channelGroupObj.SetValue("pRTU", channelGroupRec);

                // Set constant values
                channelGroupObj.SetValue("Parameters", 3, ACK_TIMEOUT); //10
                channelGroupObj.SetValue("Parameters", 4, TEST_FRAME_TIMEOUT);//20
                channelGroupObj.SetValue("Parameters", 5, LATEST_ACK); //8
                channelGroupObj.SetValue("Parameters", 8, LATEST_ACK);//8
                channelGroupObj.SetValue("Indic", INDIC);

                // Add to Dictionary
                if (!this._channelGroupDict.ContainsKey(name))
                {
                    this._channelGroupDict.Add(name, channelGroupRec);
                }
            }

            Logger.CloseXMLLog();
        }

        /// <summary>
        /// Helper function to set pChannel.
        /// </summary>
        /// <param name="channelGroupObj">Current CHANNEL_GROUP object.</param>
        /// <param name="channelName">Current Channel Group name.</param>
        public void SetpChannel(DbObject channelGroupObj, string channelName, int index)
        {
            if (this._channelDict.TryGetValue(channelName, out int pChannel))
            {
                channelGroupObj.SetValue("pCHANNEL",index, pChannel);
            }
            else
            {
                Logger.Log("NO pCHANNEL MATCH", LoggerLevel.INFO, $"Could not match channel group to a pCHANNEL:{channelName}");
            }
        }
    }
}
