using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CommonNetworkObjects
{
    public static class UDPBroadcast
    {
        public static bool tcpConnectionActive = false;
        public static UdpClient client;
        public void StartUdpBroadcast(int port)
        {
            client = new UdpClient();
            IPEndPoint ep = new IPEndPoint(IPAddress.Broadcast, port); // endpoint where server is listening
            client.Connect(ep);

            // send data
            var smokeSignal = (new UdpPacket()).Serialize();
            int result = client.Send(smokeSignal, smokeSignal.Length);

            //Console.Write("receive data from " + remoteEP.ToString());
            //udpServer.Send(new byte[] { 1 }, 1, remoteEP); // reply back
        }

        private static void recv(IAsyncResult res)
        {
            client.Send(new byte[] { 1, 2, 3, 4, 5 }, 5);
        }
    }
}
