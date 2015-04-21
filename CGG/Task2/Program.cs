using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net.Mime;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CGG;
using Brushes = System.Drawing.Brushes;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;
using Point = System.Drawing.Point;

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
            
            if (windowSize.X * windowSize.Y == 0) {
                throw new ArgumentException("Неверные параметры! xFrom не должно быть больше xTo," +
                                            "измерения windowSize не должны быть нулевыми.");
            }
            var image = new Bitmap(windowSize.X, windowSize.Y);

            Point pFrom, pTo;
            double xFrom, xTo;

            if (Math.Abs(c) < Core.Eps)      // x = at^2 + bt
            {                                // y = d
                if (Math.Abs(a) < Core.Eps)
                {
                    if (Math.Abs(b) < Core.Eps)
                    {
                        // При a = 0, b = 0, c = 0 график - одна точка (d; 0). [по центру]
                        pFrom = new Point(windowSize.X/2, windowSize.Y/2);
                        pTo = new Point(windowSize.X/2, windowSize.Y/2);
                    }
                    else
                    {
                        // При a = 0, b ≠ 0, c = 0 {(x, y) | y = bt, x = d, где b ≠ 0, t <- R },
                        // т.е. прямая x = d [По центру, от левого до правого края экрана]
                        pFrom = new Point(0, windowSize.Y/2);
                        pTo = new Point(windowSize.X, windowSize.Y/2);
                    }
                    Core.DrawAxisForNewCenter(image, axisPen, windowSize, d, 0);
                }
                else
                {
                    // При a ≠ 0, c = 0 имеем {(x, y) | x = at^2+bt, y = d, где t <- R }, т.е. часть прямой y = d 
                    // При a > 0, x=x(t) - парабола, ветви вверх
                    // При a < 0, x=x(t) - парабола, ветви вниз
                    // Точка экстремума - вершина параболы (-b/2a; -b*b/4a)
                    // Т.е. у нас луч из точки (-b*b/4a; d)
                    Core.DrawAxisForNewCenter(image, axisPen, windowSize, -b * b / (4 * a), d);
                    pFrom = new Point(windowSize.X/2, windowSize.Y/2);
                    pTo = new Point((a > 0) ? windowSize.X : 0, windowSize.Y/2);
                }

                if (Math.Abs(windowSize.X/2 - pFrom.X) < windowSize.X/2 &&
                    Math.Abs(windowSize.Y/2 - pFrom.Y) < windowSize.Y/2)
                {
                    pen.Width = 4;
                    var g = Graphics.FromImage(image);
                    g.DrawEllipse(pen, pFrom.X - 2, pFrom.Y - 2, 4, 4);
                    pen.Width = 1;
                }
                Core.DrawLineWithBresenham(pFrom, pTo, image, pen);
                Core.ShowImageInWindow(image);
                return;
            }
    // Далее c ≠ 0

            if (Math.Abs(a) < Core.Eps)                 // x = (b/c)*(y - d)
            {
                if (Math.Abs(b) < Core.Eps)             // x = 0
                {
                    Core.DrawAxisAtCenter(image, axisPen, windowSize);
                    Core.DrawLineWithBresenham(new Point(windowSize.X/2, 1), new Point(windowSize.X/2, windowSize.Y-1),
                        image, pen.Color);
                    Core.ShowImageInWindow(image);
                }
                else                                    // y = (c/b)*x + d
                {
                    Core.DrawFunction((x => (c / b) * x + d), -windowSize.X/2f, windowSize.X/2f, windowSize);
                }
                return;
            }
    // Далее a ≠ 0

            // x =   A    * y^2  +         B      * y  +         C
            // x = (a/c^2)*(y^2) + (b/c - 2ad/c^2)*(y) + (ad^2/c^2 - bd/c)
            //
            // D = B*B - 4*A*C = ... = (b/c)^2 >= 0
            // Вершина: y0 = -B/(2A) = d - bc/(2a)
            //          x0 = -D/(4A) = -b*b/(4a)
            // Параметр: p =  1/|2a| = c*c/(2a)
            // Канонический вид:
            // (y -      y0    )^2 = 2 *     p      * (x -    x0   )
            // (y - (d-bc/(2a)))^2 = 2 * (c*c/(2a)) * (x + b*b/(4a))
            // То есть:
            //     x0 =   -b*b / (4a)
            //     y0 = d - bc / (2a)
            //     p  =    c*c / (2a)
            // Сделаем перенос начала координат в точку (x0; y0), тогда для отрисовки необходим
            // фокальный параметр и оси координат в нужном месте.

            double x0 = b*b / (-4*a),
                   y0 = d - b*c / (2*a),
                    p = c*c / (2*a);
            Console.WriteLine("{0} {1}", x0, y0);
            Core.DrawAxisForNewCenter(image, axisPen, windowSize, x0, y0);
            Core.DrawParabolaWithBresenham(p: p, image: image, color: pen.Color, windowSize: windowSize);
            Core.ShowImageInWindow(image);
        }

        private static void DrawParabolaWithBresenham(double p, Bitmap image, Color color, Point windowSize)
        {
            int maxX = windowSize.X/2, maxY = windowSize.Y/2;       // Размеры области [Замечание: везде пользуемся половиной]
            int x = 0, y = 0;                                       // Перенос вершины (x0; y0) в (0; 0). Экранные=(maxX; maxY)
            var deltaX = (p < Core.Eps) ? -1 : 1;                   // Рост x в зависимости от направления ветвей параболы

        // Рисуем параболу с фок.параметром p в области [-maxX; maxX]x[-maxY; maxY]
        // Вершина по центру
            image.SetPixel(maxX, maxY, color);
            Func<int, int, int> accuracyOf = (x2, y2) => y2 * y2 - (int)(2 * p * x2);

            for (;;) {                                              // Выход учтён выше :)
            // Расстояния от трёх возможных точек до параболы:
                int dDiag = accuracyOf(x + deltaX, y + 1),
                    dVert = accuracyOf(x, y + 1),
                    dHori = accuracyOf(x + deltaX, y);
                if (Math.Abs(dHori) <= Math.Abs(dVert))
                {
                    if (Math.Abs(dDiag) < Math.Abs(dHori))
                        y++;
                    x += deltaX;
                }
                else
                {
                    if (Math.Abs(dDiag) < Math.Abs(dVert))
                        x += deltaX;
                    y++;
                }
//          Точка, сдвинутая к центру (x0; y0), масштабированная в интервале [-width/2; width/2]
//              int screenX = (int)(((x+x0) - (x0 -width/2f)) * width  / ((x0+ width/2f) - (x0- width/2f))),
//                  screenY = (int)(((y0+height/2f) - (y+y0)) * height / ((y0+height/2f) - (y0-height/2f)));
//          Упростив, получаем:
//              int screenX = x + width/2,
//                  screenY = height/2 - y;
//          Рисуем только {scrX, scrY}, которые принадлежат (0, width)x(0, height).
                if (Math.Abs(x) >= maxX || Math.Abs(y) >= maxY) break;// Полотно кончилось
                image.SetPixel(x + maxX, maxY - y, color);           // "Верхняя" ветвь (y++)
                image.SetPixel(x + maxX, maxY + y, color);           // "Нижняя"
            }
        }

        private static readonly Params[] Tests =
        {
            
            new Params { A = -1, B = -30, C = 1,  D = 10  },
            // Вырожденные случаи C=0 (точка, луч, прямая)
            new Params { A = 0,     B = 0,  C = 0,  D = 0 },  // Одна точка (0; 0) (рисуем по центру)
            new Params { A = 0,     B = 0,  C = 0,  D = 100 },// Одна точка (0; 100) (рисуем по центру)
            new Params { A = 0,     B = 7,  C = 0,  D = 1 },  // Прямая y = d
            new Params { A = 1,     B = 0,  C = 0,  D = 0 },  // Луч вправо из нуля
            new Params { A = 1,     B = 10, C = 0,  D = 100 },// Луч вправо из (-b*b/4a; d)
            new Params { A =-1,     B = 10, C = 0,  D = 100 },// Луч влево  из (-b*b/4a; d)

            // Вырожденные случаи A=0 (прямые)
            new Params { A = 0,     B = 0,  C = 1,  D = 123 },// Прямая x = 0
            new Params { A = 0,     B = 1,  C = 1,  D = 100 },// Прямая y =  x + 100
            new Params { A = 0,     B = 2,  C = 6,  D =-100 },// Прямая y = 3x - 100

            // Параболы (A≠0, C≠0)
            new Params { A =-1,     B = -2, C = 3,  D = 200 },// Влево, с центром ( 1; 197)
            new Params { A = 1/7f,  B =-2,  C =-3,  D =-40  },// Вправо,с центром (-7; -61)
            new Params { A = 1/2f,  B = 10, C = 6,  D = 300 },// Вправо,с центром (50; 240)
            new Params { A = 1/10f, B = 10, C = 1,  D = 50  },// Вправо,с центром ( 0; 250)
            new Params { A =-1/7f,  B =-2,  C =-3,  D =-40  },// Влево, с центром ( 7; -19)
            new Params { A =-1/2f,  B = 10, C = 6,  D = 300 },// Влево, с центром (50; 360)
            new Params { A =-1/10f, B = 10, C = 1,  D = 50  },// Влево, с центром (250;100)
        };

        public static void Main(string[] args)
        {
            foreach (var param in Tests)
                DrawParametricFunctionWithParams(param);
            var r = new Random(0);
            while (true)
            {
                double a = r.Next(-100, 100) / (double)(r.Next(10) + 1),
                       b = r.Next(-100, 100) / (double)(r.Next(10) + 1),
                       c = r.Next(-100, 100) / (double)(r.Next(10) + 1),
                       d = r.Next(-100, 100) / (double)(r.Next(10) + 1);
                DrawParametricFunctionWithParams(new Params {A = a, B = b, C = c, D = d});
            }
        }
    }
}
