using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace MeowDFLib
{
    /// <summary>
    /// 所有图的继承对象
    /// </summary>
    public class Charts : IDisposable
    {
        /// <summary>
        /// 图宽度
        /// </summary>
        public int Width { get; set; } = 500;
        /// <summary>
        /// 图高度
        /// </summary>
        public int Height { get; set; } = 500;
        /// <summary>
        /// 安全边距
        /// </summary>
        public int SafePadding { get; set; } = 20;
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; } = "null";
        /// <summary>
        /// 轴
        /// </summary>
        public List<Axis> Axis { get; } = new();
        /// <summary>
        /// 数据集合
        /// </summary>
        public List<Curve> Curves { get; } = new();
        /// <summary>
        /// 背景色
        /// </summary>
        public Color bgcolor { get; set; } = Color.White;
        /// <summary>
        /// 图
        /// </summary>
        public Bitmap b { get; set; }
        /// <summary>
        /// 使用using的dispose信号量
        /// </summary>
        private bool disposedValue;

        public string ShowBitmapAsBase64()
        {
            var s = Base64.ImageToBase64(b);
            Console.WriteLine(s);
            return s;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    b.Dispose();
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
    /// <summary>
    /// 生成一个2D函数图
    /// </summary>
    public class Charts2D:Charts
    {
        /// <summary>
        /// 初始画布
        /// </summary>
        /// <param name="xmax">X轴最大值</param>
        /// <param name="xmin">X轴最小值</param>
        /// <param name="ymax">Y轴最大值</param>
        /// <param name="ymin">Y轴最小值</param>
        /// <param name="x_label">x的标题</param>
        /// <param name="y_label">y的标题</param>
        /// <param name="x_Color">x轴颜色</param>
        /// <param name="y_Color">y轴颜色</param>
        /// <returns></returns>
        public Charts2D InitGraph(
            double? xmax = null, double? xmin = null,
            double? ymax = null, double? ymin = null,
            string x_label = "X Axis", string y_label = "Y Axis",
            Color? x_Color = null, Color? y_Color = null
            )
        {
            //初始化位图
            b = new(Width, Height);
            //初始化画布
            using Graphics g = Graphics.FromImage(b);
            //设置背景色
            g.Clear(bgcolor);
            //设置x轴
            Axis.Add(new()
            {
                AxisLabel = x_label,
                AxisColor = x_Color ?? Color.Blue,
                MaxValue = xmax,
                MinValue = xmin,
            });
            //设置y轴
            Axis.Add(new()
            {
                AxisLabel = y_label,
                AxisColor = y_Color ?? Color.Blue,
                MaxValue = ymax,
                MinValue = ymin,
            });
            //返还实例
            return this;
        }
        /// <summary>
        /// 添加一个曲线
        /// </summary>
        /// <param name="c">曲线</param>
        /// <returns></returns>
        public Charts2D InsertCurve(Curve c)
        {
            Curves.Add(c);
            return this;
        }
        /// <summary>
        /// 构图
        /// </summary>
        /// <returns></returns>
        public Charts2D Plot()
        {
            //确定轴大小
            foreach (var d in Curves)
            {
                if (Axis[0].MaxValue == null)
                    Axis[0].MaxValue = (d.x.Max() > (Axis[0].MaxValue ?? 0) ? d.x.Max() : (Axis[0].MaxValue ?? 0));
                if (Axis[0].MinValue == null)
                    Axis[0].MinValue = (d.x.Min() < (Axis[0].MinValue ?? 0) ? d.x.Min() : (Axis[0].MinValue ?? 0));
                if (Axis[1].MaxValue == null)
                    Axis[1].MaxValue = (d.y.Max() > (Axis[1].MaxValue ?? 0) ? d.y.Max() : (Axis[1].MaxValue ?? 0));
                if (Axis[1].MinValue == null)
                    Axis[1].MinValue = (d.y.Min() < (Axis[1].MinValue ?? 0) ? d.y.Min() : (Axis[1].MinValue ?? 0));
            }
            //确定缩放比例
            var xScaletrim = (Width - SafePadding) / (Axis[0].MaxValue - Axis[0].MinValue);
            var yScaletrim = (Height - SafePadding) / (Axis[1].MaxValue - Axis[1].MinValue);
            foreach (var d in Curves)
            {
                //确定左右平移量
                for (int i = 0; i < d.x.Length; i++)
                {
                    var xx = (int)Math.Floor(d.x[i] * xScaletrim ?? 1);
                    var yy = (int)Math.Floor(d.y[i] * yScaletrim ?? 1);
                    Axis[0].StartPos = xx < Axis[0].StartPos ? xx : Axis[0].StartPos;
                    Axis[1].StartPos = yy < Axis[1].StartPos ? yy : Axis[1].StartPos;
                }
            }
            //确定起始位置
            var y0 = -(int)Axis[1].StartPos;
            var x0 = (int)Axis[0].StartPos;
            //初始化画板
            using Graphics g = Graphics.FromImage(b);
            //x轴
            g.DrawLine(new(Axis[0].AxisColor, 1), new(0, y0), new(Width, y0));
            //y轴
            g.DrawLine(new(Axis[1].AxisColor, 1), new(x0, 0), new(x0, Height));
            //逐一绘制曲线
            foreach (var d in Curves)
            {
                //构造点集
                List<Point> points = new();
                //循环遍历补足点集
                for (int i = 0; i < d.x.Length; i++)
                {
                    var xx = (int)Math.Floor(d.x[i] * xScaletrim ?? 1);
                    var yy = (int)Math.Floor(d.y[i] * yScaletrim ?? 1);
                    points.Add(new(xx + (int)Axis[0].StartPos,-(yy + (int)Axis[1].StartPos)));
                }
                //构图曲线
                g.DrawCurve(new(d.CurveColor ?? Color.Black, d.CurveBold), points.ToArray());
                //计算点厚度
                var dpointsize = d.CurveBold + 2;
                //构图点位置
                if (d.ShowPoint)
                {
                    foreach (var dd in points)
                    {
                        //画点
                        g.DrawLine(
                            new(d.PointColor ?? Color.Red, dpointsize*2),
                            new(dd.X - dpointsize / 2, dd.Y - dpointsize / 2),
                            new(dd.X + dpointsize / 2, dd.Y + dpointsize / 2)
                            );
                    }
                }
            }
            //返还实例
            return this;
        }
    }
    /// <summary>
    /// 数据集合
    /// </summary>
    public class Curve
    {
        /// <summary>
        /// 曲线X集合
        /// </summary>
        public double[] x { get; }
        /// <summary>
        /// 曲线y集合
        /// </summary>
        public double[] y { get; }
        /// <summary>
        /// 曲线颜色
        /// </summary>
        public Color? CurveColor { get; } = null;
        /// <summary>
        /// 曲线粗细
        /// </summary>
        public int CurveBold { get; } = 3;
        /// <summary>
        /// 是否显示点
        /// </summary>
        public bool ShowPoint { get; } = false;
        /// <summary>
        /// 点颜色
        /// </summary>
        public Color? PointColor { get; } = null;

        /// <summary>
        /// 生成一个曲线
        /// </summary>
        /// <param name="x">x集合</param>
        /// <param name="y">y集合</param>
        /// <param name="curveColor">曲线颜色</param>
        /// <param name="curveBold">曲线粗细</param>
        /// <param name="showPoint">是否显示点</param>
        /// <param name="pointColor">点颜色</param>
        public Curve(
            double[] x, double[] y,
            Color? curveColor = null, int curveBold = 0, 
            bool showPoint = false, Color? pointColor = null
            )
        {
            this.x = x;
            this.y = y;
            CurveColor = curveColor;
            CurveBold = curveBold;
            ShowPoint = showPoint;
            PointColor = pointColor;
        }
    }
    /// <summary>
    /// 轴
    /// </summary>
    public class Axis
    {
        /// <summary>
        /// 轴标记
        /// </summary>
        public string AxisLabel;
        /// <summary>
        /// 最小刻度标
        /// </summary>
        public double MinorScale;
        /// <summary>
        /// 起始位置
        /// </summary>
        public double StartPos = 0;
        /// <summary>
        /// 最大值
        /// </summary>
        public double? MaxValue;
        /// <summary>
        /// 最小值
        /// </summary>
        public double? MinValue;
        /// <summary>
        /// 轴颜色
        /// </summary>
        public Color AxisColor = Color.Black;
    }
    /// <summary>
    /// Base64 tools
    /// </summary>
    public class Base64
    {
        /// <summary>
        /// Base64加密，采用utf8编码方式加密
        /// </summary>
        /// <param name="source">待加密的明文</param>
        /// <returns>加密后的字符串</returns>
        public static string Base64Encode(string source)
        {
            return Base64Encode(Encoding.UTF8, source);
        }
        /// <summary>
        /// Base64加密
        /// </summary>
        /// <param name="encodeType">加密采用的编码方式</param>
        /// <param name="source">待加密的明文</param>
        /// <returns></returns>
        public static string Base64Encode(Encoding encodeType, string source)
        {
            byte[] bytes = encodeType.GetBytes(source);
            string encode;
            try
            {
                encode = Convert.ToBase64String(bytes);
            }
            catch
            {
                encode = source;
            }
            return encode;
        }
        /// <summary>
        /// Base64解密，采用utf8编码方式解密
        /// </summary>
        /// <param name="result">待解密的密文</param>
        /// <returns>解密后的字符串</returns>
        public static string Base64Decode(string result) => Base64Decode(Encoding.UTF8, result);
        /// <summary>
        /// Base64解密
        /// </summary>
        /// <param name="encodeType">解密采用的编码方式，注意和加密时采用的方式一致</param>
        /// <param name="result">待解密的密文</param>
        /// <returns>解密后的字符串</returns>
        public static string Base64Decode(Encoding encodeType, string result)
        {
            byte[] bytes = Convert.FromBase64String(result);
            string decode;
            try
            {
                decode = encodeType.GetString(bytes);
            }
            catch
            {
                decode = result;
            }
            return decode;
        }
        /// <summary>
        /// Image 转成 base64
        /// </summary>
        /// <param name="fileFullName"></param>
        public static string ImageToBase64(string fileFullName)
        {
            try
            {
                Bitmap bmp = new Bitmap(fileFullName);
                MemoryStream ms = new MemoryStream();
                var suffix = fileFullName.Substring(fileFullName.LastIndexOf('.') + 1,
                    fileFullName.Length - fileFullName.LastIndexOf('.') - 1).ToLower();
                var suffixName = suffix == "png"
                    ? ImageFormat.Png
                    : suffix == "jpg" || suffix == "jpeg"
                        ? ImageFormat.Jpeg
                        : suffix == "bmp"
                            ? ImageFormat.Bmp
                            : suffix == "gif"
                                ? ImageFormat.Gif
                                : ImageFormat.Jpeg;

                bmp.Save(ms, suffixName);
                byte[] arr = new byte[ms.Length]; ms.Position = 0;
                ms.Read(arr, 0, (int)ms.Length); ms.Close();
                bmp.Dispose();
                return Convert.ToBase64String(arr);
            }
            catch (Exception)
            {
                return null;
            }

        }
        /// <summary>
        /// bitmap转换Base64 
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static string ImageToBase64(Bitmap bmp)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                bmp.Save(ms, ImageFormat.Jpeg);
                byte[] arr = new byte[ms.Length]; ms.Position = 0;
                ms.Read(arr, 0, (int)ms.Length); ms.Close();
                bmp.Dispose();
                return Convert.ToBase64String(arr);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
