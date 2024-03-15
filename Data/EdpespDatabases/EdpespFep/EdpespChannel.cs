using System;
using System.Collections.Generic;
using System.Data;
using OSII.ConversionToolkit;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespFep
{
    class EdpespChannel
    {
        private readonly EdpespParser _parser;  // Local reference to the parser object.
        private readonly FEP _fepDb;  // Local reference to the FEP database object.
        private readonly Dictionary<string, int> _channelDict;  // Local reference to the Channel Dictionary.
        private Dictionary<string, int> _OldRTURecDict;
        /// <summary>
        /// Default constructor.
        /// Sets important local references and variables.
        /// </summary>
        /// <param name="par">Current parser object.</param>
        /// <param name="fepDb">Current FEP database object.</param>
        /// <param name="channelDict">Current channel dictionary.</param>
        public EdpespChannel(EdpespParser par, FEP fepDb, Dictionary<string, int> channelDict, Dictionary<string, int> OldRTUIrnRecNoDict)
        {
            this._parser = par;
            this._fepDb = fepDb;
            this._channelDict = channelDict;
            this._OldRTURecDict = OldRTUIrnRecNoDict;
        }

        /// <summary>
        /// Function to convert all CHANNEL objects.
        /// </summary>
        public void ConvertChannel()
        {
            Logger.OpenXMLLog();

            DbObject channelObj = this._fepDb.GetDbObject("CHANNEL");
            int channelRec = 0;

            // Constant values from mapping
            int INDICT = 1;
            int CHANNEL_RESP_TIMEOUT = 30;
            int PHYSICAL_PORT = 2404;
            int rturec = 0;
            int CHANNEL_REQ_DELAY = 90;

            this._parser.RtuTbl.Sort("IRN");
            //foreach (var OldRTU in this._OldRTURecDict) //RTURecConstant: BD
            //{
            //    DataRow channelRow = this._parser.RtuTbl.GetInfo(new object[] { OldRTU.Key });
            //    if (channelRow == null)// skip the points that are present in the new file but that RTU is absent in the old file
            //    {
            //        // this._RTUNotFoundInOld.Add(OldRTU.Key);

            //        continue;
            //    }
            //    if (new List<string> { "138093201", "206805261", "1798949251", "1795852251", "1972835251", "1992663251", "1995038251", "2009856251", "2016324251", "2016325251", "2020391251", "2026310251", "2042422251", "2043217251", "2076307251", "222452251", "2073195251", "2089811251" }.Contains(channelRow["IRN"].ToString()))
            //    {
            //        Logger.Log("channelSkippedPoints", LoggerLevel.INFO, $"STATION: Belongs to Triangle gateway. ExID: {channelRow["EXTERNAL_IDENTITY"].ToString()}\t ");
            //        continue;

            //    }
            //    rturec++;
            //    // Set Record and Name

            
            //    string name1 = EdpespFepExtensions.GetNameWithoutRTU(channelRow["IDENTIFICATION_TEXT"].ToString());
            //    string name = name1;// EdpespFepExtensions.GetNameWithoutRTU(channelRow["EXTERNAL_IDENTITY"].ToString());
            //    string indentificationtext = EdpespFepExtensions.GetNameWithoutRTU(channelRow["IDENTIFICATION_TEXT"].ToString());

            //    // DataRow RTUIP = this._parser.RtuIpTbl.GetInfo(new object[] { channelRow["IDENTIFICATION_TEXT"].ToString() });
            //    //if(RTUIP != null)
            //    {
            //        //channelObj.CurrentRecordNo = ++channelRec;
            //        //if (
            //        {
            //            if(channelRec > 3600)
            //            {
            //                int t = 0;
            //            }
            //            channelObj.CurrentRecordNo = ++channelRec;
            //            name = indentificationtext + "_C1";
            //            string isp = string.IsNullOrEmpty(channelRow["Channel 1 ISP Name"].ToString()) ? "xxx" : channelRow["Channel 1 ISP Name"].ToString();
            //            name1 = rturec + "_" + isp + "_C1";
            //            name1 = channelRow["EXTERNAL_IDENTITY"].ToString().StartsWith("RTU") ? "R_" + name1 : "F_" + name1;
            //            channelObj.SetValue("Name", name1);


            //            // Set Constant values
            //            channelObj.SetValue("Indic", INDICT);
            //            channelObj.SetValue("ChannelRespTimeoutMsec", CHANNEL_RESP_TIMEOUT);
            //            channelObj.SetValue("PhysicalPort", PHYSICAL_PORT);
            //            // Set pChannelGroup - this will have the same record number
            //            channelObj.SetValue("pCHANNEL_GROUP", rturec);
            //            //Set hostname
            //            string hostname = string.IsNullOrEmpty(channelRow["Channel 1 IP Details"].ToString()) ? "0.0.0.0" : channelRow["Channel 1 IP Details"].ToString().Trim();
            //            channelObj.SetValue("Hostname", hostname);
            //            // Add to Dictionary
            //            if (!this._channelDict.ContainsKey(name))
            //            {
            //                this._channelDict.Add(name, channelRec);
            //            }
            //        }
            //        if (!string.IsNullOrEmpty(channelRow["Channel 2 IP Details"].ToString()) && channelRow["Channel 2 IP Details"].ToString()!= "0") //second channel
            //        {
            //            channelObj.CurrentRecordNo = ++channelRec;
            //            name = indentificationtext + "_C2";
            //            string isp = string.IsNullOrEmpty(channelRow["Channel 2 ISP Name"].ToString()) ? "xxx" : channelRow["Channel 2 ISP Name"].ToString();
            //            name1 = rturec + "_" + isp + "_C2";
            //            name1 = channelRow["EXTERNAL_IDENTITY"].ToString().StartsWith("RTU") ? "R_" + name1 : "F_" + name1;
            //            channelObj.SetValue("Name", name1);

            //            // Set Constant values
            //            channelObj.SetValue("Indic", INDICT);
            //            channelObj.SetValue("ChannelRespTimeoutMsec", CHANNEL_RESP_TIMEOUT);
            //            channelObj.SetValue("PhysicalPort", PHYSICAL_PORT);
            //            // Set pChannelGroup - this will have the same record number
            //            channelObj.SetValue("pCHANNEL_GROUP", rturec);
            //            //Set Hostname
            //            string hostname = string.IsNullOrEmpty(channelRow["Channel 2 IP Details"].ToString()) ? "0.0.0.0" : channelRow["Channel 2 IP Details"].ToString().Trim();
            //            channelObj.SetValue("Hostname", hostname);
            //            // Add to Dictionary
            //            if (!this._channelDict.ContainsKey(name))
            //            {
            //                this._channelDict.Add(name, channelRec);
            //            }
            //        }
            //        if (!string.IsNullOrEmpty(channelRow["Channel 3 IP Details"].ToString()) && channelRow["Channel 3 IP Details"].ToString() != "0") //third channel
            //        {
            //            channelObj.CurrentRecordNo = ++channelRec;
            //            name = indentificationtext + "_C3";
            //            string isp = string.IsNullOrEmpty(channelRow["Channel 3 ISP Name"].ToString()) ? "xxx" : channelRow["Channel 3 ISP Name"].ToString();
            //            name1 = rturec + "_" + isp + "_C3";
            //            name1 = channelRow["EXTERNAL_IDENTITY"].ToString().StartsWith("RTU") ? "R_" + name1 : "F_" + name1;
            //            channelObj.SetValue("Name", name1);

            //            // Set Constant values
            //            channelObj.SetValue("Indic", INDICT);
            //            channelObj.SetValue("ChannelRespTimeoutMsec", CHANNEL_RESP_TIMEOUT);
            //            channelObj.SetValue("PhysicalPort", PHYSICAL_PORT);
            //            // Set pChannelGroup - this will have the same record number
            //            channelObj.SetValue("pCHANNEL_GROUP", rturec);
            //            //Set Hostname
            //            string hostname = string.IsNullOrEmpty(channelRow["Channel 3 IP Details"].ToString()) ? "0.0.0.0" : channelRow["Channel 3 IP Details"].ToString().Trim();
            //            channelObj.SetValue("Hostname", hostname);
            //            // Add to Dictionary
            //            if (!this._channelDict.ContainsKey(name))
            //            {
            //                this._channelDict.Add(name, channelRec);
            //            }
            //        }
            //        if (!string.IsNullOrEmpty(channelRow["Channel 4 IP Details"].ToString()) && channelRow["Channel 4 IP Details"].ToString() != "0") //fourth channel
            //        {
            //            channelObj.CurrentRecordNo = ++channelRec;
            //            name = indentificationtext + "_C4";
            //            string isp = string.IsNullOrEmpty(channelRow["Channel 4 ISP Name"].ToString()) ? "xxx" : channelRow["Channel 4 ISP Name"].ToString();
            //            name1 = rturec + "_" + isp + "_C4";
            //            name1 = channelRow["EXTERNAL_IDENTITY"].ToString().StartsWith("RTU") ? "R_" + name1 : "F_" + name1;
            //            channelObj.SetValue("Name", name1);

            //            // Set Constant values
            //            channelObj.SetValue("Indic", INDICT);
            //            channelObj.SetValue("ChannelRespTimeoutMsec", CHANNEL_RESP_TIMEOUT);
            //            channelObj.SetValue("PhysicalPort", PHYSICAL_PORT);
            //            // Set pChannelGroup - this will have the same record number
            //            channelObj.SetValue("pCHANNEL_GROUP", rturec);
            //            //Set Hostname
            //            string hostname = string.IsNullOrEmpty(channelRow["Channel 4 IP Details"].ToString()) ? "0.0.0.0" : channelRow["Channel 4 IP Details"].ToString().Trim();
            //            channelObj.SetValue("Hostname", hostname);
            //            // Add to Dictionary
            //            if (!this._channelDict.ContainsKey(name))
            //            {
            //                this._channelDict.Add(name, channelRec);
            //            }
            //        }

            //    }
            //}
            foreach (DataRow channelRow in this._parser.RtuTbl.Rows) 
            {
                //if (this._OldRTURecDict.ContainsKey(channelRow["IRN"].ToString())) continue; //RTURecConstant: BD

                //skip if it contains trianglr gateway
                //if (channelRow["EXTERNAL_IDENTITY"].ToString().Contains("DMS TRIANGLE GATEWAY")) continue;// commented by BD: 030423
                //if (new List<string> { "138093201", "206805261", "1798949251", "1795852251", "1972835251", "1992663251", "1995038251", "2009856251", "2016324251", "2016325251", "2020391251", "2026310251", "2042422251", "2043217251", "2076307251", "222452251", "2073195251", "2089811251" }.Contains(channelRow["IRN"].ToString()))
                if (new List<string> { "138093201", "222452251", "1798949251", "2089811251", "1972835251", "1995038251", "2016324251", "2016325251", "1795852251", "2009856251", "2073195251", "1992663251", "2026310251", "206805261", "2020391251", "2043217251", "2042422251", "2076307251" }.Contains(channelRow["IRN"].ToString()))
                {
                    Logger.Log("channelSkippedPoints", LoggerLevel.INFO, $"STATION: Belongs to Triangle gateway. ExID: {channelRow["EXTERNAL_IDENTITY"].ToString()}\t ");
                    continue;

                }
                ++rturec;
                // Set Record and Name

                //if (channelObj.CurrentRecordNo > 1) return; //BD: FEPCHECK
                string name1 = EdpespFepExtensions.GetNameWithoutRTU(channelRow["IDENTIFICATION_TEXT"].ToString());
                string name = name1;// EdpespFepExtensions.GetNameWithoutRTU(channelRow["EXTERNAL_IDENTITY"].ToString());
                string indentificationtext = EdpespFepExtensions.GetNameWithoutRTU(channelRow["IDENTIFICATION_TEXT"].ToString());

               // DataRow RTUIP = this._parser.RtuIpTbl.GetInfo(new object[] { channelRow["IDENTIFICATION_TEXT"].ToString() });
               //if(RTUIP != null)
                {
                    //channelObj.CurrentRecordNo = ++channelRec;
                    //if (
                    {
                        channelObj.CurrentRecordNo = ++channelRec;
                        //if (channelObj.CurrentRecordNo > 10) return; //BD: FEPCHECK
                        name = indentificationtext + "_C1";
                        string isp = string.IsNullOrEmpty(channelRow["Channel 1 ISP Name"].ToString()) ? "xxx" : channelRow["Channel 1 ISP Name"].ToString();
                        name1 =   rturec + "_"+ isp + "_C1";
                        name1 = channelRow["EXTERNAL_IDENTITY"].ToString().StartsWith("RTU") ? "R_" + name1 : "F_" + name1;
                        channelObj.SetValue("Name", name1);
                        

                        // Set Constant values
                        channelObj.SetValue("Indic", INDICT);
                        channelObj.SetValue("ChannelRespTimeoutMsec", CHANNEL_RESP_TIMEOUT);
                        channelObj.SetValue("PhysicalPort", PHYSICAL_PORT);
                        channelObj.SetValue("ChannelReqDelay", CHANNEL_REQ_DELAY);
                        // Set pChannelGroup - this will have the same record number
                        channelObj.SetValue("pCHANNEL_GROUP", rturec);
                        //Set hostname
                        string hostname = string.IsNullOrEmpty(channelRow["Channel 1 IP Details"].ToString()) ? "0.0.0.0": channelRow["Channel 1 IP Details"].ToString().Trim();
                        channelObj.SetValue("Hostname", hostname);
                        // Add to Dictionary
                        if (!this._channelDict.ContainsKey(name))
                        {
                            this._channelDict.Add(name, channelRec);
                        }
                    }
                    //if (!string.IsNullOrEmpty(channelRow["Channel 2 IP Details"].ToString()) && channelRow["Channel 2 IP Details"].ToString() != "0") //second channel
                    {
                        channelObj.CurrentRecordNo = ++channelRec;
                        name = indentificationtext + "_C2";
                        string isp = string.IsNullOrEmpty(channelRow["Channel 2 ISP Name"].ToString()) ? "xxx" : channelRow["Channel 2 ISP Name"].ToString();
                        name1 = rturec + "_" + isp + "_C2";
                        name1 = channelRow["EXTERNAL_IDENTITY"].ToString().StartsWith("RTU") ? "R_" + name1 : "F_" + name1;
                        channelObj.SetValue("Name", name1);
                        
                        // Set Constant values
                        channelObj.SetValue("Indic", INDICT);
                        channelObj.SetValue("ChannelRespTimeoutMsec", CHANNEL_RESP_TIMEOUT);
                        channelObj.SetValue("PhysicalPort", PHYSICAL_PORT);
                        channelObj.SetValue("ChannelReqDelay", CHANNEL_REQ_DELAY);
                        // Set pChannelGroup - this will have the same record number
                        channelObj.SetValue("pCHANNEL_GROUP", rturec);
                        //Set Hostname
                        string hostname = string.IsNullOrEmpty(channelRow["Channel 2 IP Details"].ToString()) ? "0.0.0.0" : channelRow["Channel 2 IP Details"].ToString().Trim();
                        channelObj.SetValue("Hostname", hostname);
                        // Add to Dictionary

                        if (!this._channelDict.ContainsKey(name))
                        {
                            this._channelDict.Add(name, channelRec);
                        }
                    }
                    //if (!string.IsNullOrEmpty(channelRow["Channel 3 IP Details"].ToString()) && channelRow["Channel 3 IP Details"].ToString() != "0") //third channel
                    if (!(channelRow["IDENTIFICATION_TEXT"].ToString().StartsWith("DMS") || channelRow["IDENTIFICATION_TEXT"].ToString().StartsWith("FRTU"))) //third channel For DMS and FRTU 2 channels rest 4 channels
                    {
                        channelObj.CurrentRecordNo = ++channelRec;
                        name = indentificationtext + "_C3";
                        string isp = string.IsNullOrEmpty(channelRow["Channel 3 ISP Name"].ToString()) ? "xxx" : channelRow["Channel 3 ISP Name"].ToString();
                        name1 = rturec + "_" + isp + "_C3";
                        name1 = channelRow["EXTERNAL_IDENTITY"].ToString().StartsWith("RTU") ? "R_" + name1 : "F_" + name1;
                        channelObj.SetValue("Name", name1);
                        
                        // Set Constant values
                        channelObj.SetValue("Indic", INDICT);
                        channelObj.SetValue("ChannelRespTimeoutMsec", CHANNEL_RESP_TIMEOUT);
                        channelObj.SetValue("PhysicalPort", PHYSICAL_PORT);
                        channelObj.SetValue("ChannelReqDelay", CHANNEL_REQ_DELAY);
                        // Set pChannelGroup - this will have the same record number
                        channelObj.SetValue("pCHANNEL_GROUP", rturec);
                        //Set Hostname
                        string hostname = string.IsNullOrEmpty(channelRow["Channel 3 IP Details"].ToString()) ? "0.0.0.0" : channelRow["Channel 3 IP Details"].ToString().Trim();
                        channelObj.SetValue("Hostname", hostname);
                        // Add to Dictionary
                        if (!this._channelDict.ContainsKey(name))
                        {
                            this._channelDict.Add(name, channelRec);
                        }
                    }
                    //if (!string.IsNullOrEmpty(channelRow["Channel 4 IP Details"].ToString()) && channelRow["Channel 4 IP Details"].ToString() != "0") //fourth channel
                    if (!(channelRow["IDENTIFICATION_TEXT"].ToString().StartsWith("DMS") || channelRow["IDENTIFICATION_TEXT"].ToString().StartsWith("FRTU"))) //fourth channel For DMS and FRTU 2 channels rest 4 channels
                    {
                        channelObj.CurrentRecordNo = ++channelRec;
                        name = indentificationtext + "_C4";
                        string isp = string.IsNullOrEmpty(channelRow["Channel 4 ISP Name"].ToString()) ? "xxx" : channelRow["Channel 4 ISP Name"].ToString();
                        name1 = rturec + "_" + isp + "_C4";
                        name1 = channelRow["EXTERNAL_IDENTITY"].ToString().StartsWith("RTU") ? "R_" + name1 : "F_" + name1;
                        channelObj.SetValue("Name", name1);
                        
                        // Set Constant values
                        channelObj.SetValue("Indic", INDICT);
                        channelObj.SetValue("ChannelRespTimeoutMsec", CHANNEL_RESP_TIMEOUT);
                        channelObj.SetValue("PhysicalPort", PHYSICAL_PORT);
                        channelObj.SetValue("ChannelReqDelay", CHANNEL_REQ_DELAY);
                        // Set pChannelGroup - this will have the same record number
                        channelObj.SetValue("pCHANNEL_GROUP", rturec);
                        //Set Hostname
                        string hostname = string.IsNullOrEmpty(channelRow["Channel 4 IP Details"].ToString()) ? "0.0.0.0" : channelRow["Channel 4 IP Details"].ToString().Trim();
                        channelObj.SetValue("Hostname", hostname);
                        // Add to Dictionary
                        if (!this._channelDict.ContainsKey(name))
                        {
                            this._channelDict.Add(name, channelRec);
                        }
                    }

                }
                
                // Set Hostname
                //SA:20211124 the file is not avaialble, so commenting
                //SetHostname(channelObj, channelRow);

                

               

                
            }

            Logger.CloseXMLLog();
        }

        /// <summary>
        /// Helper function to set Hostname.
        /// </summary>
        /// <param name="channelObj">Current CHANNEL Object.</param>
        /// <param name="channelRow">Current DataRow row from the input.</param>
        public void SetHostname(DbObject channelObj, DataRow channelRow)
        {
            string externalId = channelRow["EXTERNAL_IDENTITY"].ToString();
            if (this._parser.Hostnames.TryGetRow(new[] { externalId}, out DataRow hostRow))
            {
                channelObj.SetValue("Hostname", hostRow["DIRECCIÓN IP"].ToString());
            }
            else
            {
                Logger.Log("NO HOSTNAME MATCH", LoggerLevel.INFO, $"Could not match external indentity to a hostname:{externalId}");
            }
        }
    }
}
