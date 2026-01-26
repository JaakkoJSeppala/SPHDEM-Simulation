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

        // Haetaan tämänhetkinen kulma
        double angle = 0;
        if (vm.RollData.Count > 0)
        {
            int idx = Math.Min(vm.AnimationIndex, vm.RollData.Count - 1);
            angle = vm.RollData[idx].angle;
        }

        // Piirretään "laiva" (suorakulmio) ja aallot (sinikäyrä taustalle)
        double cx = 200, cy = 160; // Canvas keskikohta
        double shipW = 120, shipH = 30;

        // Piirrä aallot taustalle
        var wave = new Avalonia.Controls.Shapes.Polyline
        {
            Stroke = Brushes.LightBlue,
            StrokeThickness = 3
        };
        var wavePoints = new Avalonia.Collections.AvaloniaList<Point>();
        for (double x = 0; x <= 400; x += 2)
        {
            double t = (x - 40) / 16.0;
            double y = cy + 30 * Math.Sin(2 * Math.PI * 0.3 * t);
            wavePoints.Add(new Point(x, y));
        }
        wave.Points = wavePoints;
        canvas.Children.Add(wave);

        // Piirretään "laiva" trapetsina (2.5D-efekti)
        double deckW = shipW;
        double deckH = shipH * 0.5;
        double hullH = shipH * 0.7;
        // Trapetsin kulmat (kansi leveämpi kuin pohja)
        Point p1 = new Point(-deckW / 2, -deckH / 2); // vasen ylä
        Point p2 = new Point(deckW / 2, -deckH / 2);  // oikea ylä
        Point p3 = new Point(deckW * 0.35, hullH / 2); // oikea ala
        Point p4 = new Point(-deckW * 0.35, hullH / 2); // vasen ala

        // Pyöräytetään pisteet roll-kulman mukaan
        Point Rotate(Point pt, double a)
        {
            double ca = Math.Cos(a), sa = Math.Sin(a);
            return new Point(
                ca * pt.X - sa * pt.Y,
                sa * pt.X + ca * pt.Y
            );
        }
        var pts = new[] { p1, p2, p3, p4 };
        for (int i = 0; i < pts.Length; i++)
            pts[i] = Rotate(pts[i], angle);
        // Siirretään laiva oikeaan kohtaan
        for (int i = 0; i < pts.Length; i++)
        {
            pts[i] = new Point(pts[i].X + cx, pts[i].Y + cy - 20);
        }

        // Piirrä varjo (tummempi trapetsi, hieman alempana)
        var shadow = new Avalonia.Controls.Shapes.Polygon
        {
            Points = new Avalonia.Collections.AvaloniaList<Point>(new[]{
                new Point(pts[0].X, pts[0].Y+10),
                new Point(pts[1].X, pts[1].Y+10),
                new Point(pts[2].X, pts[2].Y+10),
                new Point(pts[3].X, pts[3].Y+10)
            }),
            Fill = Brushes.Gray,
            Opacity = 0.3
        };
        canvas.Children.Add(shadow);

        // Piirrä laivan runko
        var hull = new Avalonia.Controls.Shapes.Polygon
        {
            Points = new Avalonia.Collections.AvaloniaList<Point>(pts),
            Fill = Brushes.SaddleBrown,
            Stroke = Brushes.Black,
            StrokeThickness = 2
        };
        canvas.Children.Add(hull);

        // Piirrä kansi (vaaleampi suorakulmio trapetsin yläreunan päälle)
        var deck = new Avalonia.Controls.Shapes.Polyline
        {
            Points = new Avalonia.Collections.AvaloniaList<Point>(new[] { pts[0], pts[1] }),
            Stroke = Brushes.BurlyWood,
            StrokeThickness = 6
        };
        canvas.Children.Add(deck);
    }
}
