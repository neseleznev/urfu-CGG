using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
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
            new [] {new Point(300, 300), new Point(200, 200),  new Point(300, 400),  new Point(400, 500), },
            new [] {new Point(100, 200), new Point(250, 210), new Point(400, 200), new Point(250, 190), },
            new [] {new Point(10, 10), new Point(10, 20), new Point(20, 20), new Point(20, 10), },
            new [] {new Point(10, 20), new Point(20, 30), new Point(30, 20), new Point(20, 40), new Point(50, 10), new Point(30, 10), new Point(10, 20),},
            new [] {new Point(10, 10), new Point(20, 10), new Point(20, 30), new Point(30, 30), new Point(30, 20), new Point(10, 20), },
            new [] {new Point(110, 110), new Point(110, 150), new Point(150, 150), new Point(150, 130), new Point(130, 130), new Point(130, 160), new Point(220, 170), new Point(200, 110), },
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
            
            windowSize = new Point(400, 500);
            var image = new Bitmap(windowSize.X, windowSize.Y);
            var g = Graphics.FromImage(image);
            g.FillRectangle(Core.DefaultBackgroundBrush, 0, 0, windowSize.X-1, windowSize.Y-1);
            g.FillPolygon(brush, polygon);
            return image;
        }

//        // Uncomment if you want minSquare to fit the screen
//        public static bool IsSquareFits(Point[] minSquare, Image image)
//        {
//            return minSquare.All(t => (0 <= t.X && t.X <= image.Width) && (0 <= t.Y && t.Y <= image.Height));
//        }

        public static Point RotateByPhi(Point p, double phi)
        {
            return new Point((int)(p.X * Math.Cos(-phi) - p.Y * Math.Sin(-phi)),
                             (int)(p.X * Math.Sin(-phi) + p.Y * Math.Cos(-phi)));
        }

        private static Point[] FindSquareConvex(Bitmap image, Point[] polygon)
        {
            var minSquare = new [] {new Point(0, 0), new Point(0, image.Height), new Point(image.Width, image.Height), new Point(image.Width, 0)};
            var square = new Point[4];
            var minSize = Math.Min(image.Width, image.Height);

            for (var phi = 0.0; phi < Math.PI/2; phi += 0.1)
            {
                var rotatedPolygon = polygon.Select(point => RotateByPhi(point, phi)).ToList();

                Point minPoint = new Point(int.MaxValue, int.MaxValue),
                    maxPoint = new Point(int.MinValue, int.MinValue);
                foreach (var v in rotatedPolygon)
                {
                    minPoint.X = Math.Min(minPoint.X, v.X);
                    minPoint.Y = Math.Min(minPoint.Y, v.Y);
                    maxPoint.X = Math.Max(maxPoint.X, v.X);
                    maxPoint.Y = Math.Max(maxPoint.Y, v.Y);
                }

                int minX = maxPoint.X - minPoint.X,
                    minY = maxPoint.Y - minPoint.Y;
                if (Math.Max(minX, minY) < minSize)
                {
                    var delta = Math.Abs(minX - minY);
                    if (minX < minY)
                    {
                        square[0] = RotateByPhi(new Point(minPoint.X - delta, minPoint.Y), -phi);
                        square[1] = RotateByPhi(new Point(minPoint.X - delta, maxPoint.Y), -phi);
                        square[2] = RotateByPhi(new Point(maxPoint.X, maxPoint.Y), -phi);
                        square[3] = RotateByPhi(new Point(maxPoint.X, minPoint.Y), -phi);
//                        // Uncomment if you want minSquare to fit the screen
//                        if (!IsSquareFits(square, image))
//                        {
//                            square[0] = RotateByPhi(new Point(minPoint.X, minPoint.Y), -phi);
//                            square[1] = RotateByPhi(new Point(minPoint.X, maxPoint.Y), -phi);
//                            square[2] = RotateByPhi(new Point(maxPoint.X+delta, maxPoint.Y), -phi);
//                            square[3] = RotateByPhi(new Point(maxPoint.X+delta, minPoint.Y), -phi);
//                            if (!IsSquareFits(square, image))
//                                continue;
//                        }
                    }
                    else
                    {
                        square[0] = RotateByPhi(new Point(minPoint.X, minPoint.Y-delta), -phi);
                        square[1] = RotateByPhi(new Point(minPoint.X, maxPoint.Y), -phi);
                        square[2] = RotateByPhi(new Point(maxPoint.X, maxPoint.Y), -phi);
                        square[3] = RotateByPhi(new Point(maxPoint.X, minPoint.Y-delta), -phi);
//                        // Uncomment if you want minSquare to fit the screen
//                        if (!IsSquareFits(square, image))
//                        {
//                            square[0] = RotateByPhi(new Point(minPoint.X, minPoint.Y), -phi);
//                            square[1] = RotateByPhi(new Point(minPoint.X, maxPoint.Y+delta), -phi);
//                            square[2] = RotateByPhi(new Point(maxPoint.X, maxPoint.Y+delta), -phi);
//                            square[3] = RotateByPhi(new Point(maxPoint.X, minPoint.Y), -phi);
//                            if (!IsSquareFits(square, image))
//                                continue;
//                        }
                    }
                    minSize = Math.Max(minX, minY);
                    for (var i = 0; i < 4; ++i)
                        minSquare[i] = square[i];
                }
            }
            Console.WriteLine(minSize);
            return minSquare;
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
