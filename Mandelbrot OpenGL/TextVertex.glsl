#version 450 core

in vec2 vpos;
in vec2 vuv;

uniform vec2 trans;

out vec2 fuv;

void main(void) {
	gl_Position = vec4(vpos +  trans, 0, 1);
	fuv = vuv;
}
