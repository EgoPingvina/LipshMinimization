using System;

using Xunit;

using LipshMinimization.ELipschitzMath;
using FluentAssertions;

namespace LipshMinimizationTests
{
    public sealed class ELipschitzMathTests
    {
        private double L(double e)
            => 1.0 / (4.0 * e) + 1;

        [Theory]
        [InlineData(-2,     3,  -Math.PI,        3 * Math.PI,    0.01,  0.1)]
        [InlineData(-1,     1,  -Math.PI/2.0,    Math.PI/2.0,    0.01,  0.02)]
        [InlineData(0,      1,  0,               Math.PI,        0.05,  0.1)]
        public void TwoDimensionF1(double a, double b, double d, double c, double e, double e2)
        {
            // Рассматриваемая функция
            double F(double x, double y)
                => Math.Abs(x) + Math.Sqrt(Math.Abs(Math.Sin(y)));

            double fMin = 0;

            var result  = ELipschitzMath.UniformSearchByBiryukov(
                    F,
                    a, b,   // [a;b]
                    d, c,   // [d;c]
                    L(e),   // L=L(e)=1/(4e)
                    e, e2); // e, e*

            (result.F - fMin).Should().BeLessOrEqualTo(e2);
        }

        [Theory]
        [InlineData(-Math.PI,       3 * Math.PI,    0.001,  0.01)]
        [InlineData(-Math.PI/2.0,   Math.PI/2.0,    0.0005,  0.001)]
        [InlineData(0,              Math.PI,        0.01,   0.1)]
        public void OneDimensionF1(double a, double b, double e, double e2)
        {
            // Рассматриваемая функция
            double F(double x)
                => Math.Abs(x) + Math.Sqrt(Math.Abs(Math.Sin(x)));

            double fMin = 0;

            var result  = ELipschitzMath.UniformSearchByBiryukov(
                   F,
                   a, b,    // [a;b]
                   L(e),    // L=L(e)=1/(4e)
                   e, e2);  // e, e*

            (result.F - fMin).Should().BeLessOrEqualTo(e2);
        }
    }
}
