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
            new [] {new Point(10, 10), new Point(10, 20), new Point(20, 20), new Point(20, 10), },
            new [] {new Point(10, 20), new Point(20, 30), new Point(30, 20), new Point(20, 40), new Point(50, 10), new Point(30, 10), new Point(10, 20),},
            new []
            {
                new Point(170,140), new Point(140,140), new Point(140,110),new Point(180,110),new Point(180,100),new Point(130,100),new Point(130,140),new Point(140,150),
                new Point(160,150),new Point(140,170),new Point(110,170),new Point(100,190),new Point(140,190),new Point(170,160),new Point(210,160),new Point(210,180),
                new Point(230,180),new Point(230,160),new Point(250,160),new Point(250,140),new Point(230,140),new Point(230,120),new Point(210,120),new Point(210,140),
                new Point(190,140),new Point(190,159),new Point(170,159),
            }, 
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
            
            windowSize = new Point(600, 600);
            var image = new Bitmap(windowSize.X, windowSize.Y);
            var g = Graphics.FromImage(image);
            g.FillRectangle(Core.DefaultBackgroundBrush, 0, 0, windowSize.X-1, windowSize.Y-1);
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
            Point minPoint = new Point(int.MaxValue, int.MaxValue),
                maxPoint = new Point(int.MinValue, int.MinValue);
            foreach (var v in polygon)
            {
                minPoint.X = Math.Min(minPoint.X, v.X);
                minPoint.Y = Math.Min(minPoint.Y, v.Y);
                maxPoint.X = Math.Max(maxPoint.X, v.X);
                maxPoint.Y = Math.Max(maxPoint.Y, v.Y);
            }
            var bestSquare = new [] {new Point(0, 0), new Point(0, image.Height), new Point(image.Width, image.Height), new Point(image.Width, 0)};
            var square = new Point[4];
            var minSize = (int)Math.Sqrt((maxPoint.X-minPoint.X)*(maxPoint.X-minPoint.X) + (maxPoint.Y-minPoint.Y)*(maxPoint.Y-minPoint.Y)) + 1;
            var cnt = 0;
            
            for (var x = minPoint.X-1; x <= maxPoint.X; ++x)
            {
                for (var y = minPoint.Y; y >= 0; --y)
                {
                    if (IsPointInsidePolygon(polygon, new Point(x, y)) == 1)
                        break;
                    for (var size = 1; size < minSize; ++size)
                    {
                        for (var phi = 0.0; phi < Math.PI / 2; phi += 0.1)
                        {
                            square[0].X = x;                                 square[0].Y = y;
                            square[1].X = x + RotateByPhiX(0, size, phi);    square[1].Y = y + RotateByPhiY(0, size, phi);
                            square[2].X = x + RotateByPhiX(size, size, phi); square[2].Y = y + RotateByPhiY(size, size, phi);
                            square[3].X = x + RotateByPhiX(size, 0, phi);    square[3].Y = y + RotateByPhiY(size, 0, phi);
                            if (maxPoint.X > square[3].X || minPoint.Y > square[1].Y)
                                goto break_phi_loop;
                            if (square[1].X < 0 || square[3].X > image.Width || square[2].Y > image.Height)
                                goto break_size_loop;   
                            if (polygon.Sum(pt => IsPointInsidePolygon(square, pt)) == polygon.Length)
                            {
                                ++cnt;
                                minSize = size;
                                bestSquare[0] = square[0];
                                bestSquare[1] = square[1];
                                bestSquare[2] = square[2];
                                bestSquare[3] = square[3];
                                break;   
                            }
                        }
                    break_phi_loop: ;
                    }
                break_size_loop: ;
                }
            }
            Console.WriteLine("{0}, {1}", cnt, minSize);
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
