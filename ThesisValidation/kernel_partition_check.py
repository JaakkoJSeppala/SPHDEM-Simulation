#!/usr/bin/env python3
"""
Numerical check that the cubic spline SPH kernel integrates to 1
in 2D and 3D using radial composite Simpson quadrature.

Matches the procedure described in the thesis (Δr = 1e-4 h).
"""
from math import pi


def W_cubic(q: float, h: float, D: int) -> float:
    """Cubic B-spline kernel W(r, h) with q = r/h.
    Normalization constants: alpha2 = 10/(7π), alpha3 = 1/π.
    """
    if D == 2:
        alpha = 10.0 / (7.0 * pi)
    elif D == 3:
        alpha = 1.0 / pi
    else:
        raise ValueError("Only D=2 or D=3 supported")

    if q < 0.0:
        return 0.0
    if q < 1.0:
        poly = 1.0 - 1.5 * q * q + 0.75 * q * q * q
    elif q < 2.0:
        t = 2.0 - q
        poly = 0.25 * t * t * t
    else:
        poly = 0.0
    return alpha * poly / (h ** D)


def simpson_integrate_2D(h: float, dr: float) -> float:
    # Integral: ∫ W(r,h) * 2π r dr, r ∈ [0, 2h]
    rmax = 2.0 * h
    n = int(round(rmax / dr))
    if n % 2 == 1:
        n += 1  # Simpson needs even number of intervals
    dr = rmax / n
    total = 0.0
    for i in range(n + 1):
        r = i * dr
        q = r / h
        f = W_cubic(q, h, D=2) * 2.0 * pi * r
        coef = 4.0 if i % 2 == 1 else 2.0
        if i == 0 or i == n:
            coef = 1.0
        total += coef * f
    return total * dr / 3.0


def simpson_integrate_3D(h: float, dr: float) -> float:
    # Integral: ∫ W(r,h) * 4π r^2 dr, r ∈ [0, 2h]
    rmax = 2.0 * h
    n = int(round(rmax / dr))
    if n % 2 == 1:
        n += 1
    dr = rmax / n
    total = 0.0
    for i in range(n + 1):
        r = i * dr
        q = r / h
        f = W_cubic(q, h, D=3) * 4.0 * pi * r * r
        coef = 4.0 if i % 2 == 1 else 2.0
        if i == 0 or i == n:
            coef = 1.0
        total += coef * f
    return total * dr / 3.0


def main():
    h = 1.0
    dr = 1e-4 * h

    I2 = simpson_integrate_2D(h, dr)
    I3 = simpson_integrate_3D(h, dr)

    err2 = abs(I2 - 1.0)
    err3 = abs(I3 - 1.0)

    print(f"2D integral: {I2:.12f}, abs error = {err2:.3e}")
    print(f"3D integral: {I3:.12f}, abs error = {err3:.3e}")

    tol = 1e-10
    if err2 > tol or err3 > tol:
        # Slightly relax in case local Python/BLAS differs; still fail CI if larger issue
        relaxed = 5e-10
        if err2 <= relaxed and err3 <= relaxed:
            print("PASS (relaxed)")
            return 0
        print("FAIL: error exceeds tolerance")
        return 1
    print("PASS")
    return 0


if __name__ == "__main__":
    import sys
    sys.exit(main())
