using OSII.ConversionToolkit;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespScada
{
    class EdpespDeviceInstance
    {
        private readonly SCADA _scadaDb;  // Local reference of the Scada Database

        /// <summary>
        /// Default constructor.
        /// Sets important local references. 
        /// </summary>
        /// <param name="scadaDb">Current SCADA db object.</param>
        public EdpespDeviceInstance(SCADA scadaDb)
        {
            this._scadaDb = scadaDb;
        }

        /// <summary>
        /// Helper Function to convert all DEVICE_INSTANCE objects.
        /// </summary>
        public void ConvertDeviceInstance()
        {
            Logger.OpenXMLLog();

            DbObject deviceInstanceObj = this._scadaDb.GetDbObject("DEVICE_INSTANCE");
            int deviceInstanceRec = 0;

            // Set 3 device instance objects based on mapping provided by PT
            deviceInstanceObj.CurrentRecordNo = ++deviceInstanceRec;
            deviceInstanceObj.SetValue("Name", "Modbus");

            deviceInstanceObj.CurrentRecordNo = ++deviceInstanceRec;
            deviceInstanceObj.SetValue("Name", "RTU");

            deviceInstanceObj.CurrentRecordNo = ++deviceInstanceRec;
            deviceInstanceObj.SetValue("Name", "ICCP");

            Logger.CloseXMLLog();
        }
    }
}
