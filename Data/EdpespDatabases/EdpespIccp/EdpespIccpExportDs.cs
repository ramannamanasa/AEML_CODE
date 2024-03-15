using System;
using System.Collections.Generic;
using System.Data;
using OSII.ConversionToolkit;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespIccp
{
    class EdpespIccpExportDs
    {
        private readonly ICCP _iccpDb;  // Locacl reference to the ICCP db.
        private readonly EdpespParser _parser;  // Local reference of the Parser object.
        private readonly List<(string, int)> _expDsList;  // Local reference of the EXPORT_DS list

        /// <summary>
        /// Default constructor. Sets important references of variables.
        /// </summary>
        /// <param name="par">Current Parser object.</param>
        /// <param name="iccpDb">Current ICCP db object.</param>
        /// <param name="expDsList">Current EXPORT_DS list.</param>
        public EdpespIccpExportDs(EdpespParser par, ICCP iccpDb, List<(string, int)> expDsList)
        {
            this._parser = par;
            this._iccpDb = iccpDb;
            this._expDsList = expDsList;
        }

        /// <summary>
        /// Function to convert all EXPORT_DS objects.
        /// </summary>
        public void ConvertIccpExportDs()
        {
            Logger.OpenXMLLog();

            DbObject exportDsObj = this._iccpDb.GetDbObject("ICCP_EXPORT_DS");
            int exportDsRec = 0;

            // Constant values from mapping
            const int STATECALC = 0;
            const int ANALOG_NEGATE = 0;

            foreach (DataRow exportDsRow in this._parser.IdbTbl.Rows)
            {
                // Skip this specific Export DS
                if (exportDsRow["CCTR"].Equals("EDPECC"))
                {
                    continue;
                }
                // Set record and name
                exportDsObj.CurrentRecordNo = ++exportDsRec;
                exportDsObj.SetValue("Name", exportDsRow["CCTR"]);

                // Set constants
                exportDsObj.SetValue("StateCalc", STATECALC);
                exportDsObj.SetValue("AnalogNegate", ANALOG_NEGATE);

                // Add to list
                this._expDsList.Add((exportDsRow["IRN"].ToString(), exportDsRec));
            } 

            Logger.CloseXMLLog();
        }
    }
}
