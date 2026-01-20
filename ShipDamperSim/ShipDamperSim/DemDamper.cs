using System.Collections.Generic;

namespace ShipDamperSim;

public sealed class DemDamper
{
    private readonly DamperConfig _cfg;
    private readonly Random _rng;
    private readonly Particle[] _p;
    private readonly double _mass;
    private readonly double _r;
    private readonly double _yMin, _yMax, _zMin, _zMax;
    // Energiaseuranta
    public double LastStepDissipatedEnergy { get; private set; } = 0.0;
    public double TotalDissipatedEnergy { get; private set; } = 0.0;
    public double LastStepContactWork { get; private set; } = 0.0;
    public double TotalContactWork { get; private set; } = 0.0;
    // Expose damper particle positions for visualization
    public IEnumerable<(double X, double Y, double Z)> ParticlesForVisualization()
    {
        foreach (var p in _p)
            yield return (p.Pos.X, p.Pos.Y, p.Pos.Z);
    }

    // Kontaktin tila stick-slip-mallia varten
    private class ContactState
    {
        public Vec3 TangentialDisp = new(0, 0, 0);
    }

    private struct Particle
    {
        public Vec3 Pos;
        public Vec3 Vel;
        public double Mass;
        public int Id; // yksilöivä tunniste
    }

    // Spatial hash -parametrit
    private double _cellSize = 0.0;
    private Dictionary<(int, int), List<int>> _spatialHash = new();
    private Dictionary<(int, int), List<int>> BuildSpatialHash()
    {
        var hash = new Dictionary<(int, int), List<int>>();
        for (int i = 0; i < _p.Length; i++)
        {
            var p = _p[i];
            int cy = (int)Math.Floor(p.Pos.Y / _cellSize);
            int cz = (int)Math.Floor(p.Pos.Z / _cellSize);
            var key = (cy, cz);
            if (!hash.ContainsKey(key)) hash[key] = new List<int>();
            hash[key].Add(i);
        }
        return hash;
    }

    // Rae–rae kontaktien tila (id1 < id2)
    private readonly Dictionary<(int, int), ContactState> _contactStates = new();

    public DemDamper(DamperConfig cfg, int seed)
    {
        _cfg = cfg;
        _rng = new Random(seed);

        _r = cfg.Radius;
        _mass = (4.0 / 3.0) * Math.PI * _r * _r * _r * cfg.Density;

        _yMin = cfg.CenterY - cfg.SizeY / 2.0;
        _yMax = cfg.CenterY + cfg.SizeY / 2.0;
        _zMin = cfg.CenterZ - cfg.SizeZ / 2.0;
        _zMax = cfg.CenterZ + cfg.SizeZ / 2.0;

        // Compute particle count from fill ratio
        double containerVol = cfg.SizeY * cfg.SizeZ * 1.0; // Assume X=1.0m (or add SizeX if available)
        double grainVol = (4.0 / 3.0) * Math.PI * _r * _r * _r;
        int nParticles = (int)(cfg.FillRatio * containerVol / grainVol);
        if (nParticles < 1) nParticles = 1;
        cfg.ParticleCount = nParticles;
        _p = new Particle[nParticles];
        _cellSize = 2.2 * _r; // hieman suurempi kuin raekoko
        InitParticles();
        // Anna jokaiselle partikkelille yksilöivä id
        for (int i = 0; i < _p.Length; i++) _p[i].Id = i;
    }

    private void InitParticles()
    {
        for (int i = 0; i < _p.Length; i++)
        {
            double y = _yMin + (_yMax - _yMin) * _rng.NextDouble();
            double z = _zMin + (_zMax - _zMin) * _rng.NextDouble();
            _p[i] = new Particle { Pos = new Vec3(0, y, z), Vel = new Vec3(0, 0, 0), Mass = _mass };
        }
    }

    public double Step(double dt, double phi, double phiDot)
    {
        const double g = 9.81;
        Vec3 gEff = new Vec3(0, g * Math.Sin(phi), -g * Math.Cos(phi));
        double mDamper = 0.0;

        double stepDissip = 0.0;
        double stepContact = 0.0;

        // Spatial hash päivitys
        _spatialHash = BuildSpatialHash();

        // Rae–rae kontaktivoimat
        Vec3[] pairForces = new Vec3[_p.Length];
        for (int i = 0; i < _p.Length; i++) pairForces[i] = new Vec3(0, 0, 0);

        for (int i = 0; i < _p.Length; i++)
        {
            var pi = _p[i];
            // Etsi naapurit
            int cy = (int)Math.Floor(pi.Pos.Y / _cellSize);
            int cz = (int)Math.Floor(pi.Pos.Z / _cellSize);
            for (int dy = -1; dy <= 1; dy++)
                for (int dz = -1; dz <= 1; dz++)
                {
                    var key = (cy + dy, cz + dz);
                    if (!_spatialHash.ContainsKey(key)) continue;
                    foreach (var j in _spatialHash[key])
                    {
                        if (j <= i) continue; // Vältä tuplakäsittely
                        var pj = _p[j];
                        Vec3 d = pj.Pos - pi.Pos;
                        double dist = d.Norm();
                        double overlap = 2 * _r - dist;
                        if (overlap > 0)
                        {
                            Vec3 n = dist > 1e-12 ? d / dist : new Vec3(0, 1, 0);
                            double vRelN = (pj.Vel - pi.Vel).Dot(n);
                            double fN = _cfg.Kn * overlap;
                            double fDamp = _cfg.GammaN * vRelN;
                            Vec3 fNormal = (fN + fDamp) * n;

                            // Stick-slip: tangential displacement
                            var idPair = (Math.Min(pi.Id, pj.Id), Math.Max(pi.Id, pj.Id));
                            if (!_contactStates.ContainsKey(idPair)) _contactStates[idPair] = new ContactState();
                            var state = _contactStates[idPair];
                            // Päivitä tangential displacement
                            Vec3 vRel = pj.Vel - pi.Vel;
                            Vec3 vRelT = vRel - vRelN * n;
                            state.TangentialDisp += vRelT * dt;
                            // Mindlin: tangential force
                            double kt = _cfg.Kt;
                            double gt = _cfg.Gt;
                            Vec3 fT = -kt * state.TangentialDisp - gt * vRelT;
                            // Coulomb rajoitus
                            double mu = _cfg.Mu;
                            double fTmax = mu * Math.Abs(fN + fDamp);
                            if (fT.Norm() > fTmax)
                                fT = fT.Normalized() * fTmax;

                            // Päivitä voimat
                            pairForces[i] += fNormal + fT;
                            pairForces[j] -= fNormal + fT;

                            // Dissipaatio ja kontaktityö
                            double dissip = Math.Abs(fDamp * vRelN * dt) + Math.Abs(gt * vRelT.Norm2() * dt);
                            stepDissip += dissip;
                            stepContact += Math.Abs(fN * vRelN * dt);
                        }
                        else
                        {
                            // Nollaa tangential displacement kun ei kontaktia
                            var idPair = (Math.Min(pi.Id, pj.Id), Math.Max(pi.Id, pj.Id));
                            if (_contactStates.ContainsKey(idPair))
                                _contactStates[idPair].TangentialDisp = new Vec3(0, 0, 0);
                        }
                    }
                }
        }

        // Yksittäisten partikkelien päivitys (seinät, gravitaatio, integraatio)
        for (int i = 0; i < _p.Length; i++)
        {
            var pi = _p[i];
            Vec3 f = pi.Mass * gEff + pairForces[i];
            Vec3 contactForce = new Vec3(0, 0, 0);
            double dissip = 0.0;

            // Wall contacts (Y)
            if (pi.Pos.Y < _yMin)
            {
                double overlap = _yMin - pi.Pos.Y;
                double vRel = -pi.Vel.Y;
                double fN = _cfg.Kn * overlap;
                double fDamp = _cfg.GammaN * vRel;
                contactForce += new Vec3(0, fN + fDamp, 0);
                dissip += Math.Abs(fDamp * vRel * dt); // Dissipated energy
                stepContact += Math.Abs(fN * vRel * dt); // Contact work
            }
            if (pi.Pos.Y > _yMax)
            {
                double overlap = pi.Pos.Y - _yMax;
                double vRel = pi.Vel.Y;
                double fN = -_cfg.Kn * overlap;
                double fDamp = -_cfg.GammaN * vRel;
                contactForce += new Vec3(0, fN + fDamp, 0);
                dissip += Math.Abs(fDamp * vRel * dt);
                stepContact += Math.Abs(fN * vRel * dt);
            }
            // Wall contacts (Z)
            if (pi.Pos.Z < _zMin)
            {
                double overlap = _zMin - pi.Pos.Z;
                double vRel = -pi.Vel.Z;
                double fN = _cfg.Kn * overlap;
                double fDamp = _cfg.GammaN * vRel;
                contactForce += new Vec3(0, 0, fN + fDamp);
                dissip += Math.Abs(fDamp * vRel * dt);
                stepContact += Math.Abs(fN * vRel * dt);
            }
            if (pi.Pos.Z > _zMax)
            {
                double overlap = pi.Pos.Z - _zMax;
                double vRel = pi.Vel.Z;
                double fN = -_cfg.Kn * overlap;
                double fDamp = -_cfg.GammaN * vRel;
                contactForce += new Vec3(0, 0, fN + fDamp);
                dissip += Math.Abs(fDamp * vRel * dt);
                stepContact += Math.Abs(fN * vRel * dt);
            }

            f += contactForce;
            Vec3 acc = f / pi.Mass;
            pi.Vel += dt * acc;
            pi.Pos += dt * pi.Vel;
            _p[i] = pi;

            double ry = pi.Pos.Y;
            double rz = pi.Pos.Z;
            mDamper += ry * f.Z - rz * f.Y;
            stepDissip += dissip;
        }
        LastStepDissipatedEnergy = stepDissip;
        TotalDissipatedEnergy += stepDissip;
        LastStepContactWork = stepContact;
        TotalContactWork += stepContact;
        return mDamper;
    }
}
