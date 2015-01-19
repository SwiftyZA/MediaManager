namespace CommonNetworkObjects
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;
    public class UDPListener
    {
        public int Port { get; private set; }

        public UdpClient client;
        public IPEndPoint remoteEP;
        public UDPListener(int port)
        {
            client = new UdpClient(port);

            remoteEP = new IPEndPoint(IPAddress.Any, port);
           
        }


    }
}
