
using CommunityToolkit.Mvvm.ComponentModel;
using OpenTK.Mathematics;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Avalonia.Threading;
using ShipDamperSim.Core;

namespace ShipDamperSim.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    // KPI-tulokset
    public double MaxRollOn { get; private set; }
    public double MaxRollOff { get; private set; }
    public double BenefitPercent { get; private set; }
    public string KpiText { get; private set; } = "";

    // Kutsu simulaatiota Coresta ja päivitä tulokset
    public void RunSimulation()
    {
        // Voit laajentaa parametreja UI:sta SimulationParametersiin
        var parameters = new SimulationParameters(0, 0, 0); // Täydennä tarvittaessa
        var result = Simulator.Run(parameters);
        MaxRollOn = result.MaxRollOn;
        MaxRollOff = result.MaxRollOff;
        BenefitPercent = result.BenefitPercent;
        KpiText = $"Vaimennin pienentää laivan maksimikallistumaa {BenefitPercent:F1} % (ON: {MaxRollOn:F3}, OFF: {MaxRollOff:F3})";
        // Voit halutessasi päivittää myös aikasarjat graafeihin:
        RollData = result.RollDataOn.Select(x => (x.t, x.roll)).ToList();
        // ...
        OnPropertyChanged(nameof(MaxRollOn));
        OnPropertyChanged(nameof(MaxRollOff));
        OnPropertyChanged(nameof(BenefitPercent));
        OnPropertyChanged(nameof(KpiText));
        OnPropertyChanged(nameof(RollData));
    }
    // OxyPlot-graafit
    public OxyPlot.PlotModel RollPlotModel { get; private set; } = new();
    public OxyPlot.PlotModel FreeSurfacePlotModel { get; private set; } = new();

    // Damperin tila (ON/OFF)
    public bool DamperOn { get; set; } = true;
    // Nykyinen rollikulma (visualisointiin/graafiin)
    public double CurrentRoll { get; set; } = 0.0;
    // Nykyinen vapaan pinnan korkeus
    public double CurrentFreeSurface { get; set; } = 0.0;

    // Päivitä graafit simulaation datasta
    public void UpdatePlots()
    {
        // Roll-graafi
        var rollSeries = new OxyPlot.Series.LineSeries { Title = "Roll" };
        foreach (var (t, angle) in RollData)
            rollSeries.Points.Add(new OxyPlot.DataPoint(t, angle));
        RollPlotModel.Series.Clear();
        RollPlotModel.Series.Add(rollSeries);
        RollPlotModel.InvalidatePlot(true);

        // Vapaan pinnan korkeus -graafi
        var fsSeries = new OxyPlot.Series.LineSeries { Title = "Free Surface" };
        foreach (var (t, fs) in FreeSurfaceData)
            fsSeries.Points.Add(new OxyPlot.DataPoint(t, fs));
        FreeSurfacePlotModel.Series.Clear();
        FreeSurfacePlotModel.Series.Add(fsSeries);
        FreeSurfacePlotModel.InvalidatePlot(true);

        OnPropertyChanged(nameof(RollPlotModel));
        OnPropertyChanged(nameof(FreeSurfacePlotModel));
    }

    // Päivitä nykyinen roll ja vapaan pinnan korkeus (graafeihin)
    public void UpdateCurrentValues()
    {
        if (RollData.Count > 0)
            CurrentRoll = RollData.Last().angle;
        if (FreeSurfaceData.Count > 0)
            CurrentFreeSurface = FreeSurfaceData.Last().freeSurface;
        OnPropertyChanged(nameof(CurrentRoll));
        OnPropertyChanged(nameof(CurrentFreeSurface));
    }
    public ICommand RunOnOffComparisonCommand { get; }
    // ON/OFF-vertailun tulokset
    public List<(double t, double rollOn, double rollOff)> RollOnOffData { get; private set; } = new();
    public List<(double t, double wallPOn, double wallPOff)> WallPressureOnOffData { get; private set; } = new();

    // Laajennetut mittarit
    public List<(double t, double fluidE, double demE, double couplingE)> EnergyData { get; private set; } = new();
    public List<(double t, int demContacts)> DemContactData { get; private set; } = new();
    public List<(double t, double freeSurface)> FreeSurfaceData { get; private set; } = new();

    // Validointimittarit
    public List<(double t, double wallPressure)> WallPressureData { get; private set; } = new();
    public List<(double t, double zeta)> ZetaData { get; private set; } = new();
    public List<(double t, double freq)> SloshingFreqData { get; private set; } = new();
    public List<(double t, double peakLoad)> PeakLoadData { get; private set; } = new();

    // SPH-only simulaation tulokset
    public List<Vector3[]> SphParticlePositions { get; private set; } = new();
    public List<(double t, double avgPressure)> SphAvgPressure { get; private set; } = new();

    private Simulation? simulation;

    public List<(double t, double angle)> RollData { get; private set; } = new();
    public List<(double t, double angle)> WaveData { get; private set; } = new();
    public List<(double t, double angle)> DampedData { get; private set; } = new();
    public int AnimationIndex { get; private set; } = 0;
    private Timer? animationTimer;
    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand CompareDamperEffectCommand { get; }

    public MainWindowViewModel()
    {
        GenerateData();
        StartCommand = new RelayCommand(OnStart);
        StopCommand = new RelayCommand(OnStop);
        ResetCommand = new RelayCommand(OnReset);
        RunOnOffComparisonCommand = new RelayCommand(RunOnOffComparison);
        CompareDamperEffectCommand = new RelayCommand(() => CompareDamperEffect());
    }

    private void GenerateData()
    {
        // Minimi-SPH–DEM-simulaatiosilmukka
        RollData = new List<(double t, double angle)>();
        WaveData = new List<(double t, double angle)>();
        DampedData = new List<(double t, double angle)>();

        // Parametrit
        double simTime = 5.0; // sekuntia
        double dt = 0.002;
        int steps = (int)(simTime / dt);

        // Alusta SPH, DEM
        var sph = new SphFluid(6, 2, 3); // pieni 3D-ruudukko
        var dem = new DemDamper(5); // muutama DEM-partikkeli

        // Alkuasetus: pieni kallistus (rollikulma)
        double initialRoll = 0.1; // rad
        foreach (var p in sph.Particles)
            p.Position.X += (float)(Math.Sin(initialRoll) * (p.Position.Y - 0.05));
        foreach (var p in dem.Particles)
            p.Position.X += (float)(Math.Sin(initialRoll) * (p.Position.Y - 0.05));

        // Simulointisilmukka
        for (int i = 0; i < steps; i++)
        {
            double t = i * dt;
            // 1. SPH-päivitys
            sph.Step((float)dt);
            // 2. DEM-päivitys
            dem.Step((float)dt);
            // 3. Kytkentä (drag, paine, noste)
            SphDemCoupler.Couple(sph.Particles, dem.Particles, (float)dt);

            // Loggaa rollikulma: DEM-massakeskipisteen X (oletetaan tankin keskilinja Y=0.2)
            double demMass = 0, demXsum = 0;
            foreach (var p in dem.Particles) { demMass += p.Mass; demXsum += p.Mass * p.Position.X; }
            double demXc = demMass > 0 ? demXsum / demMass : 0;
            // Loggaa myös SPH-massakeskipisteen X
            double sphMass = 0, sphXsum = 0;
            foreach (var p in sph.Particles) { sphMass += p.Mass; sphXsum += p.Mass * p.Position.X; }
            double sphXc = sphMass > 0 ? sphXsum / sphMass : 0;

            // Tallennetaan: t, "rollikulma" (esim. massakeskipisteen X)
            RollData.Add((t, demXc));
            WaveData.Add((t, sphXc));
            DampedData.Add((t, 0)); // Ei käytössä, placeholder
        }
        OnPropertyChanged(nameof(RollData));
        OnPropertyChanged(nameof(WaveData));
        OnPropertyChanged(nameof(DampedData));
    }

    private void RunBackgroundDamperStats()
    {
        System.Threading.Tasks.Task.Run(() => CompareDamperEffect());
    }

    public void RunUnifiedSphDemSimulation()
    {
        WallPressureData = new();
        ZetaData = new();
        SloshingFreqData = new();
        PeakLoadData = new();
        RollData = new();
        WaveData = new();
        DampedData = new();
        EnergyData = new();
        DemContactData = new();
        FreeSurfaceData = new();

        double simTime = 10.0;
        double dt = 0.002;
        int steps = (int)(simTime / dt);

        simulation = new Simulation(8, 2, 4, 8);

        double maxWallPressure = 0;
        double maxRoll = 0;
        double prevRoll = 0;
        double prevT = 0;
        double lastPeakT = 0;

        for (int i = 0; i < steps; i++)
        {
            double t = i * dt;
            simulation.Step(dt);

            // Rollikulma (DEM-massakeskipisteen X)
            double demMass = 0, demXsum = 0;
            foreach (var p in simulation.Dem.Particles) { demMass += p.Mass; demXsum += p.Mass * p.Position.X; }
            double demXc = demMass > 0 ? demXsum / demMass : 0;
            RollData.Add((t, demXc));

            // SPH-massakeskipisteen X
            double sphMass = 0, sphXsum = 0;
            foreach (var p in simulation.Sph.Particles) { sphMass += p.Mass; sphXsum += p.Mass * p.Position.X; }
            double sphXc = sphMass > 0 ? sphXsum / sphMass : 0;
            WaveData.Add((t, sphXc));

            // Seinämäpaine: etsitään suurin paine boundary-partikkeleista (esim. vasen seinä)
            double wallP = 0;
            foreach (var b in simulation.Sph.Boundaries)
                if (Math.Abs(b.Position.X + 0.6f) < 0.02f)
                    wallP = Math.Max(wallP, b.Pressure);
            WallPressureData.Add((t, wallP));
            if (wallP > maxWallPressure) maxWallPressure = wallP;

            // Peak load reduction: seuraa seinämäpaineen huippuja
            if (wallP > maxWallPressure * 0.95 && t - lastPeakT > 0.2)
            {
                PeakLoadData.Add((t, wallP));
                lastPeakT = t;
            }

            // ζ (zeta): vaimennuskerroin, arvioidaan rollikulman vaimenemisesta
            if (Math.Abs(demXc) > maxRoll) maxRoll = Math.Abs(demXc);
            if (prevRoll > 0 && demXc < 0 && prevRoll > 0.01)
            {
                double zeta = -Math.Log(Math.Abs(demXc) / prevRoll) / Math.PI;
                ZetaData.Add((t, zeta));
            }
            prevRoll = demXc;

            // Sloshing-taajuus: nollan ylitysten välinen aika
            if (prevRoll < 0 && demXc > 0)
            {
                double freq = 1.0 / (t - prevT);
                SloshingFreqData.Add((t, freq));
                prevT = t;
            }

            // Energiataseet
            double fluidE = 0, demE = 0, couplingE = 0;
            foreach (var p in simulation.Sph.Particles)
                fluidE += 0.5 * p.Mass * p.Velocity.LengthSquared;
            foreach (var p in simulation.Dem.Particles)
                demE += 0.5 * p.Mass * p.Velocity.LengthSquared;
            // couplingE: placeholder, vaatii oikean laskennan couplerista
            EnergyData.Add((t, fluidE, demE, couplingE));

            // DEM-kontaktien määrä (arvioidaan etäisyyden perusteella)
            int demContacts = 0;
            var demParticles = simulation.Dem.Particles;
            for (int a = 0; a < demParticles.Count; a++)
                for (int b = a + 1; b < demParticles.Count; b++)
                    if ((demParticles[a].Position - demParticles[b].Position).Length < (demParticles[a].Radius + demParticles[b].Radius + 1e-4))
                        demContacts++;
            DemContactData.Add((t, demContacts));

            // SPH-vapaan pinnan korkeus (korkein neste-partikkeli)
            double freeSurface = double.MinValue;
            foreach (var p in simulation.Sph.Particles)
                if (p.Position.Y > freeSurface) freeSurface = p.Position.Y;
            FreeSurfaceData.Add((t, freeSurface));
        }
        OnPropertyChanged(nameof(RollData));
        OnPropertyChanged(nameof(WaveData));
        OnPropertyChanged(nameof(WallPressureData));
        OnPropertyChanged(nameof(PeakLoadData));
        OnPropertyChanged(nameof(ZetaData));
        OnPropertyChanged(nameof(SloshingFreqData));
    }

    public void RunSphOnlySimulation()
    {
        SphParticlePositions = new List<Vector3[]>();
        SphAvgPressure = new List<(double t, double avgPressure)>();

        double simTime = 2.0; // sekuntia
        double dt = 0.002;
        int steps = (int)(simTime / dt);

        // Alusta SPH (esim. pieni 2D-ruudukko)
        var sph = new SphFluid(8, 1, 4); // 2D-"viipale"

        // Alkuasetus: pieni häiriö (esim. X-suunnan siirto)
        foreach (var p in sph.Particles)
            p.Position.X += 0.05f * (float)(new Random().NextDouble() - 0.5);

        for (int i = 0; i < steps; i++)
        {
            double t = i * dt;
            sph.Step((float)dt);
            // Tallenna partikkelien sijainnit
            var positions = new Vector3[sph.Particles.Count];
            for (int j = 0; j < sph.Particles.Count; j++)
                positions[j] = sph.Particles[j].Position;
            SphParticlePositions.Add(positions);
            // Laske keskimääräinen paine
            double avgP = 0;
            foreach (var p in sph.Particles) avgP += p.Pressure;
            avgP /= Math.Max(1, sph.Particles.Count);
            SphAvgPressure.Add((t, avgP));
        }
        OnPropertyChanged(nameof(SphParticlePositions));
        OnPropertyChanged(nameof(SphAvgPressure));
    }

    [ObservableProperty]
    private double damperSize = 1.0;
    [ObservableProperty]
    private double fillRatio = 0.5;
    [ObservableProperty]
    private double damperPosition = 0.0;

    private void OnStart()
    {
        RunSimulation();
        AnimationIndex = 0;
        animationTimer?.Dispose();
        animationTimer = new Timer(30);
        animationTimer.Elapsed += (s, e) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (AnimationIndex >= RollData.Count - 1)
                {
                    animationTimer?.Stop();
                    return;
                }
                AnimationIndex++;
                OnPropertyChanged(nameof(AnimationIndex));
            });
        };
        animationTimer.Start();
    }

    private void OnStop()
    {
        animationTimer?.Stop();
    }

    private void OnReset()
    {
        animationTimer?.Stop();
        AnimationIndex = 0;
        DamperSize = 1.0;
        FillRatio = 0.5;
        DamperPosition = 0.0;
        GenerateData();
        OnPropertyChanged(nameof(RollData));
        OnPropertyChanged(nameof(WaveData));
        OnPropertyChanged(nameof(DampedData));
        OnPropertyChanged(nameof(AnimationIndex));
    }

    // Aja simulaatio sekä damper ON että OFF, tallenna mittarit rinnakkain
    public void RunOnOffComparison()
    {
        RollOnOffData = new();
        WallPressureOnOffData = new();
        double simTime = 10.0;
        double dt = 0.002;
        int steps = (int)(simTime / dt);

        // ON: damper käytössä
        var simOn = new Simulation(8, 2, 4, 8) { DamperEnabled = true };
        // OFF: damper pois päältä
        var simOff = new Simulation(8, 2, 4, 8) { DamperEnabled = false };

        for (int i = 0; i < steps; i++)
        {
            double t = i * dt;
            simOn.Step(dt);
            simOff.Step(dt);

            // Rollikulma (DEM-massakeskipisteen X)
            double demMassOn = 0, demXsumOn = 0;
            foreach (var p in simOn.Dem.Particles) { demMassOn += p.Mass; demXsumOn += p.Mass * p.Position.X; }
            double rollOn = demMassOn > 0 ? demXsumOn / demMassOn : 0;
            double demMassOff = 0, demXsumOff = 0;
            foreach (var p in simOff.Dem.Particles) { demMassOff += p.Mass; demXsumOff += p.Mass * p.Position.X; }
            double rollOff = demMassOff > 0 ? demXsumOff / demMassOff : 0;
            RollOnOffData.Add((t, rollOn, rollOff));

            // Seinämäpaine (vasen seinä)
            double wallPOn = 0, wallPOff = 0;
            foreach (var b in simOn.Sph.Boundaries)
                if (Math.Abs(b.Position.X + 0.6f) < 0.02f)
                    wallPOn = Math.Max(wallPOn, b.Pressure);
            foreach (var b in simOff.Sph.Boundaries)
                if (Math.Abs(b.Position.X + 0.6f) < 0.02f)
                    wallPOff = Math.Max(wallPOff, b.Pressure);
            WallPressureOnOffData.Add((t, wallPOn, wallPOff));
        }
        OnPropertyChanged(nameof(RollOnOffData));
        OnPropertyChanged(nameof(WallPressureOnOffData));
    }

    /// <summary>
    /// Simulaation hyvyyden mittari: esim. rollikulman ja seinäpaineen RMS-ero ON/OFF-tilojen välillä
    /// </summary>
    public double SimulationQualityScore
    {
        get
        {
            if (RollOnOffData == null || RollOnOffData.Count == 0)
                return 0;
            // Esimerkki: RMS-ero rollikulman välillä ON/OFF
            double sumSq = 0;
            int n = Math.Min(500, RollOnOffData.Count); // rajoitetaan laskenta
            for (int i = 0; i < n; i++)
            {
                var d = RollOnOffData[i];
                double diff = d.rollOn - d.rollOff;
                sumSq += diff * diff;
            }
            double rms = Math.Sqrt(sumSq / Math.Max(1, n));
            return rms;
        }
    }

    /// <summary>
    /// Damperin vaikutuksen tilastollinen vertailu: useita ajanjaksoja, rollikulmat ON/OFF, t-testi
    /// </summary>
    public class DamperEffectStats
    {
        public double MeanOn { get; set; }
        public double MeanOff { get; set; }
        public double VarianceOn { get; set; }
        public double VarianceOff { get; set; }
        public double TStatistic { get; set; }
        public double PValue { get; set; }
        public int SampleCount { get; set; }
    }

    public DamperEffectStats? DamperEffectComparisonResults { get; private set; }

    public void CompareDamperEffect(int periods = 5, double periodLength = 2.0, double dt = 0.002)
    {
        // Simuloi useita ajanjaksoja, kerää rollikulmat ON/OFF
        int stepsPerPeriod = (int)(periodLength / dt);
        List<double> rollOn = new();
        List<double> rollOff = new();
        for (int p = 0; p < periods; p++)
        {
            var simOn = new Simulation(8, 2, 4, 8) { DamperEnabled = true };
            var simOff = new Simulation(8, 2, 4, 8) { DamperEnabled = false };
            // Voit halutessasi randomisoida alkuasennon
            for (int i = 0; i < stepsPerPeriod; i++)
            {
                simOn.Step(dt);
                simOff.Step(dt);
                double demMassOn = 0, demXsumOn = 0;
                foreach (var pOn in simOn.Dem.Particles) { demMassOn += pOn.Mass; demXsumOn += pOn.Mass * pOn.Position.X; }
                double rollValOn = demMassOn > 0 ? demXsumOn / demMassOn : 0;
                rollOn.Add(rollValOn);
                double demMassOff = 0, demXsumOff = 0;
                foreach (var pOff in simOff.Dem.Particles) { demMassOff += pOff.Mass; demXsumOff += pOff.Mass * pOff.Position.X; }
                double rollValOff = demMassOff > 0 ? demXsumOff / demMassOff : 0;
                rollOff.Add(rollValOff);
            }
        }
        // Laske tilastolliset tunnusluvut
        double meanOn = rollOn.Count > 0 ? rollOn.Average() : 0;
        double meanOff = rollOff.Count > 0 ? rollOff.Average() : 0;
        double varOn = rollOn.Count > 1 ? rollOn.Select(x => (x - meanOn) * (x - meanOn)).Sum() / (rollOn.Count - 1) : 0;
        double varOff = rollOff.Count > 1 ? rollOff.Select(x => (x - meanOff) * (x - meanOff)).Sum() / (rollOff.Count - 1) : 0;
        // t-testi (yksinkertainen, equal variance)
        int nOn = rollOn.Count;
        int nOff = rollOff.Count;
        double pooledStd = Math.Sqrt(((nOn - 1) * varOn + (nOff - 1) * varOff) / (nOn + nOff - 2));
        double tStat = (meanOn - meanOff) / (pooledStd * Math.Sqrt(1.0 / nOn + 1.0 / nOff));
        // p-arvo (approksimaatio, normaalijakauma)
        double pValue = 2.0 * (1.0 - CumulativeNormal(Math.Abs(tStat)));
        DamperEffectComparisonResults = new DamperEffectStats
        {
            MeanOn = meanOn,
            MeanOff = meanOff,
            VarianceOn = varOn,
            VarianceOff = varOff,
            TStatistic = tStat,
            PValue = pValue,
            SampleCount = Math.Min(nOn, nOff)
        };
        OnPropertyChanged(nameof(DamperEffectComparisonResults));
    }

    // Normaalijakauman kertymäfunktio (approksimaatio)
    private static double CumulativeNormal(double x)
    {
        // Abramowitz & Stegun formula 7.1.26
        double t = 1.0 / (1.0 + 0.2316419 * x);
        double d = 0.3989423 * Math.Exp(-x * x / 2.0);
        double prob = d * t * (0.3193815 + t * (-0.3565638 + t * (1.781478 + t * (-1.821256 + t * 1.330274))));
        if (x > 0) return 1.0 - prob; else return prob;
    }
}

