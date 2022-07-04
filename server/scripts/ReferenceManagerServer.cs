using Godot;

public class ReferenceManagerServer : Node
{
	public static Node ServerManager { get; private set; }

	public override void _Ready()
	{
		base._Ready();

		ServerManager = GetTree().CurrentScene.GetNode(nameof(ServerManager));
	}
}
