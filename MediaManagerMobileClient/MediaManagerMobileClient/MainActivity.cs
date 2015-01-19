using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using CommonNetworkObjects;

namespace MediaManagerMobileClient
{
	[Activity(Label = "Media Manager Client", MainLauncher = true)]
	public class MainActivity : Activity
	{
		UDPListener Udp;
		static IPAddress ServerIp;
		static TCPClient client;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			Button button1 = (Button)FindViewById(Resource.Id.button1);
			Button button2 = (Button)FindViewById(Resource.Id.button2);
			Button button3 = (Button)FindViewById(Resource.Id.button3);
			Button button4 = (Button)FindViewById(Resource.Id.button4);
			Button button5 = (Button)FindViewById(Resource.Id.button5);
			Button buttonAdd = (Button)FindViewById(Resource.Id.buttonAdd);

			button1.Click += delegate
			{
				Previous();
			};

			button2.Click += delegate
			{
				Stop();
			};

			button3.Click += delegate
			{
				Play();
			};

			button4.Click += delegate
			{
				VolDown();
			};

			button5.Click += delegate
			{
				VolUp();
			};

			buttonAdd.Click += delegate
			{
				Next();
			};

			StartUDPListener();
		}

		#region TCP CONNECTION
		public static bool CreateTCPConnection()
		{
			var serverPort = 2211;

			var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			try
			{
				socket.Connect(ServerIp, serverPort);
				client = new TCPClient(socket);
				client.Received += client_Received;
				client.Disconnected += client_Disconnected;
			}
			catch (Exception ex) { return false; }

			return true;
		}

		static void client_Disconnected(TCPClient sender)
		{
			sender.sck.Dispose();
		}

		static void client_Received(TCPClient sender, byte[] data)
		{
			if (data.Length == 0) return;

			var memoryStream = new MemoryStream(data);
			var msg = (NetworkPacket)(new BinaryFormatter()).Deserialize(memoryStream);
			if (msg.Type == MessageType.PingClient)
				return;

			switch (msg.Type)
			{
			case MessageType.Message:
				break;
			default:
			case MessageType.PingClient:
				return;// Do nothing, server is just checking if the connection is up
				break;
			}

			if (msg.Result == CommunicationResult.Failure)
			{
				//Todo - Handle
			}
		}

		public static bool SendMessage(NetworkPacket coms)
		{
			return client.SendMessage(coms);
		}
		#endregion TCP CONNECTION


		#region UDP CONNECTION
		private void StartUDPListener()
		{
			Udp = new UDPListener(2211);
			Udp.client.BeginReceive(new AsyncCallback(recv), Udp.client);// listen on port 11000
		}

		private void recv(IAsyncResult res)
		{
			if (client == null || client.sck == null || !client.sck.Connected) {
				byte[] received = Udp.client.EndReceive (res, ref Udp.remoteEP);
				var memoryStream = new MemoryStream (received);
				var msg = (UdpPacket)(new BinaryFormatter ()).Deserialize (memoryStream);
				ServerIp = msg.ServerIp;
				CreateTCPConnection ();
			}
		}
		#endregion UDP CONNECTION

		#region WINAMP FUNCTIONS
		private void Play()
		{
			if (client != null && client.sck != null && client.sck.Connected)
				client.SendMessage (new NetworkPacket () {
					Type = MessageType.WinampCommand,
					WinampCommand = new WinampCommand () { Opperation = WinampOpperations.Play }
				});
			else
				CreateTCPConnection ();
		}

		private void Pause()
		{
			if (ServerIp != null && (client == null || client.sck == null || !client.sck.Connected))
				CreateTCPConnection ();//check if connection is lost, if so connect again. Temporary work around
			if(client != null && client.sck != null && client.sck.Connected)
				client.SendMessage(new NetworkPacket() { Type = MessageType.WinampCommand, WinampCommand = new WinampCommand() { Opperation = WinampOpperations.Pause } });

		}

		private void Stop()
		{
			if (ServerIp != null && (client == null || client.sck == null || !client.sck.Connected))
				CreateTCPConnection ();//check if connection is lost, if so connect again. Temporary work around
			if(client != null && client.sck != null && client.sck.Connected)
				client.SendMessage(new NetworkPacket() { Type = MessageType.WinampCommand, WinampCommand = new WinampCommand() { Opperation = WinampOpperations.Stop } });
		}

		private void Next()
		{
			if (ServerIp != null && (client == null || client.sck == null || !client.sck.Connected))
				CreateTCPConnection ();//check if connection is lost, if so connect again. Temporary work around
			if(client != null && client.sck != null && client.sck.Connected)
				client.SendMessage(new NetworkPacket() { Type = MessageType.WinampCommand, WinampCommand = new WinampCommand() { Opperation = WinampOpperations.NextTrack } });
		}

		private void Previous()
		{
			if (ServerIp != null && (client == null || client.sck == null || !client.sck.Connected))
				CreateTCPConnection ();//check if connection is lost, if so connect again. Temporary work around
			if(client != null && client.sck != null && client.sck.Connected)
			 client.SendMessage(new NetworkPacket() { Type = MessageType.WinampCommand, WinampCommand = new WinampCommand() { Opperation = WinampOpperations.PreviousTrack } });
		}

		private void VolUp()
		{
			if (ServerIp != null && (client == null || client.sck == null || !client.sck.Connected))
				CreateTCPConnection ();//check if connection is lost, if so connect again. Temporary work around
			if(client != null && client.sck != null && client.sck.Connected)
				client.SendMessage(new NetworkPacket() { Type = MessageType.WinampCommand, WinampCommand = new WinampCommand() { Opperation = WinampOpperations.VolumeUp } });
		}

		private void VolDown()
		{
			if (ServerIp != null && (client == null || client.sck == null || !client.sck.Connected))
				CreateTCPConnection ();//check if connection is lost, if so connect again. Temporary work around
			if(client != null && client.sck != null && client.sck.Connected)
				client.SendMessage(new NetworkPacket() { Type = MessageType.WinampCommand, WinampCommand = new WinampCommand() { Opperation = WinampOpperations.VolumeDown } });
		}

		private void SetVol()
		{
			//client.SendMessage(new NetworkPacket() { Type = MessageType.WinampCommand, WinampCommand = new WinampCommand() { Opperation = WinampOpperations.SetVolume, Value = textBox1.Text } });
		}
		#endregion WINAMP FUNCTIONS
	}
}
