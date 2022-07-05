using System.Net;
using Godot;
using NExLib.Common;

public class ClientPlayerController : Spatial
{
	public override void _Ready()
	{
		base._Ready();

		ClientManager clientManager = ClientReferenceManager.ClientManager;
		clientManager.Client.PacketReceived += HandlePositionPacket;
	}

	public override void _PhysicsProcess(float delta)
	{
		base._Process(delta);

		using (Packet packet = new Packet((int)PacketConnectedMethod.Input))
		{
			packet.Writer.Write(Input.IsActionPressed("move_forward"));
			packet.Writer.Write(Input.IsActionPressed("move_backwards"));
			packet.Writer.Write(Input.IsActionPressed("move_right"));
			packet.Writer.Write(Input.IsActionPressed("move_left"));

			ClientReferenceManager.ClientManager.Client.SendPacket(packet);
		}
	}

	private void HandlePositionPacket(Packet packet, IPEndPoint clientIPEndPoint)
	{
		if (packet.ConnectedMethod != (int)PacketConnectedMethod.Input)
		{
			return;
		}

		Translation = packet.Reader.ReadVector3();
	}
}
