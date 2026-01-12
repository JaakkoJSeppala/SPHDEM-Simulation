"""
Unit tests for SPH-DEM ship simulation (sph_2d_example.py)
"""
import unittest
import numpy as np
from sph_2d_example import Simulaatio, compute_buoyancy, compute_damper_reaction

class TestSimulationUtils(unittest.TestCase):
    def test_buoyancy_zero(self):
        # No particles under ship
        pos = np.array([[0.5, 0.5], [0.6, 0.6]])
        p = np.array([0.0, 0.0])
        result = compute_buoyancy(pos, p, 0.0, 0.1, 0.1, 1.0, 2)
        self.assertEqual(result, 0.0)

    def test_damper_reaction_zero(self):
        # No DEM particles at bottom
        damper_pos = np.array([[0.5, 0.5], [0.6, 0.6]])
        damper_r = 0.01
        damper_y = 0.0
        damper_k = 1000.0
        DEM_N = 2
        result = compute_damper_reaction(damper_pos, damper_r, damper_y, damper_k, DEM_N)
        self.assertEqual(result, 0.0)

class TestSimulationRun(unittest.TestCase):
    def test_simulation_runs(self):
        # Basic run should not raise exceptions
        sim = Simulaatio(N=10, L=1.0, h=0.07, m=0.02, rho0=1.0, k=1000.0, mu=0.1,
                        G=np.array([0, -9.81]), dt=0.002, steps=5, fill_frac=0.2)
        result = sim.aja()
        self.assertIn('ship_y_hist', result)
        self.assertIn('damper_kin_energy_hist', result)
        self.assertEqual(len(result['ship_y_hist']), 5)
        self.assertEqual(len(result['damper_kin_energy_hist']), 5)

if __name__ == "__main__":
    unittest.main()
