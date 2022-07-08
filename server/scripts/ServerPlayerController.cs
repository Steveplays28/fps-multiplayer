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
	[Export(PropertyHint.Range, "0, 100")] public float CameraRollMultiplier = 0.1f;
	[Export(PropertyHint.Range, "0, 100")] public float CameraRollSpeed = 0.1f;
	[Export(PropertyHint.Range, "0, 100")] public float ClimbHeight = 10f;
	[Export(PropertyHint.Range, "0, 100")] public float ClimbLunge = 10f;
	[Export] private readonly NodePath CameraNodePath = nameof(Camera);
	[Export] private readonly string AnimationTreeNodePath;
	[Export] private readonly NodePath FloorRayCastNodePath;
	[Export] private readonly NodePath ClimbRayCastNodePath;

	public int ClientId { get; private set; }
	public Vector3 Velocity { get; private set; } = Vector3.Zero;
	public Vector3 VelocityLocal { get; private set; } = Vector3.Zero;
	public bool IsJumping { get; private set; } = false;
	public bool IsSliding { get; private set; } = false;

	private Camera camera;
	private AnimationTree animationTree;
	private RayCast floorRayCast;
	private RayCast climbRayCast;
	private float jumpTimeLeft;
	private int jumpsLeft;
	private Vector3 targetVelocity;

	public override void _Ready()
	{
		camera = GetNode<Camera>(CameraNodePath);
		animationTree = GetNode<AnimationTree>(AnimationTreeNodePath);
		floorRayCast = GetNode<RayCast>(FloorRayCastNodePath);
		climbRayCast = GetNode<RayCast>(ClimbRayCastNodePath);

		ServerReferenceManager.ServerManager.Server.PacketReceived += HandleMovementInputPacket;
		ServerReferenceManager.ServerManager.Server.PacketReceived += HandleMouseInputPacket;
	}

	public override void _PhysicsProcess(float delta)
	{
		base._PhysicsProcess(delta);

		// HandleRestartInput();

		HandleGravity(delta);
		HandleSlideInput();
		HandleJumpInput(delta);
		HandleClimb();

		ApplyVelocity(delta);
	}


	public bool IsGrounded()
	{
		return floorRayCast.IsColliding();
	}

	public Vector3 GetLocalVelocity()
	{
		return Velocity.Rotated(Vector3.Up, Rotation.y);
	}

	private void ApplyVelocity(float delta)
	{
		float acceleration = IsGrounded() ? Acceleration : AirAcceleration;
		Velocity = new Vector3(Mathf.Lerp(Velocity.x, targetVelocity.x, acceleration * delta), targetVelocity.y, targetVelocity.z);
		Velocity = new Vector3(targetVelocity.x, targetVelocity.y, Mathf.Lerp(Velocity.z, targetVelocity.z, acceleration * delta));

		if (IsJumping)
		{
			Velocity = MoveAndSlide(Velocity, Transform.basis.y, true);
		}
		else
		{
			Velocity = MoveAndSlideWithSnap(Velocity, Transform.basis.y * -2f, Transform.basis.y, true);

			float idle_walk_blend_amount = (float)animationTree.Get("parameters/idle_walk_blend/blend_amount");
			if (Velocity.Abs().Length() > 0.1f)
			{
				animationTree.Set("parameters/idle_walk_blend/blend_amount", Mathf.Clamp(idle_walk_blend_amount + delta, 0f, 1f));
				animationTree.Set("parameters/time_scale/scale", GetLocalVelocity().Length() / MaxMovementSpeed / 2f);
			}
			else
			{
				animationTree.Set("parameters/idle_walk_blend/blend_amount", Mathf.Clamp(idle_walk_blend_amount - delta, 0f, 1f));

				float time_scale = (float)animationTree.Get("parameters/time_scale/scale");
				animationTree.Set("parameters/time_scale/scale", Mathf.Clamp(time_scale + delta, 0f, 1f));
			}
		}

		using (Packet packet = new Packet((int)PacketConnectedMethodExtension.Input))
		{
			packet.Writer.Write(GlobalTransform.origin);

			ServerReferenceManager.ServerManager.Server.SendPacketTo(packet, ClientId);
		}
	}

	private void HandleRestartInput()
	{
		if (Input.IsActionJustPressed("restart"))
		{
			GetTree().ReloadCurrentScene();
		}
	}

	private void HandleMouseInputPacket(Packet packet, IPEndPoint clientIpEndPoint)
	{
		if (packet.ConnectedMethod != (int)PacketConnectedMethodExtension.MouseInput)
		{
			return;
		}

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

	private void HandleGravity(float delta)
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

	private void HandleSlideInput()
	{
		if (Input.IsActionJustPressed("slide"))
		{
			IsSliding = true;
		}
		if (Input.IsActionJustReleased("slide"))
		{
			IsSliding = false;
		}
	}

	private void HandleMovementInputPacket(Packet packet, IPEndPoint clientIPEndPoint)
	{
		if (packet.ConnectedMethod != (int)PacketConnectedMethodExtension.Input)
		{
			return;
		}

		bool moveForwardActionPressed = packet.Reader.ReadBoolean();
		bool moveBackwardsActionPressed = packet.Reader.ReadBoolean();
		bool moveRightActionPressed = packet.Reader.ReadBoolean();
		bool moveLeftActionPressed = packet.Reader.ReadBoolean();
		bool sprintActionPressed = packet.Reader.ReadBoolean();
		bool slideActionPressed = packet.Reader.ReadBoolean();
		bool jumpActionPressed = packet.Reader.ReadBoolean();

		float maxMovementSpeed;
		if (sprintActionPressed)
		{
			maxMovementSpeed = MaxSprintMovementSpeed;
		}
		else
		{
			maxMovementSpeed = MaxMovementSpeed;
		}

		Vector3 inputDirection = Vector3.Zero;
		Vector3 cameraRotationDegrees = camera.RotationDegrees;
		if (!IsSliding)
		{
			if (moveForwardActionPressed)
			{
				inputDirection -= Transform.basis.z;
			}
			if (moveBackwardsActionPressed)
			{
				inputDirection += Transform.basis.z;
			}
			if (moveRightActionPressed)
			{
				inputDirection += Transform.basis.x;

				cameraRotationDegrees.z = Mathf.Lerp(cameraRotationDegrees.z, Mathf.Clamp(cameraRotationDegrees.z - CameraRollSpeed * maxMovementSpeed, -CameraRollMultiplier * maxMovementSpeed, CameraRollMultiplier * maxMovementSpeed), CameraRollSpeed);
			}
			if (moveLeftActionPressed)
			{
				inputDirection -= Transform.basis.x;

				cameraRotationDegrees.z = Mathf.Lerp(cameraRotationDegrees.z, Mathf.Clamp(cameraRotationDegrees.z + CameraRollSpeed * maxMovementSpeed, -CameraRollMultiplier * maxMovementSpeed, CameraRollMultiplier * maxMovementSpeed), CameraRollSpeed);
			}
		}
		if (!moveRightActionPressed && !moveLeftActionPressed)
		{
			cameraRotationDegrees.z = Mathf.Lerp(camera.RotationDegrees.z, 0f, CameraRollSpeed);
		}

		camera.RotationDegrees = cameraRotationDegrees;
		inputDirection = inputDirection.Normalized();
		targetVelocity += inputDirection * maxMovementSpeed;

		float decceleration = IsSliding ? SlideDecceleration : Decceleration;
		decceleration = IsGrounded() ? decceleration : AirDecceleration;
		if (inputDirection.x == 0f)
		{
			targetVelocity = new Vector3(Mathf.Lerp(targetVelocity.x, 0f, decceleration * GetProcessDeltaTime()), targetVelocity.y, targetVelocity.z);
		}
		if (inputDirection.z == 0f)
		{
			targetVelocity = new Vector3(targetVelocity.x, targetVelocity.y, Mathf.Lerp(targetVelocity.z, 0f, decceleration * GetProcessDeltaTime()));
		}

		if (targetVelocity.Length() > maxMovementSpeed)
		{
			float targetVelocityY = targetVelocity.y;
			targetVelocity = targetVelocity.Normalized() * maxMovementSpeed;
			targetVelocity.y = targetVelocityY;
		}
	}

	private void HandleJumpInput(float delta)
	{
		if (Input.IsActionJustPressed("jump") && jumpsLeft > 0)
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
				jumpTimeLeft -= delta;
			}
			else
			{
				IsJumping = false;
			}
		}
	}

	private void HandleClimb()
	{
		if (climbRayCast.IsColliding() && Mathf.Rad2Deg(climbRayCast.GetCollisionNormal().AngleTo(Vector3.Up)) >= 90f)
		{
			targetVelocity = Transform.basis.y * ClimbHeight - Transform.basis.z * ClimbLunge;
		}
	}
}
