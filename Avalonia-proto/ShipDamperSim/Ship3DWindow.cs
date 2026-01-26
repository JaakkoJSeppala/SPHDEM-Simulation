using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Mathematics;
using System;
using ShipDamperSim.Core;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.IO;

namespace ShipDamperSim
{
    public class Ship3DWindow : GameWindow
    {
        private const float VIS_SCALE = 8.0f;
        // Yleinen laatikkofunktio
        private void DrawBox(float width, float height, float depth)
        {
            float w = width * 0.5f;
            float h = height * 0.5f;
            float d = depth * 0.5f;
            GL.Begin(PrimitiveType.Quads);
            // Top
            GL.Vertex3(-w, h, -d); GL.Vertex3(w, h, -d); GL.Vertex3(w, h, d); GL.Vertex3(-w, h, d);
            // Bottom
            GL.Vertex3(-w, -h, -d); GL.Vertex3(-w, -h, d); GL.Vertex3(w, -h, d); GL.Vertex3(w, -h, -d);
            // Front
            GL.Vertex3(-w, -h, d); GL.Vertex3(-w, h, d); GL.Vertex3(w, h, d); GL.Vertex3(w, -h, d);
            // Back
            GL.Vertex3(-w, -h, -d); GL.Vertex3(w, -h, -d); GL.Vertex3(w, h, -d); GL.Vertex3(-w, h, -d);
            // Left
            GL.Vertex3(-w, -h, -d); GL.Vertex3(-w, h, -d); GL.Vertex3(-w, h, d); GL.Vertex3(-w, -h, d);
            // Right
            GL.Vertex3(w, -h, -d); GL.Vertex3(w, -h, d); GL.Vertex3(w, h, d); GL.Vertex3(w, h, -d);
            GL.End();
        }

        // Laivan runko: yksi vaalea laatikko
        private void DrawShipHull(double roll, double heave)
        {
            GL.PushMatrix();
            GL.Translate(0f, (float)heave, 0f);
            GL.Rotate(MathHelper.RadiansToDegrees((float)roll), 0f, 0f, 1f);
            GL.Color3(0.85f, 0.85f, 0.85f);
            DrawBox(8.0f, 1.2f, 2.5f);
            GL.PopMatrix();
        }
        private Simulation simulation;
        private double simDt = 0.01;
        private bool damperOn = true;

        public Ship3DWindow()
            : base(
                GameWindowSettings.Default,
                new NativeWindowSettings()
                {
                    Title = "ShipDamperSim 3D (OpenTK)",
                    ClientSize = new Vector2i(800, 600),
                    APIVersion = new System.Version(3, 3),
                    Profile = ContextProfile.Compatability
                }
            )
        {
            simulation = new Simulation(8, 2, 4, 8);
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(0.2f, 0.3f, 0.4f, 1.0f);
            GL.Enable(EnableCap.DepthTest);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            // Damper ON/OFF logiikka
            simulation.Dem.Enabled = damperOn;
            simulation.Step(simDt);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Perspektiiviprojektio ja kamera
            GL.Viewport(0, 0, (int)Size.X, (int)Size.Y);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60f), Size.X / (float)Size.Y, 0.1f, 100f);
            GL.LoadMatrix(ref projection);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            Matrix4 view = Matrix4.LookAt(new Vector3(0, 5, 15), Vector3.Zero, Vector3.UnitY);
            GL.LoadMatrix(ref view);

            // Piirrä rakeet (DEM-partikkelit)
            DrawDamperParticles(view);

            // Laivan runko (harmaa, kerran per frame)
            GL.Color3(0.9f, 0.9f, 0.9f);
            DrawShipHull(0.5f, 0f);

            SwapBuffers();
        }

        // Piirrä vapaan pinnan aaltokäyrä
        private void DrawFluidSurface(Matrix4 projection, Matrix4 view, double heave)
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref view);
            GL.Color3(0.2f, 0.4f, 0.9f); // sininen
            GL.Begin(PrimitiveType.LineStrip);
            double H = heave;
            double A = 0.08;
            double k = 2.0;
            double omega = 2.0;
            for (double x = -2.0; x <= 2.0; x += 0.05)
            {
                double y = H + A * Math.Sin(k * x - omega * simulation.Time);
                GL.Vertex3(x, y, 0.0);
            }
            GL.End();
        }

        // Piirrä laiva
        private void DrawShip(Matrix4 view, double rollAngle, double heave)
        {
            // Piirrä laiva harmaana laatikkona omalla transformilla
            GL.PushMatrix();
            GL.LoadMatrix(ref view);
            GL.Translate(0f, (float)heave, 0f);
            GL.Rotate(MathHelper.RadiansToDegrees((float)(rollAngle * 1.2)), 0f, 0f, 1f);
            GL.Color3(0.85f, 0.85f, 0.85f); // vaalea harmaa
            GL.Begin(PrimitiveType.Quads);
            GL.Vertex3(-1.5, -0.3, -0.5);
            GL.Vertex3(1.5, -0.3, -0.5);
            GL.Vertex3(1.5, 0.3, -0.5);
            GL.Vertex3(-1.5, 0.3, -0.5);
            GL.Vertex3(-1.5, -0.3, 0.5);
            GL.Vertex3(1.5, -0.3, 0.5);
            GL.Vertex3(1.5, 0.3, 0.5);
            GL.Vertex3(-1.5, 0.3, 0.5);
            GL.End();
            GL.PopMatrix();
        }

        // Piirrä damperin partikkelit
        private void DrawDamperParticles(Matrix4 view)
        {
            GL.LoadMatrix(ref view);
            GL.PointSize(10f);
            GL.Color3(1f, 0f, 0f); // punainen
            foreach (var p in simulation.Dem.Particles)
            {
                // Piirrä riittävän iso punainen kuutio
                GL.PushMatrix();
                GL.Translate(p.Position * VIS_SCALE);
                DrawBox(0.3f, 0.3f, 0.3f); // testaa isolla koolla
                GL.PopMatrix();
            }
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Keys.D)
            {
                damperOn = !damperOn;
                Title = $"ShipDamperSim 3D (Damper {(damperOn ? "ON" : "OFF")})";
            }
        }
    }
}