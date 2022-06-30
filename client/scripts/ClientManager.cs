using Godot;
using NExLib.Client;
using NExLib.Common;

public class ClientManager : Node
{
	[Export(PropertyHint.PlaceholderText, "For example: 127.0.0.1")] public string ServerIp = "127.0.0.1";
	[Export(PropertyHint.Range, "0,65535")] public int ServerPort = 26665;

	private Client client;
	private Label label;

	public override void _Ready()
	{
		base._Ready();

		label = GetNode<Label>("../Label");

		client = new Client();
		client.LogHelper.Log += Log;
		client.Connect(ServerIp, ServerPort);
	}

	public override void _Process(float delta)
	{
		base._Process(delta);

		client.Tick();
	}

	public override void _Notification(int what)
	{
		if (what == MainLoop.NotificationWmQuitRequest || what == NotificationCrash)
		{
			client.Disconnect();
			client.Close();
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
}
