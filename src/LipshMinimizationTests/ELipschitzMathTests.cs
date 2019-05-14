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

            // ожидаемый результат
            double fMin = 0;

            // полученное решение
            var result  = MathStrategy.UniformSearchByBiryukov(
                    F,
                    a, b,   // [a;b]
                    d, c,   // [d;c]
                    L(e),   // L=L(e)=1/(4e)
                    e, e2); // e, e*

            // Проверка прохождения теста
            (result.F - fMin).Should().BeLessOrEqualTo(e2);
        }

        [Theory]
        [InlineData(-2, 3, -Math.PI, 3 * Math.PI, 0.01, 0.1)]
        [InlineData(-1, 1, -Math.PI / 2.0, Math.PI / 2.0, 0.01, 0.02)]
        [InlineData(0, 1, 0, Math.PI, 0.05, 0.1)]
        public void TwoDimensionF2(double a, double b, double d, double c, double e, double e2)
        {
            // Рассматриваемая функция
            double F(double x, double y)
                => Math.Sqrt(Math.Abs(x)) + Math.Abs(Math.Sin(y));

            // ожидаемый результат
            double fMin = 0;

            // полученное решение
            var result = MathStrategy.UniformSearchByBiryukov(
                    F,
                    a, b,   // [a;b]
                    d, c,   // [d;c]
                    L(e),   // L=L(e)=1/(4e)
                    e, e2); // e, e*

            // Проверка прохождения теста
            (result.F - fMin).Should().BeLessOrEqualTo(e2);
        }

        [Theory]
        [InlineData(-Math.PI, 3 * Math.PI, 0.001, 0.01)]
        [InlineData(-Math.PI / 2.0, Math.PI / 2.0, 0.0005, 0.001)]
        [InlineData(0, Math.PI, 0.01, 0.1)]
        public void OneDimensionF1(double a, double b, double e, double e2)
        {
            // Рассматриваемая функция
            double F(double x)
                => Math.Abs(x) + Math.Sqrt(Math.Abs(Math.Sin(x)));

            // ожидаемый результат
            double fMin = 0;

            // полученное решение
            var result = MathStrategy.UniformSearchByBiryukov(
                   F,
                   a, b,    // [a;b]
                   L(e),    // L=L(e)=1/(4e)
                   e, e2);  // e, e*

            // Проверка прохождения теста
            (result.F - fMin).Should().BeLessOrEqualTo(e2);
        }

        [Theory]
        [InlineData(-15,    15, -4, -1, 3,  -1, -1.005, 0.5,    0.0001, 0.001)]
        [InlineData(-5,     5,  -2, 1,  3,  0,  -0.01,  0,      0.001,  0.01)]
        public void OneDimensionF3(double a, double b, double a1, double a2, double a3, double b1, double b2, double b3, double e, double e2)
        {
            // Рассматриваемая функция
            double F(double x)
             => Math.Min(
                 Math.Min(
                     Math.Sqrt(Math.Abs(x - a1)) + b1,
                     Math.Sqrt(Math.Abs(x - a2)) + b2),
                 Math.Sqrt(Math.Abs(x - a3)) + b3);

            // ожидаемый результат
            double fMin = 0;

            // полученное решение
            var result = MathStrategy.UniformSearchByBiryukov(
                   F,
                   a, b,    // [a;b]
                   L(e),    // L=L(e)=1/(4e)
                   e, e2);  // e, e*

            // Проверка прохождения теста
            (result.F - fMin).Should().BeLessOrEqualTo(e2);
        }

        [Theory]
        [InlineData(-Math.PI, 3 * Math.PI, 0.001, 0.01)]
        [InlineData(-Math.PI / 2.0, Math.PI / 2.0, 0.0005, 0.001)]
        [InlineData(0, Math.PI, 0.01, 0.1)]
        public void OneDimensionF2(double a, double b, double e, double e2)
        {
            // Рассматриваемая функция
            double F(double x)
                => Math.Sqrt(Math.Abs(x)) + Math.Abs(Math.Sin(x));

            // ожидаемый результат
            double fMin = 0;

            // полученное решение
            var result = MathStrategy.UniformSearchByBiryukov(
                   F,
                   a, b,    // [a;b]
                   L(e),    // L=L(e)=1/(4e)
                   e, e2);  // e, e*

            // Проверка прохождения теста
            (result.F - fMin).Should().BeLessOrEqualTo(e2);
        }
    }
}