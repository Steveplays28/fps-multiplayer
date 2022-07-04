using Godot;
using NExLib.Common;

public class PlayerControllerClient : Node
{
	public override void _PhysicsProcess(float delta)
	{
		base._Process(delta);

		using (Packet packet = new Packet((int)PacketConnectedMethod.Input))
		{
			packet.Writer.Write(Input.IsActionPressed("move_forward"));
			packet.Writer.Write(Input.IsActionPressed("move_backwards"));
			packet.Writer.Write(Input.IsActionPressed("move_right"));
			packet.Writer.Write(Input.IsActionPressed("move_left"));

			ReferenceManagerClient.ClientManager.Call(nameof(ClientManager.Client.SendPacket), packet);
		}
	}
}
