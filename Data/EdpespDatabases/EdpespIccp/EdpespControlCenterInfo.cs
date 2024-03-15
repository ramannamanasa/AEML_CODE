using System;
using System.Collections.Generic;
using System.Data;
using OSII.ConversionToolkit;
using OSII.ConversionToolkit.Generic;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespIccp
{
    class EdpespControlCenterInfo
    {
        private readonly ICCP _iccpDb;  // Locacl reference to the ICCP db.
        private readonly EdpespParser _parser;  // Local reference of the Parser object.
        private readonly Dictionary<string, int> _controlCenterDict;  // Local reference to the Control Center dictionary.
        private readonly List<string> usedNames;  // Keeps track of names already used to avoid duplicates.

        /// <summary>
        /// Default constructor. Sets import 
        /// </summary>
        /// <param name="par"></param>
        /// <param name="iccpDb"></param>
        /// <param name="controlCenterDict"></param>
        public EdpespControlCenterInfo(EdpespParser par, ICCP iccpDb, Dictionary<string, int> controlCenterDict)
        {
            this._parser = par;
            this._iccpDb = iccpDb;
            this._controlCenterDict = controlCenterDict;
            usedNames = new List<string>();
        }

        /// <summary>
        /// Function to convert all CONTROL_CENTER_INFO objects.
        /// </summary>
        public void ConvertControlCenterInfo()
        {
            Logger.OpenXMLLog();

            DbObject controlCenterObj = this._iccpDb.GetDbObject("CONTROL_CENTER_INFO");
            int controlCenterRec = 0;

            // Sort tables for later
            this._parser.ParametrosTbl.Sort("Common_Name");
            this._parser.IdassnTbl.Sort("ARNM");
            this._parser.IdbTbl.Sort("IRN");
            
            foreach (DataRow controlMapRow in this._parser.ControlCenterMapTbl.Rows)
            {
                // Get the name of the control center and check if its already been converted.
                string cctr = controlMapRow["CCTR"].ToString();
                if (usedNames.Contains(cctr))
                {
                    continue;
                }
                List<DataRow> hosts = GetAllHosts(cctr);

                // Get cross reference rows from input files. Do not convert point if either of these rows are null.
                DataRow controlCenterRow = GetXTableRow(this._parser.ParametrosTbl, controlMapRow["Common_Name"].ToString());
                if (null == controlCenterRow)
                {
                    continue;
                }
                DataRow idassnRow = GetXTableRow(this._parser.IdassnTbl, controlCenterRow["Common_Name"].ToString());
                if (null == idassnRow)
                {
                    continue;
                }

                // Set Record and Name
                controlCenterObj.CurrentRecordNo = ++controlCenterRec;
                controlCenterObj.SetValue("Name", cctr);

                // Set IP Addresses and Host Names
                foreach (DataRow hostRow in hosts)
                {
                    SetIPAddress(controlCenterObj, hostRow["IP_Addr"].ToString());
                }
                foreach(DataRow hostRow in hosts)
                {
                    SetHostNames(controlCenterObj, hostRow["Common_Name"].ToString());
                }

                // Set PSEL, SSEL, and TSEL
                controlCenterObj.SetValue("PSEL", controlCenterRow["PSEL"]);
                controlCenterObj.SetValue("SSEL", controlCenterRow["SSEL"]);
                controlCenterObj.SetValue("TSEL", controlCenterRow["TSEL"]);

                // port, AP_Title, and AE_Qualifier
                controlCenterObj.SetValue("port", idassnRow["SFTS"]);
                controlCenterObj.SetValue("AP_Title", idassnRow["ARAP"]);
                controlCenterObj.SetValue("AE_Qualifier", idassnRow["ARAE"]);

                // Add to dictionary and list
                this._controlCenterDict.Add(idassnRow["IRN"].ToString(), controlCenterRec);
                usedNames.Add(cctr);
            }

            // Add extra ControlCenterInfo objects from input.
            foreach (DataRow extraControlCenRow in this._parser.ControlCenterInfoExtra.Rows)
            {
                // Set record and name
                controlCenterObj.CurrentRecordNo = ++controlCenterRec;
                controlCenterObj.SetValue("Name", extraControlCenRow["Name"]);

                // Set IP Addresses and Host Names
                controlCenterObj.SetValue("IPAddress1", extraControlCenRow["IPAddress1"]);
                controlCenterObj.SetValue("Hostname1", extraControlCenRow["Hostname1"]);

                // Set PSEL, SSEL, and TSEL
                controlCenterObj.SetValue("PSEL", extraControlCenRow["PSEL"]);
                controlCenterObj.SetValue("SSEL", extraControlCenRow["SSEL"]);
                controlCenterObj.SetValue("TSEL", extraControlCenRow["TSEL"]);

                // port, AP_Title, and AE_Qualifier
                controlCenterObj.SetValue("port", extraControlCenRow["Port"]);
                controlCenterObj.SetValue("AP_Title", extraControlCenRow["AP_Title"]);
                controlCenterObj.SetValue("AE_Qualifier", extraControlCenRow["AE_Qualifier"]);
            }

            Logger.CloseXMLLog();
        }

        /// <summary>
        /// Helper function to get all hosts of a control center.
        /// </summary>
        /// <param name="cctr">Current name of control center.</param>
        /// <returns>List of hosts' rows.</returns>
        public List<DataRow> GetAllHosts(string cctr)
        {
            List<DataRow> hosts = new List<DataRow>();
            foreach (DataRow controlMapRow in this._parser.ControlCenterMapTbl.Rows)
            {
                if (controlMapRow["CCTR"].ToString().Equals(cctr))
                {
                    hosts.Add(controlMapRow);
                }
            }
            return hosts;
        }

        /// <summary>
        /// Helper function to set IP Addresses of current controlCenterObj.
        /// </summary>
        /// <param name="controlCenterObj">Current Control Center object.</param>
        /// <param name="addr">Current Address.</param>
        public void SetIPAddress(DbObject controlCenterObj, string addr)
        {
            if (controlCenterObj.GetValue("IPAddress1", 0).Equals("\"\""))
            {
                controlCenterObj.SetValue("IPAddress1", addr);
            }
            else if (controlCenterObj.GetValue("IPAddress2", 0).Equals("\"\""))
            {
                controlCenterObj.SetValue("IPAddress2", addr);
            }
            else if (controlCenterObj.GetValue("IPAddress3", 0).Equals("\"\""))
            {
                controlCenterObj.SetValue("IPAddress3", addr);
            }
            else
            {
                Logger.Log("Out of IP Adresses", LoggerLevel.INFO, "All 3 IP Address fields were full. One or more addresses not set.");
            }
        }

        /// <summary>
        /// Helper function to set hostnames of current controlCenterObj.
        /// </summary>
        /// <param name="controlCenterObj">Current Control Center object.</param>
        /// <param name="name">Current hostname.</param>
        public void SetHostNames(DbObject controlCenterObj, string name)
        {
            if (controlCenterObj.GetValue("Hostname1", 0).Equals("\"\""))
            {
                controlCenterObj.SetValue("Hostname1", name);
            }
            else if (controlCenterObj.GetValue("Hostname2", 0).Equals("\"\""))
            {
                controlCenterObj.SetValue("Hostname2", name);
            }
            else if (controlCenterObj.GetValue("Hostname3", 0).Equals("\"\""))
            {
                controlCenterObj.SetValue("Hostname3", name);
            }
            else
            {
                Logger.Log("Out of hostnames", LoggerLevel.INFO, "All 3 hostnames fields were full. One or more hostnames not set.");
            }
        }
        
        /// <summary>
        /// Helper function to find a cross referenced DataRow. Sort table by column desired to be searched BEFORE calling this.
        /// </summary>
        /// <param name="tableToSearch">GenericTable object to be searched.</param>
        /// <param name="valueToMatch">String value to search in table.</param>
        /// <returns></returns>
        public DataRow GetXTableRow(GenericTable tableToSearch, string valueToMatch)
        {
            if (tableToSearch.TryGetRow(new[] { valueToMatch }, out DataRow rowOut))
            {
                return rowOut;
            }
            else
            {
                Logger.Log("NO ROW MATCH", LoggerLevel.INFO, $"No row match for table: {tableToSearch} \t and Value: {valueToMatch} \t Control Center Info point will not be created.");
                return null;
            }
        }
    }
}
