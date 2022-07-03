using Godot;

public class ReferenceManagerServer : Node
{
	public static KinematicBody Player { get; private set; }

	public override void _Ready()
	{
		base._Ready();

		// Player = GetTree().CurrentScene.GetNode<KinematicBody>("Player");
	}
}
