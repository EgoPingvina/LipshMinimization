namespace LipshMinimizationMath
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public static class LipshMinimizationMath
    {
        /// <summary>
        /// Метод ломанных(Пиявского)
        /// </summary>
        public static double PolygonalMethod(Func<double, double> F, double a, double b, double L, double e, double? x0 = null)
        {
            // координаты оси абсцисс, в которых расположены вершины "шапочек" строящейся ломаной
            // для удобства берём за основу ломаную с вершинами в концах рассматриваемого отрезка
            var u = new List<double> { a, b };

            // низины ломаной на текущей итерации построения
            var k = new List<(double x, double y)>();

            // координата оси абсцисс точки пересечения двух "шапочек"
            double ui(double left, double right)
                => (F(right) - F(left)) / (2 * L) + (left + right) / 2;

            // функция "шапочки" с вершиной в точке (xi, F(xi))
            double g(double x, double xi)
                => F(xi) - L * Math.Abs(x - xi);

            // полученная на текущей итерации ломаная
            double p(double x)
                => u.Select(t => g(x, t)).Max();

            // точка, в которой p минимальна, станет новой вершиной
            // изначально это либо заданная начальная точка, либо точка пересечения "шапочек" построенных на концах отрезка
            var temp = x0 ?? ui(a, b);
            (double x, double y) xk = (temp, p(temp));

            int position;
            do
            {
                // выбранная на прошлой итерация низина становится очередной вершиной для строящейся ломаной
                u.Add(xk.x);

                u.Sort();

                // удаляем переквалифицированную в вершины низину и добавляем низины-пересечения "шапочки" из неё с соседними
                k.Remove(xk);
                position = u.IndexOf(xk.x);
                for (int i = 0; i < 2; i++)
                {
                    if (position == 0 && i == 1)
                        continue;

                    if (position == u.Count && i == 0)
                        continue;

                    temp = ui(u[position - i], u[position - i + 1]);
                    k.Add((temp, p(temp)));
                }

                // выбираем новую минимальную низину
                xk = k.OrderByDescending(intersection => intersection.y).Last();
            } while (Math.Abs(F(xk.x) - xk.y) > e);

            return xk.x;
        }

        #region Методы покрытий

        /// <summary>
        /// Метод перебора (метод равномерного поиска, перебор по сетке)
        /// </summary>
        public static double EnumerationMethod(Func<double, double> F, double a, double b, int n)  // из википедии
        {
            var x = new List<double>();
            double factor;

            if (n / 2 == 0)
            {
                factor = (b - a) / (n / 2.0 + 1);
                for (int i = 1; i <= n / 2; i++)
                    x.Add(a + 2.0 * i * factor);
            }
            else
            {
                factor = (b - a) / (n + a);
                for (int i = 1; i <= n; i++)
                    x.Add(a + i * factor);
            }


            return x.Select(xi => (xi, F(xi))).OrderByDescending(pair => pair.Item2).Last().xi;
        }

        /// <summary>
        /// Метод равномерного перебора(перебор на равномерной сетке)
        /// </summary>
        public static double UniformSearchMethod(Func<double, double> F, double a, double b, double L, double e)  // номер 2 в Васильеве(ПАССИВНЫЙ!!!)
        {
            double h = 2.0 * e / L,
                ui = a + h / 2;
            var u = new List<double> { ui };

            var exitParam = b - h / 2;
            while (!(ui < exitParam && exitParam <= (ui = ui + h)))
                u.Add(ui);

            u.Add(Math.Min(ui, b));

            return u.Select(xi => (xi, F(xi))).OrderByDescending(pair => pair.Item2).Last().xi;
        }

        /// <summary>
        /// Метод последовательного перебора(перебор на неравномерной сетке)
        /// </summary>
        public static double SerialEnumerationMethod(Func<double, double> F, double a, double b, double L, double e)   // номер 3 в Васильеве(Евтушенко, если не ошибаюсь)
        {
            double h = 2.0 * e / L,
                ui = a + h / 2;
            var u = new List<double> { ui };

            double Fi()
                => u.Select(x => F(x)).Min();

            var exitParam = b - h / 2.0;
            while (!(ui < exitParam && exitParam <= (ui = ui + h + (F(ui) - Fi()) / L)))
                u.Add(ui);
            
            u.Add(Math.Min(ui, b));

            return u.Select(xi => (xi, F(xi))).OrderByDescending(pair => pair.Item2).Last().xi;
        }

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
            var sw = new Stopwatch();
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
        public static (double L, double h, double x, double F, double n, long time) SimplifiedUniformSearchByBiryukov(Func<double, double> F, double a, double b, double L, double e, double e2)
        {
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

        public static double NoName(Func<double, double> F, double a, double b, double L, double e)    // номер 4 в Васильеве
        {
            int n = 0, k;
            double hi = b - a;

            // определяем минимальное целое n
            while (hi / Math.Pow(2, n + 1) > e / L)
                n++;

            // вводим на отрезке [a;b] точки c[i,k]
            var c = new List<List<double>>();
            for (k = 0; k <= n; k++)
            {
                c.Add(new List<double>());

                for (int i = 0; i <= Math.Pow(2, k); i++)
                    c[k].Add(a + i * hi / Math.Pow(2, k));
            }

            // Удаляет интервал с левой границей element и шагом step на уровнях level и выше(дробления этого интервала),
            // также убирает пустые уровни
            int RemoveAndCleaning(int level, double element, double step)
            {
                c[level].Remove(element);

                for (int i = level + 1; i < c.Count(); i++)
                    c[i].RemoveAll(arg => arg >= element && arg < element + step);

                int removing = 0;
                for (int i = 0; i <= level; i++)
                {
                    if (c[i].Count <= 1)
                    {
                        c.Remove(c[i]);
                        removing++;
                    }
                }

                return level - removing;
            }

            double ai = a,
                bi = b,
                ui = (ai + bi) / 2,
                fi = F(ui);

            if (n == 0)
                return fi;

            int j = 1;  // рассматриваемый уровень
            k = 0;      // левая граница рассматриваемого интервала
            double minX = ui, temp;
            while (c.SelectMany(arg => arg).Any())
            {
                ai = c[j][k];
                bi = c[j][k + 1];

                hi = (bi - ai) / 2;
                ui = (ai + bi) / 2;

                // F[k] = min{ F[k-1]; J(u[k]) }
                temp = F(ui);
                if (temp < fi)
                {
                    fi = temp;
                    minX = ui;
                }

                // если j=n, то отрезок исключается из рассмотрения и на следующем шаге
                // переходим к одному из оставшихся отрезков меньшего уровня
                if (j == c.Count-1)
                {
                    j = RemoveAndCleaning(j, c[j][k], hi);

                    j = 0;
                    k = 0;
                    
                    continue;
                }

                // если выполняется условие, иссключаем интервал из рассмотрения, а в качестве следующего
                // берём один из оставшихся отрезков самого меньшего уровня
                if (fi <= F(ui) - L * hi + e)
                {
                    j = RemoveAndCleaning(j, c[j][k], hi);

                    j = 0;
                    k = 0;
                    
                    continue;
                }

                // если условие не выполняется берём одну из половин этого интервала
                j++;
                k = 0;
            }

            return minX;
        }

        #endregion

        /// <summary>
        /// Метод поиска глобального минимума(Стронгина)
        /// </summary>
        public static double GlobalMinimumSearch(Func<double, double> F, double a, double b, double e)
        {
            double v = 1.0, mk = 2.0;

            List<double> u = new List<double> { a, b },
                L = new List<double>();

            var R = new List<(int, double)>();

            int s = 0;
            double d, Lmax;
            do
            {
                // если вновь сюда вошли-сортируем, чтобы последняя добавленная точка нашла своё место;
                // для первого же прохода ничего не изменится
                u.Sort();

                L.Clear();
                for (int j = 1; j < u.Count; j++)
                    L.Add(Math.Abs(F(u[j]) - F(u[j - 1])) / (u[j] - u[j - 1]));
                Lmax = L.Max();

                d = Lmax > 0 ? mk * Lmax : v;

                R.Clear();
                for (int j = 1; j < u.Count; j++)
                    R.Add((
                        j,
                        d * (u[j] - u[j - 1])
                            + Math.Pow(F(u[j]) - F(u[j - 1]), 2) / (d * (u[j] - u[j - 1]))
                            - 2 * (F(u[j]) + F(u[j - 1]))));

                s = R.OrderByDescending(r => r.Item2).First().Item1;

                u.Add((u[s] + u[s - 1]) / 2.0 - (F(u[s]) - F(u[s - 1])) / (2.0 * d));
            }
            while (u[s] - u[s - 1] >= e);

            // если условие выхода выполнилось => сортирвока н ебыла проведена => u(k+1) находится в конце коллекции
            return u.Last();
        }
    }
}