using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InternalCombustion;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;

namespace Firecraft
{
    public class Application
    {
        public List<IRenderable> renderQueue = new List<IRenderable>();
        public ICRenderer renderer;
        public ICWindow window;

        public Shader _drawShader;
        public Shader _unwPostProc;
        public PostProcShader _postShader;

        public ICCamera cam;

        public Mesh block;

        public int _screenTex;

        public void Load()
        {
            renderer = new ICRenderer(System.Drawing.Color.Blue, 800, 600);

            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);

            GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);

            _drawShader = new Shader("vertex.glsl", "fragment.glsl");
            _unwPostProc = new Shader("screenVertex.glsl", "postProc.glsl");
            _postShader = new PostProcShader(_unwPostProc);

            _drawShader.Use();

            block = Mesh.GenMeshCube();

            block.material = ICMaterial.Default;

            renderQueue.Add(block);

            cam = new ICCamera(new Vector3(0, 0, 0), window.Size.X / (float)window.Size.Y);

            renderer.shader = _postShader;

            _screenTex = renderer.GenRenderTexture();

            window.CursorGrabbed = true;
        }

        public void Render(double dt)
        {
            renderer.Clear();
            _drawShader.SetMatrix4("view", cam.GetViewMatrix());
            _drawShader.SetMatrix4("proj", cam.GetProjectionMatrix());

            _drawShader.Use();

            foreach (IRenderable rend in renderQueue)
                renderer.Draw(rend);

            renderer.DebindFBO();

            renderer.DrawScreen(_screenTex);
        }

        public void Update(double dt)
        {

        }

        void OnKeyDown(ICKeyEventArgs args)
        {
            if (args.key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.W)
                cam.Position += cam.Front * 1.5f * (float)args._dt;
            if (args.key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.S)
                cam.Position -= cam.Front * 1.5f * (float)args._dt;
            if (args.key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.A)
                cam.Position -= cam.Right * 1.5f * (float)args._dt;
            if (args.key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.D)
                cam.Position += cam.Right * 1.5f * (float)args._dt;
            if (args.key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape)
                window.Close();
        }

        bool firstMouse = true;
        float lastX = 0, lastY = 0;
        void OnMouseMove(ICMouseEventArgs e)
        {
            if (firstMouse)
            {
                lastX = e.position.X;
                lastY = e.position.Y;
                firstMouse = false;
            }
            float xOffset = (e.position.X - lastX);
            float yOffset = (lastY - e.position.Y);
            lastX = e.position.X;
            lastY = e.position.Y;
            float sensitivity = 0.5f;
            xOffset *= sensitivity;
            yOffset *= sensitivity;

            cam.Yaw += xOffset;
            cam.Pitch += yOffset;
        }

        public void Exit()
        {
            block.Delete();
        }

        public void Run()
        {
            window.Run();
        }

        public Application(string title)
        {
            ICWindow.Name = title;
            ICWindow.size = new OpenTK.Mathematics.Vector2i(800, 600);

            window = new ICWindow();
            window.Context.SwapInterval = 1;

            window._OnLoad += Load;
            window._OnRender += Render;
            window._OnUpdate += Update;
            window._OnExit += Exit;

            DevInput.onKeyPressed += OnKeyDown;
            DevInput.onMouseMove += OnMouseMove;
        }
    }
}
