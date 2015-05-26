using System;
using System.Drawing;
using CGG;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;
using Point = System.Drawing.Point;

namespace Task3
{
    //    Task3: Описать вокруг произвольного многоугольника
    //           квадрат минимальной площади.

    class Program
    {
        private static readonly Point[][] Tests =
        {
            new [] {new Point(100, 100), new Point(100, 200), new Point(200, 200), new Point(200, 100), },
//            new Point[] {},
//            new Point[] {}, 
        };

        private static void DrawPolygon(Point[] polygon)
        {
            DrawParametricFunctionWithParams(polygon, Core.DefaultPen, Core.DefaultWindowSize);
        }
        private static void DrawParametricFunctionWithParams(Point[] arg, Pen pen, Point windowSize)
        {
            if (windowSize.X * windowSize.Y == 0)
            {
                throw new ArgumentException("Неверные параметры! xFrom не должно быть больше xTo," +
                                            "измерения windowSize не должны быть нулевыми.");
            }
            var image = new Bitmap(windowSize.X, windowSize.Y);
            Graphics g = Graphics.FromImage(image);
            Brush brush = Brushes.DarkRed; // TODO <- DefaulPen.Color
            //g. (brush, 0, 0, windowSize.X, windowSize.Y);
  
            Core.ShowImageInWindow(image);
        }

        public static void Main(string[] args)
        {
            foreach (var polygon in Tests)
            {

            }
        }
    }
}
