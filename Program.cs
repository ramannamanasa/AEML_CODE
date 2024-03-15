using System;
using OSII.ConversionToolkit;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;
using edpesp_db.Data;
using System.IO;
using OSII.DatabaseToolkit.Db;
using System.Linq;
using System.Collections.Generic;

namespace aeml_db
{
    internal static class Program
    {
        private static string inputDir = null;  // Where to find input files
        private static string outputDir = null;
        static void Main(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-f":
                        if (i < args.Length - 1)
                        {
                            inputDir = args[i + 1];
                            i++;
                        }
                        break;
                    case "--help":
                        PrintUsage();
                        break;

                    default:
                        Console.WriteLine("Invalid parameter '{0}'", args[i]);
                        return;
                }
            }

            ConversionSettings.Start("AEML");
            Logger.OpenXMLLog("aeml_db");

            //SA:20211124 WE ARE GETTING SOME ERROR FILE MISSING
            //// Load in extra scada objects
            SCADA extraScadaDb = new SCADA();
            //extraScadaDb.LoadDat(Path.Combine(inputDir, "AdditionalSCADAElements.dat"));  
            //
            //EdpespParser parser = new EdpespParser(inputDir, extraScadaDb);
            EdpespParser parser = new EdpespParser(inputDir);  //SA:20211124 i have copied a new line removing 2nd argument
            parser.CreateTables();
            //parser.StoreLockedKeys();
            //
            EdpespConverter converter = new EdpespConverter(parser);
            //// Load in static DBs
            //converter.FepDb.LoadDat(Path.Combine(inputDir, "FEP_STATIC.dat"));
            //converter.AlarmsDb.LoadDat(Path.Combine(inputDir, "ALARMS_STATIC.dat"));
            //converter.StatesDb.LoadDat(Path.Combine(inputDir, "STATES_STATIC.dat"));
            converter.FepDb.LoadDat(Path.Combine(inputDir, "FEP_16_SCAN.dat"));// "Fep_gsdnew.dat"));
            converter.AlarmsDb.LoadDat(Path.Combine(inputDir, "Alarms.dat"));
            converter.StartConversion(inputDir);
            //
            //AppendScadaDb(extraScadaDb, converter);

            Logger.CloseXMLLog();

            Database[] dbs = new Database[] { converter.ScadaDb, converter.StatesDb, converter.AlarmsDb, converter.FepDb, converter.IccpDb, converter.OpenCalcDb };
            foreach (Database db in dbs)
            {
                db.Write(ConversionSettings.FullDataPath);
            }

            DatabaseSettings.End(false);
            ConversionSettings.End(true, "", "");

            Logger.CloseXMLLog();
        }

        /// <summary>
        /// Gives the user the usage of this program.
        /// </summary>
        public static void PrintUsage()
        {
            Console.WriteLine();
            Console.WriteLine("Usage:  ");
            Console.WriteLine("edpesp.exe -f inputDir [options]");
            Console.WriteLine();
            Console.WriteLine("Program will convert databases from inputDir and generate .DAT files with their information");
            Console.WriteLine("<options>");
            Console.WriteLine("--help\t\tPrint usage information");
        }

        /// <summary>
        /// Helper function to add extra DB objects at the end of the conversion.
        /// </summary>
        /// <param name="converter">Current EdpespConverter object.</param>
        public static void AppendScadaDb(SCADA extraScadaDb, EdpespConverter converter)
        {
            // Add each object present in the extra database
            DbObject extraStatusObj = extraScadaDb.GetDbObject("STATUS");
            AddScadaObject(extraStatusObj, converter);

            DbObject extraAnalogObj = extraScadaDb.GetDbObject("ANALOG");
            AddScadaObject(extraAnalogObj, converter);

            DbObject extraAccumObj = extraScadaDb.GetDbObject("ACCUMULATOR");
            AddScadaObject(extraAccumObj, converter);

            DbObject extraAccumPerObj = extraScadaDb.GetDbObject("ACCUM_PERIOD");
            AddScadaObject(extraAccumPerObj, converter);
        }

        /// <summary>
        /// Helper function to copy extra scada objects to the converter's scada DB.
        /// </summary>
        /// <param name="extraObj">Extra object to be added.</param>
        /// <param name="converter">Current EspespConverter object.</param>
        public static void AddScadaObject(DbObject extraObj, EdpespConverter converter)
        {
            DbObject scadaObj = converter.ScadaDb.GetDbObject(extraObj.Name);
            foreach (int rec in extraObj.UsedRecords)
            {
                extraObj.CurrentRecordNo = rec;
                scadaObj.CopyRecordFrom(rec, scadaObj.NextAvailableRecord, extraObj);
            }
        }
    }
}
