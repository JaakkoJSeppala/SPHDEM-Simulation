using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;
using ShipHydroSim.Core.SPH;
using ShipHydroSim.Core.DEM;
using System;
using System.Collections.Generic;

namespace ShipHydroSim.App.Controls;

public class ParticleViewport : Control
{
    private IReadOnlyList<Particle>? _particles;
    private IReadOnlyList<RigidBody>? _rigidBodies;
    private float _cameraYaw = 45f;
    private float _cameraPitch = 30f;
    private float _cameraZoom = 50f;
    
    public void UpdateScene(IReadOnlyList<Particle> particles, IReadOnlyList<RigidBody> rigidBodies)
    {
        _particles = particles;
        _rigidBodies = rigidBodies;
        InvalidateVisual();
    }
    
    public void RotateCamera(float deltaYaw, float deltaPitch)
    {
        _cameraYaw += deltaYaw;
        _cameraPitch = Math.Clamp(_cameraPitch + deltaPitch, -85f, 85f);
        InvalidateVisual();
    }
    
    public void ZoomCamera(float delta)
    {
        _cameraZoom = Math.Clamp(_cameraZoom + delta, 20f, 150f);
        InvalidateVisual();
    }
    
    public override void Render(DrawingContext context)
    {
        base.Render(context);
        
        if (_particles == null || _particles.Count == 0) return;
        
        var bounds = Bounds;
        float centerX = (float)bounds.Width / 2;
        float centerY = (float)bounds.Height / 2;
        
        context.FillRectangle(new SolidColorBrush(Color.FromRgb(20, 20, 30)), bounds);
        context.Custom(new ParticleRenderOp(_particles, _rigidBodies, centerX, centerY, _cameraYaw, _cameraPitch, _cameraZoom));
    }
    
    private class ParticleRenderOp : ICustomDrawOperation
    {
        private readonly IReadOnlyList<Particle> _particles;
        private readonly IReadOnlyList<RigidBody>? _rigidBodies;
        private readonly float _cx, _cy, _yaw, _pitch, _zoom;
        
        public ParticleRenderOp(IReadOnlyList<Particle> particles, IReadOnlyList<RigidBody>? rigidBodies, float cx, float cy, float yaw, float pitch, float zoom)
        {
            _particles = particles;
            _rigidBodies = rigidBodies;
            _cx = cx;
            _cy = cy;
            _yaw = yaw;
            _pitch = pitch;
            _zoom = zoom;
        }
        
        public Rect Bounds => new Rect(double.NegativeInfinity, double.NegativeInfinity, double.PositiveInfinity, double.PositiveInfinity);
        public bool HitTest(Point p) => false;
        public bool Equals(ICustomDrawOperation? other) => false;
        public void Dispose() { }
        
        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature == null) return;
            
            using var lease = leaseFeature.Lease();
            var canvas = lease.SkCanvas;
            
            float yawRad = _yaw * MathF.PI / 180f;
            float pitchRad = _pitch * MathF.PI / 180f;
            
            var sorted = new List<(Particle p, float depth)>();
            
            foreach (var p in _particles)
            {
                if (p.IsBoundary) continue;
                
                float x = (float)p.Position.X - 5f;
                float y = (float)p.Position.Y - 3f;
                float z = (float)p.Position.Z - 5f;
                
                float rotZ = -x * MathF.Sin(yawRad) + z * MathF.Cos(yawRad);
                float depth = y * MathF.Sin(pitchRad) + rotZ * MathF.Cos(pitchRad);
                
                sorted.Add((p, depth));
            }
            
            sorted.Sort((a, b) => a.depth.CompareTo(b.depth));
            
            foreach (var (p, depth) in sorted)
            {
                float x = (float)p.Position.X - 5f;
                float y = (float)p.Position.Y - 3f;
                float z = (float)p.Position.Z - 5f;
                
                float rotX = x * MathF.Cos(yawRad) + z * MathF.Sin(yawRad);
                float rotZ = -x * MathF.Sin(yawRad) + z * MathF.Cos(yawRad);
                float rotY = y * MathF.Cos(pitchRad) - rotZ * MathF.Sin(pitchRad);
                
                float screenX = _cx + rotX * _zoom;
                float screenY = _cy - rotY * _zoom;
                
                float speed = (float)p.Velocity.Length;
                byte r = (byte)Math.Clamp(speed / 5f * 255, 50, 255);
                byte g = (byte)Math.Clamp((1f - speed / 5f) * 255, 75, 200);
                
                var paint = new SKPaint
                {
                    Color = new SKColor(r, g, 200),
                    IsAntialias = true
                };
                
                canvas.DrawCircle(screenX, screenY, 4f, paint);
            }
            
            // Draw axes
            DrawAxis(canvas, _cx, _cy, 1, 0, 0, _zoom * 2, yawRad, pitchRad, SKColors.Red);
            DrawAxis(canvas, _cx, _cy, 0, 1, 0, _zoom * 2, yawRad, pitchRad, SKColors.Green);
            DrawAxis(canvas, _cx, _cy, 0, 0, 1, _zoom * 2, yawRad, pitchRad, SKColors.Blue);
            
            // Draw rigid bodies (ships)
            if (_rigidBodies != null)
            {
                foreach (var body in _rigidBodies)
                {
                    DrawRigidBody(canvas, body, yawRad, pitchRad);
                }
            }
        }
        
        private void DrawRigidBody(SKCanvas canvas, RigidBody body, float yawRad, float pitchRad)
        {
            // Draw ship as wireframe box
            if (body.Shape is Core.Ships.BoxShape box)
            {
                float l = (float)box.Length;
                float h = (float)box.Height;
                float w = (float)box.Width;
                
                // Box vertices (relative to center)
                var vertices = new[]
                {
                    new { x = -l/2, y = -h/2, z = -w/2 },
                    new { x =  l/2, y = -h/2, z = -w/2 },
                    new { x =  l/2, y =  h/2, z = -w/2 },
                    new { x = -l/2, y =  h/2, z = -w/2 },
                    new { x = -l/2, y = -h/2, z =  w/2 },
                    new { x =  l/2, y = -h/2, z =  w/2 },
                    new { x =  l/2, y =  h/2, z =  w/2 },
                    new { x = -l/2, y =  h/2, z =  w/2 }
                };
                
                // Transform and project vertices
                var projected = new SKPoint[8];
                // Body orientation quaternion rotation
                var q = body.Orientation; // assume unit quaternion
                for (int i = 0; i < 8; i++)
                {
                    // local vertex
                    float vx = (float)vertices[i].x;
                    float vy = (float)vertices[i].y;
                    float vz = (float)vertices[i].z;
                    // rotate by quaternion: v' = q * v * q^{-1}
                    float qw = (float)q.W; float qx = (float)q.X; float qy = (float)q.Y; float qz = (float)q.Z;
                    // t = 2 * cross(q.xyz, v)
                    float tx = 2f * (qy * vz - qz * vy);
                    float ty = 2f * (qz * vx - qx * vz);
                    float tz = 2f * (qx * vy - qy * vx);
                    // v' = v + q.w * t + cross(q.xyz, t)
                    float rx = vx + qw * tx + (qy * tz - qz * ty);
                    float ry = vy + qw * ty + (qz * tx - qx * tz);
                    float rz = vz + qw * tz + (qx * ty - qy * tx);
                    // world position
                    float x = rx + (float)body.Position.X - 5f;
                    float y = ry + (float)body.Position.Y - 3f;
                    float z = rz + (float)body.Position.Z - 5f;
                    // camera rotation
                    float camX = x * MathF.Cos(yawRad) + z * MathF.Sin(yawRad);
                    float camZ = -x * MathF.Sin(yawRad) + z * MathF.Cos(yawRad);
                    float camY = y * MathF.Cos(pitchRad) - camZ * MathF.Sin(pitchRad);
                    projected[i] = new SKPoint(_cx + camX * _zoom, _cy - camY * _zoom);
                }
                
                // Draw edges
                var paint = new SKPaint { Color = SKColors.Yellow, StrokeWidth = 2, Style = SKPaintStyle.Stroke, IsAntialias = true };
                // emphasize roll/pitch by coloring edges differently if angular velocity present
                if (body.AngularVelocity.Length > 0.01)
                {
                    paint.Color = new SKColor(255, 180, 80);
                }
                
                // Bottom face
                canvas.DrawLine(projected[0], projected[1], paint);
                canvas.DrawLine(projected[1], projected[2], paint);
                canvas.DrawLine(projected[2], projected[3], paint);
                canvas.DrawLine(projected[3], projected[0], paint);
                
                // Top face
                canvas.DrawLine(projected[4], projected[5], paint);
                canvas.DrawLine(projected[5], projected[6], paint);
                canvas.DrawLine(projected[6], projected[7], paint);
                canvas.DrawLine(projected[7], projected[4], paint);
                
                // Vertical edges
                canvas.DrawLine(projected[0], projected[4], paint);
                canvas.DrawLine(projected[1], projected[5], paint);
                canvas.DrawLine(projected[2], projected[6], paint);
                canvas.DrawLine(projected[3], projected[7], paint);
            }
        }
        
        private void DrawAxis(SKCanvas c, float cx, float cy, float dx, float dy, float dz, float len, float yaw, float pitch, SKColor col)
        {
            float rx = (dx * MathF.Cos(yaw) + dz * MathF.Sin(yaw)) * len;
            float rz = (-dx * MathF.Sin(yaw) + dz * MathF.Cos(yaw)) * len;
            float ry = (dy * MathF.Cos(pitch) - rz * MathF.Sin(pitch)) * len;
            
            c.DrawLine(cx, cy, cx + rx, cy - ry, new SKPaint { Color = col, StrokeWidth = 2, IsAntialias = true });
        }
    }
}
