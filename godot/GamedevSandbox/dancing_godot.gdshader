shader_type canvas_item;

uniform vec2 iResolution;

void fragment() {
	vec2 uv = UV * 2.0 - 1.0;
	uv.x *= iResolution.x / iResolution.y;
	
	float d = length(uv);
	
	d = sin(d*8.0)/8.0;
	d = abs(d);
	d = smoothstep(0.0, 0.1, d);
	
	COLOR = vec4(d, d, d, 1.0);
}