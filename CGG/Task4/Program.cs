using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
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
            return new Point(PlaneX(x, y, z) + Center.X, PlaneY(x, y, z) + Center.Y);
        }

        private static int PlaneX(double x, double y, double z)
        {
            return (int)(-x / (2 * Math.Sqrt(2)) + y);
        }

        private static int PlaneY(double x, double y, double z)
        {
            return (int)(x / (2 * Math.Sqrt(2)) - z);
        }
    }

    class Program
    {
        static private double GetZoomCoeff(Core.Real3DFunction function, Point from, Point to, double accuracy,
            int windowSize)
        {
            var zoomCoef = (double) windowSize/Math.Max(Math.Abs(to.X - from.X), Math.Abs(to.Y - from.Y)); 
            Point minPoint = new Point(int.MaxValue, int.MaxValue),
                maxPoint = new Point(int.MinValue, int.MinValue);

            var coordConverter = new CoordConverter(windowSize/2, windowSize/2);
            foreach (var x in Core.Range(to.X, from.X, accuracy))
            {
                foreach (var y in Core.Range(from.Y, to.Y, accuracy / 100))
                {
                    var z = function(x, y);
                    var planePoint = coordConverter.ToPlaneCoord(x * zoomCoef, y * zoomCoef, z * zoomCoef);

                    minPoint.X = Math.Min(minPoint.X, planePoint.X);
                    minPoint.Y = Math.Min(minPoint.Y, planePoint.Y);
                    maxPoint.X = Math.Max(maxPoint.X, planePoint.X);
                    maxPoint.Y = Math.Max(maxPoint.Y, planePoint.Y);
                }
            }
            return zoomCoef * windowSize / Math.Max(maxPoint.X - minPoint.X, maxPoint.Y - minPoint.Y);
        }

		static private void CreateImage1(Core.Real3DFunction function, Point from, Point to, double zoomCoef, double accuracy,
            Image image, Pen topPen, Pen bottomPen, int windowSize)
		{
            var coordConverter = new CoordConverter(windowSize / 2, windowSize / 2);
			var topHorizon = new int[windowSize];
			var bottomHorizon = new int[windowSize];
			for (var i = 0; i < windowSize; i++)
			{
				topHorizon[i] = int.MinValue;
				bottomHorizon[i] = int.MaxValue;
			}

			foreach (var x in Core.Range(to.X, from.X, accuracy))
			{
				var z = function(x, from.Y);
				var lastPoint = coordConverter.ToPlaneCoord(x * zoomCoef, from.Y * zoomCoef, z * zoomCoef);
                foreach (var y in Core.Range(from.Y, to.Y, accuracy / 100))
				{
					z = function(x, y);
					var zoomedX = x * zoomCoef;
					var zoomedY = y * zoomCoef;
					var zoomedZ = z * zoomCoef;
					var planePoint = coordConverter.ToPlaneCoord(zoomedX, zoomedY, zoomedZ);

					if (planePoint.X < 0 || planePoint.X >= windowSize || planePoint.Y < 0 || planePoint.Y >= windowSize)
						continue;
					if (lastPoint.X == planePoint.X)
						continue;
                    if (lastPoint.X < 0 || lastPoint.X >= windowSize || lastPoint.Y < 0 || lastPoint.Y >= windowSize)
                        lastPoint = planePoint;

					if (planePoint.Y >= topHorizon[planePoint.X])
					{
                        topHorizon[planePoint.X] = planePoint.Y;
                        //Graphics.FromImage(image).DrawLine(topPen, lastPoint.X, lastPoint.Y, planePoint.X, planePoint.Y);
                        Core.DrawLineWithBresenham(lastPoint, planePoint, (Bitmap)image, topPen);
					}

					if (planePoint.Y <= bottomHorizon[planePoint.X])
					{
                        bottomHorizon[planePoint.X] = planePoint.Y;
                        //Graphics.FromImage(image).DrawLine(bottomPen, lastPoint.X, lastPoint.Y, planePoint.X, planePoint.Y);
                        Core.DrawLineWithBresenham(lastPoint, planePoint, (Bitmap)image, bottomPen);
					}
					lastPoint = planePoint;
				}
			}
		}

        static private void CreateImage2(Core.Real3DFunction function, Point from, Point to, double zoomCoef, double accuracy,
            Image image, Pen topPen, Pen bottomPen, int windowSize)
		{
            var coordConverter = new CoordConverter(windowSize / 2, windowSize / 2);
			var topHorizon = new int[windowSize];
			var bottomHorizon = new int[windowSize];

			for (var i = 0; i < windowSize; i++)
			{
				topHorizon[i] = int.MinValue;
				bottomHorizon[i] = int.MaxValue;
			}

            foreach (var y in Core.Range(to.Y, from.Y, accuracy))
			{
				var z = function(from.X, y);
				var lastPoint = coordConverter.ToPlaneCoord(from.X * zoomCoef, y * zoomCoef, z * zoomCoef);
                foreach (var x in Core.Range(from.X, to.X, accuracy / 100))
				{
					z = function(x, y);
					var zoomedX = x * zoomCoef;
					var zoomedY = y * zoomCoef;
					var zoomedZ = z * zoomCoef;
					var planePoint = coordConverter.ToPlaneCoord(zoomedX, zoomedY, zoomedZ);

                    if (planePoint.X < 0 || planePoint.X >= windowSize || planePoint.Y < 0 || planePoint.Y >= windowSize)
                        continue;
                    if (lastPoint.X == planePoint.X)
                        continue;
                    if (lastPoint.X < 0 || lastPoint.X >= windowSize || lastPoint.Y < 0 || lastPoint.Y >= windowSize)
                        lastPoint = planePoint;

					if (planePoint.Y >= topHorizon[planePoint.X])
					{
						topHorizon[planePoint.X] = planePoint.Y;
						//Graphics.FromImage(image).DrawLine(topPen, lastPoint.X, lastPoint.Y, planePoint.X, planePoint.Y);
                        Core.DrawLineWithBresenham(lastPoint, planePoint, (Bitmap)image, topPen);
					}

					if (planePoint.Y <= bottomHorizon[planePoint.X])
					{
						bottomHorizon[planePoint.X] = planePoint.Y;
                        //Graphics.FromImage(image).DrawLine(bottomPen, lastPoint.X, lastPoint.Y, planePoint.X, planePoint.Y);
                        Core.DrawLineWithBresenham(lastPoint, planePoint, (Bitmap)image, bottomPen);
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
            if ((windowSize.X * windowSize.Y == 0) || ((windowSize.X ^ windowSize.Y) != 0))
                throw new ArgumentException("Неверные параметры!" +
                                            "Измерения windowSize не должны быть нулевыми и совпадать.");

            var image = new Bitmap(windowSize.X, windowSize.Y);
            (Graphics.FromImage(image))
                .FillRectangle(Brushes.White, 0, 0, image.Width, image.Height);

            var accuracy = Math.Min((double)(to.X - from.X) / 100, (double)(to.Y - from.Y) / 100);
            var zoomCoef = GetZoomCoeff(z, from, to, accuracy, windowSize.X);

            CreateImage1(z, from, to, zoomCoef, accuracy, image, topPen, bottomPen, windowSize.X);
            CreateImage2(z, from, to, zoomCoef, accuracy, image, topPen, bottomPen, windowSize.X);
            Core.ShowImageInWindow(image);
        }

        private static readonly Core.Real3DFunction[] Functions =
        {
            (x, y) => x*Math.Sin((x+y)),
            
            (x, y) => x*x-y*y+2,
            (x, y) => (x),
            (x, y) => (x + y),
            (x, y) => x*y,
            (x, y) => x*Math.Sin(x*y),
            (x, y) => Math.Sin(y * Math.Cos(x)),
            (x, y) => Math.Sin(x*x + y*y) * Math.Cos(x - y),
//            (x, y) => ,
//            (x, y) => ,
//            (x, y) => ,
//            (x, y) => ,

        };

        public static void Main(string[] args)
        {
            foreach (var function in Functions)
                Draw3DFunction(function, new Point(-5, -5), new Point(5, 5), new Point(700, 700));
        }
    }
}
