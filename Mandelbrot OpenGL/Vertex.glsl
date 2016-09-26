#version 450 core

in vec2 vpos;
out vec2 fpos;

void main(void) {
	gl_Position = vec4(vpos, 0, 1);
	fpos = gl_Position.xy;
}
