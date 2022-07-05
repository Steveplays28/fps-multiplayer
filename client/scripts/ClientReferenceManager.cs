using Godot;

public class ClientReferenceManager : Node
{
	public static ClientManager ClientManager { get; private set; }

	public override void _Ready()
	{
		base._Ready();

		ClientManager = GetNode<ClientManager>($"/root/{nameof(ClientManager)}");
	}
}
