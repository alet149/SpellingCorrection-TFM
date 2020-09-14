using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Data.OleDb;

namespace SpellCheck
{
    class spellCheck
    { 
        public static bool IsAlpha(String strToCheck)
        {
            Regex objAlphaPattern = new Regex("[^a-zA-ZñÑáéíóúÁÉÍÓÚüÜ0-9]");
            return !objAlphaPattern.IsMatch(strToCheck);
        }       
 
        private static void CreateExcelFileExperimento(Dictionary<int, ExperimentSpell> matrix, string experimento)
        {
            string fileName = @"D:\cuantificacion\Experimentos\experimento" + experimento + ".xlsx";
            string fileNameTemplate = @"D:\cuantificacion\Experimentos\experimentoTemplate.xlsx";
            string connectionString = String.Format("Provider=Microsoft.ACE.OLEDB.12.0 ;Data Source={0}; Extended Properties='Excel 12.0; HDR=Yes'", fileName);

            if (File.Exists(fileName))
                File.Delete(fileName);
            File.Copy(fileNameTemplate, fileName);

            using OleDbConnection cn = new OleDbConnection(connectionString);
            cn.Open();
            int i = 0;
            string strOriginal;
            foreach (KeyValuePair<int, ExperimentSpell> item in matrix)
            {
                strOriginal = ((ExperimentSpell)item.Value).original is null ? "" : ((ExperimentSpell)item.Value).original.Replace("'", "");
                OleDbCommand cmd1 = new OleDbCommand();
                string sql = "INSERT INTO [Hoja1$] (Original, DefaultLookup,Correction) " +
                        "values('" + strOriginal + "', '" +
                        ((ExperimentSpell)item.Value).correctionLookupCompound.Replace("'", "") + "', '" +
                        ((ExperimentSpell)item.Value).correction.Replace("'", "") + "')";
                cmd1.Connection = cn;
                cmd1.CommandText = sql;
                cmd1.ExecuteNonQuery();
            }
            cn.Close();
        }
        private static void Experimento3()
        {
            string strPath = @"D:\json\";
            string[] fileEntries = Directory.GetFiles(strPath);
            StringBuilder OCROriginal = new StringBuilder();
            string fileName = @"D:\cuantificacion\Experimentos\experimento3.xlsx";
            string connectionString = String.Format(@"Provider=Microsoft.ACE.OLEDB.12.0;" +
                    "Data Source={0};Extended Properties='Excel 12.0;HDR=YES;IMEX=0'", fileName);
            EditDistanceLength editDistance = new EditDistanceLength();
            const int initialCapacity = 82765;
            const int maxEditDistance = 5;
            const int prefixLength = 7;
            SymSpell symSpell = new SymSpell(initialCapacity, maxEditDistance, prefixLength);
            Dictionary<int, ExperimentSpell> excelMatrix = new Dictionary<int, ExperimentSpell>();
            foreach (string path in fileEntries)
            {
                string jsonText = File.ReadAllText(path, Encoding.Default);
                var response = Google.Protobuf.JsonParser.Default.Parse<Google.Cloud.Vision.V1.AnnotateFileResponse>(jsonText);
                foreach (var respuestas in response.Responses)
                {
                    var annotation = respuestas.FullTextAnnotation;
                    if (annotation != null)
                        OCROriginal.Append(annotation.Text);
                }
            }
            symSpell.LoadDictionary(@"D:\load8.txt", 0, 1);
            List<SymSpell.SuggestItem> suggestions = symSpell.LookupCompound(OCROriginal.ToString(), 2);
                var arraySymspell = suggestions[0].ToString().Replace("\n", " ").Replace("{", "").Replace("}", "").Split(' ');
            var arrayOCROriginal = OCROriginal.ToString().Replace("\n", " ").Replace("{", "").Replace("}", "").Replace(": ", "***").Replace(" : ", " ").Replace(":", " ").Replace("***", ": ").Replace(". ", " ").Replace(", ", " ").Replace("-", " ").Split(' ');
            int j = 0, k = 0;
            double similarity;

            for (int i = 0; i < arraySymspell.Length; i++)
            {
                if (j == arrayOCROriginal.Length)
                    break;
                similarity = editDistance.CalculateSimilarity(arraySymspell[i], arrayOCROriginal[j].ToLower());
                ExperimentSpell exp1 = new ExperimentSpell();

                if (similarity == 1)
                {
                    exp1.correction = "igual";
                    exp1.correctionLookupCompound = arraySymspell[i];
                    exp1.original = arrayOCROriginal[j];
                    j++;
                }
                else
                {
                    if (similarity >= .4)
                    {
                        exp1.correction = "Corregida";
                        exp1.correctionLookupCompound = arraySymspell[i];
                        exp1.original = arrayOCROriginal[j];
                        j++;
                    }
                    else
                    {
                        if (similarity > 0.06)
                        {
                            exp1.correction = "Espacios";
                            exp1.correctionLookupCompound = arraySymspell[i];
                            exp1.original = arrayOCROriginal[j];

                        }
                        else
                        {
                            if (j > 0)
                                similarity = editDistance.CalculateSimilarity(arraySymspell[i], arrayOCROriginal[j - 1].ToLower());
                            else
                                similarity = 0;
                            if (similarity == 1)
                            {
                                j--;
                                exp1.correction = "igual";
                                exp1.correctionLookupCompound = arraySymspell[i];
                                exp1.original = arrayOCROriginal[j];
                            }
                            else
                            {
                                if (similarity >= .4)
                                {
                                    j--;
                                    exp1.correction = "Corregida";
                                    exp1.correctionLookupCompound = arraySymspell[i];
                                    exp1.original = arrayOCROriginal[j];
                                }
                                else
                                {
                                    if (similarity > 0.06)
                                    {
                                        j--;
                                        exp1.correction = "Espacios";
                                        exp1.correctionLookupCompound = arraySymspell[i];
                                        exp1.original = arrayOCROriginal[j];

                                    }
                                    else
                                    {
                                        if (j + 1 < arrayOCROriginal.Length)
                                            similarity = editDistance.CalculateSimilarity(arraySymspell[i], arrayOCROriginal[j + 1].ToLower());
                                        else
                                            similarity = 0;

                                        if (similarity == 1)
                                        {
                                            j++;
                                            exp1.correction = "igual";
                                            exp1.correctionLookupCompound = arraySymspell[i];
                                            exp1.original = arrayOCROriginal[j];
                                        }
                                        else
                                        {
                                            if (similarity >= .4)
                                            {
                                                j++;
                                                exp1.correction = "Corregida";
                                                exp1.correctionLookupCompound = arraySymspell[i];
                                                exp1.original = arrayOCROriginal[j];
                                            }
                                            else
                                            {
                                                if (similarity > 0.06)
                                                {
                                                    j++;
                                                    exp1.correction = "Espacios";
                                                    exp1.correctionLookupCompound = arraySymspell[i];
                                                    exp1.original = arrayOCROriginal[j];

                                                }
                                                else
                                                {
                                                    exp1.correction = "Error";
                                                    exp1.correctionLookupCompound = arraySymspell[i];
                                                    exp1.original = arrayOCROriginal[j];
                                                    j++;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                excelMatrix.Add(k++, exp1);
            }
            CreateExcelFileExperimento(excelMatrix, "3");            
        }
        private static void Experimento2()
        {
            Stopwatch stopWatch = new Stopwatch();
            string strPath = @"D:\json\";
            string[] fileEntries = Directory.GetFiles(strPath);
            StringBuilder OCROriginal = new StringBuilder();

            EditDistanceLength editDistance = new EditDistanceLength();
            //Symspell parameters
            const int initialCapacity = 82765;
            const int maxEditDistance = 5;
            const int prefixLength = 7;
            SymSpell symSpell = new SymSpell(initialCapacity, maxEditDistance, prefixLength);
            Dictionary<int, ExperimentSpell> excelMatrix = new Dictionary<int, ExperimentSpell>();
            foreach (string path in fileEntries)
            {
                string jsonText = File.ReadAllText(path, Encoding.Default);
                var response = Google.Protobuf.JsonParser.Default.Parse<Google.Cloud.Vision.V1.AnnotateFileResponse>(jsonText);
                foreach (var respuestas in response.Responses)
                {
                    var annotation = respuestas.FullTextAnnotation;
                    if (annotation != null)
                        OCROriginal.Append(annotation.Text);
                }
            }
            symSpell.LoadDictionary(@"D:\load6.txt", 0, 1);
             var arrayOCROriginal = OCROriginal.ToString().Replace("\n", " ").Replace("{", "").Replace("}", "").Replace(": ", "***").Replace(" : ", " ").Replace(":", " ").Replace("***", ": ").Replace(". ", " ").Replace(", ", " ").Replace("-", " ").Split(' ');

            int j = 0, k = 0;

            foreach (string item in arrayOCROriginal)
            {
                ExperimentSpell exp1 = new ExperimentSpell();
                exp1.correction = "igual";
                exp1.original = item;
                exp1.correctionLookupCompound = item;

                List<SymSpell.SuggestItem> suggestions = symSpell.Lookup(item, SymSpell.Verbosity.Top);
                if (suggestions.Count > 0)
                {
                    exp1.correction = "modificada";
                    exp1.correctionLookupCompound = suggestions[0].term;
                }
                excelMatrix.Add(k++, exp1);
            }
            CreateExcelFileExperimento(excelMatrix, "2");

        }
        private static void Experimento2_1()
        {
            Stopwatch stopWatch = new Stopwatch();
            string strPath = @"D:\json\";
            string[] fileEntries = Directory.GetFiles(strPath);
            StringBuilder OCROriginal = new StringBuilder();
            string fileName = @"D:\cuantificacion\Experimentos\experimento2.xlsx";
            string connectionString = String.Format(@"Provider=Microsoft.ACE.OLEDB.12.0;" +
                    "Data Source={0};Extended Properties='Excel 12.0;HDR=YES;IMEX=0'", fileName);
            EditDistanceLength editDistance = new EditDistanceLength();
            //Symspell parameters
            const int initialCapacity = 82765;
            const int maxEditDistance = 5;
            const int prefixLength = 7;
            SymSpell symSpell = new SymSpell(initialCapacity, maxEditDistance, prefixLength);
            Dictionary<int, ExperimentSpell> excelMatrix = new Dictionary<int, ExperimentSpell>();
            foreach (string path in fileEntries)
            {
                string jsonText = File.ReadAllText(path, Encoding.Default);
                var response = Google.Protobuf.JsonParser.Default.Parse<Google.Cloud.Vision.V1.AnnotateFileResponse>(jsonText);
                foreach (var respuestas in response.Responses)
                {
                    var annotation = respuestas.FullTextAnnotation;
                    if (annotation != null)
                        OCROriginal.Append(annotation.Text);
                }
            }

            stopWatch.Start();
            //load symspell dictionary default 
            symSpell.LoadDictionary(@"D:\load8.txt", 0, 1);
            //process symspell
            List<SymSpell.SuggestItem> suggestions = symSpell.LookupCompound(OCROriginal.ToString(), 2);
            stopWatch.Stop();

            var arraySymspell = suggestions[0].ToString().Replace("\n", " ").Replace("}", "").Split(' ');
            var arrayOCROriginal = OCROriginal.ToString().Replace("\n", " ").Replace("}", "").Replace(": ", "***").Replace(" : ", " ").Replace(":", " ").Replace("***", ": ").Replace(". ", " ").Replace(", ", " ").Replace("-", " ").Split(' ');
            int j = 0, k = 0;

            for (int i = 0; i < arraySymspell.Length; i++)
            {
                ExperimentSpell exp1 = new ExperimentSpell();
                exp1.correction = "igual";
                exp1.correctionLookupCompound = arraySymspell[i];
                if (j < arrayOCROriginal.Length)
                    exp1.original = arrayOCROriginal[j];
                else
                    exp1.original = "";
                j++;
                excelMatrix.Add(k++, exp1);
            }           
            CreateExcelFileExperimento(excelMatrix, "2");

        }
        private static void Experimento1()
        {
            Stopwatch stopWatch = new Stopwatch();
            string strPath = @"D:\json\";
            string[] fileEntries = Directory.GetFiles(strPath);
            StringBuilder OCROriginal = new StringBuilder();

            EditDistanceLength editDistance = new EditDistanceLength();
            //Symspell parameters
            const int initialCapacity = 82765;
            const int maxEditDistance = 5;
            const int prefixLength = 7;
            SymSpell symSpell = new SymSpell(initialCapacity, maxEditDistance, prefixLength);
            Dictionary<int, ExperimentSpell> excelMatrix = new Dictionary<int, ExperimentSpell>();
            foreach (string path in fileEntries)
            {
                string jsonText = File.ReadAllText(path, Encoding.Default);
                var response = Google.Protobuf.JsonParser.Default.Parse<Google.Cloud.Vision.V1.AnnotateFileResponse>(jsonText);
                foreach (var respuestas in response.Responses)
                {
                    var annotation = respuestas.FullTextAnnotation;
                    if (annotation != null)
                        OCROriginal.Append(annotation.Text);
                }
            }

            symSpell.LoadDictionary(@"D:\DictionaryFiles\default.txt", 0, 1);
             var arrayOCROriginal = OCROriginal.ToString().Replace("\n", " ").Replace("{", "").Replace("}", "").Replace(": ", "***").Replace(" : ", " ").Replace(":", " ").Replace("***", ": ").Replace(". ", " ").Replace(", ", " ").Replace("-", " ").Split(' ');

            int j = 0, k = 0;
          
            foreach (string item in arrayOCROriginal)
            {
                ExperimentSpell exp1 = new ExperimentSpell();
                exp1.correction = "igual";
                exp1.original = item;
                exp1.correctionLookupCompound = item;

                List<SymSpell.SuggestItem> suggestions = symSpell.Lookup(item, SymSpell.Verbosity.Top);
                if (suggestions.Count > 0)
                {
                    exp1.correction = "modificada";
                    exp1.correctionLookupCompound = suggestions[0].term;
                }
                excelMatrix.Add(k++, exp1);
            }
            CreateExcelFileExperimento(excelMatrix, "1");
        }
    }
}

public struct ExperimentSpell
{
    public string original;
    public string correctionLookupCompound;
    public string correction;
}

