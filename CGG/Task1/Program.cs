using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using CGG;

namespace Task1
{
    //    I. Нарисовать f(x) на интервале [a; b].
    //       Отрисовывать оси только при необходимости.
    //       Масштабировать график.

    public class Program
    {
        private static readonly Core.RealFunction[] Functions =
        {
            x => 1/x,
            x => 1,
            x => 0,
            x => (1/x)*Math.Sin(1/x),
            x => x*Math.Cos(x*x),
            x => Math.Cos(1/x),
            x => Math.Sqrt(x),
            x => x*x*x,
            x => x*x*x*x*x,
        };

        public static void Main(string[] args)
        {
            foreach (var function in Functions)
                Core.DrawFunction(function, -3.0, 3.0);
        }
    }
}
