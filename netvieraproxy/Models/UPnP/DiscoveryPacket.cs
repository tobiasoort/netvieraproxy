using System.Collections.Generic;
using System.Text;

namespace netvieraproxy.Models.UPnP
{
    /// <summary>
    /// Discovery packet for SSDP, implemented for Panasonic TVs.
    /// </summary>
    public class DiscoveryPacket
    {
        static readonly string Header = "M-SEARCH * HTTP/1.1";
        static readonly string Newline = "\r\n";
        public static readonly string Seperator = ": ";
        static readonly Dictionary<string, string> Fields =
            new Dictionary<string, string>{
            {"ST", "urn:panasonic-com:device:p00RemoteController:1"},
            {"MX", "1"},
            {"MAN", "\"ssdp:discover\""},
            {"HOST", "239.255.255.250:1900"}
        };
        int _Length;
        public int Length
        {
            get
            {
                if (_Length == 0)
                {
                    _Length = ToByteArray().Length;
                }
                return _Length;
            }
        }

        /// <summary>
        /// Represents the UDP SSDP Packet in binary form
        /// </summary>
        /// <returns>The byte array.</returns>
        public byte[] ToByteArray()
        {
            var output = new List<byte>();
            output.AddRange(Encoding.ASCII.GetBytes(Header));
            output.AddRange(Encoding.ASCII.GetBytes(Newline));
            foreach(var kv in Fields) {
                output.AddRange(Encoding.ASCII.GetBytes(
                    $"{kv.Key}{Seperator}{kv.Value}{Newline}"));
            }
            return output.ToArray();
        }


    }
}
