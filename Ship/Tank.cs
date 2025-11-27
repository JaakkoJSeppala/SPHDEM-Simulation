using System;

namespace SPHDEM_Simulation_New.Ship {
    /// <summary>
    /// Ballastitankin malli. Geometria ja reunaehdot perustuvat Faltinsen (2000) ja Konar et al. (2023).
    /// </summary>
    public class Tank {
        // Tankin mitat (m) - tyypillinen suorakulmainen ballastitankki
        public double Width { get; set; } = 4.0;   // Faltinsen 2000
        public double Height { get; set; } = 3.0;  // Faltinsen 2000
        public double Length { get; set; } = 10.0; // Konar 2023

        // Sijainti simulaatiossa
        public double X { get; set; } = 0.0;
        public double Y { get; set; } = 0.0;

        /// <summary>
        /// Tarkistaa, onko piste (x, y) tankin sisällä. Käytetään reunaehtojen toteutukseen.
        /// Viittaa: Faltinsen 2000, luku 2.2 (reunaehdot)
        /// </summary>
        public bool IsInside(double x, double y) {
            return x >= X && x <= X + Length && y >= Y && y <= Y + Height;
        }

        /// <summary>
        /// Korjaa partikkelin sijainnin tankin sisälle ja palauttaa korjatun arvon.
        /// Viittaa: Faltinsen 2000, luku 2.2 (reunaehdot)
        /// </summary>
        public (double x, double y) EnforceBoundaries(double x, double y, double restitution = 0.5) {
            double newX = x, newY = y;
            if (x < X) newX = X + restitution * (X - x);
            if (x > X + Length) newX = X + Length - restitution * (x - (X + Length));
            if (y < Y) newY = Y + restitution * (Y - y);
            if (y > Y + Height) newY = Y + Height - restitution * (y - (Y + Height));
            return (newX, newY);
        }
    }
}
