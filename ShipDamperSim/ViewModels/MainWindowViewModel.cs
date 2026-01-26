using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.Timers;
using Avalonia.Threading;

namespace ShipDamperSim.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private double damperSize = 1.0;

    [ObservableProperty]
    private double fillRatio = 0.5;

    [ObservableProperty]
    private double damperPosition = 0.0;

    public List<(double t, double angle)> RollData { get; private set; } = new();
    public List<(double t, double angle)> WaveData { get; private set; } = new();
    public List<(double t, double angle)> DampedData { get; private set; } = new();

    public int AnimationIndex { get; private set; } = 0;
    private System.Timers.Timer? animationTimer;
    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand ResetCommand { get; }

    public MainWindowViewModel()
    {
        GenerateData();
        StartCommand = new RelayCommand(OnStart);
        StopCommand = new RelayCommand(OnStop);
        ResetCommand = new RelayCommand(OnReset);
    }

    private void GenerateData()
    {
        RollData = new List<(double t, double angle)>();
        WaveData = new List<(double t, double angle)>();
        DampedData = new List<(double t, double angle)>();
        double waveAmp = 0.5;
        double waveFreq = 0.3;
        double damping = 0.05 + 0.15 * FillRatio; // damperin vaikutus
        for (double t = 0; t < 20; t += 0.1)
        {
            double wave = waveAmp * Math.Sin(2 * Math.PI * waveFreq * t);
            double damped = Math.Exp(-damping * t) * Math.Sin(2 * Math.PI * waveFreq * t);
            double combined = wave + damped;
            WaveData.Add((t, wave));
            DampedData.Add((t, damped));
            RollData.Add((t, combined));
        }
    }

    private void OnStart()
    {
        if (animationTimer != null)
            return;
        animationTimer = new System.Timers.Timer(50);
        animationTimer.Elapsed += (s, e) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                AnimationIndex++;
                if (AnimationIndex >= RollData.Count)
                    AnimationIndex = 0;
                OnPropertyChanged(nameof(AnimationIndex));
            });
        };
        animationTimer.Start();
    }

    private void OnStop()
    {
        animationTimer?.Stop();
        animationTimer = null;
    }

    private void OnReset()
    {
        AnimationIndex = 0;
        OnPropertyChanged(nameof(AnimationIndex));
    }
}
