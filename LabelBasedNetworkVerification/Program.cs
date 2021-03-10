using System;
using System.Collections.Generic;
using ZenLib;
using static ZenLib.Language;

namespace LabelBasedNetworkVerification
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            Zen<IDictionary<string, string>> l1 = Language.EmptyDict<string, string>();
            Zen<IDictionary<string, string>> l2 = Language.EmptyDict<string, string>();
            Zen<IDictionary<string, string>> l3 = Language.EmptyDict<string, string>();
            l1 = l1.Add("user", "1");
            l2 = l2.Add("user", "2");
            l3 = l3.Add("user", "3");
            var p1 = Pod.Create("default", l1, new List<string>() { "user" });
            var p2 = Pod.Create("default", l2, new List<string>() { "user" });
            var p3 = Pod.Create("default", l3, new List<string>() { "user" });
            Zen<Pod>[] pods = new Zen<Pod>[] { p1, p2, p3 };

            Zen<IDictionary<string, string>> sl1 = Language.EmptyDict<string, string>();
            Zen<IDictionary<string, string>> sl2 = Language.EmptyDict<string, string>();
            Zen<IDictionary<string, string>> al1 = Language.EmptyDict<string, string>();
            Zen<IDictionary<string, string>> al2 = Language.EmptyDict<string, string>();
            sl1 = sl1.Add("user", "1");
            sl2 = sl2.Add("user", "2");
            al1 = al1.Add("user", "2");
            al2 = al2.Add("user", "3");
            var po1 = Policy.Create("default", sl1, al1, al1,
                new List<string>() { "user" },
                new List<string>() { "user" },
                new List<string>() { "user" });
            var po2 = Policy.Create( "default", sl2, al2, al1,
                new List<string>() {"user"},
                new List<string>() {"user"},
                new List<string>() {"user"} );
            Zen<Policy>[] policies = new Zen<Policy>[] { po1, po2 };

            var mx = Algorithms.CreateReachMatrix(pods, policies);
            Console.WriteLine(mx.Length);
            for (int i = 0; i < 3; ++i)
                Console.WriteLine(mx[i]);

            //var mx = Algorithms.CreateNSMatrix(pods);
            var userHash = Algorithms.GetUserHash(pods, "user");
            Console.WriteLine("User 1: {0}",userHash.Get(p1.GetLabels().Get("user").Value().GetHashCode()).Value());
            Console.WriteLine("User 2: {0}",userHash.Get(p2.GetLabels().Get("user").Value().GetHashCode()).Value());
            var allreach = Verifier.AllReachableCheck(mx, true);
            Console.WriteLine(allreach);
            //var crossL = Verifier.UserCrossCheck(mx, userHash, pods, "user");
            //Console.WriteLine("User cross: {0}",crossL);
            var pp1 = Pod.Create("default", EmptyDict<string, string>(), EmptyList<string>());
            var pp2 = Pod.Create("ns1", EmptyDict<string, string>(), EmptyList<string>());
            var pp3 = Pod.Create("ns2", EmptyDict<string, string>(), EmptyList<string>());
            Zen<Pod>[] ppods = new Zen<Pod>[] {pp1, pp2, pp3};
            var output = Algorithms.CreateNSMatrix(ppods);
            Zen<IList<bool>> r1 = Language.EmptyList<bool>();
            r1 = r1.AddBack(true);
            r1 = r1.AddBack(false);
            r1 = r1.AddBack(false);
            Zen<IList<bool>> r2 = Language.EmptyList<bool>();
            r2 = r2.AddBack(false);
            r2 = r2.AddBack(true);
            r2 = r2.AddBack(false);
            Zen<IList<bool>> r3 = Language.EmptyList<bool>();
            r3 = r3.AddBack(false);
            r3 = r3.AddBack(false);
            r3 = r3.AddBack(true);
            Console.WriteLine(r1.ToString());
            Console.WriteLine(output.Get(p1.GetNS().GetHashCode()).Value().ToString());
            */
        }
    }
}
