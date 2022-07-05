using System.Net;
using Godot;
using NExLib.Client;
using NExLib.Common;

public class ClientManager : Node
{
	[Export(PropertyHint.PlaceholderText, "For example: 127.0.0.1")] public string ServerIp = "127.0.0.1";
	[Export(PropertyHint.Range, "0,65535")] public int ServerPort = 26665;

	public Client Client { get; private set; }

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

	private void ConnectButtonPressed()
	{
		Client.Connect(ServerIp, ServerPort);
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
}
