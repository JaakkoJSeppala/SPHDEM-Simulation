using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ShipDamperSim.Core
{
    public static class CsvExporter
    {
        public static void WriteEnergies(string path, List<float> time, List<float> shipKin, List<float> granKin, List<float> diss)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Time,ShipKinetic,GranularKinetic,Dissipated");
            for (int i = 0; i < time.Count; i++)
            {
                sb.AppendLine($"{time[i]},{shipKin[i]},{granKin[i]},{diss[i]}");
            }
            File.WriteAllText(path, sb.ToString());
        }
    }
}
