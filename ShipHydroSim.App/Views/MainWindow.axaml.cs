using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using ShipHydroSim.App.ViewModels;
using System;

namespace ShipHydroSim.App.Views;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _renderTimer;
    private Point? _lastMousePosition;
    
    public MainWindow()
    {
        InitializeComponent();
        
        // Setup rendering loop
        _renderTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33) // ~30 FPS
        };
        _renderTimer.Tick += OnRenderTick;
        _renderTimer.Start();
        
        // Mouse controls for camera
        Viewport.PointerPressed += OnViewportPointerPressed;
        Viewport.PointerMoved += OnViewportPointerMoved;
        Viewport.PointerReleased += OnViewportPointerReleased;
        Viewport.PointerWheelChanged += OnViewportWheel;
    }
    
    private void OnRenderTick(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            Viewport.UpdateScene(vm.Engine.Solver.Particles, vm.Engine.Solver.RigidBodies);
        }
    }
    
    private void OnViewportPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _lastMousePosition = e.GetPosition(Viewport);
    }
    
    private void OnViewportPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_lastMousePosition.HasValue && e.GetCurrentPoint(Viewport).Properties.IsLeftButtonPressed)
        {
            var currentPos = e.GetPosition(Viewport);
            var delta = currentPos - _lastMousePosition.Value;
            
            Viewport.RotateCamera((float)delta.X * 0.5f, (float)delta.Y * 0.5f);
            
            _lastMousePosition = currentPos;
        }
    }
    
    private void OnViewportPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _lastMousePosition = null;
    }
    
    private void OnViewportWheel(object? sender, PointerWheelEventArgs e)
    {
        Viewport.ZoomCamera((float)-e.Delta.Y * 5f);
    }
}