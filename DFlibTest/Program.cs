using MeowDFLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace DFlibTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var k = Enumerable.Range(0, 20).ToList();
            List<double> X = new();
            List<double> Y1 = new();
            List<double> Y2 = new();
            k.ForEach(x => X.Add(x));
            k.ForEach(x => Y1.Add(Math.Sin(x)));
            k.ForEach(x => Y2.Add(Math.Cos(x)));


            new Charts2D()
            {
                Width = 500,
                Height = 500,
            }
            .InitGraph()
                .InsertCurve(new(X.ToArray(), Y1.ToArray(), Color.Aqua, 2, true))
                .InsertCurve(new(X.ToArray(), Y2.ToArray(), Color.Bisque, 2, true))
                    .Plot().ShowBitmapAsBase64();
        }
    }
}
