tool
class_name MultiMeshGenerator
extends MultiMeshInstance

export var width := 100.0
export var length := 100.0
export(float, 0, 100) var spacing : float = 10
export(float, 0, 100) var position_variation : float = 0.5
export(float, 0, 360) var rotation_variation_degrees : float = 45
export(float, 0, 100) var height_min : float = 0.75
export(float, 0, 100) var height_max : float = 1.25
export(Texture) var heightmap : Texture
export(float, 0.1, 100) var height_scale : float = 1
export(float, 0, 1) var cutoff_height : float = 0.75

func _get_tool_buttons():
	return [{
		call="generate",
		text="Generate MultiMesh"
	}, {
		call="delete",
		text="Delete MultiMesh"
	}]

func generate():
	var heightmap_image := heightmap.get_data()
	heightmap_image.lock()

	var instance_count_per_axis := floor(sqrt((multimesh as MultiMesh).instance_count))
	var instance_count := instance_count_per_axis * instance_count_per_axis
	(multimesh as MultiMesh).instance_count = instance_count

	var i := 0
	for instance_count_x in instance_count_per_axis:
		for instance_count_z in instance_count_per_axis:
			var x : float = instance_count_x * spacing + rand_range(-position_variation, position_variation)
			var instance_height = rand_range(height_min, height_max)
			var y : float = (instance_height - 1) / 2
			if instance_height < 1:
				y = (instance_height - 1)
			var z : float = instance_count_z * spacing + rand_range(-position_variation, position_variation)

			var pixel := Vector2(round(heightmap_image.get_width() / instance_count_per_axis * instance_count_x), round(heightmap_image.get_height() / instance_count_per_axis * instance_count_z))
			var image_data = heightmap_image.get_pixelv(pixel)
			y += image_data.get_luminance() * height_scale - height_scale / 2

			var instance_width := 1
			var instance_length := 1
			if image_data.get_luminance() < cutoff_height:
				instance_width = 0
				instance_height = 0
				instance_length = 0

			var instance_basis := Basis(Vector3(0, rand_range(0, deg2rad(rotation_variation_degrees)), 0)).scaled(Vector3(instance_width, instance_height, instance_length))
			var instance_transform := Transform(instance_basis, Vector3(x, y, z))
			(multimesh as MultiMesh).set_instance_transform(i, instance_transform)

			# print("N: %d (X: %d Y: %d Z: %d)" % [i, x, y, z])

			i += 1

func delete():
	(multimesh as MultiMesh).instance_count = 0
	property_list_changed_notify()
