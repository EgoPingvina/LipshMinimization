﻿/* Методы поиска глобального минимума функции одной переменной
 * (методы ломаныхи, Стронгина и несколько вариаций методов покрытий)
 */

namespace LipshMinimization
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    public static class Program
    {
        public static void Main(string[] args)
        {
            double //a = Input<double>("a="),
               // b = Input<double>("b="),
               // L = Input<double>("L="),
                e = Math.Pow(10, -4);

            double a1 = -2, a2 = 1, a3 = 3, b1 = 0, b2 = 0, b3 = 0;
            Console.WriteLine(
                EvtushenkoMethodByArytunova(
                    (x) => Math.Min(Math.Min(Math.Sqrt(Math.Abs(x - a1)) + b1, Math.Sqrt(Math.Abs(x - a2)) + b2), Math.Sqrt(Math.Abs(x - a3)) + b3),
                    -5, 5,              // [a;b]
                    1.0 / (4.0 * e),    // L=L(e)=1/(4e)
                    e, 0.001));          // e, e*


            Console.ReadKey();

            //Console.WriteLine($"L={L = LipschitzConstantEstimate(a, b)}\n");

            //Console.WriteLine($"Метод ломанных(Пиявского): х={PolygonalMethod(a, b, 4, e)}");

            ////Console.WriteLine($"Метод перебора на неравномерной сетке(Евтушенко): min ={CoatingMethod(a, b, L, e)}");

            //Console.WriteLine($"Метод Поиска глобального минимума(Стронгина): х={GlobalMinimumSearch(a, b, e)}");

            //Console.WriteLine("\nЧто-то ещё :D :");

            //int n = Input<int>("n=");
            //Console.WriteLine($"Метод перебора (метод равномерного поиска, перебор по сетке): х={EnumerationMethod(a, b, n)}");
            //Console.WriteLine($"Метод перебора (метод равномерного поиска, перебор по сетке): х={UniformSearchMethod(a, b, L, e)}");

            //Console.WriteLine($"Метод последовательного перебора: х={SerialEnumerationMethod(a, b, L, e)}");

            //Console.WriteLine($"Ещё один метод покрытий(4 из методов покрытий в Ваильеве): х={NoName(a, b, L, e)}");

            //Console.ReadKey();
        }

        /// <summary>
        /// Запрос ввода числа с проверкой допустимости значения
        /// </summary>
        /// <param name="message">Наименование запрашиваемой величины</param>
        /// <returns>Считанное значение</returns>
        private static T Input<T>(string message)
        {
            bool isOk = true;
            var converter = TypeDescriptor.GetConverter(typeof(T));
            string input;

            do
            {
                Console.Write(message);
                input = Console.ReadLine();

                if (converter != null && converter.IsValid(input.Replace(',', '.')))
                    break;

                isOk = false;
                Console.WriteLine("Некорректный ввод.");
            }
            while (!isOk);

            return (T)converter.ConvertFromString(input.Replace('.', ','));
        }

        /// <summary>
        /// Рассматриваемая функция
        /// </summary>
        private static double F(double x)  // min{|x^2-1|, (x-2)^2+3}
            => Math.Min(
                Math.Abs(Math.Pow(x, 2) - 1),
                Math.Pow(x - 2, 2) + 3);

        /// <summary>
        /// Производная рассматриваемой функции
        /// </summary>
        private static double Fp(double x)
            => Math.Abs(Math.Pow(x, 2) - 1) < Math.Pow(x - 2, 2) + 3
                ? 2.0 * x * (Math.Pow(x, 2) - 1) / Math.Abs(Math.Pow(x, 2) - 1)         // производная первой, если гарфик функции ниже
                : Math.Abs(Math.Pow(x, 2) - 1) > Math.Pow(x - 2, 2) + 3
                    ? 2 * x - 4                                                         // производная второй, если она ниже первой
                    : Math.Max(                                                         // в точке пересечения выбирается наибольшая из производных
                        2.0 * x * (Math.Pow(x, 2) - 1) / Math.Abs(Math.Pow(x, 2) - 1),
                        2 * x - 4);

        /// <summary>
        /// Оценка константы Липшица(делю отрезок на 100 частей и выбираю в качестве L максимальную по модулю производнюу функции)
        /// </summary>
        private static double LipschitzConstantEstimate(double a, double b)
        {
            var n = 100;
            var h = (b - a) / n;
            double L = 0;

            for (int i = 0; i <= n; i++)
                L = Math.Max(L, Math.Abs(Fp(a + i * h)));

            return L;
        }

        /// <summary>
        /// Метод ломанных(Пиявского)
        /// </summary>
        private static double PolygonalMethod(double a, double b, double L, double e, double? x0 = null)
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
        /// Метод перебора на неравномерной сетке(Евтушенко)
        /// </summary>
        private static void CoatingMethod(double a, double b, double L, double e)     // ToDo не реализован!
        {
            //int z = -1, // поиск минимума
            //    n = 2;  // двумерное пространство

            //double p = e / L * Math.Sqrt(n);

            //var x = new List<double>[2];

            //double Fk(int level)
            //    => z * x[level].Select(xi => z * F(xi)).Max();

            //double Rk(int level, double xk)
            //    => (e + z * (Fk(level) - F(xk))) / L;

            //double nea(double ai)
            //    => ai + p;

            //x[0]=new List<double> {  }
        }

        /// <summary>
        /// Метод перебора (метод равномерного поиска, перебор по сетке)
        /// </summary>
        private static double EnumerationMethod(double a, double b, int n)  // из википедии
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
        /// Метод равномерного перебора
        /// </summary>
        private static double UniformSearchMethod(double a, double b, double L, double e)  // номер 2 в Васильеве(ПАССИВНЫЙ!!!)
        {
            double h = 2.0 * e / L,
                ui = a + h / 2;
            var u = new List<double> { ui };

            var exitParam = b - h / 2;
            while (!(ui < exitParam && exitParam <= (ui = ui + h)))
                u.Add(ui);

            u.Add(new double[2] { ui, b }.Min());

            return u.Select(xi => (xi, F(xi))).OrderByDescending(pair => pair.Item2).Last().xi;
        }

        /// <summary>
        /// Метод последовательного перебора
        /// </summary>
        private static double SerialEnumerationMethod(double a, double b, double L, double e)   // номер 3 в Васильеве
        {
            double h = 2.0 * e / L,
                ui= a + h / 2;
            var u = new List<double> { ui };

            double Fi()
                => u.Select(x => F(x)).Min();

            var exitParam = b - h / 2.0;
            while (!(ui < exitParam && exitParam <= (ui = ui + h + (F(ui) - Fi()) / L)))
                u.Add(ui);
            
            u.Add(new double[2] { ui, b }.Min());

            return u.Select(xi => (xi, F(xi))).OrderByDescending(pair => pair.Item2).Last().xi;
        }

        /// <summary>
        /// Модификация метода Евтушенко поиска глобального минимума для случая непрерывной на отрезке функции
        /// </summary>
        private static (double x, double F, double n) EvtushenkoMethodByArytunova(Func<double, double> F, double a, double b, double L, double e, double e2)                                   // какая-то странная вариация Арутюновой
        {
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

            // xi_1 уже хранит xn. 
            return (xMin, Math.Min(Fmin, F(xi_1)), i);
            
        }

        private static double NoName(double a, double b, double L, double e)    // номер 4 в Васильеве
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
        private static double GlobalMinimumSearch(double a, double b, double e)
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