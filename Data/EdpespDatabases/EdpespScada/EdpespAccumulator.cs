using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using OSII.ConversionToolkit;
using OSII.ConversionToolkit.Extensions;
using OSII.ConversionToolkit.Generic;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespScada
{
    class EdpespAccumulator
    {
        private readonly EdpespParser _parser;  // Local reference of the parser
        private readonly SCADA _scadaDb;  // Local reference of the Scada Database
        private readonly FEP _fepDb;  // Local reference of the Fep Database
        private readonly Dictionary<string, int> _stationDict;  // Local reference of the station Dict.
        private readonly Dictionary<string, int> _aorDict;  // Local reference of the aorgroup Dict.
        private readonly Dictionary<string, int> _unitDict;  // Local reference of the unit dictionary.
        private readonly Dictionary<string, int> _class2Dict;  // Local reference of the Class2 Dict. 
        private readonly Dictionary<string, int> _alarmsDict;  // Local reference of the alarm Dict.
        private readonly Dictionary<string, int> _rtuDataDict;  // Local reference to the RTU_DATA Dictionary.
        private readonly Dictionary<(int, int), int> _rtuIoaDict;  // Local reference to the RTU, subtype to IOA dictionary.
        private readonly Dictionary<string, int> _scaleDict;  // Local reference of the scale dictionary
        private readonly GenericTable _scadaXref;  // Local reference to the Scada Xref
        private readonly GenericTable _scadaToFepXref;  // Local reference to the Scada to Fep Xref

        /// <summary>
        /// Default Constructor.
        /// Assigns local references of important variables.
        /// </summary>
        /// <param name="par">Current Parser.</param>
        /// <param name="scadaDb">Current Scada database.</param>
        /// <param name="fepDb">Current Fep database object.</param>
        /// <param name="stationDict">Station dictionary object.</param>
        /// <param name="unitDict">Unit dictionary object.</param>
        /// <param name="class2Dict">Class2 dictionary object.</param>
        /// <param name="alarmsDict">Alarm dictionary object.</param
        /// <param name="rtuDataDict">RTU_DATA dictionary object.</param>
        /// <param name="rtuIoaDict">RTU to IOA dictionary.</param>
        /// <param name="scaleDict">Current Scale Dictionary.</param>
        /// <param name="scadaXref">Current Scada XREF object.</param>
        /// /// <param name="AorDict">Aorgroup dictionary object.</param>
        public EdpespAccumulator(EdpespParser par, SCADA scadaDb, FEP fepDb, Dictionary<string, int> stationDict, Dictionary<string, int> unitDict, Dictionary<string, int> class2Dict,
            Dictionary<string, int> alarmsDict, Dictionary<string, int> rtuDataDict, Dictionary<(int, int), int> rtuIoaDict, 
                Dictionary<string, int> scaleDict, GenericTable scadaXref, GenericTable scadaToFepXref, Dictionary<string, int> AorDict)
        {
            this._parser = par;
            this._scadaDb = scadaDb;
            this._fepDb = fepDb;
            this._stationDict = stationDict;
            this._unitDict = unitDict;
            this._class2Dict = class2Dict;
            this._scadaXref = scadaXref;
            this._alarmsDict = alarmsDict;
            this._rtuDataDict = rtuDataDict;
            this._rtuIoaDict = rtuIoaDict;
            this._scaleDict = scaleDict;
            this._scadaToFepXref = scadaToFepXref;
        }

        /// <summary>
        /// Function to convert all ACCUMULATOR objects.
        /// </summary>
        public void ConvertAccumulator()
        {
            Logger.OpenXMLLog();

            DbObject accumObj = this._scadaDb.GetDbObject("ACCUMULATOR");
            int accumRec = 0;
            // Const values from mapping.
            int AOR_GROUP = 1;
            const int pACCUM_PERIOD = 1;
            //const int pALARM_GROUP = 1;
            int pALARM_GROUP = 1;
            foreach (DataRow accumRow in this._parser.AccumulatorTbl.Rows)
            {
                // Check for pStation first to know which points to skip.
                int pStation = EdpespScadaExtensions.GetpStation(accumRow["STATION_IRN"].ToString(), this._stationDict);
                pALARM_GROUP = EdpespScadaExtensions.GetpAorGroup2(accumRow["SUBSYSTEM_IRN"].ToString(), this._stationDict);
                if (pStation.Equals(-1))
                {
                    continue;
                }

                AOR_GROUP = EdpespScadaExtensions.GetpAorGroup2(accumRow["SUBSYSTEM_IRN"].ToString(), this._aorDict);
                if (AOR_GROUP.Equals(-1))
                {
                    AOR_GROUP = 1;
                }
                // Check points in display to know which to skip.
                //SA:20111124
                //if (EdpespScadaExtensions.ToSkip(this._parser.StationsToSkip, this._parser.PointsFromDisplay, accumRow["STATION_IRN"].ToString(), accumRow["EXTERNAL_IDENTITY"].ToString()))
                //{
                //    continue;
                //}
                // Check external identity to and skip those containing PROTT
                if (accumRow["EXTERNAL_IDENTITY"].ToString().Contains("PROTT"))
                {
                    continue;
                }
                // Check Value per pulse and skip those with 0
                if (accumRow["VALUE_PER_PULSE"].ToString().Equals("0"))
                {
                    continue;
                }
                // Check if IDENTIFICATION_TEXT contains Carripont
                if (accumRow["IDENTIFICATION_TEXT"].ToString().Contains("Carripont"))
                {
                    continue;
                }
                // Check if RTU_IRN is non empty, if so: check to skip.
                if (!string.IsNullOrEmpty(accumRow["RTU_IRN"].ToString()) && EdpespScadaExtensions.SkipRtuPoint(this._parser.RtuTbl, accumRow["RTU_IRN"].ToString()))
                {
                    continue;
                }

                // Set Record and Type
                accumObj.CurrentRecordNo = ++accumRec;
                int type = GetType(accumRow);
                accumObj.SetValue("Type", type);

                // Set Name and pClass2
                //SA:20111124
                string name = EdpespScadaExtensions.GetName(accumRow["IDENTIFICATION_TEXT"].ToString(), this._parser.StationAbbrTbl);
                //string name = "";
                string voltage = EdpespScadaExtensions.SetpClass2(accumObj, accumRow["IDENTIFICATION_TEXT"].ToString(), this._class2Dict, this._parser.StationAbbrTbl);
                if (!string.IsNullOrEmpty(voltage)) name = name.Replace(voltage, "");
                if (type.Equals((int)AccumulatorType.T_ACCUM))
                {
                    name = EdpespScadaExtensions.GetpDeviceInstance(name, false, out int _);  // This pDeviceInstance should always be 2
                    accumObj.SetValue("pDeviceInstance", 2);
                }
                accumObj.SetValue("Name", name.Trim());

                // Set pStation, pAORGroup, pAlarm_Group, and Archive Group
                accumObj.SetValue("pStation", pStation);
                accumObj.SetValue("pAORGroup", AOR_GROUP);
                accumObj.SetValue("pALARM_GROUP", pALARM_GROUP);
                //EdpespScadaExtensions.SetArchiveGroup(accumObj, ScadaType.ACCUMULATOR, accumRow["HIS_TYPE_IRN"].ToString());
                EdpespScadaExtensions.SetArchiveGroup2(accumObj, ScadaType.ACCUMULATOR, accumRow["HIS_TYPE_IRN"].ToString());//BD: Updated as per v3 mapping file

                // Set Scada Key
                string key = EdpespScadaExtensions.SetScadaKey(accumObj, type, pStation, this._scadaDb, ScadaType.ACCUMULATOR, 
                    this._parser.ScadaKeyTbl, this._parser.LockedKeys, accumRow["EXTERNAL_IDENTITY"].ToString(), false);
                if (string.IsNullOrEmpty(key))
                {
                    Logger.Log("EMPTY KEY", LoggerLevel.INFO, $"ACCUMULATOR: Key was empty. Name: {name}\t pStation: {pStation}\t Type: {type}");
                }

                // Set pUnit, pScale, pACCUM_PERIOD
                EdpespScadaExtensions.SetpUnit(accumObj, accumRow, this._unitDict);
                SetpScale(accumObj, accumRow);
                accumObj.SetValue("pACCUM_PERIOD", pACCUM_PERIOD);

                // Add to protocol count and set pRtu
                int pRtu = EdpespScadaExtensions.FindpRtu(accumRow, this._rtuDataDict);
                //accumObj.SetValue("pRTU", pRtu);
                EdpespScadaExtensions.I104PointTypes fepPointType = EdpespScadaExtensions.I104PointTypes.None;
                int address = 0;
                string ioa = accumRow["PXINT1"].ToString();
                if (!pRtu.Equals(-1) && !string.IsNullOrEmpty(ioa))
                {
                    fepPointType = EdpespScadaExtensions.I104PointTypes.INT_TOTAL;
                    address = EdpespScadaExtensions.GetNextRtuDefnAddress(pRtu, fepPointType);
                    accumObj.SetValue("pPoint", address);
                    accumObj.SetValue("AppID", 2);
                    //int index = this._fepDb.AddToRtuCount(pRtu, address, false, (int)fepPointType, true);
                    this._scadaToFepXref.AddRecordSetValues(new string[] { "ACCUMULATOR", accumRec.ToString(), pRtu.ToString(), ((int)fepPointType).ToString(), key, ioa, "False", address.Equals(0) ? string.Empty : address.ToString(), accumRow["EXTERNAL_IDENTITY"].ToString() });
                    accumObj.SetValue("pRTU", pRtu);
                }

                // Add to XREF
                string[] scadaXrefFields = { "ACCUMULATOR", accumRow["IDENTIFICATION_TEXT"].ToString(), key, pStation.ToString(), accumRec.ToString(),
                    accumRow["EXTERNAL_IDENTITY"].ToString(), pRtu.Equals(-1)? string.Empty : pRtu.ToString(), 
                        ((int)fepPointType).Equals(0)? string.Empty : ((int)fepPointType).ToString(), address.Equals(0)? string.Empty : address.ToString(), ioa,
                            accumRow["IRN"].ToString(),"False", Regex.Replace(accumRow["EXTERNAL_IDENTITY"].ToString(), @"\s+", ""), type.ToString()};
                this._scadaXref.AddRecordSetValues(scadaXrefFields);
            }

            Logger.CloseXMLLog();
        }

        /// <summary>
        /// Helper function to get get Type.
        /// </summary>
        /// <param name="accumRow">Current DataRow row.</param>
        /// <returns></returns>
        public int GetType(DataRow accumRow)
        {
            string accumTypeCode = accumRow["ACCUMULATOR_TYPE_CODE"].ToString();

            switch (accumTypeCode)
            {
                case "R":
                    return (int)AccumulatorType.T_ACCUM;
                case "C":
                    return (int)AccumulatorType.C_ACCUM;
                case "M":
                    return (int)AccumulatorType.M_ACCUM;
                default:
                    Logger.Log("UHANDLED ACCUMULATOR TYPE CODE", LoggerLevel.INFO, $"Provided Accumulator Type Code unmapped: {accumTypeCode}\tSetting Type to M_ACCUM");
                    return (int)AccumulatorType.M_ACCUM;
            }
        }

        /// <summary>
        /// Helper fuction to set pScale.
        /// </summary>
        /// <param name="accumObj">Current ACCUMULATOR object.</param>
        /// <param name="accumRow">Current DataRow row from input.</param>
        public void SetpScale(DbObject accumObj, DataRow accumRow)
        {
            string valuePerPulse = accumRow["VALUE_PER_PULSE"].ToString().Replace(',', '.');
            if (this._scaleDict.TryGetValue(valuePerPulse, out int pScale))
            {
                accumObj.SetValue("pScale", pScale);
            }
            else
            {
                Logger.Log("UNFOUND SCALE", LoggerLevel.INFO, $"Value per pulse was did not link to a pScale: {valuePerPulse}\t");
            }
        }
    }
}
