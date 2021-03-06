﻿using System;
using OpenGL;
using Tao.FreeGlut;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Mandelbrot {
    static class Program {

        static int width = 1200, height = 800;
        const string titleName = "Mandelbrot Renderer - Copyright Alex Tan 2016";

        static ShaderProgram mandShader;
        static ShaderProgram juliaShader;
        static ShaderProgram textShader;

        static VBO<Vector2> mandVertices;
        static VBO<Vector2> juliaVertices;
        static VBO<int> elements;

        static bool[] keys = new bool[255];
        static Vector2 mousePos = new Vector2(0, 0);

        static Vector2 mandPos = new Vector2(0, 0);
        static Vector2 juliaPos = new Vector2(0, 0);

        static float mandZoom = 2;
        static float juliaZoom = 2;

        static float mandRot = 0;
        static float juliaRot = 0;

        static float maxIter = 1500f;

        static float prevTime;
        static float deltaTime;
        static float countTime;
        static int countTimeFrames;
        static int fps;
        static Stopwatch watch = new Stopwatch();

        static Text mandText;
        static Text juliaText;

        static void Main() {
            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);

            // width = Glut.glutGet(Glut.GLUT_SCREEN_WIDTH) == 0 ? width : Glut.glutGet(Glut.GLUT_SCREEN_WIDTH);
            // height = Glut.glutGet(Glut.GLUT_SCREEN_HEIGHT) == 0 ? height : Glut.glutGet(Glut.GLUT_SCREEN_HEIGHT);
            //Glut.glutGameModeString(width + "x" + height + ":32@60");

            Glut.glutInitWindowSize(width, height);
            Glut.glutCreateWindow(titleName);

            Glut.glutReshapeFunc(OnReshape);

            Glut.glutKeyboardFunc(OnKeyDown);
            Glut.glutKeyboardUpFunc(OnKeyUp);

            Glut.glutMotionFunc(OnMouseMove);
            Glut.glutPassiveMotionFunc(OnMouseMove);

            Gl.Viewport(0, 0, width, height);
            //Glut.glutEnterGameMode();
            Glut.glutDisplayFunc(delegate () { });
            Glut.glutIdleFunc(MainLoop);

            Init();

            Glut.glutMainLoop();

            CleanUp();

        }

        private static void Init() {
            mandShader = new ShaderProgram(LoadFromFile("MandelbrotVertex.glsl"), LoadFromFile("MandelbrotFragment.glsl"));
            Console.WriteLine("Mandelbrot shader log: ");
            Console.WriteLine(mandShader.ProgramLog);
            Console.WriteLine();
            mandVertices = new VBO<Vector2>(new Vector2[] {
                new Vector2(-1,1),
                new Vector2(-1,-1),
                new Vector2(0,-1),
                new Vector2(0,1)
            });

            juliaShader = new ShaderProgram(LoadFromFile("JuliaVertex.glsl"), LoadFromFile("JuliaFragment.glsl"));
            Console.WriteLine("Julia shader log: ");
            Console.WriteLine(juliaShader.ProgramLog);
            Console.WriteLine();
            juliaVertices = new VBO<Vector2>(new Vector2[] {
                new Vector2(0,1),
                new Vector2(0,-1),
                new Vector2(1,-1),
                new Vector2(1,1)
            });

            elements = new VBO<int>(new int[] { 0, 1, 2, 3, 0 }, BufferTarget.ElementArrayBuffer);

            textShader = new ShaderProgram(LoadFromFile("TextVertex.glsl"), LoadFromFile("TextFragment.glsl"));
            Console.WriteLine("Text shader log: ");
            Console.WriteLine(mandShader.ProgramLog);
            Console.WriteLine();

            Font font = new Font("Constantia", 50, new Vector3(0, 0, 0.5));
            mandText = new Text(font);
            juliaText = new Text(font);
        }

        private static void ResetMandelbrot() {
            maxIter = 1500f;
            mandZoom = 2;
            mandRot = 0;
            mandPos = Vector2.Zero;
        }

        private static void ResetJulia() {
            maxIter = 1500f;
            juliaZoom = 2;
            juliaRot = 0;
            juliaPos = Vector2.Zero;
        }

        private static void MainLoop() {
            Gl.ClearColor(0, 0, 1, 1);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            watch.Stop();
            float curTime = watch.ElapsedMilliseconds;
            deltaTime = curTime - prevTime;
            prevTime = curTime;
            countTime += deltaTime;
            countTimeFrames++;
            if (countTime > 1000) {
                fps = countTimeFrames;
                countTime = 0;
                countTimeFrames = 0;
            }
            Glut.glutSetWindowTitle(titleName + " | Zoom: " + String.Format("{0:0.000000}", mandZoom) + " / " + String.Format("{0:0.000000}", juliaZoom) + " | Max Iterations: " + (int)maxIter + " | FPS: " + fps);
            watch.Start();

            float amt = deltaTime / 2000;
            if (mousePos.x < width / 2) {
                if (keys['w']) {
                    mandPos.x += amt * mandZoom * (float)Math.Sin(mandRot);
                    mandPos.y += amt * mandZoom * (float)Math.Cos(mandRot);
                }
                if (keys['s']) {
                    mandPos.x -= amt * mandZoom * (float)Math.Sin(mandRot);
                    mandPos.y -= amt * mandZoom * (float)Math.Cos(mandRot);
                }
                if (keys['d']) {
                    mandPos.x += amt * mandZoom * (float)Math.Cos(mandRot);
                    mandPos.y -= amt * mandZoom * (float)Math.Sin(mandRot);
                }
                if (keys['a']) {
                    mandPos.x -= amt * mandZoom * (float)Math.Cos(mandRot);
                    mandPos.y += amt * mandZoom * (float)Math.Sin(mandRot);
                }

                if (keys['i']) mandZoom = mandZoom * (float)Math.Pow(0.9995, deltaTime);
                if (keys['k']) mandZoom = mandZoom / (float)Math.Pow(0.9995, deltaTime);
                if (mandZoom < 0.0001f) mandZoom = 0.0001f;
                if (mandZoom > 5f) mandZoom = 5f;

                if (keys['j']) mandRot -= deltaTime / 4000;
                if (keys['l']) mandRot += deltaTime / 4000;

                if (keys['r']) ResetMandelbrot();
            } else {
                if (keys['w']) {
                    juliaPos.x += amt * juliaZoom * (float)Math.Sin(juliaRot);
                    juliaPos.y += amt * juliaZoom * (float)Math.Cos(juliaRot);
                }
                if (keys['s']) {
                    juliaPos.x -= amt * juliaZoom * (float)Math.Sin(juliaRot);
                    juliaPos.y -= amt * juliaZoom * (float)Math.Cos(juliaRot);
                }
                if (keys['d']) {
                    juliaPos.x += amt * juliaZoom * (float)Math.Cos(juliaRot);
                    juliaPos.y -= amt * juliaZoom * (float)Math.Sin(juliaRot);
                }
                if (keys['a']) {
                    juliaPos.x -= amt * juliaZoom * (float)Math.Cos(juliaRot);
                    juliaPos.y += amt * juliaZoom * (float)Math.Sin(juliaRot);
                }

                if (keys['i']) juliaZoom = juliaZoom * (float)Math.Pow(0.9995, deltaTime);
                if (keys['k']) juliaZoom = juliaZoom / (float)Math.Pow(0.9995, deltaTime);
                if (juliaZoom < 0.0001f) juliaZoom = 0.0002f;
                if (juliaZoom > 5f) juliaZoom = 5f;

                if (keys['j']) juliaRot -= deltaTime / 4000;
                if (keys['l']) juliaRot += deltaTime / 4000;

                if (keys['r']) ResetJulia();
            }

            float iterFactor = 0.2f;
            if (keys['y']) maxIter += deltaTime * iterFactor;
            if (keys['h']) maxIter -= deltaTime * iterFactor;
            if (maxIter < 1) maxIter = 1;

            //Console.WriteLine(mandPos);
            //mandText.SetText(String.Format("Zoom: {0:0.000000}, Position: {1}", mandZoom, CNumString(mandPos)));
            //juliaText.SetText(String.Format("Zoom: {0:0.000000}, Position: {1}", juliaZoom, CNumString(juliaPos)));

            Render();

            Glut.glutSwapBuffers();
        }

        private static string CNumString(Vector2 v) {
            StringBuilder s = new StringBuilder(String.Format("{0:0.000000}", v.x));
            if (v.y >= 0) {
                s = s.Append(" + ");
            } else {
                s = s.Append(" - ");
            }
            s = s.Append(String.Format("{0:0.000000}", Math.Abs(v.x)) + "i");
            return s.ToString();
        }

        private static void OnMouseMove(int x, int y) {
            mousePos.x = x;
            mousePos.y = y;
        }

        private static void Render() {
            mandShader.Use();
            mandShader["pos"].SetValue(mandPos);
            mandShader["aspectRatio"].SetValue((float)width / height);
            mandShader["zoom"].SetValue(mandZoom);
            mandShader["rot"].SetValue(Matrix4.CreateRotationZ(mandRot));
            mandShader["maxIter"].SetValue((int)maxIter);
            Gl.BindBufferToShaderAttribute(mandVertices, mandShader, "vpos");
            Gl.BindBuffer(elements);
            Gl.DrawElements(BeginMode.TriangleStrip, elements.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            juliaShader.Use();
            juliaShader["pos"].SetValue(juliaPos);
            juliaShader["aspectRatio"].SetValue((float)width / height);
            juliaShader["zoom"].SetValue(juliaZoom);
            juliaShader["rot"].SetValue(Matrix4.CreateRotationZ(juliaRot));
            juliaShader["maxIter"].SetValue((int)maxIter);
            juliaShader["vc"].SetValue(mandPos);
            Gl.BindBufferToShaderAttribute(juliaVertices, juliaShader, "vpos");
            Gl.BindBuffer(elements);
            Gl.DrawElements(BeginMode.TriangleStrip, elements.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            RenderText(mandText, new Vector2(-1, -1));
            RenderText(juliaText, new Vector2(0, -1));
        }

        private static void RenderText(Text text, Vector2 translation) {
            textShader.Use();
            TexModel model = text.model;
            textShader["trans"].SetValue(translation);
            textShader["colour"].SetValue(text.font.colour);
            Gl.BindBufferToShaderAttribute(model.vertices, textShader, "vpos");
            Gl.BindBufferToShaderAttribute(model.uvs, textShader, "vuv");
            Gl.BindTexture(model.texture);
            Gl.BindBuffer(model.elements);
            Gl.DrawElements(model.drawingMode, model.elements.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }

        private static void OnKeyDown(byte key, int x, int y) {
            key = (byte)char.ToLower((char)key);
            keys[key] = true;
            if (key == 27) Glut.glutLeaveMainLoop();
        }

        private static void OnKeyUp(byte key, int x, int y) {
            keys[key] = false;
        }

        private static void OnReshape(int width, int height) {
            Glut.glutSetWindowTitle("OIIII STOP RESIZING THE WINDOW - DODGY STUFF IS HAPPENING");
        }

        private static void CleanUp() {
            mandVertices.Dispose();
            elements.Dispose();
            mandShader.DisposeChildren = true;
            mandShader.Dispose();
        }

        private static string LoadFromFile(string source) {
            StreamReader reader = new StreamReader(source);
            StringBuilder sb = new StringBuilder();
            string s;
            while ((s = reader.ReadLine()) != null) {
                sb.Append(s);
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
