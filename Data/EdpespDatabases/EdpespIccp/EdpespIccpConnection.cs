using System.Data;
using OSII.ConversionToolkit;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespIccp
{
    class EdpespIccpConnection
    {

        private readonly ICCP _iccpDb;  // Locacl reference to the ICCP db.
        private readonly EdpespParser _parser;  // Local reference of the Parser object.

        /// <summary>
        /// Enums for ICCP_CONNECTION type.
        /// </summary>
        private enum ConnectionType
        {
            LISTENER = 3,
            ASSOCIATOR = 2
        }

        /// <summary>
        /// Default constructor. Sets important references of variables.
        /// </summary>
        /// <param name="par">Current Parser object.</param>
        /// <param name="iccpDb">Current ICCP db object.</param>
        public EdpespIccpConnection(EdpespParser par, ICCP iccpDb)
        {
            this._parser = par;
            this._iccpDb = iccpDb;
        }

        /// <summary>
        /// Function to convert all ICCP_CONNECTION objects.
        /// </summary>
        public void ConvertIccpConnection()
        {
            Logger.OpenXMLLog();

            DbObject connectionObj = this._iccpDb.GetDbObject("ICCP_CONNECTION");
            int connectionRec = 0;

            foreach (DataRow connectionRow in this._parser.IdbTbl.Rows)
            {
                // Set Record
                connectionObj.CurrentRecordNo = ++connectionRec;

                // Set Type
                SetType(connectionObj, connectionRow);
            }

            Logger.CloseXMLLog();
        }

        /// <summary>
        /// Helper function to set Type based on input name.
        /// </summary>
        /// <param name="connectionObj">Current ICCP_CONNECTION object.</param>
        /// <param name="connectionRow">Current DataRow row from input.</param>
        public void SetType(DbObject connectionObj, DataRow connectionRow)
        {
            if (connectionRow["CCTR"].ToString().StartsWith("REE_"))
            {
                connectionObj.SetValue("type", (int)ConnectionType.LISTENER);
            }
            else
            {
                connectionObj.SetValue("type", (int)ConnectionType.ASSOCIATOR);
            }
        }
    }
}
