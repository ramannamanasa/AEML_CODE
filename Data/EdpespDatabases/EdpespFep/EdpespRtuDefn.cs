using OSII.ConversionToolkit;
using OSII.ConversionToolkit.Extensions;
using OSII.ConversionToolkit.Generic;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace edpesp_db.Data.EdpespDatabases.EdpespFep
{
    class EdpespRtuDefn
    {
        private readonly FEP _fepDb;  // Local reference to the FEP database object.
        private readonly GenericTable _scadaToFepXref;  // Local reference to the Scada to Fep Xref
        /// <summary>
        /// Default constructor.
        /// Sets important local references.
        /// </summary>
        /// <param name="fepDb">Current FEP database object.</param>
        public EdpespRtuDefn(FEP fepDb, GenericTable scadaToFepXref)
        {
            this._fepDb = fepDb;
            this._scadaToFepXref = scadaToFepXref;
        }



        /// <summary>
        /// Function to convert all RTU_DEFN objects.
        /// </summary>
        /// 


        public void ConvertRtuDefn()
        {
            Logger.OpenXMLLog();

            DbObject rtuDefnObj = this._fepDb.GetDbObject("RTU_DEFN");
            DbObject rtuDataObj = this._fepDb.GetDbObject("RTU_DATA");
            int dataRec = rtuDataObj.RecordCount;
            int rtuDefnRec = 0;

            this._scadaToFepXref.ChangeColumnType("IOA", typeof(int));
            //this._scadaToFepXref.Sort("pRtu, FEP Type, IOA");
            //this._scadaToFepXref.Sort("pRtu");// FEPCHECK : BD
            //System.IO.File.WriteAllLines(@"D:\text.txt", this._scadaToFepXref(x => string.Join(",", x)));
            // Constant values from mapping.
            const int SINGLE_PT = 1;
            const int DOUBLE_PT = 2;
            const int MEAS_VALUE = 3;
            const int INT_TOTAL = 5;
            List<DataRow> check = new List<DataRow>();
            List<int> num = new List<int>();
            // dataRec = 2; // just for testing after testing remove it 
            //dataRec = 1;
            for (int i = 0; i < dataRec; i++)
            {
                // Set Record
                rtuDefnObj.CurrentRecordNo = ++rtuDefnRec;
                //if (rtuDefnObj.CurrentRecordNo > 10) return; //BD: FEPCHECK
                //Dictionary<string, List<DataRow>> fepPointGroups = new Dictionary<string, List<DataRow>>();//SA:20211130 making it concurrent dict
                ConcurrentDictionary<string, List<DataRow>> fepPointGroups = new ConcurrentDictionary<string, List<DataRow>>();
                Dictionary<string, int> feptypecount = new Dictionary<string, int>();
                //CountPointTypeGroups2(rtuDefnRec, ref fepPointGroups, ref feptypecount);
                CountPointTypeGroups(rtuDefnRec, ref fepPointGroups, ref feptypecount);
                int index = 0;
                int prevCount = 0;
                int nextAddr = 0;
                int checkcount = 0;
                int singleaddress = 0;
                int doubleaddress = 0;
                int measaddress = 0;
                if (fepPointGroups.Count != 0)
                {

                    //var Single = fepPointGroups.Where(p => p.Key.Contains("SINGLES")).Select(x => x.Value);//.Count();//.Aggregate("",(x, y) => x + y);
                    //foreach (var s in Single)
                    //{
                    //    foreach (var p in s)
                    //    {
                    //        num.Add((int)p.ItemArray[5]);
                    //        //check.Add(p);
                    //    }
                    //}
                    //num.Sort();ALARM_GROUP.csv
                    //var result = num.GroupWhile((x, y) => y - x == 1).Select(x => new { i = x.First(), len = x.Count() }).ToList();
                    //var result = num.Zip(num.Skip(1).Concat(new[] { int.MaxValue }), (i0, i1) => new { i = i0, diff = i1 - i0 }).Reverse().Scans((state, z) => state == null ? Tuple.Create(z.i, z.diff, 0) : Tuple.Create(z.i, z.diff, z.diff > 1 ? state.Item3 + 1 : state.Item3),(Tuple<int, int, int>)null) .Skip(1).Reverse().GroupBy(t => t.Item3, (i, l) => new { l.First().Item1, l = l.Count() });
                    //var consecutiveGroups = num.FindConsecutiveGroups((x) => x > 10, 3);
                    //check.OrderBy(row => row.Field<int>("IOA"));
                    //var single2 = Single.ToList();
                    //Commenting now for testing  FEPCHECK
                    //var aa = fepPointGroups.Keys.OrderBy(x => x).ToList();
                    var singlelist = fepPointGroups.Keys.Where(x => x.Contains("SINGLE")).ToList();
                    var doublelist = fepPointGroups.Keys.Where(x => x.Contains("DOUBLE")).ToList();
                    var measlist = fepPointGroups.Keys.Select(x => x.Contains("MEAS")).ToList();
                    //foreach (var bb in aa)//Uncomment for right output//BD
                    //{
                    //    var each = fepPointGroups[bb];
                    //    var pointtype = (bb.Contains("SINGLE")) ? SINGLE_PT : (bb.Contains("DOUBLE")) ? DOUBLE_PT : MEAS_VALUE;
                    //    rtuDefnObj.SetValue("PointType", index, pointtype);
                    //    rtuDefnObj.SetValue("Start", index, each[0]["IOA"]);
                    //    //feptypecount.TryGetValue("SINGLES", out int singlevalue);
                    //    var count = (pointtype == 2) ? each.Count() * 2 : each.Count();
                    //    rtuDefnObj.SetValue("Count", index, count);
                    //    rtuDefnObj.SetValue("Parm1", index, each[0]["IOA"]);
                    //    prevCount += each.Count;
                    //    index++;
                    //}

                    //Commenting now for testing  FEPCHECK
                    rtuDefnObj.SetValue("PointType", index, SINGLE_PT);
                    rtuDefnObj.SetValue("Start", index, 1);
                    feptypecount.TryGetValue("SINGLES", out int singlevalue);
                    rtuDefnObj.SetValue("Count", index, singlevalue);
                    rtuDefnObj.SetValue("Parm1", index, 0);
                    ++index;
                    // var doubles = fepPointGroups.Where(p => p.Key.Contains("")).Select(x => x.Value.Count).Count();//.Aggregate("",(x, y) => x + y);
                    rtuDefnObj.SetValue("PointType", index, DOUBLE_PT);
                    rtuDefnObj.SetValue("Start", index, 1);
                    feptypecount.TryGetValue("DOUBLES", out int doublevalue);
                    rtuDefnObj.SetValue("Count", index, doublevalue);
                    rtuDefnObj.SetValue("Parm1", index, 0);
                    ++index;
                    // var meas = fepPointGroups.Where(p => p.Key.Contains("MEAS")).Select(x => x.Value.Count).Count();//.Aggregate("",(x, y) => x + y);
                    rtuDefnObj.SetValue("PointType", index, MEAS_VALUE);
                    rtuDefnObj.SetValue("Start", index, 1);
                    feptypecount.TryGetValue("MEAS", out int measvalue);
                    rtuDefnObj.SetValue("Count", index, measvalue);
                    rtuDefnObj.SetValue("Parm1", index, 0);
                    //foreach (KeyValuePair<string, List<DataRow>> pair in fepPointGroups) //SA:20211130 trying to make the foreach loop run faster
                    ////                                                                     //Parallel.ForEach(fepPointGroups, (KeyValuePair<string, List<DataRow>> pair) =>
                    //{
                    //    string key = pair.Key;
                    //    DataRow data = pair.Value.First();
                    //    //   if (pair.Key.Contains("SINGLES"))
                    //    //    {
                    //    //        rtuDefnObj.SetValue("PointType", index, SINGLE_PT); //100
                    //    //        checkcount++;
                    //    //    }
                    //    //    else if (pair.Key.Contains("DOUBLES"))
                    //    //    {
                    //    //        rtuDefnObj.SetValue("PointType", index, DOUBLE_PT);//500
                    //    //    }
                    //    //    else if (pair.Key.Contains("MEAS"))
                    //    //    {
                    //    //        rtuDefnObj.SetValue("PointType", index, MEAS_VALUE);//600
                    //    //    }
                    //    //    else if (pair.Key.Contains("INTS"))
                    //    //    {
                    //    //        rtuDefnObj.SetValue("PointType", index, INT_TOTAL);
                    //    //    }

                    //    //    rtuDefnObj.SetValue("Start", index, prevCount + 1);
                    //    //    rtuDefnObj.SetValue("Count", index, pair.Value.Count);
                    //    //    prevCount += pair.Value.Count;
                    //    //    rtuDefnObj.SetValue("Parm1", index, data["IOA"]);
                    //    //    //++index; //SA:20211125 making the index:0.
                    //    //int address = 0;
                    //    //    // Add addresses to the xref.
                    //    foreach (DataRow currentRow in pair.Value)
                    //    {
                    //        int address = 0;
                    //        if (pair.Key.Contains("SINGLES"))
                    //        {
                    //            singleaddress++;
                    //            address = singleaddress;
                    //        }
                    //        else if (pair.Key.Contains("DOUBLES"))
                    //        {
                    //            doubleaddress++;
                    //            address = doubleaddress;
                    //        }
                    //        else if (pair.Key.Contains("MEAS"))
                    //        {
                    //            measaddress++;
                    //            address = measaddress;
                    //        }
                    //       // int address = ++nextAddr;//SA:20211130
                    //        //var currentCount = Interlocked.Increment(ref address);

                    //        //AddToXref(address, currentRow["SCADA Key"].ToString());//FEPCHECK : BD

                    //        //AddToXref(currentCount, currentRow["SCADA Key"].ToString());
                    //        //        //AddToXref(currentCount, currentRow[5].ToString());
                    //    }
                    //}
                    //); 
                    //for(int j =0; j <singlelist.Count; j++)
                    //{
                    //    int address = 0;
                    //    string key = singlelist[j].ToString();
                    //    var aa = fepPointGroups[key];
                    //    foreach (DataRow currentRow in aa)
                    //    {
                    //        singleaddress++;
                    //        address = singleaddress;
                    //        AddToXref(address, currentRow["SCADA Key"].ToString());//FEPCHECK : BD
                    //    }
                        
                    //}
                    //for (int j = 0; j < doublelist.Count; j++)
                    //{
                    //    int address = 0;
                    //    string key = doublelist[j].ToString();
                    //    var aa = fepPointGroups[key];
                    //    foreach (DataRow currentRow in aa)
                    //    {
                    //        doubleaddress++;
                    //        address = doubleaddress;
                    //        AddToXref(address, currentRow["SCADA Key"].ToString());//FEPCHECK : BD
                    //    }

                    //}
                    //for (int j = 0; j < measlist.Count; j++)
                    //{
                    //    int address = 0;
                    //    string key = measlist[j].ToString();
                    //    var aa = fepPointGroups[key];
                    //    foreach (DataRow currentRow in aa)
                    //    {
                    //        doubleaddress++;
                    //        address = doubleaddress;
                    //        AddToXref(address, currentRow["SCADA Key"].ToString());//FEPCHECK : BD
                    //    }

                    //}
                }
            }

            Logger.CloseXMLLog();
        }

        /// <summary>
        /// Helper function to count how many groups there are for a point type.
        /// </summary>
        /// <param name="pRtu">Current pRTU</param>
        /// <param name="fepPointGroups">Dictionary of fepPointGroups.</param>
        // public void CountPointTypeGroups(int pRtu, Dictionary<string, List<DataRow>> fepPointGroups) //SA:20211130 adding concurrent dict
        public void CountPointTypeGroups(int pRtu, ref ConcurrentDictionary<string, List<DataRow>> fepPointGroups, ref Dictionary<string, int> feptypecount)
        {
            Dictionary<int, List<DataRow>> mapRTUDdataRow = new Dictionary<int, List<DataRow>>();
            //this._scadaToFepXref.Sort("IOA");
            DataView dataView = this._scadaToFepXref.DataView;
            for (int i = 0; i < dataView.Count; i++)
            {
                DataRow dataRow = dataView[i].Row;
                int RTURec = dataRow["pRtu"].ToInt();
                if (RTURec.Equals(pRtu))
                {
                    List<DataRow> DataRowlist = new List<DataRow>();
                    if (mapRTUDdataRow.ContainsKey(RTURec))
                        DataRowlist = mapRTUDdataRow[RTURec];

                    //if(termlist == null ) termlist.Add(dictvalue);
                    //if (!DataRowlist.Contains(dictvalue) && !string.IsNullOrEmpty(dictvalue))
                    {
                        DataRowlist.Add(dataRow);
                    }
                    mapRTUDdataRow[RTURec] = DataRowlist;
                }
            }
            //ConcurrentDictionary<string, List<DataRow>> fepPointGroups = fepPointGroups2;// new ConcurrentDictionary<string, List<DataRow>>();
            if (mapRTUDdataRow.ContainsKey(pRtu))
            {
                int prevIOA = 0;
                int singles = 0;
                int doubles = 0;
                int meas = 0;
                int ints = 0;
                int doublecount = 0;
                int singlecount = 0;
                int meascount = 0;

                fepPointGroups = new ConcurrentDictionary<string, List<DataRow>>();
                feptypecount = new Dictionary<string, int>();

                var DataRowlist = mapRTUDdataRow[pRtu];
                //++gap;
                //for (int i = 0; i < dataView.Count; i++)
                for (int i = 0; i < DataRowlist.Count; i++)
                {
                    DataRow dataRow = DataRowlist[i];
                    if (dataRow["pRtu"].ToInt().Equals(pRtu))
                    {
                        switch (dataRow["FEP Type"].ToString())
                        {
                            case "1":
                                AddToDict(fepPointGroups, dataRow, "SINGLES", ref singles, ref prevIOA);
                                singlecount = singlecount + 1;
                                break;
                            case "2":
                                AddToDict(fepPointGroups, dataRow, "DOUBLES", ref doubles, ref prevIOA);
                                doublecount = doublecount + 1; //Quad point DB
                                break;
                            case "3":
                                AddToDict(fepPointGroups, dataRow, "MEAS", ref meas, ref prevIOA);
                                meascount = meascount + 1;
                                break;
                            case "5":
                                AddToDict(fepPointGroups, dataRow, "INTS", ref ints, ref prevIOA);
                                break;
                        }
                    }

                }
                feptypecount.Add("SINGLES", singlecount);
                feptypecount.Add("DOUBLES", doublecount);
                feptypecount.Add("MEAS", meascount);
            }

        }
        public void CountPointTypeGroups2(int pRtu, ref ConcurrentDictionary<string, List<DataRow>> fepPointGroups, ref Dictionary<string, int> feptypecount)
        {
            int gap = 0;
            Dictionary<int, List<DataRow>> mapRTUDdataRow = new Dictionary<int, List<DataRow>>();
            //this._scadaToFepXref.Sort("IOA");
            DataView dataView = this._scadaToFepXref.DataView;
            for (int i = 0; i < dataView.Count; i++)
            {
                DataRow dataRow = dataView[i].Row;
                int RTURec = dataRow["pRtu"].ToInt();
                if (RTURec.Equals(pRtu))
                {
                    List<DataRow> DataRowlist = new List<DataRow>();
                    if (mapRTUDdataRow.ContainsKey(RTURec))
                        DataRowlist = mapRTUDdataRow[RTURec];

                    //if(termlist == null ) termlist.Add(dictvalue);
                    //if (!DataRowlist.Contains(dictvalue) && !string.IsNullOrEmpty(dictvalue))
                    {
                        DataRowlist.Add(dataRow);
                    }
                    mapRTUDdataRow[RTURec] = DataRowlist;
                }
            }
            //ConcurrentDictionary<string, List<DataRow>> fepPointGroups = fepPointGroups2;// new ConcurrentDictionary<string, List<DataRow>>();
            if (mapRTUDdataRow.ContainsKey(pRtu))
            {
                do
                {
                    int prevIOA = 0;
                    int singles = 0;
                    int doubles = 0;
                    int meas = 0;
                    int ints = 0;
                    int doublecount = 0;
                    int singlecount = 0;
                    int meascount = 0;
                    
                    fepPointGroups = new ConcurrentDictionary<string, List<DataRow>>();
                    feptypecount = new Dictionary<string, int>();

                    var DataRowlist = mapRTUDdataRow[pRtu];
                    ++gap;
                    //for (int i = 0; i < dataView.Count; i++)
                    for(int i = 0; i < DataRowlist.Count; i++)
                    {
                        DataRow dataRow = DataRowlist[i];
                        if (dataRow["pRtu"].ToInt().Equals(pRtu))
                        {
                            switch (dataRow["FEP Type"].ToString())
                            {
                                case "1":
                                    AddToDict2(fepPointGroups, dataRow, gap, "SINGLES", ref singles, ref prevIOA);
                                    singlecount = singlecount + 1;
                                    break;
                                case "2":
                                    AddToDict2(fepPointGroups, dataRow, gap, "DOUBLES", ref doubles, ref prevIOA);
                                    doublecount = doublecount + 1;
                                    break;
                                case "3":
                                    AddToDict2(fepPointGroups, dataRow, gap, "MEAS", ref meas, ref prevIOA);
                                    meascount = meascount + 1;
                                    break;
                                case "5":
                                    AddToDict2(fepPointGroups, dataRow, gap, "INTS", ref ints, ref prevIOA);
                                    break;
                            }
                        }

                    }
                    feptypecount.Add("SINGLES", singlecount);
                    feptypecount.Add("DOUBLES", doublecount);
                    feptypecount.Add("MEAS", meascount);
                } while (fepPointGroups?.Count > 32);  //to limit the index to 32 else overflow will occure, if crossing 32 try to adjust gap by increasing 1
            }
            //int gap = 2;
            //do
            //{
            //    ++gap;
            //    var fepPointGroups2 = new List<Dictionary<string, int>>();
            //    foreach (var ptype in fePointGroups.Keys.OrderByDescending(x => x).ToList()) //321
            //    {
            //        fePointGroups[ptype] = fePointGroups[ptype]?.OrderBy(x => x).Distinct().ToList();
            //        if (fePointGroups[ptype].Count > 0)
            //        {
            //            var max = fePointGroups[ptype].Max();
            //            var min = fePointGroups[ptype].Min();
            //            for (var i = fePointGroups[ptype].Count - 1; i >= 1; i--) //highest to lowest
            //            {
            //                var currioa = totdict[ptype][i];
            //                var previoa = totdict[ptype][i - 1];

            //                if ((currioa - previoa) >= gap)
            //                {
            //                    Dictionary<string, int> di = new Dictionary<string, int>();
            //                    di["PointType"] = ptype;
            //                    di["Start"] = currioa;
            //                    di["Count"] = (max - currioa) + 1 + ((ptype == 1 && quadlist.Contains(max)) ? 1 : 0);  //quad point status then add 1 to count
            //                    convertedVal.Add(di);
            //                    max = previoa;
            //                }
            //            }

            //            if (max != min) //for the 1st record mostly 1
            //            {
            //                Dictionary<string, int> di = new Dictionary<string, int>();
            //                di["PointType"] = ptype;
            //                di["Start"] = min;
            //                di["Count"] = (max - min) + 1 + ((ptype == 1 && quadlist.Contains(max)) ? 1 : 0); //quad point status then add 1 to count
            //                convertedVal.Add(di);
            //            }
            //        }
            //        else
            //        {
            //            //Dictionary<string, int> di = new Dictionary<string, int>();
            //            //di["PointType"] = ptype;
            //            //di["Start"] = 0;
            //            //di["Count"] = 0;
            //            //convertedVal.Add(di);
            //        }
            //    }
            //} while (convertedVal?.Count > 32 && !isManualRTU);  //to limit the index to 32 else overflow will occure, if crossing 32 try to adjust gap by increasing 1
            //while (fepPointGroups?.Count > 32);  //to limit the index to 32 else overflow will occure, if crossing 32 try to adjust gap by increasing 1


        }

        /// <summary>
        /// Helper function to properly add to the fepPointGroups dictionary.
        /// </summary>
        /// <param name="fepPointGroups">Current dictionary object.</param>
        /// <param name="dataRow">Current DataRow object.</param>
        /// <param name="mainGroup">Name of the main group this object is apart of.</param>
        /// <param name="subGroup">Number of the current subgroup count.</param>
        /// <param name="prevIOA">Previous objects IOA.</param>
        //public void AddToDict(Dictionary<string, List<DataRow>> fepPointGroups, DataRow dataRow, string mainGroup, ref int subGroup, ref int prevIOA)//SA:20211130
        public void AddToDict(ConcurrentDictionary<string, List<DataRow>> fepPointGroups, DataRow dataRow, string mainGroup, ref int subGroup, ref int prevIOA)
        {
            if (dataRow["IOA"].TryParseInt(out int ioa) && (ioa.Equals(prevIOA + 1) || prevIOA.Equals(0)))
            {
                if (fepPointGroups.ContainsKey($"{mainGroup}{subGroup}"))
                {
                    fepPointGroups[$"{mainGroup}{subGroup}"].Add(dataRow);
                }
                else
                {
                    List<DataRow> temp = new List<DataRow>
                    {
                        dataRow
                    };
                    fepPointGroups.TryAdd($"{mainGroup}{subGroup}", temp);
                    //fepPointGroups.Add($"{mainGroup}{subGroup}", temp); //SA:20211130 trying to add concurrentdict
                }
                prevIOA = ioa;
            }
            else
            {
                ++subGroup;
                List<DataRow> temp = new List<DataRow>
                {
                    dataRow
                };
                //fepPointGroups.Add($"{mainGroup}{subGroup}", temp);//SA:20211130 trying to add concurrentdict
                fepPointGroups.TryAdd($"{mainGroup}{subGroup}", temp);
                prevIOA = ioa;
            }
        }
        public void AddToDict2(ConcurrentDictionary<string, List<DataRow>> fepPointGroups,  DataRow dataRow, int gap, string mainGroup,  ref int subGroup, ref int prevIOA)
        {
            //if (dataRow["IOA"].TryParseInt(out int ioa) && (ioa.Equals(prevIOA + 1) || prevIOA.Equals(0)))
            if (dataRow["IOA"].TryParseInt(out int ioa) && (ioa <= prevIOA + gap || prevIOA.Equals(0)))
            //if (dataRow["IOA"].TryParseInt(out int ioa) && ((ioa - prevIOA) >= gap || prevIOA.Equals(0)))
            {
                if (fepPointGroups.ContainsKey($"{mainGroup}{subGroup}"))
                {
                    fepPointGroups[$"{mainGroup}{subGroup}"].Add(dataRow);
                }
                else
                {
                    List<DataRow> temp = new List<DataRow>
                    {
                        dataRow
                    };
                    fepPointGroups.TryAdd($"{mainGroup}{subGroup}", temp);
                    //fepPointGroups.Add($"{mainGroup}{subGroup}", temp); //SA:20211130 trying to add concurrentdict
                }
            prevIOA = ioa;
            }
            else
            {
                ++subGroup;
                List<DataRow> temp = new List<DataRow>
                {
                    dataRow
                };
                //fepPointGroups.Add($"{mainGroup}{subGroup}", temp);//SA:20211130 trying to add concurrentdict
                fepPointGroups.TryAdd($"{mainGroup}{subGroup}", temp);
                prevIOA = ioa;
            }
        }

        /// <summary>
        /// Helper function to add to the scadaToFepXref.
        /// </summary>
        /// <param name="address">Current address of the object.</param>
        /// <param name="key">Current SCADA key of the object.</param>
        public void AddToXref(int address, string key)
        {
            //this._scadaToFepXref.TryGetRow(new[] { key }, out DataRow row);
            //this._scadaToFepXref.Sort("SCADA Key");
            if (key == "01001001")
            {
                int t = 0;
            }
            foreach (DataRow dataRow in this._scadaToFepXref.Rows) //SA:20211130
            //var yo = this._scadaToFepXref.Rows;
            //List<DataRow> lis = new List<DataRow>();
            //ConcurrentDictionary < string,List < DataRow >> lis = new ConcurrentDictionary<string,List<DataRow>>();

            // Parallel.ForEach(yo., (DataRow dataRow) =>
                
            {
                if (dataRow["SCADA Key"].Equals(key))
                {

                    dataRow["Address"] = address;  // SA:20211130 
                }
            }
            this._scadaToFepXref.ResetTable();
        }

        //public void GetCount(JoinRtuDefnRtusObservers.Payload tables, RtuDefnJoint self)
        //{
        //    dynamic convertedVal;
        //    convertedVal = new List<int>();
        //    Dictionary<int, int> pointTypes = new Dictionary<int, int>();
        //    foreach (var point in tables.RtuDefnC)
        //    {
        //        int typeName = 0;
        //        int count = 0;
        //        if (point.RtuDefnAnalog != null)
        //        {
        //            count = Convert.ToInt32(point.RtuDefnAnalog.InputTables.Scadamom_XDUCER.i_PHYADR) + 1;
        //            typeName = 4;
        //        }
        //        else if (point.RtuDefnStatus != null)
        //        {
        //            count = Convert.ToInt32(point.RtuDefnStatus.InputTables.Scadamom_CONECT.i_PHYADR) + 1;
        //            typeName = 1;
        //        }
        //        else if (point.RtuDefnAccumulator != null)
        //        {
        //            count = Convert.ToInt32(point.RtuDefnAccumulator.InputTables.Scadamom_PULSE.i_PHYADR) + 1;
        //            if (point.RtuDefnAccumulator.InputTables.Scadamom_CTRTYP.i_FREEZABL == "T")
        //            {
        //                typeName = 8;
        //            }
        //            else
        //            {
        //                typeName = 9;
        //            }
        //        }

        //        if (!pointTypes.ContainsKey(typeName)) pointTypes[typeName] = 0;
        //        if (count > pointTypes[typeName])
        //        {
        //            pointTypes[typeName] = count;
        //        }
        //    }

        //    convertedVal = ((List<int>)self.PointType).Select((x => pointTypes[x])).ToList();
        //    return convertedVal;
        //}
    }

    public static class Extensions
    {
        public static IEnumerable<IEnumerable<T>> GroupWhile<T>(this IEnumerable<T> seq, Func<T, T, bool> condition)
        {
            T prev = seq.First();
            List<T> list = new List<T>() { prev };

            foreach (T item in seq.Skip(1))
            {
                if (condition(prev, item) == false)
                {
                    yield return list;
                    list = new List<T>();
                }
                list.Add(item);
                prev = item;
            }

            yield return list;
        }
    }
}
//public static class Extensions
//{
//    public static IEnumerable<IEnumerable<T>> FindConsecutiveGroups<T>(this IEnumerable<T> sequence, Predicate<T> predicate, int count)
//    {
//        IEnumerable<T> current = sequence;

//        while (current.Count() > count)
//        {
//            IEnumerable<T> window = current.Take(count);

//            if (window.Where(x => predicate(x)).Count() >= count)
//                yield return window;

//            current = current.Skip(1);
//        }
//    }
//}

