namespace act.core.web.Models.BuildSpec
{
    public class JsonInspecAttributes
    {
        // ReSharper disable InconsistentNaming
        public string fqdn { get; set; }
        public string os_name { get; set; }
        public string os_version { get; set; }
        public int[] tcp_ports { get; set; }
        public int[] udp_ports { get; set; }
        public int[] tcp6_ports { get; set; }
        public int[] udp6_ports { get; set; }
        public string[] features { get; set; }
        public string[] products { get; set; }
        public string[] packages { get; set; }
        // ReSharper restore InconsistentNaming


        internal static JsonInspecAttributes Empty(string fqdn)
        {
            return new JsonInspecAttributes
            {
                fqdn = fqdn,
                os_name = "Unknown",
                tcp_ports = new int[0],
                udp_ports = new int[0],
                tcp6_ports = new int[0],
                udp6_ports = new int[0],
                packages = new string[0],
                features = new string[0],
                products = new string[0]
            };
        }
        internal static JsonInspecAttributes ForNix(string fqdn, string osName, string osVersion, int[] tcp, int[]udp,
            int[] tcp6, int[] udp6, string[] packages)
        {
            return new JsonInspecAttributes
            {
                fqdn = fqdn,
                os_name = osName,
                os_version = osVersion,
                tcp_ports = tcp,
                udp_ports = udp,
                tcp6_ports = tcp6,
                udp6_ports = udp6,
                packages = packages
            };

        }
        internal static JsonInspecAttributes ForWindows(string fqdn, string osName, string osVersion, int[] tcp, int[] udp, string[] features, string[] products)
        {
            return new JsonInspecAttributes
            {
                fqdn = fqdn,
                os_name = osName,
                os_version = osVersion,
                tcp_ports = tcp,
                udp_ports = udp,
                features = features,
                products = products
            };

        }

        internal static JsonInspecAttributes ForOther(string fqdn, string osName, string osVersion, int[] tcp, int[] udp)
        {
            return new JsonInspecAttributes
            {
                fqdn = fqdn,
                os_name = osName,
                os_version = osVersion,
                tcp_ports = tcp,
                udp_ports = udp
            };



        }
    }
}
 