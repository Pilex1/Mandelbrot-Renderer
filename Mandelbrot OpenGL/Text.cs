using System;
using OpenGL;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Drawing;

namespace Mandelbrot {

    class TexModel {
        public BeginMode drawingMode { get; internal set; }
        public VBO<Vector2> vertices { get; internal set; }
        public VBO<int> elements { get; internal set; }
        public VBO<Vector2> uvs { get; internal set; }
        public Texture texture { get; internal set; }
        internal TexModel(VBO<Vector2> vertices, VBO<int> elements, VBO<Vector2> uvs, Texture texture, BeginMode drawingMode) {
            this.vertices = vertices;
            this.elements = elements;
            this.uvs = uvs;
            this.texture = texture;
            this.drawingMode = drawingMode;
        }
    }

    class CharacterInfo {
        internal int x, y, width, height, xoffset, yoffset, xadvance;
        internal CharacterInfo(int x, int y, int width, int height, int xoffset, int yoffset, int xadvance) {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.xoffset = xoffset;
            this.yoffset = yoffset;
            this.xadvance = xadvance;
        }
    }

    class Font {

        internal Dictionary<char, CharacterInfo> charSet = new Dictionary<char, CharacterInfo>(255);
        internal string fontName;
        internal float size;
        internal Vector3 colour;

        public Font(string fontName, float size, Vector3 colour) {
            this.fontName = fontName;
            this.size = size / 512;
            this.colour = colour;
            StreamReader reader = new StreamReader("Fonts/" + fontName + ".fnt");
            Regex regex = new Regex(@"^char id=(-?\d+)\D+x=(-?\d+)\D+y=(-?\d+)\D+width=(-?\d+)\D+height=(-?\d+)\D+xoffset=(-?\d+)\D+yoffset=(-?\d+)\D+xadvance=(-?\d+)");
            string s;
            while ((s = reader.ReadLine()) != null) {
                Match match = regex.Match(s);
                if (match.Success) {
                    charSet.Add((char)int.Parse(match.Groups[1].Value), new CharacterInfo(
                        int.Parse(match.Groups[2].Value),
                        int.Parse(match.Groups[3].Value),
                        int.Parse(match.Groups[4].Value),
                        int.Parse(match.Groups[5].Value),
                        int.Parse(match.Groups[6].Value),
                        int.Parse(match.Groups[7].Value),
                        int.Parse(match.Groups[8].Value)
                    ));
                }
            }
        }
    }

    class Text {

        public TexModel model { get; private set; }
        public Font font { get; private set; }

        public Text(Font font) {
            this.font = font;
            Texture texture = new Texture("Fonts/" + font.fontName + ".png");
            VBO<Vector2> vertices;
            VBO<int> elements;
            VBO<Vector2> uvs;
            SetTextHelper("", texture.Size.Width, texture.Size.Height, out vertices, out elements, out uvs);
            model = new TexModel(vertices, elements, uvs, texture, BeginMode.Triangles);
        }

        private void SetTextHelper(string s, int width, int height, out VBO<Vector2> vertices, out VBO<int> elements, out VBO<Vector2> uvs) {
            Vector2[] verticesArr = new Vector2[s.Length * 4];
            Vector2[] uvsArr = new Vector2[s.Length * 4];

            int xptr = 0;
            int imageWidth = width, imageHeight = height;
            for (int i = 0; i < s.Length; i++) {

                char c = s[i];
                CharacterInfo info = font.charSet[c];

                //topleft, bottomleft, bottomright, topright
                verticesArr[4 * i] = new Vector2(xptr + info.xoffset, info.yoffset + info.height);
                verticesArr[4 * i + 1] = new Vector2(xptr + info.xoffset, 0);
                verticesArr[4 * i + 2] = new Vector2(xptr + info.xoffset + info.width, 0);
                verticesArr[4 * i + 3] = new Vector2(xptr + info.xoffset + info.width, info.yoffset + info.height);
                xptr += info.xadvance;

                uvsArr[4 * i] = new Vector2(info.x, info.y);
                uvsArr[4 * i + 1] = new Vector2(info.x, info.y + info.height);
                uvsArr[4 * i + 2] = new Vector2(info.x + info.width, info.y + info.height);
                uvsArr[4 * i + 3] = new Vector2(info.x + info.width, info.y);
            }

            for (int i = 0; i < uvsArr.Length; i++) {
                uvsArr[i].x /= imageWidth;
                uvsArr[i].y /= imageHeight;
                uvsArr[i].y = 1 - uvsArr[i].y;
            }

            for (int i = 0; i < verticesArr.Length; i++) {
                verticesArr[i].x *= font.size;
                verticesArr[i].y *= font.size;
            }

            int[] elementsArr = new int[s.Length * 6];
            for (int i = 0; i < s.Length; i++) {
                elementsArr[6 * i] = 4 * i;
                elementsArr[6 * i + 1] = 4 * i + 1;
                elementsArr[6 * i + 2] = 4 * i + 2;
                elementsArr[6 * i + 3] = 4 * i;
                elementsArr[6 * i + 4] = 4 * i + 2;
                elementsArr[6 * i + 5] = 4 * i + 3;
            }

            vertices = new VBO<Vector2>(verticesArr);
            uvs = new VBO<Vector2>(uvsArr);
            elements = new VBO<int>(elementsArr, BufferTarget.ElementArrayBuffer);
        }

        public void SetText(string s) {
            VBO<Vector2> vertices;
            VBO<int> elements;
            VBO<Vector2> uvs;
            SetTextHelper(s, model.texture.Size.Width, model.texture.Size.Height, out vertices, out elements, out uvs);
            model.vertices.Dispose();
            model.elements.Dispose();
            model.uvs.Dispose();
            model.vertices = vertices;
            model.elements = elements;
            model.uvs = uvs;
        }
    }
}
