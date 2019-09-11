﻿using System;
using KKdBaseLib;
using KKdMainLib.IO;
using KKdDEX = KKdMainLib.DEX;

namespace PD_Tool.Tools
{
    public class DEX
    {
        public static void Processor(bool JSON)
        {
            Console.Title = "DEX Converter";
            KKdDEX DEX;
            Program.Choose(1, "dex", out string[] FileNames);
            if (FileNames.Length < 1) return;
            string filepath = "";
            string ext      = "";
            
            bool  MP   = true;
            bool _JSON = true;
            foreach (string file in FileNames)
                     if (file.EndsWith(".mp"  ))
                {  MP   = false; break; }
                else if (file.EndsWith(".json"))
                { _JSON = false; break; }

            Console.Clear();
            string format = "";
            Program.ConsoleDesign(true);
            Program.ConsoleDesign("        Choose type of exporting file:");
            Program.ConsoleDesign(false);
            Program.ConsoleDesign("1. F/FT PS3/PS4/PSVita");
            Program.ConsoleDesign("2. F2   PS3/PSVita");
            Program.ConsoleDesign("3. X    PS4/PSVita");
            if ( MP   && !JSON) Program.ConsoleDesign("9. MessagePack");
            if (_JSON &&  JSON) Program.ConsoleDesign("9. JSON");
            Program.ConsoleDesign(false);
            Program.ConsoleDesign(true);
            Console.WriteLine();
            format = Console.ReadLine();
            
            Format Format = Format.NULL;
                 if (format == "1") Format = Format.F   ;
            else if (format == "2") Format = Format.F2LE;
            else if (format == "3") Format = Format.X   ;
            else if (format == "9" && (MP && _JSON)) Format = Format.NULL;
            else return;

            int state;
            foreach (string file in FileNames)
            {
                DEX = new KKdDEX();
                ext      = Path.GetExtension(file);
                filepath = file.Replace(ext, "");
                ext = ext.ToLower();
                
                Console.Title = "DEX Converter: " + Path.GetFileNameWithoutExtension(file);
                if (ext == ".bin" || ext == ".dex")
                     state = DEX.    DEXReader(filepath, ext );
                else state = DEX.MsgPackReader(filepath, JSON);

                if (state == 1)
                {
                    if (Format > Format.NULL)
                         DEX.    DEXWriter(filepath, Format);
                    else DEX.MsgPackWriter(filepath, JSON);
                }
                DEX = null;
            }
        }
    }
}
