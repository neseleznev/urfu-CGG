using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task2
{
    //    Task2: нарисовать с помощью алгоритма Брезенхема
    //           параметрическую функцию
    //     / y = at^2 + bt
    //     \ x = ctx + d

    struct Params
    {
        public double A { get; set; }
        public double B { get; set; }
        public double C { get; set; }
        public double D { get; set; }
    }

    class Program
    {

        private static void DrawParametricFunction(Params arg)
        {
            if (Math.Abs(arg.C) < CGG.Core.Eps)
            {
                
            }
        }


        private static readonly Params[] Tests =
        {
            new Params { A = 1, B = 2, C = 0, D = 3 },
            new Params { A = 5, B = 6, C = 0, D = 7 },
            new Params { A = 7, B = 8, C = 0, D = 9 },
        };

        public static void Main(string[] args)
        {
            foreach (var param in Tests)
                DrawParametricFunction(param);
        }
    }
}
