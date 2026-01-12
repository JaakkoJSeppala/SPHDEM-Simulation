# Dam-break + rigid cube SPH-simulaatio PySPH:lla
# Tämä on yksinkertaistettu esimerkki, joka pohjautuu PySPH:n dam_break_2d- ja rigid_body_dam_break -esimerkkeihin.
# Varmista, että PySPH on asennettu: pip install pysph

from pysph.base.utils import get_particle_array
from pysph.solver.application import Application
from pysph.sph.scheme import WCSPHScheme
from pysph.sph.rigid_body import RigidBody3DScheme
import numpy as np

class DamBreakWithCube(Application):
    def create_particles(self):
        # Säiliön mitat
        tank_length = 1.0
        tank_height = 0.5
        dx = 0.02
        h = 1.3 * dx
        # Nesteen mitat
        fluid_length = 0.4
        fluid_height = 0.4
        # Kuutio
        cube_size = 0.08
        cube_x = 0.7
        cube_y = 0.1

        # Nesteen partikkelit
        x, y = np.mgrid[dx/2:fluid_length:dx, dx/2:fluid_height:dx]
        x = x.ravel()
        y = y.ravel()
        fluid = get_particle_array(name='fluid', x=x, y=y, h=h, m=dx*dx*1000, rho=1000)

        # Seinät
        wx1, wy1 = np.mgrid[dx/2:tank_length:dx, -dx/2:0:dx]
        wx2, wy2 = np.mgrid[dx/2:tank_length:dx, tank_height: tank_height+dx/2:dx]
        wx = np.concatenate([wx1.ravel(), wx2.ravel()])
        wy = np.concatenate([wy1.ravel(), wy2.ravel()])
        wall = get_particle_array(name='wall', x=wx, y=wy, h=h, m=dx*dx*1000, rho=1000)
        # Vasemman ja oikean reunan seinät
        wxl, wyl = np.mgrid[-dx/2:0:dx, 0:tank_height:dx]
        wxr, wyr = np.mgrid[tank_length:tank_length+dx/2:dx, 0:tank_height:dx]
        wall.add_particles(x=np.concatenate([wxl.ravel(), wxr.ravel()]),
                          y=np.concatenate([wyl.ravel(), wyr.ravel()]))

        # Kuutio (jäykkä kappale)
        cx, cy = np.mgrid[cube_x:cube_x+cube_size:dx, cube_y:cube_y+cube_size:dx]
        cube = get_particle_array(name='cube', x=cx.ravel(), y=cy.ravel(), h=h, m=dx*dx*1000, rho=1000)
        cube.add_property('body_id')
        cube.add_property('contact_force_is_boundary')
        cube.body_id[:] = 0
        cube.contact_force_is_boundary[:] = 1

        return [fluid, wall, cube]

    def create_scheme(self):
        s = RigidBody3DScheme(
            fluids=['fluid'], solids=['cube'], boundaries=['wall'],
            dim=2, rho0=1000, c0=10, h0=0.026, hdx=1.3,
            gamma=7.0, alpha=0.1, beta=0.0, nu=0.0
        )
        return s

    def configure_scheme(self):
        scheme = self.scheme
        scheme.configure_solver(tf=1.0, dt=1e-4, adaptive_timestep=True)

if __name__ == '__main__':
    app = DamBreakWithCube()
    app.run()
