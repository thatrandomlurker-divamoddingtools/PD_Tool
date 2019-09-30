﻿using System;
using System.Net;
using KKdBaseLib;
using KKdMainLib.IO;

namespace KKdMainLib
{
    public class DataBank
    {
        public DataBank() { Success = false; IO = null; pvList = null; psrDat = null; }

        private Stream IO;
        private int i;

        private  PvList[] pvList;
        private psrData[] psrDat;

        private const string d = ".";
        private const string c = ",";

        public bool Success { get; private set; }

        public void DBReader(string file)
        {
            Success = false;
            if (!File.Exists(file)) return;
            string text = File.ReadAllText(file);
            while (text.Contains("%")) text = WebUtility.UrlDecode(text);
            string[] array = text.Split(',');

            if (file.Contains("psrData") && array.Length % 13 < 2)
            {
                psrDat = new psrData[array.Length / 13];
                for (i = 0; i < psrDat.Length; i++) psrDat[i].SetValue(array, i);
                Success = true;
            }
            else if (file.Contains("psrData")) { psrDat = null; Success = true; }
            else if (file.Contains("PvList") && array.Length % 7 < 2)
            {
                pvList = new PvList[array.Length / 7];
                for (i = 0; i < pvList.Length; i++) pvList[i].SetValue(array, i);
                Success = true;
            }
            else if (file.Contains("PvList")) { pvList = null; Success = true; }
        }

        public void DBWriter(string file, uint num2)
        {
            if (!Success) return;

            IO = File.OpenWriter();
            if (file.Contains("psrData"))
            {
                if (psrDat != null && psrDat.Length > 0)
                    for (i = 0; i < psrDat.Length; i++)
                        IO.Write(psrDat[i].ToString() + c);
            }
            else if (file.Contains("PvList"))
            {
                if (pvList != null && pvList.Length > 0)
                    for (i = 0; i < pvList.Length; i++)
                        IO.Write(UrlEncode(pvList[i].ToString() +
                            (i < pvList.Length ? c : "")));
                else IO.Write("%2A%2A%2A");
            }
             
            byte[] data = IO.ToArray(true);
            ushort num = DCC.CalculateChecksum(data);
            File.WriteAllBytes(file + "_" + num + "_" + num2 + ".dat", data);
        }

        public void MsgPackReader(string file, bool JSON)
        {
            Success = false;
            MsgPack MsgPack = file.ReadMPAllAtOnce(JSON);
            bool compact = MsgPack.ReadBoolean("Compact");

            if (file.Contains("psrData"))
            {
                MsgPack psrData;
                if ((psrData = MsgPack["psrData", true]).NotNull)
                {
                    psrDat = new psrData[psrData.Array.Length];
                    for (i = 0; i < psrDat.Length; i++)
                        psrDat[i].SetValue(psrData[i]);
                }
                else if (MsgPack["psrData"].NotNull) psrDat = null;
                Success = true;
                psrData.Dispose();
            }
            else if (file.Contains("PvList"))
            {
                MsgPack PvList;
                if ((PvList = MsgPack["PvList", true]).NotNull)
                {
                    pvList = new PvList[PvList.Array.Length];
                    for (i = 0; i < pvList.Length; i++)
                        pvList[i].SetValue(PvList[i], compact);
                }
                else if (MsgPack["PvList"].NotNull) pvList = null;
                Success = true;
                PvList.Dispose();
            }
            
            MsgPack.Dispose();
        }

        public void MsgPackWriter(string file, bool JSON, bool Compact = true)
        {
            if (!Success) return;
            MsgPack MsgPack = MsgPack.New;

            if (file.Contains("psrData"))
            {
                if (psrDat != null)
                {
                    MsgPack psrData = new MsgPack(psrDat.Length, "psrData");
                    for (i = 0; i < psrDat.Length; i++) psrData[i] = psrDat[i].WriteMP();
                    MsgPack.Add(psrData);
                }
                else MsgPack.Add(new MsgPack("psrData", null));
            }
            else if (file.Contains("PvList"))
            {
                if (pvList != null)
                {
                    if (Compact) MsgPack.Add("Compact", Compact);

                    MsgPack PvList = new MsgPack(pvList.Length, "PvList");
                    for (i = 0; i < pvList.Length; i++) PvList[i] = pvList[i].WriteMP(Compact);
                    MsgPack.Add(PvList);
                }
                else MsgPack.Add(new MsgPack("PvList", null));
            }
            MsgPack.Write(file, JSON).Dispose();
        }

        public static string UrlEncode(string value) =>
            WebUtility.UrlEncode(value).Replace("+", "%20");

        public struct psrData
        {
            public Player p1;
            public Player p2;
            public Player p3;
            public int PV_ID;

            public void SetValue(string[] data, int i = 0)
            {
                p1.SetValue(data, i, 0);
                p2.SetValue(data, i, 1);
                p3.SetValue(data, i, 2);
                PV_ID = int.Parse(data[i * 13 + 12]);
            }

            public void SetValue(MsgPack msg)
            {
                int? ID = msg.ReadNInt32("PV_ID");
                if (ID != null) PV_ID = (int)ID;
                else { ID = msg.ReadNInt32("ID"); if (ID != null) PV_ID = (int)ID; }
                MsgPack Temp;
                if ((Temp = msg["P1", true]).NotNull) p1.SetValue(Temp);
                if ((Temp = msg["P2", true]).NotNull) p2.SetValue(Temp);
                if ((Temp = msg["P3", true]).NotNull) p3.SetValue(Temp);
                Temp.Dispose();
            }

            public MsgPack WriteMP() =>
                MsgPack.New.Add("PV_ID", PV_ID).Add(p1.WriteMP("P1"))
                     .Add(p2.WriteMP("P2")).Add(p3.WriteMP("P3"));

            public override string ToString() =>
                UrlEncode(p1.ToString() + c + p2.ToString() + c + p3.ToString() + c + PV_ID);
        }

        public struct Player
        {
            public int Score0;
            public int Score1;
            public string Name0;
            public string Name1;
            public Difficulty Diff;
            public bool Has2P => Name1 != null;

            public void SetValue(string[] data, int i = 0, int offset = 0)
            {
                string[] array = data[i * 13 + offset * 4].Split('.');
                Score0 = int.Parse(array[0]);
                if (array.Length > 1) Score1 = int.Parse(array[1]);
                string text = "";
                for (int j = 0; j < data[i * 13 + 1 + offset * 4].Length; j++)
                {
                    text += data[i * 13 + 1 + offset * 4][j].ToString();
                    if (text.EndsWith("xxx"))
                    {
                        Name0 = text.Remove(text.Length - 3);
                        text = "";
                    }
                }
                if (array.Length == 1) Name0 = text;
                else Name1 = text;
                if (int.TryParse(data[i * 13 + 2 + offset * 4], out int Diff))
                    this.Diff = (Difficulty)Diff;
                else
                    Enum.TryParse(data[i * 13 + 2 + offset * 4], out this.Diff);
            }

            public void SetValue(MsgPack msg)
            {
                 Diff  = (Difficulty)msg.ReadInt32("Diff");
                Score0 = msg.ReadInt32 ("Score");
                 Name0 = msg.ReadString( "Name");
                if (Name0 == null)
                {
                    Score0 = msg.ReadInt32 ("Score0");
                    Score1 = msg.ReadInt32 ("Score1");
                     Name0 = msg.ReadString( "Name0");
                     Name1 = msg.ReadString( "Name1");
                }
                else Name1 = null;
            }

            public MsgPack WriteMP(string name) =>
                Has2P ? new MsgPack(name).Add("Diff", (int)Diff).Add("Score0", Score0)
                .Add("Score1", Score1).Add("Name0", Name0).Add("Name1", Name1) :
                        new MsgPack(name).Add("Diff", (int)Diff)
                .Add("Score" , Score0).Add("Name" , Name0);

            public override string ToString() =>
                (Score0 + (Has2P ? (d + Score1) : "") + c + UrlEncode(Name0) +
                (Has2P ? ("xxx" + UrlEncode(Name1)) : "") + c +
                Diff + c + (Has2P ? "0.1" : "0")).Replace("*", "%2A");
        }

        public struct PvList
        {
            public int PV_ID;
            public bool Enable;
            public bool Extra;
            public Date AdvDemoStart;
            public Date AdvDemoEnd;
            public Date StartShow;
            public Date   EndShow;

            public void SetValue(string[] data, int i = 0)
            {
                PV_ID  = int.Parse(data[i * 7]);
                Enable = int.Parse(data[i * 7 + 1]) == 1;
                Extra  = int.Parse(data[i * 7 + 2]) == 1;
                AdvDemoStart.SetValue(data[i * 7 + 3]);
                AdvDemoEnd  .SetValue(data[i * 7 + 4]);
                StartShow   .SetValue(data[i * 7 + 5]);
                  EndShow   .SetValue(data[i * 7 + 6]);
            }

            public void SetValue(MsgPack msg, bool Compact)
            {
                this.Enable =  true;
                this.Extra  = false;

                int? ID = msg.ReadNInt32("PV_ID");
                if (ID != null) PV_ID = (int)ID;
                else { ID = msg.ReadNInt32("ID"); if (ID != null) PV_ID = (int)ID; }
                bool? Enable = msg.ReadNBoolean("Enable");
                bool? Extra  = msg.ReadNBoolean("Extra");
                if (Enable != null) this.Enable = (bool)Enable;
                if (Extra  != null) this.Extra  = (bool)Extra ;
                if (Compact)
                {
                    AdvDemoStart.SetValue(msg.ReadNInt32("AdvDemoStart"),  true);
                    AdvDemoEnd  .SetValue(msg.ReadNInt32("AdvDemoEnd"  ), false);
                    StartShow   .SetValue(msg.ReadNInt32("StartShow"   ), false);
                      EndShow   .SetValue(msg.ReadNInt32(  "EndShow"   ),  true);
                    return;
                }
                MsgPack Temp;
                if ((Temp = msg["AdvDemoStart", true]).NotNull) AdvDemoStart.SetValue(Temp,  true);
                if ((Temp = msg["AdvDemoEnd"  , true]).NotNull) AdvDemoEnd  .SetValue(Temp, false);
                if ((Temp = msg["StartShow"   , true]).NotNull) StartShow   .SetValue(Temp, false);
                if ((Temp = msg[  "EndShow"   , true]).NotNull)   EndShow   .SetValue(Temp,  true);
                Temp.Dispose();
            }

            public MsgPack WriteMP(bool Compact)
            {
                MsgPack MsgPack = MsgPack.New;
                MsgPack.Add("ID", PV_ID);
                if (!Enable) MsgPack.Add("Enable", Enable);
                if ( Extra ) MsgPack.Add("Extra" , Extra );
                if (Compact)
                {
                    if (AdvDemoStart.WriteUpper) MsgPack.Add("AdvDemoStart", AdvDemoStart.WriteInt());
                    if (AdvDemoEnd  .WriteLower) MsgPack.Add("AdvDemoEnd"  , AdvDemoEnd  .WriteInt());
                    if (StartShow   .WriteLower) MsgPack.Add("StartShow"   , StartShow   .WriteInt());
                    if (  EndShow   .WriteUpper) MsgPack.Add(  "EndShow"   ,   EndShow   .WriteInt());
                }
                else
                {
                    if (AdvDemoStart.WriteUpper) MsgPack.Add(AdvDemoStart.WriteMP("AdvDemoStart"));
                    if (AdvDemoEnd  .WriteLower) MsgPack.Add(AdvDemoEnd  .WriteMP("AdvDemoEnd"  ));
                    if (StartShow   .WriteLower) MsgPack.Add(StartShow   .WriteMP("StartShow"   ));
                    if (  EndShow   .WriteUpper) MsgPack.Add(  EndShow   .WriteMP(  "EndShow"   ));
                }
                return MsgPack;
            }

            public override string ToString() =>
                UrlEncode(PV_ID + c + (Enable ? 1 : 0) + c + (Extra ? 1 : 0) + c +
                    AdvDemoStart.ToString() + c + AdvDemoEnd.ToString() + c +
                    StartShow.ToString() + c + EndShow.ToString());
        }

        public struct Date
        {
            private int  year;
            private int month;
            private int  day;

            public int  Year { get =>  year; set {  year = value; CheckDate(); } }

            public int Month { get => month; set { month = value; CheckDate(); } }

            public int   Day { get =>   day; set {   day = value; CheckDate(); } }

            public bool WriteUpper => Year != 2029 || Month != 1 || Day != 1;
            public bool WriteLower => Year != 2000 || Month != 1 || Day != 1;

            public void SetDefaultLower() => Year = 2000;
            public void SetDefaultUpper() => Year = 2029;

            public void SetValue(string data)
            {
                string[] array = data.Split('-');
                if (array.Length == 3)
                {
                     Year = int.Parse(array[0]);
                    Month = int.Parse(array[1]);
                      Day = int.Parse(array[2]);
                }
            }

            public void SetValue(int? YMD, bool SetDefaultUpper)
            {
                if (!SetDefaultUpper) SetDefaultLower();
                else             this.SetDefaultUpper();
                if (YMD != null)
                {
                    year  = YMD.Value / 10000;
                    month = YMD.Value / 100 % 100;
                    day   = YMD.Value % 100;
                    CheckDate();
                }
            }

            public void SetValue(MsgPack msg, bool SetDefaultUpper)
            {
                if (!SetDefaultUpper) SetDefaultLower();
                else             this.SetDefaultUpper();
                int?  Year = msg.ReadNInt32( "Year");
                int? Month = msg.ReadNInt32("Month");
                int?   Day = msg.ReadNInt32(  "Day");
                if ( Year != null)  year =  Year.Value;
                if (Month != null) month = Month.Value;
                if (  Day != null)   day =   Day.Value;
                CheckDate();
            }

            public int WriteInt() =>
                (Year * 100 + Month) * 100 + Day;

            public MsgPack WriteMP(string name) =>
                new MsgPack(name).Add("Year", Year).Add("Month", Month).Add("Day", Day);

            private void CheckDate()
            {
                if (year <  2000) { year = 2000; month = 1; day = 1; return; }
                if (year >= 2029) { year = 2029; month = 1; day = 1; return; }
                     if (month <  1) month = 1;
                else if (month > 12) month = 12;
                     if (day <  1) day = 1;
                else if (day > 31 && (month == 1 || month ==  3 || month ==  5 ||
                        month == 7 || month == 8 || month == 10 || month == 12)) day = 31;
                else if (day > 30 && (month == 4 || month ==  6 ||
                                      month == 9 || month == 11)) day = 30;
                else if (day > 29 && month == 2 && year % 4 == 0) day = 29;
                else if (day > 28 && month == 2 && year % 4 != 0) day = 28;
            }

            public override string ToString() =>
                Year.ToString("d4") + "-" + Month.ToString("d2") + "-" + Day.ToString("d2");
        }

        public enum Difficulty
        {
            Easy    = 0,
            Normal  = 1,
            Hard    = 2,
            Extreme = 3,
        }
    }
}
