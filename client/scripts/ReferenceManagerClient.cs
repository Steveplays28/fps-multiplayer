using Godot;

public class ReferenceManagerClient : Node
{
	public static Node ClientManager { get; private set; }

	public override void _Ready()
	{
		base._Ready();

		ClientManager = GetTree().CurrentScene.GetNode(nameof(ClientManager));
	}
}
