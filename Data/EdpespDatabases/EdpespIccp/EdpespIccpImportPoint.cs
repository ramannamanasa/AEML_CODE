using System;
using System.Collections.Generic;
using System.Data;
using OSII.ConversionToolkit;
using OSII.ConversionToolkit.Generic;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespIccp
{
    class EdpespIccpImportPoint
    {
        private readonly ICCP _iccpDb;  // Locacl reference to the ICCP db.
        private readonly EdpespParser _parser;  // Local reference of the Parser object.
        private readonly List<(string, int)> _impDsList;  // Local reference to the ImportDs list.
        private readonly GenericTable _scadaXref;  // Local reference of SCADA xref

        /// <summary>
        /// Default Constructor. Sets local references of important variables.
        /// </summary>
        /// <param name="par">Current Parser object.</param>
        /// <param name="iccpDb">Current ICCP db object.</param>
        /// <param name="impDsList">Current impDs list.</param>
        /// <param name="scadaXref">Scada XREF table.</param>
        public EdpespIccpImportPoint(EdpespParser par, ICCP iccpDb, List<(string, int)> impDsList, GenericTable scadaXref)
        {
            this._parser = par;
            this._iccpDb = iccpDb;
            this._impDsList = impDsList;
            this._scadaXref = scadaXref;
        }

        /// <summary>
        /// Function to convert all ICCP_IMPORT_POINT objects.
        /// </summary>
        public void ConvertIccpImportPoint()
        {
            Logger.OpenXMLLog();

            DbObject importPointObj = this._iccpDb.GetDbObject("ICCP_IMPORT_POINT");
            int importPointRec = 0;

            // Sort xref for later
            this._scadaXref.Sort("IRN");

            // Constant values from mapping
            const int DB_NUM = 10;
            const int FIELD_NUM = 20;

            // Sort IddValTbl and IddSetTbl for later
            this._parser.IddValTbl.Sort("NAME, IDBTBL_IRN");
            this._parser.IddsetTbl.Sort("IRN");

            // Create IddVal dataview
            DataView iddValView = new DataView(this._parser.IddValTbl.DataTable)
            {
                //Sort = "NAME, HOST"
                Sort = "NAME, IDBTBL_IRN" //BD
            };

            foreach (DataRow importPointRow in this._parser.IddRefTbl.Rows)
            {
                // Check for points to skip
                string name = importPointRow["DREF"].ToString();
                if(name == "KALWA2220SALSET3PMvMoment")
                {
                    int tt = 0;
                }
                if (name.Contains("Transfer_Set_"))
                {
                    continue;
                }
                // Find IRN and DataRow for cross reference
                string irn = importPointRow["IDDVAL_IRN"].ToString();
                string idbTblIrn = importPointRow["IDBTBL_IRN"].ToString();
                if (!this._parser.IddValTbl.TryGetRow(new[] { name, idbTblIrn }, out DataRow iddValRow))
                {
                    continue;
                }
                // if host_type is Measurand, check if that measurand point is skipped, if it is, skip this import point as well
                if (iddValRow["HOST_TYPE"].ToString().Equals("MEASURAND") && EdpespIccpExtensions.SkipICCPPoint(this._scadaXref, iddValRow["MEASURAND_IRN"].ToString()))
                {
                    continue;
                }
                // if host_type is Measurand, check if that measurand point is skipped, if it is, skip this import point as well
                if (iddValRow["HOST_TYPE"].ToString().Equals("INDICATION") && EdpespIccpExtensions.SkipICCPPoint(this._scadaXref, iddValRow["INDICATION_IRN"].ToString()))
                {
                    continue;
                }

                // Set Record and Name
                importPointObj.CurrentRecordNo = ++importPointRec;
                importPointObj.SetValue("Name", name);

                if (null != iddValRow)
                {
                    bool hasDuplicate = false;
                    // Check for multiple entries
                    if (iddValView.FindRows(new[] { iddValRow["NAME"].ToString(), iddValRow["HOST"].ToString() }).Length > 1)
                    {
                        hasDuplicate = true;
                    }
                    // Set Type, Obj_num, and Rec_key
                    EdpespIccpExtensions.SetType(importPointObj, iddValRow);
                    EdpespIccpExtensions.SetObjNum(importPointObj, iddValRow);
                    EdpespIccpExtensions.SetRecKey(importPointObj, iddValRow, this._scadaXref, hasDuplicate, idbTblIrn);
                }

                // Set pImpDs
                EdpespIccpExtensions.SetpImpDs(importPointObj, importPointRow["IDDSET_IRN"].ToString(), this._parser.IddsetTbl, this._impDsList);

                // Set Constant values
                importPointObj.SetValue("DB_NUM", DB_NUM);
                importPointObj.SetValue("FIELD_NUM", FIELD_NUM);
            }

            Logger.CloseXMLLog();
        }
    }
}
