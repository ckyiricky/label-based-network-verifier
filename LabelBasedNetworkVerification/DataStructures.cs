using System;
using System.Collections.Generic;
using ZenLib;
using static ZenLib.Language;

namespace LabelBasedNetworkVerification
{
    /// <summary>
    /// Pod in k8s clusters.
    /// </summary>
    public class Pod
    {
        /// <summary>
        /// Namespace of the pod.
        /// </summary>
        public string Namespace;
        /// <summary>
        /// Labels of the pod.
        /// </summary>
        public IDictionary<string, string> Labels;
        /// <summary>
        /// Keys of labels, use list to tranverse keys with ZenLib.
        /// </summary>
        public IList<string> LabelKeys;
        /// <summary>
        /// Create a Zen Pod from namespace, labels and label keys.
        /// </summary>
        /// <param name="ns">name of the namespace.</param>
        /// <param name="labels">labels of the pod.</param>
        /// <param name="keys">keys of the labels.</param>
        /// <returns>The Zen pod.</returns>
        public static Zen<Pod> Create(Zen<string> ns, Zen<IDictionary<string, string>> labels, Zen<IList<string>> keys)
        {
            return Language.Create<Pod>(("Namespace", ns), ("Labels", labels), ("LabelKeys", keys));
        }
        /// <summary>
        /// Convert pod to a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}.{1}", Namespace, string.Join(".", Labels));
        }
    }
    /// <summary>
    /// Pod Zen Extension methods.
    /// </summary>
    public static class PodExtensions
    {
        /// <summary>
        /// Get labels of the pod.
        /// </summary>
        /// <param name="pod">pod.</param>
        /// <returns>labels.</returns>
        public static Zen<IDictionary<string, string>> GetLabels(this Zen<Pod> pod) => pod.GetField<Pod, IDictionary<string, string>>("Labels");
        /// <summary>
        /// Get keys of labels of the pod.
        /// </summary>
        /// <param name="pod">pod.</param>
        /// <returns>list of label keys.</returns>
        public static Zen<IList<string>> GetKeys(this Zen<Pod> pod) => pod.GetField<Pod, IList<string>>("LabelKeys");
        /// <summary>
        /// Get namespace of the pod.
        /// </summary>
        /// <param name="pod">pod.</param>
        /// <returns>name of namespace.</returns>
        public static Zen<string> GetNS(this Zen<Pod> pod) => pod.GetField<Pod, string>("Namespace");
    }
    /// <summary>
    /// Network policy in k8s
    /// 1. Multi-rules k8s policy should be split into multiple policies.
    /// 2. Empty allowNS and allowLabel => Allow all traffic.
    /// </summary>
    public class Policy
    {
        /// <summary>
        /// Policy type (ingress or egress).
        /// </summary>
        public bool Ingress;
        /// <summary>
        /// Deny all traffic flag, if this flag is set, allow selectors will be omitted (allowNS and allowLabels).
        /// </summary>
        public bool DenyAll;
        /// <summary>
        /// Namespace of the policy.
        /// </summary>
        public string Namespace;
        /// <summary>
        /// Pod selector labels.
        /// </summary>
        public IDictionary<string, string> SelectLabels;
        /// <summary>
        /// Namespace selector of allowed pods.
        /// </summary>
        public IDictionary<string, string> AllowNamespaces;
        /// <summary>
        /// Pod selector of allowed pods
        /// Selected pods = selected labels in selected ns.
        /// </summary>
        public IDictionary<string, string> AllowLabels;
        /// <summary>
        /// Keys of labels of pod selector.
        /// </summary>
        public IList<string> SelectKeys;
        /// <summary>
        /// Keys of labels of allowed namespaces.
        /// </summary>
        public IList<string> AllowNSKeys;
        /// <summary>
        /// Keys of labels of allowed pods.
        /// </summary>
        public IList<string> AllowKeys;
        /// <summary>
        /// Create a Zen Policy
        /// Default constructor: ingress == true and denyAll == false.
        /// </summary>
        /// <param name="ns">namespace name of the policy.</param>
        /// <param name="select">selected labels.</param>
        /// <param name="allowLb">labels of allowed pods.</param>
        /// <param name="allowNs">labels of allowed namespace.</param>
        /// <param name="selectKeys">keys of labels of pod selector.</param>
        /// <param name="allowKeys">keys of labels of allowed pods.</param>
        /// <param name="allowNSKeys">keys of labels of allowed namespace.</param>
        /// <returns>Zen Policy.</returns>
        public static Zen<Policy> Create(Zen<string> ns, Zen<IDictionary<string, string>> select,
            Zen<IDictionary<string, string>> allowLb,
            Zen<IDictionary<string, string>> allowNs,
            Zen<IList<string>> selectKeys,
            Zen<IList<string>> allowKeys,
            Zen<IList<string>> allowNSKeys)
        {
            return Language.Create<Policy>(("Namespace", ns), ("SelectLabels", select),
                ("AllowLabels", allowLb), ("AllowNamespaces", allowNs),
                ("SelectKeys", selectKeys), ("AllowKeys", allowKeys),
                ("AllowNSKeys", allowNSKeys), ("Ingress", True()), ("DenyAll", False()));
        }
        /// <summary>
        /// Create a Zen Policy.
        /// </summary>
        /// <param name="ns">namespace name of the policy.</param>
        /// <param name="select">selected labels.</param>
        /// <param name="allowLb">labels of allowed pods.</param>
        /// <param name="allowNs">labels of allowed namespace.</param>
        /// <param name="selectKeys">keys of labels of pod selector.</param>
        /// <param name="allowKeys">keys of labels of allowed pods.</param>
        /// <param name="allowNSKeys">keys of labels of allowed namespace.</param>
        /// <param name="ingress">policy type.</param>
        /// <param name="denyAll">deny all flag.</param>
        /// <returns>Zen Policy.</returns>
        public static Zen<Policy> Create(Zen<string> ns, Zen<IDictionary<string, string>> select,
            Zen<IDictionary<string, string>> allowLb,
            Zen<IDictionary<string, string>> allowNs,
            Zen<IList<string>> selectKeys,
            Zen<IList<string>> allowKeys,
            Zen<IList<string>> allowNSKeys,
            Zen<bool> ingress,
            Zen<bool> denyAll)
        {
            return Language.Create<Policy>(("Namespace", ns), ("SelectLabels", select),
                ("AllowLabels", allowLb), ("AllowNamespaces", allowNs),
                ("SelectKeys", selectKeys), ("AllowKeys", allowKeys),
                ("AllowNSKeys", allowNSKeys), ("Ingress", ingress), ("DenyAll", denyAll));
        }
    }
    /// <summary>
    /// Policy Zen extension methods.
    /// </summary>
    public static class PolicyExtensions
    {
        /// <summary>
        /// Get namespace of the policy.
        /// </summary>
        /// <param name="policy">policy.</param>
        /// <returns>name of the namespace.</returns>
        public static Zen<string> GetNS(this Zen<Policy> policy) => policy.GetField<Policy, string>("Namespace");
        /// <summary>
        /// Get keys of pod selector.
        /// </summary>
        /// <param name="policy">policy.</param>
        /// <returns>keys of labels of pod selector.</returns>
        public static Zen<IList<string>> GetSelectKeys(this Zen<Policy> policy) => policy.GetField<Policy, IList<string>>("SelectKeys");
        /// <summary>
        /// Get keys of allowed pods.
        /// </summary>
        /// <param name="policy">policy.</param>
        /// <returns>keys of labels of allowed pods.</returns>
        public static Zen<IList<string>> GetAllowKeys(this Zen<Policy> policy) => policy.GetField<Policy, IList<string>>("AllowKeys");
        /// <summary>
        /// Get keys of allowed namespaces.
        /// </summary>
        /// <param name="policy">policy.</param>
        /// <returns>keys of labels of allowed namespaces.</returns>
        public static Zen<IList<string>> GetAllowNSKeys(this Zen<Policy> policy) => policy.GetField<Policy, IList<string>>("AllowNSKeys");
        /// <summary>
        /// Get labels of pod selector.
        /// </summary>
        /// <param name="policy">policy.</param>
        /// <returns>labels of pod selector.</returns>
        public static Zen<IDictionary<string, string>> GetSelectLabels(this Zen<Policy> policy) => policy.GetField<Policy, IDictionary<string, string>>("SelectLabels");
        /// <summary>
        /// Get labels of allowed pods.
        /// </summary>
        /// <param name="policy">policy.</param>
        /// <returns>labels of allowed pods.</returns>
        public static Zen<IDictionary<string, string>> GetAllowLabels(this Zen<Policy> policy) => policy.GetField<Policy, IDictionary<string, string>>("AllowLabels");
        /// <summary>
        /// Get labels of allowed namespaces.
        /// </summary>
        /// <param name="policy">policy.</param>
        /// <returns>labels of allowed namespaces.</returns>
        public static Zen<IDictionary<string, string>> GetAllowNS(this Zen<Policy> policy) => policy.GetField<Policy, IDictionary<string, string>>("AllowNamespaces");
        /// <summary>
        /// Get policy type.
        /// </summary>
        /// <param name="policy">policy.</param>
        /// <returns>if it is an ingress policy.</returns>
        public static Zen<bool> GetIngress(this Zen<Policy> policy) => policy.GetField<Policy, bool>("Ingress");
        /// <summary>
        /// Get deny all flag.
        /// </summary>
        /// <param name="policy">policy.</param>
        /// <returns>if it is a deny-all policy.</returns>
        public static Zen<bool> GetDenyAll(this Zen<Policy> policy) => policy.GetField<Policy, bool>("DenyAll");
        /// <summary>
        /// Get if this policy allows all traffic.
        /// </summary>
        /// <param name="policy">policy.</param>
        /// <returns>if it is an allow-all policy.</returns>
        public static bool GetAllowAll(this Zen<Policy> policy)
        {
            var labelKeys = policy.GetAllowKeys().Length();
            var nsKeys = policy.GetAllowNSKeys().Length();
            var denyAll = policy.GetDenyAll();
            return (labelKeys.EqualToNumber(0) && nsKeys.EqualToNumber(0) && denyAll.Equals(False()));
        }
    }
    /// <summary>
    /// Namespace in k8s.
    /// </summary>
    public class Namespace
    {
        /// <summary>
        /// Name of the namespace.
        /// </summary>
        public string Name;
        /// <summary>
        /// Labels of the namespace.
        /// </summary>
        public IDictionary<string, string> Labels;
        /// <summary>
        /// Keys of labels of the namespace.
        /// </summary>
        public IList<string> LabelKeys;
        /// <summary>
        /// Create a Zen Namespace.
        /// </summary>
        /// <param name="name">name of the namespace.</param>
        /// <param name="labels">labels of the namespace.</param>
        /// <param name="labelKeys">keys of labels of the namespace.</param>
        /// <returns>Zen Namespace.</returns>
        public static Zen<Namespace> Create(Zen<string> name, Zen<IDictionary<string, string>> labels, Zen<IList<string>> labelKeys)
        {
            return Language.Create<Namespace>(("Name", name), ("Labels", labels), ("LabelKeys", labelKeys));
        }
    }
    /// <summary>
    /// Namespace Zen extension methods.
    /// </summary>
    public static class NamespaceExtensions
    {
        /// <summary>
        /// Get name of the namesapce.
        /// </summary>
        /// <param name="ns">namespace.</param>
        /// <returns>name of the ns.</returns>
        public static Zen<string> GetName(this Zen<Namespace> ns) => ns.GetField<Namespace, string>("Name");
        /// <summary>
        /// Get labels of the namespace.
        /// </summary>
        /// <param name="ns">namespace.</param>
        /// <returns>labels.</returns>
        public static Zen<IDictionary<string, string>> GetLabels(this Zen<Namespace> ns) => ns.GetField<Namespace, IDictionary<string, string>>("Labels");
        /// <summary>
        /// Get keys of labels of the namespace.
        /// </summary>
        /// <param name="ns">namespace.</param>
        /// <returns>keys of labels of ns.</returns>
        public static Zen<IList<string>> GetLabelKeys(this Zen<Namespace> ns) => ns.GetField<Namespace, IList<string>>("LabelKeys");
    }
}
