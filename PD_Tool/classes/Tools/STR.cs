﻿using System;
using KKdMainLib.IO;
using KKdSTR = KKdMainLib.STR;

namespace PD_Tool.Tools
{
    public class STR
    {
        public static void Processor(bool JSON)
        {
            Console.Title = "STR Converter";
            Program.Choose(1, "str", out string[] FileNames);

            KKdSTR Data;
            string filepath = "";
            string ext      = "";
            foreach (string file in FileNames)
            {
                ext      = Path.GetExtension(file);
                filepath = file.Replace(ext, "");
                ext      = ext.ToLower();
                Data = new KKdSTR();

                Console.Title = "PD_Tool: Converter Tools: STR Reader: " +
                    Path.GetFileNameWithoutExtension(file);
                if (ext == ".str" || ext == ".bin")
                {
                    Data.STRReader    (filepath, ext);
                    Data.MsgPackWriter(filepath, JSON);
                }
                else if (ext == ".json" || ext == ".mp")
                {
                    Data.MsgPackReader(filepath, ext == ".json");
                    Data.STRWriter    (filepath);
                }
                Data = null;
            }
        }
    }
}
