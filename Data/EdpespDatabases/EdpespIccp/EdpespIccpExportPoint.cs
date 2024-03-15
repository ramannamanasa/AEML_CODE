using System;
using System.Collections.Generic;
using System.Data;
using OSII.ConversionToolkit;
using OSII.ConversionToolkit.Generic;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespIccp
{
    class EdpespIccpExportPoint
    {
        private readonly ICCP _iccpDb;  // Locacl reference to the ICCP db.
        private readonly EdpespParser _parser;  // Local reference of the Parser object.
        private readonly List<(string, int)> _expDsList;  // Local reference of the EXPORT_DS list
        private readonly GenericTable _scadaXref;  // Local reference of SCADA xref

        /// <summary>
        /// Default constructor. Sets important references of variables.
        /// </summary>
        /// <param name="par">Current Parser object.</param>
        /// <param name="iccpDb">Current ICCP db object.</param>
        /// <param name="expDsList">Current EXPORT_DS list.</param>
        /// <param name="scadaXref">Scada XREF object.</param>
        public EdpespIccpExportPoint(EdpespParser par, ICCP iccpDb, List<(string, int)> expDsList, GenericTable scadaXref)
        {
            this._parser = par;
            this._iccpDb = iccpDb;
            this._expDsList = expDsList;
            this._scadaXref = scadaXref;
        }

        /// <summary>
        /// Function to convert all EXPORT_POINT objects.
        /// </summary>
        public void ConvertIccpExportPoint()
        {
            Logger.OpenXMLLog();

            DbObject exportPointObj = this._iccpDb.GetDbObject("ICCP_EXPORT_POINT");
            int exportPointRec = 0;

            // Sort XREF for later
            this._scadaXref.Sort("IRN");

            // Constant values from mapping
            const int DB_NUM = 10;
            const int FIELD_NUM = 20;
            const int STATECALC = 0;
            const int pSCANCLASS = 1;

            // Create IddVal dataview
            DataView iddValView = new DataView(this._parser.IddValTbl.DataTable)
            {
                Sort = "NAME, HOST"
            };

            foreach (DataRow exportPointRow in this._parser.IddValTbl.Rows)
            {
                // Check CLNT field, if equal to 0: convert as export point, else skip.
                if (!exportPointRow["CLNT"].ToString().Equals("0"))
                {
                    continue;
                }
                // if IDBTBL_IRN is 108144201, skip these points as the VCC isn't used.
                if (exportPointRow["CLNT"].ToString().Equals("0") && exportPointRow["IDBTBL_IRN"].ToString().Equals("108144201"))
                {
                    continue;
                }
                // if host_type is Measurand, check if that measurand point is skipped, if it is, skip this export point as well
                if (exportPointRow["HOST_TYPE"].ToString().Equals("MEASURAND") && EdpespIccpExtensions.SkipICCPPoint(this._scadaXref, exportPointRow["MEASURAND_IRN"].ToString()))
                {
                    continue;
                }
                // if host_type is Measurand, check if that measurand point is skipped, if it is, skip this export point as well
                if (exportPointRow["HOST_TYPE"].ToString().Equals("INDICATION") && EdpespIccpExtensions.SkipICCPPoint(this._scadaXref, exportPointRow["INDICATION_IRN"].ToString()))
                {
                    continue;
                }

                // Set Record and Name
                exportPointObj.CurrentRecordNo = ++exportPointRec;
                exportPointObj.SetValue("Name", exportPointRow["NAME"]);

                bool hasDuplicate = false;

                // Set Obj_Num and Rec_Key
                EdpespIccpExtensions.SetObjNum(exportPointObj, exportPointRow);
                EdpespIccpExtensions.SetRecKey(exportPointObj, exportPointRow, this._scadaXref, hasDuplicate, exportPointRow["IDBTBL_IRN"].ToString());

                // Set Type, pScanClass, and pExpDs
                EdpespIccpExtensions.SetType(exportPointObj, exportPointRow);
                exportPointObj.SetValue("pScanClass", pSCANCLASS);
                SetpExpDs(exportPointObj, exportPointRow);

                // Set Constant values
                exportPointObj.SetValue("DB_NUM", DB_NUM);
                exportPointObj.SetValue("FIELD_NUM", FIELD_NUM);
                exportPointObj.SetValue("StateCalc", STATECALC);

            }

            Logger.CloseXMLLog();
        }

        /// <summary>
        /// Helper function to set pExpDs for EXPORT_POINT objects.
        /// </summary>
        /// <param name="exportPointObj">Current EXPORT_POINT Object.</param>
        /// <param name="exportPointRow">Current DataRow row from input.</param>
        public void SetpExpDs(DbObject exportPointObj, DataRow exportPointRow)
        {
            string irn = exportPointRow["IDBTBL_IRN"].ToString();
            foreach ((string, int) expDs in this._expDsList)
            {
                if (expDs.Item1.Equals(irn))
                {
                    exportPointObj.SetValue("pExpDS", expDs.Item2);
                    return;
                }
            }
        }
    }
}
