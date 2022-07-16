shader_type spatial;

uniform sampler2D height_map;
uniform sampler2D normal_map;
uniform float height_scale = 1;

vec3 unpack_normal_map(vec4 rgba) {
	vec3 n = rgba.xzy * 2.0 - vec3(1.0);
	n.z *= -1.0;

	return n;
}

void vertex() {
	float height = texture(height_map, UV).x;
	VERTEX.y += height * height_scale;

	vec4 packed_normal_map = texture(normal_map, UV);
	vec3 unpacked_normal_map = unpack_normal_map(packed_normal_map);
	NORMAL = unpacked_normal_map * height_scale;
}

void fragment() {
	ALBEDO = vec3(0.25, 0.25, 0.25);
}
