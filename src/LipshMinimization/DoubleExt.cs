using System;

namespace LipshMinimization
{
    public static class DoubleExt
    {
        public static double ToRadians(this double angle)
            => (Math.PI / 180) * angle;
    }
}