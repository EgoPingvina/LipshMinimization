using System;
using System.Diagnostics;

namespace LipshMinimization.ELipschitzMath
{
    public static class MathStrategy
    {
        /// <summary>
        /// Модификация метода Евтушенко поиска глобального минимума для случая непрерывной на отрезке функции
        /// </summary>
        /// <param name="F">Исследуемая на глобальный минимум функция</param>
        /// <param name="a">Левая граница отрезка</param>
        /// <param name="b">Правая граница отрезка</param>
        /// <param name="L">Константа Липшица</param>
        /// <param name="e">Параметр, выбираемый из условия e-Липшицевости</param>
        /// <param name="e2">Погрешность, с которой отыскивается приближённое значение минимума функции</param>
        /// <returns>
        /// x-значение на оси Ox, в котором достигается глобальный минимум;
        /// F-глобальный минимум переданной функции на рассмтариваемом отрезке;
        /// n-количество пробных точек(итераций);
        /// time-время, затраченное на выполнение поиска.</returns>
        public static (double x, double F, double n, long time) EvtushenkoMethodByArytunova(Func<double, double> F, double a, double b, double L, double e, double e2)    // модифицированный Арутюновой метод Евтушенко
        {
            // Таймер для приблизительного измерения производительности алгоритма
            var sw      = new Stopwatch();
            sw.Start();

            double h    = 2.0 * (e2 - e) / L
                , xi
                , Fmin
                , xMin;

            // получение xi+1
            double NextX(double x)
                => x + h + (F(x) - Fmin) / L;

            double exitParam = b - h / 2.0;

            double xi_1 = xMin = a + h / 2.0, tmp = 0;
            Fmin        = F(xi_1);
            int i       = 1;
            do
            {
                xi      = xi_1;

                tmp     = Math.Min(Fmin, F(xi));
                if(tmp!=Fmin)
                {
                    Fmin = tmp;
                    xMin = xi;
                }

                xi_1    = NextX(xi);
                i++;
            } while (!(xi < exitParam && exitParam <= xi_1));
            
            xi = Math.Min(xi_1, b);
            tmp = Math.Min(Fmin, F(xi));
            if (tmp != Fmin)
            {
                Fmin = tmp;
                xMin = xi;
            }

            sw.Stop();

            // xi_1 уже хранит xn. 
            return (xMin, Math.Min(Fmin, F(xi_1)), i, sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Модификация метода равномерного перебора поиска глобального минимума для эпсилон-липшецевых функций
        /// </summary>
        /// <param name="F">Исследуемая на глобальный минимум функция</param>
        /// <param name="a">Левая граница отрезка</param>
        /// <param name="b">Правая граница отрезка</param>
        /// <param name="L">Константа Липшица</param>
        /// <param name="e">Параметр, выбираемый из условия e-Липшицевости</param>
        /// <param name="e2">Погрешность, с которой отыскивается приближённое значение минимума функции</param>
        /// <returns>
        /// x-значение на оси Ox, в котором достигается глобальный минимум;
        /// F-глобальный минимум переданной функции на рассмтариваемом отрезке;
        /// n-количество пробных точек(итераций);
        /// time-время, затраченное на выполнение поиска.</returns>
        public static (double L, double h, double x, double F, double n, long time) UniformSearchByBiryukov(Func<double, double> F, double a, double b, double L, double e, double e2)
        {
            // Таймер для приблизительного измерения производительности алгоритма
            var sw      = new Stopwatch();
            sw.Start();

            double h    = (e2 - e) / L  // шаг
                , xi    = a             // координата на оси Ox, значение функции в которой рассмтаривается на текущей итерации
                , xMin  = xi            // координата оси Ox, на которой достигается лучшее приближение к глобальному минимуму функции на текущей итерации
                , fMin  = F(xMin)       // лучшее приближение к глобальному минимуму функции на текущей итерации
                , tmp;                  // временное хранилище для подмены лучшего приближения к глобальному минимуму
            int i       = 0;

            while ((xi += h) < b)
            {
                // если значение функции в текущей точке меньше последнего сохранённого - заменяем его
                if ((tmp = F(xi)) < fMin)
                {
                    xMin = xi;
                    fMin = tmp;
                }
                i++;
            }
            
            sw.Stop();

            return (L, h, xMin, fMin, i, sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Модификация метода равномерного перебора поиска глобального минимума для эпсилон-липшецевых функций двух переменных
        /// </summary>
        /// <param name="F">Исследуемая на глобальный минимум функция</param>
        /// <param name="a">Левая граница бруса</param>
        /// <param name="b">Правая граница бруса</param>
        /// <param name="d">Нижняя граница бруса</param>
        /// <param name="c">Верхняя граница бруса</param>
        /// <param name="L">Константа Липшица</param>
        /// <param name="e">Параметр, выбираемый из условия e-Липшицевости</param>
        /// <param name="e2">Погрешность, с которой отыскивается приближённое значение минимума функции</param>
        /// <returns>
        /// x-значение на оси Ox, в котором достигается глобальный минимум;
        /// y-значение на оси Oy, в котором достигается глобальный минимум;
        /// F-глобальный минимум переданной функции на рассмтариваемом отрезке;
        /// n-количество пробных точек по горизонтали;
        /// n-количество пробных точек по вертикали;
        /// time-время, затраченное на выполнение поиска.</returns>
        public static (double L, double hx, double hy, double x, double y, double F, double n, double m, long time) UniformSearchByBiryukov(Func<double, double, double> F, double a, double b, double d, double c, double L, double e, double e2)
        {
            // Таймер для приблизительного измерения производительности алгоритма
            var sw      = new Stopwatch();
            sw.Start();

            double m    = Math.Ceiling((c - d) * L / (e2 - e)), // число минимально необходимых пробных точек по оси Oy
                n       = Math.Ceiling((b - a) * L / (e2 - e)); // число минимально необходимых пробных точек по оси Ox

            double hx   = (e2 - e) / L  // шаг по оси Ox
                , hy    = (e2 - e) / L  // шаг по оси Oy
                , xi    = a             // координата на оси Ox, значение функции в которой рассмтаривается на текущей итерации
                , yi    = d             // координата на оси Oy, значение функции в которой рассмтаривается на текущей итерации
                , xMin  = xi            // координата оси Ox, на которой достигается лучшее приближение к глобальному минимуму функции на текущей итерации
                , yMin  = yi            // координата оси Oy, на которой достигается лучшее приближение к глобальному минимуму функции на текущей итерации
                , fMin  = F(xMin, yMin) // лучшее приближение к глобальному минимуму функции на текущей итерации
                , tmp;                  // временное хранилище для подмены лучшего приближения к глобальному минимуму

            do
            {
                xi = a;

                while ((xi += hx) < b)
                {
                    // если значение функции в текущей точке меньше последнего сохранённого - заменяем его
                    if ((tmp = F(xi, yi)) < fMin)
                    {
                        xMin = xi;
                        yMin = yi;
                        fMin = tmp;
                    }
                }
            }
            while ((yi += hy) < c);

            sw.Stop();

            return (L, hx, hy, xMin, yMin, fMin, n, m, sw.ElapsedMilliseconds);
        }
    }
}