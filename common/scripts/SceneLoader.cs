using System.Linq;
using Godot;

public class SceneLoader : Node
{
	public override void _EnterTree()
	{
		base._EnterTree();

		bool isClient = false;

		if (OS.GetCmdlineArgs().Length > 0)
		{
			if (OS.GetCmdlineArgs().Contains("--client"))
			{
				isClient = true;
			}
		}

		string debugText = OS.IsDebugBuild() ? "(debug)" : "";
		if (isClient)
		{
			GetTree().ChangeScene("client/scenes/client_main.tscn");
			OS.SetWindowTitle($"Client - {ProjectSettings.GetSetting("application/config/name")} {debugText}");
		}
		else
		{
			GetTree().ChangeScene("server/scenes/server_main.tscn");
			OS.SetWindowTitle($"Server - {ProjectSettings.GetSetting("application/config/name")} {debugText}");
		}
	}
}
