using System.Collections.Generic;

namespace ShipDamperSim.Core;

public record SimulationResult(
    double MaxRollOn,
    double MaxRollOff,
    double BenefitPercent,
    List<(double t, double roll)> RollDataOn,
    List<(double t, double roll)> RollDataOff
);
