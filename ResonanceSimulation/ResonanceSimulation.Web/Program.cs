using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using ResonanceSimulation.Core;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();
var app = builder.Build();
app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

// Staattinen frontend
app.UseDefaultFiles();
app.UseStaticFiles();

// Simulaation tila (yksinkertainen, globaali)
Simulator? simulator = null;
Measurements? lastMeasurements = null;
int lastStep = 0;

// API: käynnistä simulaatio
app.MapPost("/api/start", async (HttpContext ctx) =>
{
    // Lue parametrit JSON:sta
    var body = await new System.IO.StreamReader(ctx.Request.Body).ReadToEndAsync();
    double frequency = 0.6;
    bool damper = true;
    double time = 10.0;
    Console.WriteLine($"[API/start] called. Body: {body}");
    if (!string.IsNullOrWhiteSpace(body))
    {
        try {
            var json = JsonDocument.Parse(body);
            if (json.RootElement.TryGetProperty("frequency", out var freqEl)) frequency = freqEl.GetDouble();
            if (json.RootElement.TryGetProperty("damper", out var dampEl)) damper = dampEl.GetBoolean();
            if (json.RootElement.TryGetProperty("time", out var timeEl)) time = timeEl.GetDouble();
        } catch (Exception ex) {
            Console.WriteLine($"[API/start] JSON parse error: {ex.Message}");
        }
    }
    var config = new SimulationConfig
    {
        Frequency = frequency,
        EnableDamper = damper,
        TotalTime = time
    };
    simulator = new Simulator(config);
    simulator.Initialize();
    lastStep = 0;
    lastMeasurements = simulator.GetMeasurements();
    // Log particle counts after init
    var sphParticles = simulator.GetType().GetField("_fluidParticles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(simulator) as List<ResonanceSimulation.Core.SPHParticle>;
    var demParticles = simulator.GetType().GetField("_damperParticles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(simulator) as List<ResonanceSimulation.Core.DEMParticle>;
    Console.WriteLine($"[API/start] Initialized: SPH={sphParticles?.Count ?? 0} DEM={demParticles?.Count ?? 0}");
    await ctx.Response.WriteAsync(JsonSerializer.Serialize(new { status = "started" }));
});

// API: hae simulaation tila (askel kerrallaan)
app.MapGet("/api/state", async (HttpContext ctx) =>
{
    if (simulator == null)
    {
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(new { status = "not_started" }));
        return;
    }

    Console.WriteLine($"[API/state] called. Simulator null? {simulator == null}");
    // Suorita yksi askel ja palauta partikkelien sijainnit + mittaukset
    simulator.Step();
    lastStep++;
    lastMeasurements = simulator.GetMeasurements();
    var fluid = lastMeasurements.Times.Count > 0 ? lastMeasurements.Times.Count - 1 : 0;

    // Kerää partikkelien sijainnit
    var sphParticles = simulator.GetType().GetField("_fluidParticles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(simulator) as List<ResonanceSimulation.Core.SPHParticle>;
    var demParticles = simulator.GetType().GetField("_damperParticles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(simulator) as List<ResonanceSimulation.Core.DEMParticle>;

    var sphPositions = new List<object>();
    if (sphParticles != null)
        sphPositions.AddRange(sphParticles.Select(p => (object)new { x = p.Position.X, y = p.Position.Y }));
    var demPositions = new List<object>();
    if (demParticles != null)
        demPositions.AddRange(demParticles.Select(p => (object)new { x = p.Position.X, y = p.Position.Y, r = p.Radius }));

    // Debug logging
    Console.WriteLine($"[API/state] step={lastStep} SPH={sphParticles?.Count ?? 0} DEM={demParticles?.Count ?? 0}");
    if (sphParticles != null && sphParticles.Count > 0)
        Console.WriteLine($"  SPH sample: x={sphParticles[0].Position.X:F3} y={sphParticles[0].Position.Y:F3}");
    if (demParticles != null && demParticles.Count > 0)
        Console.WriteLine($"  DEM sample: x={demParticles[0].Position.X:F3} y={demParticles[0].Position.Y:F3} r={demParticles[0].Radius:F3}");

    await ctx.Response.WriteAsync(JsonSerializer.Serialize(new
    {
        step = lastStep,
        time = lastMeasurements.Times.Count > 0 ? lastMeasurements.Times[fluid] : 0.0,
        wallPressure = lastMeasurements.WallPressures.Count > 0 ? lastMeasurements.WallPressures[fluid] : 0.0,
        freeSurface = lastMeasurements.FreeSurfaceHeights.Count > 0 ? lastMeasurements.FreeSurfaceHeights[fluid] : 0.0,
        kineticEnergy = lastMeasurements.KineticEnergies.Count > 0 ? lastMeasurements.KineticEnergies[fluid] : 0.0,
        tankDisplacement = lastMeasurements.TankDisplacements.Count > 0 ? lastMeasurements.TankDisplacements[fluid] : 0.0,
        progress = lastStep / 1000.0,
        status = "running",
        sphParticles = sphPositions,
        demParticles = demPositions
    }));
});

app.Run();
