#version 450 core

in vec2 fpos;
out vec4 frag;

uniform float aspectRatio;

uniform vec2 pos;
uniform float zoom;

uniform mat4 rot;

uniform int maxIter;
const int specLen = 50;

const float ln2 = log(2);

vec3 colours[6] = vec3[] (
vec3(64, 63, 42),
vec3(215, 123, 25),
vec3(201, 195, 54),
vec3(34, 10, 204),
vec3(87, 40, 209),
vec3(64, 63, 42)
);


float iter(vec2 c) {
	vec2 z = vec2(0, 0);
	for (int i = 0; i < maxIter; i++) {
		z = vec2(z.x * z.x - z.y * z.y + c.x, 2 * z.x * z.y + c.y);
		if (z.x * z.x + z.y * z.y > 4) {
			z = vec2(z.x * z.x - z.y * z.y + c.x, 2 * z.x * z.y + c.y);
			z = vec2(z.x * z.x - z.y * z.y + c.x, 2 * z.x * z.y + c.y);
			float mod = sqrt(z.x * z.x + z.y * z.y);
			return i - log(log(mod)) / ln2;
		}
	}
	return maxIter;
}

vec3 lerpClr(float n, vec3 a, vec3 b) {
	float cr = a.x + (b.x - a.x) * n;
	float cg = a.y + (b.y - a.y) * n;
	float cb = a.z + (b.z - a.z) * n;
	return vec3(cr / 255, cg / 255, cb / 255);
}

float map(float n, float inputLow, float inputHigh, float outputLow, float outputHigh) {
	return (n - inputLow) * (outputHigh - outputLow) / (inputHigh - inputLow) + outputLow;
}

vec3 calcClr(float i) {
	float j = 0;
	int count = 0;
	for (; j + float(1) / (colours.length() - 1) < i; j += float(1) / (colours.length() - 1), count++) {
	}
	i = map(i, float(count) / (colours.length() - 1), float(count + 1) / (colours.length() - 1), 0, 1);
	return lerpClr(i, colours[count], colours[count + 1]);
}

void main(void) {
	vec2 z = fpos;
	
	z.y /= aspectRatio;
	z = (vec4(z, 0, 1) * rot).xy;
	z.x *= zoom;
	z.y *= zoom;
	z += pos;

	

	float iter = iter(z);
	if (iter == maxIter) frag = vec4(0, 0, 0, 1);
	else {
		vec3 clr = calcClr(mod(iter, specLen) / specLen);
		frag = vec4(clr, 1);
	}
}


