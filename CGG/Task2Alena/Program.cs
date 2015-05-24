using System;
using System.Drawing;
using CGG;
using Brush = System.Drawing.Brush;
using Brushes = System.Drawing.Brushes;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;
using Point = System.Drawing.Point;

namespace Task2Alena
{
    //    Task2: нарисовать с помощью алгоритма Брезенхема
    //           параметрическую функцию
    //    ax + by + c(x-y)^2 = 0
    struct Params
    {
        public double A { get; set; }
        public double B { get; set; }
        public double C { get; set; }
    }

    class Program
    {
        private static void DrawParametricFunctionWithParams(Params arg)
        {
            DrawParametricFunctionWithParams(arg, Core.DefaultPen, Core.DefaultAxisPen, Core.DefaultWindowSize);
        }
        private static void DrawParametricFunctionWithParams(Params arg, Pen pen, Pen axisPen, Point windowSize)
        {
            double a = arg.A, b = arg.B, c = arg.C;

            if (windowSize.X * windowSize.Y == 0)
            {
                throw new ArgumentException("Неверные параметры! xFrom не должно быть больше xTo," +
                                            "измерения windowSize не должны быть нулевыми.");
            }
            var image = new Bitmap(windowSize.X, windowSize.Y);

            Point pFrom, pTo;
            double xFrom, xTo;

            if (Math.Abs(c) < Core.Eps) // ax + by = 0
            {
                // Нарисуем оси с началом координат. Позже убедимся,
                // что в случае c = 0 это действительно так.
                Core.DrawAxisAtCenter(image, axisPen, windowSize);

                if (Math.Abs(a) < Core.Eps) // by = 0
                {
                    if (Math.Abs(b) < Core.Eps) // 0 = 0
                    {
                    // При a = 0, b = 0, c = 0 график - плоскость R2
                        Graphics g = Graphics.FromImage(image);
                        Brush brush = Brushes.DarkRed; // TODO <- DefaulPen.Color
                        g.FillRectangle(brush, 0, 0, windowSize.X, windowSize.Y);
                        //(Graphics.FromImage(image)).FillRectangle(Brushes.DarkRed, 0, 0, windowSize.X, windowSize.Y);
                    }
                    else
                    {
                    // При a = 0, b ≠ 0, c = 0 график - ось абсцисс y = 0
                    // [По центру, от левого до правого края экрана]
                        pFrom = new Point(0, windowSize.Y/2);
                        pTo = new Point(windowSize.X, windowSize.Y/2);
                        Core.DrawLineWithBresenham(pFrom, pTo, image, pen.Color);
                    }
                }
                else
                {
                    if (Math.Abs(b) < Core.Eps)
                    {
                    // При a ≠ 0, b = 0, c = 0 график - ось ординат x = 0
                    // [По центру, от верхнего до нижнего края экрана]
                        pFrom = new Point(windowSize.X/2, 0);
                        pTo = new Point(windowSize.X/2, windowSize.Y);
                        Core.DrawLineWithBresenham(pFrom, pTo, image, pen.Color);
                    }
                    else
                    {
                    // При a ≠ 0, b ≠ 0, c = 0 график - прямая y = (a/b)*x
                        Core.DrawFunction((x => (a/b)*x), -windowSize.X/2f, windowSize.X/2f, windowSize);
                        return;
                    }
                }
                Core.ShowImageInWindow(image);
                return;
            }
            // Далее везде c ≠ 0
            // Получили (2c)y^2 + y(sq(2)/2)(b-a) + x(sq(2)/2)(a+b) при повороте на pi/2

            if (Math.Abs(a + b) < Core.Eps)
            {
                // При c ≠ 0, a+b = 0 график - пара параллельных прямых y = x и y = x - (b-a/c)
                Core.DrawAxisAtCenter(image, axisPen, windowSize);

                // y = x
                pFrom = new Point(1, windowSize.Y-1);
                pTo = new Point(windowSize.X-1, 1);
                Core.DrawLineWithBresenham(pFrom, pTo, image, pen.Color);
                // y = x - (b-a)/c в зависимости от знака (b-a)/c нарисуем параллельную прямую ниже или выше y=x
                if (Math.Abs(b - a) > Core.Eps)
                {   // Если прямые не совпадают
                    if (b - a < Core.Eps)
                    {
                        pFrom = new Point(1, (int)(windowSize.Y * 9.0 / 10));
                        pTo = new Point((int)(windowSize.X * 9.0 / 10), 1);
                    }
                    else
                    {
                        pFrom = new Point((int)(windowSize.X * 1.0 / 10), windowSize.Y - 1);
                        pTo = new Point(windowSize.X - 1, (int)(windowSize.Y * 1.0 / 10));
                    }
                    Core.DrawLineWithBresenham(pFrom, pTo, image, pen.Color); 
                }

                Core.ShowImageInWindow(image);
                return;
            }
            // (2c)y^2 + y(sq(2)/2)(b-a) + x(sq(2)/2)(a+b) при повороте на pi/4
            // (y + (sq(2)/8)*(b-a)/c)^2 = 2 * (-(sq(2)/8)*(a+b)/c) * (x - (sq(2)/32)*((b-a)^2)/(b+a))
            // Вершина: y0 = -(sq(2)/8)*(b-a)/c
            //          x0 = (sq(2)/32)*((b-a)^2)/(b+a)
            // Параметр: p = -(sq(2)/8)*(a+b)/c
            // Сделаем перенос начала координат в точку (x0; y0), тогда для отрисовки необходим
            // фокальный параметр и оси координат в нужном месте.

            double x0 = (Math.Sqrt(2)/32)*(b - a)*(b - a)/(b + a),
                y0 = (Math.Sqrt(2)/8)*(b - a)/c,
                p = -(Math.Sqrt(2)/8)*(a + b)/c;
            Console.WriteLine("{0} {1}", x0, y0);
            Core.DrawAxisRotatedForNewCenter(image, axisPen, windowSize, x0, y0);
            Core.DrawParabolaWithBresenham(p: p, image: image, color: pen.Color, windowSize: windowSize);
            Core.ShowImageInWindow(image);
        }

        private static void DrawParabolaWithBresenham(double p, Bitmap image, Color color, Point windowSize)
        {
            int maxX = windowSize.X / 2, maxY = windowSize.Y / 2;       // Размеры области [Замечание: везде пользуемся половиной]
            int x = 0, y = 0;                                       // Перенос вершины (x0; y0) в (0; 0). Экранные=(maxX; maxY)
            var deltaX = (p < Core.Eps) ? -1 : 1;                   // Рост x в зависимости от направления ветвей параболы

            // Рисуем параболу с фок.параметром p в области [-maxX; maxX]x[-maxY; maxY]
            // Вершина по центру
            image.SetPixel(maxX, maxY, color);
            Func<int, int, int> accuracyOf = (x2, y2) => y2 * y2 - (int)(2 * p * x2);

            for (; ; )
            {                                              // Выход учтён выше :)
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
            // Вырожденные случаи C=0 (R2, прямые)
            new Params { A = 0,   B = 0,  C = 0 }, // Плоскость R2
            new Params { A = 0,   B = 1,  C = 0 }, // Ось абсцисс y = 0
            new Params { A = 1,   B = 0,  C = 0 }, // Ось ординат x = 0
                                                   // Прямые y = (a/b)*x
            new Params { A = 1,   B = 1,  C = 0 }, // y = x
            new Params { A = -1,  B = 1,  C = 0 }, // y = -x
            new Params { A = 10,  B = 1,  C = 0 }, // y = 10*x

//            // Вырожденные случаи A+B=0 (параллельные прямые)
              new Params { A = 0,     B = 0,  C = 1 },// Совпали y=x. y=x
              new Params { A =-1,     B = 1,  C = 1 },// Прямые y=x, y=x-1
              new Params { A = 1,     B =-1,  C = 6 },// Прямые y=x, y=x+1
//
//            // Параболы (A+B≠0, C≠0)
            new Params { A = 1,     B = 1,  C = 1 },
            new Params { A = 100,     B = 10, C =-1 },
            new Params { A =-100,     B = 10, C =-1 },
            new Params { A =-200,     B = 10, C =-10},
        };

        public static void Main(string[] args)
        {
            foreach (var param in Tests)
                DrawParametricFunctionWithParams(param);

            var r = new Random(0);
            while (true)
            {
                DrawParametricFunctionWithParams(new Params
                {
                    A = r.Next(-1000, 1000) / (double)(r.Next(10) + 1),
                    B = r.Next(-1000, 1000) / (double)(r.Next(10) + 1),
                    C = r.Next(-1000, 1000) / (double)(r.Next(10) + 1)
                });
            }
        }
    }
}
