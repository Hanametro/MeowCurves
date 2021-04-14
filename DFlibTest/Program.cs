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
            List<double> Y3 = new();
            k.ForEach(x => X.Add(x));
            k.ForEach(x => Y3.Add(Math.Pow(x,2)));


            new Charts2D()
            {
                Width = 500,
                Height = 500,
            }
            .InitGraph()
                .InsertCurve(new(X.ToArray(), Y3.ToArray(), Color.Black, 2, true))
                    .Plot().ShowBitmapAsBase64();
            //在屏幕中写出图片的base64,是sin和cos的图像
        }
    }
}
