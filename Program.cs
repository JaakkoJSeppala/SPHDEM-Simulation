using Avalonia;
using System;
using SPHDEM_Simulation_New.SPH;
using SPHDEM_Simulation_New.DEM;
using SPHDEM_Simulation_New.Ship;
using System.Threading.Tasks;

namespace SPHDEM_Simulation_New;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Length > 0 && (args[0] == "csv" || args[0] == "json"))
        {
            Console.WriteLine($"Ajetaan testitapaukset ({args[0]})...");
            var runner = new TestRunner();
            Task.Run(async () => {
                await runner.RunTestCasesAsync();
                Console.WriteLine($"Testitapaukset suoritettu. Tulokset tallennettu ({args[0]}) tiedostoihin.");
                Environment.Exit(0);
            }).Wait();
            return;
        }
        // Automaattitestit
        Console.WriteLine("SPH tank boundary test:");
        SPH.TankBoundaryTest.RunTest();
        Console.WriteLine("DEM tank boundary test:");
        DEM.TankBoundaryTest.RunTest();
        Console.WriteLine("Ship boundary test:");
        Ship.ShipBoundaryTest.RunTest();
        // Käynnistä Avalonia
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
