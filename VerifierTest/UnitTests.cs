using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZenLib;
using LabelBasedNetworkVerification;
using System.Collections;
using System.Collections.Generic;
using static ZenLib.Language;

namespace VerifierTest
{
    /// <summary>
    /// Test algorithms of kano verifier
    /// </summary>
    [TestClass]
    public class AlgorithmTests
    {
        /// <summary>
        /// Test ns(name) - pods is mapped correctly
        /// </summary>
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
        /// <summary>
        /// Test label-ns is mapped correctly
        /// </summary>
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
        /// <summary>
        /// Test one-side(ingress) can't allow traffic from podA to podB
        /// </summary>
        [TestMethod]
        public void ReachabilityMatrix_OneDirectionOnly()
        {
            Zen<Pod>[] pods = new Zen<Pod>[]
            {
                Pod.Create("default", EmptyDict<string, string>().Add("k0", "v0"), EmptyList<string>().AddBack("k0")),
                Pod.Create("ns1", EmptyDict<string, string>().Add("k1", "v1"), EmptyList<string>().AddBack("k1")),
            };
            Zen<Namespace>[] namespaces = new Zen<Namespace>[]
            {
                Namespace.Create("default", EmptyDict<string, string>().Add("k0", "v0"), EmptyList<string>().AddBack("k0")),
                Namespace.Create("ns1", EmptyDict<string, string>().Add("k1", "v1"), EmptyList<string>().AddBack("k1"))
            };
            Zen<Policy>[] policies = new Zen<Policy>[]
            {
                // only one side allows ingress traffic
                Policy.Create("default", EmptyDict<string, string>().Add("k0", "v0"),
                EmptyDict<string, string>(), EmptyDict<string, string>().Add("k1", "v1"),
                EmptyList<string>().AddBack("k0"), EmptyList<string>(), EmptyList<string>().AddBack("k1")),

                Policy.Create("ns1", EmptyDict<string, string>().Add("k1", "v1"),
                EmptyDict<string, string>(), EmptyDict<string, string>().Add("k1", "v1"),
                EmptyList<string>().AddBack("k1"), EmptyList<string>(), EmptyList<string>().AddBack("k1"),
                False(), False()),
            };
            var output = Algorithms.CreateReachMatrix(pods, policies, namespaces).ingressMatrix;
            var r0 = EmptyList<bool>().AddBack(true).AddBack(false);
            var r1 = EmptyList<bool>().AddBack(false).AddBack(true);
            TestHelper.AssertMatrixEqual(output, new Zen<IList<bool>>[] {r0, r1}, "Reachability Matrix, one direction only");
        }
        /// <summary>
        /// Test two-side(ingress+egress) allows traffic from podA to podB
        /// </summary>
        [TestMethod]
        public void ReachabilityMatrix_TwoDirection()
        {
            Zen<Pod>[] pods = new Zen<Pod>[]
            {
                Pod.Create("default", EmptyDict<string, string>().Add("k0", "v0"), EmptyList<string>().AddBack("k0")),
                Pod.Create("ns1", EmptyDict<string, string>().Add("k1", "v1"), EmptyList<string>().AddBack("k1")),
            };
            Zen<Namespace>[] namespaces = new Zen<Namespace>[]
            {
                Namespace.Create("default", EmptyDict<string, string>().Add("k0", "v0"), EmptyList<string>().AddBack("k0")),
                Namespace.Create("ns1", EmptyDict<string, string>().Add("k1", "v1"), EmptyList<string>().AddBack("k1"))
            };
            Zen<Policy>[] policies = new Zen<Policy>[]
            {
                // only ingress is allowed
                Policy.Create("default", EmptyDict<string, string>().Add("k0", "v0"),
                EmptyDict<string, string>(), EmptyDict<string, string>().Add("k1", "v1"),
                EmptyList<string>().AddBack("k0"), EmptyList<string>(), EmptyList<string>().AddBack("k1")),

                // only egress is allowed
                Policy.Create("ns1", EmptyDict<string, string>().Add("k1", "v1"),
                EmptyDict<string, string>(), EmptyDict<string, string>().Add("k0", "v0"),
                EmptyList<string>().AddBack("k1"), EmptyList<string>(), EmptyList<string>().AddBack("k0"),
                False(), False()),
            };
            var ingress = Algorithms.CreateReachMatrix(pods, policies, namespaces).ingressMatrix;
            var r0 = EmptyList<bool>().AddBack(true).AddBack(true);
            var r1 = EmptyList<bool>().AddBack(false).AddBack(true);
            TestHelper.AssertMatrixEqual(ingress, new Zen<IList<bool>>[] { r0, r1 }, "Happy path(ingress matrix)");

            var egress = Algorithms.CreateReachMatrix(pods, policies, namespaces).egressMatrix;
            r0 = EmptyList<bool>().AddBack(true).AddBack(false);
            r1 = EmptyList<bool>().AddBack(true).AddBack(true);
            TestHelper.AssertMatrixEqual(egress, new Zen<IList<bool>>[] { r0, r1 }, "Happy path(egress matrix)");
        }
        /// <summary>
        /// Test when no allowed ns is defined, 
        /// NS of the policy is used
        /// </summary>
        [TestMethod]
        public void ReachabilityMatrix_DefaultNSScope()
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

            var ingress = Algorithms.CreateReachMatrix(pods, policies, namespaces).ingressMatrix;
            var r0 = EmptyList<bool>().AddBack(true).AddBack(true).AddBack(false).AddBack(false);
            var r1 = EmptyList<bool>().AddBack(false).AddBack(true).AddBack(true).AddBack(true);
            var r2 = EmptyList<bool>().AddBack(false).AddBack(true).AddBack(true).AddBack(true);
            var r3 = EmptyList<bool>().AddBack(false).AddBack(true).AddBack(true).AddBack(true);
            TestHelper.AssertMatrixEqual(ingress, new Zen<IList<bool>>[] { r0, r1, r2, r3 }, "Reachability_Matrix_Test default NS(ingress matrix)");
            
            var egress = Algorithms.CreateReachMatrix(pods, policies, namespaces).egressMatrix;
            r0 = EmptyList<bool>().AddBack(true).AddBack(false).AddBack(false).AddBack(false);
            r1 = EmptyList<bool>().AddBack(true).AddBack(true).AddBack(true).AddBack(true);
            r2 = EmptyList<bool>().AddBack(false).AddBack(true).AddBack(true).AddBack(true);
            r3 = EmptyList<bool>().AddBack(false).AddBack(true).AddBack(true).AddBack(true);
            TestHelper.AssertMatrixEqual(egress, new Zen<IList<bool>>[] { r0, r1, r2, r3 }, "Reachability_Matrix_Test default NS(egress matrix)");
        }
        /// <summary>
        /// Test when only ns selector is used, all pods in the ns will be allowed
        /// </summary>
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

            var ingress = Algorithms.CreateReachMatrix(pods, policies, namespaces).ingressMatrix;
            var r0 = EmptyList<bool>().AddBack(true).AddBack(false).AddBack(false).AddBack(true);
            var r1 = EmptyList<bool>().AddBack(false).AddBack(true).AddBack(false).AddBack(true);
            var r2 = EmptyList<bool>().AddBack(false).AddBack(true).AddBack(true).AddBack(false);
            var r3 = EmptyList<bool>().AddBack(false).AddBack(true).AddBack(false).AddBack(true);
            TestHelper.AssertMatrixEqual(ingress, new Zen<IList<bool>>[] { r0, r1, r2, r3 }, "Reachability_Matrix_Test_NS_Only(ingress matrix)");

            var egress = Algorithms.CreateReachMatrix(pods, policies, namespaces).egressMatrix;
            r0 = EmptyList<bool>().AddBack(true).AddBack(false).AddBack(false).AddBack(false);
            r1 = EmptyList<bool>().AddBack(false).AddBack(true).AddBack(true).AddBack(true);
            r2 = EmptyList<bool>().AddBack(false).AddBack(false).AddBack(true).AddBack(false);
            r3 = EmptyList<bool>().AddBack(true).AddBack(true).AddBack(false).AddBack(true);
            TestHelper.AssertMatrixEqual(egress, new Zen<IList<bool>>[] { r0, r1, r2, r3 }, "Reachability_Matrix_Test_NS_Only(egress matrix)");
        }
        /// <summary>
        /// Test when ns + pod selector is used,
        /// only allowed pods in allowed ns are allowed
        /// </summary>
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

                Policy.Create("default", EmptyDict<string, string>().Add("k0", "v0"),
                EmptyDict<string, string>().Add("k2", "v2"), EmptyDict<string, string>().Add("k1", "v1"),
                EmptyList<string>().AddBack("k0"), EmptyList<string>().AddBack("k2"), EmptyList<string>().AddBack("k1"),
                False(), False()),

                Policy.Create("ns1", EmptyDict<string, string>().Add("k3", "v3"),
                EmptyDict<string, string>().Add("k2","v2"), EmptyDict<string, string>().Add("k0", "v0"),
                EmptyList<string>().AddBack("k3"), EmptyList<string>().AddBack("k2"), EmptyList<string>().AddBack("k0")),
            };
            var ingress = Algorithms.CreateReachMatrix(pods, policies, namespaces).ingressMatrix;
            var r0 = EmptyList<bool>().AddBack(true).AddBack(false).AddBack(true).AddBack(false);
            var r1 = EmptyList<bool>().AddBack(false).AddBack(true).AddBack(true).AddBack(false);
            var r2 = EmptyList<bool>().AddBack(true).AddBack(true).AddBack(true).AddBack(false);
            var r3 = EmptyList<bool>().AddBack(false).AddBack(false).AddBack(false).AddBack(true);
            TestHelper.AssertMatrixEqual(ingress, new Zen<IList<bool>>[] { r0, r1, r2, r3 }, "Reachability_Matrix_Test_NS_Pods(ingress matrix)");
            
            var egress = Algorithms.CreateReachMatrix(pods, policies, namespaces).egressMatrix;
            r0 = EmptyList<bool>().AddBack(true).AddBack(false).AddBack(true).AddBack(false);
            r1 = EmptyList<bool>().AddBack(false).AddBack(true).AddBack(true).AddBack(false);
            r2 = EmptyList<bool>().AddBack(true).AddBack(true).AddBack(true).AddBack(false);
            r3 = EmptyList<bool>().AddBack(false).AddBack(false).AddBack(false).AddBack(true);
            TestHelper.AssertMatrixEqual(egress, new Zen<IList<bool>>[] { r0, r1, r2, r3 }, "Reachability_Matrix_Test_NS_Pods(egress matrix)");
        }
        /// <summary>
        /// Test allow all policy
        /// </summary>
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
            var ingress = Algorithms.CreateReachMatrix(pods, policies, namespaces).ingressMatrix;
            var r0 = EmptyList<bool>().AddBack(true).AddBack(false).AddBack(true).AddBack(false);
            var r1 = EmptyList<bool>().AddBack(false).AddBack(true).AddBack(true).AddBack(false);
            var r2 = EmptyList<bool>().AddBack(false).AddBack(false).AddBack(true).AddBack(false);
            var r3 = EmptyList<bool>().AddBack(false).AddBack(false).AddBack(false).AddBack(true);
            TestHelper.AssertMatrixEqual(ingress, new Zen<IList<bool>>[] { r0, r1, r2, r3 }, "Reachability_Matrix_Test_Allow_All(ingress matrix)");
            
            var egress = Algorithms.CreateReachMatrix(pods, policies, namespaces).egressMatrix;
            r0 = EmptyList<bool>().AddBack(true).AddBack(false).AddBack(false).AddBack(false);
            r1 = EmptyList<bool>().AddBack(false).AddBack(true).AddBack(false).AddBack(false);
            r2 = EmptyList<bool>().AddBack(true).AddBack(true).AddBack(true).AddBack(false);
            r3 = EmptyList<bool>().AddBack(false).AddBack(false).AddBack(false).AddBack(true);
            TestHelper.AssertMatrixEqual(egress, new Zen<IList<bool>>[] { r0, r1, r2, r3 }, "Reachability_Matrix_Test_Allow_All(egress matrix)");
        }
        /// <summary>
        /// Test deny all policy
        /// </summary>
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
            };
            var ingress = Algorithms.CreateReachMatrix(pods, policies, namespaces).ingressMatrix;
            var r0 = EmptyList<bool>().AddBack(true).AddBack(false).AddBack(false).AddBack(false);
            var r1 = EmptyList<bool>().AddBack(false).AddBack(true).AddBack(false).AddBack(false);
            var r2 = EmptyList<bool>().AddBack(false).AddBack(false).AddBack(true).AddBack(true);
            var r3 = EmptyList<bool>().AddBack(false).AddBack(false).AddBack(true).AddBack(true);
            TestHelper.AssertMatrixEqual(ingress, new Zen<IList<bool>>[] { r0, r1, r2, r3 }, "Reachability_Matrix_Test_Deny_All(ingress matrix)");

            var egress = Algorithms.CreateReachMatrix(pods, policies, namespaces).egressMatrix;
            r0 = EmptyList<bool>().AddBack(true).AddBack(false).AddBack(false).AddBack(false);
            r1 = EmptyList<bool>().AddBack(false).AddBack(true).AddBack(false).AddBack(false);
            r2 = EmptyList<bool>().AddBack(false).AddBack(false).AddBack(true).AddBack(true);
            r3 = EmptyList<bool>().AddBack(false).AddBack(false).AddBack(true).AddBack(true);
            TestHelper.AssertMatrixEqual(egress, new Zen<IList<bool>>[] { r0, r1, r2, r3 }, "Reachability_Matrix_Test_Deny_All(egress matrix)");
        }
    }
    /// <summary>
    /// Test of helper functions
    /// </summary>
    [TestClass]
    public class HelperTest
    {
        /// <summary>
        /// Test matrix transpose
        /// </summary>
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
    /// <summary>
    /// Helper functions for test
    /// </summary>
    public static class TestHelper
    {
        /// <summary>
        /// Assert two Zen<IList<bool>> are equal
        /// </summary>
        /// <param name="output">output matrix</param>
        /// <param name="expected">expected matrix</param>
        /// <param name="msg">test msg</param>
        public static void AssertMatrixEqual(Zen<IList<bool>>[] output, Zen<IList<bool>>[] expected, string msg="")
        {
            var n = output.Length;
            var m = expected.Length;
            Assert.AreEqual(n, m, "{0}: output has length {1} while expected has length {2}", n, m, msg);
            for (int i = 0; i < n; ++i)
                Assert.IsTrue(output[i].ToString().Equals(expected[i].ToString()), "{0}: pod{1} has wrong reachability", msg, i);
        }
    }
}
