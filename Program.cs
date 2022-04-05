using System;
using System.Linq;
using System.Collections.Generic;
using Tesseract;
using System.IO;
using System.Threading.Tasks;

namespace OCR_Vorgangsnummer
{
    class Program
    {
        static string filePath = @"C:\Users\uplat\OneDrive\Desktop\Arbeit\PS-Eva";
        static string tessPath = @"E:\Saved\OCR_Vorgangsnummer\tessdata";
        static Task<List<string>> vorgangsnummern;
        static void Main(string[] args)
        {
            vorgangsnummern = GetVorgangsNummerFromDocuments(filePath);
        }

        static async Task<List<string>> GetVorgangsNummerFromDocuments(string path_to_file_directory)
        {
            List<string> VorgangsNummer_List = new();
            Task[] tasks = new Task[Directory.EnumerateFiles(path_to_file_directory).Count()];
            int index = 0;

            foreach (string file in Directory.EnumerateFiles(path_to_file_directory))
            {
                tasks[index] = Task.Run(() =>
                {
                    string vorgangsNummer;
                    OCRDocument(out vorgangsNummer, file);
                    VorgangsNummer_List.Add(vorgangsNummer);
                });
                index++;
            }

            Task.WaitAll(tasks);
            return VorgangsNummer_List;
        }
        static void OCRDocument(out string vorgangsNummer, string path_to_file)
        {
            try
            {
                using (TesseractEngine engine = new TesseractEngine(tessPath, "deu", EngineMode.Default))
                {
                    engine.DefaultPageSegMode = PageSegMode.Auto;
                    using (Pix img = Pix.LoadFromFile(path_to_file))
                    {
                        using (Page page = engine.Process(img))
                        {
                            Console.WriteLine("Scanning " + Path.GetFileNameWithoutExtension(path_to_file) + "...");

                            //Dashier ist etwas billig, aber es funktioniert...
                            List<string> pageContent = page.GetText().Split("\n").ToList();
                            string[] relevantLine = pageContent[11].Split(" ");
                            vorgangsNummer = relevantLine[8];
                            List<int> indexToRemove = new();

                            for (int i = 0; i < vorgangsNummer.Length; i++)
                            {
                                if (!Char.IsDigit(vorgangsNummer[i]))
                                {
                                    indexToRemove.Add(i);
                                }
                            }

                            if (indexToRemove.Count > 0)
                            {
                                try
                                {
                                    for (int i = indexToRemove.Count - 1; i >= 0; i--)
                                    {
                                        vorgangsNummer = vorgangsNummer.Remove(indexToRemove[i]);
                                    }
                                }
                                catch (Exception e) { vorgangsNummer = null; Console.WriteLine(e.Message); }
                            }
                            Console.WriteLine("done!");
                        }
                    }
                }
            }
            catch (Exception e) { vorgangsNummer = null; Console.WriteLine(e.Message); }
        }
    }
}
