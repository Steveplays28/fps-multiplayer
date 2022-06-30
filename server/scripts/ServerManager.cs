using Godot;
using NExLib.Common;
using NExLib.Server;

public class ServerManager : Node
{
	[Export(PropertyHint.Range, "0,65535")] public int ServerPort = 26665;

	private Server server;
	private Label label;

	public override void _Ready()
	{
		base._Ready();

		label = GetNode<Label>("../Label");

		server = new Server(ServerPort);
		server.LogHelper.Log += Log;
		server.Start();
	}

	public override void _Process(float delta)
	{
		base._Process(delta);

		server.Tick();
	}

	public override void _Notification(int what)
	{
		if (what == NotificationWmQuitRequest || what == NotificationCrash)
		{
			server.Stop();
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
