using System;
using System.Collections.Generic;
using System.Data;
using OSII.ConversionToolkit;
using OSII.ConversionToolkit.Extensions;
using OSII.ConversionToolkit.Generic;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespScada
{
    class EdpespAor
    {
        private readonly EdpespParser _parser;  // Local reference of the parser
        private readonly SCADA _scadaDb;  // Local reference of the Scada Database
        private readonly FEP _fepDb;  // Local reference of the Fep Database
        private readonly Dictionary<string, int> _stationDict;  // Local reference of the station Dict. 
        private readonly Dictionary<string, int> _DivaorDict;  // Local reference of the subsystem Dict. 
        private readonly Dictionary<string, string> _RtuIrnDivDict;  // Local reference of the subsystem Dict. 
        private readonly Dictionary<string, string> _areaDict;  // Local reference of the subsystem Dict. 
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
        public EdpespAor(EdpespParser par, SCADA scadaDb, Dictionary<string, int> DivAorDict, Dictionary<string, int> AreaDict, Dictionary<string, string> RtuIrnDivDict)
        {
            this._parser = par;
            this._scadaDb = scadaDb;
            this._DivaorDict = DivAorDict;
            this._RtuIrnDivDict = RtuIrnDivDict;
            
            //this._fepDb = fepDb;
            //this._stationDict = stationDict;
            //this._unitDict = unitDict;
            //this._scadaXref = scadaXref;
            //this._alarmsDict = alarmsDict;
            //this._rtuDataDict = rtuDataDict;
            //this._rtuIoaDict = rtuIoaDict;
            //this._deviceRecDict = deviceRecDict;
            //this._measScales = measScales;
            //this._scadaToFepXref = scadaToFepXref;
            //primarySourceDict = new Dictionary<string, int>();
            //secondarySourceDict = new Dictionary<int, string>();
        }

        /// <summary>
        /// Function to convert all ANALOG objects.
        /// </summary>
        public void ConvertAor()
        {
            Logger.OpenXMLLog();

            DbObject aorObj = this._scadaDb.GetDbObject("AOR_GROUP");
            //List<string> myaorlist = new List<string> { "AORGroup01", "AORGroup02", "AORGroup03", "AORGroup04", "AORGroup05" };
            //for(int i = 1; i<=  myaorlist.Count; i++)
            //{
            //    aorObj.SetValue("Name", i, 0, myaorlist[i-1]);
            // aorObj.SetValue("Indic", i, 0, 1);
            //    aorObj.SetValue("AORList", i, (i-1), 1);
            //}
            int aorRec = 1;

            //foreach (DataRow aor in this._parser.SubSystemTbl.Rows)//BD
            //{
            //    if (!(aor["SUBSYSTEM_TEXT"].ToString().Contains("Zone") || aor["SUBSYSTEM_TEXT"].ToString().Contains("Power Subsys") || aor["SUBSYSTEM_TEXT"].ToString()==("Transmission") || aor["SUBSYSTEM_TEXT"].ToString().Contains("Control Subsys")))
            //        continue;
            //    aorObj.CurrentRecordNo = aorRec++;

            //    aorObj.SetValue("Name", aor["SUBSYSTEM_TEXT"].ToString());
            //    aorObj.SetValue("Indic", 1);
            //    aorObj.SetValue("AORList",  (aorRec - 1), 1);
            //    string subsystemIRN = aor["IRN"].ToString();
            //    if (!this._aorDict.ContainsKey(subsystemIRN))
            //    {
            //        this._aorDict.Add(subsystemIRN, aorObj.CurrentRecordNo);
            //    }
            //}
            List<string> aorlist = new List<string>();
            foreach (DataRow aor in this._parser.AorNewTbl.Rows)//BD
            {
                //if (!(aor["SUBSYSTEM_TEXT"].ToString().Contains("Zone") || aor["SUBSYSTEM_TEXT"].ToString().Contains("Power Subsys") || aor["SUBSYSTEM_TEXT"].ToString() == ("Transmission") || aor["SUBSYSTEM_TEXT"].ToString().Contains("Control Subsys")))
                //    continue;
                if (new List<string> { "138093201", "222452251", "1798949251", "2089811251", "1972835251", "1995038251", "2016324251", "2016325251", "1795852251", "2009856251", "2073195251", "1992663251", "2026310251", "206805261", "2020391251", "2043217251", "2042422251", "2076307251" }.Contains(aor["IRN"].ToString()))
                {
                    //Logger.Log("StatusSkippedPoints", LoggerLevel.INFO, $"STATUS: Belongs to Triangle gateway. ExID: {statusRow["EXTERNAL_IDENTITY"].ToString()}\t ");
                    continue;

                }
                if(aor["IRN"].ToString() == "223512261" || aor["IRN"].ToString() == "2152677546")
                {
                    int t = 0;
                }
                if (string.IsNullOrEmpty(aor["EXTERNAL_IDENTITY"].ToString()) || aor["DIVISION"].ToString().StartsWith("1S-MH-MU") || aor["DIVISION"].ToString().StartsWith("#N/A")) continue;
                if (!aorlist.Contains(aor["DIVISION"].ToString()) )
                {

                    aorlist.Add(aor["DIVISION"].ToString());
                    aorObj.CurrentRecordNo = aorRec++;

                    aorObj.SetValue("Name", aor["DIVISION"].ToString());
                    aorObj.SetValue("Indic", 1);
                    aorObj.SetValue("AORList", (aorRec - 2), 1);
                    string rtuIRN = aor["IRN"].ToString();
                    
                    if (!this._DivaorDict.ContainsKey(aor["DIVISION"].ToString()))
                    {
                        this._DivaorDict.Add(aor["DIVISION"].ToString(), aorObj.CurrentRecordNo);
                    }
                    
                }
                if (!this._RtuIrnDivDict.ContainsKey(aor["IRN"].ToString()))
                {
                    this._RtuIrnDivDict.Add(aor["IRN"].ToString(), aor["DIVISION"].ToString());
                }
            }
            aorObj.CurrentRecordNo = aorObj.NextAvailableRecord;
            aorObj.SetValue("Name", "System_Calc");
            aorObj.SetValue("Indic", 1);
            aorObj.SetValue("AORList", (aorObj.CurrentRecordNo - 1), 1);
            this._DivaorDict.Add("System_Calc", aorObj.CurrentRecordNo);
            this._RtuIrnDivDict.Add("System_Calc", "System_Calc");

            aorObj.CurrentRecordNo = aorObj.NextAvailableRecord;
            aorObj.SetValue("Name", "DMS");
            aorObj.SetValue("Indic", 1);
            aorObj.SetValue("AORList", (aorObj.CurrentRecordNo - 1), 1);
            this._DivaorDict.Add("DMS", aorObj.CurrentRecordNo);
            this._RtuIrnDivDict.Add("DMS", "DMS");

            aorObj.CurrentRecordNo = aorObj.NextAvailableRecord;
            aorObj.SetValue("Name", "Control");
            aorObj.SetValue("Indic", 1);
            aorObj.SetValue("AORList", (aorObj.CurrentRecordNo - 1), 1);
            this._DivaorDict.Add("Control", aorObj.CurrentRecordNo);
            this._RtuIrnDivDict.Add("Control", "Control");
            //for (int i =1 ; i < 6; i++)
            //{
            //    if (i < 6)
            //    {
            //        switch (i)
            //        {


            //            case 1:
            //                aorObj.SetValue("Name",i,0,"AORGroup01");
            //                aorObj.SetValue("Indic",i,0,1);
            //                for (int j = 0; j < 37; j++)
            //                {
            //                    aorObj.SetValue("AORList", i, j, 1);
            //                    if (j == 0) { aorObj.SetValue("AORList", i, j, 1); }
            //                    else aorObj.SetValue("AORList", i, j, 0);
            //                }
            //                break;
            //            case 2:
            //                aorObj.SetValue("Name", i, 0, "AORGroup02");
            //                aorObj.SetValue("Indic", i, 0, 1);

            //                for (int j = 0; j < 37; j++)
            //                    if (j == 1) { aorObj.SetValue("AORList", i, j, 1); }
            //                    else aorObj.SetValue("AORList", i, j, 0);
            //                break;
            //            case 3:
            //                aorObj.SetValue("Name", i, 0, "AORGroup03");
            //                aorObj.SetValue("Indic", i, 0, 1);

            //                for (int j = 0; j < 37; j++)
            //                    if (j == 2) { aorObj.SetValue("AORList", i, j, 1); }
            //                    else aorObj.SetValue("AORList", i, j, 0);
            //                break;
            //            case 4:
            //                aorObj.SetValue("Name", i, 0, "AORGroup04");
            //                aorObj.SetValue("Indic", i, 0, 1);

            //                for (int j = 0; j < 37; j++)
            //                    if (j == 3) { aorObj.SetValue("AORList", i, j, 1); }
            //                    else aorObj.SetValue("AORList", i, j, 0);
            //                break;
            //            case 5:
            //                aorObj.SetValue("Name", i, 0, "AORGroup05");
            //                aorObj.SetValue("Indic", i, 0, 1);

            //                for (int j = 0; j < 37; j++)
            //                    if (j == 4) { aorObj.SetValue("AORList", i, j, 1); }
            //                    else aorObj.SetValue("AORList", i, j, 0);
            //                break;


            //        }
            //    }
            //}
            //for (int k = 1; k < 256; k++)
            //{
            //    for (int j = 38; j < 127; j++)
            //    {
            //        aorObj.SetValue("AORList", k, j, 0);
            //    }
            //}



            Logger.CloseXMLLog();
        }

        /// <summary>
        /// Helper function to get get Type.
        /// </summary>
        /// <param name="aorRow">Current DataRow row.</param>
        /// <returns></returns>
    }
}