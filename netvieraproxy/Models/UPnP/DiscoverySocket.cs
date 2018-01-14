using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace netvieraproxy.Models.UPnP
{
    public class DiscoverySocket : Socket
    {
        readonly int ssdpPort = 1900;
        readonly string ssdpMcastAddr = "239.255.255.250";
        readonly string bcastAddr = "0.0.0.0";

        public DiscoverySocket() :
            base(AddressFamily.InterNetwork,
                 SocketType.Dgram,
                 ProtocolType.Udp)
        {
            var ip = IPAddress.Parse(bcastAddr);
            Bind(new IPEndPoint(ip, ssdpPort));
            ReceiveTimeout = 5000;
        }

        public string Discover(int attempts = 3) {
            var packet = new DiscoveryPacket();
            var buffer = new byte[1024];
            int bytesrecvd = 0;
            for (var i = 1; i <= attempts && bytesrecvd == 0; i++) {
                try
                {
                    Send(packet);
                    bytesrecvd = Receive(buffer);
                } 
                catch(SocketException e) 
                {
                    if (e.SocketErrorCode != SocketError.TimedOut) {
                        throw e;
                    }
                    if (i == attempts) {
                        throw new TimeoutException($"Discovery timed out after {i} attempts.");
                    }
                }
            }

            var endpoint = ParseLocationData(buffer, bytesrecvd);
            return endpoint;
        }

        void Send(DiscoveryPacket packet) {
            var ssdpEndpoint = new IPEndPoint(IPAddress.Parse(ssdpMcastAddr), ssdpPort);
            SendTo(packet.ToByteArray(), 0, packet.Length, SocketFlags.None, ssdpEndpoint);
        }

        string ParseLocationData(byte[] input, int count)
        {
            var inputstr = Encoding.Default.GetString(input.Take(count).ToArray());
            var splitByLines = inputstr.Split(
                new[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
            );
            foreach (var line in splitByLines)
            {
                var parts = line.Split(new[] {
                    DiscoveryPacket.Seperator
                }, StringSplitOptions.None);
                if (parts.Length > 1 && parts[0] == "LOCATION")
                {
                    return parts[1];
                }
            }
            return string.Empty;
        }
    }
}
