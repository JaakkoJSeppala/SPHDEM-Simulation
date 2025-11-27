using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Controls.Shapes;
using Avalonia.Threading;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;
using SPHDEM_Simulation_New.SPH;
using SPHDEM_Simulation_New.DEM;
using System;
using SPHDEM_Simulation_New.Coupling;
using System.IO;
using System.Globalization;
using System.Threading.Tasks;

namespace SPHDEM_Simulation_New {
    public partial class MainWindow : Window {
                // CLI-konstruktori
                public MainWindow(bool cliMode) : base() {
                    // Älä alusta UI-komponentteja
                    simulationCanvas = null;
                    waveTypeComboBox = null;
                    frequencyUpDown = null;
                    amplitudeUpDown = null;
                    methodComboBox = null;
                    demSizeUpDown = null;
                    tankLengthUpDown = null;
                    tankHeightUpDown = null;
                    waterHeightUpDown = null;
                    shipMassUpDown = null;
                    runTestsButton = null;
                    InitParticles();
                    InitDEMParticles();
                    // Oletusparametrit
                    double tankHeight = CanvasHeight * 0.5;
                    double tankY = CanvasHeight - tankHeight;
                    double tankLength = CanvasWidth * 0.8;
                    double tankX = CanvasWidth * 0.1;
                    double waterHeight = tankHeight * 0.15;
                    double shipMass = 500;
                    ship = new Ship.Ship(tankX + tankLength / 2 - 40, tankY + waterHeight - 30) { Mass = shipMass };
                    ship.Height = 30;
                }
        private List<SPHParticle> particles = new();
        private SPHSimulator simulator = null!;
        private List<DEMParticle> demParticles = new();
        private DEMSimulator demSimulator = null!;
        private Canvas? simulationCanvas;
        private ComboBox? methodComboBox;
        private NumericUpDown? demSizeUpDown;
        private NumericUpDown? tankLengthUpDown;
        private NumericUpDown? tankHeightUpDown;
        private NumericUpDown? waterHeightUpDown;
        private NumericUpDown? shipMassUpDown;
        private Random rand = new Random();
        private const int ParticleCount = 100;
        private const double CanvasWidth = 760;
        private const double CanvasHeight = 400;
        private Ship.Ship ship;
        private SPHDEMCoupler coupler = new SPHDEMCoupler();
        private StreamWriter? resultsWriter;
        private bool resultsInitialized = false;

        private Button? runTestsButton;

        public MainWindow() : base() {
            AvaloniaXamlLoader.Load(this);
            simulationCanvas = this.FindControl<Canvas>("SimulationCanvas");
            waveTypeComboBox = this.FindControl<ComboBox>("WaveTypeComboBox");
            frequencyUpDown = this.FindControl<NumericUpDown>("FrequencyUpDown");
            amplitudeUpDown = this.FindControl<NumericUpDown>("AmplitudeUpDown");
            methodComboBox = this.FindControl<ComboBox>("MethodComboBox");
            demSizeUpDown = this.FindControl<NumericUpDown>("DEMSizeUpDown");
            tankLengthUpDown = this.FindControl<NumericUpDown>("TankLengthUpDown");
            tankHeightUpDown = this.FindControl<NumericUpDown>("TankHeightUpDown");
            waterHeightUpDown = this.FindControl<NumericUpDown>("WaterHeightUpDown");
            shipMassUpDown = this.FindControl<NumericUpDown>("ShipMassUpDown");
            runTestsButton = this.FindControl<Button>("RunTestsButton");
            if (runTestsButton != null)
                runTestsButton.Click += async (_, __) => await RunTestCasesAsync();
            InitParticles();
            InitDEMParticles();
            // Tankin ja veden parametrit
            double tankHeight = tankHeightUpDown != null && tankHeightUpDown.Value.HasValue ? (double)tankHeightUpDown.Value.Value : CanvasHeight * 0.5;
            double tankY = CanvasHeight - tankHeight;
            double tankLength = tankLengthUpDown != null && tankLengthUpDown.Value.HasValue ? (double)tankLengthUpDown.Value.Value : CanvasWidth * 0.8;
            double tankX = CanvasWidth * 0.1;
            double waterHeight = waterHeightUpDown != null && waterHeightUpDown.Value.HasValue ? (double)waterHeightUpDown.Value.Value : tankHeight * 0.15;
            double shipMass = shipMassUpDown != null && shipMassUpDown.Value.HasValue ? (double)shipMassUpDown.Value.Value : 500;
            // Laiva veden pinnan yläpuolelle
            ship = new Ship.Ship(tankX + tankLength / 2 - 40, tankY + waterHeight - 30) { Mass = shipMass };
            ship.Height = 30; // oletuskorkeus
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            timer.Tick += (s, e) => RenderFrame();
            timer.Start();
        }

        private void InitParticles() {
            particles.Clear();
            double tankHeight = tankHeightUpDown != null && tankHeightUpDown.Value.HasValue ? (double)tankHeightUpDown.Value.Value : CanvasHeight * 0.5;
            double tankLength = tankLengthUpDown != null && tankLengthUpDown.Value.HasValue ? (double)tankLengthUpDown.Value.Value : CanvasWidth * 0.8;
            double tankX = CanvasWidth * 0.1;
            double waterHeight = waterHeightUpDown != null && waterHeightUpDown.Value.HasValue ? (double)waterHeightUpDown.Value.Value : tankHeight * 0.15;
            double waterY = CanvasHeight - waterHeight;
            simulator = new SPHSimulator(particles) {
                Tank = new Ship.Tank {
                    Height = tankHeight,
                    Y = CanvasHeight - tankHeight,
                    Length = tankLength,
                    X = tankX
                }
            };
            for (int i = 0; i < ParticleCount; i++) {
                double x = tankX + rand.NextDouble() * tankLength;
                double y = waterY + rand.NextDouble() * waterHeight;
                particles.Add(new SPHParticle(x, y, 1.0));
            }
        }
        private void InitDEMParticles() {
            demParticles.Clear();
            int demCount = 30;
            double demRadius = 6;
            double demMass = 2.0;
            for (int i = 0; i < demCount; i++) {
                double x = CanvasWidth / 2 + (i % 10) * demRadius * 2 - 30;
                double y = CanvasHeight * 0.5 + (i / 10) * demRadius * 2;
                demParticles.Add(new DEMParticle { X = x, Y = y, Mass = demMass, Radius = demRadius });
            }
            demSimulator = new DEMSimulator(demParticles);
        }

        private ComboBox? waveTypeComboBox;
        private NumericUpDown? frequencyUpDown;
        private NumericUpDown? amplitudeUpDown;

        private void ApplyWave(double time) {
            if (waveTypeComboBox == null || frequencyUpDown == null || amplitudeUpDown == null)
                return;
            string waveType = (waveTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "None";
            double freq = frequencyUpDown.Value.HasValue ? (double)frequencyUpDown.Value.Value : 1.0;
            double amp = amplitudeUpDown.Value.HasValue ? (double)amplitudeUpDown.Value.Value : 20.0;
            switch (waveType) {
                case "Sine":
                    foreach (var p in particles) {
                        p.Y += amp * Math.Sin(2 * Math.PI * freq * time + p.X / 80.0) * 0.01;
                    }
                    break;
                case "Square":
                    foreach (var p in particles) {
                        double val = Math.Sin(2 * Math.PI * freq * time + p.X / 80.0);
                        p.Y += amp * (val > 0 ? 1 : -1) * 0.01;
                    }
                    break;
                case "Custom":
                    foreach (var p in particles) {
                        p.Y += amp * Math.Sin(2 * Math.PI * freq * time + Math.Sin(p.X / 40.0)) * 0.01;
                    }
                    break;
                default:
                    break;
            }
        }

        private void InitResultsWriter() {
            if (!resultsInitialized) {
                resultsWriter = new StreamWriter("simulation_results.csv", false);
                resultsWriter.WriteLine("Time,ShipY,WaterLevel,ParticleCount,Damping,SloshingAmplitude,ShipTrajectory,EnergyTransfer");
                resultsInitialized = true;
            }
        }

        private double elapsedTime = 0;
        private double maxWaterLevel = double.MinValue;
        private double minWaterLevel = double.MaxValue;
        private double maxShipY = double.MinValue;
        private double minShipY = double.MaxValue;
        private double initialShipVY = 0;
        private double initialShipKE = 0;
        private bool metricsInitialized = false;
        private void UpdateMetrics(double waterLevel, double shipY, double shipVY) {
            if (!metricsInitialized) {
                initialShipVY = shipVY;
                initialShipKE = 0.5 * ship.Mass * shipVY * shipVY;
                metricsInitialized = true;
            }
            if (waterLevel > maxWaterLevel) maxWaterLevel = waterLevel;
            if (waterLevel < minWaterLevel) minWaterLevel = waterLevel;
            if (shipY > maxShipY) maxShipY = shipY;
            if (shipY < minShipY) minShipY = shipY;
        }
        private double GetDamping(double shipVY) {
            return initialShipVY != 0 ? Math.Abs(shipVY / initialShipVY) : 0;
        }
        private double GetSloshingAmplitude() {
            return maxWaterLevel - minWaterLevel;
        }
        private double GetShipTrajectory() {
            return maxShipY - minShipY;
        }
        private double GetEnergyTransfer(double shipVY) {
            double currentKE = 0.5 * ship.Mass * shipVY * shipVY;
            return initialShipKE != 0 ? currentKE / initialShipKE : 0;
        }

        private void RenderFrame() {
            // Update DEM particle size from UI
            if (demSizeUpDown != null) {
                double newRadius = demSizeUpDown.Value.HasValue ? (double)demSizeUpDown.Value.Value : 6.0;
                foreach (var dp in demParticles) dp.Radius = newRadius;
            }
            // Get selected method
            string method = methodComboBox != null && methodComboBox.SelectedItem is ComboBoxItem item ? item.Content?.ToString() ?? "SPH-DEM" : "SPH-DEM";

            // Update simulation
            if (method == "SPH" || method == "SPH-DEM") simulator.Update();
            if (method == "DEM" || method == "SPH-DEM") demSimulator.Update();
            if (method == "SPH-DEM") coupler.ApplyCoupling(particles, demParticles);
            elapsedTime += 0.016;
            ApplyWave(elapsedTime);
            ship.UpdatePhysics(particles, 0.016);
            simulationCanvas?.Children.Clear();
            // Tallennetaan tulokset
            double waterLevel = particles.Count > 0 ? particles[0].Y : 0;
            double shipVY = ship.VY;
            UpdateMetrics(waterLevel, ship.Y, shipVY);
            // Tallennetaan tulokset
            InitResultsWriter();
            resultsWriter?.WriteLine(string.Format(CultureInfo.InvariantCulture,
                "{0:F3},{1:F2},{2:F2},{3},{4:F3},{5:F2},{6:F2},{7:F2}",
                elapsedTime, ship.Y, waterLevel, particles.Count,
                GetDamping(shipVY), GetSloshingAmplitude(), GetShipTrajectory(), GetEnergyTransfer(shipVY)));
            resultsWriter?.Flush();
            // Draw boundaries and ship
            // ...existing code...
            // Draw DEM particles if DEM or SPH-DEM selected
            if (method == "DEM" || method == "SPH-DEM") {
                foreach (var dp in demParticles) {
                    var demEllipse = new Ellipse {
                        Width = dp.Radius * 2,
                        Height = dp.Radius * 2,
                        Fill = Brushes.Orange,
                        Stroke = Brushes.DarkRed,
                        StrokeThickness = 1
                    };
                    simulationCanvas?.Children.Add(demEllipse);
                    if (demEllipse != null) {
                        Canvas.SetLeft(demEllipse, dp.X - dp.Radius);
                        Canvas.SetTop(demEllipse, dp.Y - dp.Radius);
                    }
                }
            }
            // Draw SPH particles if SPH or SPH-DEM selected
            if (method == "SPH" || method == "SPH-DEM") {
                foreach (var p in particles) {
                    var ellipse = new Ellipse {
                        Width = 6,
                        Height = 6,
                        Fill = Brushes.Blue
                    };
                    simulationCanvas?.Children.Add(ellipse);
                    if (ellipse != null) {
                        Canvas.SetLeft(ellipse, p.X);
                        Canvas.SetTop(ellipse, p.Y);
                    }
                }
            }

            // Draw boundaries
            simulationCanvas?.Children.Add(new Line {
                StartPoint = new Point(0, 0), EndPoint = new Point(CanvasWidth, 0), Stroke = Brushes.Black, StrokeThickness = 2 });
            simulationCanvas?.Children.Add(new Line {
                StartPoint = new Point(0, CanvasHeight), EndPoint = new Point(CanvasWidth, CanvasHeight), Stroke = Brushes.Black, StrokeThickness = 2 });
            simulationCanvas?.Children.Add(new Line {
                StartPoint = new Point(0, 0), EndPoint = new Point(0, CanvasHeight), Stroke = Brushes.Black, StrokeThickness = 2 });
            simulationCanvas?.Children.Add(new Line {
                StartPoint = new Point(CanvasWidth, 0), EndPoint = new Point(CanvasWidth, CanvasHeight), Stroke = Brushes.Black, StrokeThickness = 2 });
            // Tankin pohja ja veden pinta
            double tankHeight = CanvasHeight * 0.5;
            double tankY = CanvasHeight - tankHeight;
            double tankLength = CanvasWidth * 0.8;
            double tankX = CanvasWidth * 0.1;
            double waterHeight = tankHeight * 0.15;
            // Tankin pohja
            simulationCanvas?.Children.Add(new Line {
                StartPoint = new Point(tankX, tankY), EndPoint = new Point(tankX + tankLength, tankY), Stroke = Brushes.DarkGray, StrokeThickness = 3 });
            // Veden pinta
            simulationCanvas?.Children.Add(new Line {
                StartPoint = new Point(tankX, tankY + waterHeight), EndPoint = new Point(tankX + tankLength, tankY + waterHeight), Stroke = Brushes.LightBlue, StrokeThickness = 3 });

            // Draw ship (rectangle)
            var shipRect = new Rectangle {
                Width = ship.Width,
                Height = ship.Height,
                Fill = Brushes.Gray,
                Stroke = Brushes.DarkBlue,
                StrokeThickness = 3
            };
            simulationCanvas?.Children.Add(shipRect);
            if (shipRect != null) {
                Canvas.SetLeft(shipRect, ship.X);
                Canvas.SetTop(shipRect, ship.Y);
            }

            // Draw DEM particles
            foreach (var dp in demParticles) {
                var demEllipse = new Ellipse {
                    Width = dp.Radius * 2,
                    Height = dp.Radius * 2,
                    Fill = Brushes.Orange,
                    Stroke = Brushes.DarkRed,
                    StrokeThickness = 1
                };
                Canvas.SetLeft(demEllipse, dp.X - dp.Radius);
                Canvas.SetTop(demEllipse, dp.Y - dp.Radius);
                simulationCanvas?.Children.Add(demEllipse);
            }

            // Draw particles
            foreach (var p in particles) {
                var ellipse = new Ellipse {
                    Width = 6,
                    Height = 6,
                    Fill = Brushes.Blue
                };
                Canvas.SetLeft(ellipse, p.X);
                Canvas.SetTop(ellipse, p.Y);
                simulationCanvas?.Children.Add(ellipse);
            }
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
            resultsWriter?.Close();
        }

        public async Task RunTestCasesAsync() {
            try {
                var cases = SimulationTestCases.GetDefaultCases();
                foreach (var testCase in cases) {
                    // Jos UI-komponentit ovat null (CLI-ajo), käytä suoraan arvoja
                    double tankLength = tankLengthUpDown != null ? (double)tankLengthUpDown.Value! : testCase.TankLength;
                    double tankHeight = tankHeightUpDown != null ? (double)tankHeightUpDown.Value! : testCase.TankHeight;
                    double waterHeight = waterHeightUpDown != null ? (double)waterHeightUpDown.Value! : testCase.WaterHeight;
                    double shipMass = shipMassUpDown != null ? (double)shipMassUpDown.Value! : testCase.ShipMass;
                    double waveFreq = frequencyUpDown != null ? (double)frequencyUpDown.Value! : testCase.WaveFrequency;
                    double waveAmp = amplitudeUpDown != null ? (double)amplitudeUpDown.Value! : testCase.WaveAmplitude;

                    // Aseta parametrit
                    if (tankLengthUpDown != null) tankLengthUpDown.Value = (decimal)tankLength;
                    if (tankHeightUpDown != null) tankHeightUpDown.Value = (decimal)tankHeight;
                    if (waterHeightUpDown != null) waterHeightUpDown.Value = (decimal)waterHeight;
                    if (shipMassUpDown != null) shipMassUpDown.Value = (decimal)shipMass;
                    if (frequencyUpDown != null) frequencyUpDown.Value = (decimal)waveFreq;
                    if (amplitudeUpDown != null) amplitudeUpDown.Value = (decimal)waveAmp;

                    // Alusta partikkelit
                    InitParticles();
                    InitDEMParticles();
                    ship = new Ship.Ship(tankLength / 2, waterHeight - 30) { Mass = shipMass };

                    // Tallennetaan tulokset omaan tiedostoon
                    resultsWriter = new StreamWriter($"results_{testCase.Name}.csv", false);
                    resultsWriter.WriteLine("Time,ShipY,WaterLevel,ParticleCount,Damping,SloshingAmplitude,ShipTrajectory,EnergyTransfer");
                    resultsInitialized = true;
                    metricsInitialized = false;
                    maxWaterLevel = double.MinValue;
                    minWaterLevel = double.MaxValue;
                    maxShipY = double.MinValue;
                    minShipY = double.MaxValue;
                    initialShipVY = 0;
                    initialShipKE = 0;
                    // Aja simulaatio 1000 askelta
                    elapsedTime = 0;
                    for (int i = 0; i < 1000; i++) {
                        RenderFrame();
                        // UI-tilassa pieni viive, CLI:ssä ei tarvita
                        if (runTestsButton != null) await Task.Delay(1);
                    }
                    resultsWriter?.Close();
                }
            } catch (Exception ex) {
                Console.WriteLine($"Virhe testitapausten ajossa: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }
    }
}