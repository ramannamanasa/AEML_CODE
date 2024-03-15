using System.Collections.Generic;
using System.Data;
using OSII.ConversionToolkit;
using OSII.ConversionToolkit.Extensions;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespOpenCalc
{
    class EdpespFormulaTemplate
    {
        private readonly EdpespParser _parser;  // Local referen of Parser object
        private readonly OpenCalc _openCalcDb;  // Local reference of OpenCalc db
        private readonly Dictionary<string, int> _formulaTemplateDict;  // Local reference of the template dictionary.
        
        /// <summary>
        /// Enum for input and output template types.
        /// </summary>
        public enum FormulaTemplateTypes
        {
            DOUBLE = 1,
            STATE = 2
        }

        /// <summary>
        /// Default constructor.
        /// Sets important local references.
        /// </summary>
        /// <param name="par">Current parser object.</param>
        /// <param name="openCalcDb">Current OpenCalc database object.</param>
        /// <param name="formulaTemplateDict">Current formula template dictionay.</param>
        public EdpespFormulaTemplate(EdpespParser par, OpenCalc openCalcDb, Dictionary<string, int> formulaTemplateDict)
        {
            this._parser = par;
            this._openCalcDb = openCalcDb;
            this._formulaTemplateDict = formulaTemplateDict;
        }

        /// <summary>
        /// Function to convert all FORMULA_TEMPLATE objects.
        /// </summary>
        public void ConverFormulaTemplate()
        {
            Logger.OpenXMLLog();

            DbObject formulaTemplateObj = this._openCalcDb.GetDbObject("FORMULA_TEMPLATE");
            int formulaTemplateRec = 0;

            foreach (DataRow templateRow in this._parser.CalcScopeTbl.Rows)
            {
                // Skip formulas that aren't being used
                if (!templateRow["Approach"].ToString().Equals("Calc formula"))
                {
                    continue;
                }
                // Set Record, Name, FunctionIdentifier, and ConfigBits
                string formulaName = templateRow["Program Name"].ToString();
                formulaTemplateObj.CurrentRecordNo = ++formulaTemplateRec;
                formulaTemplateObj.SetValue("Name", formulaName);
                formulaTemplateObj.SetValue("FunctionIdentifier", formulaName);
                formulaTemplateObj.SetValue("ConfigBits", 1);

                // Find formula row in table to get its inputs and outputs
                Dictionary<string, int> inputsAndOuputsDict = new Dictionary<string, int>();
                if (this._parser.InputAndOutputTbl.TryGetRow(new[] { formulaName }, out DataRow inputOutputRow))
                {
                    CountInputsAndOutputs(inputOutputRow, inputsAndOuputsDict);
                }

                // Set Inputs and Outputs types
                SetInputs(formulaTemplateObj, inputsAndOuputsDict);
                SetOutputs(formulaTemplateObj, inputsAndOuputsDict);

                // Add to dictionary
                this._formulaTemplateDict.Add(formulaName, formulaTemplateRec);
            }

            Logger.CloseXMLLog();
        }

        /// <summary>
        /// Helper fuction to set input value types.
        /// </summary>
        /// <param name="formulaTemplateObj">Current FORMULA_TEMPLATE object.</param>
        /// <param name="inputsAndOuputsDict">Dictionary of inputs.</param>
        public void SetInputs(DbObject formulaTemplateObj, Dictionary<string, int> inputsAndOuputsDict)
        {
            int currentInput = 0;
            foreach (KeyValuePair<string, int> input in inputsAndOuputsDict)
            {
                switch (input.Key)
                {
                    case "Measurand Inputs":
                        int doubleTotal = input.Value;
                        for (int i = 0; i < doubleTotal; i++)
                        {
                            formulaTemplateObj.SetValue("InputDataType", currentInput, (int)FormulaTemplateTypes.DOUBLE);
                            ++currentInput;
                        }
                        break;
                    case "Indication Inputs":
                        int stateTotal = input.Value;
                        for (int i = 0; i < stateTotal; i++)
                        {
                            formulaTemplateObj.SetValue("InputDataType", currentInput, (int)FormulaTemplateTypes.STATE);
                            ++currentInput;
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Helper fuction to set output value types
        /// </summary>
        /// <param name="formulaTemplateObj">Current FORMULA_TEMPLATE object.</param>
        /// <param name="inputsAndOuputsDict">Dict of outputs.</param>
        public void SetOutputs(DbObject formulaTemplateObj, Dictionary<string, int> inputsAndOuputsDict)
        {
            int currentOutput = 0;
            foreach (KeyValuePair<string, int> output in inputsAndOuputsDict)
            {
                switch (output.Key)
                {
                    case "Measurand Outputs":
                        int doubleTotal = output.Value;
                        for (int i = 0; i < doubleTotal; i++)
                        {
                            formulaTemplateObj.SetValue("OutputDataType", currentOutput, (int)FormulaTemplateTypes.DOUBLE);
                            ++currentOutput;
                        }
                        break;
                    case "Indication Outputs":
                        int stateTotal = output.Value;
                        for (int i = 0; i < stateTotal; i++)
                        {
                            formulaTemplateObj.SetValue("OutputDataType", currentOutput, (int)FormulaTemplateTypes.STATE);
                            ++currentOutput;
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Helper function to Count the Inputs and Outputs of a formula template.
        /// </summary>
        /// <param name="inputsOutputsRow">Current inputOutput row.</param>
        /// <param name="inputsAndOutputsDict">intputOutput Dictionary object.</param>
        public void CountInputsAndOutputs(DataRow inputsOutputsRow, Dictionary<string,int> inputsAndOutputsDict)
        {
            string startingType = inputsOutputsRow["Start Inputs with"].ToString();
            switch (startingType)
            {
                case "INDICATION":
                    if (!string.IsNullOrEmpty(inputsOutputsRow["Number of Indication Inputs"].ToString()))
                    {
                        inputsAndOutputsDict.Add("Indication Inputs", inputsOutputsRow["Number of Indication Inputs"].ToInt());
                    }
                    if (!string.IsNullOrEmpty(inputsOutputsRow["Number of Measurand Inputs"].ToString()))
                    {
                        inputsAndOutputsDict.Add("Measurand Inputs", inputsOutputsRow["Number of Measurand Inputs"].ToInt());
                    }
                    break;
                case "MEASURAND":
                default:
                    if (!string.IsNullOrEmpty(inputsOutputsRow["Number of Measurand Inputs"].ToString()))
                    {
                        inputsAndOutputsDict.Add("Measurand Inputs", inputsOutputsRow["Number of Measurand Inputs"].ToInt());
                    }
                    if (!string.IsNullOrEmpty(inputsOutputsRow["Number of Indication Inputs"].ToString()))
                    {
                        inputsAndOutputsDict.Add("Indication Inputs", inputsOutputsRow["Number of Indication Inputs"].ToInt());
                    }
                    break;
            }
            
            if (!string.IsNullOrEmpty(inputsOutputsRow["Number of Measurand Outputs"].ToString()))
            {
                inputsAndOutputsDict.Add("Measurand Outputs", inputsOutputsRow["Number of Measurand Outputs"].ToInt());
            }
            if (!string.IsNullOrEmpty(inputsOutputsRow["Number of Indication Outputs"].ToString()))
            {
                inputsAndOutputsDict.Add("Indication Outputs", inputsOutputsRow["Number of Indication Outputs"].ToInt());
            }
        }
    }
}
