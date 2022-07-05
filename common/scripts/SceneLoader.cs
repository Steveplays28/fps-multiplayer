using System.Linq;
using Godot;

public class SceneLoader : Node
{
	// Dictionaries containing node name and autoload scene path
	[Export] public PackedScene[] ServerAutoLoads = new PackedScene[0];
	[Export] public PackedScene[] ClientAutoLoads = new PackedScene[0];

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
			if (ClientAutoLoads.Length <= 0)
			{
				GD.PushError($"{nameof(ClientAutoLoads)} haven't been set yet. Please set them in the editor.");
				return;
			}

			foreach (PackedScene autoLoad in ClientAutoLoads)
			{
				GetTree().Root.CallDeferred("add_child", autoLoad.Instance());
			}

			GetTree().ChangeScene("client/scenes/client_main.tscn");
			OS.SetWindowTitle($"Client - {ProjectSettings.GetSetting("application/config/name")} {debugText}");
		}
		else
		{
			if (ServerAutoLoads.Length <= 0)
			{
				GD.PushError($"{nameof(ServerAutoLoads)} haven't been set yet. Please set them in the editor.");
				return;
			}

			foreach (PackedScene autoLoad in ServerAutoLoads)
			{
				GetTree().Root.CallDeferred("add_child", autoLoad.Instance());
			}

			GetTree().ChangeScene("server/scenes/server_main.tscn");
			OS.SetWindowTitle($"Server - {ProjectSettings.GetSetting("application/config/name")} {debugText}");
		}
	}
}
