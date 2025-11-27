namespace SPHDEM_Simulation_New.SPH {
    public class SPHParticle {
        public double X { get; set; }
        public double Y { get; set; }
        public double VX { get; set; }
        public double VY { get; set; }
        public double Mass { get; set; }
        public double Density { get; set; }
        public double Pressure { get; set; }

        public SPHParticle(double x, double y, double mass) {
            X = x;
            Y = y;
            VX = 0;
            VY = 0;
            Mass = mass;
            Density = 0;
            Pressure = 0;
        }
    }
}
