using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

namespace ShipDamperSim
{
    /// <summary>
    /// Minimal OpenTK 3D visualization for ship, SPH fluid, and granular damper.
    /// </summary>
    public class Visualization3D : GameWindow
    {
        private readonly Simulation _sim;
        private int _step = 0;
        private List<Vector3> _fluidPositions = new();
        private List<Vector3> _damperPositions = new();
        // Modern OpenGL fields
        private int _shaderProgram;
        private int _vao;
        private int _vbo;
        private int _cubeVertexCount;

        public Visualization3D(Simulation sim)
            : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            _sim = sim;
            Title = "ShipDamperSim 3D Demo";
            Size = new Vector2i(1280, 720);
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(0.1f, 0.1f, 0.2f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            // Load and compile shaders
            string baseDir = AppContext.BaseDirectory;

            string vertPath = Path.Combine(baseDir, "shader.vert");
            string fragPath = Path.Combine(baseDir, "shader.frag");

            string vertSrc = File.ReadAllText(vertPath);
            string fragSrc = File.ReadAllText(fragPath);
            int vertShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertShader, vertSrc);
            GL.CompileShader(vertShader);
            int fragShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragShader, fragSrc);
            GL.CompileShader(fragShader);
            _shaderProgram = GL.CreateProgram();
            GL.AttachShader(_shaderProgram, vertShader);
            GL.AttachShader(_shaderProgram, fragShader);
            GL.LinkProgram(_shaderProgram);
            GL.DeleteShader(vertShader);
            GL.DeleteShader(fragShader);

            // Set up cube mesh (for ship and spheres)
            float[] cubeVertices = {
                // positions
                -0.5f, -0.5f,  0.5f,
                 0.5f, -0.5f,  0.5f,
                 0.5f,  0.5f,  0.5f,
                -0.5f,  0.5f,  0.5f,
                -0.5f, -0.5f, -0.5f,
                 0.5f, -0.5f, -0.5f,
                 0.5f,  0.5f, -0.5f,
                -0.5f,  0.5f, -0.5f
            };
            uint[] cubeIndices = {
                0, 1, 2, 2, 3, 0, // front
                1, 5, 6, 6, 2, 1, // right
                5, 4, 7, 7, 6, 5, // back
                4, 0, 3, 3, 7, 4, // left
                3, 2, 6, 6, 7, 3, // top
                4, 5, 1, 1, 0, 4  // bottom
            };
            _cubeVertexCount = cubeIndices.Length;
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            int ebo = GL.GenBuffer();
            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, cubeVertices.Length * sizeof(float), cubeVertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, cubeIndices.Length * sizeof(uint), cubeIndices, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.BindVertexArray(0);
        }

        protected override void OnRenderFrame(OpenTK.Windowing.Common.FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Kamera Z-akselilla, katsoo XY-tasoon (vesi vaakasuora)
            Matrix4 model = Matrix4.Identity;
            float camZ = 8.0f; // Kamera korkealle Z-akselille
            Vector3 camPos = new Vector3(0, 0, camZ);
            Vector3 target = new Vector3(0, 0, 0);
            Vector3 up = new Vector3(0, 1, 0); // Y-akseli ylös
            Matrix4 view = Matrix4.LookAt(camPos, target, up);
            float orthoWidth = 8.5f, orthoHeight = orthoWidth * Size.Y / (float)Size.X;
            Matrix4 proj = Matrix4.CreateOrthographic(orthoWidth, orthoHeight, 0.1f, 100f);
            Matrix4 mvp = model * view * proj;

            GL.UseProgram(_shaderProgram);
            int mvpLoc = GL.GetUniformLocation(_shaderProgram, "uMVP");
            int colorLoc = GL.GetUniformLocation(_shaderProgram, "uColor");

            // Draw ship (as a box) -- huomioi heave ja roll
            var ship = _sim.GetType().GetProperty("_ship", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(_sim);
            double y = 1.0, phi = 0.0;
            if (ship != null)
            {
                var yProp = ship.GetType().GetProperty("Y");
                var phiProp = ship.GetType().GetProperty("Phi");
                if (yProp != null) y = (double)yProp.GetValue(ship);
                if (phiProp != null) phi = (double)phiProp.GetValue(ship);
            }
            Matrix4 shipModel = Matrix4.CreateScale(1.0f, 1.0f, 0.4f)
                * Matrix4.CreateRotationZ((float)phi)
                * Matrix4.CreateTranslation(0.0f, (float)y, 0.0f);
            Matrix4 shipMVP = shipModel * view * proj;
            GL.UniformMatrix4(mvpLoc, false, ref shipMVP);
            GL.Uniform3(colorLoc, new Vector3(0.7f, 0.7f, 0.7f));
            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, _cubeVertexCount, DrawElementsType.UnsignedInt, 0);

            // Draw SPH fluid particles (as blue cubes for now)
            GL.Uniform3(colorLoc, new Vector3(0.2f, 0.4f, 1.0f));
            foreach (var p in _fluidPositions)
            {
                Matrix4 particleModel = Matrix4.CreateScale(0.05f) * Matrix4.CreateTranslation(p);
                Matrix4 particleMVP = particleModel * view * proj;
                GL.UniformMatrix4(mvpLoc, false, ref particleMVP);
                GL.DrawElements(PrimitiveType.Triangles, _cubeVertexCount, DrawElementsType.UnsignedInt, 0);
            }
            // Draw damper particles (as orange cubes)
            GL.Uniform3(colorLoc, new Vector3(1.0f, 0.5f, 0.1f));
            foreach (var p in _damperPositions)
            {
                Matrix4 particleModel = Matrix4.CreateScale(0.05f) * Matrix4.CreateTranslation(p);
                Matrix4 particleMVP = particleModel * view * proj;
                GL.UniformMatrix4(mvpLoc, false, ref particleMVP);
                GL.DrawElements(PrimitiveType.Triangles, _cubeVertexCount, DrawElementsType.UnsignedInt, 0);
            }
            GL.BindVertexArray(0);
            GL.UseProgram(0);
            SwapBuffers();
        }

        protected override void OnUpdateFrame(OpenTK.Windowing.Common.FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            // Step simulation and update positions
            _sim.StepOnce();
            _fluidPositions.Clear();
            foreach (var p in _sim.FluidParticles)
                _fluidPositions.Add(new Vector3((float)p.Position.X, (float)p.Position.Y, (float)p.Position.Z));
            // Update granular damper positions
            _damperPositions.Clear();
            var damper = _sim.DamperParticles;
            if (damper != null)
            {
                foreach (var p in damper)
                    _damperPositions.Add(new Vector3((float)p.X, (float)p.Y, (float)p.Z));
            }
        }

        // DrawBox and DrawSphere removed: now handled by instanced cube rendering
    }
}
