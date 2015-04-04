using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CGG;

namespace Task2
{
    //    Task2: нарисовать с помощью алгоритма Брезенхема
    //           параметрическую функцию
    //     / x = at^2 + bt
    //     \ y = ct + d

    struct Params
    {
        public double A { get; set; }
        public double B { get; set; }
        public double C { get; set; }
        public double D { get; set; }
    }

    class Program
    {
        private static void DrawParametricFunctionWithParams(Params arg)
        {
            DrawParametricFunctionWithParams(arg, Core.DefaultPen, Core.DefaultAxisPen, Core.DefaultWindowSize);
        }

        private static void DrawParametricFunctionWithParams(Params arg, Pen pen, Pen axisPen, Point windowSize)
        {
            double a = arg.A, b = arg.B, c = arg.C, d = arg.D;

            if (windowSize.X * windowSize.Y == 0)
            {
                throw new ArgumentException("Неверные параметры! xFrom не должно быть больше xTo," +
                                            "измерения windowSize не должны быть нулевыми.");
            }

            var image = new Bitmap(windowSize.X, windowSize.Y);
            var g = Graphics.FromImage(image);
            g.FillRectangle(Brushes.WhiteSmoke, 0, 0, windowSize.X, windowSize.Y); // Brushes.BlanchedAlmond

            Point pFrom, pTo;
            double xFrom, xTo;

            if (Math.Abs(c) < Core.Eps)      // x = at^2 + bt
            {                                // y = d
                if (Math.Abs(a) < Core.Eps)
                {
                    if (Math.Abs(b) < Core.Eps)
                    {
                        // При a = 0, b = 0 график - одна точка (0; d). [по центру]
                        pFrom = new Point(windowSize.X/2, windowSize.Y/2);
                        pTo = new Point(windowSize.X/2, windowSize.Y/2);
                        xFrom = xTo = 0.0;
                    }
                    else
                    {
                        // При a = 0, b ≠ 0 {(x, y) | x = bt, y = d, где b ≠ 0, t <- R },
                        // т.е. прямая y = d [По центру, от левого до правого края экрана]
                        pFrom = new Point(-10, windowSize.Y/2);
                        pTo = new Point(windowSize.X + 10, windowSize.Y/2);
                        xFrom = -100; xTo = 100;
                    }
                }
                else
                {
                    // При a ≠ 0 имеем {(x, y) | x = at^2+bt, y = d, где t <- R }, т.е. прямая y = d 
                    // При a > 0, x=x(t) - парабола, ветви вверх
                    // При a < 0, x=x(t) - парабола, ветви вниз
                    // Точка экстремума - вершина параболы (-b/2a; -b*b/4a)
                    // Т.е. у нас луч из точки (-b*b/4a; d)
                    if (a > 0) {
                        pFrom = new Point(0, windowSize.Y/2);
                        pTo = new Point(windowSize.X, windowSize.Y/2);
                        xFrom = -b*b / (4.0*a); xTo = 10;
                    } else {
                        pFrom = new Point(windowSize.X, windowSize.Y/2);
                        pTo = new Point(0, windowSize.Y/2);
                        xFrom = -10; xTo = (-b*b / (4.0*a));
                    }
                }

                Core.DrawAxis(g, axisPen, windowSize, xFrom, xTo, d, d);
                pen.Width = 4;
                g.DrawEllipse(pen, pFrom.X - 2, pFrom.Y - 2, 4, 4);
                pen.Width = 1;
                g.DrawLine(pen, pFrom, pTo);
                Core.ShowImageInWindow(image);
                return;
            }
        // Далее c ≠ 0

            if (Math.Abs(a) < Core.Eps)                 // x = (b/c)*y - (bd/c)
            {
                if (Math.Abs(b) < Core.Eps)             // x = 0
                {
                    pFrom = new Point(windowSize.X/2, 0);
                    pTo = new Point(windowSize.X/2, windowSize.Y);
                    Core.DrawAxis(g, axisPen, windowSize, 0.0, 0.0, 0.0, 0.0);
                    g.DrawLine(pen, pFrom, pTo);
                    Core.ShowImageInWindow(image);
                }
                else                                    // y = (c/b)*x + d
                {
                    Core.DrawFunction((x => (c/b)*x + d), -10.0, 10.0, windowSize);
                }
                return;
            }
        // Далее a ≠ 0

            // x = (a/c^2)*(y^2)  +  (b/c - 2ad/c^2)*(y)  +  (ad^2/c^2 - bd/c)
            //
            // D = B*B - 4*A*C = ... = (b/c)^2 >= 0
            // Вершина: y0 = -B/(2A) = d - bc/(2a)
            //          x0 = -D/(4A) = -b*b/(4a)
            // Канонический вид:
            // (y - ( d - bc/(2a) ))^2   =   2 * (c*c/(2a)) * (x + b*b/(4a))
            // То есть:
            //     y0 = d - bc / (2a)
            //     p  =    c*c / (2a)
            //     x0 =   -b*b / (4a)

            Core.DrawFunction((y => (a/(c*c)) * (y - (d - (b*c)/(2*a))) * (y - (d - (b*c)/(2*a))) + (-b*b)/(4*a)), -10, 10);

            double x0 = -b*b/(4*a),
                   y0 = d - b*c/(2*a),
                    p = c*c/(2*a);
            xFrom = -10; xTo = 10;
            int screenX = 100,
                //(int) ((x0 - xFrom)*windowSize.X/(xTo - xFrom)),
                screenY = 0;//300; //(int) ((x0 - xFrom)*windowSize.X/(xTo - xFrom));

            DrawParabolaWithBresenham(x0, y0, p, screenX, screenY, g, pen, axisPen, windowSize);
            Core.ShowImageInWindow(image);
        }

        private static void DrawPixel(Graphics g, Pen pen, int x, int y)
        {
            pen.Width = 4;
            g.DrawEllipse(pen, x - 2, y - 2, 4, 4);
            pen.Width = 1;
        }

        private static void DrawParabolaWithBresenham(double y0, double x0, double p, int x, int y, Graphics g, Pen pen, Pen axisPen, Point windowSize)
        {
            int maxX = windowSize.X;

            // Расстояния от трёх возможных точек до параболы:
            double Sd = ((y + 1)*(y + 1)) - 2*p*(x + 1),   // Диагональ
                   Sv = ((y + 1)*(y + 1)) - 2*p*x,         // Вертикаль
                   Sh = (y*y) - 2*p*(x + 1);               // Горизонталь
            //DrawPixel(g, pen, x0, y0);

            while (x + x0 < maxX) //пока полотно не кончится
            {
                if (Math.Abs(Sh) - Math.Abs(Sv) <= 0)
                {
                    if (Math.Abs(Sd) - Math.Abs(Sh) < 0)
                        y++;
                    x++;
                }
                else
                {
                    if (Math.Abs(Sv) - Math.Abs(Sd) > 0)
                        x++;
                    y++;
                }

                DrawPixel(g, pen, x + (int)x0, y + (int)y0);

                Sd = ((y + 1)*(y + 1)) - 2*p*(x + 1);
                Sv = ((y + 1)*(y + 1)) - 2*p*x;
                Sh = (y*y) - 2*p*(x + 1);
            }
            Core.DrawAxis(g, axisPen, windowSize, 0, 10, -10, 0);
        }

        private static readonly Params[] Tests =
        {
            // Вырожденные случаи C=0 (точка, луч, прямая)
//            new Params { A = 0, B = 0, C = 0, D = 0 },  // Одна точка 0;0 (рисуем по центру)
//            new Params { A = 0, B = 0, C = 0, D = 1 },  // Одна точка 0;1 (рисуем по центру)
//            new Params { A = 0, B = 7, C = 0, D = 1 },  // Прямая y = d
//            new Params { A = 1, B = 0, C = 0, D = 1 },  // Луч вправо из нуля
//            new Params { A = 1, B =-1, C = 0, D = 1 },  // Луч вправо из -1/4;d
//            new Params { A =-1, B =10, C = 0, D = 1 },  // Луч влево из 25;d

            // Вырожденные случаи A=0 (прямые)
//            new Params { A = 0, B = 0, C = 1, D =123},  // Прямая x = 0
//            new Params { A = 0, B = 1, C = 1, D = 1 },  // Прямая y =  x + 1
//            new Params { A = 0, B = 2, C = 6, D =-1 },  // Прямая y = 3x - 1

            // Параболы (A≠0, C≠0)
            new Params { A =-1, B = 2, C = 3, D = 4 },  // 
            new Params { A = 1, B =-2, C =-3, D =-4 },  // 
            new Params { A = 1, B = 10, C = 6, D = 30 },  // 
            new Params { A = 1, B = 10, C = 1, D = 5 },  // 
        };

        public static void Main(string[] args)
        {
            foreach (var param in Tests)
                DrawParametricFunctionWithParams(param);
        }
    }
}
