shader_type spatial;

uniform sampler2D height_map;
uniform sampler2D normal_map;
uniform float height_scale = 1.0;

uniform sampler2D albedo_tex: hint_albedo;
uniform float metallic = 0.0;
uniform float roughness = 1.0;
uniform float specular = 0.5;
uniform vec2 uv_scale = vec2(1.0, 1.0);

vec3 unpack_normal_map(vec4 rgba) {
	vec3 n = rgba.xzy * 2.0 - vec3(1.0);
	n.z *= -1.0;

	return n;
}

vec4 hash4(vec2 p) {
	return fract(sin(vec4(1.0 + dot(p, vec2(37.0, 17.0)), 2.0 + dot(p, vec2(11.0, 47.0)), 3.0 + dot(p, vec2(41.0, 29.0)), 4.0 + dot(p, vec2(23.0, 31.0)))) * 103.0);
}

vec4 texture_no_tile(sampler2D samp, vec2 uv)
{
	vec2 iuv = vec2(floor(uv));
	vec2 fuv = fract(uv);

	// Generate per tile transform
	vec4 ofa = hash4(iuv + vec2(0,0));
	vec4 ofb = hash4(iuv + vec2(1,0));
	vec4 ofc = hash4(iuv + vec2(0,1));
	vec4 ofd = hash4(iuv + vec2(1,1));

	vec2 ddx = dFdx(uv);
	vec2 ddy = dFdy(uv);

	// Transform per tile UVs
	ofa.zw = sign(ofa.zw-0.5);
	ofb.zw = sign(ofb.zw-0.5);
	ofc.zw = sign(ofc.zw-0.5);
	ofd.zw = sign(ofd.zw-0.5);
    
	// UVs and derivatives (for correct mipmapping)
	vec2 uva = uv * ofa.zw + ofa.xy, ddxa = ddx * ofa.zw, ddya = ddy * ofa.zw;
	vec2 uvb = uv * ofb.zw + ofb.xy, ddxb = ddx * ofb.zw, ddyb = ddy * ofb.zw;
	vec2 uvc = uv * ofc.zw + ofc.xy, ddxc = ddx * ofc.zw, ddyc = ddy * ofc.zw;
	vec2 uvd = uv * ofd.zw + ofd.xy, ddxd = ddx * ofd.zw, ddyd = ddy * ofd.zw;
   
	// Fetch and blend
	vec2 b = smoothstep(0.25, 0.75, fuv);

	return mix(mix(textureGrad(samp, uva, ddxa, ddya), textureGrad(samp, uvb, ddxb, ddyb), b.x), mix(textureGrad(samp, uvc, ddxc, ddyc), textureGrad(samp, uvd, ddxd, ddyd), b.x), b.y);
}

void vertex() {
	float height = texture(height_map, UV).x;
	VERTEX.y += height * height_scale;

//	vec4 packed_normal_map = texture(normal_map, UV);
//	vec3 unpacked_normal_map = unpack_normal_map(packed_normal_map);
//	NORMAL = unpacked_normal_map * height_scale;

	vec3 terrain_normal = unpack_normal_map(texture(normal_map, UV)) * vec3(1, 1, -1);

	// Combine terrain normals with detail normals (not sure if correct but looks ok)
//	vec3 normal = normalize(vec3(
//		terrain_normal.x + ground_normal.x, 
//		terrain_normal.y, 
//		terrain_normal.z + ground_normal.z));
	vec3 normal = normalize(vec3(
		terrain_normal.x, 
		terrain_normal.y, 
		terrain_normal.z));
	NORMAL = ((WORLD_MATRIX * -1.0 * vec4(normal, 0.0))).xyz * ((WORLD_MATRIX * 1.0 * vec4(normal, 0.0))).xyz * mat3(vec3(2), vec3(1), vec3(1));
}

void fragment() {
	vec2 uv = UV;
	uv *= uv_scale;
	
	ALBEDO = texture_no_tile(albedo_tex, uv).rgb;
	METALLIC = metallic;
	ROUGHNESS = roughness;
	SPECULAR = specular;
}
