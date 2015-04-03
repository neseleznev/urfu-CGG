using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CGG
{
    public class Core
    {
        public const double Eps = 1e-9;
        public delegate double FuncToShow(double x);

        public static readonly Pen DefaultPen = new Pen(Color.DarkRed, 1);
        public static readonly Pen DefaultAxisPen = new Pen(Color.DarkBlue, 3);
        public static readonly Point DefaultWindowSize = new Point(600, 600);

        public static double SafeValue(FuncToShow f, double x)
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


        // Вычислим минимальное и максимальное значения функции на заданном интервале [xFrom; xTo]
        public static Tuple<double, double> GetMinMaxValues(FuncToShow f, double xFrom, double xTo, int windowWidth)
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


        // Нарисуем необходимые оси в соответствии с диапазоном значений функции
        public static void DrawAxis(Graphics g, Pen axisPen, Point windowSize,
                                     double minX, double maxX, double minY, double maxY)
        {
            int? screenX = null, screenY = null;

            // Необходимость оси ординат
            if (Math.Abs(maxX) < Eps && Math.Abs(minX) < Eps)
                screenX = windowSize.Y / 2;
            if (minX < 0 && maxX > 0 && Math.Abs(maxX - minX) > Eps)
                screenX = (int)(-minX * windowSize.X / (maxX - minX));
            // Необходимость оси абсцисс
            if (Math.Abs(maxY) < Eps && Math.Abs(maxY) < Eps)
                screenY = windowSize.X / 2;
            if (minY < 0 && maxY > 0)
                screenY = (int)(maxY * windowSize.Y / (maxY - minY));

            if (screenX != null)
                g.DrawLine(axisPen, screenX.Value, windowSize.Y, screenX.Value, 0);
            if (screenY != null)
                g.DrawLine(axisPen, 0, screenY.Value, windowSize.X, screenY.Value);
        }

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

    }
}
