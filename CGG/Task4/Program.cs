using System;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using CGG;
// Task5
// Интервальный алгоритм построчного сканирования
// Объект: 2 тетраэдра
//
//
namespace Task4
{
    public class CoordConverter
    {
        public Point Center { get; private set; }

        public CoordConverter(int centerX, int centerY)
        {
            Center = new Point(centerX, centerY);
        }

        public Point ToPlaneCoord(double x, double y, double z)
        {
            return new Point(PlaneX(x, y) + Center.X, PlaneY(x, z) + Center.Y);
        }

        private static int PlaneX(double x, double y)
        {
            return (int)(-x / (2 * Math.Sqrt(2)) + y);
        }

        private static int PlaneY(double x, double z)
        {
            return (int)(x / (2 * Math.Sqrt(2)) - z);
        }
    }

    class Program
    {
        public static double SafeValue(Core.Real3DFunction z, double x, double y)
        {
            try
            {
                return z(x, y);
            }
            catch (OverflowException)
            {
                return double.NaN;
            }
        }

        private static double GetZoomCoeff(Core.Real3DFunction function, Point from, Point to,
            CoordConverter coordConverter, double accuracy, Point windowSize)
        {
            var zoomCoef = Math.Min((double)windowSize.X / Math.Abs(to.X - from.X), (double)windowSize.Y / Math.Abs(to.Y - from.Y));
            Point minPoint = new Point(int.MaxValue, int.MaxValue),
                maxPoint = new Point(int.MinValue, int.MinValue);

            foreach (var x in Core.Range(to.X, from.X, accuracy))
            {
                foreach (var y in Core.Range(from.Y, to.Y, accuracy/100))
                {
                    var z = SafeValue(function, x, y);
                    if (double.IsNaN(z) || Math.Abs(z) > 1e9) continue;
                    var planePoint = coordConverter.ToPlaneCoord(x*zoomCoef, y*zoomCoef, z*zoomCoef);
                    minPoint.X = Math.Min(minPoint.X, planePoint.X);
                    minPoint.Y = Math.Min(minPoint.Y, planePoint.Y);
                    maxPoint.X = Math.Max(maxPoint.X, planePoint.X);
                    maxPoint.Y = Math.Max(maxPoint.Y, planePoint.Y);
                }
            }
            foreach (var y in Core.Range(to.Y, from.Y, accuracy))
            {
                foreach (var x in Core.Range(from.X, to.X, accuracy/100))
                {
                    var z = SafeValue(function, x, y);
                    if (double.IsNaN(z) || Math.Abs(z) > 1e9) continue;
                    var planePoint = coordConverter.ToPlaneCoord(x*zoomCoef, y*zoomCoef, z*zoomCoef);
                    minPoint.X = Math.Min(minPoint.X, planePoint.X);
                    minPoint.Y = Math.Min(minPoint.Y, planePoint.Y);
                    maxPoint.X = Math.Max(maxPoint.X, planePoint.X);
                    maxPoint.Y = Math.Max(maxPoint.Y, planePoint.Y);
                }
            }
            return zoomCoef * Math.Min((double)windowSize.X / (maxPoint.X-minPoint.X), (double)windowSize.Y / (maxPoint.Y-minPoint.Y));
        }

        private static Tuple<Point, Point> GetMinMaxPoints(Core.Real3DFunction function, Point from, Point to,
            double zoomCoef, CoordConverter coordConverter, double accuracy)
        {
            Point minPoint = new Point(int.MaxValue, int.MaxValue),
                maxPoint = new Point(int.MinValue, int.MinValue);
            foreach (var y in Core.Range(to.Y, from.Y, accuracy))
            {
                foreach (var x in Core.Range(from.X, to.X, accuracy / 100))
                {
                    var z = SafeValue(function, x, y);
                    if (double.IsNaN(z) || Math.Abs(z) > 1e9) continue;
                    var planePoint = coordConverter.ToPlaneCoord(x*zoomCoef, y*zoomCoef, z*zoomCoef);
                    minPoint.X = Math.Min(minPoint.X, planePoint.X);
                    minPoint.Y = Math.Min(minPoint.Y, planePoint.Y);
                    maxPoint.X = Math.Max(maxPoint.X, planePoint.X);
                    maxPoint.Y = Math.Max(maxPoint.Y, planePoint.Y);
                }
            }
            return new Tuple<Point, Point>(minPoint, maxPoint);
        }

        static private void CreateImage(Core.Real3DFunction function, Point from, Point to, Point minPoint, Point maxPoint,
            CoordConverter coordConverter, double zoomCoef, double accuracy, Graphics g, Pen topPen, Pen bottomPen, Point windowSize)
		{
			var topHorizon = new int[windowSize.X];
			var bottomHorizon = new int[windowSize.X];
			for (var i = 0; i < windowSize.X; i++)
			{
				topHorizon[i] = int.MinValue;
				bottomHorizon[i] = int.MaxValue;
			}
            int shiftX = - minPoint.X + (windowSize.X - (maxPoint.X-minPoint.X))/2,
                shiftY = - minPoint.Y + (windowSize.Y - (maxPoint.Y-minPoint.Y))/2;

			foreach (var x in Core.Range(to.X, from.X, accuracy))
			{
                var lastPoint = coordConverter.ToPlaneCoord(x * zoomCoef, from.Y * zoomCoef, function(x, from.Y) * zoomCoef);
			    lastPoint.X += shiftX;
			    lastPoint.Y += shiftY;

                foreach (var y in Core.Range(from.Y, to.Y, accuracy / 100))
				{
                    var planePoint = coordConverter.ToPlaneCoord(x * zoomCoef, y * zoomCoef, function(x, y) * zoomCoef);
				    planePoint.X += shiftX;
				    planePoint.Y += shiftY;

					if (planePoint.X < 0 || planePoint.X >= windowSize.X || planePoint.Y < 0 || planePoint.Y >= windowSize.Y)
						continue;
					if (lastPoint.X == planePoint.X)
						continue;
                    if (lastPoint.X < 0 || lastPoint.X >= windowSize.X || lastPoint.Y < 0 || lastPoint.Y >= windowSize.Y)
                        lastPoint = planePoint;

					if (planePoint.Y >= topHorizon[planePoint.X])
					{
                        topHorizon[planePoint.X] = planePoint.Y;
                        g.DrawLine(topPen, lastPoint.X, lastPoint.Y, planePoint.X, planePoint.Y);
					}
					if (planePoint.Y <= bottomHorizon[planePoint.X])
					{
                        bottomHorizon[planePoint.X] = planePoint.Y;
                        g.DrawLine(bottomPen, lastPoint.X, lastPoint.Y, planePoint.X, planePoint.Y);
					}
					lastPoint = planePoint;
				}
			}


//            topHorizon = new int[windowSize.Y];
//            bottomHorizon = new int[windowSize.Y];
            for (var i = 0; i < windowSize.X; i++)
            {
                topHorizon[i] = int.MinValue;
                bottomHorizon[i] = int.MaxValue;
            }

            foreach (var y in Core.Range(to.Y, from.Y, accuracy))
            {
                var lastPoint = coordConverter.ToPlaneCoord(from.X * zoomCoef, y * zoomCoef, function(from.X, y) * zoomCoef);
                lastPoint.X += shiftX;
                lastPoint.Y += shiftY;
                
                foreach (var x in Core.Range(from.X, to.X, accuracy / 100))
                {
                    var planePoint = coordConverter.ToPlaneCoord(x * zoomCoef, y * zoomCoef, function(x, y) * zoomCoef);
                    planePoint.X += shiftX;
                    planePoint.Y += shiftY;

                    if (planePoint.X < 0 || planePoint.X >= windowSize.X || planePoint.Y < 0 || planePoint.Y >= windowSize.Y)
                        continue;
                    if (lastPoint.X == planePoint.X)
                        continue;
                    if (lastPoint.X < 0 || lastPoint.X >= windowSize.X || lastPoint.Y < 0 || lastPoint.Y >= windowSize.Y)
                        lastPoint = planePoint;

                    if (planePoint.Y >= topHorizon[planePoint.X])
                    {
                        topHorizon[planePoint.X] = planePoint.Y;
                        g.DrawLine(topPen, lastPoint.X, lastPoint.Y, planePoint.X, planePoint.Y);
                    }
                    if (planePoint.Y <= bottomHorizon[planePoint.X])
                    {
                        bottomHorizon[planePoint.X] = planePoint.Y;
                        g.DrawLine(bottomPen, lastPoint.X, lastPoint.Y, planePoint.X, planePoint.Y);
                    }
                    lastPoint = planePoint;
                }
            }
        }

// Task4
        public static void Draw3DFunction(Core.Real3DFunction z, Point from, Point to)
        {
            Draw3DFunction(z, from, to, Core.DefaultPen, Core.DefaultPen3DTop, Core.DefaultPen3DBottom, Core.DefaultWindowSize);
        }
        public static void Draw3DFunction(Core.Real3DFunction z, Point from, Point to, Pen pen, Pen axisPen)
        {
            Draw3DFunction(z, from, to, pen, Core.DefaultPen3DTop, Core.DefaultPen3DBottom, Core.DefaultWindowSize);
        }
        public static void Draw3DFunction(Core.Real3DFunction z, Point from, Point to, Point windowSize)
        {
            Draw3DFunction(z, from, to, Core.DefaultPen, Core.DefaultPen3DTop, Core.DefaultPen3DBottom, windowSize);
        }
        public static void Draw3DFunction(Core.Real3DFunction z, Point from, Point to, Pen pen, Pen topPen, Pen bottomPen, Point windowSize)
        {
            if ((windowSize.X * windowSize.Y == 0))// || ((windowSize.X ^ windowSize.Y) != 0))
                throw new ArgumentException("Неверные параметры! Измерения windowSize должны быть ненулевыми и совпадать.");

            var image = new Bitmap(windowSize.X, windowSize.Y);
            var g = Graphics.FromImage(image);
            g.FillRectangle(Core.DefaultBackgroundBrush, 0, 0, image.Width, image.Height);

            var coordConverter = new CoordConverter(windowSize.X / 2, windowSize.Y / 2);

            var accuracy = Math.Min((double)(to.X - from.X) / 100, (double)(to.Y - from.Y) / 100);
            var zoomCoef = GetZoomCoeff(z, from, to, coordConverter, accuracy, windowSize);
            var minMax = GetMinMaxPoints(z, from, to, zoomCoef, coordConverter, accuracy);

            CreateImage(z, from, to, minMax.Item1, minMax.Item2, coordConverter, zoomCoef, accuracy, g, topPen, bottomPen, windowSize);
            Core.ShowImageInWindow(image);
        }

        public static double SlicedSphere(double x, double y)
        {
            const double r = 2.5;
            if (x > 0 && y < 0) // 4 Квадрант
                return -Math.Sqrt(-(x-r)*(x-r) - (y+r)*(y+r) + r*r) + r/2;
            if (x < 0 && y > 0) // 2 Квадрант
                return Math.Sqrt(-(x+r)*(x+r) - (y-r)*(y-r) + r*r) - r/2;
            return double.NaN;
        }

        private static readonly Core.Real3DFunction[] Functions =
        {
            (x, y) => x*x + y*y,
            (x, y) => 3*Math.Sin(x*y),
            SlicedSphere,
            (x, y) => (x),
            (x, y) => (x + y),
            (x, y) => x*y,
            (x, y) => x*Math.Sin(x*y),
            (x, y) => Math.Sin(y * Math.Cos(x)),
            (x, y) => Math.Sin(x*x + y*y) * Math.Cos(x - y),
            (x, y) =>  Math.Sqrt(x*x+y*y)+3*Math.Cos(Math.Sqrt(x*x+y*y)) + 5,
//            (x, y) => ,
//            (x, y) => ,
//            (x, y) => ,
//            (x, y) => ,

//             Злые примеры
//            (x, y) => Math.Sin(Math.Sqrt(x*y)) + Math.Log10(Math.Cos(y)),
//            (x, y) => Math.Sin(x*y + y) * Math.Log10(Math.Cos(y)),
        };

        public static void Main(string[] args)
        {
            Core.Real3DFunction goodMorning =
                (x, y) => 100.0 - 3.0/Math.Sqrt(Math.Max(x*x + y*y, 0.02)) + Math.Sin(Math.Sqrt(x*x + y*y)) +
                          Math.Sqrt(200.0 - x*x + y*y + 10.0*Math.Sin(x) + 10.0*Math.Sin(y))/1000.0;
            Draw3DFunction(goodMorning, new Point(-15, -15), new Point(15, 15), new Point(1920 - 100, 1080 - 100));

            foreach (var function in Functions)
                Draw3DFunction(function, new Point(-2, -2), new Point(5, 5), new Point(1200, 675));
        }
    }
}
