using Godot;
using NExLib.Common;

public class PlayerControllerClient : Node
{
	public override void _PhysicsProcess(float delta)
	{
		base._Process(delta);

		using (Packet packet = new Packet((int)ClientPacket.Input))
		{
			packet.Writer.Write(Input.IsActionPressed("move_forward"));

			ReferenceManagerClient.Client.Call(nameof(ClientManager.Client.SendPacket), packet);
		}
	}
}
