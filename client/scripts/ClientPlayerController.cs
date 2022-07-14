using System.Net;
using Godot;
using NExLib.Common;

public class ClientPlayerController : Spatial
{
	[Export] public Vector2 Sensitivity = new Vector2(0.1f, 0.1f);
	[Export] public float MaxVerticalRotation = 90f;

	[Export] private readonly NodePath CameraNodePath = nameof(Camera);

	public int ClientId;

	private Camera camera;

	public override void _Ready()
	{
		base._Ready();

		camera = GetNode<Camera>(CameraNodePath);

		ClientReferenceManager.ClientManager.Client.PacketReceived += HandlePositionPacket;
	}

	public override void _PhysicsProcess(float delta)
	{
		base._Process(delta);

		HandleMouseCursorVisibilityInput();

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

	public override void _UnhandledInput(InputEvent inputEvent)
	{
		base._UnhandledInput(inputEvent);

		if (inputEvent is InputEventMouseMotion)
		{
			InputEventMouseMotion inputEventMouseMotion = inputEvent as InputEventMouseMotion;
			HandleMouseInput(inputEventMouseMotion);
		}
	}

	private void HandleMouseCursorVisibilityInput()
	{
		if (Input.IsActionJustPressed("toggle_mouse_cursor_visibility"))
		{
			if (Input.MouseMode == Input.MouseModeEnum.Visible)
			{
				Input.MouseMode = Input.MouseModeEnum.Captured;
			}
			else
			{
				Input.MouseMode = Input.MouseModeEnum.Visible;
			}
		}
	}

	private void HandleMouseInput(InputEventMouseMotion inputEventMouseMotion)
	{
		// Calculate mouse motion with sensitivity
		Vector2 mouseMotion = new Vector2(-inputEventMouseMotion.Relative.y * Sensitivity.x, -inputEventMouseMotion.Relative.x * Sensitivity.y);

		// Rotate camera and player
		camera.RotateX(Mathf.Deg2Rad(mouseMotion.x));
		RotateY(Mathf.Deg2Rad(mouseMotion.y));

		// Clamp camera vertical rotation
		Vector3 cameraRotationClamped = camera.RotationDegrees;
		cameraRotationClamped.x = Mathf.Clamp(cameraRotationClamped.x, -MaxVerticalRotation, MaxVerticalRotation);
		camera.RotationDegrees = cameraRotationClamped;

		// Send mouse input packet to server
		using (Packet packet = new Packet((int)PacketConnectedMethodExtension.MouseInput))
		{
			packet.Writer.Write(-inputEventMouseMotion.Relative.y * Sensitivity.x);
			packet.Writer.Write(-inputEventMouseMotion.Relative.x * Sensitivity.y);

			ClientReferenceManager.ClientManager.Client.SendPacket(packet);
		}
	}

	private void HandlePositionPacket(Packet packet, IPEndPoint serverIPEndPoint)
	{
		if (packet.ConnectedMethod != (int)PacketConnectedMethodExtension.Input)
		{
			return;
		}

		Translation = packet.Reader.ReadVector3();
	}
}
