using OSII.ConversionToolkit;
using OSII.ConversionToolkit.Generic;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;
using System;
using System.Collections.Generic;
using System.Data;

namespace edpesp_db.Data.EdpespDatabases.EdpespFep
{
    class EdpespScanData
    {
        private readonly FEP _fepDb;  // Local reference of the Fep Database
        private readonly GenericTable _scadaToFepXref;  // Local reference to the Scada to Fep Xref

        /// <summary>
        /// Default constructor. Sets important local references.
        /// </summary>
        /// <param name="fepDb">Current FEP database object.</param>
        /// <param name="scadaToFepXref">Current SCADA to FEP xref object.</param>
        public EdpespScanData(FEP fepDb, GenericTable scadaToFepXref)
        {
            this._fepDb = fepDb;
            this._scadaToFepXref = scadaToFepXref;
        }

        /// <summary>
        /// Function to convert SCAN_DATA objects.
        /// </summary>
        public void ConvertScanData()
        {
            Logger.OpenXMLLog();

            Dictionary<string,string> ioakey = new Dictionary<string, string>();
            Dictionary<string, string> externid = new Dictionary<string, string>();
            List<string> ioa = new List<string>();
            foreach (DataRow scada in this._scadaToFepXref.Rows)
            {
                //ioa.Add((scada["pRTU"] + "," + scada["FEP Type"] +","+scada["Address"]+","+scada["IOA"]).ToString());
                try
                {
                    //    ioakey.Add(scada["SCADA Key"].ToString(), scada["IOA"].ToString());
                    //}
                    ioakey.Add((scada["pRTU"] + "," + scada["FEP Type"] + "," + scada["Address"].ToString()), scada["IOA"].ToString());
                    externid.Add((scada["pRTU"] + "," + scada["FEP Type"] + "," + scada["Address"].ToString()), scada["ExtrnID"].ToString());
                }
                catch(Exception ex)
                {
                    string d = scada["SCADA Key"].ToString();
                }
            }
            DbObject scanDataObj = this._fepDb.GetDbObject("SCAN_DATA");
            int scanDataRec;

            this._scadaToFepXref.Sort("pRtu, FEP Type, Address");
            //this._scadaToFepXref.Sort("pRtu, FEP Type, IOA");
            //this._scadaToFepXref.Sort("SCADA Key, pRtu, FEP Type");

            for (scanDataRec = 1; scanDataRec <= scanDataObj.RecordCount; scanDataRec++)
            {
                scanDataObj.CurrentRecordNo = scanDataRec;
                string pRtu = scanDataObj.GetValue("pRTU", 0);
                string key = scanDataObj.GetValue("Key", 0);
                string protocolType = scanDataObj.GetValue("ProtocolType", 0);
                string addr = scanDataObj.GetValue("PointAddress", 0);
                //if (pRtu !="1") break;//FEPCHECK
                //if (this._scadaToFepXref.TryGetRow(new[] { pRtu, protocolType, addr }, out DataRow xrefRow))
                // string value = "null";
                string compare = pRtu + "," + protocolType + ","+addr;
                //int index = ioa.FindIndex(a => a.StartsWith(compare));
                //if (index != -1)
                //{
                //    string[] value = ioa[index].Split(',');
                //    scanDataObj.SetValue("IntParms", value[3]);
                //    ioa.RemoveAt(index);
                //}

                if (ioakey.TryGetValue(compare, out string value))
                {
                    scanDataObj.SetValue("IntParms",0, value);
                }
                else
                {
                    string k = key;
                }
                if ((pRtu == "1" || pRtu =="184" || pRtu =="2") && value == "0")
                {
                    int t = 0;
                }
                if (externid.TryGetValue(compare, out string value1))
                {
                    scanDataObj.SetValue("Name", 0, value1);
                }
                //Set additional parameter: DestinationDatabase, fepType(status, Analog, accum), StateType(normal, Quad), DestinationObject
                //if (this._scadaToFepXref.TryGetRow(new[] { key, pRtu, protocolType }, out DataRow xrefRow))
                if (this._scadaToFepXref.TryGetRow(new[] { pRtu, protocolType, addr }, out DataRow xrefRow))
                {
                    scanDataObj.SetValue("DestinationKey", xrefRow["SCADA Key"]); 
                    
                    scanDataObj.SetValue("DestinationRecordHint", xrefRow["OSI Record"]);
                    scanDataObj.SetValue("DestinationDatabase", 0, "10");
                    
                    string feptype = xrefRow["Object"].ToString() == "STATUS"? "1": xrefRow["Object"].ToString() == "ANALOG" ? "3": xrefRow["Object"].ToString() == "Accum"? "5" : "1";
                    scanDataObj.SetValue("fepType", 0, feptype);
                    string obj = xrefRow["Object"].ToString() == "STATUS" ? "4" : xrefRow["Object"].ToString() == "ANALOG" ? "5" : xrefRow["Object"].ToString() == "Accum" ? "6" : "4";
                    scanDataObj.SetValue("DestinationObject", 0, obj);
                    string statetype = protocolType == "2" ? "1" : "0";
                    //scanDataObj.SetValue("StateType", 0, statetype);
                    //scanDataObj.SetValue("IntParms", xrefRow["IOA"]);
                }
                else
                {
                    Logger.Log("MISSING SCAN DATA LINK", LoggerLevel.INFO, $"Could not find scada point in xref with pRtu: {pRtu}\t protocolType: {protocolType}\t IOA: {addr}");
                }
            }

            Logger.CloseXMLLog();
        }
    }
}
