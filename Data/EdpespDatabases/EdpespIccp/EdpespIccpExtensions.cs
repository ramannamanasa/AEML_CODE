using System;
using System.Collections.Generic;
using System.Data;
using OSII.ConversionToolkit;
using OSII.ConversionToolkit.Generic;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespIccp
{
    public static class EdpespIccpExtensions
    {

        /// <summary>
        /// Helper function to set pImpDs.
        /// </summary>
        /// <param name="obj">Current ICCP object.</param>
        /// <param name="irn">IRN of current ICCP object to cross reference to iddset.</param>
        /// <param name="iddSetTbl">IddSetTbl from the parser.</param>
        /// <param name="impDsList">List of pImpDs</param>
        public static void SetpImpDs(DbObject obj, string irn, GenericTable iddSetTbl, List<(string,int)> impDsList)
        {
            string name;
            if (iddSetTbl.TryGetRow(new[] { irn }, out DataRow setRow))
            {
                name = setRow["NAME"].ToString();
            }
            else
            {
                Logger.Log("UNMATCHED IRN", LoggerLevel.INFO, $"Could not match IRN to iddset: {irn}\t DbObject number: {obj.Number}");
                return;
            }
            int index = 0;
            foreach ((string, int) impDs in impDsList)
            {
                if (impDs.Item1.Equals(name))
                {
                    obj.SetValue("pImpDS", index, impDs.Item2);
                    index++;
                    // if IMPORT_POINT, return after one
                    if (obj.Number.Equals(16))
                    {
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Helper function to set the OBJ_NUM for an ICCP point.
        /// </summary>
        /// <param name="obj">Current ICCP point object.</param>
        /// <param name="row">Current DataRow row from iddval file.</param>
        public static void SetObjNum(DbObject obj, DataRow row)
        {
            string hostType = row["HOST_TYPE"].ToString();
            switch (hostType)
            {
                case "MEASURAND":
                    obj.SetValue("OBJ_NUM", (int)ScadaType.ANALOG);
                    break;
                case "INDICATION":
                    obj.SetValue("OBJ_NUM", (int)ScadaType.STATUS);
                    break;
                default:
                    Logger.Log("INVALID HOST TYPE", LoggerLevel.INFO, $"Invalid Host type for ICCP Point: {hostType}\t Obj_num not set.");
                    break;
            }
        }

        /// <summary>
        /// Helper function to set the REC_KEY for ICCP point objects.
        /// </summary>
        /// <param name="obj">Current ICCP point object.</param>
        /// <param name="row">Current DataRow row from iddval file.</param>
        /// <param name="scadaXref">SCADA XREF table.</param>
        public static void SetRecKey(DbObject obj, DataRow row, GenericTable scadaXref, bool hasDuplicate, string idbTblIRN)
        {
            string host = row["HOST_IRN"].ToString();
            if (idbTblIRN.Equals("106427201") && hasDuplicate)
            {
                host += "_Copy";
            }
            if (scadaXref.TryGetRow(new[] { host }, out DataRow hostRow))
            {
                obj.SetValue("REC_KEY", hostRow["SCADA Key"]);
            }
            else
            {
                Logger.Log("UNMATCHED HOST", LoggerLevel.INFO, $"Unmatched Host for ICCP Point, not found in SCADA XREF: {host}\t Rec_key not set.");
            }
        }

        /// <summary>
        /// Helper function to set the TYPE for an ICCP Points.
        /// </summary>
        /// <param name="obj">Current ICCP Point object.</param>
        /// <param name="row">Current DataRow row from input file.</param>
        public static void SetType(DbObject obj, DataRow row)
        {
            string hostType = row["HOST_TYPE"].ToString();
            switch (hostType)
            {
                case "MEASURAND":
                    obj.SetValue("TYPE", (int)IccpPointTypes.RealQ);
                    break;
                case "INDICATION":
                    obj.SetValue("TYPE", (int)IccpPointTypes.StateQTime);
                    break;
                default:
                    Logger.Log("INVALID HOST TYPE", LoggerLevel.INFO, $"Invalid Host type for ICCP Point:{hostType}\t Type not set.");
                    break;
            }
        }

        /// <summary>
        /// Helper function to find points to skip.
        /// </summary>
        /// <param name="IRN">MEASURAND or INDICATION IRN of current iccp point.</param>
        /// <returns>true if point is to skip. false if it should be converted.</returns>
        public static bool SkipICCPPoint(GenericTable scadaXref, string IRN)
        {
            // if the IRN is in the xref, the point was converted so this iccp point should also be converted.
            if (scadaXref.TryGetRow(new[] { IRN }, out DataRow _))
            {
                return false;
            }
            return true;
        }
    }
}
