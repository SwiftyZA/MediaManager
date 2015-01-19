namespace MediaManager.Networking
{
    using System.Net;
    using System.Linq;
    using System.Collections.Generic;
    using System.Net.Sockets;
    using MediaManager.Data.Models;
    using MediaManager.Framework.Enumerators;
    using System;
    using System.Text;
    using Caliburn.Micro;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading.Tasks;
    using MediaManager.Framework.Functions;
    using MediaManager.Data;
    using CommonNetworkObjects;
    public class ConnectionMonger : PropertyChangedBase
    {
        readonly IEventAggregator _eventAggregator;
        TCPListener Listener;
        List<TCPClient> _connections = new List<TCPClient>();

        public bool ServerRunning
        {
            get
            {
                if (Listener != null)
                    return Listener.Listening;
                return false;
            }
        }

        public ConnectionMonger()
        {
            _eventAggregator = IoC.Get<IEventAggregator>();
        }

        public bool StartServer()
        {
            try
            {
                Listener = new TCPListener(2211);
                Listener.SocketAccepted += new TCPListener.SocketAcceptedHandler(Listener_SocketAccepted);
                Listener.Start();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        void Listener_SocketAccepted(Socket e)
        {
            var client = new TCPClient(e);
            client.Received += client_Received;
            client.Disconnected += client_Disconnected;
            _connections.Add(client);
            UDPBroadcaster.tcpConnectionActive = true;
        }

        void client_Disconnected(TCPClient sender)
        {
            sender.sck.Dispose();
            _connections.Remove(sender);
            if (_connections.Count == 0)
                UDPBroadcaster.StartUdpBroadcast(2211);
        }

        void client_Received(TCPClient sender, byte[] data)
        {
            var memoryStream = new MemoryStream(data);
            var msg = (NetworkPacket)(new BinaryFormatter()).Deserialize(memoryStream);

            switch (msg.Type)
            {
                case MessageType.InitialDetails:
                    break;
                case MessageType.LoggedInElsewhere: //handle DC request response
                    if (msg.Result == CommunicationResult.Success)//This should allways be true, but check it anyways
                    {
                        //Close connection
                        sender.Close();
                        _connections.Remove(sender);
                    }
                    break;
                case MessageType.WinampCommand:
                    _eventAggregator.PublishOnUIThread(msg.WinampCommand);
                    break;
            }
        }
    }
}
