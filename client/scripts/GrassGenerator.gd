tool
extends MultiMeshInstance

export(float, 0, 100) var spacing : float = 10;
export(float, 0, 100) var rotation_variation : float = 10;
export(float, 0, 100) var height_min : float = 0.75;
export(float, 0, 100) var height_max : float = 1.25;

func _ready():
	generate()

func generate():
	var heightmap_texture: Texture = load("res://client/resources/heightmaps/terrain/terrain_heightmap.png")
	var heightmap_image: Image = heightmap_texture.get_data()
	heightmap_image.lock()

	var instance_count_per_axis: int = floor(sqrt(multimesh.instance_count))
	var instance_count: int = instance_count_per_axis * instance_count_per_axis
	multimesh.instance_count = instance_count

	var i: int = 0
	for instance_count_x in instance_count_per_axis:
		for instance_count_z in instance_count_per_axis:
			# var image_data = heightmap_image.get_pixelv(Vector2(x, z))
			# var y = image_data.get_luminance() * 5

			var x: float = instance_count_x * spacing
			var y: float = 0
			var z: float = instance_count_z * spacing

			var height = rand_range(height_min, height_max)
			var instance_basis: Basis = Basis(Vector3(0, rand_range(0, rotation_variation), 0)).scaled(Vector3(1, height, 1))
			var instance_transform: Transform = Transform(instance_basis, Vector3(x, y + height / 2, z))
			multimesh.set_instance_transform(i, instance_transform)

			# print("N: %d (X: %d Y: %d Z: %d)" % [i, x, y, z])

			i += 1
	
	print(i)
