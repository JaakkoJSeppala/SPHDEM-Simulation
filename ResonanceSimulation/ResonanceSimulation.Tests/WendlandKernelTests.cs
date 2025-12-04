using System;
using Xunit;
using ResonanceSimulation.Core;

namespace ResonanceSimulation.Tests.SPH
{
    public class WendlandKernelTests
    {
        [Fact]
        public void W_ReturnsExpectedValue_AtKeyPoints()
        {
            // At r=0, should be maximum: W(0,1) = 7/(4*pi) ≈ 0.557042
            double expectedAtZero = 7.0 / (4.0 * Math.PI);
            double actualAtZero = WendlandKernel.W(0.0, 1.0);
            Assert.Equal(expectedAtZero, actualAtZero, 6);

            // At r=2h, should be zero
            Assert.Equal(0.0, WendlandKernel.W(2.0, 1.0), 6);

            // At r=h, should be positive
            double valueAtH = WendlandKernel.W(1.0, 1.0);
            Assert.True(valueAtH > 0.0);
        }

        [Fact]
        public void GradW_ZeroAtOriginAndOutsideSupport()
        {
            Assert.Equal(0.0, WendlandKernel.GradW(0.0, 1.0), 6);
            Assert.Equal(0.0, WendlandKernel.GradW(2.0, 1.0), 6);
        }

        [Fact]
        public void LaplacianW_ZeroOutsideSupport()
        {
            Assert.Equal(0.0, WendlandKernel.LaplacianW(2.0, 1.0), 6);
        }

        [Fact]
        public void W_IsSymmetricAndPositiveWithinSupport()
        {
            double h = 1.0;
            for (double r = 0.0; r < 2.0 * h; r += 0.1)
            {
                double w1 = WendlandKernel.W(r, h);
                double w2 = WendlandKernel.W(-r, h);
                Assert.True(w1 >= 0.0);
                Assert.Equal(w1, w2, 6);
            }
        }
    }
}
