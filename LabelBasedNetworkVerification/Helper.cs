using System;
using System.Collections;
using System.Collections.Generic;
using ZenLib;
using static ZenLib.Language;

namespace LabelBasedNetworkVerification
{
    /// <summary>
    /// Helper functions.
    /// </summary>
    public static class HelperFunctions
    {
        /// <summary>
        /// And operation of two Zen List of bool.
        /// </summary>
        /// <param name="ma">list a.</param>
        /// <param name="mb">list b.</param>
        /// <returns>list of a and b.</returns>
        public static Zen<IList<bool>> And(this Zen<IList<bool>> ma, Zen<IList<bool>> mb)
        {
            var index = mb.Length();
            var ret = ma.Select(_ => Language.And(ma.At(index -= 1).Value(), mb.At(index).Value()));
            return ret;
        }
        /// <summary>
        /// Or operation of two Zen List of bool.
        /// </summary>
        /// <param name="ma">list a.</param>
        /// <param name="mb">list b.</param>
        /// <returns>list of a or b.</returns>
        public static Zen<IList<bool>> Or(this Zen<IList<bool>> ma, Zen<IList<bool>> mb)
        {
            var index = mb.Length();
            var ret = ma.Select(_ => Language.Or(ma.At(index -= 1).Value(), mb.At(index).Value()));
            return ret;
        }
        /// <summary>
        /// Not operation of one Zen List of bool.
        /// </summary>
        /// <param name="ma">list a.</param>
        /// <returns>!a.</returns>
        public static Zen<IList<bool>> Not(this Zen<IList<bool>> ma)
        {
            var ret = ma.Select(r => Language.Not(r));
            return ret;
        }
        /// <summary>
        /// Compare if the value of a Zen ushort is equal to ushort b.
        /// </summary>
        /// <param name="a">Zen ushort.</param>
        /// <param name="b">ushort.</param>
        /// <returns>if their values are equal.</returns>
        public static bool EqualToNumber(this Zen<ushort> a, ushort b)
        {
            return a.ToString().Equals(b.ToString());
        }
        /// <summary>
        /// Transpose a matrix and return it.
        /// </summary>
        /// <param name="x">input matrix.</param>
        /// <returns>transposed matrix(it is the same ref of the input matrix).</returns>
        public static BitArray[] Transpose(this BitArray[] x)
        {
            var n = x.Length;
            for (int i = 0; i < n; ++i)
            {
                for (int j = i; j < n; ++j)
                {
                    var tmp = x[i][j];
                    x[i][j] = x[j][i];
                    x[j][i] = tmp;
                }
            }
            return x;
        }
        /// <summary>
        /// Ad-hoc to get value of pod label of a specific key.
        /// </summary>
        /// <param name="pod"></param>
        /// <param name="key"></param>
        /// <returns>string value of a pod's label associated with the key.</returns>
        public static string LabelValue(this Zen<Pod> pod, Zen<string> key)
        {
            var userValue = pod.GetLabels().Get(key).Value().ToString();
            var index = userValue.IndexOf(" Value=");
            if (index == -1)
                return null;
            var substr = userValue.Substring(index + 7);
            index = substr.IndexOf(',');
            if (index == -1)
                return null;
            return substr.Substring(0, index);
        }
        /// <summary>
        /// Ad-hoc get string value of a Zen string from Zen-IDictionary-(string, string).
        /// </summary>
        /// <param name="s"></param>
        /// <returns>string value of a Zen string.</returns>
        public static string StringValue(this Zen<string> s)
        {
            var userValue = s.ToString();
            var index = userValue.IndexOf(" Value=");
            if (index == -1)
                return null;
            var substr = userValue.Substring(index + 7);
            index = substr.IndexOf(',');
            if (index == -1)
                return null;
            return substr.Substring(0, index);
        }
    }
}
