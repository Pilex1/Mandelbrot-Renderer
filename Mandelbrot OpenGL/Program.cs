using System;
using OpenGL;
using Tao.FreeGlut;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Mandelbrot {
    static class Program {

        static int width = 1200, height = 800;
        const string titleName = "Mandelbrot Renderer - Copyright Alex Tan 2016";

        static ShaderProgram shader;
        static VBO<Vector2> vertices;
        static VBO<int> elements;

        static bool[] keys = new bool[255];
        static Vector2 pos = new Vector2(0, 0);
        static float zoom = 2;
        static float angle = 0;
        static float maxIter = 1500f;

        static float prevTime;
        static float deltaTime;
        static float countTime;
        static int countTimeFrames;
        static int fps;
        static Stopwatch watch = new Stopwatch();


        static void Main() {
            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);

            // width = Glut.glutGet(Glut.GLUT_SCREEN_WIDTH) == 0 ? width : Glut.glutGet(Glut.GLUT_SCREEN_WIDTH);
            // height = Glut.glutGet(Glut.GLUT_SCREEN_HEIGHT) == 0 ? height : Glut.glutGet(Glut.GLUT_SCREEN_HEIGHT);
            //Glut.glutGameModeString(width + "x" + height + ":32@60");

            Glut.glutInitWindowSize(width, height);
            Glut.glutCreateWindow(titleName);

            Glut.glutKeyboardFunc(OnKeyDown);
            Glut.glutKeyboardUpFunc(OnKeyUp);

            Gl.Viewport(0, 0, width, height);
            //Glut.glutEnterGameMode();
            Glut.glutDisplayFunc(delegate () { });
            Glut.glutIdleFunc(MainGameLoop);

            Init();

            Glut.glutMainLoop();

            CleanUp();

        }

        private static void Init() {
            shader = new ShaderProgram(LoadFromFile("Vertex.glsl"), LoadFromFile("Fragment.glsl"));
            Debug.WriteLine("Shader Log: ");
            Debug.WriteLine(shader.ProgramLog);
            shader.Use();

            vertices = new VBO<Vector2>(new Vector2[] {
                new Vector2(-1,1),
                new Vector2(-1,-1),
                new Vector2(1,-1),
                new Vector2(1,1)
            });
            elements = new VBO<int>(new int[] { 0, 1, 2, 3, 0 }, BufferTarget.ElementArrayBuffer);
        }

        private static void MainGameLoop() {
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
            Glut.glutSetWindowTitle(titleName + " | Zoom: " + String.Format("{0:0.000000}", zoom) + " | Max Iterations: " + (int)maxIter + " | FPS: " + fps);
            watch.Start();

            float amt = deltaTime / 2000;
            if (keys['w']) {
                pos.x += amt * zoom * (float)Math.Sin(angle);
                pos.y += amt * zoom * (float)Math.Cos(angle);
            }
            if (keys['s']) {
                pos.x -= amt * zoom * (float)Math.Sin(angle);
                pos.y -= amt * zoom * (float)Math.Cos(angle);
            }
            if (keys['d']) {
                pos.x += amt * zoom * (float)Math.Cos(angle);
                pos.y -= amt * zoom * (float)Math.Sin(angle);
            }
            if (keys['a']) {
                pos.x -= amt * zoom * (float)Math.Cos(angle);
                pos.y += amt * zoom * (float)Math.Sin(angle);
            }

            if (keys['i']) zoom = zoom * (float)Math.Pow(0.9995, deltaTime);
            if (keys['k']) zoom = zoom / (float)Math.Pow(0.9995, deltaTime);
            if (zoom < 0.0001f) zoom = 0.0001f;
            if (zoom > 5f) zoom = 5f;

            if (keys['j']) angle -= deltaTime / 4000;
            if (keys['l']) angle += deltaTime / 4000;

            float iterFactor = 0.2f;
            if (keys['y']) maxIter += deltaTime * iterFactor;
            if (keys['h']) maxIter -= deltaTime * iterFactor;

            Render();

            Glut.glutSwapBuffers();
        }

        private static void Render() {
            shader.Use();
            shader["pos"].SetValue(pos);
            shader["aspectRatio"].SetValue((float)width / height);
            shader["zoom"].SetValue(zoom);
            shader["rot"].SetValue(Matrix4.CreateRotationZ(angle));
            shader["maxIter"].SetValue((int)maxIter);
            Gl.BindBufferToShaderAttribute(vertices, shader, "vpos");
            Gl.BindBuffer(elements);
            Gl.DrawElements(BeginMode.TriangleStrip, elements.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }

        private static void OnKeyDown(byte key, int x, int y) {
            keys[key] = true;
            if (key == 27) Glut.glutLeaveMainLoop();
        }

        private static void OnKeyUp(byte key, int x, int y) {
            keys[key] = false;
        }

        private static void CleanUp() {
            vertices.Dispose();
            elements.Dispose();
            shader.DisposeChildren = true;
            shader.Dispose();
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
