using System;
using System.Collections.Generic;
using System.Data;
using edpesp_db.Data;
using OSII.ConversionToolkit;
using OSII.ConversionToolkit.Extensions;
using OSII.ConversionToolkit.Generic;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;


namespace edpesp_db.Data.EdpespDatabases.EdpespScada
{
    class EdpespEquip
    {
        private readonly EdpespParser _parser;  // Local reference of the parser
        private readonly SCADA _scadaDb;  // Local reference of the Scada Database
        private readonly FEP _fepDb;  // Local reference of the Fep Database
        private readonly Dictionary<string, int> _stationDict;  // Local reference of the station Dict. 
        private readonly Dictionary<string, int> _switchDict;  // Local reference of the subsystem Dict. 
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
        private readonly Dictionary<string, string> _transEquipGorai;
        private readonly Dictionary<string, string> _transEquipAarey;
        private readonly Dictionary<string, string> _transEquipChemb;
        private readonly Dictionary<string, string> _transEquipSaki;
        private readonly Dictionary<string, string> _transEquipGhod;
        private readonly Dictionary<string, string> _transEquipVers;

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
        public EdpespEquip(EdpespParser par, SCADA scadaDb, Dictionary<string, int> switchDict, Dictionary<string, string> transEquipGorai, Dictionary<string, string> transEquipAarey,
             Dictionary<string, string> transEquipChemb, Dictionary<string, string> transEquipSaki, Dictionary<string, string> transEquipGhod, Dictionary<string, string> transEquipVers)
        {
            this._parser = par;
            this._scadaDb = scadaDb;
            this._switchDict = switchDict;
            this._transEquipGorai = transEquipGorai;
            this._transEquipAarey = transEquipAarey;
            this._transEquipChemb = transEquipChemb;
            this._transEquipSaki = transEquipSaki;
            this._transEquipGhod = transEquipGhod;
            this._transEquipVers = transEquipVers;


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
        public void ConvertEquip()
        {
            Logger.OpenXMLLog();

            DbObject equip = this._scadaDb.GetDbObject("EQUIP");
            //List<string> myaorlist = new List<string> { "AORGroup01", "AORGroup02", "AORGroup03", "AORGroup04", "AORGroup05" };
            //for(int i = 1; i<=  myaorlist.Count; i++)
            //{
            //    aorObj.SetValue("Name", i, 0, myaorlist[i-1]);
            // aorObj.SetValue("Indic", i, 0, 1);
            //    aorObj.SetValue("AORList", i, (i-1), 1);
            //}
            int aorRec = 1;
            List<string> equiplist = new List<string>();
            foreach (DataRow switchrow in this._parser.SwitchTbl.Rows)//BD
            {



                if (string.IsNullOrEmpty(switchrow["DESCRIPTIVE_LOC"].ToString()) || string.IsNullOrEmpty(switchrow["TAGNAM"].ToString())) continue;
                string name = switchrow["DESCRIPTIVE_LOC"].ToString() + "_" + switchrow["TAGNAM"].ToString();
                if (!equiplist.Contains(name))
                {
                    equip.CurrentRecordNo = equip.NextAvailableRecord;
                    equip.SetValue("Name", name);
                    equip.SetValue("Description", name);
                    //if (i == 1) equip.SetValue("ConfigVersion", i, 0, 1638033238757905);
                    //else equip.SetValue("ConfigVersion", i, 0, 1638352592475411);
                    if (!this._switchDict.ContainsKey(name))
                    {
                        this._switchDict.Add(name, equip.CurrentRecordNo);
                    }
                    equiplist.Add(name);
                }

            }

            foreach (DataRow TansRow in this._parser.TransmissionTbl.Rows)//RM
            {
                if (string.IsNullOrEmpty(TansRow["EQUIP"].ToString())) continue;

                string name = TansRow["EQUIP"].ToString();
                if (!equiplist.Contains(name))
                {
                    equip.CurrentRecordNo = equip.NextAvailableRecord;
                    equip.SetValue("Name", name);
                    equip.SetValue("Description", name);
                    //if (i == 1) equip.SetValue("ConfigVersion", i, 0, 1638033238757905);
                    //else equip.SetValue("ConfigVersion", i, 0, 1638352592475411);
                    if (!this._switchDict.ContainsKey(name))
                    {
                        this._switchDict.Add(name, equip.CurrentRecordNo);
                    }
                    equiplist.Add(name);
                }

            }
            foreach (DataRow Tans12Row in this._parser.Trans123Tbl.Rows)//RM
            { 
                if (string.IsNullOrEmpty(Tans12Row["SW"].ToString()) || string.IsNullOrEmpty(Tans12Row["EQUIP_NAME"].ToString())) continue;

                string transkey = Tans12Row["SW"].ToString();
                string transvalue = Tans12Row["EQUIP_NAME"].ToString();
                string IrnKey = Tans12Row["IRN"].ToString();

                if (IrnKey == "1431150241")
                {
                    if (!this._transEquipGorai.ContainsKey(transvalue))
                    {
                        this._transEquipGorai.Add(transkey, transvalue);
                    }
                }
                if (IrnKey == "138096201")
                {
                    if (!this._transEquipAarey.ContainsKey(transvalue))
                    {
                        this._transEquipAarey.Add(transkey, transvalue);
                    }
                }
                if (IrnKey == "1837192241")
                {
                    if (!this._transEquipChemb.ContainsKey(transvalue))
                    {
                        this._transEquipChemb.Add(transkey, transvalue);
                    }
                }
                if (IrnKey == "2093087241")
                {
                    if (!this._transEquipSaki.ContainsKey(transvalue))
                    {
                        this._transEquipSaki.Add(transkey, transvalue);
                    }
                }
                if (IrnKey == "101219201")// 101219201
                {
                    if (!this._transEquipGhod.ContainsKey(transvalue))
                    {
                        this._transEquipGhod.Add(transkey, transvalue);
                    }
                }
                if (IrnKey == "138097201")
                {
                    if (!this._transEquipVers.ContainsKey(transvalue))
                    {
                        this._transEquipVers.Add(transkey, transvalue);
                    }
                }


            }
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
