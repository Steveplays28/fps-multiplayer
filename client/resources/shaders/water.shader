// NOTE: Shader automatically converted from Godot Engine 3.5.rc6.mono's SpatialMaterial.

shader_type spatial;
render_mode async_visible, blend_mix, depth_draw_always, cull_back, diffuse_burley, specular_schlick_ggx;

uniform vec4 albedo : hint_color = vec4(0.5, 0.75, 1, 0.75);
uniform sampler2D texture_albedo : hint_albedo;
uniform float specular : hint_range(0, 1) = 0.5;
uniform float metallic : hint_range(0, 1) = 1.0;
uniform float roughness : hint_range(0, 1) = 0.1;
uniform sampler2D texture_refraction;
uniform float refraction : hint_range(-16, 16);
uniform vec4 refraction_texture_channel;
uniform sampler2D texture_normal : hint_normal;
uniform float normal_scale : hint_range(-16, 16) = 1.0;
uniform float proximity_fade_distance = 0.25;
uniform vec3 uv1_scale;
uniform vec3 uv1_offset;
uniform vec2 water_direction = vec2(1, 0);
uniform float wave_1_speed : hint_range(0, 10) = 1;
uniform float wave_2_speed : hint_range(-10, 0) = -1;

void vertex() {
	UV = UV * uv1_scale.xy + uv1_offset.xy;
}

void fragment() {
	// Texture scrolling (wave animation)
	vec2 base_uv = UV + (water_direction * TIME * 0.001 * wave_1_speed);
	vec2 base_uv_negative = UV + (water_direction * TIME * 0.001 * wave_2_speed);

	// Material properties
	vec4 albedo_tex = texture(texture_albedo, base_uv);
	ALBEDO = albedo.rgb * albedo_tex.rgb;
	METALLIC = metallic;
	ROUGHNESS = roughness;
	SPECULAR = specular;
	NORMALMAP = mix(texture(texture_normal, base_uv).rgb, texture(texture_normal, base_uv_negative).rgb, 0.5);
	NORMALMAP_DEPTH = normal_scale;

	// Emmission and transparency
	vec3 unpacked_normal = NORMALMAP;
	unpacked_normal.xy = unpacked_normal.xy * 2.0 - 1.0;
	unpacked_normal.z = sqrt(max(0.0, 1.0 - dot(unpacked_normal.xy, unpacked_normal.xy)));
	vec3 ref_normal = normalize(mix(NORMAL,TANGENT * unpacked_normal.x + BINORMAL * unpacked_normal.y + NORMAL * unpacked_normal.z,NORMALMAP_DEPTH));
	vec2 ref_ofs = SCREEN_UV - ref_normal.xy * dot(texture(texture_refraction, base_uv), refraction_texture_channel) * refraction;
	float ref_amount = 1.0 - albedo.a * albedo_tex.a;
	EMISSION += textureLod(SCREEN_TEXTURE, ref_ofs, ROUGHNESS * 8.0).rgb * ref_amount;
	ALBEDO *= 1.0 - ref_amount;
	ALPHA = albedo.a;

	// Proximity fade
	float depth_tex = textureLod(DEPTH_TEXTURE, SCREEN_UV,0.0).r;
	vec4 world_pos = INV_PROJECTION_MATRIX * vec4(SCREEN_UV * 2.0 - 1.0, depth_tex * 2.0 - 1.0, 1.0);
	world_pos.xyz /= world_pos.w;
	ALPHA *= clamp(1.0 - smoothstep(world_pos.z + proximity_fade_distance, world_pos.z, VERTEX.z), 0.0, 1.0);
}
