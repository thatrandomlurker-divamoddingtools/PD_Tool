﻿using KKdMainLib;
using KKdMainLib.IO;

namespace PD_Tool
{
    public class DIVAFILE
    {
        public static void Decrypt(string file)
        {
            Stream reader = File.OpenReader(file);
            if (reader.RI64() != 0x454C494641564944) { reader.C(); return; }
            reader.C();

            System.Console.Title = "DIVAFILE Decrypt: " + Path.GetFileName(file);
            file.Decrypt();
        }

        public static void Encrypt(string file)
        {
            Stream reader = File.OpenReader(file);
            if (reader.RI64() == 0x454C494641564944) { reader.C(); return; }
            reader.C();

            System.Console.Title = "DIVAFILE Encrypt: " + Path.GetFileName(file);
            file.Encrypt();
        }
    }
}
