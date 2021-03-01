using System;
using System.Collections.Generic;
using ZenLib;
using static ZenLib.Language;

namespace LabelBasedNetworkVerification
{
    public class Pod
    {
        public string Namespace;
        // use two lists to store labels (key list and value list)
        public IDictionary<string, string> Labels;
        public IList<string> LabelKeys;
        public static Zen<Pod> Create(Zen<string> ns, Zen<IDictionary<string, string>> labels, Zen<IList<string>> keys)
        {
            return Language.Create<Pod>(("Namespace", ns), ("Labels", labels), ("LabelKeys", keys));
        }
        
        public override string ToString()
        {
            return string.Format("{0}.{1}", Namespace, string.Join(".", Labels));
        }
    }
    public static class PodExtensions
    {
        public static Zen<IDictionary<string, string>> GetLabels(this Zen<Pod> pod) => pod.GetField<Pod, IDictionary<string, string>>("Labels");
        public static Zen<IList<string>> GetKeys(this Zen<Pod> pod) => pod.GetField<Pod, IList<string>>("LabelKeys");
        public static Zen<string> GetNS(this Zen<Pod> pod) => pod.GetField<Pod, string>("Namespace");
    }
    public class Policy
    {
        public bool Ingress = true;
        // Corner case for all deny policy
        public bool DenyAll = false;
        public string Namespace;
        // Empty dictionary means all selected
        public IDictionary<string, string> SelectLabels;
        public IDictionary<string, string> AllowLabels;
        public IDictionary<string, string> AllowNamespaces;
        public IList<string> SelectKeys;
        public IList<string> AllowKeys;
        public IList<string> AllowNSKeys;
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
        public static Zen<Policy> Create(Zen<string> ns, Zen<IDictionary<string, string>> select, 
            Zen<IDictionary<string, string>> allowLb, 
            Zen<IDictionary<string, string>> allowNs,
            Zen<IList<string>> selectKeys,
            Zen<IList<string>> allowKeys,
            Zen<IList<string>> allowNSKeys,
            Zen<bool> ingress,
            Zen<bool> DenyAll)
        {
            return Language.Create<Policy>(("Namespace", ns), ("SelectLabels", select), 
                ("AllowLabels", allowLb), ("AllowNamespaces", allowNs),
                ("SelectKeys", selectKeys), ("AllowKeys", allowKeys),
                ("AllowNSKeys", allowNSKeys), ("Ingress", ingress), ("DenyAll", DenyAll));
        }
    }
    public static class PolicyExtensions 
    {
        public static Zen<string> GetNS(this Zen<Policy> pod) => pod.GetField<Policy, string>("Namespace");
        public static Zen<IList<string>> GetSelectKeys(this Zen<Policy> policy) => policy.GetField<Policy, IList<string>>("SelectKeys");
        public static Zen<IList<string>> GetAllowKeys(this Zen<Policy> policy) => policy.GetField<Policy, IList<string>>("AllowKeys");
        public static Zen<IList<string>> GetAllowNSKeys(this Zen<Policy> policy) => policy.GetField<Policy, IList<string>>("AllowNSKeys");
        public static Zen<IDictionary<string, string>> GetAllowLabels(this Zen<Policy> policy) => policy.GetField<Policy, IDictionary<string, string>>("AllowLabels");
        public static Zen<IDictionary<string, string>> GetSelectLabels(this Zen<Policy> policy) => policy.GetField<Policy, IDictionary<string, string>>("SelectLabels");
        public static Zen<IDictionary<string, string>> GetAllowNS(this Zen<Policy> policy) => policy.GetField<Policy, IDictionary<string, string>>("AllowNamespaces");
        public static Zen<bool> GetIngress(this Zen<Policy> policy) => policy.GetField<Policy, bool>("Ingress");
        public static Zen<bool> GetDenyAll(this Zen<Policy> policy) => policy.GetField<Policy, bool>("DenyAll");
    }
    public class Namespace 
    {
        public string Name;
        public IDictionary<string, string> Labels;
        public IList<string> LabelKeys;
        public static Zen<Namespace> Create(Zen<string> name, Zen<IDictionary<string, string>> labels, Zen<IList<string>> labelKeys)
        {
            return Language.Create<Namespace>(("Name", name), ("Labels", labels), ("LabelKeys", labelKeys));
        }
        /*
        public override string ToString()
        {
            return string.Format("{0}.{1}", Name, string.Join(".", Labels));
        }
        */
    }
    public static class NamespaceExtensions
    {
        public static Zen<string> GetName(this Zen<Namespace> ns) => ns.GetField<Namespace, string>("Name");
        public static Zen<IDictionary<string, string>> GetLabels(this Zen<Namespace> ns) => ns.GetField<Namespace, IDictionary<string, string>>("Labels");
        public static Zen<IList<string>> GetLabelKeys(this Zen<Namespace> ns) => ns.GetField<Namespace, IList<string>>("LabelKeys");
    }
}
