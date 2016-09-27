#version 450 core

in vec2 fuv;

out vec4 frag;

uniform vec3 colour;

uniform sampler2D texture;

void main(void) {
	frag = texture2D(texture, fuv);
	if (frag.a == 0) discard;
	else frag = colour;
}