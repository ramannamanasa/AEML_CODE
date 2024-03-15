using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespScada
{
    class Duplicate
    {
        private readonly EdpespParser _parser;  // Local reference of the parser
        private readonly SCADA _scadaDb;

        /// <param name="par">Current Parser.</param>
        /// <param name="scadaDb">Current Scada database.</param>

        public Duplicate(EdpespParser par, SCADA scadaDb)
        {
            this._parser = par;
            this._scadaDb = scadaDb;
        }

        public enum ItemStatus
        { }
        public void Convertduplicates(string indir)
        {
            DbObject voltageObj = this._scadaDb.GetDbObject("CLASS2");
            DbObject SwitchObj = this._scadaDb.GetDbObject("DEVICE_INSTANCE");
            DbObject signalObj = this._scadaDb.GetDbObject("EQUIP");
            DbObject SubequipObj = this._scadaDb.GetDbObject("CLASS3");

            List<string> Voltage1 = new List<string>();
            List<string> Switch1 = new List<string>();
            List<string> Signal1 = new List<string>();
            List<string> Sub_Eqip1 = new List<string>();
           
            var file = (Path.Combine(indir, "measurands_e.csv"));
            int count = 1;
            using (var reader = new StreamReader(file))
            {

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    if (count != 1)
                    {
                        Voltage1.Add(values[2]);
                        Switch1.Add(values[3]);
                        Signal1.Add(values[4]);
                        Sub_Eqip1.Add(values[5]);
                        
                    }
                    count = 2;
                }
            }

            List<string> Voltage = Voltage1.Distinct().ToList();
            List<string> Switch = Switch1.Distinct().ToList();
            List<string> Signal = Signal1.Distinct().ToList();
            List<string> Sub_Eqip = Sub_Eqip1.Distinct().ToList();
            Dictionary<int, string> voltagedict = new Dictionary<int, string>();
            //var enumStatusList = Voltage.Select(x => Enum.Parse(typeof(ItemStatus), x)).Cast().ToList();
            int a=1, b=1, c=1, d = 1;

            foreach (string volt in Voltage)
            {   
                voltageObj.SetValue("Name",a, 0,volt);
                voltagedict.Add(a, volt);
                a++;
            }

            foreach (string switchs in Switch)
            {
                SwitchObj.SetValue("Name", b,0,switchs);
                b++;
            }

            foreach (string sig in Signal)
            {
                signalObj.SetValue("Name",c,0, sig);
                c++;
            }

            foreach (string Equip in Sub_Eqip)
            {
                SubequipObj.SetValue("Name",d,0,Equip);
                d++;
            }
        }
    }
}

