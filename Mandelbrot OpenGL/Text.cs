using System;
using OpenGL;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Mandelbrot {

    class CharacterInfo {
        int x, y, width, height, xoffset, yoffset, xadvance;
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

    class Text {

        private static Dictionary<char, CharacterInfo> charSet = new Dictionary<char, CharacterInfo>(255);

        internal static void Init(string fileName) {
            StreamReader reader = new StreamReader(fileName + ".fnt");
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
}
