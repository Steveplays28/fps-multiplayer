// NOTE: Shader automatically converted from Godot Engine 3.5.rc6.mono's SpatialMaterial.

shader_type spatial;
render_mode async_visible, blend_mix, depth_draw_opaque, cull_disabled, diffuse_burley, specular_schlick_ggx;

uniform vec4 albedo : hint_color;
uniform sampler2D texture_albedo : hint_albedo;
uniform float specular : hint_range(0, 1) = 0.5;
uniform float metallic : hint_range(0, 1) = 0.0;
uniform float roughness : hint_range(0, 1) = 1.0;
uniform float alpha_scissor_threshold : hint_range(0, 1) = 0.5;
uniform vec3 uv1_scale = vec3(1.0);
uniform vec3 uv1_offset;

uniform sampler2D wave_noise_map;
uniform float wave_speed : hint_range(0, 100) = 1.0;
uniform float wave_amount : hint_range(0, 10) = 1.0;
uniform vec2 wave_direction = vec2(1);

void vertex() {
	UV = UV * uv1_scale.xy + uv1_offset.xy;

	vec3 world_pos = (WORLD_MATRIX * vec4(VERTEX, 1.0)).xyz;

	vec2 wave_direction_normalized = normalize(wave_direction);
	VERTEX.xz += wave_direction_normalized * sin(TIME * normalize(world_pos).y * wave_speed) * wave_amount * (1.0 - UV.y);
}

void fragment() {
	vec2 base_uv = UV;

	// Material properties
	vec4 albedo_tex = texture(texture_albedo, base_uv);
	ALBEDO = albedo.rgb * albedo_tex.rgb;
	METALLIC = metallic;
	ROUGHNESS = roughness;
	SPECULAR = specular;
	ALPHA = albedo.a * albedo_tex.a;
	ALPHA_SCISSOR = alpha_scissor_threshold;
}
