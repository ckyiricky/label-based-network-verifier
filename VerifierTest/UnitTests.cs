using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZenLib;
using LabelBasedNetworkVerification;
using System.Collections;
using System.Collections.Generic;
using static ZenLib.Language;

namespace VerifierTest
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void NSPodsMapTests()
        {
            var p1 = Pod.Create("default", EmptyDict<string, string>(), EmptyList<string>());
            var p2 = Pod.Create("ns1", EmptyDict<string, string>(), EmptyList<string>());
            var p3 = Pod.Create("ns2", EmptyDict<string, string>(), EmptyList<string>());
            Zen<Pod>[] pods = new Zen<Pod>[] {p1, p2, p3};
            var output = Algorithms.CreateNSMatrix(pods);
            Zen<IList<bool>> r1 = Language.EmptyList<bool>().AddBack(true).AddBack(false).AddBack(false);
            Zen<IList<bool>> r2 = Language.EmptyList<bool>().AddBack(false).AddBack(true).AddBack(false);
            Zen<IList<bool>> r3 = Language.EmptyList<bool>().AddBack(false).AddBack(false).AddBack(true);
            Assert.IsTrue(output.Get(p1.GetNS().GetHashCode()).Value().ToString().Equals(r1.ToString()));
            Assert.IsTrue(output.Get(p2.GetNS().GetHashCode()).Value().ToString().Equals(r2.ToString()));
            Assert.IsTrue(output.Get(p3.GetNS().GetHashCode()).Value().ToString().Equals(r3.ToString()));
        }
        [TestMethod]
        public void NSLabelTest()
        {
            Zen<IDictionary<string, string>> l1 = EmptyDict<string, string>().Add("k1", "v1");
            Zen<IDictionary<string, string>> l2 = EmptyDict<string, string>().Add("k2", "v1");
            Zen<IDictionary<string, string>> l3 = EmptyDict<string, string>().Add("k3", "v1");
            Zen<IList<string>> k1 = EmptyList<string>().AddBack("k1");
            Zen<IList<string>> k2 = EmptyList<string>().AddBack("k2");
            Zen<IList<string>> k3 = EmptyList<string>().AddBack("k3");
            var ns1 = Namespace.Create("defaul", l1, k1);
            var ns2 = Namespace.Create("defaul", l2, k2);
            var ns3 = Namespace.Create("defaul", l3, k3);
            Zen<Namespace>[] namespaces = new Zen<Namespace>[] { ns1, ns2, ns3 };
            var output = Algorithms.CreateNSLabelMatrix(namespaces);
            var r1 = EmptyList<bool>().AddBack(true).AddBack(false).AddBack(false);
            var r2 = EmptyList<bool>().AddBack(false).AddBack(true).AddBack(false);
            var r3 = EmptyList<bool>().AddBack(false).AddBack(false).AddBack(true);
            Assert.IsTrue(output.Get(k1.At(0).Value().GetHashCode()).Value().ToString().Equals(r1.ToString()));
            Assert.IsTrue(output.Get(k2.At(0).Value().GetHashCode()).Value().ToString().Equals(r2.ToString()));
            Assert.IsTrue(output.Get(k3.At(0).Value().GetHashCode()).Value().ToString().Equals(r3.ToString()));
        }
        [TestMethod]
        public void ReachabilityMatrix_NSScope()
        {
            Zen<Pod>[] pods = new Zen<Pod>[]
            {
                Pod.Create("default", EmptyDict<string, string>().Add("k0", "v0"), EmptyList<string>().AddBack("k0")),
                Pod.Create("default", EmptyDict<string, string>().Add("k1", "v1"), EmptyList<string>().AddBack("k1")),
                Pod.Create("default", EmptyDict<string, string>().Add("k2", "v2"), EmptyList<string>().AddBack("k2")),
                Pod.Create("ns1", EmptyDict<string, string>().Add("k3", "v3"), EmptyList<string>().AddBack("k3"))
            };
            Zen<Namespace>[] namespaces = new Zen<Namespace>[]
            {
                Namespace.Create("default", EmptyDict<string, string>(), EmptyList<string>()),
                Namespace.Create("ns1", EmptyDict<string, string>(), EmptyList<string>())
            };
            Zen<Policy>[] policies = new Zen<Policy>[]
            {
                Policy.Create("default", EmptyDict<string, string>().Add("k0", "v0"),
                EmptyDict<string, string>().Add("k1", "v1"), EmptyDict<string, string>(),
                EmptyList<string>().AddBack("k0"), EmptyList<string>().AddBack("k1"), EmptyList<string>()),

                Policy.Create("default", EmptyDict<string, string>().Add("k0", "v0"),
                EmptyDict<string, string>().Add("k3", "v3"), EmptyDict<string, string>(),
                EmptyList<string>().AddBack("k0"), EmptyList<string>().AddBack("k3"), EmptyList<string>()),

                Policy.Create("default", EmptyDict<string, string>().Add("k3", "v3"),
                EmptyDict<string, string>().Add("k2", "v2"), EmptyDict<string, string>(),
                EmptyList<string>().AddBack("k3"), EmptyList<string>().AddBack("k2"), EmptyList<string>()),
            };

            var output = Algorithms.CreateReachMatrix(pods, policies, namespaces);
            var r0 = EmptyList<bool>().AddBack(true).AddBack(true).AddBack(false).AddBack(false);
            var r12 = EmptyList<bool>().AddBack(true).AddBack(true).AddBack(true).AddBack(false);
            var r3 = EmptyList<bool>().AddBack(false).AddBack(false).AddBack(false).AddBack(true);
            Assert.AreEqual(output.Length, 4, "reachability matrix should be 4*4");
            Assert.IsTrue(output[0].ToString().Equals(r0.ToString()), "pod0 has wrong reachability");
            Assert.IsTrue(output[1].ToString().Equals(r12.ToString()), "pod1 has wrong reachability");
            Assert.IsTrue(output[2].ToString().Equals(r12.ToString()), "pod2 has wrong reachability");
            Assert.IsTrue(output[3].ToString().Equals(r3.ToString()), "pod3 has wrong reachability");
        }
        [TestMethod]
        public void ReachabilityMatrix_NSOnly()
        {
            Zen<Pod>[] pods = new Zen<Pod>[]
            {
                Pod.Create("default", EmptyDict<string, string>().Add("k0", "v0"), EmptyList<string>().AddBack("k0")),
                Pod.Create("default", EmptyDict<string, string>().Add("k1", "v1"), EmptyList<string>().AddBack("k1")),
                Pod.Create("ns1", EmptyDict<string, string>().Add("k2", "v2"), EmptyList<string>().AddBack("k2")),
                Pod.Create("ns1", EmptyDict<string, string>().Add("k3", "v3"), EmptyList<string>().AddBack("k3"))
            };
            Zen<Namespace>[] namespaces = new Zen<Namespace>[]
            {
                Namespace.Create("default", EmptyDict<string, string>().Add("k0", "v0"), EmptyList<string>().AddBack("k0")),
                Namespace.Create("ns1", EmptyDict<string, string>().Add("k1", "v1"), EmptyList<string>().AddBack("k1"))
            };
            Zen<Policy>[] policies = new Zen<Policy>[]
            {
                Policy.Create("default", EmptyDict<string, string>().Add("k0", "v0"),
                EmptyDict<string, string>(), EmptyDict<string, string>().Add("k1", "v1"),
                EmptyList<string>().AddBack("k0"), EmptyList<string>(), EmptyList<string>().AddBack("k1")),

                Policy.Create("ns1", EmptyDict<string, string>().Add("k2", "v2"),
                EmptyDict<string, string>(), EmptyDict<string, string>().Add("k0", "v0"),
                EmptyList<string>().AddBack("k2"), EmptyList<string>(), EmptyList<string>().AddBack("k0")),
            };

            var output = Algorithms.CreateReachMatrix(pods, policies, namespaces);
            var r0 = EmptyList<bool>().AddBack(true).AddBack(false).AddBack(true).AddBack(true);
            var r1 = EmptyList<bool>().AddBack(true).AddBack(true).AddBack(false).AddBack(false);
            var r2 = EmptyList<bool>().AddBack(true).AddBack(true).AddBack(true).AddBack(false);
            var r3 = EmptyList<bool>().AddBack(false).AddBack(false).AddBack(true).AddBack(true);
            Assert.AreEqual(output.Length, 4, "reachability matrix should be 4*4");
            Assert.IsTrue(output[0].ToString().Equals(r0.ToString()), "pod0 has wrong reachability");
            Assert.IsTrue(output[1].ToString().Equals(r1.ToString()), "pod1 has wrong reachability");
            Assert.IsTrue(output[2].ToString().Equals(r2.ToString()), "pod2 has wrong reachability");
            Assert.IsTrue(output[3].ToString().Equals(r3.ToString()), "pod3 has wrong reachability");
        }
        [TestMethod]
        public void ReachabilityMatrix_NSAndPod()
        {
            Zen<Pod>[] pods = new Zen<Pod>[]
            {
                Pod.Create("default", EmptyDict<string, string>().Add("k0", "v0"), EmptyList<string>().AddBack("k0")),
                Pod.Create("default", EmptyDict<string, string>().Add("k1", "v1"), EmptyList<string>().AddBack("k1")),
                Pod.Create("ns1", EmptyDict<string, string>().Add("k2", "v2"), EmptyList<string>().AddBack("k2")),
                Pod.Create("ns1", EmptyDict<string, string>().Add("k3", "v3"), EmptyList<string>().AddBack("k3"))
            };
            Zen<Namespace>[] namespaces = new Zen<Namespace>[]
            {
                Namespace.Create("default", EmptyDict<string, string>().Add("k0", "v0"), EmptyList<string>().AddBack("k0")),
                Namespace.Create("ns1", EmptyDict<string, string>().Add("k1", "v1"), EmptyList<string>().AddBack("k1"))
            };
            Zen<Policy>[] policies = new Zen<Policy>[]
            {
                Policy.Create("default", EmptyDict<string, string>().Add("k0", "v0"),
                EmptyDict<string, string>().Add("k2", "v2"), EmptyDict<string, string>().Add("k1", "v1"),
                EmptyList<string>().AddBack("k0"), EmptyList<string>().AddBack("k2"), EmptyList<string>().AddBack("k1")),

                Policy.Create("ns1", EmptyDict<string, string>().Add("k3", "v3"),
                EmptyDict<string, string>().Add("k2","v2"), EmptyDict<string, string>().Add("k0", "v0"),
                EmptyList<string>().AddBack("k3"), EmptyList<string>().AddBack("k2"), EmptyList<string>().AddBack("k0")),
            };
            var output = Algorithms.CreateReachMatrix(pods, policies, namespaces);
            var r0 = EmptyList<bool>().AddBack(true).AddBack(false).AddBack(true).AddBack(false);
            var r1 = EmptyList<bool>().AddBack(true).AddBack(true).AddBack(false).AddBack(false);
            var r2 = EmptyList<bool>().AddBack(false).AddBack(false).AddBack(true).AddBack(true);
            var r3 = EmptyList<bool>().AddBack(false).AddBack(false).AddBack(false).AddBack(true);
            Assert.AreEqual(output.Length, 4, "reachability matrix should be 4*4");
            Assert.IsTrue(output[0].ToString().Equals(r0.ToString()), "pod0 has wrong reachability");
            Assert.IsTrue(output[1].ToString().Equals(r1.ToString()), "pod1 has wrong reachability");
            Assert.IsTrue(output[2].ToString().Equals(r2.ToString()), "pod2 has wrong reachability");
            Assert.IsTrue(output[3].ToString().Equals(r3.ToString()), "pod3 has wrong reachability");
        }
        [TestMethod]
        public void ReachabilityMatrix_AllowAll()
        {
            Zen<Pod>[] pods = new Zen<Pod>[]
            {
                Pod.Create("default", EmptyDict<string, string>().Add("k0", "v0"), EmptyList<string>().AddBack("k0")),
                Pod.Create("default", EmptyDict<string, string>().Add("k1", "v1"), EmptyList<string>().AddBack("k1")),
                Pod.Create("ns1", EmptyDict<string, string>().Add("k2", "v2"), EmptyList<string>().AddBack("k2")),
                Pod.Create("ns1", EmptyDict<string, string>().Add("k3", "v3"), EmptyList<string>().AddBack("k3"))
            };
            Zen<Namespace>[] namespaces = new Zen<Namespace>[]
            {
                Namespace.Create("default", EmptyDict<string, string>().Add("k0", "v0"), EmptyList<string>().AddBack("k0")),
                Namespace.Create("ns1", EmptyDict<string, string>().Add("k1", "v1"), EmptyList<string>().AddBack("k1"))
            };
            Zen<Policy>[] policies = new Zen<Policy>[]
            {
                Policy.Create("default", EmptyDict<string, string>(),
                EmptyDict<string, string>(), EmptyDict<string, string>(),
                EmptyList<string>(), EmptyList<string>(), EmptyList<string>()),

                Policy.Create("ns1", EmptyDict<string, string>().Add("k3", "v3"),
                EmptyDict<string, string>().Add("k2","v2"), EmptyDict<string, string>().Add("k0", "v0"),
                EmptyList<string>().AddBack("k3"), EmptyList<string>().AddBack("k2"), EmptyList<string>().AddBack("k0")),
            };
            var output = Algorithms.CreateReachMatrix(pods, policies, namespaces);
            var r0 = EmptyList<bool>().AddBack(true).AddBack(true).AddBack(false).AddBack(false);
            var r1 = EmptyList<bool>().AddBack(true).AddBack(true).AddBack(false).AddBack(false);
            var r2 = EmptyList<bool>().AddBack(false).AddBack(false).AddBack(true).AddBack(true);
            var r3 = EmptyList<bool>().AddBack(false).AddBack(false).AddBack(false).AddBack(true);
            Assert.AreEqual(output.Length, 4, "reachability matrix should be 4*4");
            Assert.IsTrue(output[0].ToString().Equals(r0.ToString()), "pod0 has wrong reachability");
            Assert.IsTrue(output[1].ToString().Equals(r1.ToString()), "pod1 has wrong reachability");
            Assert.IsTrue(output[2].ToString().Equals(r2.ToString()), "pod2 has wrong reachability");
            Assert.IsTrue(output[3].ToString().Equals(r3.ToString()), "pod3 has wrong reachability");
        }
        [TestMethod]
        public void Reachabilitymatrix_DenyAll()
        {
            Zen<Pod>[] pods = new Zen<Pod>[]
            {
                Pod.Create("default", EmptyDict<string, string>().Add("k0", "v0"), EmptyList<string>().AddBack("k0")),
                Pod.Create("default", EmptyDict<string, string>().Add("k1", "v1"), EmptyList<string>().AddBack("k1")),
                Pod.Create("ns1", EmptyDict<string, string>().Add("k2", "v2"), EmptyList<string>().AddBack("k2")),
                Pod.Create("ns1", EmptyDict<string, string>().Add("k3", "v3"), EmptyList<string>().AddBack("k3"))
            };
            Zen<Namespace>[] namespaces = new Zen<Namespace>[]
            {
                Namespace.Create("default", EmptyDict<string, string>().Add("k0", "v0"), EmptyList<string>().AddBack("k0")),
                Namespace.Create("ns1", EmptyDict<string, string>().Add("k1", "v1"), EmptyList<string>().AddBack("k1"))
            };
            Zen<Policy>[] policies = new Zen<Policy>[]
            {
                Policy.Create("default", EmptyDict<string, string>(),
                EmptyDict<string, string>().Add("k1", "v1"), EmptyDict<string, string>().Add("k0", "v0"),
                EmptyList<string>(), EmptyList<string>().AddBack("k1"), EmptyList<string>().AddBack("k0"),
                True(), True()),

                Policy.Create("ns1", EmptyDict<string, string>().Add("k3", "v3"),
                EmptyDict<string, string>().Add("k2","v2"), EmptyDict<string, string>().Add("k0", "v0"),
                EmptyList<string>().AddBack("k3"), EmptyList<string>().AddBack("k2"), EmptyList<string>().AddBack("k0")),
            };
            var output = Algorithms.CreateReachMatrix(pods, policies, namespaces);
            var r0 = EmptyList<bool>().AddBack(true).AddBack(false).AddBack(false).AddBack(false);
            var r1 = EmptyList<bool>().AddBack(false).AddBack(true).AddBack(false).AddBack(false);
            var r2 = EmptyList<bool>().AddBack(false).AddBack(false).AddBack(true).AddBack(true);
            var r3 = EmptyList<bool>().AddBack(false).AddBack(false).AddBack(false).AddBack(true);
            Assert.AreEqual(output.Length, 4, "reachability matrix should be 4*4");
            Assert.IsTrue(output[0].ToString().Equals(r0.ToString()), string.Format("pod0 has wrong reachability, \nexpect: {0}, \nget{1}", r0.ToString(), output[0].ToString()));
            Assert.IsTrue(output[1].ToString().Equals(r1.ToString()), string.Format("pod1 has wrong reachability, \nexpect: {0}, \nget{1}", r1.ToString(), output[1].ToString()));
            Assert.IsTrue(output[2].ToString().Equals(r2.ToString()), string.Format("pod2 has wrong reachability, \nexpect: {0}, \nget{1}", r2.ToString(), output[2].ToString()));
            Assert.IsTrue(output[3].ToString().Equals(r3.ToString()), string.Format("pod3 has wrong reachability, \nexpect: {0}, \nget{1}", r3.ToString(), output[3].ToString()));
        }
    }
    [TestClass]
    public class HelperTest
    {
        [TestMethod]
        public void TestTranspose()
        {
            BitArray[] x = new BitArray[3]
            {
                new BitArray(new bool[3] {true, true, false}),
                new BitArray(new bool[3] {false, false, true}),
                new BitArray(new bool[3] {true, true, false})
            };
            x.Transpose();
            BitArray[] y = new BitArray[3]
            {
                new BitArray(new bool[3] {true, false, true}),
                new BitArray(new bool[3] {true, false, true}),
                new BitArray(new bool[3] {false, true, false})
            };
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                    Assert.AreEqual(x[i][j], y[i][j], string.Format("{0}{1} doesn't transpose", i, j));
            }
        }
    }
}
