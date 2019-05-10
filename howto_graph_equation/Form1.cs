namespace howto_graph_equation
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Linq;
    using System.Windows.Forms;

    public partial class Form1 : Form
    {
        private const float xmin = (float)(-2 * Math.PI);//0;

        private const float xmax = (float)(3 * Math.PI);//3;

        private const float ymin = (float)(-2 * Math.PI);//-9;

        private const float ymax = (float)(3 * Math.PI);//5;

        public Form1()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            var qwe = new List<(int a, float b)> { (1, 11), (3, 4.3f), (5,0.17f) };
            var asd = qwe.Replace((3, 4.3f), (33, 44)).ToList();

            this.Axis();
            this.Plot(F, Color.Red);
            //MessageBox.Show(this.Polygon(0, 3, 4, (float)Math.Exp(-7)).ToString());
        }

        /// <summary>
        /// Построение осей гарфика
        /// </summary>
        private void Axis()
        {
            int wid = picGraph.ClientSize.Width;
            int hgt = picGraph.ClientSize.Height;

            var bm = new Bitmap(wid, hgt);

            using (var gr = Graphics.FromImage(bm))
            {
                gr.SmoothingMode = SmoothingMode.AntiAlias;

                // Transform to map the graph bounds to the Bitmap.
                RectangleF rect = new RectangleF(xmin, ymin, xmax - xmin, ymax - ymin);
                PointF[] pts =
                {
                    new PointF(0, hgt),
                    new PointF(wid, hgt),
                    new PointF(0, 0),
                };
                gr.Transform = new Matrix(rect, pts);

                using (var pen = new Pen(Color.Black, 0))
                {
                    gr.DrawLine(pen, xmin, 0, xmax, 0);
                    gr.DrawLine(pen, 0, ymin, 0, ymax);

                    for (int x = (int)xmin; x <= xmax; x++)
                        gr.DrawLine(pen, x, -0.1f, x, 0.1f);

                    for (int y = (int)ymin; y <= ymax; y++)
                        gr.DrawLine(pen, -0.1f, y, 0.1f, y);
                }
            }

            // Display the result.
            picGraph.Image = bm;
        }

        /// <summary>
        /// Построение графика указанной функции
        /// </summary>
        private void Plot(Func<float, float> function, Color? plotColor = null)
        {
            // Make the Bitmap.
            int wid = picGraph.ClientSize.Width;
            int hgt = picGraph.ClientSize.Height;
            Bitmap bm = new Bitmap(picGraph.Image);

            using (Graphics gr = Graphics.FromImage(bm))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;

                // Transform to map the graph bounds to the Bitmap.
                RectangleF rect = new RectangleF(xmin, ymin, xmax - xmin, ymax - ymin);
                PointF[] pts =
                {
                    new PointF(0, hgt),
                    new PointF(wid, hgt),
                    new PointF(0, 0),
                };
                gr.Transform = new Matrix(rect, pts);

                // Draw the graph.
                using (var pen = new Pen(Color.Black, 0))
                {
                    // цвет кисти
                    pen.Color = plotColor ?? Color.Red;

                    // See how big 1 pixel is horizontally.
                    Matrix inverse = gr.Transform;
                    inverse.Invert();
                    PointF[] pixel_pts =
                    {
                        new PointF(0, 0),
                        new PointF(1, 0)
                    };

                    inverse.TransformPoints(pixel_pts);
                    float dx = pixel_pts[1].X - pixel_pts[0].X;
                    dx /= 2;

                    // Loop over x values to generate points.
                    List<PointF> points = new List<PointF>();
                    for (float x = xmin; x <= xmax; x += dx)
                    {
                        bool valid_point = false;
                        try
                        {
                            // Get the next point.
                            float y = function(x);

                            // If the slope is reasonable, this is a valid point.
                            if (points.Count == 0)
                                valid_point = true;
                            else
                            {
                                float dy = y - points[points.Count - 1].Y;
                                if (Math.Abs(dy / dx) < 1000) valid_point = true;
                            }
                            if (valid_point)
                                points.Add(new PointF(x, y));
                        }
                        catch
                        {
                        }

                        // If the new point is invalid, draw
                        // the points in the latest batch.
                        if (!valid_point)
                        {
                            if (points.Count > 1) gr.DrawLines(pen, points.ToArray());
                            points.Clear();
                        }

                    }

                    // Draw the last batch of points.
                    if (points.Count > 1)
                        gr.DrawLines(pen, points.ToArray());
                }
            }

            // Display the result.
            picGraph.Image = bm;
        }

        /// <summary>
        /// Функция, по которой строится график
        /// </summary>
        private float F(float x)
            => (float)(Math.Abs(x) + Math.Sqrt(Math.Abs(Math.Sin(x))));

        //   => Math.Sqrt(Math.Abs(x)) + Math.Abs(Math.Sin(x));

        //     => Math.Min(
        //         Math.Min(
        //             Math.Sqrt(Math.Abs(x - a1)) + b1,
        //             Math.Sqrt(Math.Abs(x - a2)) + b2),
        //         Math.Sqrt(Math.Abs(x - a3)) + b3);
        //=> (float)Math.Min(
        //    Math.Abs(Math.Pow(x, 2) - 1),
        //    Math.Pow(x - 2, 2) + 3);

        //private float Polygon(float a, float b, float L, float e, float? x0 = null)
        //{
        //    // координаты оси абсцисс, в которых расположены вершины "шапочек" строящейся ломаной
        //    // для удобства берём за основу ломаную с вершинами в концах рассматриваемого отрезка
        //    var u = new List<float> { a, b };

        //    // низины ломаной на текущей итерации построения
        //    var k = new List<(float x, float y)>();

        //    // координата оси абсцисс точки пересечения двух "шапочек"
        //    float ui(float left, float right)
        //        => (F(right) - F(left)) / (2 * L) + (left + right) / 2;

        //    // функция "шапочки" с вершиной в точке (xi, F(xi))
        //    float g(float x, float xi)
        //        => F(xi) - L * Math.Abs(x - xi);

        //    // полученная на текущей итерации ломаная
        //    float p(float x)
        //        => u.Select(t => g(x, t)).Max();

        //    // точка, в которой p минимальна, станет новой вершиной
        //    // изначально это либо заданная начальная точка, либо точка пересечения "шапочек" построенных на концах отрезка
        //    var temp = x0 ?? ui(a, b);
        //    (float x, float y) xk = (temp, p(temp));

        //    int position;
        //    do
        //    {
        //        // выбранная на прошлой итерация низина становится очередной вершиной для строящейся ломаной
        //        u.Add(xk.x);

        //        u.Sort();

        //        // удаляем переквалифицированную в вершины низину и добавляем низины-пересечения "шапочки" из неё с соседними
        //        k.Remove(xk);
        //        position = u.IndexOf(xk.x);
        //        for (int i = 0; i < 2; i++)
        //        {
        //            temp = ui(u[position - i], u[position - i + 1]);
        //            k.Add((temp, p(temp)));
        //        }

        //        // выбираем новую минимальную низину
        //        xk = k.OrderByDescending(intersection => intersection.y).Last();
        //    } while (Math.Abs(F(xk.x) - xk.y) > e);

        //    // отрисовывем "шапочки"
        //    for (int i = 0; i < u.Count; i++)
        //        this.Plot((arg) => g(arg, u[i]), Color.Blue);

        //    return xk.x;
        //}
    }

    public static class Ext
    {
        public static IEnumerable<T> Replace<T>(this IEnumerable<T> source, T oldValue, T newValue, IEqualityComparer<T> comparer = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            comparer = comparer ?? EqualityComparer<T>.Default;

            foreach (var item in source)
            {
                yield return
                    comparer.Equals(item, oldValue)
                        ? newValue
                        : item;
            }
        }
    }
}