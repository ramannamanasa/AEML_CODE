using System;
using System.Collections.Generic;
using System.Data;
using OSII.ConversionToolkit;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespScada
{
    class EdpespUnit
    {
        private readonly EdpespParser _parser;  // Local reference of the parser
        private readonly SCADA _scadaDb;  // Local reference of the Scada Database
        private readonly Dictionary<string, int> _unitDict;  // Local reference of the unit Dict. 
        public int unitRec = 0;  // Current UNIT record

        /// <summary>
        /// Default constructor.
        /// Sets local references of important values.
        /// </summary>
        /// <param name="par">Current parser object.</param>
        /// <param name="scadaDb">Current Scada db.</param>
        /// <param name="unitDict">Unit dictionary object.</param>
        public EdpespUnit(EdpespParser par, SCADA scadaDb, Dictionary<string, int> unitDict)
        {
            this._parser = par;
            this._scadaDb = scadaDb;
            this._unitDict = unitDict;
        }

        /// <summary>
        /// Function to convert all UNIT objects.
        /// </summary>
        public void ConvertUnit()
        {
            Logger.OpenXMLLog();

            DbObject unitObj = this._scadaDb.GetDbObject("UNITS");

            // Convert all Units in the Analog table
            foreach (DataRow unitRow in this._parser.MeasurandTbl.Rows)
            {
                // Check for blank names or 0s
                string name = unitRow["ENGINEERING_UNIT"].ToString();
                if (string.IsNullOrEmpty(name) || name.Equals("0"))
                {
                    continue;
                }
                CreateUnit(unitObj, name);
            }

            // Convert all units in the Accumulator table
            //SA:20211124 We don't have accumulator input files
            //foreach (DataRow unitRow in this._parser.AccumulatorTbl.Rows)
            //{
            //    // Check for blank names or 0s
            //    string name = unitRow["ENGINEERING_UNIT"].ToString();
            //    if (string.IsNullOrEmpty(name) || name.Equals("0"))
            //    {
            //        continue;
            //    }
            //    CreateUnit(unitObj, name);
            //}

            // Create a custom unit object
            CreateUnit(unitObj, "s");

            Logger.CloseXMLLog();
        }

        /// <summary>
        /// Helper function to actually set values and add to dictionary.
        /// </summary>
        /// <param name="unitObj">Current UNIT object.</param>
        /// <param name="name">Current name of UNIT.</param>
        public void CreateUnit(DbObject unitObj, string name)
        {
            // Set Record and Name if not present.
            if (!this._unitDict.ContainsKey(name))
            {
                unitObj.CurrentRecordNo = ++unitRec;
                unitObj.SetValue("Name", name);
                this._unitDict.Add(name, unitRec);
            }
        }
    }
}
