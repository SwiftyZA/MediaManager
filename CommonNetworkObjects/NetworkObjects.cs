using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace CommonNetworkObjects
{
    [SerializableAttribute]
    public class UdpPacket
    {
        public byte[] Serialize()
        {
            ServerIp = Dns.GetHostAddresses(Dns.GetHostName()).FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
            using (var memoryStream = new MemoryStream())
            {
                (new BinaryFormatter()).Serialize(memoryStream, this);
                return memoryStream.ToArray();
            }
        }
        public IPAddress ServerIp { get; set; }
    }

    [SerializableAttribute]
    public class NetworkPacket
    {
        public byte[] Serialize()
        {
            using (var memoryStream = new MemoryStream())
            {
                (new BinaryFormatter()).Serialize(memoryStream, this);
                return memoryStream.ToArray();
            }
        }

        public MessageType Type { get; set; }
        public string Value { get; set; }
        public CommunicationResult Result { get; set; }
        public WinampCommand WinampCommand { get; set; }
    }

    [SerializableAttribute]
    public enum MessageType
    {
        InitialDetails = 1,
        ServerReponse = 2,
        PingClient = 3,
        Message = 4,
        LoggedInElsewhere = 5,
        WinampCommand = 6,
        ItemUpdate = 7
    }

    [SerializableAttribute]
    public enum CommunicationResult
    {
        Success = 1,
        Failure = 2
    }

    [Serializable]
    public class WinampCommand
    {
        public string Value { get; set; }
        public WinampOpperations Opperation { get; set; }
    }

    [Serializable]
    public enum WinampOpperations
    {
        NextTrack = 1,
        PreviousTrack = 2,
        Play = 3,
        Stop = 4,
        Pause = 5,
        VolumeUp = 6,
        VolumeDown = 7,
        SetVolume = 8
    }
}
