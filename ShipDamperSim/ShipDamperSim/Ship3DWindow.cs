using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;

namespace ShipDamperSim
{
    public class Ship3DWindow : GameWindow
    {
        public Ship3DWindow() : base(
            GameWindowSettings.Default,
            CreateNativeWindowSettings())
        {
        }

        private static NativeWindowSettings CreateNativeWindowSettings()
        {
            return new NativeWindowSettings
            {
                Size = new Vector2i(800, 600),
                Title = "ShipDamperSim 3D (OpenTK)",
                Profile = ContextProfile.Compatability
            };
        }

        // Fysiikkamuuttujat
        private float rollAngle = 0f; // rad
        private float rollVelocity = 0f; // rad/s
        private float time = 0f;

        // Laivan ominaisuudet
        private const float Inertia = 1000f; // kg m^2
        private const float Damping = 400f; // Nms/rad (suurempi vaimennus)
        private const float Restoring = 5000f; // Nm/rad
        private float heavePos = 0f; // m (laivan painopisteen korkeus)
        private float heaveVel = 0f; // m/s
        // Laivan mitat ja massa fysiikan mukaan
        private const float draft = 0.3f; // m
        private const float area = 1.4f * 0.6f; // m^2
        private const float rho = 1000f; // kg/m^3
        // Massan on oltava täsmälleen syrjäytetyn veden massa tasapainossa
        private const float Mass = rho * area * draft; // kg, tasapainossa
        private const float HeaveSpring = 5e4f; // Pehmeämpi palautus (N/m)
        private const float HeaveDamping = 35000f; // Paljon suurempi vaimennus
        private const float AirDamping = 35000f; // Ilmavaimennus veden yläpuolella

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(0.2f, 0.3f, 0.5f, 1.0f);
            GL.Enable(EnableCap.DepthTest);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Debug: print to console every 60 frames
            if (((int)(time * 60)) % 60 == 0)
            {
                Console.WriteLine($"OnRenderFrame: time={time:F2}");
            }

            // Kamera
            Matrix4 model = Matrix4.Identity;
            Matrix4 view = Matrix4.LookAt(new Vector3(0, 2, 7), new Vector3(0, 0, 0), Vector3.UnitY);
            Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60), Size.X / (float)Size.Y, 0.1f, 100f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref proj);
            GL.MatrixMode(MatrixMode.Modelview);
            Matrix4 mv = view * model;
            GL.LoadMatrix(ref mv);

            // Testi: piirrä valkoinen laatikko keskelle
            GL.Color3(1.0f, 1.0f, 1.0f);
            DrawBox(-0.5f, 0, -0.5f, 0.5f, 0.5f, 0.5f);

            // Piirrä kirkas aallokko
            float waveAmp = 1.0f; // Suurempi amplitudi
            float waveLen = 2.5f;
            float waveFreq = 0.7f;
            float waveSpeed = 0.7f;
            float t = time;
            GL.Color3(0.0f, 1.0f, 1.0f); // Kirkas syaani
            GL.Begin(PrimitiveType.Quads);
            for (float x = -4; x < 4; x += 0.4f)
            {
                for (float z = -4; z < 4; z += 0.4f)
                {
                    float y1 = waveAmp * (float)Math.Sin(2 * Math.PI * (x / waveLen + waveFreq * t)) * (float)Math.Cos(2 * Math.PI * (z / waveLen + 0.3f * t));
                    float y2 = waveAmp * (float)Math.Sin(2 * Math.PI * ((x + 0.4f) / waveLen + waveFreq * t)) * (float)Math.Cos(2 * Math.PI * (z / waveLen + 0.3f * t));
                    float y3 = waveAmp * (float)Math.Sin(2 * Math.PI * ((x + 0.4f) / waveLen + waveFreq * t)) * (float)Math.Cos(2 * Math.PI * ((z + 0.4f) / waveLen + 0.3f * t));
                    float y4 = waveAmp * (float)Math.Sin(2 * Math.PI * (x / waveLen + waveFreq * t)) * (float)Math.Cos(2 * Math.PI * ((z + 0.4f) / waveLen + 0.3f * t));
                    if (((int)(time * 60)) % 60 == 0 && x == 0 && z == 0)
                        Console.WriteLine($"wave quad: ({x},{y1},{z}) ({x + 0.4f},{y2},{z}) ({x + 0.4f},{y3},{z + 0.4f}) ({x},{y4},{z + 0.4f})");
                    GL.Vertex3(x, y1, z);
                    GL.Vertex3(x + 0.4f, y2, z);
                    GL.Vertex3(x + 0.4f, y3, z + 0.4f);
                    GL.Vertex3(x, y4, z + 0.4f);
                }
            }
            GL.End();

            // Laivan paikka (0,0)
            float shipX = 0f, shipZ = 0f;
            float waveHeight = waveAmp * (float)Math.Sin(2 * Math.PI * (shipX / waveLen + waveFreq * t)) * (float)Math.Cos(2 * Math.PI * (shipZ / waveLen + 0.3f * t));

            // Yliopistotason fysiikka: laivan pohjan korkeus ja noste
            float shipBottom = heavePos - draft / 2f;
            float waterSurface = waveHeight;
            float upotus = Math.Clamp(waterSurface + draft / 2f - heavePos, 0, draft); // Kuinka paljon laivasta on veden alla (max draft)
            float upotusSuhde = upotus / draft; // 0...1
            float buoyancy = (upotus > 0) ? rho * 9.81f * area * upotus : 0f; // Noste vain upotetulle osalle
            float weight = Mass * 9.81f; // Paino
            float damping = -HeaveDamping * heaveVel;
            // Ilmavaimennus, jos laiva on kokonaan ilmassa
            if (upotus <= 0 && heaveVel > 0)
                damping += -AirDamping * heaveVel;
            // Hydrostaattinen palauttava voima: pyrkii palauttamaan laivan tasapainoon
            float equilibriumHeave = waterSurface + draft / 2f - draft * (weight / (rho * 9.81f * area));
            float springForce = -HeaveSpring * (heavePos - equilibriumHeave);
            float netForce = buoyancy + damping + springForce - weight;
            float heaveAcc = netForce / Mass;
            heaveVel += heaveAcc * (float)args.Time;
            // Rajoitetaan maksiminopeutta ilmassa, jotta laiva ei hyppää liian korkealle
            if (upotus <= 0 && heaveVel > 2.0f) heaveVel = 2.0f;
            heavePos += heaveVel * (float)args.Time;
            // Pidetään laiva veden pinnan yläpuolella (ei uppoa rajatta)
            if (heavePos < waterSurface - draft) heavePos = waterSurface - draft;
            if (heavePos > waterSurface + draft) heavePos = waterSurface + draft;
            // Jos laiva on kokonaan veden yläpuolella ja nopeus on alaspäin, vaimennetaan roiskeita
            if (upotus <= 0 && heaveVel < 0)
            {
                heaveVel *= 0.2f; // Vaimenna putoamisnopeutta entistä enemmän
            }
            float dYdX = waveAmp * (2 * (float)Math.PI / waveLen) * (float)Math.Cos(2 * Math.PI * (shipX / waveLen + waveFreq * t)) * (float)Math.Cos(2 * Math.PI * (shipZ / waveLen + 0.3f * t));

            // Fysiikkapohjainen rullaus: Euler-integraatio
            float externalMoment = 1000f * dYdX; // Pienempi ulkoinen momentti
            float hydro = -Damping * rollVelocity;
            float restoring = -Restoring * rollAngle;
            float totalMoment = hydro + restoring + externalMoment;
            float angularAcc = totalMoment / Inertia;
            rollVelocity += angularAcc * (float)args.Time;
            rollAngle += rollVelocity * (float)args.Time;

            if (((int)(time * 60)) % 60 == 0)
                Console.WriteLine($"ship pos: (0,{heavePos},0) roll={rollAngle}");

            GL.PushMatrix();
            GL.Translate(shipX, heavePos + 1.0f, shipZ);
            GL.Rotate(rollAngle * 180 / (float)Math.PI, 0, 0, 1);
            GL.Color3(1.0f, 0.5f, 0.0f); // Oranssi laiva
            DrawBox(-0.7f, 0, -0.3f, 0.7f, 0.3f, 0.3f);
            GL.Color3(1.0f, 1.0f, 0.5f); // Keltainen kansi
            DrawBox(-0.7f, 0.28f, -0.3f, 0.7f, 0.35f, 0.3f);
            GL.PopMatrix();

            SwapBuffers();
            time += (float)args.Time * waveSpeed;
        }

        // Piirtää boxin (laivan runko/kansi)
        private void DrawBox(float x0, float y0, float z0, float x1, float y1, float z1)
        {
            // 6 sivua
            GL.Begin(PrimitiveType.Quads);
            // etu
            GL.Vertex3(x0, y0, z1); GL.Vertex3(x1, y0, z1); GL.Vertex3(x1, y1, z1); GL.Vertex3(x0, y1, z1);
            // taka
            GL.Vertex3(x0, y0, z0); GL.Vertex3(x1, y0, z0); GL.Vertex3(x1, y1, z0); GL.Vertex3(x0, y1, z0);
            // vasen
            GL.Vertex3(x0, y0, z0); GL.Vertex3(x0, y0, z1); GL.Vertex3(x0, y1, z1); GL.Vertex3(x0, y1, z0);
            // oikea
            GL.Vertex3(x1, y0, z0); GL.Vertex3(x1, y0, z1); GL.Vertex3(x1, y1, z1); GL.Vertex3(x1, y1, z0);
            // ylä
            GL.Vertex3(x0, y1, z0); GL.Vertex3(x1, y1, z0); GL.Vertex3(x1, y1, z1); GL.Vertex3(x0, y1, z1);
            // ala
            GL.Vertex3(x0, y0, z0); GL.Vertex3(x1, y0, z0); GL.Vertex3(x1, y0, z1); GL.Vertex3(x0, y0, z1);
            GL.End();
        }
    }
}
    
