using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using CGG;

namespace Task1
{
    //    I. Нарисовать f(x) на интервале [a; b].
    //       Отрисовывать оси только при необходимости.
    //       Масштабировать график.

    public class Program
    {
        // Нарисовать f на [xFrom; xTo] (с ручками и размером окна по умолчанию)
        private static void DrawFunction(Core.RealFunction f, double xFrom, double xTo)
        {
            DrawFunction(f, xFrom, xTo, Core.DefaultPen, Core.DefaultAxisPen, Core.DefaultWindowSize);
        }

        // Нарисовать f на [xFrom; xTo] ручкой pen, оси координат ручкой axisPen (размером окна по умолчанию)
        private static void DrawFunction(Core.RealFunction f, double xFrom, double xTo, Pen pen, Pen axisPen)
        {
            DrawFunction(f, xFrom, xTo, pen, axisPen, Core.DefaultWindowSize);
        }

        // Нарисовать f на [xFrom; xTo] в окне размером windowSize (с ручками по умолчанию) 
        private static void DrawFunction(Core.RealFunction f, double xFrom, double xTo, Point windowSize)
        {
            DrawFunction(f, xFrom, xTo, Core.DefaultPen, Core.DefaultAxisPen, windowSize);
        }

        // Нарисовать f на [xFrom; xTo] ручкой pen, оси координат ручкой axisPen, размером окна windowSize
        private static void DrawFunction(Core.RealFunction f, double xFrom, double xTo, Pen pen, Pen axisPen, Point windowSize)
        {
            if (windowSize.X * windowSize.Y == 0 || xFrom > xTo)
            {
                throw new ArgumentException("Неверные параметры! xFrom не должно быть больше xTo," +
                                            "измерения windowSize не должны быть нулевыми."
                );
            }

            var image = new Bitmap(windowSize.X, windowSize.Y);
            Graphics g = Graphics.FromImage(image);
            g.FillRectangle(Brushes.BlanchedAlmond, 0, 0, windowSize.X, windowSize.Y);

            Tuple<double, double> minMax = Core.GetMinMaxValues(f, xFrom, xTo, windowSize.Y);
            double minY = minMax.Item1,
                   maxY = minMax.Item2;

            Core.DrawAxis(g, axisPen, windowSize, xFrom, xTo, minY, maxY);

            // Если нужно нарисовать 1 точку по x
            if (Math.Abs(xFrom - xTo) < Core.Eps)
            {
                // Если значение в ней определено
                var y = Core.SafeValue(f, xFrom);
                if (!double.IsNaN(y) && !double.IsInfinity(y))
                {
                    var point = new Point(windowSize.X / 2, windowSize.Y / 2);
                    g.DrawLine(pen, point, point);
                }
                Core.ShowImageInWindow(image);
                return;
            }

            // Определим первую точку по x, которую необходимо отобразить
            int screenX = 0, screenY;
            Point? oldPoint = null, nextPoint = null;
            bool maxYEqualsMinY = Math.Abs(maxY - minY) < Core.Eps;

            for (; screenX < windowSize.X; ++screenX)
            {
                double x = xFrom + screenX * (xTo - xFrom) / windowSize.X,
                       y = Core.SafeValue(f, x);
                if (double.IsNaN(y) || double.IsInfinity(y))
                    continue;
                // Если функция вида y = C (minY=maxY), рисуем на середине экрана; иначе обычный перевод в экранные
                screenY = maxYEqualsMinY ? windowSize.Y / 2
                                         : (int)((maxY - y) * windowSize.Y / (maxY - minY));
                oldPoint = new Point(screenX, screenY);
                break;
            }

            // Нашли первую определенную точку
            if (oldPoint != null)
            {
                for (; screenX < windowSize.X; ++screenX)
                {
                    double x = xFrom + screenX * (xTo - xFrom) / windowSize.X,
                           y = Core.SafeValue(f, x);
                    if (double.IsNaN(y) || double.IsInfinity(y))
                    {
                        // Новая точка не определена, нужно нарисовать точку в текущем положении
                        if (oldPoint != null)
                            g.DrawLine(pen, oldPoint.Value, oldPoint.Value);
                        oldPoint = null;
                        continue;
                    }
                    screenY = maxYEqualsMinY ? windowSize.Y / 2
                                             : (int)((maxY - y) * windowSize.Y / (maxY - minY));
                    nextPoint = new Point(screenX, screenY);

                    // Если есть откуда рисовать, проводим прямую до новой точки 
                    if (oldPoint != null)
                        g.DrawLine(pen, oldPoint.Value, nextPoint.Value);
                    oldPoint = nextPoint;
                }
            }
            //image.Save("img.png", ImageFormat.Png);
            Core.ShowImageInWindow(image);
        }


        private static readonly Core.RealFunction[] Functions =
        {
            x => 1/x,
            x => 1,
            x => 0,
            x => (1/x)*Math.Sin(1/x),
            x => x*Math.Cos(x*x),
            x => Math.Cos(1/x),
            x => Math.Sqrt(x),
            x => x*x*x,
            x => x*x*x*x*x,
        };

        public static void Main(string[] args)
        {
            foreach (var function in Functions)
                DrawFunction(function, -1, 10);
        }
    }
}
