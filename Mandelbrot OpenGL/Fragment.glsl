#version 450 core

in vec2 fpos;
out vec4 frag;

uniform float aspectRatio;

uniform vec2 pos;
uniform float zoom;

const int maxIter = 1500;
const int specLen = 30;

const float ln2 = log(2);

vec3 colours[5] = vec3[] (
	vec3(163, 233, 237), 
	vec3(215, 67, 18), 
	vec3(0, 0, 0), 
	vec3(78, 34, 210), 
	vec3(13, 103, 228)
);

float iter(vec2 c) {
	vec2 z = vec2(0, 0);
	for (int i = 0; i < maxIter; i++) {
		z = vec2(z.x * z.x - z.y * z.y + c.x, 2 * z.x * z.y + c.y);
		if (z.x * z.x + z.y * z.y > 4) {
			return i;
			//z = vec2(z.x * z.x - z.y * z.y + c.x, 2 * z.x * z.y + c.y);
			//z = vec2(z.x * z.x - z.y * z.y + c.x, 2 * z.x * z.y + c.y);
			//float mod = sqrt(z.x * z.x + z.y * z.y);
			//return i - (log(log(mod))) / ln2;
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

vec3 calcClr(float i) {
	int idx1 = int(i * colours.length());
	int idx2 = idx1 + 1;
	if (idx2 >= colours.length) idx2 = 0;
	vec3 clr1 = colours[idx1];
	vec3 clr2 = colours[idx2];
	float ratio = mod(i, float(1) / colours.length()) / colours.length();
	return lerpClr(ratio, clr1, clr2);
}

void main(void) {
	vec2 z = fpos;
	z.y /= aspectRatio;
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


