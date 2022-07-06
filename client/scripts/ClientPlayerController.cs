using System.Net;
using Godot;
using NExLib.Common;

public class ClientPlayerController : Spatial
{
	public int ClientId;

	public override void _Ready()
	{
		base._Ready();

		ClientReferenceManager.ClientManager.Client.PacketReceived += HandlePositionPacket;
	}

	public override void _PhysicsProcess(float delta)
	{
		base._Process(delta);

		using (Packet packet = new Packet((int)PacketConnectedMethodExtension.Input))
		{
			packet.Writer.Write(Input.IsActionPressed("move_forward"));
			packet.Writer.Write(Input.IsActionPressed("move_backwards"));
			packet.Writer.Write(Input.IsActionPressed("move_right"));
			packet.Writer.Write(Input.IsActionPressed("move_left"));
			packet.Writer.Write(Input.IsActionPressed("sprint"));
			packet.Writer.Write(Input.IsActionPressed("slide"));
			packet.Writer.Write(Input.IsActionJustPressed("jump"));

			ClientReferenceManager.ClientManager.Client.SendPacket(packet);
		}
	}

	private void HandlePositionPacket(Packet packet, IPEndPoint clientIPEndPoint)
	{
		if (packet.ConnectedMethod != (int)PacketConnectedMethodExtension.Input)
		{
			return;
		}

		Translation = packet.Reader.ReadVector3();
	}
}
