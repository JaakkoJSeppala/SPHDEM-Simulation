
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia;
using ShipDamperSim.ViewModels;
using System.Collections.Generic;
using System;

namespace ShipDamperSim.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.Opened += (_, __) => DrawPlot();
        this.DataContextChanged += (_, __) => SubscribeToVm();
        SubscribeToVm();
    }

    private void SubscribeToVm()
    {
        if (this.DataContext is MainWindowViewModel vm)
        {
            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.RollData) || e.PropertyName == nameof(vm.WaveData) || e.PropertyName == nameof(vm.DampedData) || e.PropertyName == nameof(vm.AnimationIndex))
                    DrawPlot();
            };
        }
    }

    private void DrawPlot()
    {
        if (this.DataContext is not MainWindowViewModel vm)
            return;
        if (this.FindControl<Canvas>("ShipCanvas") is not Canvas canvas)
            return;

        // Tyhjennä vanhat piirrot
        canvas.Children.Clear();

        // Jos SPH-only simulaatio on ajettu, piirrä partikkelit scatterina
        if (vm.SphParticlePositions != null && vm.SphParticlePositions.Count > 0)
        {
            // Piirretään viimeisen askeleen partikkelit
            var last = vm.SphParticlePositions[vm.SphParticlePositions.Count - 1];
            double cx = 200, cy = 160, scale = 600; // skaalaus ja siirto
            foreach (var pos in last)
            {
                double x = cx + pos.X * scale;
                double y = cy - pos.Y * scale;
                var ellipse = new Avalonia.Controls.Shapes.Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = Brushes.CornflowerBlue
                };
                Canvas.SetLeft(ellipse, x - 4);
                Canvas.SetTop(ellipse, y - 4);
                canvas.Children.Add(ellipse);
            }
            // Piirrä myös boundary-partikkelit, jos halutaan (ei toteutettu tässä)
            return;
        }

        // ...existing code... (laivan ja aallon piirto)
    }
}