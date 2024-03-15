using System;
using System.Collections.Generic;
using System.Data;
using OSII.ConversionToolkit;
using OSII.ConversionToolkit.Extensions;
using OSII.ConversionToolkit.Generic;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespOpenCalc
{
    class EdpespFormula
    {
        private readonly EdpespParser _parser;  // Local referen of Parser object
        private readonly OpenCalc _openCalcDb;  // Local reference of OpenCalc db
        private readonly Dictionary<string, int> _formulaTemplateDict;  // Local reference of the template dictionary.
        private readonly GenericTable _scadaXref;  // Local reference of the SCADA xref.

        /// <summary>
        /// Enum for input and output types.
        /// </summary>
        public enum FormulaTypes
        {
            DOUBLE = 1,
            STATE = 3
        }

        /// <summary>
        /// Default constructor.
        /// Set important local references.
        /// </summary>
        /// <param name="par">Current parser object.</param>
        /// <param name="openCalcDb">Current OpenCalc db object.</param>
        /// <param name="formulaTemplateDict">Formula template dictionary.</param>
        /// <param name="scadaXref">Current SCADA xref object.</param>
        public EdpespFormula(EdpespParser par, OpenCalc openCalcDb, Dictionary<string, int> formulaTemplateDict, GenericTable scadaXref)
        {
            this._parser = par;
            this._openCalcDb = openCalcDb;
            this._formulaTemplateDict = formulaTemplateDict;
            this._scadaXref = scadaXref;
        }

        /// <summary>
        /// Function to convert all FORMULA objects.
        /// </summary>
        public void ConvertFormula() 
        {
            Logger.OpenXMLLog();

            // Sort xref for later
            this._scadaXref.Sort("IRN");

            DbObject formulaObj = this._openCalcDb.GetDbObject("FORMULA");
            int formulaRec = 0;

            foreach (DataRow formulaRow in this._parser.IscPointGroupTbl.Rows)
            {
                // Check for template, if not present: skip.
                int pTemplateRecord = GetpTemplateRecord(formulaRow["CLASSIFICATION"].ToString());
                if (pTemplateRecord.Equals(-1))
                {
                    continue;
                }
                // Set Record and name
                formulaObj.CurrentRecordNo = ++formulaRec;
                formulaObj.SetValue("Name", formulaRow["EXTERNAL_IDENTITY"]);

                // Set pTemplateRecord, pExecutionGroup, and ConfigBits
                formulaObj.SetValue("pTemplateRecord", pTemplateRecord);
                SetpExecutionGroup(formulaObj, formulaRow["CLASSIFICATION"].ToString());
                formulaObj.SetValue("ConfigBits", 1);

                // Find DataRow from isc_elements
                DataRow[] elementRows = FindElementRows(formulaRow["IRN"].ToString());
                if (null == elementRows)
                {
                    continue;
                }
                List<DataRow> sortedElements = SortElementsByNumber(elementRows);
                // Set input and output keys
                int outputCount = OutputCount(pTemplateRecord);
                SetOutputKeys(formulaObj, sortedElements, outputCount);
                SetInputKeys(formulaObj, sortedElements, outputCount);

            }
            Logger.CloseXMLLog();
        }

        /// <summary>
        /// Helper function to set the pTemplateRecord.
        /// </summary>
        /// <param name="templateName">CLASSIFICATION name from input.</param>
        /// <returns>pTemplateRecord on success. -1 on failure.</returns>
        public int GetpTemplateRecord(string templateName)
        {
            if (this._formulaTemplateDict.TryGetValue(templateName, out int pTemplateRecord))
            {
                return pTemplateRecord;
            }
            else
            {
                Logger.Log("DID NOT FIND pTEMPLATERECORD", LoggerLevel.INFO, $"Could not find pTemplateRecord for template name: {templateName}");
                return -1;
            }
        }

        /// <summary>
        /// Helper function to find the matching isc_element rows.
        /// </summary>
        /// <param name="irn">IRN of current isc_point_group row.</param>
        /// <returns>Returns elementRows on success. null on failure.</returns>
        public DataRow[] FindElementRows(string irn)
        {
            DataRow[] elementRows = this._parser.IscElementTbl.GetMultipleRowInfo(new[] { irn });
            if (null != elementRows)
            {
                return elementRows;
            }
            else
            {
                Logger.Log("COULD NOT FIND ELEMENT ROWS", LoggerLevel.INFO, $"Could not find irn in isc_element table {irn}");
                return null;
            }
        }

        /// <summary>
        /// Helper function to set input keys.
        /// </summary>
        /// <param name="formulaObj">Current FORMULA object.</param>
        /// <param name="elementRows">Current row of elements.</param>
        /// <param name="outputCount">Number of outputs.</param>
        public void SetInputKeys(DbObject formulaObj, List<DataRow> elementRows, int outputCount)
        {
            int count = 0;
            foreach (DataRow currentRow in elementRows)
            {
                if (currentRow["ELEMENT_NUMBER"].ToInt() > outputCount)
                {
                    string type = currentRow["PDB_CONCEPT_NAME"].ToString();
                    switch (type)
                    {
                        case "MEASURAND":
                            if (this._scadaXref.TryGetRow(new[] { currentRow["MEASURAND_IRN"].ToString() }, out DataRow measurandRow))
                            {
                                formulaObj.SetValue("InputKey", count, measurandRow["SCADA Key"]);
                                formulaObj.SetValue("InputType", count, (int)FormulaTypes.DOUBLE);
                            }
                            else
                            {
                                Logger.Log("DID NOT FIND IRN IN XREF", LoggerLevel.INFO, $"MEASURAND IRN not found in scada xref. irn: {currentRow["MEASURAND_IRN"]}");
                            }
                            ++count;
                            break;
                        case "INDICATION":
                            if (this._scadaXref.TryGetRow(new[] { currentRow["INDICATION_IRN"].ToString() }, out DataRow indicationRow))
                            {
                                formulaObj.SetValue("InputKey", count, indicationRow["SCADA Key"]);
                                formulaObj.SetValue("InputType", count, (int)FormulaTypes.STATE);
                            }
                            else
                            {
                                Logger.Log("DID NOT FIND IRN IN XREF", LoggerLevel.INFO, $"INDICATION IRN not found in scada xref. irn: {currentRow["INDICATION_IRN"]}");
                            }
                            ++count;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Helper function to set output keys.
        /// </summary>
        /// <param name="formulaObj">Current FORMULA object.</param>
        /// <param name="elementRows">Current row of elements.</param>
        /// <param name="outputCount">Number of outputs.</param>
        public void SetOutputKeys(DbObject formulaObj, List<DataRow> elementRows, int outputCount)
        {
            int count = 1;
            foreach (DataRow currentRow in elementRows)
            {
                if (count < outputCount + 1 && currentRow["ELEMENT_NUMBER"].ToInt().Equals(count))
                {
                    string type = currentRow["PDB_CONCEPT_NAME"].ToString();
                    switch (type)
                    {
                        case "MEASURAND":
                            if (this._scadaXref.TryGetRow(new[] { currentRow["MEASURAND_IRN"].ToString() }, out DataRow measurandRow))
                            {
                                formulaObj.SetValue("OutputKey", count - 1, measurandRow["SCADA Key"]);
                                formulaObj.SetValue("OutputType", count - 1, (int)FormulaTypes.DOUBLE);
                            }
                            else
                            {
                                Logger.Log("DID NOT FIND IRN IN XREF", LoggerLevel.INFO, $"MEASURAND IRN not found in scada xref. irn: {currentRow["MEASURAND_IRN"]}");
                            }
                            ++count;
                            break;
                        case "INDICATION":
                            if (this._scadaXref.TryGetRow(new[] { currentRow["INDICATION_IRN"].ToString() }, out DataRow indicationRow))
                            {
                                formulaObj.SetValue("OutputKey", count - 1, indicationRow["SCADA Key"]);
                                formulaObj.SetValue("OutputType", count - 1, (int)FormulaTypes.STATE);
                            }
                            else
                            {
                                Logger.Log("DID NOT FIND IRN IN XREF", LoggerLevel.INFO, $"INDICATION IRN not found in scada xref. irn: {currentRow["INDICATION_IRN"]}");
                            }
                            ++count;
                            break;
                    }
                }
            }
        }


        /// <summary>
        /// Helper function to count the number of outputs.
        /// </summary>
        /// <param name="pTemplateRec">Current pTemplateRecord</param>
        /// <returns>Count of output.</returns>
        public int OutputCount(int pTemplateRec)
        {
            DbObject templateObj = this._openCalcDb.GetDbObject("FORMULA_TEMPLATE");
            templateObj.CurrentRecordNo = pTemplateRec;

            int count = 0;
            for (int i = 0; i < 5; i++)
            {
                if (!templateObj.GetValue("OutputDataType", i).Equals("0"))
                {
                    ++count;
                }
            }
            return count;
        }

        /// <summary>
        /// Helper function to sort the element rows by their number.
        /// </summary>
        /// <param name="elementRows">Current array of element rows.</param>
        /// <returns>A sorted list of DataRows.</returns>
        public List<DataRow> SortElementsByNumber(DataRow[] elementRows)
        {
            List<DataRow> sortedRows = new List<DataRow>();
            int count = 0;
            while (count < elementRows.Length)
            {
                foreach (DataRow currentRow in elementRows)
                {
                    if (currentRow["ELEMENT_NUMBER"].ToInt().Equals(count + 1))
                    {
                        sortedRows.Add(currentRow);
                    }
                }
                ++count;
            }
            return sortedRows;
        }

        /// <summary>
        /// Helper function to set execution group based on formula template offsets.
        /// </summary>
        /// <param name="formulaObj">Current formula object.</param>
        /// <param name="templateRecord">Template name for current formula object.</param>
        public void SetpExecutionGroup(DbObject formulaObj, string templateRecord)
        {
            if (!this._parser.ExecTimeTbl.TryGetRow(new[] { templateRecord }, out DataRow execRow))
            {
                Logger.Log("UNMATCHED EXECUTION GROUP", LoggerLevel.INFO, $"No row found for Template:{templateRecord}");
                return;
            }
            string offsetHour = execRow["OFFSET HOUR"].ToString();
            string offsetMinute = execRow["OFFSET MINUTE"].ToString();
            string offsetSecond = execRow["OFFSET SECOND"].ToString();
            
            // Switch on each offset going from longest time unit to shortest.
            // Unfortunately have to do 3 nested switches for this.
            switch (offsetHour)
            {
                case "0":
                    switch (offsetMinute)
                    {
                        case "0":
                            switch (offsetSecond)
                            {
                                case "0":
                                case "10":
                                    formulaObj.SetValue("pExecutionGroup", 3);
                                    break;
                                case "4":
                                    formulaObj.SetValue("pExecutionGroup", 1);
                                    break;
                                case "5":
                                    formulaObj.SetValue("pExecutionGroup", 2);
                                    break;
                                case "15":
                                    formulaObj.SetValue("pExecutionGroup", 4);
                                    break;
                                case "20":
                                    formulaObj.SetValue("pExecutionGroup", 5);
                                    break;
                                case "30":
                                    formulaObj.SetValue("pExecutionGroup", 6);
                                    break;
                                case "35":
                                    formulaObj.SetValue("pExecutionGroup", 7);
                                    break;
                                case "40":
                                    formulaObj.SetValue("pExecutionGroup", 8);
                                    break;
                                case "50":
                                    formulaObj.SetValue("pExecutionGroup", 9);
                                    break;
                                default:
                                    Logger.Log("UNMATCHED EXECUTION GROUP", LoggerLevel.INFO, $"Unmatched execution group: OFFSET HOUR: {offsetHour} \t OFFSET MINUTE: {offsetMinute} \t OFFSET SECOND: {offsetSecond}");
                                    break;
                            }
                            break;
                        case "1":
                            switch (offsetSecond)
                            {
                                case "0":
                                    formulaObj.SetValue("pExecutionGroup", 10);
                                    break;
                                default:
                                    Logger.Log("UNMATCHED EXECUTION GROUP", LoggerLevel.INFO, $"Unmatched execution group: OFFSET HOUR: {offsetHour} \t OFFSET MINUTE: {offsetMinute} \t OFFSET SECOND: {offsetSecond}");
                                    break;
                            }
                            break;
                        case "5":
                            switch (offsetSecond)
                            {
                                case "0":
                                    formulaObj.SetValue("pExecutionGroup", 11);
                                    break;
                                default:
                                    Logger.Log("UNMATCHED EXECUTION GROUP", LoggerLevel.INFO, $"Unmatched execution group: OFFSET HOUR: {offsetHour} \t OFFSET MINUTE: {offsetMinute} \t OFFSET SECOND: {offsetSecond}");
                                    break;
                            }
                            break;
                        case "10":
                            switch (offsetSecond)
                            {
                                case "0":
                                    formulaObj.SetValue("pExecutionGroup", 12);
                                    break;
                                default:
                                    Logger.Log("UNMATCHED EXECUTION GROUP", LoggerLevel.INFO, $"Unmatched execution group: OFFSET HOUR: {offsetHour} \t OFFSET MINUTE: {offsetMinute} \t OFFSET SECOND: {offsetSecond}");
                                    break;
                            }
                            break;
                        default:
                            Logger.Log("UNMATCHED EXECUTION GROUP", LoggerLevel.INFO, $"Unmatched execution group: OFFSET HOUR: {offsetHour} \t OFFSET MINUTE: {offsetMinute} \t OFFSET SECOND: {offsetSecond}");
                            break;
                    }
                    break;
                case "1":
                    switch (offsetMinute)
                    {
                        case "0":
                            switch (offsetSecond)
                            {
                                case "0":
                                    formulaObj.SetValue("pExecutionGroup", 13);
                                    break;
                                default:
                                    Logger.Log("UNMATCHED EXECUTION GROUP", LoggerLevel.INFO, $"Unmatched execution group: OFFSET HOUR: {offsetHour} \t OFFSET MINUTE: {offsetMinute} \t OFFSET SECOND: {offsetSecond}");
                                    break;
                            }
                            break;
                        default:
                            Logger.Log("UNMATCHED EXECUTION GROUP", LoggerLevel.INFO, $"Unmatched execution group: OFFSET HOUR: {offsetHour} \t OFFSET MINUTE: {offsetMinute} \t OFFSET SECOND: {offsetSecond}");
                            break;
                    }
                    break;
                default:
                    Logger.Log("UNMATCHED EXECUTION GROUP", LoggerLevel.INFO, $"Unmatched execution group: OFFSET HOUR: {offsetHour} \t OFFSET MINUTE: {offsetMinute} \t OFFSET SECOND: {offsetSecond}");
                    break;
            }
        }
    }
}
