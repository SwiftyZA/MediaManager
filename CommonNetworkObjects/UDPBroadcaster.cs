using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommonNetworkObjects
{
    public static class UDPBroadcaster
    {
        public static bool tcpConnectionActive = false;
        public static UdpClient client;
        public static void StartUdpBroadcast(int port)
        {
            client = new UdpClient();
            var ep = new IPEndPoint(IPAddress.Broadcast, port); // endpoint where server is listening
            var smokeSignal = (new UdpPacket()).Serialize();

            Task.Run(async () =>
            {
                while (!tcpConnectionActive)
                {
                    client.Connect(ep);
                    // send data
                    var r = client.Send(smokeSignal, smokeSignal.Length);
                    Thread.Sleep(5000);
                }
            });
        }
    }
}
