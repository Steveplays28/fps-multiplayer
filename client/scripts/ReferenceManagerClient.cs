using Godot;

public class ReferenceManagerClient : Node
{
	public static Node Client { get; private set; }

	public override void _Ready()
	{
		base._Ready();

		Client = GetTree().CurrentScene.GetNode(nameof(ClientManager));
	}
}
