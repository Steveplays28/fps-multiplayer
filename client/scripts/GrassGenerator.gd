tool
extends MultiMeshInstance

export(int, 0, 100) var spacing : int = 10;

func _ready():
	generate()

func generate():
	var heightmap_texture: Texture = load("res://client/resources/heightmaps/terrain/terrain_heightmap.png")
	var heightmap_image: Image = heightmap_texture.get_data()
	heightmap_image.lock()

	var i = 0
	for instance_count_x in multimesh.instance_count / 10:
		for instance_count_z in multimesh.instance_count / 10:
			# var image_data = heightmap_image.get_pixelv(Vector2(x, z))
			# var y = image_data.get_luminance() * 5

			var x = instance_count_x * spacing
			var y = 5
			var z = instance_count_z * spacing
			multimesh.set_instance_transform(i, Transform(Basis(), Vector3(x, y, z)))

			i += 1
			print("N: %d (X: %d Y: %d Z: %d)" % [i, x, y, z])
