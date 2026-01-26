using System;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace ShipDamperSim.Core;

public static class SphDemCoupler
{
    public static void Couple(
        List<SphFluid.Particle> sphParticles,
        List<DemDamper.Particle> demParticles,
        float dt,
        float dragCoeff = 8.0f,
        float demPressureCoeff = 0.5f,
        float h = 0.13f,
        float fluidDensity = 1000f,
        float fluidViscosity = 3.5f,
        float gravity = 9.81f)
    {
        Vector3[] demForces = new Vector3[demParticles.Count];
        Vector3[] sphForces = new Vector3[sphParticles.Count];
        for (int i = 0; i < demParticles.Count; i++)
        {
            var dem = demParticles[i];
            float demVolume = (4f / 3f) * (float)Math.PI * (float)Math.Pow(dem.Radius, 3);
            float demArea = (float)Math.PI * dem.Radius * dem.Radius;
            Vector3 fDrag = Vector3.Zero;
            Vector3 fBuoyancy = Vector3.Zero;
            Vector3 fPressure = Vector3.Zero;
            // TODO: Lisää fysiikkatermit tarvittaessa
            demForces[i] = fDrag + fBuoyancy + fPressure;
        }
        for (int j = 0; j < sphParticles.Count; j++)
        {
            sphParticles[j].Velocity += sphForces[j] / sphParticles[j].Mass * dt;
        }
        for (int i = 0; i < demParticles.Count; i++)
        {
            var dem = demParticles[i];
            dem.Velocity += demForces[i] / dem.Mass * dt;
        }
    }
}
