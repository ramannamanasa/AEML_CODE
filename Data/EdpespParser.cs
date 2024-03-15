using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using OSII.ConversionToolkit.Generic;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data
{
    class EdpespParser
    {
        private readonly System.Text.Encoding encoding = System.Text.Encoding.Default;  // Encoding for ANSI
        private readonly string _inputDir;  // Directory of where to find the input files
        private readonly SCADA _extraScadaDb;  // SCADA db for extra objects post conversion.

        public GenericTable RTURecNo { get; set; }  // Table for Old RTU Rec No
        public GenericTable PointsFromDisplay { get; set; }  // Table for points from the diplay
        public GenericTable AccumulatorTbl { get; set; }  // Table for Accumulator data
        public GenericTable IndicationTbl { get; set; }  // Table for Status data
        public GenericTable AlarmIndicationTbl { get; set; }  // Table for Status Alarm data
        public GenericTable SwitchTbl { get; set; }  // Table for Switch data
        public GenericTable MeasurandTbl { get; set; }  // Table for Analog data
        public GenericTable StationKeysTbl { get; set; }  // Table for Analog data
        public GenericTable AlarmMeasurandTbl { get; set; }  // Table for Analog Alarm data
        public GenericTable Measurand_eTbl { get; set; }
        public GenericTable Measurand2Tbl { get; set; } //Table for RTU Missing points
        public GenericTable MeasOmitTbl { get; set; } //Table for RTU Missing points
        public GenericTable IndOmitTbl { get; set; } //Table for RTU Missing points
        public GenericTable IndCMDTbl { get; set; } //Table for RTU Missing points
        public GenericTable IndIOATbl { get; set; } //Table for RTU Missing points
        public GenericTable SubSystemTbl { get; set; }// Table for AORGroup BD
        public GenericTable OnetStatesTbl { get; set; }// Table for Onetnet states BD
        public GenericTable RtuIpTbl { get; set; }// Table for IP  BD
        public GenericTable RtuIpTblold { get; set; }// Table for IP  BD
        public GenericTable SetpointTbl { get; set; }  // Table for SetPoint data
        public GenericTable SetPointsToSkipTbl { get; set; }  // Table for SetPoint data to skip
        public GenericTable StationTbl { get; set; }  // Table for Station data
        public GenericTable StationAbbrTbl { get; set; }  // Table for Station Abbreviations
        public GenericTable StationsToSkip { get; set; }  // Table for Stations of points to skip
        public GenericTable StatesTbl { get; set; }  // Table for States data
        public GenericTable AdditionalStates { get; set; }  // Table for States data
        public GenericTable StatusAlarmsTbl { get; set; }  // Table for Status Alarms data
        public GenericTable AnalogAlarmsTbl { get; set; }  // Table for Analog Alarms data
        public GenericTable LimitsTbl { get; set; }  // Table for Meaurand limits
        public GenericTable RtuTbl { get; set; }  // Table for RTU and CHANNEL data
        public GenericTable RtuTblOld { get; set; }  // Table for Old RTU and CHANNEL data
        public GenericTable Hostnames { get; set; }  // Table for Channel Hostnames data
        public GenericTable IdbTbl { get; set; }  // Table for ICCP 
        public GenericTable IddsetTbl { get; set; }  // Table for ICCP 
        public GenericTable IdassnTbl { get; set; }  // Table for ICCP
        public GenericTable IdintrTbl { get; set; }  // Table for ICCP Import Ds points
        public GenericTable IddValTbl { get; set; }  // Table for ICCP Export and Import points
        public GenericTable IddRefTbl { get; set; }  // Table for ICCP Import points
        public GenericTable IdcPntTbl { get; set; }  // Table for INBOUND_CTRL points
        public GenericTable ParametrosTbl { get; set; }  // Table for Control Center Info
        public GenericTable ControlCenterMapTbl { get; set; }  // Table for Control Center mapping between files
        public GenericTable ControlCenterInfoExtra { get; set; }  // Table for extra Control Center Info objects provided by the customer.
        public GenericTable CalcScopeTbl { get; set; }  // Table for Calc Scope information
        public StreamReader CalcFormulaLib { get; set; }  // .cal file for calc library
        public GenericTable IscPointGroupTbl { get; set; }  // Table for calc points
        public GenericTable IscElementTbl { get; set; }  // Table for calc points
        public GenericTable InputAndOutputTbl { get; set; }  // Table for input and output count of formula templates.
        public GenericTable ExecTimeTbl { get; set; }  // Table for calc points execution times
        public GenericTable ScadaKeyTbl { get; set; }  // EDPESP_XREF from desired output for key locking.
        public HashSet<string> LockedKeys { get; set; }  // HashSet of locked keys.
        public GenericTable CalcPointsTbl { get; set; }  // Table for Status calc points 
        public GenericTable BayTbl { get; set; }  // Table for Status Manual points 
        public GenericTable AorTbl { get; set; }  // Table for AOR 
        public GenericTable AorNewTbl { get; set; }  // Table for AOR 
        public GenericTable TrsfmrTbl { get; set; }  // Table for Status Manual points 
        public GenericTable TransmissionTbl { get; set; }

        public GenericTable Trans123Tbl { get; set; }

        /// <summary>
        /// Default constructor.
        /// Assign an input Directory for input files.
        /// </summary>
        /// <param name="inDir">The input directory.</param>
        public EdpespParser(string inDir, SCADA extraScadaDb)
        {
            this._inputDir = inDir;
            this._extraScadaDb = extraScadaDb;
        }

        //SA:20211124 REMOVED THE EXTRASCADADB ARGUMENT and addeda an overload mehtod
        public EdpespParser(string inDir)
        {
            this._inputDir = inDir;
            //this._extraScadaDb = extraScadaDb;
        }

        /// <summary>
        /// Creates the Generic Tables for each csv File.
        /// </summary>
        public void CreateTables()
        {

            Dictionary<string, string> nameconversion = new Dictionary<string, string>();
            //var dict = System.IO.File.ReadLines(Path.Combine(this._inputDir,"Sample Data For PDS.csv")).Select(line => line.Split(',')).ToDictionary(line => line[0], line => line[1]+" " +line[2]);
            // SCADA tables
            //PointsFromDisplay = new GenericTable(Path.Combine(this._inputDir, "Points.csv"), "EXTERNAL_IDENTITY", ',');  //SA:20211124 file is not found in the inputs given
            //AccumulatorTbl = new GenericTable(Path.Combine(this._inputDir, "accumulator.csv"), "IDENTIFICATION_TEXT", ';'); //SA:20211124 file is not found in the inputs given
            //IndicationTbl = new GenericTable(Path.Combine(this._inputDir, "indication.csv"), "IDENTIFICATION_TEXT", ',', '\"'); //SA:20211124 ; changed to ,
            AorNewTbl = new GenericTable(Path.Combine(this._inputDir, "AORNew.csv"), "IRN", ',', '\"'); //BD
            IndicationTbl = new GenericTable(Path.Combine(this._inputDir, "indication_new.csv"), "RTU_IRN", ',', '\"'); //BD
            IndicationTbl.Sort("RTU_IRN");
            AlarmIndicationTbl = new GenericTable(Path.Combine(this._inputDir, "AlarmIndication.csv"), "IRN", ',', '\"'); //BD
            AlarmIndicationTbl.Sort("IRN");
            SwitchTbl = new GenericTable(Path.Combine(this._inputDir, "switch.csv"), "IRN", ',', '\"'); //BD
            SwitchTbl.Sort("EXTERNAL_IDENTITY");
            MeasurandTbl = new GenericTable(Path.Combine(this._inputDir, "measurand_new.csv"), "IDENTIFICATION_TEXT", ',', '\"');
            AlarmMeasurandTbl = new GenericTable(Path.Combine(this._inputDir, "AlarmMeasurand.csv"), "IRN", ',', '\"');
            AlarmMeasurandTbl.Sort("IRN");
            //New Add files: BD
            StationKeysTbl = new GenericTable(Path.Combine(this._inputDir, "StationKeys.csv"), "EXTERNAL_IDENTITY", ',', '\"'); //New Addition BD
            StationKeysTbl.Sort("RTU_IRN");
            Measurand2Tbl = new GenericTable(Path.Combine(this._inputDir, "MEASURAND_RTUIrnMissingPoints.csv"), "EXTERNAL_IDENTITY", ',', '\"'); //New Addition BD
            Measurand2Tbl.Sort("EXTERNAL_IDENTITY");
            IndCMDTbl = new GenericTable(Path.Combine(this._inputDir, "IndCMDMissingPoints.csv"), "EXTERNAL_IDENTITY", ',', '\"'); //New Addition BD
            IndCMDTbl.Sort("EXTERNAL_IDENTITY");
            IndIOATbl = new GenericTable(Path.Combine(this._inputDir, "IndIoaMissingPoints.csv"), "EXTERNAL_IDENTITY", ',', '\"'); //New Addition BD
            IndIOATbl.Sort("EXTERNAL_IDENTITY");
            MeasOmitTbl = new GenericTable(Path.Combine(this._inputDir, "measurandPointstobeOmitted.csv"), "EXTERNAL_IDENTITY", ',', '\"'); //New Addition BD
            MeasOmitTbl.Sort("EXTERNAL_IDENTITY");
            IndOmitTbl = new GenericTable(Path.Combine(this._inputDir, "IndPointsTobeDeleted.csv"), "EXTERNAL_IDENTITY", ',', '\"'); //New Addition BD
            IndOmitTbl.Sort("EXTERNAL_IDENTITY");
            //Measurand_eTbl = new GenericTable(Path.Combine(this._inputDir, "measurands_e.csv"), "IDENTIFICATION_TEXT", ',', '\"');
            SubSystemTbl = new GenericTable(Path.Combine(this._inputDir, "SUBSYSTEM.csv"), "SUBSYSTEM_TEXT", ',', '\"'); //New Addition BD
            OnetStatesTbl = new GenericTable(Path.Combine(this._inputDir, "OPENNET_STATES.csv"), "SUBSYSTEM_TEXT", ',', '\"'); //New Addition BD
            RtuIpTbl = new GenericTable(Path.Combine(this._inputDir, "RTU_IP_Details_new.csv"), "EXTERNAL_IDENTITY", ',', '\"'); //New Addition BD
            //RtuIpTblold = new GenericTable(Path.Combine(@"D:\Source\Repos\AEML\Database Conversion\Input\Database_Conversion\Input_Files\Database_CSV\CSV\RTU_IP_Details.csv"), "EXTERNAL_IDENTITY", ',', '\"'); //New Addition BD
            SetpointTbl = new GenericTable(Path.Combine(this._inputDir, "set_point_value_new.csv"), "IDENTIFICATION_TEXT", ',', '\"');  //SA:20211124 ; changed to ,
            //SetPointsToSkipTbl = new GenericTable(Path.Combine(this._inputDir, "set_points_to_skip.txt"), "Name", ','); //SA:20211124 file is not found in the inputs given
            StationTbl = new GenericTable(Path.Combine(this._inputDir, "station.csv"), "STATION_ID_TEXT", ',', '\"'); //SA:20211124 ; changed to ,
            //StationAbbrTbl = new GenericTable(Path.Combine(this._inputDir, "Station_Name_Abv_EDP.csv"), "Station Name", ';'); //SA:20211124 file is not found in the inputs given
            //StationsToSkip = new GenericTable(Path.Combine(this._inputDir, "StationsToSkip.csv"), "IRN", ','); //SA:20211124 file is not found in the inputs given
            //StatesTbl = new GenericTable(Path.Combine(this._inputDir, "States Tables.csv"), "Map to (Table number)", ','); //SA:20211124 file is not found in the inputs given
            //AdditionalStates = new GenericTable(Path.Combine(this._inputDir, "additional_states.csv"), "state1", ',');  //SA:20211124 file is not found in the inputs given
            StatusAlarmsTbl = new GenericTable(Path.Combine(this._inputDir, "alarm_group_ind.csv"), "ALARM_PROC_GROUP_NAME", ','); //SA:20211124 file is not found in the inputs given, we have a alarm_group.csv file, but not sure if that is the same
            AnalogAlarmsTbl = new GenericTable(Path.Combine(this._inputDir, "alarm_group_meas.csv"), "ALARM_PROC_GROUP_NAME", ','); //SA:20211124 file is not found in the inputs given, we have a alarm_group.csv file, but not sure if that is the same
            LimitsTbl = new GenericTable(Path.Combine(this._inputDir, "measurand_new.csv" /*"measurandLIMITS.csv"*/), "Full_Name", ',', '\"'); //SA:20211124 file is not found in the inputs given, we have a measurand.csv file only  //SA:20211124 ; changed to ,
            CalcPointsTbl = new GenericTable(Path.Combine(this._inputDir, "Calc_Points.csv"), "Signal", ',', '\"'); //New Addition BD 20221121
            BayTbl = new GenericTable(Path.Combine(this._inputDir, "BAY.csv"), "IDENTIFICATION_TEXT", ',', '\"');
            AorTbl = new GenericTable(Path.Combine(this._inputDir, "AOR.csv"), "EXTERNAL_IDENTITY", ',', '\"');
            
            TrsfmrTbl = new GenericTable(Path.Combine(this._inputDir, "Trsfrm_Bay_Points.csv"), "SignalName", ',', '\"'); //BD:20221212 transformer bay points

            ScadaKeyTbl = new GenericTable(Path.Combine(this._inputDir, "EDPESP_XREF.cbt"), "External ID", '\t');  //SA:20211124 file is not found in the inputs given
            //ScadaKeyTbl = new GenericTable(Path.Combine(this._inputDir, "EDPESP_XREF.csv"), "External ID", ',');
            //LockedKeys = new HashSet<string>();

            // FEP tables
            //RtuTbl = new GenericTable(Path.Combine(this._inputDir, "rtu.csv"), "IDENTIFICATION_TEXT", ',', '\"');  //SA:20211124 ; changed to ,
            RtuTbl = new GenericTable(Path.Combine(this._inputDir, "RTU_IP_Details_new.csv"), "IDENTIFICATION_TEXT", ',', '\"');  //BD:20220726 ; changed to different file with IP details
            //Hostnames = new GenericTable(Path.Combine(this._inputDir, "hostnames.txt"), "EXTERNAL_IDENTITY", '\t'); //SA:20211124 file is not found in the inputs given
            RtuTblOld = new GenericTable(Path.Combine(@"D:\Source\Repos\AEML\Database Conversion\Input\Database_Conversion\Input_Files\Database_CSV\CSV\RTU_IP_Details.csv"), "IDENTIFICATION_TEXT", ',', '\"');
            //SA:20211124 calc and iccp files are not present, so commenting for now.
            //// ICCP tables
            IdbTbl = new GenericTable(Path.Combine(this._inputDir, @"ICCP\idbtbl.csv"), "CCTR", ',', '\"');
            IddsetTbl = new GenericTable(Path.Combine(this._inputDir, @"ICCP\iddset.csv"), "IDBTBL_IRN", ',', '\"');
            IdassnTbl = new GenericTable(Path.Combine(this._inputDir, @"ICCP\idassn.csv"), "IDBTBL_IRN", ',', '\"');
            IdintrTbl = new GenericTable(Path.Combine(this._inputDir, @"ICCP\idintr.csv"), "NAME", ',', '\"');// Not found in new dump: BD
            IddValTbl = new GenericTable(Path.Combine(this._inputDir, @"ICCP\iddval.csv"), "HOST", ',', '\"');
            IddRefTbl = new GenericTable(Path.Combine(this._inputDir, @"ICCP\iddref.csv"), "DREF", ',', '\"');
            IdcPntTbl = new GenericTable(Path.Combine(this._inputDir, @"ICCP\idcpnt.csv"), "NAME", ',', '\"');// Not found in new dump: BD
            //ParametrosTbl = new GenericTable(Path.Combine(this._inputDir, @"ICCP\Parametros.csv"), "Common_Name", ',');
            //ControlCenterMapTbl = new GenericTable(Path.Combine(this._inputDir, @"ICCP\ControlCenterMapping.csv"), "Common_Name", ',');
            //ControlCenterInfoExtra = new GenericTable(Path.Combine(this._inputDir, @"ICCP\ControlCenterInfoExtra.csv"), "Name", ',');
            //
            //// Calc Tables 
            //CalcScopeTbl = new GenericTable(Path.Combine(this._inputDir, @"Calcs\CSVs\CALCS_scope.csv"), "Program Name", ',');
            //CalcFormulaLib = new StreamReader(Path.Combine(this._inputDir, @"Calcs\CSVs\calc_formula_lib.cal"));
            IscPointGroupTbl = new GenericTable(Path.Combine(this._inputDir, @"Calcs\CSVs\isc_point_group.csv"), "EXTERNAL_IDENTITY", ';');// Not found in new dump: BD
            //IscElementTbl = new GenericTable(Path.Combine(this._inputDir, @"Calcs\CSVs\isc_element.csv"), "ISC_POINT_GROUP_IRN", ';');
            //InputAndOutputTbl = new GenericTable(Path.Combine(this._inputDir, @"Calcs\CSVs\Templates_NumberOf_Inputs&Outputs.csv"), "CLASSIFICATION", ';');
            //ExecTimeTbl = new GenericTable(Path.Combine(this._inputDir, @"Calcs\CSVs\Execution_Times.txt"), "POINT GROUP CLASS", '\t');

            TransmissionTbl = new GenericTable(Path.Combine(this._inputDir, "TransmissionNew.csv"), "EQUIP", ',', '\"'); //RM for tramission 33kv 

            Trans123Tbl = new GenericTable(Path.Combine(this._inputDir, "trans123.csv"), "SW", ',', '\"');

        }

        /// <summary>
        /// Helper function to store all the locked keys from the SCADA xref and extra SCADA db into a HashSet.
        /// </summary>
        public void StoreLockedKeys()
        {
            // Used keys from XREF
            foreach (DataRow row in ScadaKeyTbl.Rows)
            {
                if (!string.IsNullOrEmpty(row["SCADA Key"].ToString()))
                {
                    LockedKeys.Add(row["SCADA Key"].ToString());
                }
            }

            // Used keys from extra SCADA objects.SA:20211209
            //DbObject statusObj = this._extraScadaDb.GetDbObject("STATUS");
            //GetUsedKeys(statusObj);
            //DbObject analogObj = this._extraScadaDb.GetDbObject("ANALOG");
            //GetUsedKeys(analogObj);
            //DbObject accumObj = this._extraScadaDb.GetDbObject("ACCUMULATOR");
            //GetUsedKeys(accumObj);
            
        }

        /// <summary>
        /// Helper function to get the used keys from extra SCADA objects.
        /// </summary>
        /// <param name="extraObj">Current extra SCADA object.</param>
        public void GetUsedKeys(DbObject extraObj)
        {
            for (int i = 1; i < extraObj.RecordCount + 1; i++)  // i = current record
            {
                extraObj.CurrentRecordNo = i;
                if (extraObj.TryGetValue("Key", 0, out object key))
                {
                    LockedKeys.Add(key.ToString());
                }
            }
        }
    }
}
