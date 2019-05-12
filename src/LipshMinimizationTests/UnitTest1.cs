using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using LipshMinimizationMath;

namespace LipshMinimizationTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TwoDimension()
        {
            double a    = -2
                , b     = 3
                , d     = -Math.PI
                , c     = 5 * Math.PI
                , e     = 0.01
                , e2    = 0.1
                , L     = 1.0 / (4.0 * e) + 1;

            // Рассматриваемая функция
            double F(double x, double y)
                => Math.Abs(x) + Math.Sqrt(Math.Abs(Math.Sin(y)));

            var result = MathStrategy.UniformSearchByBiryukov(
                    F,
                    a, b,   // [a;b]
                    d, c,   // [d;c]
                    L,      // L=L(e)=1/(4e)
                    e, e2); // e, e*

            Assert.IsTrue((result.F - 0) <= e2);
        }

        [TestMethod]
        public void OneDimension()
        {
            double a    = -2 * Math.PI          // Input<double>("a="),
               , b      = 3 * Math.PI           // Input<double>("b="),
               , e      = 0.0001
               , e2     = 0.001
               , L      = 1.0 / (4.0 * e) + 1;  // Input<double>("L="),

            // Рассматриваемая функция
            double F(double x)
                => Math.Abs(x) + Math.Sqrt(Math.Abs(Math.Sin(x)));

            var result = MathStrategy.UniformSearchByBiryukov(
                   F,
                   a.ToRadians(), b.ToRadians(),   // [a;b]
                   L,                              // L=L(e)=1/(4e)
                   e, e2);                         // e, e*

            Assert.IsTrue((result.F - 0) <= e2);
        }
    }
}
