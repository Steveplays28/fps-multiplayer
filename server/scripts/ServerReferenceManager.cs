using Godot;

public class ServerReferenceManager : Node
{
	public static ServerManager ServerManager { get; private set; }
	public static Label DebugLabel { get; private set; }

	public override void _Ready()
	{
		base._Ready();

		ServerManager = GetNode<ServerManager>($"/root/{nameof(ServerManager)}");
		DebugLabel = ServerManager.GetNode<Label>(nameof(DebugLabel));
	}
}
