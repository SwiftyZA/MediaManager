namespace CommonNetworkObjects
{
    using CommonNetworkObjects;
    using System;
    using System.Net;
    using System.Net.Sockets;
    public class TCPClient
    {
        public Guid ID { get; private set; }
        public IPEndPoint EndPoint { get; private set; }

        public Socket sck;
        public TCPClient(Socket accepted)
        {
            sck = accepted;
            ID = Guid.NewGuid();
            EndPoint = (IPEndPoint)sck.RemoteEndPoint;
            sck.BeginReceive(new byte[] { 0 }, 0, 0, 0, callback, null);
        }

        void callback(IAsyncResult ar)
        {
            try
            {
                sck.EndReceive(ar);
                byte[] buf = new byte[8192];

                int rec = sck.Receive(buf, buf.Length, 0);

                if (rec < buf.Length)
                {
                    Array.Resize<byte>(ref buf, rec);
                }

                if (Received != null)
                {
                    Received(this, buf);
                }

                sck.BeginReceive(new byte[] { 0 }, 0, 0, 0, callback, null);
            }
            catch (Exception ex)
            {
                //TODO
                Close();

                if (Disconnected != null)
                    Disconnected(this);

            }
        }

        public bool SendMessage(NetworkPacket msg)
        {
            int s = sck.Send(msg.Serialize());
            if (s > 0)
                return true;
            return false;
        }

        public void Close()
        {
            sck.Close();
            sck.Dispose();

        }

        public delegate void ClientReceivedHandler(TCPClient sender, byte[] data);
        public delegate void ClientDisconnectedHandler(TCPClient sender);

        public event ClientReceivedHandler Received;
        public event ClientDisconnectedHandler Disconnected;
    }
}
