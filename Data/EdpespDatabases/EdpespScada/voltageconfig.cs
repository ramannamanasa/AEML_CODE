using System;
using System.Collections.Generic;
using System.Data;
using OSII.ConversionToolkit;
using OSII.ConversionToolkit.Extensions;
using OSII.ConversionToolkit.Generic;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespScada
{
    class EdpespVoltageconfig
    {
        private readonly EdpespParser _parser;  // Local reference of the parser
        private readonly SCADA _scadaDb;  // Local reference of the Scada Database
        private readonly FEP _fepDb;  // Local reference of the Fep Database
        private readonly Dictionary<string, int> _stationDict;  // Local reference of the station Dict. 
        private readonly Dictionary<string, int> _alarmsDict;  // Local reference of the alarm Dict. 
        private readonly Dictionary<string, int> _unitDict;  // Local reference of the unit dictionary.
        private readonly Dictionary<string, int> _rtuDataDict;  // Local reference to the RTU_DATA Dictionary.
        private readonly Dictionary<(int, int), int> _rtuIoaDict;  // Local reference to the RTU, subtype to IOA dictionary.
        private readonly GenericTable _scadaXref;  // Local reference to the Scada Xref
        private readonly Dictionary<string, int> primarySourceDict;  // Dictionary for primary source points. <IRN, Rec>
        private readonly Dictionary<int, string> secondarySourceDict;  // Dictionary for secondary source points. <Rec, PAIR IRN>
        private readonly Dictionary<string, int> _deviceRecDict;  // Local reference to the deviceRecDict
        private readonly Dictionary<(string, string), int> _measScales;  // Local reference to the scale dictionary
        private readonly GenericTable _scadaToFepXref;  // Local reference to the Scada to Fep Xref

        /// <summary>
        /// Default Constructor.
        /// Assigns local references of important variables.
        /// </summary>
        /// <param name="par">Current Parser.</param>
        /// <param name="scadaDb">Current Scada database.</param>
        /// <param name="fepDb">Current Fep Database object.</param>
        /// <param name="stationDict">Station dictionary object.</param>
        /// <param name="unitDict">Unit dictionary object.</param>
        /// <param name="alarmsDict">Alarm dictionary object.</param>
        /// <param name="rtuDataDict">RTU_DATA dictionary object.</param>
        /// <param name="rtuIoaDict">RTU to IOA dictionary.</param>
        /// <param name="deviceRecDict">pDeviceInstance dictionary.</param>
        /// <param name="measScales">pScale dictionary object.</param>
        /// <param name="scadaXref">Current Scada XREF object.</param>
        /// <param name="scadaToFepXref">Xref for SCADA to FEP.</param>
        public  EdpespVoltageconfig(EdpespParser par, SCADA scadaDb)
        {
            this._parser = par;
            this._scadaDb = scadaDb;
            //this._fepDb = fepDb;
            //this._stationDict = stationDict;
            //this._unitDict = unitDict;
            //this._scadaXref = scadaXref;
            //this._alarmsDict = alarmsDict;
            //this._rtuDataDict = rtuDataDict;
            //this._rtuIoaDict = rtuIoaDict;
            //this._deviceRecDict = deviceRecDict;
            //this._measScales = measScales;
            //this._scadaToFepXref = scadaToFepXref;
            //primarySourceDict = new Dictionary<string, int>();
            //secondarySourceDict = new Dictionary<int, string>();
        }

        /// <summary>
        /// Function to convert all ANALOG objects.
        /// </summary>
        public void Convertvoltageconfig()
        {
            Logger.OpenXMLLog();

            DbObject voltageconfigObj = this._scadaDb.GetDbObject("CLASS2");
    
            List<string> myaorlist = new List<string> { "","Spare", "48V", "110V", "220V", "440V", "3.3KV", "6.6KV", "11KV", "22KV", "33KV", "66KV", "110KV", "220KV", "400KV" };
            for(int i = 1; i<=  myaorlist.Count; i++)
            {
                voltageconfigObj.SetValue("Name", i, 0, myaorlist[i - 1]);
                if (i == 1) voltageconfigObj.SetValue("ConfigVersion", i, 0, 1638033238757905);
                else voltageconfigObj.SetValue("ConfigVersion", i, 0, 1638352592475411);
            }
            Logger.CloseXMLLog();
        }

        public void Convertvoltageconfig2() // temp function
        {
            Logger.OpenXMLLog();

           
            DbObject class1 = this._scadaDb.GetDbObject("CLASS1");           
            class1.SetValue("Name", 1, 0, "");
            class1.SetValue("ConfigVersion", 1, 0, "");


            DbObject class3 = this._scadaDb.GetDbObject("CLASS3");
            List<string> myclass1list = new List<string> { "", "Spare", "1+6 & 8 Breaker", "10 MVA Transformer", "2", "2 Charger", "DCDB Bus 1", "DCDB Bus 2", "4+5 & 9 Breaker", "7SJ80 NEF" };
            for (int i = 1; i <= myclass1list.Count; i++)
            {
                class3.SetValue("Name", i, 0, myclass1list[i - 1]);
                if (i == 1)class3.SetValue("ConfigVersion", i, 0, 1638033238757905); 
                else class3.SetValue("ConfigVersion", i, 0, 1638352592475411);
            }
            
            
            DbObject equip = this._scadaDb.GetDbObject("EQUIP");
            List<string> myequiplist = new List<string>{"","Spare","48V Batt Curr","48V Batt Volt","A Ph LA Count","A Phase Current","A Phase CVT","A Phase THD","A Phase Voltage","AC Inp Volt L1" };
            List<string> myequiplist2 = new List<string> { "", "Spare", "48V Battery Current", "48V Battery Voltage", "A Phase LA Count", "A Phase Current", "A Phase CVT", "A Phase Total Harmonic Distortion", "A Phase Voltage", "AC Input Voltage L1" };
            for (int i = 1; i <= myequiplist.Count; i++)
            {
                equip.CurrentRecordNo = equip.NextAvailableRecord;
                equip.SetValue("Name",  myequiplist[i - 1]);
                equip.SetValue("Description",  myequiplist2[i - 1]);
                if (i == 1) equip.SetValue("ConfigVersion", 1638033238757905);
                else equip.SetValue("ConfigVersion",  1638352592475411);
                
            }
            Logger.CloseXMLLog();
        }


        /// <summary>
        /// Helper function to get get Type.
        /// </summary>
        /// <param name="aorRow">Current DataRow row.</param>
        /// <returns></returns>
    }
}