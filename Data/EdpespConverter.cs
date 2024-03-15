using System.Collections.Generic;
using edpesp_db.Data.EdpespDatabases.EdpespAlarms;
using edpesp_db.Data.EdpespDatabases.EdpespFep;
using edpesp_db.Data.EdpespDatabases.EdpespIccp;
using edpesp_db.Data.EdpespDatabases.EdpespScada;
using edpesp_db.Data.EdpespDatabases.EdpespStates;
using edpesp_db.Data.EdpespDatabases.EdpespOpenCalc;
using OSII.ConversionToolkit;
using OSII.ConversionToolkit.Generic;
using OSII.DatabaseConversionToolkit;
using System;

namespace edpesp_db.Data
{
    class EdpespConverter
    {

        protected EdpespParser _parser;  // Local reference of the parser

        // Databases
        public SCADA ScadaDb { get; }  // Scada Database Object
        public STATES StatesDb { get; }  // States Database Object
        public ALARMS AlarmsDb { get; }  // Alarms Database Object
        public FEP FepDb { get; }  // Fep Database Object
        public ICCP IccpDb { get; }  // Iccp Database Object
        public OpenCalc OpenCalcDb { get; }  // OpenCalc Database Object

        // SCADA Tables
        private Dictionary<string, int> StationDict { get; }  // Dictionary for Station IRN to Record
        private Dictionary<string, int> SwitchDict { get; }  // Dictionary for switch IRN to Record: Equip
        private Dictionary<string, string> TransEquipGorai { get; }
        private Dictionary<string, string> TransEquipAarey { get; }
        private Dictionary<string, string> TransEquipChemb { get; }
        private Dictionary<string, string> TransEquipSaki{ get; }
        private Dictionary<string, string> TransEquipGhod { get; }
        private Dictionary<string, string> TransEquipVers { get; }

        private List<string> RTUNotFoundInOld { get; }
        private List<string> OldRTUNotFoundInNew { get; }
        private Dictionary<string, string> BayDict { get; }  // Dictionary for Station IRN to Record
        private Dictionary<string, string> StnNameDict { get; }
        private Dictionary<string, int> DivisionAorRecDict { get; }  // Dictionary for Subsystem IRN to Record: AOR
        private Dictionary<string, string> RTUIrnDivisionDict { get; }  // Dictionary for Subsystem IRN to Record: AOR
        private Dictionary<string, int> OldRTUIrnRecNoDict { get; }  // Dictionary for Old RTU IRN to Record: RTU, Station
        private Dictionary<string, int> AreaDict { get; }  // Dictionary for Area
        private Dictionary<string, int> Class2Dict { get; }  // Dictionary for Class2 Name to Record
        private Dictionary<string, int> AlarmsDict { get; }  // Dictionary for Alarm IRN to Record
        private Dictionary<string, int> UnitDict { get; }  // Dictionary for UNIT to Record
        private Dictionary<(string, string), int> OldStateDict { get; }  // Dictionary for (state1, state2) to record.
        private Dictionary<(string, string), int> NewStateDict { get; }  // Dictionary for (state1, state2) to record.
        public Dictionary<int, string> statedict { get; set; }
       private Dictionary<string, int> DeviceRecDict { get; }  // Dictionary for id to pDeviceInstance.
        private Dictionary<string, int> ScaleDict { get; }  // Dictionary for scale to rec.
        private Dictionary<(string, string), int> MeasurandScales { get; }  // Dictionary for Measurand Scales.

        private Dictionary<string, List<string>> StnRTUStatus { get; }  // Dictionary for status RTUs
        private Dictionary<string, List<string>> StnRTUAnalog{ get; }  // Dictionary for Analog RTUS.
        private Dictionary<string, List<string>> StnRTUSetPoint { get; }  // Dictionary for Setpoint RTUs.
        //private Dictionary<string, string> StnNameDict { get; }
        // FEP Tables
        private Dictionary<string, int> ChannelDict { get; }  // Dictionary for Channel to Record
        private Dictionary<string, int> ChannelGroupDict { get; }  // Dictionary for Channel Group to Record
        private Dictionary<string, int> RtuDataDict { get; }  // Dictionary for RTU_DATA to Record
        private Dictionary<(int, int), int> RtuIoaDict { get; }  // Dictionary for RTU, subtype to IOA value.

        // ICCP Tables
        private Dictionary<string, int> ControlCenterDict { get; }  // Dictionary for CONTROL_CENTER_INFO to rec
        private List<(string, int)> ImpDsList { get; }  // List for IMPORT_DS to rec
        private List<(string, int)> ExpDsList { get; }  // List for EXPORT_DS to rec

        // OpenCalc Tables
        private Dictionary<string, int> TimerDict { get; }  // Dictionary for TIMER to rec
        private Dictionary<string, int> FormulaTemplateDict { get; }  // Dictionary for FORMULA_TEMPLATE to rec

        // XREFS
        protected static readonly string[] scadaXrefFields = { "Object", "Name", "SCADA Key", "pStation", "OSI Record",
            "External ID", "pRtu", "FEP PointType", "Address", "IOA", "IRN", "isQuad", "ExtID","SCADA Type" };  // Array of fields for the Scada Xref
        private GenericTable ScadaXref { get; }  // Scada XREF as a GenericTable Object
        protected static readonly string[] scadaToFepFields = { "Object", "OSI Record", "pRtu", "FEP Type", "SCADA Key", "IOA", "isQuad", "Address", "ExtrnID" };  // Array of fields for the Scada to Fep Xref
        private readonly GenericTable ScadaToFepXref;  // XREF to create RTU points and link Scan Data
        

        /// <summary>
        /// Default Constructor.
        /// Sets local copies and initiliazes important variables.
        /// </summary>
        /// <param name="par">Current Parser Object.</param>
        public EdpespConverter(EdpespParser par)
        {
            this._parser = par;
            ScadaDb = new SCADA();
            StatesDb = new STATES();
            AlarmsDb = new ALARMS();
            FepDb = new FEP();
            IccpDb = new ICCP();
            OpenCalcDb = new OpenCalc();
            StationDict = new Dictionary<string, int>();
            StnNameDict= new Dictionary<string, string>();
            DivisionAorRecDict = new Dictionary<string, int>();
            OldRTUIrnRecNoDict = new Dictionary<string, int>();
            RTUNotFoundInOld = new List<string>();
            OldRTUNotFoundInNew = new List<string>();
            RTUIrnDivisionDict = new Dictionary<string, string>();
            Class2Dict = new Dictionary<string, int>();
            AlarmsDict = new Dictionary<string, int>();
            UnitDict = new Dictionary<string, int>();
            OldStateDict = new Dictionary<(string, string), int>();
            NewStateDict = new Dictionary<(string, string), int>();
            ChannelDict = new Dictionary<string, int>();
            ChannelGroupDict = new Dictionary<string, int>();
            RtuDataDict = new Dictionary<string, int>();
            RtuIoaDict = new Dictionary<(int, int), int>();
            ControlCenterDict = new Dictionary<string, int>();
            TimerDict = new Dictionary<string, int>();
            FormulaTemplateDict = new Dictionary<string, int>();
            ImpDsList = new List<(string, int)>();
            ExpDsList = new List<(string, int)>();
            DeviceRecDict = new Dictionary<string, int>();
            ScaleDict = new Dictionary<string, int>();
            MeasurandScales = new Dictionary<(string, string), int>();
            ScadaXref = new GenericTable(scadaXrefFields, "OSI Record");
            ScadaToFepXref = new GenericTable(scadaToFepFields, "pRtu, FEP Type, IOA");
            statedict = new Dictionary<int, string>();
            BayDict = new Dictionary<string, string>();
            AreaDict = new Dictionary<string, int>();
            SwitchDict = new Dictionary<string, int>();
            TransEquipGorai = new Dictionary<string, string>();
            TransEquipAarey = new Dictionary<string, string>();
            TransEquipChemb = new Dictionary<string, string>();
            TransEquipSaki = new Dictionary<string, string>();
            TransEquipGhod = new Dictionary<string, string>();
            TransEquipVers = new Dictionary<string, string>();
        }

        /// <summary>
        /// Function used to start the conversion.
        /// Calls each object's conversion proccess in each database.
        /// </summary>
        public virtual void StartConversion(string input)
        {
            Logger.OpenXMLLog();

            Duplicate duplicate = new Duplicate(this._parser, ScadaDb);
            duplicate.Convertduplicates(input);

            EdpespState state = new EdpespState(this._parser, StatesDb, OldStateDict, NewStateDict,statedict, BayDict);
            state.ConvertState();

            // Convert ALARMS Database Objects
            EdpespAlarmGroup alarmGroup = new EdpespAlarmGroup(this._parser, AlarmsDb, AlarmsDict);
            alarmGroup.ConvertAlarmGroup();

            

            // Convert SCADA Database Objects
            EdpespUnit unit = new EdpespUnit(this._parser, ScadaDb, UnitDict);
            unit.ConvertUnit();

            EdpespDeviceInstance deviceInstance = new EdpespDeviceInstance(ScadaDb);
            //deviceInstance.ConvertDeviceInstance();  //SA:20211124 I think we don't have device_instance object in the scada database

            EdpespAor aor = new EdpespAor(this._parser, ScadaDb, DivisionAorRecDict, AreaDict, RTUIrnDivisionDict);
            aor.ConvertAor();

            EdpespEquip equip = new EdpespEquip(this._parser, ScadaDb, SwitchDict, TransEquipGorai, TransEquipAarey, TransEquipChemb, TransEquipSaki, TransEquipGhod, TransEquipVers);
            equip.ConvertEquip();
            EdpespArea area = new EdpespArea(this._parser, ScadaDb, DivisionAorRecDict, AreaDict);
            area.ConvertArea();

            EdpespStation station = new EdpespStation(this._parser, ScadaDb, StationDict, DivisionAorRecDict, StnNameDict, AreaDict, RTUIrnDivisionDict, OldRTUIrnRecNoDict, RTUNotFoundInOld, OldRTUNotFoundInNew);
            station.ConvertStation();

            //Convert first FEP Database Objects
            EdpespChannel channel = new EdpespChannel(this._parser, FepDb, ChannelDict, OldRTUIrnRecNoDict);
            channel.ConvertChannel();

            EdpespChannelGroup channelGroup = new EdpespChannelGroup(this._parser, FepDb, ChannelGroupDict, ChannelDict, OldRTUIrnRecNoDict, RTUIrnDivisionDict, DivisionAorRecDict);
            channelGroup.ConvertChannelGroup();

            EdpespRtuData rtuData = new EdpespRtuData(this._parser, FepDb, ChannelGroupDict, RtuDataDict, OldRTUIrnRecNoDict, RTUIrnDivisionDict, DivisionAorRecDict);
            rtuData.ConvertRtuData();

            EdpespClass2 class2 = new EdpespClass2(this._parser, ScadaDb, Class2Dict);
            //class2.ConvertClass2(); //SA:20111124

            EdpespScale scale = new EdpespScale(this._parser, ScadaDb, ScaleDict, MeasurandScales);
            scale.ConvertScale();

            EdpespStatus status = new EdpespStatus(this._parser, ScadaDb, FepDb, StationDict, BayDict, OldStateDict, NewStateDict, AlarmsDict, Class2Dict, RtuDataDict, RtuIoaDict, ScadaXref, ScadaToFepXref, statedict, DivisionAorRecDict, StnNameDict, RTUIrnDivisionDict);
            status.ConvertStatus();

            EdpespAnalog analog = new EdpespAnalog(this._parser, ScadaDb, FepDb, StationDict, UnitDict, AlarmsDict, RtuDataDict, RtuIoaDict, DeviceRecDict, MeasurandScales, ScadaXref, ScadaToFepXref, DivisionAorRecDict, ScaleDict, StnNameDict,
                RTUIrnDivisionDict, SwitchDict, TransEquipGorai, TransEquipAarey, TransEquipChemb, TransEquipSaki, TransEquipGhod, TransEquipVers);
            analog.ConvertAnalog();

            EdpespAnalogConfig analogConfig = new EdpespAnalogConfig(this._parser, ScadaDb, StationDict, Class2Dict, DeviceRecDict);
            //analogConfig.ConvertAnalogConfig();


            EdpespVoltageconfig voltage = new EdpespVoltageconfig(this._parser, ScadaDb);
            voltage.Convertvoltageconfig();
            voltage.Convertvoltageconfig2();

            EdpespAccumPeriod accumPeriod = new EdpespAccumPeriod(ScadaDb);
           // accumPeriod.ConvertAccumPeriod(); //SA:20111124

            EdpespAccumulator accumulator = new EdpespAccumulator(this._parser, ScadaDb, FepDb, StationDict, UnitDict, Class2Dict, AlarmsDict, RtuDataDict, RtuIoaDict, ScaleDict, ScadaXref, ScadaToFepXref, DivisionAorRecDict);
            //accumulator.ConvertAccumulator(); //SA:20111124

            EdpespSetpoint setpoint = new EdpespSetpoint(this._parser, ScadaDb, StationDict, UnitDict, Class2Dict, RtuDataDict, ScaleDict, ScadaXref, DivisionAorRecDict, StnNameDict);
            setpoint.ConvertSetpoint();

            // convert RTU_DEFN, update quads, and first fep build

            //EdpespRtuDefn rtuDefn = new EdpespRtuDefn(FepDb, ScadaToFepXref);
            //rtuDefn.ConvertRtuDefn();//FEPCHECK


            //FepDb.UpdateQuads();
            //SA:20211126 taking long time to run.
            //FEPCHECK
            //try
            //{
             //   FepDb.PopulateAndFepBuild(false, false, false, true);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}
            //FepDb.PopulateAndFepBuild(false, false, false, true);//:BD 

            //Convert the rest of FEP Database Objects
            EdpespRtuControl rtuControl = new EdpespRtuControl(this._parser, FepDb, ScadaXref, RtuDataDict);
            //SA:20211125 rtu is taking more than 1 hr but still its converting so commenting it for testing 
            rtuControl.ConvertRtuControl();

            //EdpespRtuDefn rtuDefn = new EdpespRtuDefn(FepDb, ScadaToFepXref);
            ////SA:20211125 rtu is taking more than 1 hr but still its converting so commenting it for testing 
            //rtuDefn.ConvertRtuDefn();

            EdpespInitScanDefn initScanDefn = new EdpespInitScanDefn(FepDb);
            initScanDefn.ConvertInitScanDefn();

            EdpespScanDefn ScanDefn = new EdpespScanDefn(FepDb);
            ScanDefn.ConvertScanDefn(); //Added newly: BD

            EdespDemandScanDefn DemandScanDefn = new EdespDemandScanDefn(FepDb);
            DemandScanDefn.ConvertDemandScanDefn(); //Added newly: BD

            EdpespFepHeader fepHeader = new EdpespFepHeader(FepDb);
            fepHeader.ConvertFepHeader();

            EdpespScanData scanData = new EdpespScanData(FepDb, ScadaToFepXref);
            scanData.ConvertScanData();//:BD FEPCHECK

            // Convert ICCP Database Objects
            //SA:20211124 iccp file is not there
            this._parser.IddValTbl.Sort("NAME");
            EdpespControlCenterInfo controlCenterInfo = new EdpespControlCenterInfo(this._parser, IccpDb, ControlCenterDict);
            //controlCenterInfo.ConvertControlCenterInfo(); //Uncommented by BD

            EdpespIccpExportDs iccpExportDs = new EdpespIccpExportDs(this._parser, IccpDb, ExpDsList);
            iccpExportDs.ConvertIccpExportDs(); //Uncommented by BD

            EdpespIccpImportDs iccpImportDs = new EdpespIccpImportDs(this._parser, IccpDb, ImpDsList);
            iccpImportDs.ConvertIccpImportDs(); //Uncommented by BD

            EdpespVccInfo vccInfo = new EdpespVccInfo(this._parser, IccpDb, ControlCenterDict, ImpDsList);
            vccInfo.ConvertVccInfo(); //Uncommented by BD

            EdpespIccpScanClass iccpScanClass = new EdpespIccpScanClass(IccpDb);
            iccpScanClass.ConvertIccpScanClass(); //Uncommented by BD

            EdpespIccpConnection iccpConnection = new EdpespIccpConnection(this._parser, IccpDb);
            iccpConnection.ConvertIccpConnection(); //Uncommented by BD

            EdpespIccpExportPoint iccpExportPoint = new EdpespIccpExportPoint(this._parser, IccpDb, ExpDsList, ScadaXref);
            iccpExportPoint.ConvertIccpExportPoint(); //Uncommented by BD

            EdpespIccpImportPoint iccpImportPoint = new EdpespIccpImportPoint(this._parser, IccpDb, ImpDsList, ScadaXref);
            iccpImportPoint.ConvertIccpImportPoint(); //Uncommented by BD

            EdpespInboundCtrl inboundCtrl = new EdpespInboundCtrl(this._parser, IccpDb, ScadaXref);
            inboundCtrl.ConvertInboundCtrl(); //Uncommented by BD

            // Convert OpenCalc objects
            EdpespTimer timer = new EdpespTimer(OpenCalcDb, TimerDict);
            timer.ConvertTimer(); //Uncommented by BD

            EdpespExecutionGroup executionGroup = new EdpespExecutionGroup(OpenCalcDb);
            executionGroup.ConvertExecutionGroup(); //Uncommented by BD

            EdpespFormulaTemplate formulaTemplate = new EdpespFormulaTemplate(this._parser, OpenCalcDb, FormulaTemplateDict);
            //formulaTemplate.ConverFormulaTemplate(); //Uncommented by BD

            EdpespFormula edpespFormula = new EdpespFormula(this._parser, OpenCalcDb, FormulaTemplateDict, ScadaXref);
            edpespFormula.ConvertFormula(); //Uncommented by BD

            // Populate and Validate
            DbUtilities.WriteAndPopulate(StatesDb);
            DbUtilities.WriteAndPopulate(AlarmsDb);
            ScadaDb.PopulateAndScadaValidate();

            //FEPCHECK
            //try
            //{
            //    FepDb.PopulateAndFepBuild(false, false, false, false);
            //}
            //catch (Exception ex)
            //{
              //  Console.WriteLine(ex.Message);
            //}

            //IccpDb.PopulateAndIccpBuild(false, false);

            // Write XREFs
            Console.WriteLine("RTU and Stations:");
            
            ScadaXref.WriteToDefault("AEML_XREF");
            ScadaToFepXref.WriteToDefault("SCADA_TO_FEP_XREF");
            
            Logger.CloseXMLLog();
        }
    }
}
