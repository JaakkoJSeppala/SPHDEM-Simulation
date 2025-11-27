namespace SPHDEM_Simulation_New.DEM {
    public class DEMParticle {
        public double X, Y;
        public double Mass;
        public double[] Velocity = new double[2];
        public double[] Force = new double[2];
        public double Radius;
        public double Density;
        public double Restitution = 0.5;
        public double Friction = 0.3;
        // Add more properties as needed
    }
}
