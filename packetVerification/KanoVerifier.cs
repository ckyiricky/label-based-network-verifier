using System;
using System.Collections;
using System.Collections.Generic;
using ZenLib;
using static ZenLib.Language;

namespace LabelBasedNetworkVerification
{
    public static class Algorithms
    {
        // ns(hashcode) - pod map
        public static Zen<IDictionary<int, IList<bool>>> CreateNSMatrix(Zen<Pod>[] pods)
        {
            var n = pods.Length;
            // create original data 
            Dictionary<int, bool[]> originalData = new Dictionary<int, bool[]>();
            for (int i = 0; i < n; ++i)
            {
                var ns = pods[i].GetNS();
                var hash = ns.GetHashCode();
                if (!originalData.ContainsKey(hash)) originalData[hash] = new bool[n];
                originalData[hash][i] = true;
            }
            // create zen data
            Zen<IDictionary<int, IList<bool>>> nsMap = Language.EmptyDict<int, IList<bool>>();
            foreach (var kv in originalData)
            {
                nsMap = nsMap.Add(kv.Key, kv.Value);
            }
            return nsMap;
        }
        public static Zen<IDictionary<int, IList<bool>>> CreateNSLabelMatrix(Zen<Namespace>[] namespaces)
        {
            var n = namespaces.Length;
            Dictionary<int, bool[]> labelHash = new Dictionary<int, bool[]>();
            for (int i = 0; i < n; ++i)
            {
                var keys = namespaces[i].GetLabelKeys();
                var k = keys.Length();
                for (k-=1; !k.EqualToNumber(ushort.MaxValue); k-=1)
                {
                    var keyHash = keys.At(k).Value().GetHashCode();
                    if (!labelHash.ContainsKey(keyHash)) labelHash[keyHash] = new bool[n];
                    labelHash[keyHash][i] = true;
                }
            }
            // create zen data
            Zen<IDictionary<int, IList<bool>>> mx = Language.EmptyDict<int, IList<bool>>();
            foreach (var kv in labelHash)
            {
                mx = mx.Add(kv.Key, kv.Value);
            }
            return mx;
        }
        public static Zen<IDictionary<int, IList<bool>>> GetUserHash(Zen<Pod>[] pods, Zen<string> userKey)
        {
            var n = pods.Length;
            Dictionary<int, bool[]> originalData = new Dictionary<int, bool[]>();
            for (int i = 0; i < n; ++i)
            {
                var userValue = pods[i].GetLabels().Get(userKey).Value();
                Console.WriteLine("???{0}", userValue);
                var hash = userValue.GetHashCode();
                if (!originalData.ContainsKey(hash)) originalData[hash] = new bool[n];
                originalData[hash][i] = true;
            }
            // create zen data
            Zen<IDictionary<int, IList<bool>>> userHashMap = Language.EmptyDict<int, IList<bool>>();
            foreach (var kv in originalData)
            {
                Console.WriteLine("{0}:{1}", kv.Key, string.Join(".", kv.Value));
                userHashMap = userHashMap.Add(kv.Key, kv.Value);
            }
            return userHashMap;
        }
        public static Zen<IList<bool>>[] CreateReachMatrix(Zen<Pod>[] pods, Zen<Policy>[] policies, Zen<Namespace>[] namespaces=null)
        {
            var n = pods.Length;
            var m = policies.Length;
            /***********************ns filter*********************/
            Dictionary<int, BitArray> nsMatrix = new Dictionary<int, BitArray>();
            for (int i = 0; i < n; ++i)
            {
                var ns = pods[i].GetNS();
                var hash = ns.GetHashCode();
                if (!nsMatrix.ContainsKey(hash)) nsMatrix[hash] = new BitArray(n);
                nsMatrix[hash].Set(i, true);
            }
            var nsSize = namespaces.Length;
            Dictionary<int, BitArray> nsLabelHash = new Dictionary<int, BitArray>();
            for (int i = 0; i < nsSize; ++i)
            {
                var keys = namespaces[i].GetLabelKeys();
                var k = keys.Length();
                for (k-=1; !k.EqualToNumber(ushort.MaxValue); k-=1)
                {
                    var keyHash = keys.At(k).Value().GetHashCode();
                    if (!nsLabelHash.ContainsKey(keyHash)) nsLabelHash[keyHash] = new BitArray(nsSize);
                    nsLabelHash[keyHash].Set(i, true);
                }
            }
            /*****************************************************/
            Dictionary<int, BitArray> labelHash = new Dictionary<int, BitArray>();
            BitArray[] reachMatrix = new BitArray[n];
            for (int i = 0; i < n; ++i)
            {
                // initialize reachability matrix to within its own ns
                reachMatrix[i] = new BitArray(n, true);
                reachMatrix[i].And(nsMatrix[pods[i].GetNS().GetHashCode()]);

                var keys = pods[i].GetKeys();
                var k = keys.Length();
                for (k-=1; !k.EqualToNumber(ushort.MaxValue); k-=1)
                {
                    var keyHash = keys.At(k).Value().GetHashCode();
                    if (!labelHash.ContainsKey(keyHash)) labelHash[keyHash] = new BitArray(n);
                    labelHash[keyHash].Set(i, true);
                }
            }
            // if a pod has not been selected, it by default allows all traffic
            var podSelected = new bool[n];
            for (int i = 0; i < m; ++i)
            {
                var selectSet = new BitArray(n, true);
                // policy has a namespace while this namespace has no pods, this policy is void
                if (!nsMatrix.ContainsKey(policies[i].GetNS().GetHashCode())) continue;
                // else select pods only in the namespace
                selectSet.And(nsMatrix[policies[i].GetNS().GetHashCode()]);

                var keys = policies[i].GetSelectKeys();
                var k = keys.Length();
                for (k-=1; !k.EqualToNumber(ushort.MaxValue); k-=1)
                {
                    var keyHash = keys.At(k).Value().GetHashCode();
                    // if such label doesn't exist in pods, no pods are selected
                    if (!labelHash.ContainsKey(keyHash))
                    {
                        selectSet.SetAll(false);
                        break;
                    }
                    selectSet = selectSet.And(labelHash[keyHash]);
                }

                var allowNSSet = new BitArray(nsSize);
                var allowSet = new BitArray(n);
                {
                    // get all namespaces match ns allowed label key
                    var allowNSKeys = policies[i].GetAllowNSKeys();
                    var allowNSLabels = policies[i].GetAllowNS();
                    k = allowNSKeys.Length();
                    // if no allow ns labels in the policy, the namespace of the policy will be used as ns scope
                    if (policies[i].GetDenyAll().Equals(True())) { }
                    else if (k.EqualToNumber(0)) allowSet.Or(nsMatrix[policies[i].GetNS().GetHashCode()]);
                    else
                    {
                        for (k -= 1; !k.EqualToNumber(ushort.MaxValue); k -= 1)
                        {
                            var keyHash = allowNSKeys.At(k).Value().GetHashCode();
                            if (!nsLabelHash.ContainsKey(keyHash))
                            {
                                allowNSSet.SetAll(false);
                                break;
                            }
                            allowNSSet.Or(nsLabelHash[keyHash]);
                        }
                        for (int j = 0; j < nsSize; ++j)
                        {
                            if (allowNSSet.Get(j))
                            {
                                var nsLabels = namespaces[j].GetLabels();
                                k = allowNSKeys.Length();
                                for (k -= 1; !k.EqualToNumber(ushort.MaxValue); k -= 1)
                                {
                                    var key = allowNSKeys.At(k).Value();
                                    // if label value is not matched, this ns is not allowed
                                    if (!allowNSLabels.Get(key).Value().GetHashCode().Equals(nsLabels.Get(key).Value().GetHashCode()))
                                    {
                                        allowNSSet.Set(j, false);
                                        break;
                                    }
                                }
                            }
                            if (allowNSSet.Get(j))
                            {
                                allowSet.Or(nsMatrix[namespaces[j].GetName().GetHashCode()]);
                            }
                        }
                    }
                }

                var labels = policies[i].GetAllowLabels();
                keys = policies[i].GetAllowKeys();
                k = keys.Length();
                for (k-=1; !k.EqualToNumber(ushort.MaxValue); k-=1)
                {
                    var keyHash = keys.At(k).Value().GetHashCode();
                    if (!labelHash.ContainsKey(keyHash))
                    {
                        allowSet.SetAll(false);
                        break;
                    }
                    allowSet = allowSet.And(labelHash[keyHash]);
                }
                for (int j = 0; j < n; ++j)
                {
                    if (allowSet.Get(j))
                    {
                        k = keys.Length();
                        var podLabels = pods[j].GetLabels();
                        for (k -= 1; !k.EqualToNumber(ushort.MaxValue); k -= 1)
                        {
                            var key = keys.At(k).Value();
                            var value = labels.Get(key).Value();
                            if (!(podLabels.Get(key).Value().GetHashCode() == value.GetHashCode()))
                            {
                                allowSet.Set(j, false);
                                break;
                            }
                        }
                    }
                }

                labels = policies[i].GetSelectLabels();
                keys = policies[i].GetSelectKeys();
                for (int j = 0; j < n; ++j)
                {
                    if (selectSet.Get(j))
                    {
                        k = keys.Length();
                        var podLabels = pods[j].GetLabels();
                        for (k -= 1; !k.EqualToNumber(ushort.MaxValue); k -= 1)
                        {
                            var key = keys.At(k).Value();
                            var value = labels.Get(key).Value();
                            if (!(podLabels.Get(key).Value().GetHashCode() == value.GetHashCode()))
                            {
                                selectSet.Set(j, false);
                                break;
                            }
                        }
                    }

                    if (selectSet.Get(j))
                    {
                        // if this pod has not been selected before, set all its reach bits to 0
                        if (!podSelected[j])
                        {
                            podSelected[j] = true;
                            reachMatrix[j].SetAll(false);
                            // intra-pod traffic is always allowed
                            reachMatrix[j].Set(j, true);
                        }
                        reachMatrix[j] = reachMatrix[j].Or(allowSet);
                    }
                }
            }
            Zen<IList<bool>>[] mx = new Zen<IList<bool>>[n];
            for (int i = 0; i < n; ++i)
            {
                var tmp = new bool[n];
                reachMatrix[i].CopyTo(tmp, 0);
                mx[i] = tmp;
            }
            return mx;
        }
    }
    public static class Verifier
    {
        public static Zen<IList<ushort>> AllReachableCheck(Zen<IList<bool>>[] matrix, Zen<bool> reach)
        {
            int n = matrix.Length;
            Zen<IList<ushort>> podList = EmptyList<ushort>();
            for (var i = 0; i < n; ++i)
            {
                //empty = empty.AddBack(If<ushort>(matrix[i].All(r => r), (ushort)i, ushort.MaxValue));
                var c = If<ushort>(matrix[i].All(r => r.Equals(True())), (ushort)i, ushort.MaxValue);
                if (c.EqualToNumber((ushort)i)) podList = podList.AddBack(c);
            }
            return podList;

        }
        // matrix is ingress as row and egress as column
        public static Zen<IList<ushort>> UserCrossCheck(
            Zen<IList<bool>>[] matrix, 
            Zen<IDictionary<int, IList<bool>>> userHashmap, 
            Zen<Pod>[] pods, 
            Zen<string> userKey
            )
        {
            var n = matrix.Length;
            Zen<IList<ushort>> podList = EmptyList<ushort>();
            for (int i = 0; i < n; ++i)
            {
                var userValue = pods[i].GetLabels().Get(userKey).Value();
                var veriSet = userHashmap.Get(userValue.GetHashCode()).Value();
                veriSet = veriSet.Not();
                veriSet = veriSet.And(matrix[i]);
                var c = If<ushort>(veriSet.All(r => r.Equals(False())), (ushort)i, ushort.MaxValue);
                // if this pod can only be reached from same user's pods, add it to list
                if (c.EqualToNumber((ushort)i)) podList = podList.AddBack(c);
            }
            return podList;
        }
        // matrix is egress as row and ingress as column
        public static Zen<IList<ushort>> SystemIsolationCheck(
            Zen<IList<bool>>[] matrix,
            Zen<ushort> index)
        {
            var n = matrix.Length;
            var ind = ushort.Parse(index.ToString());
            var veriSet = matrix[ind].Not();
            Zen<IList<ushort>> cList = EmptyList<ushort>();
            for (int i = 0; i < n; ++i)
            {
                var c = If<ushort>(veriSet.At((ushort)i).Value().Equals(True()), (ushort)i, ushort.MaxValue);
                if (c.EqualToNumber((ushort)i)) cList = cList.AddBack(c);
            }
            return cList;
        }
    }
}
