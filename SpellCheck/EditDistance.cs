using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellCheck
{
    class EditDistanceLength
    {
        public EditDistanceLength()
        {

        }
        private int ComputeLevenshteinDistance(string source, string target)
        {
            if ((source == null) || (target == null)) return 0;
            if ((source.Length == 0) || (target.Length == 0)) return 0;
            if (source == target) return source.Length;

            int sourceWordCount = source.Length;
            int targetWordCount = target.Length;

            // Step 1
            if (sourceWordCount == 0)
                return targetWordCount;

            if (targetWordCount == 0)
                return sourceWordCount;

            int[,] distance = new int[sourceWordCount + 1, targetWordCount + 1];

            // Step 2
            for (int i = 0; i <= sourceWordCount; distance[i, 0] = i++) ;
            for (int j = 0; j <= targetWordCount; distance[0, j] = j++) ;

            for (int i = 1; i <= sourceWordCount; i++)
            {
                for (int j = 1; j <= targetWordCount; j++)
                {
                    // Step 3
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                    // Step 4
                    distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
                }
            }

            return distance[sourceWordCount, targetWordCount];
        }
        public double CalculateSimilarity(string source, string target)
        {
            //F23.StringSimilarity.Cosine cosine = new F23.StringSimilarity.Cosine();
            //F23.StringSimilarity.Levenshtein levenshtein = new F23.StringSimilarity.Levenshtein();
            F23.StringSimilarity.NGram ngram = new F23.StringSimilarity.NGram();
            //F23.StringSimilarity.JaroWinkler jaroWinkler = new F23.StringSimilarity.JaroWinkler();
            //double cosineRes = cosine.Distance(source, target);
            //double levenshteinRes = (1.0 - ((double)levenshtein.Distance(source, target) / (double)Math.Max(source.Length, target.Length)));
            double ngramRes = 1 - ngram.Distance(source, target);
            //double jaroWinklerRes = 1- jaroWinkler.Distance(source, target);
            if ((source.Length * 2 <= target.Length || target.Length * 2 <= source.Length) &&
                (source.Contains(target) || target.Contains(source)))
            {
                return 2.5;
            }
            if ((source.Length * 2 <= target.Length || target.Length * 2 <= source.Length) &&
                (!source.Contains(target) || !target.Contains(source)))
            {
                return 0;
            }

            //if ((source == null) || (target == null)) return 0.0;
            //if ((source.Length == 0) || (target.Length == 0)) return 0.0;
            //if (source == target) return 1.0;

            //int stepsToSame = ComputeLevenshteinDistance(source, target);
            if (source.Length == 1 && target == source || target.Length == 1 && target == source)
                return 1;
            if (source.Length == 1 && target.Length > 1 || source.Length == 1 && target != source ||
                target.Length == 1 && source.Length > 1 || target.Length == 1 && target != source)
                return 0;
            //return (1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length)));
            return ngramRes;
        }
    }
}
