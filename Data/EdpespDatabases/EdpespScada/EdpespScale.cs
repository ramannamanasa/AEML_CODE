using OSII.ConversionToolkit;
using OSII.ConversionToolkit.Extensions;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;
using System;
using System.Collections.Generic;
using System.Data;

namespace edpesp_db.Data.EdpespDatabases.EdpespScada
{
    class EdpespScale
    {
        private readonly EdpespParser _parser;  // Local reference of the parser object.
        private readonly SCADA _scadaDb;  // Local reference of the Scada Database
        private readonly Dictionary<string, int> _scaleDict;  // Local reference of the scale dictionary
        private readonly List<string> ScaleList = new List<string>();  // List of different scales.
        private readonly Dictionary<(string, string), int> _measurandScales = new Dictionary<(string, string), int>();  // Dictionary of scales from the Measurand Table.

        /// <summary>
        /// Default constructor.
        /// Sets important references.
        /// </summary>
        /// <param name="par">Current Parser object.</param>
        /// <param name="scadaDb">Current SCADA database object.</param>
        /// <param name="ScaleDict">Current Scale Dictionary.</param>
        public EdpespScale(EdpespParser par, SCADA scadaDb, Dictionary<string, int> ScaleDict, Dictionary<(string, string), int> measureScales)
        {
            this._parser = par;
            this._scadaDb = scadaDb;
            this._scaleDict = ScaleDict;
            this._measurandScales = measureScales;
        }

        /// <summary>
        /// Function to convert all SCALE objects.
        /// </summary>
        public void ConvertScale()
        {
            Logger.OpenXMLLog();

            DbObject scaleObj = this._scadaDb.GetDbObject("SCALE");
            int scaleRec = 0;

            // Const from mapping
            const int SCALE = 1;
            const int OFFSET = 0;

            scaleObj.CurrentRecordNo = ++scaleRec;
            scaleObj.SetValue("Scale", SCALE);
            scaleObj.SetValue("Offset", OFFSET);
            scaleObj.SetValue("Name", "Default");
            //this._scaleDict.Add("1", scaleRec);
            this._scaleDict.Add("Default", scaleRec);
            ////SA:20111124
            //foreach (DataRow accumRow in this._parser.AccumulatorTbl.Rows)
            //{
            //    string valuePerPulse = accumRow["VALUE_PER_PULSE"].ToString().Replace(',', '.');
            //    if (!this.ScaleList.Contains(valuePerPulse) && !valuePerPulse.Equals("0") && !valuePerPulse.Equals("1"))
            //    {
            //        this.ScaleList.Add(valuePerPulse);
            //    }
            //}
            //BD:20220704 commenting this part as the scale object os created while creating the analog object
            // Sort by lowest number first. 
            //ScaleList.Sort();
            //foreach (string scale in ScaleList)
            //{
            //    scaleObj.CurrentRecordNo = ++scaleRec;
            //    scaleObj.SetValue("Scale", scale);
            //    scaleObj.SetValue("Offset", OFFSET);
            //    scaleObj.SetValue("Name", $"Accum {scale}");
            //    this._scaleDict.Add(scale, scaleRec);
            //}

            //// Find scales from the Measurand Table for telemetered points.
            //foreach (DataRow measRow in this._parser.MeasurandTbl.Rows)
            //{
            //    const int DIVIDE_VALUE = 32767;
            //    string rtuIRN = measRow["RTU_IRN"].ToString();
            //    string min = measRow["MIN_VALUE"].ToString().Replace(',', '.');
            //    string max = measRow["MAX_VALUE"].ToString().Replace(',', '.');
            //    if (!string.IsNullOrEmpty(rtuIRN) && !this._measurandScales.ContainsKey((min, max)))
            //    {
            //        scaleObj.CurrentRecordNo = ++scaleRec;
            //        // Find name based on min and max
            //        string name = $"{min} - {max}";

            //        // Find offset based on min
            //        double measOffset = 0;
            //        if (min.ToDouble() > 0)
            //        {
            //            measOffset = min.ToDouble();
            //        }

            //        // Find scale based on max and offset, and then divide by value given by PT.
            //        double measScale = (max.ToDouble() - measOffset) / DIVIDE_VALUE;

            //        // Check dictionary for existing scale and then set values
            //        if (!this._measurandScales.ContainsKey((min, max)))
            //        {
            //            scaleObj.SetValue("Name", name);
            //            scaleObj.SetValue("Offset", measOffset);
            //            scaleObj.SetValue("Scale", measScale);

            //            this._measurandScales.Add((min, max), scaleRec);
            //        }
            //    }
            //}

            // Create a few setpoint scales with mapping from customer
            AddSetpointScale(scaleObj, ++scaleRec, "Setpoint -4 +4", 0.00012207403);  // 4 / 32767
            AddSetpointScale(scaleObj, ++scaleRec, "Setpoint -320 +320", 0.00976592303);  // 320 / 32767
            AddSetpointScale(scaleObj, ++scaleRec, "Setpoint 0-600", 0.01831110568);  // 600 / 32767

            Logger.CloseXMLLog();
        }

        /// <summary>
        /// Helper function to set setpoint scales.
        /// </summary>
        /// <param name="scaleObj">Current scale object.</param>
        /// <param name="scaleRec">Current record to set the Scale object to.</param>
        /// <param name="name">Name of the scale.</param>
        /// <param name="scale">Value of the scale.</param>
        public void AddSetpointScale(DbObject scaleObj, int scaleRec, string name, double scale)
        {
            scaleObj.CurrentRecordNo = scaleRec;
            scaleObj.SetValue("Name", name);
            scaleObj.SetValue("Scale", scale);
            this._scaleDict.Add(name, scaleRec);
        }
    }
}
