using System;
using System.Collections.Generic;
using System.Data;
using OSII.ConversionToolkit;
using OSII.ConversionToolkit.Extensions;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespScada
{
    class EdpespClass2
    {
        private readonly EdpespParser _parser;  // Local reference of the parser
        private readonly SCADA _scadaDb;  // Local reference of the Scada Database
        private readonly Dictionary<string, int> _class2Dict;  // Local reference of the Class 2 Dict.
        private readonly List<double> Class2List = new List<double>();  // List of different class2.

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="par">Parser obejct.</param>
        /// <param name="scadaDb">Current SCADA db object.</param>
        /// <param name="class2Dict">pClass2 dictionary.</param>
        public EdpespClass2(EdpespParser par, SCADA scadaDb, Dictionary<string, int> class2Dict)
        {
            this._parser = par;
            this._scadaDb = scadaDb;
            this._class2Dict = class2Dict;
        }
        
        /// <summary>
        /// Function to convert all Class2 objects.
        /// </summary>
        public void ConvertClass2()
        {
            Logger.OpenXMLLog();

            DbObject class2Obj = this._scadaDb.GetDbObject("CLASS2");
            int class2Rec = 0;

            // Convert Status Class2
            foreach (DataRow statusRow in this._parser.IndicationTbl.Rows)
            {
                string name = EdpespScadaExtensions.ExtractVoltage(statusRow["IDENTIFICATION_TEXT"].ToString(), this._parser.StationAbbrTbl);
                //SA:20211125 Skipping Extract Voltage 
                //string name = "";
                if (name.Equals("Bad Input") || string.IsNullOrEmpty(name))
                {
                    Logger.Log("BAD CLASS2 NAME", LoggerLevel.INFO, $"STATUS: Class2 name could not be found in rawName:{statusRow["IDENTIFICATION_TEXT"]}");
                }
                else
                {
                    double nameValue = name.ToDouble();
                    if (!this.Class2List.Contains(nameValue))
                    {
                        this.Class2List.Add(nameValue);
                    }
                }
            }

            // Convert Analog Class2
            foreach (DataRow analogRow in this._parser.MeasurandTbl.Rows)
            {
                string name = EdpespScadaExtensions.ExtractVoltage(analogRow["IDENTIFICATION_TEXT"].ToString(), this._parser.StationAbbrTbl);
                //string name = "";
                if (name.Equals("Bad Input") || string.IsNullOrEmpty(name))
                {
                    Logger.Log("BAD CLASS2 NAME", LoggerLevel.INFO, $"ANALOG: Class2 name could not be found in rawName:{analogRow["IDENTIFICATION_TEXT"]}");
                }
                else
                {
                    double nameValue = name.ToDouble();
                    if (!this.Class2List.Contains(nameValue))
                    {
                        this.Class2List.Add(nameValue);
                    }
                }
            }

            // Convert Accumulator Class2
            foreach (DataRow accumRow in this._parser.AccumulatorTbl.Rows)
            {
                string name = EdpespScadaExtensions.ExtractVoltage(accumRow["IDENTIFICATION_TEXT"].ToString(), this._parser.StationAbbrTbl);
                //string name = "";
                if (name.Equals("Bad Input") || string.IsNullOrEmpty(name))
                {
                    Logger.Log("BAD CLASS2 NAME", LoggerLevel.INFO, $"ACCUMULATOR: Class2 name could not be found in rawName:{accumRow["IDENTIFICATION_TEXT"]}");
                }
                else
                {
                    double nameValue = name.ToDouble();
                    if (!this.Class2List.Contains(nameValue))
                    {
                        this.Class2List.Add(nameValue);
                    }
                }
            }

            // Convert Setpoint Class2
            foreach (DataRow setpointRow in this._parser.SetpointTbl.Rows)
            {
                string name = EdpespScadaExtensions.ExtractVoltage(setpointRow["IDENTIFICATION_TEXT"].ToString(), this._parser.StationAbbrTbl);
                //SA:20211125 Skipping Extract Voltage 
                //string name = "";
                if (name.Equals("Bad Input") || string.IsNullOrEmpty(name))
                {
                    Logger.Log("BAD CLASS2 NAME", LoggerLevel.INFO, $"SETPOINT: Class2 name could not be found in rawName:{setpointRow["IDENTIFICATION_TEXT"]}");
                }
                else
                {
                    double nameValue = name.ToDouble();
                    if (!this.Class2List.Contains(nameValue))
                    {
                        this.Class2List.Add(nameValue);
                    }
                }
            }

            // Sort the class2 list and then create class2 objects for each one.
            this.Class2List.Sort();
            foreach (double class2 in this.Class2List)
            {
                class2Obj.CurrentRecordNo = ++class2Rec;
                class2Obj.SetValue("Name", class2);
                this._class2Dict.Add(class2.ToString(), class2Rec);
            }

            Logger.CloseXMLLog();
        }
    }
}
