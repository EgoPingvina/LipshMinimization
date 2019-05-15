using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using LipshMinimization.ELipschitzMath;

namespace LipshMinimization
{
    public partial class Form1 : Form
    {
        private const float xmin = (float)(-2 * Math.PI);

        private const float xmax = (float)(3 * Math.PI);

        private const float ymin = -1;

        private const float ymax = 5;
        
        /// <summary>
        /// Функция, по которой строится график
        /// </summary>
        private double F(double x)
            => Math.Sqrt(Math.Abs(x)) + Math.Abs(Math.Sin(x));

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public Form1()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            this.Axis();
            this.Plot(F, Color.Red);

            double e = 0.001
                , e2 = 0.01
                , L = 1.0 / (4.0 * e) + 1;

            var result = MathStrategy.UniformSearchByBiryukov(F, xmin, xmax, L, e, e2);
            MessageBox.Show($"e={e}, e2={e2}\nМетода равномерного перебора поиска глобального минимума для эпсилон-липшицевых функций:\nh={result.h.ToString("F6")}, x={result.x.ToString("F6")}, F={result.F.ToString("F6")}, n={result.n}, t={result.time}\n{result}");
        }

        /// <summary>
        /// Построение осей гарфика
        /// </summary>
        private void Axis()
        {
            var width   = this.picGraph.ClientSize.Width;
            var height  = this.picGraph.ClientSize.Height;
            var bitmap  = new Bitmap(width, height);

            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.HighQuality;

                // Transform to map the graph bounds to the Bitmap.
                var rect        = new RectangleF(xmin, ymin, xmax - xmin, ymax - ymin);
                PointF[] points =
                    {
                        new PointF(0,       height),
                        new PointF(width,   height),
                        new PointF(0,       0),
                    };
                graphics.Transform = new Matrix(rect, points);

                using (var pen = new Pen(Color.Black, 0))
                {
                    graphics.DrawLine(pen, xmin, 0, xmax, 0);
                    graphics.DrawLine(pen, 0, ymin, 0, ymax);

                    for (int x = (int)xmin; x <= xmax; x++)
                        graphics.DrawLine(pen, x, -0.1f, x, 0.1f);

                    for (int y = (int)ymin; y <= ymax; y++)
                        graphics.DrawLine(pen, -0.1f, y, 0.1f, y);
                }
            }

            // Display the result.
            this.picGraph.Image = bitmap;
        }

        /// <summary>
        /// Построение графика указанной функции
        /// </summary>
        private void Plot(Func<double, double> function, Color? plotColor = null)
        {
            // Make the Bitmap.
            var width   = this.picGraph.ClientSize.Width;
            var height  = this.picGraph.ClientSize.Height;
            var bitmap  = new Bitmap(this.picGraph.Image);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.HighQuality;

                // Transform to map the graph bounds to the Bitmap.
                var rect = new RectangleF(xmin, ymin, xmax - xmin, ymax - ymin);
                PointF[] points =
                    {
                        new PointF(0,       height),
                        new PointF(width,   height),
                        new PointF(0,       0),
                    };
                graphics.Transform = new Matrix(rect, points);

                // Draw the graph.
                using (var pen = new Pen(Color.Black, 0))
                {
                    // Set pen color.
                    pen.Color   = plotColor ?? Color.Red;

                    // See how big 1 pixel is horizontally.
                    var inverse = graphics.Transform;
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
                    var graphPoints = new List<PointF>();
                    for (float x = xmin; x <= xmax; x += dx)
                    {
                        bool valid_point = false;
                        try
                        {
                            // Get the next point.
                            float y = (float)function(x);

                            // If the slope is reasonable, this is a valid point.
                            if (graphPoints.Count == 0)
                                valid_point = true;
                            else
                            {
                                float dy = y - graphPoints[graphPoints.Count - 1].Y;
                                if (Math.Abs(dy / dx) < 1000)
                                    valid_point = true;
                            }

                            if (valid_point)
                                graphPoints.Add(new PointF(x, y));
                        }
                        catch
                        {
                        }

                        // If the new point is invalid, draw
                        // the points in the latest batch.
                        if (!valid_point)
                        {
                            if (graphPoints.Count > 1)
                                graphics.DrawLines(pen, graphPoints.ToArray());
                            graphPoints.Clear();
                        }

                    }

                    // Draw the last batch of points.
                    if (graphPoints.Count > 1)
                        graphics.DrawLines(pen, graphPoints.ToArray());
                }
            }

            // Display the result.
            this.picGraph.Image = bitmap;
        }
    }
}