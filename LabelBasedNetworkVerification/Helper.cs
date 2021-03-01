﻿using System;
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
    }
}
