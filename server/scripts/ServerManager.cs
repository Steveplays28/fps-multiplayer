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

		label = GetNode<Label>("DebugLabel");

		Server = new Server(ServerPort);
		Server.LogHelper.Log += Log;
		Server.Start();
	}

	public override void _Process(float delta)
	{
		base._Process(delta);

		Server.Tick();
	}

	public override void _Notification(int what)
	{
		base._Notification(what);

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
}
