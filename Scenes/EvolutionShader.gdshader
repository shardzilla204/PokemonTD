shader_type canvas_item;

const vec4 new_color = vec4(1,1,1,1);

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	// Called for every pixel the material is visible on.
	vec4 current_color = texture(TEXTURE, UV);
	COLOR.rgb = new_color.rgb;
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
