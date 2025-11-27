using SPHDEM_Simulation_New.SPH;
using SPHDEM_Simulation_New.DEM;
using System.Collections.Generic;

namespace SPHDEM_Simulation_New.Coupling {
    public class SPHDEMCoupler {
        public void ApplyCoupling(List<SPHParticle> sphParticles, List<DEMParticle> demParticles) {
            double couplingRadius = 12.0;
            foreach (var dp in demParticles) {
                foreach (var sp in sphParticles) {
                    double dx = dp.X - sp.X;
                    double dy = dp.Y - sp.Y;
                    double dist2 = dx * dx + dy * dy;
                    if (dist2 < couplingRadius * couplingRadius) {
                        // Fluid drag on DEM particle
                        double relVX = sp.VX - dp.Velocity[0];
                        double relVY = sp.VY - dp.Velocity[1];
                        double dragCoeff = 0.2;
                        dp.Force[0] += dragCoeff * relVX;
                        dp.Force[1] += dragCoeff * relVY;
                        // DEM resistance on SPH particle
                        double demResistance = 0.1;
                        sp.VX += demResistance * (-relVX);
                        sp.VY += demResistance * (-relVY);
                    }
                }
            }
        }
    }
}
