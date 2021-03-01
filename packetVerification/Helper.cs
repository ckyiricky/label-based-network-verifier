using System;
using System.Collections;
using System.Collections.Generic;
using ZenLib;
using static ZenLib.Language;

namespace LabelBasedNetworkVerification
{
    public static class HelperFunctions
    {
        public static Zen<IList<bool>> And(this Zen<IList<bool>> ma, Zen<IList<bool>> mb)
        {
            var index = mb.Length(); 
            var ret = ma.Select(_ => Language.And(ma.At(index-=1).Value(), mb.At(index).Value()));
            //var ret = mb.Select(r => mb.At(index -= 1).Value());
            return ret;
        }
        public static Zen<IList<bool>> Or(this Zen<IList<bool>> ma, Zen<IList<bool>> mb)
        {
            var index = mb.Length(); 
            var ret = ma.Select(_ => Language.Or(ma.At(index-=1).Value(), mb.At(index).Value()));
            //var ret = mb.Select(r => mb.At(index -= 1).Value());
            return ret;
        }
        public static Zen<IList<bool>> Not(this Zen<IList<bool>> ma)
        {
            var ret = ma.Select(r => Language.Not(r));
            return ret;
        }
        public static bool EqualToNumber(this Zen<ushort> a, ushort b)
        {
            return a.ToString().Equals(b.ToString());
        }
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
