using System;
using System.Net;
using Godot;
using NExLib.Common;
using NExLib.Server;

public class ServerManager : Node
{
	[Export(PropertyHint.Range, "0,65535")] public int ServerPort = 26665;

	public Server Server { get; private set; }

	private Label label;

	public override void _Ready()
	{
		base._Ready();

		label = GetNode<Label>("../Label");

		Server = new Server(ServerPort);
		Server.LogHelper.Log += Log;
		Server.PacketReceived += PacketReceived;
		Server.Start();
	}

	public override void _Process(float delta)
	{
		base._Process(delta);

		Server.Tick();
	}

	public override void _Notification(int what)
	{
		if (what == NotificationWmQuitRequest || what == NotificationCrash)
		{
			Server.Stop();
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

	private void PacketReceived(Packet packet, IPEndPoint clientEndPoint)
	{
		Log(LogHelper.LogLevel.Info, Enum.GetName(typeof(PacketConnectedMethod), packet.ConnectedMethod));
	}
}
