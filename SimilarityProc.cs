using Microsoft.SqlServer.Server;
using System;
using System.Linq;

/// <summary>
/// http://blog.hoegaerden.be/category/sqlserver/master-data-services/
/// </summary>
public class SimilarityProc
{
    [Microsoft.SqlServer.Server.SqlProcedure]
    public static double? Similarity(string source, string target, int method, double weight, double minScore)
    {
        //SqlContext.Pipe.Send("Hello world!" + Environment.NewLine);
        //text = "Hello world!";
        double? score = null;

        switch (method)
        {
            case 0:
                score = DamerauLevenshtein(source, target);
                break;

            case 2:
                score = JaroWinkler(source, target, weight);
                break;

            case 4:
                score = Levenshtein(source, target);
                break;

            default:
                break;
        }

        // This replicates the functionality of SQL Server. 
        // Return 0 if score is less than minScore
        if (score != null && score < minScore)
        {
            score = 0;
        }

        return score;
    }

    /// <summary>
    /// Jaro-Winkler
    /// https://stackoverflow.com/questions/19123506/jaro-winkler-distance-algorithm-in-c-sharp
    /// </summary>
    /// <param name="aString1">First String</param>
    /// <param name="aString2">Second String</param>
    /// <returns>Returns the distance between the specified strings. The distance is symmetric and will fall in the range 0 (no match) to 1 (perfect match). </returns>
    private static double JaroWinkler(string aString1, string aString2, double mWeightThreshold)
    {
        // Size of the prefix to be concidered by the Winkler modification. 
        // Winkler's paper used a default value of 4
        int mNumChars = 4;

        int lLen1 = aString1.Length;
        int lLen2 = aString2.Length;
        if (lLen1 == 0)
            return lLen2 == 0 ? 1.0 : 0.0;

        int lSearchRange = Math.Max(0, Math.Max(lLen1, lLen2) / 2 - 1);

        // default initialized to false
        bool[] lMatched1 = new bool[lLen1];
        bool[] lMatched2 = new bool[lLen2];

        int lNumCommon = 0;
        for (int i = 0; i < lLen1; ++i)
        {
            int lStart = Math.Max(0, i - lSearchRange);
            int lEnd = Math.Min(i + lSearchRange + 1, lLen2);
            for (int j = lStart; j < lEnd; ++j)
            {
                if (lMatched2[j]) continue;
                if (aString1[i] != aString2[j])
                    continue;
                lMatched1[i] = true;
                lMatched2[j] = true;
                ++lNumCommon;
                break;
            }
        }
        if (lNumCommon == 0) return 0.0;

        int lNumHalfTransposed = 0;
        int k = 0;
        for (int i = 0; i < lLen1; ++i)
        {
            if (!lMatched1[i]) continue;
            while (!lMatched2[k]) ++k;
            if (aString1[i] != aString2[k])
                ++lNumHalfTransposed;
            ++k;
        }
        // System.Diagnostics.Debug.WriteLine("numHalfTransposed=" + numHalfTransposed);
        int lNumTransposed = lNumHalfTransposed / 2;

        // System.Diagnostics.Debug.WriteLine("numCommon=" + numCommon + " numTransposed=" + numTransposed);
        double lNumCommonD = lNumCommon;
        double lWeight = (lNumCommonD / lLen1
                         + lNumCommonD / lLen2
                         + (lNumCommon - lNumTransposed) / lNumCommonD) / 3.0;

        if (lWeight <= mWeightThreshold) return lWeight;
        int lMax = Math.Min(mNumChars, Math.Min(aString1.Length, aString2.Length));
        int lPos = 0;
        while (lPos < lMax && aString1[lPos] == aString2[lPos])
            ++lPos;
        if (lPos == 0) return lWeight;

        return lWeight + 0.1 * lPos * (1.0 - lWeight);
    }

    /// <summary>
    /// Levenshtein Distance
    /// https://en.wikibooks.org/wiki/Algorithm_Implementation/Strings/Levenshtein_distance
    /// </summary>
    /// <param name="source">First String</param>
    /// <param name="target">Second String</param>
    /// <returns>Returns the number of edits needed to turn one string into another.</returns>
    private static double Levenshtein(string source, string target)
    {
        if (string.IsNullOrEmpty(source))
        {
            if (string.IsNullOrEmpty(target)) return 0;
            return target.Length;
        }
        if (string.IsNullOrEmpty(target)) return source.Length;

        if (source.Length > target.Length)
        {
            var temp = target;
            target = source;
            source = temp;
        }

        int m = target.Length;
        int n = source.Length;
        int[,] distance = new int[2, m + 1];
        // Initialize the distance matrix
        for (var j = 1; j <= m; j++) distance[0, j] = j;

        var currentRow = 0;
        for (var i = 1; i <= n; ++i)
        {
            currentRow = i & 1;
            distance[currentRow, 0] = i;
            var previousRow = currentRow ^ 1;
            for (var j = 1; j <= m; j++)
            {
                var cost = (target[j - 1] == source[i - 1] ? 0 : 1);
                distance[currentRow, j] = Math.Min(Math.Min(
                            distance[previousRow, j] + 1,
                            distance[currentRow, j - 1] + 1),
                            distance[previousRow, j - 1] + cost);
            }
        }

        var result = Convert.ToDouble(distance[currentRow, m]);

        if (result == 0)
        {
            return 1;
        }
        else
        {
            return 1 - (Convert.ToDouble(result) / Convert.ToDouble(Math.Max(source.Length, target.Length)));
        }
    }

    /// <summary>
    /// Damerau-Levenshtein Distance
    /// https://gist.github.com/wickedshimmy/449595/ec5535d8d967741e64af5a1a5c843f34eed49381
    /// </summary>
    /// <param name="source">First String</param>
    /// <param name="target">Second String</param>
    /// <returns>Returns the number of edits needed to turn one string into another.</returns>
    private static double DamerauLevenshtein(string source, string target)
    {
        int len_orig = source.Length;
        int len_diff = target.Length;

        var matrix = new int[len_orig + 1, len_diff + 1];
        for (int i = 0; i <= len_orig; i++)
            matrix[i, 0] = i;
        for (int j = 0; j <= len_diff; j++)
            matrix[0, j] = j;

        for (int i = 1; i <= len_orig; i++)
        {
            for (int j = 1; j <= len_diff; j++)
            {
                int cost = target[j - 1] == source[i - 1] ? 0 : 1;
                var vals = new int[] {
                    matrix[i - 1, j] + 1,
                    matrix[i, j - 1] + 1,
                    matrix[i - 1, j - 1] + cost
                };
                matrix[i, j] = vals.Min();
                if (i > 1 && j > 1 && source[i - 1] == target[j - 2] && source[i - 2] == target[j - 1])
                    matrix[i, j] = Math.Min(matrix[i, j], matrix[i - 2, j - 2] + cost);
            }
        }

        var result = Convert.ToDouble(matrix[len_orig, len_diff]);

        if (result == 0)
        {
            return 1;
        }
        else
        {
            return 1 - (Convert.ToDouble(result) / Convert.ToDouble(Math.Max(source.Length, target.Length)));
        }
    }
}
