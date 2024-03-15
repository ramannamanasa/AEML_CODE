using System;
using System.Collections.Generic;
using System.Data;
using OSII.ConversionToolkit;
using OSII.ConversionToolkit.Generic;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespFep
{
    class EdpespRtuControl
    {
        private readonly EdpespParser _parser;  // Local reference to the parser object.
        private readonly FEP _fepDb;  // Local reference to the FEP database object.
        private readonly GenericTable _scadaXref;  // Local reference to the Scada XREF.
        private readonly Dictionary<string, int> _rtuDataDict;  // Local reference to the rtuData dictionary.

        /// <summary>
        /// Default constructor.
        /// Sets important local references.
        /// </summary>
        /// <param name="par">Current Parser object.</param>
        /// <param name="fepDb">CUrrent FEP database object.</param>
        /// <param name="ScadaXref">Current Scada XREF.</param>
        /// <param name="RtuDataDict">Current RTU DATA dictionary.</param>
        public EdpespRtuControl(EdpespParser par, FEP fepDb, GenericTable ScadaXref, Dictionary<string, int> RtuDataDict)
        {
            this._parser = par;
            this._fepDb = fepDb;
            this._scadaXref = ScadaXref;
            this._rtuDataDict = RtuDataDict;
        }

        /// <summary>
        /// Function to convert all RTU_CONTROL objects.
        /// </summary>
        public void ConvertRtuControl()
        {
            Logger.OpenXMLLog();

            DbObject rtuControlObj = this._fepDb.GetDbObject("RTU_CONTROL");
            int rtuControlRec = 0;
            this._scadaXref.Sort("Name");

            foreach (DataRow rtuControlRow in this._parser.IndicationTbl.Rows)
            {
                if (rtuControlRow["DESCRIPTION"].ToString() == "OSI_NA" || rtuControlRow["EXTERNAL_IDENTITY"].ToString().EndsWith("PKUP") || rtuControlRow["EXTERNAL_IDENTITY"].ToString().EndsWith("TRIP"))
                {
                    //Logger.Log("StatusSkippedPoints", LoggerLevel.INFO, $"STATUS: Description is OSI_NA/Ends with PKUP or TRIP. ExID: {rtuControlRow["EXTERNAL_IDENTITY"].ToString()}\t ");
                    continue;

                }
                //skip the points belonging to trianagle gateway Stations:BD
                //if (new List<string> { "222452251", "1798949251", "1795852251", "2042422251", "2073195251", "2089811251", "1995038251", "2016324251", "2016325251", "1992663251", "2009856251", "2026310251", "1972835251", "2076307251", "2020391251", "2043217251" }.Contains(rtuControlRow["RTU_IRN"].ToString())) continue;
                //if (new List<string> { "138093201", "206805261", "1798949251", "1795852251", "1972835251", "1992663251", "1995038251", "2009856251", "2016324251", "2016325251", "2020391251", "2026310251", "2042422251", "2043217251", "2076307251", "222452251", "2073195251", "2089811251" }.Contains(rtuControlRow["RTU_IRN"].ToString()))
                if (new List<string> { "138093201", "222452251", "1798949251", "2089811251", "1972835251", "1995038251", "2016324251", "2016325251", "1795852251", "2009856251", "2073195251", "1992663251", "2026310251", "206805261", "2020391251", "2043217251", "2042422251", "2076307251" }.Contains(rtuControlRow["RTU_IRN"].ToString()))
                {
                    //Logger.Log("StatusSkippedPoints", LoggerLevel.INFO, $"STATUS: Belongs to Triangle gateway. ExID: {statusRow["EXTERNAL_IDENTITY"].ToString()}\t ");
                    continue;

                }
                // Check field 'CONTROLABLE_POINT' and 'PXINT1CMD' for 1 and non-0 respectively, if true create control, else continue. 
                if (!(rtuControlRow["CONTROLABLE_POINT"].ToString().Equals("1") && !string.IsNullOrEmpty(rtuControlRow["PXINT1CMD"].ToString())))
                {
                    continue;
                }
                // Set Record and ScadaKey
                rtuControlObj.CurrentRecordNo = ++rtuControlRec;
                //if (rtuControlObj.CurrentRecordNo > 1000) return; //BD: FEPCHECK
                if(rtuControlRow["IDENTIFICATION_TEXT"].ToString() == "D_SZ6     Kherwadi South 4       32248Isolator")
                {
                    int tt = 0;
                }
                DataRow xrefRow = SetScadaKey(rtuControlObj, rtuControlRow["IDENTIFICATION_TEXT"].ToString());

                // Set Name
                if (null != xrefRow)
                {
                    SetName(rtuControlObj, xrefRow);
                }
                else
                {
                    Logger.Log("MISSING RTU CONTROL NAME", LoggerLevel.INFO, $"Could not find match in xref, no name set for id_text:{rtuControlRow["IDENTIFICATION_TEXT"]}\t record:{rtuControlRec}");
                }

                // Set Point Address and pRTU
                rtuControlObj.SetValue("point_address", rtuControlRow["PXINT1CMD"].ToString());
               // var rtu = rtuControlRow["IDENTIFICATION_TEXT"].ToString().Split(' ');
                rtuControlObj.SetValue("Name", rtuControlRow["IDENTIFICATION_TEXT"].ToString());//Magesh
                SetpRtu(rtuControlObj, rtuControlRow);

                // Set Proto Parms
                SetProtoParms(rtuControlObj, rtuControlRow);

                // Set Control Type, Format, and parms
                SetControlType(rtuControlObj, rtuControlRow);
                rtuControlObj.SetValue("control_format", (int)ControlFormat.SBO);
                if (null != xrefRow && xrefRow["isQuad"].ToString().Equals("True"))
                {
                    rtuControlObj.SetValue("control_bit_parms", 1);
                }
            }

            foreach (DataRow rtuControlRow in this._parser.SetpointTbl.Rows)
            {
                //skip the points belonging to trianagle gateway Stations:BD
                if (new List<string> { "138093201", "222452251", "1798949251", "2089811251", "1972835251", "1995038251", "2016324251", "2016325251", "1795852251", "2009856251", "2073195251", "1992663251", "2026310251", "206805261", "2020391251", "2043217251", "2042422251", "2076307251" }.Contains(rtuControlRow["RTU_IRN"].ToString())) continue;
                // Only convert those with RTU_TYPE IEC104
                if (!rtuControlRow["RTU_TYPE_ASC"].ToString().Equals("I104"))
                {
                    continue;
                }
                // Check external identity to and skip those containing PROTT
                if (rtuControlRow["EXTERNAL_IDENTITY"].ToString().Contains("PROTT"))
                {
                    continue;
                }
                //skip if the IOA is empty: BD : Magesh
                if (string.IsNullOrEmpty(rtuControlRow["PXINT1"].ToString()))
                {
                    continue;
                }

                // Set Record and ScadaKey
                rtuControlObj.CurrentRecordNo = ++rtuControlRec;
                DataRow xrefRow = SetScadaKey(rtuControlObj, rtuControlRow["IDENTIFICATION_TEXT"].ToString());

                // Set Name
                if (null != xrefRow)
                {
                    SetName(rtuControlObj, xrefRow);
                }
                else
                {
                    Logger.Log("MISSING RTU CONTROL NAME", LoggerLevel.INFO, $"Could not find match in xref, no name set for id_text:{rtuControlRow["IDENTIFICATION_TEXT"]}\t record:{rtuControlRec}");
                }

                // Set Point Address and pRTU
                rtuControlObj.SetValue("point_address", rtuControlRow["PXINT1"].ToString());
                SetpRtu(rtuControlObj, rtuControlRow);

                // Set Proto Parms
                rtuControlObj.SetValue("proto_parms", 0, 0);
                rtuControlObj.SetValue("proto_parms", 1, 0);

                // Set Control Type, Format, and parms
                rtuControlObj.SetValue("control_type", (int)RTU_CONTROL_TYPE.setpoint_control);
                rtuControlObj.SetValue("control_format", (int)ControlFormat.SBO);
                if (null != xrefRow && xrefRow["isQuad"].ToString().Equals("True"))
                {
                    rtuControlObj.SetValue("control_bit_parms", 1);
                }
            }

            Logger.CloseXMLLog();
        }
        
        /// <summary>
        /// Helper function to set a SCADA Key for the RTU_CONTROLs.
        /// </summary>
        /// <param name="rtuControlObj">Current RTU_CONTROL object.</param>
        /// <param name="name">Current RTU_CONTROL name.</param>
        /// <returns>Xref row on success, else null.</returns>
        public DataRow SetScadaKey(DbObject rtuControlObj, string name)
        {
            if ( this._scadaXref.TryGetRow(new[] { name }, out DataRow xrefRow))
            {
                rtuControlObj.SetValue("SourceKey", xrefRow["SCADA Key"]); //SA:20211130 Getting error 
                //return xrefRow;
                return null;
            }
            else
            {
                Logger.Log("NO SCADA KEY MATCH", LoggerLevel.INFO, $"Could not find Scada Key for RTU_CONTROL:{name}");
                return null;
            }
        }

        /// <summary>
        /// Helper function to set control type.
        /// </summary>
        /// <param name="rtuControlObj">Current RTU_CONTROL object.</param>
        /// <param name="rtuControlRow">Current DataRow row from input.</param>
        public void SetControlType(DbObject rtuControlObj, DataRow rtuControlRow)
        {
            string commandType = rtuControlRow["COMMAND_TYPE"].ToString();
            switch (commandType)
            {
                case "C":
                    rtuControlObj.SetValue("control_type", (int)RTU_CONTROL_TYPE.close_only);
                    break;
                case "D":
                    rtuControlObj.SetValue("control_type", (int)RTU_CONTROL_TYPE.open_and_close);
                    break;
                default:
                    //rtuControlObj.SetValue("control_type", (int)RTU_CONTROL_TYPE.auto);
                    rtuControlObj.SetValue("control_type", (int)RTU_CONTROL_TYPE.close_only); // FEPCHECK: BD
                    break;
            }
        }

        /// <summary>
        /// Helper function to set pRTU.
        /// </summary>
        /// <param name="rtuControlObj">Current RTU_CONTROL object.</param>
        /// <param name="rtuControlRow">Current DataRow row from input.</param>
        public void SetpRtu(DbObject rtuControlObj, DataRow rtuControlRow)
        {
            string rtuIRN = rtuControlRow["RTU_IRN"].ToString();
            if (this._rtuDataDict.TryGetValue(rtuIRN, out int pRtu))
            {
                rtuControlObj.SetValue("pRTU", pRtu);
            }
            else
            {
                Logger.Log("MISSING pRTU", LoggerLevel.INFO, $"pRTU not found for control object. RTU IRN:{rtuIRN}");
            }
        }

        /// <summary>
        /// Helper function to set the name of RTU_CONTROL objects.
        /// </summary>
        /// <param name="rtuControlObj">Current RTU_CONTROL object.</param>
        /// <param name="xrefRow">Current xrefRow object.</param>
        public void SetName(DbObject rtuControlObj, DataRow xrefRow)
        {
            char[] key = xrefRow["SCADA Key"].ToString().ToCharArray();
            string type = xrefRow["Object"].ToString();

            if (key.Length.Equals(8))
            {
                string point = key[5].ToString() + key[6] + key[7];  // take the last 3 digits of the key
                rtuControlObj.SetValue("Name", type + '_' + point);
            }
            else
            {
                Logger.Log("BAD KEY NAME", LoggerLevel.INFO, $"Bad key found through xref:{key}");
            }
        }

        /// <summary>
        /// Helper function to set Proto Parms.
        /// </summary>
        /// <param name="rtuControlObj">Current RTU_CONTROL object.</param>
        /// <param name="rtuControlRow">Current DataRow from input file.</param>
        public void SetProtoParms(DbObject rtuControlObj, DataRow rtuControlRow)
        {
            string coll_pulse_length = rtuControlRow["COLL_PULSE_LENGTH"].ToString();
            string command_type = rtuControlRow["COMMAND_TYPE"].ToString();

            // check coll_pulse_length for proto parms index 0.
            switch (coll_pulse_length)
            {
                case "0":
                case "5":
                    rtuControlObj.SetValue("proto_parms", 0, 0);
                    break;
                default:
                    rtuControlObj.SetValue("proto_parms", 0, 2);
                    break;
            }

            // check command_type for proto parms index 1.
            switch (command_type)
            {
                case "D":
                    rtuControlObj.SetValue("proto_parms", 1, 1);
                    break;
                default:
                    rtuControlObj.SetValue("proto_parms", 1, 0);
                    break;
            }
        }
    }
}
