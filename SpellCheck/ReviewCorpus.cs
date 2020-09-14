using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Publicar
{
    class ReviewCorpus
    {
        private static object _oEndOfDoc = "\\endofdoc";


        public static string Proof(string proofText, Document _wordDoc, Application _wordApp)
        {            
            var language = _wordApp.Languages[WdLanguageID.wdSpanish];
            SpellingSuggestions spellingSuggestions =
                _wordApp.GetSpellingSuggestions(proofText, IgnoreUppercase: true,
                MainDictionary: language.Name);

            foreach (SpellingSuggestion spellingSuggestion in spellingSuggestions)
            {
                proofText = spellingSuggestion.Name;
                break;
            }
            return proofText.ToLower();
        }        

        private static string[] ParseWords(string text)
        {
            MatchCollection mc = Regex.Matches(text, @"['’\w-[_]]+");
            var matches = new string[mc.Count];
            for (int i = 0; i < matches.Length; i++) matches[i] = mc[i].ToString();
            return matches;
        }
        public static bool IsAlpha(String strToCheck)
        {
            return Regex.IsMatch(strToCheck, @"^[a-zA-ZñÑáéíóúÁÉÍÓÚüÜ]+$");
        }
        public static void CorrectCorpus(Application wApp, Document document)
        {
            SpellingSuggestions spellingSuggestions;
            var language = wApp.Languages[WdLanguageID.wdSpanish];

            int count = document.Words.Count;
            for (int i = 1; i <= count; i++)
            {
                var strText = document.Words[i].Text.Trim();
                if (IsAlpha(strText))
                {
                    spellingSuggestions = wApp.GetSpellingSuggestions(strText,
                           IgnoreUppercase: true, MainDictionary: language.Name);
                    if (spellingSuggestions.Count > 0)
                    {

                        document.Words[i].Text = document.Words[i].Text.Replace(strText,
                            spellingSuggestions[1].Name);
                    }
                }
            }
        }
        private static void frecuencyCounter()
        {
            List<int> listaTexto = new List<int>();
            long contador = 0;
            foreach (string item in File.ReadAllLines(@"D:\DictionaryFiles\load1.txt"))
            {
                if (Convert.ToInt32(item.Split(' ')[1]) <= 5) contador++;
            }
            Console.WriteLine(contador);
        }
        private static void deleteBadWords()
        {
            string alphabet = "abcdefghijklmnñopqrstuvwxyz";
            string newWord = "";
            bool contains;
            using (StreamWriter fileWriter = new StreamWriter(@"D:\DictionaryFiles\load7.txt"))
            {
                foreach (string item in File.ReadAllLines(@"D:\DictionaryFiles\load6.txt"))
                {
                    contains = false;
                    foreach (char letter in alphabet)
                    {
                        newWord = letter.ToString() + letter.ToString() + letter.ToString();
                        if (item.Split(' ')[0].Contains(newWord))
                        {
                            contains = true;
                            break;
                        }
                    }
                    if (!contains)
                        fileWriter.WriteLine(item);
                }
                fileWriter.Close();
            }
        }
        private static void DeleteRepeated(string path, string path2)
        {
            List<Frecuency> listaTexto = new List<Frecuency>();
            foreach (string item in File.ReadAllLines(path))
            {
                Frecuency fr = new Frecuency();
                var array = item.Split(' ');
                fr.text = array[0];
                fr.frecuency = array[1];
                listaTexto.Add(fr);
            }
            SortByText sortByText = new SortByText();
            SortByFrecuency sortByFrecuency = new SortByFrecuency();
            DistinctItemComparer distinctItemComparer = new DistinctItemComparer();
            listaTexto.Sort(sortByText);
            List<Frecuency> listaTextoNoDuplicate = listaTexto.Distinct(distinctItemComparer).ToList();
            listaTexto.Sort(sortByFrecuency);

            using StreamWriter fileWriter = new StreamWriter(path2);
            foreach (Frecuency itemfrecuency in listaTextoNoDuplicate)
            {
                var newfrecuency = listaTexto.First(item => string.Equals(itemfrecuency.text, item.text)).frecuency;
                fileWriter.WriteLine(itemfrecuency.text + ' ' + newfrecuency);
            }
            fileWriter.Close();
        }
    }
    public struct Frecuency
    {
        public string text;
        public string frecuency;
    }
    public class SortByText : IComparer<Frecuency>
    {
        public int Compare(Frecuency x, Frecuency y)
        {
            return x.text.CompareTo(y.text);
        }
    }
    public class SortByFrecuency : IComparer<Frecuency>
    {
        public int Compare(Frecuency x, Frecuency y)
        {
            if (Convert.ToInt32(y.frecuency) > Convert.ToInt32(x.frecuency))
                return 1;
            if (Convert.ToInt32(y.frecuency) < Convert.ToInt32(x.frecuency))
                return -1;
            return 0;
        }
    }
    public class DistinctItemComparer : IEqualityComparer<Frecuency>
    {
        public bool Equals(Frecuency x, Frecuency y)
        {
            return x.text.Equals(y.text);
        }

        public int GetHashCode(Frecuency obj)
        {
            return obj.GetHashCode();
        }
    }
}
