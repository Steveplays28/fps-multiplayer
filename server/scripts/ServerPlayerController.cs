using System.Net;
using Godot;
using NExLib.Common;

public class ServerPlayerController : KinematicBody
{
	[Export] public float MaxMovementSpeed = 10f;
	[Export] public float MaxSprintMovementSpeed = 25f;
	[Export] public float Mass = 80f;
	[Export] public Vector3 Gravity = new Vector3(0, -9.81f, 0);
	[Export] public float GravityScale = 1f;
	[Export(PropertyHint.Range, "0, 100")] public float Acceleration = 10f;
	[Export(PropertyHint.Range, "0, 100")] public float AirAcceleration = 5f;
	[Export(PropertyHint.Range, "0, 100")] public float Decceleration = 10f;
	[Export(PropertyHint.Range, "0, 100")] public float AirDecceleration = 0.5f;
	[Export(PropertyHint.Range, "0, 100")] public float SlideDecceleration = 0.5f;
	[Export] public float MaxVerticalRotation = 90f;
	[Export] public float JumpLength = 0.25f;
	[Export] public float JumpSpeed = 100f;
	[Export] public int JumpAmount = 2;
	[Export] public float DashLength = 0.25f;
	[Export] public float DashSpeed = 100f;
	[Export] public int DashAmount = 2;
	[Export(PropertyHint.Range, "0, 100")] public float CameraRollMultiplier = 0.1f;
	[Export(PropertyHint.Range, "0, 100")] public float CameraRollSpeed = 0.1f;
	[Export(PropertyHint.Range, "0, 100")] public float ClimbHeight = 10f;
	[Export(PropertyHint.Range, "0, 100")] public float ClimbLunge = 10f;

	[Export] private readonly NodePath CameraNodePath = nameof(Camera);
	[Export] private readonly NodePath FloorRayCastNodePath;
	[Export] private readonly NodePath ClimbRayCastNodePath;

	public int ClientId { get; private set; }
	public Vector3 Velocity { get; private set; } = Vector3.Zero;
	public Vector3 VelocityLocal { get; private set; } = Vector3.Zero;
	public bool IsJumping { get; private set; } = false;
	public bool IsSliding { get; private set; } = false;
	public bool IsDashing { get; private set; } = false;

	private Camera camera;
	private RayCast floorRayCast;
	private RayCast climbRayCast;
	private Vector3 targetVelocity;
	private int jumpsLeft;
	private float jumpTimeLeft;
	private int dashesLeft;
	private float dashTimeLeft;

	private readonly bool[] movementActions = new bool[8];
	private Vector3 inputDirection;
	private Vector3 slideDirection;

	private enum MovementActions
	{
		MoveForward,
		MoveBackwards,
		MoveRight,
		MoveLeft,
		Sprint,
		Slide,
		Jump,
		Dash
	}

	public override void _Ready()
	{
		camera = GetNode<Camera>(CameraNodePath);
		floorRayCast = GetNode<RayCast>(FloorRayCastNodePath);
		climbRayCast = GetNode<RayCast>(ClimbRayCastNodePath);

		ServerReferenceManager.ServerManager.Server.Listen((int)PacketTypes.Input, ReceiveInputPacket);
		ServerReferenceManager.ServerManager.Server.Listen((int)PacketTypes.MouseInput, ReceiveMouseInputPacket);
	}

	public override void _PhysicsProcess(float delta)
	{
		base._PhysicsProcess(delta);

		ApplyGravity(delta);
		Climb();

		ApplyVelocity(delta);
		SendPositionPacket();
	}


	public bool IsGrounded()
	{
		return floorRayCast.IsColliding();
	}

	public Vector3 GetLocalVelocity()
	{
		return Velocity.Rotated(Vector3.Up, Rotation.y);
	}

	private void HandleRestartInput()
	{
		if (Input.IsActionJustPressed("restart"))
		{
			GetTree().ReloadCurrentScene();
		}
	}

	private void ApplyGravity(float delta)
	{
		if (!IsGrounded())
		{
			targetVelocity += Gravity * Mass * GravityScale * delta;
		}
		else if (targetVelocity.y < 0f)
		{
			targetVelocity.y = 0f;
			jumpsLeft = JumpAmount;
		}
	}

	private void Climb()
	{
		if (climbRayCast.IsColliding() && Mathf.Rad2Deg(climbRayCast.GetCollisionNormal().AngleTo(Vector3.Up)) >= 90f)
		{
			targetVelocity = Transform.basis.y * ClimbHeight - Transform.basis.z * ClimbLunge;
		}
	}

	private void ReceiveMouseInputPacket(Packet packet, IPEndPoint clientIpEndPoint)
	{
		// Read mouse input packet
		Vector2 mouseMotion = packet.Reader.ReadVector2();

		// Rotate camera and player
		camera.RotateX(Mathf.Deg2Rad(mouseMotion.x));
		RotateY(Mathf.Deg2Rad(mouseMotion.y));

		// Clamp camera vertical rotation
		Vector3 cameraRotationClamped = camera.RotationDegrees;
		cameraRotationClamped.x = Mathf.Clamp(cameraRotationClamped.x, -MaxVerticalRotation, MaxVerticalRotation);
		camera.RotationDegrees = cameraRotationClamped;
	}

	private void ReceiveInputPacket(Packet packet, IPEndPoint clientIPEndPoint)
	{
		for (int i = 0; i < movementActions.Length; i++)
		{
			movementActions[i] = packet.Reader.ReadBoolean();
		}

		float maxMovementSpeed;
		if (movementActions[(int)MovementActions.Sprint])
		{
			maxMovementSpeed = MaxSprintMovementSpeed;
		}
		else
		{
			maxMovementSpeed = MaxMovementSpeed;
		}

		inputDirection = Vector3.Zero;
		targetVelocity = Vector3.Zero;
		Vector3 cameraRotationDegrees = camera.RotationDegrees;
		if (!IsSliding)
		{
			inputDirection += (movementActions[(int)MovementActions.MoveForward] ? Transform.basis.z : Vector3.Zero) + (movementActions[(int)MovementActions.MoveBackwards] ? -Transform.basis.z : Vector3.Zero);
			inputDirection += (movementActions[(int)MovementActions.MoveRight] ? Transform.basis.x : Vector3.Zero) + (movementActions[(int)MovementActions.MoveLeft] ? -Transform.basis.x : Vector3.Zero);

			cameraRotationDegrees.z = Mathf.Lerp(cameraRotationDegrees.z, Mathf.Clamp(cameraRotationDegrees.z + CameraRollSpeed * maxMovementSpeed * inputDirection.x, -CameraRollMultiplier * maxMovementSpeed, CameraRollMultiplier * maxMovementSpeed), CameraRollSpeed);
		}

		camera.RotationDegrees = cameraRotationDegrees;
		targetVelocity = inputDirection * maxMovementSpeed;

		// if (targetVelocity.Length() > maxMovementSpeed)
		// {
		// 	float targetVelocityY = targetVelocity.y;
		// 	targetVelocity = targetVelocity.Normalized() * maxMovementSpeed;
		// 	targetVelocity.y = targetVelocityY;
		// }

		if (movementActions[(int)MovementActions.Jump])
		{
			Jump();
		}
		if (movementActions[(int)MovementActions.Slide])
		{
			Slide();
		}
		if (movementActions[(int)MovementActions.Dash])
		{
			Dash();
		}
	}

	private void ApplyVelocity(float delta)
	{
		float maxMovementSpeed = movementActions[(int)MovementActions.Sprint] ? MaxSprintMovementSpeed : MaxMovementSpeed;
		targetVelocity = inputDirection * maxMovementSpeed;

		float acceleration = IsGrounded() ? Acceleration : AirAcceleration;
		Velocity = new Vector3(Mathf.Lerp(Velocity.x, targetVelocity.x, acceleration * delta), targetVelocity.y, Mathf.Lerp(Velocity.z, targetVelocity.z, acceleration * delta));

		float decceleration = IsSliding ? SlideDecceleration : Decceleration;
		decceleration = IsGrounded() ? decceleration : AirDecceleration;

		Velocity = IsJumping ? MoveAndSlide(Velocity, Transform.basis.y, true) : MoveAndSlideWithSnap(Velocity, Transform.basis.y * -2f, Transform.basis.y, true);
	}

	private void SendPositionPacket()
	{
		using (Packet packet = new Packet((int)PacketTypes.Input))
		{
			packet.Writer.Write(GlobalTransform.origin);

			ServerReferenceManager.ServerManager.Server.SendPacket(packet, ClientId);
		}
	}

	private void Jump()
	{
		if (jumpsLeft > 0)
		{
			IsJumping = true;
			jumpTimeLeft = JumpLength;
			jumpsLeft -= 1;
		}

		if (IsJumping)
		{
			if (jumpTimeLeft > 0)
			{
				targetVelocity = new Vector3(targetVelocity.x, JumpSpeed * jumpTimeLeft, targetVelocity.z);
				jumpTimeLeft -= GetProcessDeltaTime();
			}
			else
			{
				IsJumping = false;
			}
		}
	}

	private void Slide()
	{
		IsSliding = !IsSliding;
		slideDirection = inputDirection;
	}

	private void Dash()
	{
		if (dashesLeft > 0)
		{
			IsDashing = true;
			dashTimeLeft = DashLength;
			dashesLeft -= 1;
		}

		if (IsDashing)
		{
			if (dashTimeLeft > 0)
			{
				targetVelocity = inputDirection * DashSpeed;
				dashTimeLeft -= GetProcessDeltaTime();
			}
			else
			{
				IsDashing = false;
			}
		}
	}
}
