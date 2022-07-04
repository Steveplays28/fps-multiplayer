using Godot;

public class ReferenceManagerClient : Node
{
	public static ClientManager ClientManager { get; private set; }

	public override void _Ready()
	{
		base._Ready();

		ClientManager = GetTree().CurrentScene.GetNode<ClientManager>(nameof(ClientManager));
	}
}
