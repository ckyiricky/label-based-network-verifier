﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ZenLib;
using static ZenLib.Language;

namespace LabelBasedNetworkVerification
{
    /// <summary>
    /// Algorithms to create Kano matrix.
    /// </summary>
    public static class Algorithms
    {
        /// <summary>
        /// Create namespace-pod mapping
        ///     hash of namespace as key, 'bitvector' of pods as value.
        /// </summary>
        /// <param name="pods">all pods.</param>
        /// <returns>namespace-pod mapping.</returns>
        public static Zen<IDictionary<int, IList<bool>>> CreateNSMatrix(Zen<Pod>[] pods)
        {
            var n = pods.Length;
            // create original data.
            Dictionary<int, bool[]> originalData = new Dictionary<int, bool[]>();
            for (int i = 0; i < n; ++i)
            {
                var ns = pods[i].GetNS();
                var hash = ns.GetHashCode();
                if (!originalData.ContainsKey(hash))
                    originalData[hash] = new bool[n];
                originalData[hash][i] = true;
            }
            // create zen data.
            Zen<IDictionary<int, IList<bool>>> nsMap = Language.EmptyDict<int, IList<bool>>();
            foreach (var kv in originalData)
            {
                nsMap = nsMap.Add(kv.Key, kv.Value);
            }
            return nsMap;
        }
        /// <summary>
        /// Create label-ns mapping
        ///     hash of label as key, 'bitvector' of ns as value.
        /// </summary>
        /// <param name="namespaces">all namespaces.</param>
        /// <returns>label-ns mapping.</returns>
        public static Zen<IDictionary<int, IList<bool>>> CreateNSLabelMatrix(Zen<Namespace>[] namespaces)
        {
            var n = namespaces.Length;
            Dictionary<int, bool[]> labelHash = new Dictionary<int, bool[]>();
            for (int i = 0; i < n; ++i)
            {
                var keys = namespaces[i].GetLabelKeys();
                var k = keys.Length();
                // iterate over Zen<List> to traverse all keys
                for (k -= 1; !k.EqualToNumber(ushort.MaxValue); k -= 1)
                {
                    var keyHash = keys.At(k).Value().GetHashCode();
                    if (!labelHash.ContainsKey(keyHash))
                        labelHash[keyHash] = new bool[n];
                    labelHash[keyHash][i] = true;
                }
            }
            // create zen data.
            Zen<IDictionary<int, IList<bool>>> mx = Language.EmptyDict<int, IList<bool>>();
            foreach (var kv in labelHash)
            {
                mx = mx.Add(kv.Key, kv.Value);
            }
            return mx;
        }
        /// <summary>
        /// Create user-pod mapping
        ///     hash of user as key, 'bitvector' of pods as value
        ///     assuming all pods have the user label.
        /// </summary>
        /// <param name="pods">all pods.</param>
        /// <param name="userKey">key of 'user label'.</param>
        /// <returns>user-pod mapping.</returns>
        public static Zen<IDictionary<int, IList<bool>>> GetUserHash(Zen<Pod>[] pods, Zen<string> userKey)
        {
            var n = pods.Length;
            Dictionary<int, bool[]> originalData = new Dictionary<int, bool[]>();
            for (int i = 0; i < n; ++i)
            {
                var labelValue = pods[i].LabelValue(userKey);
                if (labelValue == null)
                    continue;
                var hash = labelValue.GetHashCode();
                if (!originalData.ContainsKey(hash))
                    originalData[hash] = new bool[n];
                originalData[hash][i] = true;
            }
            // create zen data.
            Zen<IDictionary<int, IList<bool>>> userHashMap = EmptyDict<int, IList<bool>>();
            foreach (var kv in originalData)
            {
                userHashMap = userHashMap.Add(kv.Key, kv.Value);
            }
            return userHashMap;
        }
        /// <summary>
        /// Create reachability matrix of all pods
        ///     egressMatrix takes egress pods as row and ingress pods as column
        ///     ingressMatrix takes ingress pods as row and egress pods as column
        ///     egressMatrix is the transpose of ingressMatrix
        ///
        /// PodA can send traffic to podB if and only if.
        /// podA is allowed to send traffic to podB and podB is allowed to receive traffic from podA.
        /// </summary>
        /// <param name="pods">all pods.</param>
        /// <param name="policies">all policies.</param>
        /// <param name="namespaces">all namespaces.</param>
        /// <returns>two reachability matrices and three BCP matrices.</returns>
        public static (Zen<IList<bool>>[] egressMatrix, Zen<IList<bool>>[] ingressMatrix, Zen<IList<bool>>[] podPolMatrix, Zen<IList<bool>>[] polPodAllowedMatrix, Zen<IList<bool>>[] polPodSelectedMatrix) CreateReachMatrix(Zen<Pod>[] pods, Zen<Policy>[] policies, Zen<Namespace>[] namespaces)
        {
            var n = pods.Length;
            var m = policies.Length;
            // BCP
            var selectedPolicies = new bool[n, m];
            var allowedPods = new bool[m, n];
            var selectedPods = new bool[m, n];
            /***********************ns filter*********************/
            // Create ns-pod mapping
            Dictionary<int, BitArray> nsMatrix = new Dictionary<int, BitArray>();
            for (int i = 0; i < n; ++i)
            {
                var ns = pods[i].GetNS();
                var hash = ns.GetHashCode();
                if (!nsMatrix.ContainsKey(hash))
                    nsMatrix[hash] = new BitArray(n);
                nsMatrix[hash].Set(i, true);
            }
            // Create label-ns mapping
            var nsSize = namespaces.Length;
            Dictionary<int, BitArray> nsLabelHash = new Dictionary<int, BitArray>();
            for (int i = 0; i < nsSize; ++i)
            {
                var keys = namespaces[i].GetLabelKeys();
                var k = keys.Length();
                // iterate over Zen<List> to traverse all labels
                for (k -= 1; !k.EqualToNumber(ushort.MaxValue); k -= 1)
                {
                    var keyHash = keys.At(k).Value().GetHashCode();
                    if (!nsLabelHash.ContainsKey(keyHash))
                        nsLabelHash[keyHash] = new BitArray(nsSize);
                    nsLabelHash[keyHash].Set(i, true);
                }
            }
            /*****************************************************/

            // Create label-pod mapping
            Dictionary<int, BitArray> labelHash = new Dictionary<int, BitArray>();
            // ingress policy matrix
            BitArray[] ingressReachMatrix = new BitArray[n];
            // egress policy matrix
            BitArray[] egressReachMatrix = new BitArray[n];
            for (int i = 0; i < n; ++i)
            {
                // initialize reachability matrix to allow all traffic (default behavior)
                ingressReachMatrix[i] = new BitArray(n, true);
                egressReachMatrix[i] = new BitArray(n, true);

                var keys = pods[i].GetKeys();
                var k = keys.Length();
                for (k -= 1; !k.EqualToNumber(ushort.MaxValue); k -= 1)
                {
                    var keyHash = keys.At(k).Value().GetHashCode();
                    if (!labelHash.ContainsKey(keyHash))
                        labelHash[keyHash] = new BitArray(n);
                    labelHash[keyHash].Set(i, true);
                }
            }
            // record which pods have been selected
            // if a pod has not been selected, it by default allows all traffic
            var podSelected = new bool[n];
            // Traverse all policies to create reachability matrix
            for (int i = 0; i < m; ++i)
            {
                // bitvector of selected pods (keys of labels only)
                var selectSet = new BitArray(n, true);
                // if policy is in a namespace which has no pods, this policy is void (no pods can be selected)
                if (!nsMatrix.ContainsKey(policies[i].GetNS().GetHashCode()))
                    continue;
                // else select pods only in the namespace of the policy
                selectSet.And(nsMatrix[policies[i].GetNS().GetHashCode()]);

                var keys = policies[i].GetSelectKeys();
                var k = keys.Length();
                for (k -= 1; !k.EqualToNumber(ushort.MaxValue); k -= 1)
                {
                    var keyHash = keys.At(k).Value().GetHashCode();
                    // if a label(key) doesn't exist in all pods, no pods are selected
                    if (!labelHash.ContainsKey(keyHash))
                    {
                        selectSet.SetAll(false);
                        break;
                    }
                    // else intersect with pods under the key
                    selectSet = selectSet.And(labelHash[keyHash]);
                }

                // bitvector of allowed ns
                var allowNSSet = new BitArray(nsSize);
                // bitvector of allowed pods
                var allowSet = new BitArray(n);
                {
                    // get all namespaces that match label key of allowed ns
                    var allowNSKeys = policies[i].GetAllowNSKeys();
                    var allowNSLabels = policies[i].GetAllowNS();
                    k = allowNSKeys.Length();
                    // if this policy is deny all, no pods are allowed to/from selected pods
                    if (policies[i].GetDenyAll().Equals(True()))
                        allowSet.SetAll(false);
                    // if this policy is allow all, all pods are allowed to/from selected pods
                    else if (policies[i].GetAllowAll())
                        allowSet.SetAll(true);
                    // if no labels of allowed ns is defined in the policy,
                    // namespace of the policy will be used as allowed ns
                    else if (k.EqualToNumber(0))
                        allowSet.Or(nsMatrix[policies[i].GetNS().GetHashCode()]);
                    // else tranverse all labels of allowed ns
                    else
                    {
                        // Get ns matchs all keys of allowed labels
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
                        // Check if selected namespaces have same label values as required in policy
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
                            // allowed pods can be only in the allowed ns
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
                // tranverse labels(key) of allowed pods
                // intersect pods under required labels(key)
                for (k -= 1; !k.EqualToNumber(ushort.MaxValue); k -= 1)
                {
                    var keyHash = keys.At(k).Value().GetHashCode();
                    if (!labelHash.ContainsKey(keyHash))
                    {
                        allowSet.SetAll(false);
                        break;
                    }
                    allowSet = allowSet.And(labelHash[keyHash]);
                }
                // check if pods have the same label values as required
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
                            if (!podLabels.Get(key).Value().StringValue().Equals(value.StringValue()))
                            {
                                allowSet.Set(j, false);
                                break;
                            }
                        }
                    }
                    // update BCP policy-pod mapping
                    if (allowSet.Get(j))
                        allowedPods[i, j] = true;
                }

                labels = policies[i].GetSelectLabels();
                keys = policies[i].GetSelectKeys();
                var reachMatrix = ingressReachMatrix;
                if (!policies[i].GetIngress().Equals(True()))
                    reachMatrix = egressReachMatrix;
                // tranverse labels(key) of pod selector
                // intersect pods under required labels(key)
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
                            if (!podLabels.Get(key).Value().StringValue().Equals(value.StringValue()))
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
                            ingressReachMatrix[j].SetAll(false);
                            egressReachMatrix[j].SetAll(false);
                            // intra-pod traffic is always allowed
                            ingressReachMatrix[j].Set(j, true);
                            egressReachMatrix[j].Set(j, true);
                        }
                        // update BCP selected mapping
                        selectedPolicies[j, i] = true;
                        selectedPods[i, j] = true;
                        // allow pods to/from selected pod
                        reachMatrix[j] = reachMatrix[j].Or(allowSet);
                    }
                }
            }
            // only ingress policy allows podA to receive traffic from podB
            // and egress policy allows podB to send traffic to podA
            // podA can receive traffic from podB
            ingressReachMatrix.Transpose();
            for (int i = 0; i < n; ++i)
                egressReachMatrix[i].And(ingressReachMatrix[i]);

            // ingress matrix is transpose of egress matrix
            ingressReachMatrix = egressReachMatrix.Select(v => (BitArray)v.Clone()).ToArray();
            ingressReachMatrix.Transpose();
            // create Zen data:
            // 1. egress matrix
            // 2. ingress matrix
            // 3. BCP pod-policy mapping
            // 4. BCP policy-pod allowed mapping
            // 5. BCP policy-pod selected mapping
            Zen<IList<bool>>[] eMx = new Zen<IList<bool>>[n];
            Zen<IList<bool>>[] inMx = new Zen<IList<bool>>[n];
            Zen<IList<bool>>[] podPolMx = new Zen<IList<bool>>[n];
            Zen<IList<bool>>[] polPodAllowedMx = new Zen<IList<bool>>[m];
            Zen<IList<bool>>[] polPodSelectedMx = new Zen<IList<bool>>[m];
            for (int i = 0; i < n; ++i)
            {
                var tmp = new bool[n];
                egressReachMatrix[i].CopyTo(tmp, 0);
                eMx[i] = tmp;
                ingressReachMatrix[i].CopyTo(tmp, 0);
                inMx[i] = tmp;
                // BCP pod-policy mapping
                podPolMx[i] = Enumerable.Range(0, m).Select(x => selectedPolicies[i, x]).ToArray();
            }
            for (int i = 0; i < m; ++i)
            {
                // BCP policy-pod mapping
                polPodAllowedMx[i] = Enumerable.Range(0, n).Select(x => allowedPods[i, x]).ToArray();
                polPodSelectedMx[i] = Enumerable.Range(0, n).Select(x => selectedPods[i, x]).ToArray();
            }
            return (eMx, inMx, podPolMx, polPodAllowedMx, polPodSelectedMx);
        }
    }
    /// <summary>
    /// Violation checkers.
    /// </summary>
    public static class Verifier
    {
        /// <summary>
        /// Find pods are all reachable/isolated.
        /// </summary>
        /// <param name="matrix">ingress reachability matrix.</param>
        /// <param name="reach">reachable or isolated flag.</param>
        /// <returns>list of reachable/isolated pods index.</returns>
        public static Zen<IList<ushort>> AllReachableCheck(Zen<IList<bool>>[] matrix, Zen<bool> reach)
        {
            int n = matrix.Length;
            Zen<IList<ushort>> podList = EmptyList<ushort>();
            for (var i = 0; i < n; ++i)
            {
                var c = If(reach, If<ushort>(matrix[i].All(r => r.Equals(reach)), (ushort)i, ushort.MaxValue), IsolatedCheckHelper(matrix[i], (ushort)i, 1));
                if (c.EqualToNumber((ushort)i))
                    podList = podList.AddBack(c);
            }
            return podList;
        }
        static Zen<ushort> IsolatedCheckHelper(Zen<IList<bool>> list, Zen<ushort> index, Zen<ushort> quota)
        {
            return list.Case(
                empty: index,
                cons: (hd, tl) => If(Not(hd), IsolatedCheckHelper(tl, index, quota), If(quota > 0, IsolatedCheckHelper(tl, index, quota - 1), ushort.MaxValue)));
        }
        /// <summary>
        /// Find pods can only be reached from pods of same user.
        /// </summary>
        /// <param name="matrix">ingress reachability matrix.</param>
        /// <param name="userHashmap">user-pod mapping.</param>
        /// <param name="pods">all pods.</param>
        /// <param name="userKey">key of user label.</param>
        /// <returns>list of pods can only be reached from pods of same user.</returns>
        public static Zen<IList<ushort>> UserCrossCheck(
            Zen<IList<bool>>[] matrix,
            Zen<IDictionary<int, IList<bool>>> userHashmap,
            Zen<Pod>[] pods,
            Zen<string> userKey)
        {
            var n = pods.Length;
            Zen<IList<ushort>> podList = EmptyList<ushort>();
            for (int i = 0; i < n; ++i)
            {
                var userString = pods[i].LabelValue(userKey);
                if (userString == null)
                    continue;
                var veriSet = userHashmap.Get(userString.GetHashCode()).Value();
                veriSet = veriSet.Not().And(matrix[i]);
                var c = If<ushort>(veriSet.All(r => r.Equals(False())), (ushort)i, ushort.MaxValue);
                // if this pod can only be reached from same user's pods, add it to list
                if (!c.EqualToNumber(ushort.MaxValue))
                    podList = podList.AddBack(c);
            }
            return podList;
        }
        /// <summary>
        /// Find isolated pods to pod index.
        /// </summary>
        /// <param name="matrix">egress reachability matrix.</param>
        /// <param name="index">pod is being checked.</param>
        /// <returns>list of pods cant be reached from pod index.</returns>
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
                if (c.EqualToNumber((ushort)i))
                    cList = cList.AddBack(c);
            }
            return cList;
        }
        /// <summary>
        /// Find shadowed policies.
        /// </summary>
        /// <param name="selectedPolicies">pods-policy mapping, policies that select this pod will be set.</param>
        /// <param name="podsAllowed">policy-pod mapping, pods that allowed by this policy will be set.</param>
        /// <param name="podsSelected">policy-pod mapping, pods that selected by this policy will be set.</param>
        /// <returns>policy pairs that the first policy shadows the second one.</returns>
        public static Zen<IList<Tuple<int, int>>> PolicyShadowCheck(Zen<IList<bool>>[] selectedPolicies, Zen<IList<bool>>[] podsAllowed, Zen<IList<bool>>[] podsSelected)
        {
            var n = selectedPolicies.Length;
            var m = podsAllowed.Length;
            var shadowPairs = EmptyList<Tuple<int, int>>();
            // cache checked pairs to avoid repeated detection
            bool[,] checkedPairs = new bool[m, m];
            for (var i = 0; i < n; ++i)
            {
                var polList = selectedPolicies[i];
                for (var j = 0; j < m; ++j)
                {
                    if (polList.At((ushort)j).Value().Equals(False()))
                        continue;
                    for (var k = 0; k < m; ++k)
                    {
                        if (checkedPairs[j, k])
                            continue;
                        if (j == k)
                            continue;
                        if (polList.At((ushort)k).Value().Equals(False()))
                            continue;
                        checkedPairs[j, k] = true;
                        var setA = podsAllowed[j];
                        var setB = podsSelected[j];
                        setA = setA.And(podsAllowed[k]);
                        setB = setB.And(podsSelected[k]);
                        // if m&&k == k, set of m covers set of k
                        if (setA.ToString().Equals(podsAllowed[k].ToString()) &&
                            setB.ToString().Equals(podsSelected[k].ToString()))
                        {
                            shadowPairs = shadowPairs.AddBack(Tuple<int, int>(j, k));
                        }
                    }
                }
            }
            return shadowPairs;
        }
    }
}
