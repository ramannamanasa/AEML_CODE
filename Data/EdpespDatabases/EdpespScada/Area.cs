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
    class EdpespArea
    {
        private readonly EdpespParser _parser;  // Local reference of the parser
        private readonly SCADA _scadaDb;  // Local reference of the Scada Database
        private readonly FEP _fepDb;  // Local reference of the Fep Database
        private readonly Dictionary<string, int> _stationDict;  // Local reference of the station Dict. 
        private readonly Dictionary<string, int> _aorDict;  // Local reference of the subsystem Dict. 
        private readonly Dictionary<string, int> _areaDict;  // Local reference of the subsystem Dict. 
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
        public EdpespArea(EdpespParser par, SCADA scadaDb, Dictionary<string, int> AorDict, Dictionary<string, int> AreaDict)
        {
            this._parser = par;
            this._scadaDb = scadaDb;
            this._aorDict = AorDict;
            this._areaDict = AreaDict;
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
        public void ConvertArea()
        {
            Logger.OpenXMLLog();

            DbObject areaObj = this._scadaDb.GetDbObject("HIERARCHY_TIER1");
            
            int areaRec = 1;

            List<string> arealist = new List<string>();
            areaObj.CurrentRecordNo = areaRec++;
            areaObj.SetValue("Name", "CSS");
            areaObj.CurrentRecordNo = areaRec++;
            areaObj.SetValue("Name", "DSS");
            areaObj.CurrentRecordNo = areaRec++;
            areaObj.SetValue("Name", "EHV");
            foreach (DataRow area in this._parser.AorTbl.Rows)//BD
            {
                
                    if (area["DSS/CSS"].ToString() == "2017779251") continue;
                    string rtuIRN = area["IRN"].ToString();
                    int arearec = 1;
                    arearec = (area["DSS/CSS"].ToString() == "CSS") ? 1 : (area["DSS/CSS"].ToString() == "DSS") ? 2 : (area["DSS/CSS"].ToString() == "EHV") ? 3 : 1;
                    if (!this._areaDict.ContainsKey(area["IRN"].ToString()))
                    {
                        this._areaDict.Add(area["IRN"].ToString(), arearec);
                    }

               
                
            }
            this._areaDict.Add("400001", 3);
                 
            this._areaDict.Add("400002", 3);
            //for (int i =1 ; i < 6; i++)
            //{
            //    if (i < 6)
            //    {
            //        switch (i)
            //        {


            //            case 1:
            //                aorObj.SetValue("Name",i,0,"AORGroup01");
            //                aorObj.SetValue("Indic",i,0,1);
            //                for (int j = 0; j < 37; j++)
            //                {
            //                    aorObj.SetValue("AORList", i, j, 1);
            //                    if (j == 0) { aorObj.SetValue("AORList", i, j, 1); }
            //                    else aorObj.SetValue("AORList", i, j, 0);
            //                }
            //                break;
            //            case 2:
            //                aorObj.SetValue("Name", i, 0, "AORGroup02");
            //                aorObj.SetValue("Indic", i, 0, 1);

            //                for (int j = 0; j < 37; j++)
            //                    if (j == 1) { aorObj.SetValue("AORList", i, j, 1); }
            //                    else aorObj.SetValue("AORList", i, j, 0);
            //                break;
            //            case 3:
            //                aorObj.SetValue("Name", i, 0, "AORGroup03");
            //                aorObj.SetValue("Indic", i, 0, 1);

            //                for (int j = 0; j < 37; j++)
            //                    if (j == 2) { aorObj.SetValue("AORList", i, j, 1); }
            //                    else aorObj.SetValue("AORList", i, j, 0);
            //                break;
            //            case 4:
            //                aorObj.SetValue("Name", i, 0, "AORGroup04");
            //                aorObj.SetValue("Indic", i, 0, 1);

            //                for (int j = 0; j < 37; j++)
            //                    if (j == 3) { aorObj.SetValue("AORList", i, j, 1); }
            //                    else aorObj.SetValue("AORList", i, j, 0);
            //                break;
            //            case 5:
            //                aorObj.SetValue("Name", i, 0, "AORGroup05");
            //                aorObj.SetValue("Indic", i, 0, 1);

            //                for (int j = 0; j < 37; j++)
            //                    if (j == 4) { aorObj.SetValue("AORList", i, j, 1); }
            //                    else aorObj.SetValue("AORList", i, j, 0);
            //                break;


            //        }
            //    }
            //}
            //for (int k = 1; k < 256; k++)
            //{
            //    for (int j = 38; j < 127; j++)
            //    {
            //        aorObj.SetValue("AORList", k, j, 0);
            //    }
            //}



            Logger.CloseXMLLog();
        }

        /// <summary>
        /// Helper function to get get Type.
        /// </summary>
        /// <param name="aorRow">Current DataRow row.</param>
        /// <returns></returns>
    }
}
