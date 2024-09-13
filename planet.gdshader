shader_type spatial;
render_mode cull_back;

uniform float min_height;
uniform float max_height;
uniform vec3 water_color;
uniform vec3 land_color;
uniform vec3 sand_color;
uniform float base_height;
uniform float land_height;
uniform sampler2D height_color;

varying float height;
varying vec3 col;

void fragment() {
	//float t = height / (max_height - min_height ) - ( min_height / ( max_height - min_height));	 
	//vec3 color = texture(height_color, vec2(t, 0)).rgb;
	//ALBEDO = color;
  
  float threshold = .001;

  if( height <= base_height + threshold && height >= base_height - threshold ) {
    ALBEDO = water_color;
  } else {
    if( height <= base_height + land_height && height >= base_height + land_height - threshold ) {
      ALBEDO = land_color;
    } else {
      ALBEDO = sand_color;
    }     
  }

  //METALLIC = 1.;
  //ALPHA = 1.;
  // ALBEDO = vec3(255,0,0);
  // ALPHA = 0.15;

  // ALBEDO = vec3(255,0,255);

  // ALBEDO = vec3(UV.x,UV2.x,0.);
}

void vertex() {
	height = length(VERTEX);

  // float threshold = .001;

  // if( height <= base_height + threshold && height >= base_height - threshold ) {
  //   COLOR = vec4( water_color.rgb, 1.);
  // } else {
  //   if( height <= base_height + land_height && height >= base_height + land_height - threshold ) {
  //     COLOR = vec4( land_color, 1.);
  //   } else {
  //     COLOR = vec4(sand_color, 1.);
  //   }     
  // }

  // float t = height / (max_height - min_height ) - ( min_height / ( max_height - min_height));	 
	// col = texture(height_color, vec2(t, 0)).rgb;
	

	//height = length(1);

  //COLOR = vec4(125,255,255,125);

  // VERTEX *= 2f;
  //VERTEX.y += cos(VERTEX.x) * sin(VERTEX.z);
}