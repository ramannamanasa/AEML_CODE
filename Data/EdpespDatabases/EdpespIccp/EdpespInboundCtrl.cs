using System.Data;
using OSII.ConversionToolkit;
using OSII.ConversionToolkit.Generic;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespIccp
{
    class EdpespInboundCtrl
    {
        private readonly ICCP _iccpDb;  // Locacl reference to the ICCP db.
        private readonly EdpespParser _parser;  // Local reference of the Parser object.
        private readonly GenericTable _scadaXref;  // Local reference of SCADA xref

        /// <summary>
        /// Default constructor.
        /// Sets important variables.
        /// </summary>
        /// <param name="par">Current parser object.</param>
        /// <param name="iccpDb">Current ICCP database object.</param>
        /// <param name="scadaXref">SCADA XREF object.</param>
        public EdpespInboundCtrl(EdpespParser par, ICCP iccpDb, GenericTable scadaXref)
        {
            this._iccpDb = iccpDb;
            this._parser = par;
            this._scadaXref = scadaXref;
        }

        /// <summary>
        /// Function to convert all INBOUND_CTRL objects.
        /// </summary>
        public void ConvertInboundCtrl()
        {
            Logger.OpenXMLLog();

            DbObject inboundCtrlObj = this._iccpDb.GetDbObject("INBOUND_CTRL");
            int inboundCtrlRec = 0;

            // Sort xref for later
            this._scadaXref.Sort("IRN");

            // Constant values from mapping
            const int OBJ_NUM = 4;
            const int CTRL_TYPE = 9;
            const int pVCC = 1;

            foreach (DataRow inboundCtrlRow in this._parser.IdcPntTbl.Rows)
            {
                // Check for linking SCADA object first, if not present: skip.
                string key = GetRecKey(inboundCtrlRow);
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }
                // Set Record and Name
                inboundCtrlObj.CurrentRecordNo = ++inboundCtrlRec;
                inboundCtrlObj.SetValue("Name", inboundCtrlRow["NAME"].ToString());

                // Link to scada key
                inboundCtrlObj.SetValue("REC_KEY", key);

                // Set constant values from mapping
                inboundCtrlObj.SetValue("OBJ_NUM", OBJ_NUM);
                inboundCtrlObj.SetValue("CtrlType", CTRL_TYPE);
                inboundCtrlObj.SetValue("pVCC", pVCC);
            }

            Logger.CloseXMLLog();
        }

        /// <summary>
        /// Helper function to set the REC_KEY for ICCP point objects.
        /// </summary>
        /// <param name="row">Current DataRow row from iddval file.</param>
        public string GetRecKey(DataRow row)
        {
            string host = row["INDICATION_IRN"].ToString();
            if (this._scadaXref.TryGetRow(new[] { host }, out DataRow hostRow))
            {
                return hostRow["SCADA Key"].ToString();
            }
            else
            {
                Logger.Log("UNMATCHED HOST", LoggerLevel.INFO, $"Unmatched Host for ICCP Point, not found in SCADA XREF: {host}\t Rec_key not set.");
                return null;
            }
        }
    }
}
