using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Shapes;
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
//            new [] {new PointF(10, 10), new PointF(10, 20), new PointF(20, 20), new PointF(20, 10), },
            new [] {new Point(10, 20), new Point(20, 30), new Point(30, 20), new Point(20, 40), new Point(50, 10), new Point(30, 10), new Point(10, 20),},
//            new Point[] {}, 
        };

        public static int IsPointInsidePolygon(Point[] p, Point point)
        {
            int flag = 0, len = p.Length;
            for (var n = 0; n < len; n++)
            {
                flag = 0;
                var i1 = n < len - 1 ? n + 1 : 0;
                while (flag == 0)
                {
                    var i2 = i1 + 1;
                    if (i2 >= len)
                        i2 = 0;
                    if (i2 == (n < len - 1 ? n + 1 : 0))
                        break;
                    var s = Math.Abs(p[i1].X * (p[i2].Y - p[n].Y) +
                                     p[i2].X * (p[n].Y - p[i1].Y) +
                                     p[n].X * (p[i1].Y - p[i2].Y));
                    var s1 = Math.Abs(p[i1].X * (p[i2].Y - point.Y) +
                                      p[i2].X * (point.Y - p[i1].Y) +
                                      point.X * (p[i1].Y - p[i2].Y));
                    var s2 = Math.Abs(p[n].X * (p[i2].Y - point.Y) +
                                      p[i2].X * (point.Y - p[n].Y) +
                                      point.X * (p[n].Y - p[i2].Y));
                    var s3 = Math.Abs(p[i1].X * (p[n].Y - point.Y) +
                                      p[n].X * (point.Y - p[i1].Y) +
                                      point.X * (p[i1].Y - p[n].Y));
                    if (s == s1 + s2 + s3)
                    {
                        flag = 1;
                        break;
                    }
                    i1 = i1 + 1;
                    if (i1 >= len)
                        i1 = 0;
                }
                if (flag == 0)
                    break;
            }
            return flag;
        }


        private static Image DrawPolygon(Point[] polygon)
        {
            return DrawPolygon(polygon, Core.DefaultPen.Brush, Core.DefaultWindowSize);
        }
        private static Image DrawPolygon(Point[] polygon, Brush brush, Point windowSize)
        {
            if (windowSize.X * windowSize.Y == 0)
                throw new ArgumentException("Неверные параметры! Измерения windowSize не должны быть нулевыми.");
            
            windowSize = new Point(100, 100);
            var image = new Bitmap(windowSize.X, windowSize.Y);
            var g = Graphics.FromImage(image);
            g.FillRectangle(Core.DefaultBackgroundBrush, 0, 0, windowSize.X, windowSize.Y);
            g.FillPolygon(brush, polygon);
            return image;
        }

        public static int RotateByPhiX(int x, int y, double phi)
        {
            return (int)(x * Math.Cos(phi) - y * Math.Sin(phi));
        }
        public static int RotateByPhiY(int x, int y, double phi)
        {
            return (int)(x * Math.Sin(phi) + y * Math.Cos(phi));
        }

        private static Point[] FindSquareConvex(Bitmap image, Point[] polygon)
        {
            int minY = int.MaxValue,
                maxX = int.MinValue;
            foreach (var v in polygon)
            {
                minY = Math.Min(minY, v.Y);
                maxX = Math.Max(maxX, v.X);
            }
            var bestSquare = new [] {new Point(0, 0), new Point(0, image.Height), new Point(image.Width, image.Height), new Point(image.Width, 0)};
            var square = new Point[4];
            var minSide = int.MaxValue;
            var cnt = 0;
            
            for (var x = 0; x <= maxX; ++x)
            {
                for (var y = 0; y <= minY; ++y)
                {
                    for (var phi = 0.0; phi < Math.PI/2; phi += 0.001)
                    {
                        for (var size = 1; size < image.Height; ++size)
                        {
                            square[0] = new Point(x,                                 y);
                            square[1] = new Point(x + RotateByPhiX(0, size, phi),    y + RotateByPhiY(0, size, phi));
                            square[2] = new Point(x + RotateByPhiX(size, size, phi), y + RotateByPhiY(size, size, phi));
                            square[3] = new Point(x + RotateByPhiX(size, 0, phi),    y + RotateByPhiY(size, 0, phi));
                            if (polygon.Sum(pt => IsPointInsidePolygon(square, pt)) == polygon.Length)
                            {
                                ++cnt;
                                var side = (square[1].X - square[0].X) * (square[1].X - square[0].X) +
                                          (square[1].Y - square[0].Y) * (square[1].Y - square[0].Y);
                                if (!(side < minSide)) break;
                                minSide = side;
                                for (var i = 0; i < 4; ++i)
                                    bestSquare[i] = square[i];
                                break;   
                            }
                        }
                    }
                }
            }
            Console.WriteLine("{0}, {1}", cnt, Math.Sqrt(minSide));
            return bestSquare;
        }

        public static void Main(string[] args)
        {
            foreach (var polygon in Tests)
            {
                var image = DrawPolygon(polygon);
                var square = FindSquareConvex((Bitmap)image, polygon);
                var pen = new Pen(Color.DodgerBlue, 1);
                Graphics.FromImage(image).DrawPolygon(pen, square);
                Core.ShowImageInWindow(image);
            }
        }
    }
}
