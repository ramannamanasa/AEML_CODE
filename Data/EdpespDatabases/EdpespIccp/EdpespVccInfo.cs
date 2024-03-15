using System;
using System.Collections.Generic;
using System.Data;
using OSII.ConversionToolkit;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespIccp
{
    class EdpespVccInfo
    {
        private readonly ICCP _iccpDb;  // Locacl reference to the ICCP db.
        private readonly EdpespParser _parser;  // Local reference of the Parser object.
        private readonly Dictionary<string, int> _controlCenterDict;  // Local reference to the Control Center dictionary.
        private readonly List<(string, int)> _impDsList;  // Local reference to the ImportDs list.

        /// <summary>
        /// Default Converter. Sets local references of important variables.
        /// </summary>
        /// <param name="par">Current Parser object.</param>
        /// <param name="iccpDb">Current ICCP db object.</param>
        /// <param name="controlCenterDict">Control Center dictionary.</param>
        /// <param name="impDsDict">ImportDs Dictionary.</param>
        public EdpespVccInfo(EdpespParser par, ICCP iccpDb, Dictionary<string, int> controlCenterDict, List<(string, int)> impDsList)
        {
            this._parser = par;
            this._iccpDb = iccpDb;
            this._controlCenterDict = controlCenterDict;
            this._impDsList = impDsList;
        }

        /// <summary>
        /// Function to convert all VCC_INFO objects.
        /// </summary>
        public void ConvertVccInfo()
        {
            Logger.OpenXMLLog();

            DbObject vccInfoObj = this._iccpDb.GetDbObject("VCC_INFO");
            int vccInfoRec = 0;

            // Sort IddSetTbl and IdassnTbl for later
            this._parser.IdintrTbl.Sort("IDBTBL_IRN");
            this._parser.IdassnTbl.Sort("IDBTBL_IRN");

            // Constant Values from mapping
            const long LOCAL_FEATURES = 11001000000;
            const int EXPORT_POINT_DOMAIN = 1;
            const int IMPORT_POINT_DOMAIN = 1;
            const int OUT_CTRL_DOMAIN = 1;
            const int IN_CTRL_DOMAIN = 1;
            const int IMP_STATE_CALC = 0;
            const int IMP_ANALOG_NEGATE = 0;
            const int EXP_STATE_CALC = 0;
            const int EXP_ANALOG_NEGATE = 0;
            const int QUALITY_MAPPING = 0;

            foreach (DataRow vccInfoRow in this._parser.IdbTbl.Rows)
            {
                // Skip this specific VCC
                if (vccInfoRow["CCTR"].Equals("EDPECC"))
                {
                    continue;
                }
                // Set Record and Name
                vccInfoObj.CurrentRecordNo = ++vccInfoRec;
                vccInfoObj.SetValue("Name", vccInfoRow["CCTR"]);

                // Set Local_DomainName and Local_Bilateral_ID
                vccInfoObj.SetValue("Local_DomainName", vccInfoRow["DOMN"]);
                vccInfoObj.SetValue("Local_Bilateral_ID", vccInfoRow["BTID"]);

                // Set Remote_DomainName and Remote_Bilateral_ID 
                vccInfoObj.SetValue("RemoteDomainName", vccInfoRow["RDOM"]);
                vccInfoObj.SetValue("Remote_Bilateral_ID", vccInfoRow["RTID"]);

                // Set Local_Version_Major and Local_Version_Minor
                vccInfoObj.SetValue("Local_Version_Major", vccInfoRow["IVMJ"]);
                vccInfoObj.SetValue("Local_Version_Minor", vccInfoRow["IVMN"]);

                // Set Remote_Version_Major and Remote_Version_Minor
                vccInfoObj.SetValue("Remote_Version_Major", vccInfoRow["IVMJ"]);
                vccInfoObj.SetValue("Remote_Version_Minor", vccInfoRow["IVMN"]);

                // Set pRemoteControlCenter, pLocalControlCenter, pImpDs, and pExpDs
                SetpRemoteControlCenter(vccInfoObj, vccInfoRow);
                SetpLocalControlCenter(vccInfoObj);
                SetVccInfopImpDs(vccInfoObj, vccInfoRow["CCTR"].ToString());
                vccInfoObj.SetValue("pExpDS", vccInfoRec);

                // Set Constant Values
                vccInfoObj.SetValue("Local_Features", LOCAL_FEATURES);
                vccInfoObj.SetValue("ExportPointDomain", EXPORT_POINT_DOMAIN);
                vccInfoObj.SetValue("ImportPointDomain", IMPORT_POINT_DOMAIN);
                vccInfoObj.SetValue("OutCtrlDomain", OUT_CTRL_DOMAIN);
                vccInfoObj.SetValue("InCtrlDomain", IN_CTRL_DOMAIN);
                vccInfoObj.SetValue("ImpStateCalc", IMP_STATE_CALC);
                vccInfoObj.SetValue("ImpAnalogNegate", IMP_ANALOG_NEGATE);
                vccInfoObj.SetValue("ExpStateCalc", EXP_STATE_CALC);
                vccInfoObj.SetValue("ExpAnalogNegate", EXP_ANALOG_NEGATE);
                vccInfoObj.SetValue("QualityMapping", QUALITY_MAPPING);
            }

            Logger.CloseXMLLog();
        }

        /// <summary>
        /// Helper function to set pRemoteControlCenter.
        /// </summary>
        /// <param name="vccInfoObj">Current VCC_INFO object.</param>
        /// <param name="vccInfoRow">Current DataRow from input.</param>
        public void SetpRemoteControlCenter(DbObject vccInfoObj, DataRow vccInfoRow)
        {
            string irn = vccInfoRow["IRN"].ToString();
            // Find cross reference in the idassn
            if (!this._parser.IdassnTbl.TryGetRow(new[] { irn }, out DataRow idassnRow))
            {
                Logger.Log("MISSING pRemoteControlCenter", LoggerLevel.INFO, $"In Table idassn, pRemoteControlCenter not found with IRN: {irn}");
                return;
            }

            if (this._controlCenterDict.TryGetValue(idassnRow["IRN"].ToString(), out int pRemoteControlCenter))
            {
                vccInfoObj.SetValue("pRemoteControlCenter", pRemoteControlCenter);
            }
            else
            {
                Logger.Log("MISSING pRemoteControlCenter", LoggerLevel.INFO, $"In dictionary, pRemoteControlCenter not found with IRN: {idassnRow["IRN"]}");
            }
        }

        /// <summary>
        /// Helper function to set pImpDs for VCC_INFO objects.
        /// These objects require different logic than the function in EdpespIccpExtensions.
        /// </summary>
        /// <param name="vccInfoObj">Current VCC_INFO object.</param>
        /// <param name="name">Name of the current VCC_INFO object.</param>
        public void SetVccInfopImpDs(DbObject vccInfoObj, string name)
        {
            int pImpDsRec = 0;
            foreach ((string, int) impDs in this._impDsList)
            {
                string impDsName = impDs.Item1;
                int impDsRec = impDs.Item2;
                // I really wish I didn't have to do it this way, but it is the only logic they will give me that works.
                switch (name)
                {
                    case "HCGMS":
                        if (impDsName.StartsWith("DMS"))
                        {
                            vccInfoObj.SetValue("pImpDS", pImpDsRec, impDsRec);
                            ++pImpDsRec;
                        }
                        break;
                    case "EDPR":
                        if (impDsName.StartsWith("EDPR"))
                        {
                            vccInfoObj.SetValue("pImpDS", pImpDsRec, impDsRec);
                            ++pImpDsRec;
                        }
                        break;
                    case "REE_CECOEL":
                        if (impDsName.Contains("CECOEL"))
                        {
                            vccInfoObj.SetValue("pImpDS", pImpDsRec, impDsRec);
                            ++pImpDsRec;
                        }
                        break;
                    case "REE_CECORE":
                        if (impDsName.Contains("CECORE"))
                        {
                            vccInfoObj.SetValue("pImpDS", pImpDsRec, impDsRec);
                            ++pImpDsRec;
                        }
                        break;
                    default:
                        Logger.Log("UNHANDLED VCC INFO", LoggerLevel.INFO, $"VCC_INFO not mapped to any pImpDs in mapping document: {name}");
                        break;

                }
            }
        }

        /// <summary>
        /// Helper function to set the pLocalControlCenter fields.
        /// </summary>
        /// <param name="vccInfoObj">Current VCC_INFO Object.</param>
        public void SetpLocalControlCenter(DbObject vccInfoObj)
        {
            DbObject controlCenterObj = this._iccpDb.GetDbObject("CONTROL_CENTER_INFO");
            int iccp1Rec = 0;
            int iccp2Rec = 0;
            int biccp1Rec = 0;
            int biccp2Rec = 0;
            // Find records of control centers with specific names.
            foreach (int rec in controlCenterObj.UsedRecords)
            {
                controlCenterObj.CurrentRecordNo = rec;
                string hostname = controlCenterObj.GetValue("Hostname1", 0);
                switch (hostname)
                {
                    case "iccp1":
                        iccp1Rec = rec;
                        break;
                    case "iccp2":
                        iccp2Rec = rec;
                        break;
                    case "biccp1":
                        biccp1Rec = rec;
                        break;
                    case "biccp2":
                        biccp2Rec = rec;
                        break;
                }
            }
            // Set the pRecords
            vccInfoObj.SetValue("pLocalControlCenter", 0, iccp1Rec);
            vccInfoObj.SetValue("pLocalControlCenter", 1, iccp2Rec);
            vccInfoObj.SetValue("pLocalControlCenter", 2, biccp1Rec);
            vccInfoObj.SetValue("pLocalControlCenter", 3, biccp2Rec);
        }
    }
}
