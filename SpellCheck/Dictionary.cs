using System;
using System.Text.RegularExpressions;

namespace Publicar
{
    class Dictionary
    {
        static void Main(string[] args)
        {
            int initialCapacity = 82765;            
            string dictionaryPath = @"D:\sbwce.txt";
            int prefixLength = 7;
            int maxEditDistanceDictionary = 2; 
            var symSpell = new SymSpell(initialCapacity, maxEditDistanceDictionary, prefixLength);

            symSpell.CreateDictionary(dictionaryPath);            
            Type typecontroller = typeof(SymSpell);
            System.Reflection.FieldInfo finfo = typecontroller.GetField("words", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField);
            System.Collections.Generic.Dictionary<string, System.Int64> collection = null;
            if (finfo != null)
                collection = (System.Collections.Generic.Dictionary<string, System.Int64>)finfo.GetValue(symSpell);

            using (System.IO.StreamWriter fileWriter = new System.IO.StreamWriter(@"D:\Personal\Master\Materias\TFM SLN\DictionaryFiles\load4.log"))
            {
                foreach (System.Collections.Generic.KeyValuePair<string, System.Int64> kvPair in collection)
                {
                    if (kvPair.Value > 50 && !Regex.IsMatch(kvPair.Key, @"^-?\d+$"))
                        fileWriter.WriteLine("{0} {1}", kvPair.Key, kvPair.Value);
                }
                fileWriter.Close();
            };           
        }
    }
}
