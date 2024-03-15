using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using OSII.ConversionToolkit;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;

namespace edpesp_db.Data.EdpespDatabases.EdpespStates
{

    class EdpespState
    {
        private readonly EdpespParser _parser;  // Local reference of the parser.
        private readonly STATES _statesDb;  // Local reference of the state db.
        private readonly Dictionary<(string, string), int> _oldStateDict;  // Local reference of the old states dictionary.
        private readonly Dictionary<string, string> _bayDict;  // Local reference of the Bay Dict. 
        private readonly Dictionary<(string, string), int> _newStateDict;  // Local reference of the new states dictionary.
        private readonly List<(string, string, string, string)> customStatesList;  // List of custom states
        public Dictionary<int, string> statedict;
        /// <summary>
        /// Default constructor. 
        /// Sets local references of important values. 
        /// </summary>
        /// <param name="par">Current Parser object.</param>
        /// <param name="statesDb">Current STATES db.</param>
        /// <param name="oldStateDict">State Dictionary for old states.</param>
        /// <param name="newStateDict">State dictionary for new states.</param>
        /// /// <param name="statedict">State dictionary for new states.</param>
        public EdpespState(EdpespParser par, STATES statesDb, Dictionary<(string, string), int> oldStateDict, Dictionary<(string, string), int> newStateDict, Dictionary<int, string> statedict, Dictionary<string, string> BayDict)
        {
            this._parser = par;
            this._statesDb = statesDb;
            this._newStateDict = newStateDict;
            this._oldStateDict = oldStateDict;
            this.statedict = statedict;
            this._bayDict = BayDict;
            customStatesList = new List<(string, string, string, string)>();
        }


        /// <summary>
        /// Function to convert all STATE objects.
        /// </summary>
        public void ConvertState()
        {
            Logger.OpenXMLLog();
            List<string> elem = new List<string>();

            DbObject stateObj = this._statesDb.GetDbObject("STATE");
            int stateRec = 200;
            int custQUAD = 0;
            string description;
            //foreach (DataRow stateRow in this._parser.IndicationTbl.Rows)
            //{
            //    // check indication type

            //    string indication = stateRow["INDICATION_TYPE_CODE"].ToString();
            //    //string description;

            //    // Find both states and combine them
            //    string state1 = stateRow["STATUS_TEXT_01"].ToString();
            //    string state2 = stateRow["STATUS_TEXT_10"].ToString();
            //    if (state1 == "" || state2 == "")
            //    {
            //        continue;
            //    }
            //    string stateName = state1 + "/" + state2;
            //    //elem.Add(stateName.Replace(":STATUS_",""));
            //    elem.Add(stateName);
            //    //string normalStatus = stateRow["NORMAL_STATUS"].ToString();
            //}
            //List<string> st = elem.Select(x => x.Replace(":STATUS_", "")).Distinct().ToList();

            //foreach (var state in st)
            //{
            //    stateObj.CurrentRecordNo = ++stateRec;
            //    description = $"Cust QUAD {++custQUAD}";

            //    var states = state.Split('/');
            //    stateObj.SetValue("names_0", "INVALID");//SA:20211214//error00
            //    stateObj.SetValue("names_1", states[0]);
            //    stateObj.SetValue("names_2", states[1]);
            //    stateObj.SetValue("names_3", "INBETWEEN");//SA:20211214//error11

            //    stateObj.SetValue("attrib_0", 00);
            //    stateObj.SetValue("attrib_1", 01);
            //    stateObj.SetValue("attrib_2", 10);
            //    stateObj.SetValue("attrib_3", 11);


            //    string combine = states[0] + '/' + states[1];
            //    statedict.Add(stateRec, combine);

            //    if (states[0] == "OPEN" && states[1] == "CLOSE") // have to think of a clear logic
            //    {
            //        for (int i = 0; i < 2; i++)
            //        {
            //            stateObj.CurrentRecordNo = ++stateRec;
            //            description = $"Cust QUAD {++custQUAD}";

            //            var statess = state.Split('/');
            //            stateObj.SetValue("names_0", "INVALID");//SA:20211214//error00
            //            stateObj.SetValue("names_1", statess[0]);
            //            stateObj.SetValue("names_2", statess[1]);
            //            stateObj.SetValue("names_3", "INBETWEEN");//SA:20211214//error11

            //            stateObj.SetValue("attrib_0", 00);
            //            stateObj.SetValue("attrib_1", 01);
            //            stateObj.SetValue("attrib_2", 10);
            //            stateObj.SetValue("attrib_3", 11);

            //            combine = statess[0] + '/' + statess[1];
            //            statedict.Add(stateRec, combine);
            //        }
            //    }
            //    if (states[0] == "CLOSE" && states[1] == "OPEN") // have to think of a clear logic
            //    {
            //        for (int i = 0; i < 2; i++)
            //        {
            //            stateObj.CurrentRecordNo = ++stateRec;
            //            description = $"Cust QUAD {++custQUAD}";

            //            var statess = state.Split('/');
            //            stateObj.SetValue("names_0", "INVALID");//SA:20211214//error00
            //            stateObj.SetValue("names_1", statess[0]);
            //            stateObj.SetValue("names_2", statess[1]);
            //            stateObj.SetValue("names_3", "INBETWEEN");//SA:20211214//error11

            //            stateObj.SetValue("attrib_0", 00);
            //            stateObj.SetValue("attrib_1", 01);
            //            stateObj.SetValue("attrib_2", 10);
            //            stateObj.SetValue("attrib_3", 11);

            //            combine = statess[0] + '/' + statess[1];
            //            statedict.Add(stateRec, combine);
            //        }
            //    }
            //    // customStatesList.Add(("Invaild", states[0], states[1], "Inbetween"));

            //    // this._oldStateDict.Add((state[0] + state[1], "NULL"), stateRec);
            //    //        customStatesList.Add(("Invaild", state1, state2, "Inbetween"));//SA:20211214//error11//error00
            //    //    }
            //}

            foreach (DataRow stateRow in this._parser.IndicationTbl.Rows)
            {
                if (!this._bayDict.ContainsKey(stateRow["BAY_IRN"].ToString()))
                {
                    this._bayDict[stateRow["BAY_IRN"].ToString()] = stateRow["RTU_IRN"].ToString();
                }
                // check indication type
                string indication = stateRow["INDICATION_TYPE_CODE"].ToString();
                //string description;

                // Find both states and combine them
                string state1 = stateRow["STATUS_TEXT_01"].ToString().Replace(":STATUS_", "");
                string state2 = stateRow["STATUS_TEXT_10"].ToString().Replace(":STATUS_", "");
                

                // Find both states and combine them
                //    string state1 = stateRow["STATUS_TEXT_01"].ToString();
                //    string state2 = stateRow["STATUS_TEXT_10"].ToString();
                if (state1 == "" || state2 == "")
                {
                    continue;
                }
                string stateName = state1 + "/" + state2;
                //    string stateName = state1 + "/" + state2;
                string normalStatus = stateRow["NORMAL_STATUS"].ToString();
                if(stateName == ":10_STATUS/:01_STATUS")
                {
                    int ttt = 0;
                }
                if (indication.Equals("D") && !this._oldStateDict.ContainsKey((state1 + state2, "NULL")) && !customStatesList.Contains(("Invaild", state1, state2, "Inbetween")))
                {
                    stateObj.CurrentRecordNo = ++stateRec;
                    description = "Quad_"+ stateName;// $"Cust QUAD {++custQUAD}";

                    stateObj.SetValue("description", description);
                    stateObj.SetValue("names_0", "Invaild");//SA:20211214//error00
                    stateObj.SetValue("names_1", state1);
                    stateObj.SetValue("names_2", state2);
                    stateObj.SetValue("names_3", "Inbetween");//SA:20211214//error11

                    stateObj.SetValue("attrib_0", 0); //0:00
                    stateObj.SetValue("attrib_1", 14); //COLOR11:01
                    stateObj.SetValue("attrib_2", 15); //COLOR13:10
                    stateObj.SetValue("attrib_3", 11);//0:11

                    //stateObj.SetValue("description", description);
                    //stateObj.SetValue("names_0", state2);
                    //stateObj.SetValue("names_1", state1);

                    //stateObj.SetValue("attrib_0", COLOR13);
                    //stateObj.SetValue("attrib_1", COLOR11);
                    statedict.Add(stateRec, description);

                    // Add to Dictionary and List
                    this._oldStateDict.Add((state1 + state2, "NULL"), stateRec);
                    customStatesList.Add(("Invaild", state1, state2, "Inbetween"));//SA:20211214//error11//error00
                }
                else if (!this._oldStateDict.ContainsKey((state1, state2)))
                {
                    stateObj.CurrentRecordNo = ++stateRec;
                    description = stateName;

                    stateObj.SetValue("description", description);
                    stateObj.SetValue("names_0", state1);
                    stateObj.SetValue("names_1", state2);

                    stateObj.SetValue("attrib_0", 0); //COLOR13:00
                    stateObj.SetValue("attrib_1", 14);//COLOR11:01
                    statedict.Add(stateRec, description);

                    // Add to Dictionary
                    this._oldStateDict.Add((state1, state2), stateRec);
                }
            }
            // These states are new states added in from an input file:BD
           
            foreach (DataRow addState in this._parser.OnetStatesTbl.Rows)
            {
                int staterec = Int16.Parse(addState["record"].ToString());
                stateObj.CurrentRecordNo = staterec ;
                stateObj.SetValue("description", addState["description"].ToString());
                stateObj.SetValue("names_0", addState["names_0"]);
                stateObj.SetValue("names_1", addState["names_1"]);
                stateObj.SetValue("names_2", addState["names_2"]);
                stateObj.SetValue("names_3", addState["names_3"]);
                stateObj.SetValue("names_4", addState["names_4"]);
                stateObj.SetValue("attrib_0", addState["attrib_0"]);
                stateObj.SetValue("attrib_1", addState["attrib_1"]);
                stateObj.SetValue("attrib_2", addState["attrib_2"]);
                stateObj.SetValue("attrib_3", addState["attrib_3"]);
                stateObj.SetValue("attrib_4", addState["attrib_4"]);
                statedict.Add(staterec, addState["description"].ToString());
                //this._newStateDict.Add((addState["state1"].ToString(), addState["state2"].ToString()), stateRec);
            }
            Logger.CloseXMLLog();
        }
    }
}
