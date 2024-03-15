using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using OSII.ConversionToolkit;
using OSII.ConversionToolkit.Extensions;
using OSII.ConversionToolkit.Generic;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;
//using Microsoft.NET.StringTools;

namespace edpesp_db.Data.EdpespDatabases.EdpespScada
{
    public static class EdpespScadaExtensions
    {
        private static readonly Dictionary<(int, I104PointTypes), int> rtuDefnAddresses = new Dictionary<(int, I104PointTypes), int>();  // Dictionary for next available rtu_defn address       
        public static int nullcount = 0;

        /// <summary>
        /// DefaultFepPrefixes for IEC104
        /// </summary>
        public enum DefaultFepPrefixes
        {
            STATUS = 1,
            QUAD_STATUS = 2,
            ANALOG = 3,
            ACCUMULATOR = 5,
            SETPOINT = 7
        }

        /// <summary>
        /// Point Types for I104 protocol objects.
        /// </summary>
        public enum I104PointTypes
        {
            None = 0,
            SINGLE_PT = 1,
            DOUBLE_PT = 2,
            MEAS_VALUE = 3,
            INT_TOTAL = 5
        }

        /// <summary>
        /// pRecord values for Device Instance
        /// </summary>
        public enum DeviceInstance
        {
            None = 0,
            MODBUS = 1,
            RTU = 2,
            ICCP = 3
        }

        public static string Truncate(string source, int length) //SA:2021210
        {
            if (source == null) { return null; }
            if (source.Length > length)
            {
                source = source.Substring(0, length);
            }
            return source;
        }

        /// <summary>
        /// Function to set the SCADA key of a scada object.
        /// </summary>
        /// <param name="obj">Current scada object.</param>
        /// <param name="type">Current scada object's type.</param>
        /// <param name="pStation">Current pStation.</param>
        /// <param name="scadaDb">Scada database object.</param>
        /// <param name="scadaType">Scada Type of dbObj</param>
        /// <param name="scadaKeyTbl">Scada XREF of locked keys.</param>
        /// <param name="lockedKeys">HashSet of locked keys.</param>
        /// <param name="externalID">Current object's External ID.</param>
        /// <returns>String value of the key.</returns>
        public static string SetScadaKey(DbObject obj, int type, int pStation, SCADA scadaDb, ScadaType scadaType,
                GenericTable scadaKeyTbl, HashSet<string> lockedKeys, string externalID, bool isQuad)
        {
            string key = string.Empty;
            DbObject stationObj = scadaDb.GetDbObject("STATION");
            stationObj.CurrentRecordNo = pStation;

            // string stationKey = stationObj.GetValue("Key", 0); SA:20211210
            //string stationKey = stationObj.GetValue("Key", 0);
            //string stationKey = pStation.ToString();
            //if (!string.IsNullOrEmpty(stationKey))
            //{
            //stationKey = Truncate(stationKey, 2);
            //string stationKey = Decode(pStation).ToString();
            string stationKey = pStation.ToString();
            //}
            //if (externalID == "33kV B9SW94 E/F INST-2 Trip")
            //{
            //    var dummyvar = 0;
            //}
            //stationKey = Decode(stationKey).ToString();
            const int ICCP_STATUS = 11;    //xx xxx 99Z
            const int ICCP_ANALOG = 13;
            switch (scadaType)
            {
                case ScadaType.STATUS:
                    switch (type)
                    {
                        case (int)StatusType.T_IND:
                            //SA:20211125 scadakey extension
                            if(isQuad) key = GenerateEdpespKey(scadaDb, 02, stationKey, externalID, scadaKeyTbl, lockedKeys);
                            else key = GenerateEdpespKey(scadaDb, (int)ScadaKeyPrefix.T_IND, stationKey, externalID, scadaKeyTbl, lockedKeys);
                            break;
                        case (int)StatusType.T_IC:
                            if (isQuad) key = key = GenerateEdpespKey(scadaDb, 02, stationKey, externalID, scadaKeyTbl, lockedKeys);
                            else key = GenerateEdpespKey(scadaDb, (int)ScadaKeyPrefix.T_IC, stationKey, externalID, scadaKeyTbl, lockedKeys);
                            break;
                        case (int)StatusType.T_CTL:
                            if(isQuad) key = GenerateEdpespKey(scadaDb, 02, stationKey, externalID, scadaKeyTbl, lockedKeys);
                            else key = GenerateEdpespKey(scadaDb, (int)ScadaKeyPrefix.T_CTL, stationKey, externalID, scadaKeyTbl, lockedKeys);
                            break;
                        case (int)StatusType.C_IND:
                            //key = GenerateEdpespKey(scadaDb, (int)ScadaKeyPrefix.C_IND, stationKey, externalID, scadaKeyTbl, lockedKeys);
                            key = GenerateEdpespKey(scadaDb, 04, stationKey, externalID, scadaKeyTbl, lockedKeys); // BD taking 04 for all calc points
                            break;
                        case (int)StatusType.M_IND:
                            key = GenerateEdpespKey(scadaDb, (int)ScadaKeyPrefix.M_IND, stationKey, externalID, scadaKeyTbl, lockedKeys);
                            break;
                        case ICCP_STATUS:
                            key = GenerateEdpespKey(scadaDb, ICCP_STATUS, stationKey, externalID, scadaKeyTbl, lockedKeys);
                            break;
                        default:
                            Logger.Log("BAD KEY TYPE", LoggerLevel.INFO, $"STATUS: Bad type: {type}\t No key assigned.");
                            break;
                    }
                    break;
                case ScadaType.ANALOG:
                    switch (type)
                    {    //SA:20211125 we dont have locked keys
                        case (int)AnalogType.T_ANLG:
                            key = GenerateEdpespKey(scadaDb, (int)ScadaKeyPrefix.T_ANLG, stationKey, externalID, scadaKeyTbl, lockedKeys);
                            break;
                        case (int)AnalogType.C_ANLG:
                            key = GenerateEdpespKey(scadaDb, (int)ScadaKeyPrefix.C_ANLG, stationKey, externalID, scadaKeyTbl, lockedKeys);
                            break;
                        case (int)AnalogType.M_ANLG:
                            key = GenerateEdpespKey(scadaDb, (int)ScadaKeyPrefix.M_ANLG, stationKey, externalID, scadaKeyTbl, lockedKeys);
                            break;
                        case ICCP_ANALOG:
                            key = GenerateEdpespKey(scadaDb, ICCP_ANALOG, stationKey, externalID, scadaKeyTbl, lockedKeys);
                            break;
                        default:
                            Logger.Log("BAD KEY TYPE", LoggerLevel.INFO, $"ANALOG: Bad type: {type}\t No key assigned.");
                            break;
                    }
                    break;
                //case ScadaType.ACCUMULATOR:
                //    switch (type)
                //    {
                //        case (int)AccumulatorType.T_ACCUM:
                //            key = GenerateEdpespKey(scadaDb, (int)ScadaKeyPrefix.T_ACCUM, stationKey, externalID, scadaKeyTbl, lockedKeys);
                //            break;
                //        case (int)AccumulatorType.C_ACCUM:
                //            key = GenerateEdpespKey(scadaDb, (int)ScadaKeyPrefix.C_ACUMM, stationKey, externalID, scadaKeyTbl, lockedKeys);
                //            break;
                //        case (int)AccumulatorType.M_ACCUM:
                //            key = GenerateEdpespKey(scadaDb, (int)ScadaKeyPrefix.M_ACCUM, stationKey, externalID, scadaKeyTbl, lockedKeys);
                //            break;
                //        default:
                //            Logger.Log("BAD KEY TYPE", LoggerLevel.INFO, $"ACCUMULATOR: Bad type: {type}\t No key assigned.");
                //            break;
                //    }
                //    break;
                case ScadaType.SETPOINT:
                    switch (type)
                    {    //SA:20211125 we dont have locked keys
                        case (int)SetpointType.C_STPNT:
                            key = GenerateEdpespKey(scadaDb, (int)ScadaKeyPrefix.C_STPNT, stationKey, externalID, scadaKeyTbl, lockedKeys);
                            break;
                        case (int)SetpointType.T_STPNT:
                            key = GenerateEdpespKey(scadaDb, (int)ScadaKeyPrefix.T_STPNT, stationKey, externalID, scadaKeyTbl, lockedKeys);
                            break;
                        case (int)SetpointType.M_STPNT:
                            key = GenerateEdpespKey(scadaDb, (int)ScadaKeyPrefix.M_STPNT, stationKey, externalID, scadaKeyTbl, lockedKeys);
                            break;
                        default:
                            Logger.Log("BAD KEY TYPE", LoggerLevel.INFO, $"SETPOINT: Bad type: {type}\t No key assigned.");
                            break;
                    }
                    break;
                default:
                    Logger.Log("BAD KEY TYPE", LoggerLevel.INFO, $"SCADA: Bad type: {scadaType}\t No key assigned.");
                    break;
            }
            if (!string.IsNullOrEmpty(key))
            {
                obj.SetValue("Key", key);
            }
            return key;
        }

        /// <summary>
        /// Helper function to find a point's pStation.
        /// </summary>
        /// <param name="stationIRN">Current point's Station IRN.</param>
        /// <param name="stationDict">Station Dictionary.</param>
        /// <returns>Returns pStation rec on success and 0 if it can't be found.</returns>
        public static int GetpStation(string stationIRN, Dictionary<string, int> stationDict)
        {
            if (stationDict.ContainsKey(stationIRN))
            {
                return stationDict[stationIRN];
            }
            else
            {
                Logger.Log("INVALID STATION IRN", LoggerLevel.INFO, $"Provided Station IRN was not found: {stationIRN}\tSetting pStation to 0");
                return 0;
            }
        }
        public static int GetpAorGroup2(string RTUIRN, Dictionary<string, int> AorDict) //BD
        {
            
            if (AorDict.ContainsKey(RTUIRN))
            {
                return AorDict[RTUIRN];
            }
            else
            {
                if (RTUIRN == "3792201")
                {
                    int t = 0;
                }
                Logger.Log("INVALID STATION IRN", LoggerLevel.INFO, $"Provided RTU IRN was not found: {RTUIRN}\tSetting pAORGRoup to 1");
                return 1;
            }
        }
        /// <summary>
        /// Helper function to find a point's pStation.
        /// </summary>
        /// <param name="SubsystemIRN">Current point's Subsystem IRN.</param>
        /// <param name="AorDict">Aorgroup Dictionary.</param>
        /// <returns>Returns pStation rec on success and 0 if it can't be found.</returns>
        public static int GetpAorGroup(string RTUIRN, Dictionary<string, int> AorDict, Dictionary<string, string> DivisionAorDict) //BD
        {
            if (RTUIRN == "400001") return 8;
            if (RTUIRN == "400002") return  8;
            string division = "";
            if (DivisionAorDict.ContainsKey(RTUIRN))
            {
                division=  DivisionAorDict[RTUIRN];
            }
            else
            {
                if (RTUIRN == "3792201")
                {
                    int t = 0;
                }
                Logger.Log("DivisionNotFound", LoggerLevel.INFO, $"Division not found for provided RTU IRN: {RTUIRN}\tSetting pAORGRoup to 1");
                //return "";
            }
            if (AorDict.ContainsKey(division))
            {
                return AorDict[division];
            }
            else
            {
                if(RTUIRN == "3792201")
                {
                    int t = 0;
                }
                Logger.Log("INVALID STATION IRN", LoggerLevel.INFO, $"Provided RTU IRN was not found: {RTUIRN}\tSetting pAORGRoup to 1");
                return 1;
            }
        }
        public static int GetArea(string RTUIRN, Dictionary<string, int> AreaDict) //BD
        {
            if (AreaDict.ContainsKey(RTUIRN))
            {
                return AreaDict[RTUIRN];
            }
            else
            {
                Logger.Log("AOR not found", LoggerLevel.INFO, $"Provided RTU IRN was not found: {RTUIRN}\tSetting pAORGRoup to 1");
                return 1;
            }
        }
        /// <summary>
        /// Helper function to set pUnit.
        /// </summary>
        /// <param name="obj">Current database object.</param>
        /// <param name="row">Current DataRow row.</param>
        /// <param name="unitDict">Unit Dictionary.</param>
        public static void SetpUnit(DbObject obj, DataRow row, Dictionary<string, int> unitDict)
        {
            string engineeringUnit = row["ENGINEERING_UNIT"].ToString();
            if (unitDict.ContainsKey(engineeringUnit))
            {
                obj.SetValue("pUNIT", unitDict[engineeringUnit]);
            }
            else
            {
                Logger.Log("INVALID ENGINEERING UNIT", LoggerLevel.INFO, $"Engineering unit does not have a matching pUNIT: {engineeringUnit}\t Name:{obj.GetValue("Name", 0)}");
            }
        }

        /// <summary>
        /// Helper function to set pClass2.
        /// </summary>
        /// <param name="obj">Current SCADA object.</param>
        /// <param name="rawName">Stirng value from input of the name.</param>
        /// <param name="class2Dict">Class2Dictionary.</param>
        /// <param name="stationAbbrTbl">Station Abbr Table from parser object.</param>
        public static string SetpClass2(DbObject obj, string rawName, Dictionary<string, int> class2Dict, GenericTable stationAbbrTbl)
        {
            string voltage = ExtractVoltage(rawName, stationAbbrTbl);
            //SA:20211125 Skipping Extract Voltage 

            //string voltage = "";
            if (class2Dict.TryGetValue(voltage, out int pClass2))
            {
                obj.SetValue("pClass2", pClass2);
                return voltage;
            }
            else
            {
                Logger.Log("UNFOUND pCLASS2", LoggerLevel.INFO, $"Class2 not found for voltage: {voltage}\t Name:{rawName}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Helper function to trim and get name for SCADA objects.
        /// Splits the name by spaces since the leading text is always different.
        /// </summary>
        /// <param name="rawName">Raw name value from input of current STATUS obj.</param>
        /// <param name="stationAbbrTbl">Station Abbr Table from parser object.</param>
        /// <returns>Returns a trimmed name value.</returns>
        /// 

        public static string GetName(string rawName, GenericTable stationAbbrTbl)
        {
            string[] nameArr = rawName.Split(' ');
            rawName=rawName.Replace(nameArr[0],"");
            rawName = rawName.TrimStart();
            //if (nameArr.Length > 1)
            //{
            //    StringBuilder newName = new StringBuilder();
            //    for (int i = 1; i < nameArr.Length; i++)
            //    {
            //        string tempName = nameArr[0] + "_" + nameArr[i];
            //        //if (i == 1 && (stationAbbrTbl.TryGetRow(new[] { tempName }, out DataRow _) || tempName.Equals("Soto_Rib.")))  // Special Case to look for
            //        //{
            //        //    continue;
            //        //}
            //        // skip empty characters
            //        if (!string.IsNullOrEmpty(nameArr[i]))
            //        {
            //            newName.Append(nameArr[i]);
            //            // If it is not the last string piece, add a space to match original.
            //            if (i < nameArr.Length - 1)
            //            {
            //                newName.Append(" ");
            //            }
            //        }
            //    }
            //    return RemoveSubstation(newName.ToString());
            //}
            //else
            //{
            //    return rawName;
            //}

            return rawName;
        }

        //public static string GetName(string rawName, GenericTable stationAbbrTbl)
        //{
        //    string[] nameArr = rawName.Split(' ');

        //    if (nameArr.Length > 1)
        //    {

        //        //string tempName = rawName.Replace(nameArr[0], ""); naming convetion 
        //        StringBuilder newName = new StringBuilder();
        //        string tempName = string.Empty;
        //        for (int i = 1; i < nameArr.Length; i++)
        //        {
        //            //tempName = nameArr[0] + "_" + nameArr[i];
        //            if (string.IsNullOrEmpty(nameArr[i])) nameArr[i] = " ";
        //            tempName = tempName + nameArr[i];
        //            //if (i == 1 && (stationAbbrTbl.TryGetRow(new[] { tempName }, out DataRow _) || tempName.Equals("Soto_Rib.")))  // Special Case to look for //SA:20211207
        //            {
        //                //continue;
        //            }
        //            // skip empty characters
        //            //if (!string.IsNullOrEmpty(nameArr[i]))
        //            //{
        //            //    newName.Append(nameArr[i]);
        //            //    // If it is not the last string piece, add a space to match original.
        //            //    if (i < nameArr.Length - 1)
        //            //    {
        //            //        newName.Append(" ");
        //            //    }
        //            //}
        //            //newName = tempName.ToString();
        //        }
        //        return RemoveSubstation(newName.ToString());
        //        //return tempName; naming convetion
        //    }
        //    else
        //    {
        //        return rawName;
        //    }
        //}

        /// <summary>
        /// Helper function to find the pDeviceInstance.
        /// Also removes strings from name.
        /// </summary>
        /// <param name="name">Current name value.</param>
        /// <param name="isIccp">Boolean value to describe if the object is part of ICCP.</param>
        /// <param name="pDeviceInstance">Pointer object for pDeviceInstance.</param>
        /// <returns>Modified string of the name.</returns>
        public static string GetpDeviceInstance(string name, bool isIccp, out int pDeviceInstance)
        {
            if (name.StartsWith("Mbus ") || name.StartsWith("Mbus.") || name.StartsWith("Modbus "))
            {
                pDeviceInstance = (int)DeviceInstance.MODBUS;
                name = name.Replace("Mbus ", "");
                name = name.Replace("Mbus.", "");
                name = name.Replace("Modbus ", "");
                return name;
            }
            else if (!isIccp)
            {
                pDeviceInstance = (int)DeviceInstance.RTU;
                if (name.StartsWith("Gen ") || name.StartsWith("Gen.") || name.StartsWith("GEN ")
                        || name.StartsWith("RTU ") || name.EndsWith(" RTU") || name.EndsWith(".RTU") || name.Contains(" RTU ")
                            || name.StartsWith("General ") || name.EndsWith(" General") || name.Contains(" General "))
                {
                    name = name.Replace("Gen ", "");
                    name = name.Replace("Gen.", "");
                    name = name.Replace("GEN ", "");
                    name = name.Replace("RTU ", "");
                    name = name.Replace(" RTU", "");
                    name = name.Replace(".RTU", "");
                    name = name.Replace(" RTU ", "");
                    name = name.Replace("General ", "");
                    name = name.Replace(" General", "");
                    name = name.Replace(" General ", "");
                }
                return name;
            }
            else
            {
                pDeviceInstance = (int)DeviceInstance.ICCP;
                return name;
            }
        }

        /// <summary>
        /// Helper function to trim name and get voltage value.
        /// Splits the name by spaces since the leading text is always different.
        /// </summary>
        /// <param name="rawName">Raw ID name from input file.</param>
        /// <param name="stationAbbrTbl">Station Abbr Table from parser object.</param>
        /// <returns>Returns string value of voltage on success, empty string otherwise.</returns>
        public static string ExtractVoltage(string rawName, GenericTable stationAbbrTbl)
        {
            // Special case for this station.
            bool isVeg = false;
            if (rawName.Contains("Vegui") || rawName.Contains("Veg."))
            {
                //SA:20111124
                // rawName = GetName(rawName, stationAbbrTbl);
                isVeg = true;
            }
            string[] nameArr = rawName.Split(' ');
            if (nameArr.Length > 1)
            {
                for (int i = 1; i < nameArr.Length; i++)
                {
                    string tempName = nameArr[0] + "_" + nameArr[i];
                    //SA:20211208
                    //if (i == 1 && (stationAbbrTbl.TryGetRow(new[] { tempName }, out DataRow _) || tempName.Equals("Soto_Rib.")))  // Special Case to look for
                    //{
                    //    continue;
                    //}
                    // skip empty characters
                    if (!string.IsNullOrEmpty(nameArr[i]))
                    {
                        if (nameArr[i].TryParseDouble(out double name))
                        {
                            return name.ToString();
                        }
                        else if (nameArr[0].TryParseDouble(out name) && isVeg)  // special case for specific station from above
                        {
                            return name.ToString();
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                return string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// Helper function to get the next available address for RtuDefn.
        /// </summary>
        /// <param name="pRtu">Current pRtu value.</param>
        /// <param name="pointType">Current FEP point type of current object.</param>
        /// <returns></returns>
        public static int GetNextRtuDefnAddress(int pRtu, I104PointTypes pointType)
        {
            if (rtuDefnAddresses.TryGetValue((pRtu, pointType), out int addr))
            {
                rtuDefnAddresses[(pRtu, pointType)]++;
                return (addr+1);
            }
            else
            {
                rtuDefnAddresses.Add((pRtu, pointType), 1);
                return 1;
            }
        }

        /// <summary>
        /// Helper function to find pRTU.
        /// </summary>
        /// <param name="dataRow">Current DataRow Object.</param>
        /// <param name="rtuDataDict">Current rtuData Dictionary object.</param>
        public static int FindpRtu(DataRow dataRow, Dictionary<string, int> rtuDataDict)
        {
            string rtuIRN = dataRow["RTU_IRN"].ToString();
            string protocol = dataRow["RTU_TYPE_ASC"].ToString();
            if ((dataRow["IDENTIFICATION_TEXT"].ToString().ToUpper().StartsWith("AAREY220  33KV") && dataRow["RTU_IRN"].ToString() == "138096201")) 
                rtuIRN = "400001";
            if ((dataRow["IDENTIFICATION_TEXT"].ToString().ToUpper().StartsWith("VERSO220  33KV") && dataRow["RTU_IRN"].ToString() == "138097201")) 
                rtuIRN = "400002";
            if (rtuDataDict.TryGetValue(rtuIRN, out int pRtu))// && protocol.Equals("I104"))
            {
                return pRtu;
            }
            else
            {
                Logger.Log("MISSING pRTU", LoggerLevel.INFO, $"pRTU not found for scada object. RTU IRN:{rtuIRN}");
                return -1;
            }
        }

        /// <summary>
        /// Helper function to find which points to skip based on points in display and specific stations.
        /// </summary>
        /// <param name="stationsToSkip">Stations to skip tbl.</param>
        /// <param name="pointsFromDisplay">Points from the displays table.</param>
        /// <param name="stationIrn">Current SCADA object's station IRN.</param>
        /// <param name="externalID">Current EXTERNAL_IDENTITY from input.</param>
        /// <returns>True if the point should be skipped. False if not.</returns>
        public static bool ToSkip(GenericTable stationsToSkip, GenericTable pointsFromDisplay, string stationIrn, string externalID)
        {
            // Check if station irn is in the stations table and if the point is in any of the displays. If both are true, the point gets included.
            if (stationsToSkip.TryGetRow(new[] { stationIrn }, out DataRow _) &&
                    pointsFromDisplay.TryGetRow(new[] { externalID }, out DataRow _))
            {
                return false;
            }
            // If the station irn isn't in the table, don't skip.
            else if (!stationsToSkip.TryGetRow(new[] { stationIrn }, out DataRow _))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Helper function to generate keys for EDPESP SCADA objects.
        /// This method is based around preventing duplicates of existing locked keys.
        /// </summary>
        /// <param name="scadaDb">Current SCADA object.</param>
        /// <param name="prefix">Prefix for the SCADA Key based on type.</param>
        /// <param name="stationKey">Key of pStation for current SCADA object.</param>
        /// <param name="externalID">ExternalID of current SCADA object.</param>
        /// <param name="scadaKeyTbl">XREF for locked keys.</param>
        /// <param name="lockedKeys">HashSet of locked keys.</param>
        /// <returns></returns>
        
        public static string GenerateEdpespKey(SCADA scadaDb, int prefix, string stationKey, string externalID,
                GenericTable scadaKeyTbl, HashSet<string> lockedKeys)
        {
            // check for locked key first
            //if (scadaKeyTbl.TryGetRow(new[] { externalID }, out DataRow keyRow))
            //{
            //    string lockedKey = keyRow["SCADA Key"].ToString();
            //    if (!string.IsNullOrEmpty(lockedKey))
            //    {
            //        return lockedKey;
            //    }
            //}

            // loop through HashSet until generated key is unique

            //if (stationKey == "37") { 
            //    stationKey = "999";
            //}
            
            //stationKey = stationKey.PadLeft(3, '0');
            string key = scadaDb.GetNextKey(prefix, stationKey);
            
            //key = key2base36(key);  //temp comment 
            if(key == "00000000")
            {
                int tt = 0;
            }
            if (string.IsNullOrEmpty(key))
            {
                nullcount++;
                string x = stationKey;
                //if (stationKey.Length == 2) { stationKey = stationKey.PadLeft(3, '0'); }
                //if (stationKey.Length == 3) { stationKey = stationKey.PadLeft(4, '0'); }
                key = keyexpansion(scadaDb, prefix, stationKey);
                if (key == "01011123")
                {
                    int tt = 0;
                }
                if (string.IsNullOrEmpty(key)) { key = "00000000"; }  //SA:20211213 Magesh,vinagayam requested this for temporary addin for some reference, this is done because pstation value is 0.

            }
         
            //while (lockedKeys.Contains(key))
            //{
            //    key = scadaDb.GetNextKey(prefix, stationKey);
            //}
            return key;
        }

        /// <summary>
        /// Helper function to skip points with certain RTUs
        /// </summary>
        /// <param name="rtuTbl">Table of RTU data.</param>
        /// <param name="rtu_irn">Current point's RTU_IRN</param>
        /// <returns></returns>

        public static string keyexpansion(SCADA scadaDb,int prefix, string num) //SA:20211213 it adds additional points for the keys.
        {
            //string num36 = scadaDb.DecToBase36(Intnum);
            string key = scadaDb.GetNextKey(prefix,num);
            if (key == "01011123")
            {
                int tt = 0;
            }
            if (!string.IsNullOrEmpty(key))
            {
                var keys = key.Select(x => x.ToString()).ToArray();
                string output = null;
                switch (keys[6])
                {
                    case "0":
                        output = keys[0] + keys[1] + keys[2] + keys[3] + keys[4] + keys[6] + keys[7] + "A";
                        break;
                    case "1":
                        output = keys[0] + keys[1] + keys[2] + keys[3] + keys[4] + keys[6] + keys[7] + "B";
                        break;
                    case "2":
                        output = keys[0] + keys[1] + keys[2] + keys[3] + keys[4] + keys[6] + keys[7] + "C";
                        break;
                    case "3":
                        output = keys[0] + keys[1] + keys[2] + keys[3] + keys[4] + keys[6] + keys[7] + "D";
                        break;
                    case "4":
                        output = keys[0] + keys[1] + keys[2] + keys[3] + keys[4] + keys[6] + keys[7] + "E";
                        break;
                    case "5":
                        output = keys[0] + keys[1] + keys[2] + keys[3] + keys[4] + keys[6] + keys[7] + "F";
                        break;
                    case "6":
                        output = keys[0] + keys[1] + keys[2] + keys[3] + keys[4] + keys[6] + keys[7] + "G";
                        break;
                    case "7":
                        output = keys[0] + keys[1] + keys[2] + keys[3] + keys[4] + keys[6] + keys[7] + "H";
                        break;
                    case "8":
                        output = keys[0] + keys[1] + keys[2] + keys[3] + keys[4] + keys[6] + keys[7] + "I";
                        break;
                    case "9":
                        output = keys[0] + keys[1] + keys[2] + keys[3] + keys[4] + keys[6] + keys[7] + "J";
                        break;
                    case "A":
                        output = keys[0] + keys[1] + keys[2] + keys[3] + keys[4] + keys[6] + keys[7] + "K";
                        break;
                    case "B":
                        output = keys[0] + keys[1] + keys[2] + keys[3] + keys[4] + keys[6] + keys[7] + "L";
                        break;
                    case "C":
                        output = keys[0] + keys[1] + keys[2] + keys[3] + keys[4] + keys[6] + keys[7] + "M";
                        break;
                    case "D":
                        output = keys[0] + keys[1] + keys[2] + keys[3] + keys[4] + keys[6] + keys[7] + "N";
                        break;
                    case "E":
                        output = keys[0] + keys[1] + keys[2] + keys[3] + keys[4] + keys[6] + keys[7] + "O";
                        break;
                    case "F":
                        output = keys[0] + keys[1] + keys[2] + keys[3] + keys[4] + keys[6] + keys[7] + "P";
                        break;
                    case "G":
                        output = keys[0] + keys[1] + keys[2] + keys[3] + keys[4] + keys[6] + keys[7] + "Q";
                        break;
                    case "H":
                        output = keys[0] + keys[1] + keys[2] + keys[3] + keys[4] + keys[6] + keys[7] + "R";
                        break;
                    case "I":
                        output = keys[0] + keys[1] + keys[2] + keys[3] + keys[4] + keys[6] + keys[7] + "S";
                        break;
                    case "J":
                        output = keys[0] + keys[1] + keys[2] + keys[3] + keys[4] + keys[6] + keys[7] + "T";
                        break;
                    case "K":
                        output = keys[0] + keys[1] + keys[2] + keys[3] + keys[4] + keys[6] + keys[7] + "U";
                        break;
                    case "L":
                        output = keys[0] + keys[1] + keys[2] + keys[3] + keys[4] + keys[6] + keys[7] + "V";
                        break;
                    case "M":
                        output = keys[0] + keys[1] + keys[2] + keys[3] + keys[4] + keys[6] + keys[7] + "W";
                        break;
                    case "N":
                        output = keys[0] + keys[1] + keys[2] + keys[3] + keys[4] + keys[6] + keys[7] + "X";
                        break;
                    case "O":
                        output = keys[0] + keys[1] + keys[2] + keys[3] + keys[4] + keys[6] + keys[7] + "Y";
                        break;
                    case "P":
                        output = keys[0] + keys[1] + keys[2] + keys[3] + keys[4] + keys[6] + keys[7] + "Z";
                        break;

                }
                //key = key2base36(output);// commenting for now 
                //return output;//BD: Added as duplicates are created //BD: Commenting now
                //key = key2base36(key); //BD: added now
                return key;
            }
            else
                return null;
        }
        public static string key2base36(string key) //SA:20211210 it converts key to base36
        {
            if (!string.IsNullOrEmpty(key))
            {
                var keys = key.Select(x => x.ToString()).ToArray();
                var keys2 = keys[2] + keys[3] + keys[4];
                string keys3 = Decode(long.Parse(keys2));
                keys3 = keys3.PadLeft(3, '0');
                string final = keys[0] + keys[1] + keys3 + keys[5] + keys[6] + keys[7];
                key = final;
                return key;
            }
            else
            {
                return key;
            }
        }
        public static bool SkipRtuPoint(GenericTable rtuTbl, string rtu_irn)
        {
            // if present in table, do not skip.
            if (rtuTbl.TryGetRow(new[] { rtu_irn }, out DataRow _))
            {
                return false;
            }
            return true;
        }
        public static DataRow MeasurandRtuPointCheck(GenericTable measrndTbl, string ExId)
        {
            // if present in table, do not skip.
            if (measrndTbl.TryGetRow(new[] { ExId }, out DataRow analog2))
            {
                return analog2;
            }
            return null;
        }
        public static DataRow IndIoaPointCheck(GenericTable IndTbl, string ExId)
        {
            // if present in table, do not skip.
            if (IndTbl.TryGetRow(new[] { ExId }, out DataRow analog2))
            {
                return analog2;
            }
            return null;
        }

        /// <summary>
        /// Helper function to remove specific substations from a points name.
        /// </summary>
        /// <param name="name">Current name of point.</param>
        /// <returns>Name with substring removed if present.</returns>
        public static string RemoveSubstation(string name)
        {
            if (name.StartsWith("Vegui "))
            {
                name = name.Replace("Vegui ", "");
            }
            if (name.StartsWith("Veg."))
            {
                name = name.Replace("Veg.", "");
            }
            return name;
        }

        /// <summary>
        /// Helper function to set the archive group of a SCADA point.
        /// </summary>
        /// <param name="obj">Current SCADA point object.</param>
        /// <param name="type">SCADA object type.</param>
        /// <param name="hisTypeIRN">HIS_TYPE_IRN from the input file.</param>
        public static void SetArchiveGroup(DbObject obj, ScadaType type, string hisTypeIRN)
        {
            ArchiveGroup archiveFlagGroup = ArchiveGroup.None;
            switch (type)
            {
                case ScadaType.ANALOG:
                    if (hisTypeIRN.Equals("87849201"))
                    {
                        archiveFlagGroup = ArchiveGroup.Index7;
                    }
                    else if (hisTypeIRN.Equals("87847201"))
                    {
                        archiveFlagGroup = ArchiveGroup.Index6;
                    }
                    else if (hisTypeIRN.Equals("114812201"))
                    {
                        archiveFlagGroup = ArchiveGroup.Index5;
                    }
                    break;
                case ScadaType.STATUS:
                    if (hisTypeIRN.Equals("87853201"))
                    {
                        archiveFlagGroup = ArchiveGroup.Index7;
                    }
                    break;
                case ScadaType.ACCUMULATOR:
                    if (hisTypeIRN.Equals("87851201"))
                    {
                        archiveFlagGroup = ArchiveGroup.Index7;
                    }
                    break;
                default:
                    Logger.Log("UNHANDLED SCADA TYPE", LoggerLevel.INFO, $"Unhandled type for archive flags: {type}");
                    return;
            }

            int archiveFlag = (int)archiveFlagGroup;
            obj.SetValue("Archive_group", archiveFlag);
        }
        public static void SetArchiveGroup2(DbObject obj, ScadaType type, string hisTypeIRN)
        {
            ArchiveGroup archiveFlagGroup = ArchiveGroup.None;
            
            if (!string.IsNullOrEmpty(hisTypeIRN))
            {
                archiveFlagGroup = ArchiveGroup.Index7;
            }
            int archiveFlag = (int)archiveFlagGroup;
            obj.SetValue("Archive_group", archiveFlag);
        }

        /// <summary>
        /// Helper function to set pAlarm_Group for status and analog points.
        /// </summary>
        /// <param name="obj">Current SCADA point object.</param>
        /// <param name="alarmIRN">Alarm IRN from the input.</param>
        /// <param name="alarmsDict">Alarm Dictionary object.</param>
        public static void SetpAlarmGroup(DbObject obj, string alarmIRN, Dictionary<string, int> alarmsDict, string objct, string RowIRN, string pointname)
        {
            if (alarmsDict.TryGetValue(alarmIRN, out int pAlarmGroup))
            {
                obj.SetValue("pALARM_GROUP", pAlarmGroup);
            }
            else
            {
                obj.SetValue("pALARM_GROUP", 1);
                Logger.Log("UNMATCHED ALARM IRN", LoggerLevel.INFO, $"Alarm Group not found: {RowIRN} in Object: {objct} and name: {pointname} \t Setting to pALARM_GROUP 1");
            }
        }

        /// <summary>
        /// This function checks if a scada point needs to be duplicated.
        /// </summary>
        /// <param name="externalID">EXTERNAL_IDENTITY from the input file.</param>
        /// <param name="iddvalTbl">ICCP table iddval.</param>
        /// <returns></returns>
        public static bool CheckForDuplicate(string externalID, GenericTable iddvalTbl)
        {
            DataRow[] allRows = iddvalTbl.GetMultipleRowInfo(new[] { externalID });
            int extraRows = 0;
            foreach (DataRow currentRow in allRows)
            {
                // Both rows must have SRCF equal to 1 for it to be a duplicate. This indicates import point in ICCP.
                if (allRows.Length > 2 && currentRow["SRCF"].ToString().Equals("1"))
                {
                    ++extraRows;  // Add this for later
                }
                else if (allRows.Length > 2 && !currentRow["SRCF"].ToString().Equals("1"))
                {
                    continue;
                }
                else if (!currentRow["SRCF"].ToString().Equals("1"))
                {
                    return false;
                }
            }

            // If allRows has more than 2 rows and only 1 them has SRCF equal to 1, return false.
            if (allRows.Length > 2 && extraRows < 2)
            {
                return false;
            }

            return allRows.Length > 1;
        }

        private static string Decode(long value)
        {
            const string base36 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var sb = new StringBuilder(13);
            do
            {
                sb.Insert(0, base36[(byte)(value % 36)]);
                value /= 36;
            } while (value != 0);
            return sb.ToString();
        }
    }
}
