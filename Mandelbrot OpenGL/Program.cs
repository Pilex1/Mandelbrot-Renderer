using System;
using OpenGL;
using Tao.FreeGlut;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Mandelbrot {
    static class Program {

        static int width = 1200, height = 800;
        const string titleName = "Mandelbrot Renderer";

        static ShaderProgram shader;
        static VBO<Vector2> vertices;
        static VBO<int> elements;

        static bool[] keys = new bool[255];
        static Vector2 pos = new Vector2(0, 0);
        static float zoom = 1;

        static float prevTime;
        static float deltaTime;
        static Stopwatch watch = new Stopwatch();


        static void Main() {
            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
            Glut.glutInitWindowSize(width, height);
            Glut.glutCreateWindow(titleName);
            //Glut.glutGameModeString(Width+"x"+Height+":32@60");

            //width = Glut.glutGet(Glut.GLUT_SCREEN_WIDTH) == 0 ? width : Glut.glutGet(Glut.GLUT_SCREEN_WIDTH);
           // height = Glut.glutGet(Glut.GLUT_SCREEN_HEIGHT) == 0 ? height : Glut.glutGet(Glut.GLUT_SCREEN_WIDTH);


            Glut.glutKeyboardFunc(OnKeyDown);
            Glut.glutKeyboardUpFunc(OnKeyUp);
            Glut.glutMouseWheelFunc(OnMouseWheel);

            Gl.Viewport(0, 0, width, height);
            Glut.glutDisplayFunc(delegate () { });
            Glut.glutIdleFunc(MainGameLoop);

            Console.WriteLine(Glut.glutExtensionSupported(" ARB_gpu_shader_fp64"));

            Init();

            Glut.glutMainLoop();

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
            int fps = (int)(1000f / deltaTime);
            if (fps < int.MaxValue && fps > int.MinValue) Glut.glutSetWindowTitle(titleName + " FPS: " + fps);
            watch.Start();

            float amt = deltaTime / 1000;

            if (keys['w']) pos.y += amt * zoom;
            if (keys['s']) pos.y -= amt * zoom;
            if (keys['d']) pos.x += amt * zoom;
            if (keys['a']) pos.x -= amt * zoom;

            Render();

            Glut.glutSwapBuffers();
        }

        private static void Render() {
            shader.Use();
            shader["pos"].SetValue(pos);
            shader["aspectRatio"].SetValue((float)width / height);
            shader["zoom"].SetValue(zoom);
            Gl.BindBufferToShaderAttribute(vertices, shader, "vpos");
            Gl.BindBuffer(elements);
            Gl.DrawElements(BeginMode.TriangleStrip, elements.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }

        private static void OnKeyDown(byte key, int x, int y) {
            keys[key] = true;
        }

        private static void OnKeyUp(byte key, int x, int y) {
            keys[key] = false;
        }

        private static void OnMouseWheel(int wheel, int direction, int x, int y) {
            float amount = 0.95f;
            if (direction < 0) zoom /= amount;
            else zoom *= amount;
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
