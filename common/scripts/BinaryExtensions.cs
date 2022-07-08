using System.IO;
using Godot;

public static class BinaryExtensions
{
	public static Vector3 ReadVector3(this BinaryReader reader)
	{
		float x = reader.ReadSingle();
		float y = reader.ReadSingle();
		float z = reader.ReadSingle();

		return new Vector3(x, y, z);
	}

	public static void Write(this BinaryWriter writer, Vector3 vector3)
	{
		writer.Write(vector3.x);
		writer.Write(vector3.y);
		writer.Write(vector3.z);
	}

	public static Vector2 ReadVector2(this BinaryReader reader)
	{
		float x = reader.ReadSingle();
		float y = reader.ReadSingle();

		return new Vector2(x, y);
	}

	public static void Write(this BinaryWriter writer, Vector2 vector2)
	{
		writer.Write(vector2.x);
		writer.Write(vector2.y);
	}
}
