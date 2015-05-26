using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CGG
{
    public class Core
    {
        // Константы
        public const double Eps = 1e-9;
        public delegate double RealFunction(double x);
        public delegate double Real3DFunction(double x, double y);

        public static readonly Pen DefaultPen = new Pen(Color.DarkRed, 1);
        public static readonly Pen DefaultAxisPen = new Pen(Color.DarkBlue, 3);
        public static readonly Brush DefaultBackgroundBrush = Brushes.White;
        public static readonly Pen DefaultPen3DTop = new Pen(Color.DeepPink);
        public static readonly Pen DefaultPen3DBottom = new Pen(Color.Blue);
        public static readonly Point DefaultWindowSize = new Point(600, 600);

        // Утилиты

        // Отрисовка самого простого окна
        public static void ShowImageInWindow(Image image)
        {
            var form = new Form { ClientSize = new Size(image.Width, image.Height) };
            form.Controls.Add(new PictureBox
            {
                Image = image,
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.CenterImage
            });
            form.ShowDialog();
        }

        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            var temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        public static double SafeValue(RealFunction f, double x)
        {
            try
            {
                return f(x);
            }
            catch (OverflowException)
            {
                return double.NaN;
            }
        }

        public static IEnumerable<double> Range(double from, double to, double accuracy)
        {
            if (from < to)
            {
                for (var i = from; i <= to; i += accuracy)
                    yield return i;
            }
            else
            {
                for (var i = from; i >= to; i -= accuracy)
                    yield return i;
            }
        }

        // Отрисовка осей координат для 1,2 задач
        public static void DrawAxisAtCenter(Bitmap image, Pen axisPen, Point windowSize)
        {
            DrawAxis(image, axisPen, windowSize, windowSize.X/2, windowSize.Y/2);
        }
        public static void DrawAxisForNewCenter(Bitmap image, Pen axisPen, Point windowSize, double newCenterX, double newCenterY)
        {
            DrawAxis(image, axisPen, windowSize, (int)(windowSize.X/2f - newCenterX), (int)(windowSize.Y/2f + newCenterY));
        }
        // Нарисуем необходимые оси по точке на экране
        public static void DrawAxis(Bitmap image, Pen axisPen, Point windowSize, int? screenX, int? screenY)
        {
            if (!(0 <= screenX && screenX <= windowSize.X))
                screenX = null;
            if (!(0 <= screenY && screenY <= windowSize.Y))
                screenY = null;

            var arrowSize = Math.Min(10, Math.Min(windowSize.X/20, windowSize.Y/20));
            if (screenX != null)
            {
                DrawLineWithBresenham(new Point(screenX.Value, 0), new Point(screenX.Value, windowSize.Y), image, axisPen);
                var verticalArrowSize = arrowSize;
                if (screenY != null)
                    verticalArrowSize = Math.Min(arrowSize, screenY.Value);
                DrawLineWithBresenham(new Point(screenX.Value, 0), new Point(screenX.Value+verticalArrowSize/2, verticalArrowSize), image, axisPen);
                DrawLineWithBresenham(new Point(screenX.Value, 0), new Point(screenX.Value-verticalArrowSize/2, verticalArrowSize), image, axisPen);
            }

            if (screenY != null)
            {
                DrawLineWithBresenham(new Point(windowSize.X, screenY.Value), new Point(0, screenY.Value), image, axisPen);
                var horizontalArrowSize = arrowSize;
                if (screenX != null)
                    horizontalArrowSize = Math.Min(arrowSize, windowSize.X - screenX.Value);
                DrawLineWithBresenham(new Point(windowSize.X, screenY.Value), new Point(windowSize.X - horizontalArrowSize,
                    screenY.Value + horizontalArrowSize / 2), image, axisPen);
                DrawLineWithBresenham(new Point(windowSize.X, screenY.Value), new Point(windowSize.X - horizontalArrowSize,
                    screenY.Value - horizontalArrowSize / 2), image, axisPen);
            }
        }
        // Нарисуем необходимые оси в соответствии с диапазоном значений функции
        public static void DrawAxis(Bitmap image, Pen axisPen, Point windowSize,
                                     double minX, double maxX, double minY, double maxY)
        {
            int? screenX = null, screenY = null;

            // Необходимость оси ординат
            if (Math.Abs(minX + maxX) < 2 * Eps)
                screenX = windowSize.Y / 2;
            else if (minX < Eps && maxX > -Eps && Math.Abs(maxX - minX) > Eps)
                screenX = (int)(-minX * windowSize.X / (maxX - minX));
            // Необходимость оси абсцисс
            if (Math.Abs(minY + maxY) < 2 * Eps)
                screenY = windowSize.X / 2;
            else if (minY < Eps && maxY > -Eps)
                screenY = (int)(maxY * windowSize.Y / (maxY - minY));

            var arrowSize = (new List<int> { 10, windowSize.X / 20, windowSize.Y / 20 }).Min();
            if (screenX != null)
            {
                DrawLineWithBresenham(new Point(screenX.Value, 0), new Point(screenX.Value, windowSize.Y), image, axisPen);
                var verticalArrowSize = arrowSize;
                if (screenY != null)
                    verticalArrowSize = Math.Min(arrowSize, screenY.Value);
                DrawLineWithBresenham(new Point(screenX.Value, 0),
                    new Point(screenX.Value + verticalArrowSize / 2, verticalArrowSize), image, axisPen);
                DrawLineWithBresenham(new Point(screenX.Value, 0),
                    new Point(Math.Max(screenX.Value - verticalArrowSize / 2, 0), verticalArrowSize), image, axisPen);
            }

            if (screenY != null)
            {
                DrawLineWithBresenham(new Point(windowSize.X, screenY.Value), new Point(0, screenY.Value), image, axisPen);
                var horizontalArrowSize = arrowSize;
                if (screenX != null)
                    horizontalArrowSize = Math.Min(arrowSize, windowSize.X - screenX.Value);
                DrawLineWithBresenham(new Point(windowSize.X, screenY.Value), new Point(windowSize.X - horizontalArrowSize,
                    screenY.Value + horizontalArrowSize / 2), image, axisPen);
                DrawLineWithBresenham(new Point(windowSize.X, screenY.Value), new Point(windowSize.X - horizontalArrowSize,
                    screenY.Value - horizontalArrowSize / 2), image, axisPen);
            }
        }
        public static void DrawAxisRotatedForNewCenter(Bitmap image, Pen axisPen, Point windowSize, double newCenterX, double newCenterY)
        {
            Point pFrom1 = new Point((int)(windowSize.X / 2f - newCenterX), (int)(windowSize.Y / 2f + newCenterY)),
                pTo1 = pFrom1;
            int dist11 = Math.Min(pFrom1.X, pFrom1.Y), dist12 = Math.Min(windowSize.X - pTo1.X, windowSize.Y - pTo1.Y);
            pFrom1.X -= dist11 - 1;
            pFrom1.Y -= dist11 - 1;
            pTo1.X += dist12 - 1;
            pTo1.Y += dist12 - 1;
            DrawLineWithBresenham(pFrom1, pTo1, image, axisPen);
            
            Point pFrom2 = new Point((int)(windowSize.X / 2f - newCenterX), (int)(windowSize.Y / 2f + newCenterY)),
                pTo2 = pFrom2;
            int dist21 = Math.Min(pFrom2.X, windowSize.Y - pFrom2.Y), dist22 = Math.Min(windowSize.X - pTo2.X, pTo2.Y);
            pFrom2.X -= dist21 - 1;
            pFrom2.Y += dist21 - 1;
            pTo2.X += dist22 - 1;
            pTo2.Y -= dist22 - 1;
            DrawLineWithBresenham(pFrom2, pTo2, image, axisPen);

            var arrowSize = (new List<int> { 10, windowSize.X / 20, windowSize.Y / 20 }).Min();
            DrawLineWithBresenham(new Point(pTo1.X, pTo1.Y),
                new Point(pTo1.X, pTo1.Y - arrowSize), image, axisPen);
            DrawLineWithBresenham(new Point(pTo1.X, pTo1.Y),
                new Point(pTo1.X - arrowSize, pTo1.Y), image, axisPen);

            DrawLineWithBresenham(new Point(pTo2.X, pTo2.Y),
                new Point(pTo2.X - arrowSize, pTo2.Y), image, axisPen);
            DrawLineWithBresenham(new Point(pTo2.X, pTo2.Y),
                new Point(pTo2.X, pTo2.Y + arrowSize), image, axisPen);
        }


// Task1
        // Вычислим минимальное и максимальное значения функции на заданном интервале [xFrom; xTo]
        public static Tuple<double, double> GetMinMaxValues(RealFunction f, double xFrom, double xTo, int windowWidth)
        {
            double maxY = double.MinValue,
                   minY = double.MaxValue;
            double x, y;
            for (int screenX = 0; screenX < windowWidth; ++screenX)
            {
                x = xFrom + screenX * (xTo - xFrom) / windowWidth;
                y = SafeValue(f, x);
                if (double.IsNaN(y) || double.IsInfinity(y))
                    continue;
                minY = Math.Min(y, minY);
                maxY = Math.Max(y, maxY);
            }
            return Tuple.Create(minY, maxY);
        }

        // Нарисовать f на [xFrom; xTo] (с ручками и размером окна по умолчанию)
        public static void DrawFunction(RealFunction f, double xFrom, double xTo)
        {
            DrawFunction(f, xFrom, xTo, DefaultPen, DefaultAxisPen, DefaultWindowSize);
        }
        // Нарисовать f на [xFrom; xTo] ручкой pen, оси координат ручкой axisPen (размером окна по умолчанию)
        public static void DrawFunction(RealFunction f, double xFrom, double xTo, Pen pen, Pen axisPen)
        {
            DrawFunction(f, xFrom, xTo, pen, axisPen, DefaultWindowSize);
        }
        // Нарисовать f на [xFrom; xTo] в окне размером windowSize (с ручками по умолчанию) 
        public static void DrawFunction(RealFunction f, double xFrom, double xTo, Point windowSize)
        {
            DrawFunction(f, xFrom, xTo, DefaultPen, DefaultAxisPen, windowSize);
        }
        // Нарисовать f на [xFrom; xTo] ручкой pen, оси координат ручкой axisPen, размером окна windowSize
        public static void DrawFunction(RealFunction f, double xFrom, double xTo, Pen pen, Pen axisPen, Point windowSize)
        {
            if (windowSize.X * windowSize.Y == 0 || xFrom > xTo)
            {
                throw new ArgumentException("Неверные параметры! xFrom не должно быть больше xTo," +
                                            "измерения windowSize не должны быть нулевыми.");
            }
            var image = new Bitmap(windowSize.X, windowSize.Y);

            var minMax = GetMinMaxValues(f, xFrom, xTo, windowSize.Y);
            double minY = minMax.Item1,
                   maxY = minMax.Item2;

            DrawAxis(image, axisPen, windowSize, xFrom, xTo, minY, maxY);

            // Если нужно нарисовать 1 точку по x
            if (Math.Abs(xFrom - xTo) < Eps)
            {
                // Если значение в ней определено
                var y = SafeValue(f, xFrom);
                if (!double.IsNaN(y) && !double.IsInfinity(y))
                {
                    pen.Width = 4;
                    var g = Graphics.FromImage(image);
                    g.DrawEllipse(pen, windowSize.X/2 - 2, windowSize.X/2, 4, 4);
                    pen.Width = 1;
                }
                ShowImageInWindow(image);
                return;
            }

            // Определим первую точку по x, которую необходимо отобразить
            int screenX = 0, screenY;
            Point? oldPoint = null, nextPoint = null;
            var maxYEqualsMinY = Math.Abs(maxY - minY) < Eps;

            for (; screenX < windowSize.X; ++screenX)
            {
                double x = xFrom + screenX*(xTo - xFrom)/windowSize.X,
                       y = SafeValue(f, x);
                if (double.IsNaN(y) || double.IsInfinity(y))
                    continue;
                // Если функция вида y = C (minY=maxY), рисуем на середине экрана; иначе обычный перевод в экранные
                screenY = maxYEqualsMinY
                    ? windowSize.Y/2
                    : (int) ((maxY - y)*windowSize.Y/(maxY - minY));
                oldPoint = new Point(screenX, screenY);
                break;
            }

            // Нашли первую определенную точку
            if (oldPoint != null)
            {
                for (; screenX < windowSize.X; ++screenX)
                {
                    double x = xFrom + screenX*(xTo - xFrom)/windowSize.X,
                           y = SafeValue(f, x);
                    if (double.IsNaN(y) || double.IsInfinity(y))
                    {
                        // Новая точка не определена, нужно нарисовать точку в текущем положении
                        if (oldPoint != null)
                            DrawLineWithBresenham(oldPoint.Value, oldPoint.Value, image, pen.Color);
                        oldPoint = null;
                        continue;
                    }
                    screenY = maxYEqualsMinY
                        ? windowSize.Y/2
                        : (int) ((maxY - y)*windowSize.Y/(maxY - minY));
                    nextPoint = new Point(screenX, screenY);

                    // Если есть откуда рисовать, проводим прямую до новой точки 
                    if (oldPoint != null)
                        DrawLineWithBresenham(oldPoint.Value, nextPoint.Value, image, pen.Color);
                    oldPoint = nextPoint;
                }
            }
            ShowImageInWindow(image);
        }

// Task2
        // Нарисовать параболу с фок.параметром p с помощью алгоритма Брехенхема
        // Версию с комментариями смотри в Task2
        public static void DrawParabolaWithBresenham(double p, Bitmap image, Color color, Point windowSize)
        {
            int maxX = windowSize.X/2, maxY = windowSize.Y/2;
            int x = 0, y = 0;
            var deltaX = (p < Eps) ? -1 : 1;
            image.SetPixel(maxX, maxY, color);
            Func<int, int, int> accuracyOf = (x2, y2) => y2*y2 - (int)(2*p*x2);

            for (;;)
            {
                int dDiag = accuracyOf(x + deltaX, y + 1),
                    dVert = accuracyOf(x, y + 1),
                    dHori = accuracyOf(x + deltaX, y);
                if (Math.Abs(dHori) < Math.Abs(dVert)) {
                    if (Math.Abs(dDiag) < Math.Abs(dHori))
                        y++;
                    x += deltaX;
                } else {
                    if (Math.Abs(dDiag) < Math.Abs(dVert))
                        x += deltaX;
                    y++;
                }
                if (Math.Abs(x) >= maxX || Math.Abs(y) >= maxY) break;
                image.SetPixel(x + maxX, maxY - y, color);
                image.SetPixel(x + maxX, maxY + y, color);
            }
        }

// Прочее

        public static void DrawLineWithBresenham(Point pFrom, Point pTo, Bitmap image, Pen pen)
        {
            if (pFrom.X == 0) pFrom.X++;
            if (pFrom.Y == 0) pFrom.Y++;
            if (pTo.X == 0) pTo.X++;
            if (pTo.Y == 0) pTo.Y++;
            if (pFrom.X == image.Width) pFrom.X--;
            if (pFrom.Y == image.Height) pFrom.Y--;
            if (pTo.X == image.Width) pTo.X--;
            if (pTo.Y == image.Height) pTo.Y--;
            DrawLineWithBresenham(pFrom.X, pFrom.Y, pTo.X, pTo.Y, image, pen);
        }
        private static void DrawLineWithBresenham(int x0, int y0, int x1, int y1, Bitmap image, Pen pen)
        {
            var width = (int)pen.Width;
            var even = (width + 1) % 2;
            for (var w = -width / 2; w <= width / 2 - even; ++w)
            {
                if (Math.Abs(image.Width / 2 - (x0 + w)) < image.Width / 2 && Math.Abs(image.Width / 2 - (x1 + w)) < image.Width / 2)
                    DrawLineWithBresenham(x0 + w, y0, x1 + w, y1, image, pen.Color);
                if (Math.Abs(image.Height / 2 - (y0 + w)) < image.Height / 2 && Math.Abs(image.Height / 2 - (y1 + w)) < image.Height / 2)
                    DrawLineWithBresenham(x0, y0 + w, x1, y1 + w, image, pen.Color);
            }
        }
        public static void DrawLineWithBresenham(Point pFrom, Point pTo, Bitmap image, Color color)
        {
            DrawLineWithBresenham(pFrom.X, pFrom.Y, pTo.X, pTo.Y, image, color);
        }
        public static void DrawLineWithBresenham(int x0, int y0, int x1, int y1, Bitmap image, Color color)
        {
            var steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            // Отражаем линию по диагонали, если угол наклона слишком большой
            if (steep)
            {
                Swap(ref x0, ref y0);
                Swap(ref x1, ref y1);
            }
            // Если линия растёт не слева направо, то меняем начало и конец отрезка местами
            if (x0 > x1)
            {
                Swap(ref x0, ref x1);
                Swap(ref y0, ref y1);
            }
            int dx = x1 - x0,
                dy = Math.Abs(y1 - y0),
                error = (dx >> 1), // Здесь используется оптимизация с умножением на dx, чтобы избавиться от лишних дробей
                ystep = (y0 < y1) ? 1 : -1, // Выбираем направление роста координаты y
                y = y0;
            for (var x = Math.Max(1, x0); x <= Math.Min(image.Width - 1, x1); x++)
            {
                if (Math.Abs(image.Width / 2 - y) >= image.Width / 2 || Math.Abs(image.Height / 2 - y) >= image.Height / 2)
                    continue;
                image.SetPixel(steep ? y : x, steep ? x : y, color);
                error -= dy;
                if (error >= 0) continue;
                y += ystep;
                error += dx;
            }
        }

    }
}
