using System.Collections.Generic;
using System.Net;
using Godot;
using NExLib.Client;
using NExLib.Common;

public class ClientManager : Node
{
	[Export(PropertyHint.PlaceholderText, "For example: 127.0.0.1")] public string ServerIp = "127.0.0.1";
	[Export(PropertyHint.Range, "0,65535")] public int ServerPort = 26665;
	[Export] public PackedScene ClientPlayer;

	public Client Client { get; private set; }
	public Dictionary<int, Spatial> Players { get; private set; } = new Dictionary<int, Spatial>();

	private Label label;
	private Button connectButton;

	public override void _Ready()
	{
		base._Ready();

		label = GetNode<Label>("Label");
		connectButton = GetNode<Button>("ConnectButton");

		connectButton.Connect("pressed", this, nameof(ConnectButtonPressed));

		Client = new Client();
		Client.LogHelper.Log += Log;
		Client.Connected += Connected;
		Client.Disconnected += Disconnected;
	}

	public override void _Process(float delta)
	{
		base._Process(delta);

		Client.Tick();
	}

	public override void _PhysicsProcess(float delta)
	{
		base._PhysicsProcess(delta);

		// using (Packet packet = new Packet((int)PacketConnectedMethod.KeepAlive))
		// {
		// 	Client.SendPacket(packet);
		// }
	}

	public override void _Notification(int what)
	{
		base._Notification(what);

		if (what == MainLoop.NotificationWmQuitRequest || what == NotificationCrash)
		{
			if (Client.IsConnected)
			{
				Client.Disconnect();
			}
			Client.Close();
		}
	}

	private void Log(LogHelper.LogLevel logLevel, string logMessage)
	{
		if (logLevel == LogHelper.LogLevel.Info)
		{
			GD.Print(logMessage);
			label.Text += $"{logMessage}\n";
		}
		else if (logLevel == LogHelper.LogLevel.Warning)
		{
			GD.PushWarning(logMessage);
			label.Text += $"{logMessage}\n";
		}
		else if (logLevel == LogHelper.LogLevel.Error)
		{
			GD.PushError(logMessage);
			label.Text += $"{logMessage}\n";
		}
	}

	private void ConnectButtonPressed()
	{
		Client.Connect(ServerIp, ServerPort);
	}

	private void Connected(Packet packet, IPEndPoint serverIPEndPoint)
	{
		ClientPlayerController player = ClientPlayer.Instance<ClientPlayerController>();
		GetTree().Root.AddChild(player);

		int clientId = ClientReferenceManager.ClientManager.Client.ClientId;
		player.ClientId = clientId;
		Players.Add(clientId, player);
	}

	private void Disconnected(Packet packet, IPEndPoint serverIPEndPoint)
	{
		int clientId = packet.Reader.ReadInt32();

		Players[clientId].QueueFree();
		Players.Remove(clientId);
	}
}
