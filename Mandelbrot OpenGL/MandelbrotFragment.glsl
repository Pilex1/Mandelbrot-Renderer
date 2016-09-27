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

bool debug = false;

vec3 colours[6] = vec3[] (
vec3(64, 63, 42),
vec3(215, 123, 25),
vec3(201, 195, 54),
vec3(34, 10, 204),
vec3(87, 40, 209),
vec3(64, 63, 42)
);

struct CNum {
	float re, im;
};

CNum cadd(CNum a, CNum b) {
	return CNum(a.re + b.re, a.im + b.im);
}
CNum csub(CNum a, CNum b) {
	return CNum(a.re - b.re, a.im - b.im);
}
CNum cmul(CNum a, CNum b) {
	return CNum(a.re * b.re - a.im * b.im, a.re * b.im + a.im * b.re);
}
CNum csquare(CNum a) {
	return CNum(a.re * a.re - a.im * a.im, 2 * a.re * a.im);
}
CNum cdiv(CNum a, CNum b) {
	float den = b.re * b.re + b.im * b.im;
	return CNum((a.re * b.re + a.im * b.im) / den, (a.im * b.re + a.re * b.im) / den);
}
float cmodsq(CNum a) {
	return a.re * a.re + a.im * a.im;
}
float cmod(CNum a) {
	return sqrt(a.re * a.re + a.im * a.im);
}
CNum crecpcl(CNum a) {
	float den = cmodsq(a);
	if (den == 0) {
		return CNum(0, 0);
	} else {
		return CNum(a.re / den, -a.im / den);
	}
}

float iter(CNum c) {
	CNum z = CNum(0, 0);
	for (int i = 0; i < maxIter; i++) {
		z = csquare(z);
		z = cadd(z, c);
		if (cmodsq(z) > 4) {
			z = csquare(z);
			z = cadd(z, c);
			z = csquare(z);
			z = cadd(z, c);
			float mod = cmod(z);
			if (mod <= 1) return i;
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
	bool line = false;
	if (
		(abs(z.x + 0.5f) < 0.001 || abs(z.y) < 0.001 * aspectRatio) && 
		((z.x + 0.5f) * (z.x + 0.5f) + (z.y / aspectRatio) * (z.y / aspectRatio) <= 0.002f)
	) line = true;
	z.y /= aspectRatio;
	z.x += 0.5f;
	z = (vec4(z, 0, 1) * rot).xy;
	z.x *= zoom;
	z.y *= zoom;
	z += pos;

	float iter = iter(CNum(z.x, z.y));
	if (iter == maxIter) frag = vec4(0, 0, 0, 1);
	else {
		frag = vec4(calcClr(mod(iter, specLen) / specLen), 1);
	}

	if (line) {
		frag.x = 1 - frag.x;
		frag.y = 1 - frag.y;
		frag.z = 1 - frag.z;
	}

	if (debug) frag = vec4(1, 1, 1, 1);
}

