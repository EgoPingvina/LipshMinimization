using System;

namespace LipshMinimizationMath
{
    /// <summary>
    /// Методы расширений для типа <see cref="double"/>
    /// </summary>
    public static class DoubleExt
    {
        /// <summary>
        /// Преобразование угла в радианы
        /// </summary>
        /// <param name="angle">Исходное значение угла в градусах</param>
        /// <returns>Значение принятого угла в радианах</returns>
        public static double ToRadians(this double angle)
            => (Math.PI / 180) * angle;
    }
}