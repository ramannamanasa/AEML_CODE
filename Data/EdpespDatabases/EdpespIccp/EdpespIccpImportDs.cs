using System;
using System.Collections.Generic;
using System.Data;
using OSII.ConversionToolkit;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespIccp
{
    class EdpespIccpImportDs
    {
        private readonly ICCP _iccpDb;  // Locacl reference to the ICCP db.
        private readonly EdpespParser _parser;  // Local reference of the Parser object.
        private readonly List<(string, int)> _impDsList;  // Local reference to the ImportDs list.
        private readonly List<string> usedNames;  // List to keep track of used impDsNames

        /// <summary>
        /// Enums for IMPORT_DS type field.
        /// </summary>
        private enum ImportDsType
        {
            EXCEPTIONS = 1,
            PERIODIC = 0
        }

        /// <summary>
        /// Default Constructor. Sets local references of important variables.
        /// </summary>
        /// <param name="par">Current Parser object.</param>
        /// <param name="iccpDb">Current ICCP db object.</param>
        /// <param name="impDsList">Current impDs List.</param>
        public EdpespIccpImportDs(EdpespParser par, ICCP iccpDb, List<(string, int)> impDsList)
        {
            this._parser = par;
            this._iccpDb = iccpDb;
            this._impDsList = impDsList;
            usedNames = new List<string>();
        }
        
        /// <summary>
        /// Function to set all IMPORT_DS objects.
        /// </summary>
        public void ConvertIccpImportDs()
        {
            Logger.OpenXMLLog();

            DbObject importDsObj = this._iccpDb.GetDbObject("ICCP_IMPORT_DS");
            int importDsRec = 0;

            // Constant values from mapping
            const int TIMEOUT = 30;
            const int STATECALC = 0;
            const int ANALOG_NEGATE = 0;

            foreach (DataRow importDsRow in this._parser.IdintrTbl.Rows)
            {
                // Check if name has already been converted. Skip if true
                if (usedNames.Contains((importDsRow["NAME"].ToString())))
                { 
                    continue;
                }

                // Set Record and Name
                importDsObj.CurrentRecordNo = ++importDsRec;
                importDsObj.SetValue("Name", importDsRow["NAME"]);

                // Set Type and IntegrityFlag
                SetType(importDsObj, importDsRow);
                SetIntegrityFlag(importDsObj, importDsRow);

                // Set Interval and IntegrityInterval
                importDsObj.SetValue("Interval", importDsRow["PAR_0"]);
                importDsObj.SetValue("IntegrityInterval", importDsRow["PAR_3"]);

                // Set BufferTime and AllChangesReported
                importDsObj.SetValue("BufferTime", importDsRow["PAR_0"]);
                SetAllChangesReported(importDsObj, importDsRow);

                // Set Constant values
                importDsObj.SetValue("Timeout", TIMEOUT);
                importDsObj.SetValue("StateCalc", STATECALC);
                importDsObj.SetValue("AnalogNegate", ANALOG_NEGATE);

                // Add to impDs list and usedNames
                this._impDsList.Add((importDsRow["NAME"].ToString(), importDsRec));
                usedNames.Add((importDsRow["NAME"].ToString()));
            }

            Logger.CloseXMLLog();
        }

        /// <summary>
        /// Helper function to set type of IMPORT_DS object.
        /// </summary>
        /// <param name="importDsObj">Current IMPORT_DS object.</param>
        /// <param name="importDsRow">Current DataRow row from input file.</param>
        public void SetType(DbObject importDsObj, DataRow importDsRow)
        {
            if (importDsRow["ALLC"].ToString().Equals("1"))
            {
                importDsObj.SetValue("Type", (int)ImportDsType.EXCEPTIONS);
            }
            else
            {
                importDsObj.SetValue("Type", (int)ImportDsType.PERIODIC);
            }
        }

        /// <summary>
        /// Helper function to set IntegrityFlag of IMPORT_DS object.
        /// </summary>
        /// <param name="importDsObj">Current IMPORT_DS object.</param>
        /// <param name="importDsRow">Current DataRow row from input file.</param>
        public void SetIntegrityFlag(DbObject importDsObj, DataRow importDsRow)
        {
            if (importDsRow["ALLC"].ToString().Equals("1"))
            {
                importDsObj.SetValue("IntegrityFlag", 1);  // Set to ON
            }
            else
            {
                importDsObj.SetValue("IntegrityFlag", 0);  // Set to OFF
            }
        }

        /// <summary>
        /// Helper function to set AllChangesReported of IMPORT_DS object.
        /// </summary>
        /// <param name="importDsObj">Current IMPORT_DS object.</param>
        /// <param name="importDsRow">Current DataRow row from input file.</param>
        public void SetAllChangesReported(DbObject importDsObj, DataRow importDsRow)
        {
            if (!importDsRow["PAR_2"].ToString().Equals("0"))
            {
                importDsObj.SetValue("AllChangesReported", 1);  // Set to ON
            }
            else
            {
                importDsObj.SetValue("AllChangesReported", 0);  // Set to OFF
            }
        }
    }
}
