using OpenCvSharp;
using System;
using System.Drawing;
using System.Timers;
using System.Windows.Forms;

namespace Camera.Calibration
{
    public static class GG
    {
        #region //Calibration
        public struct Calibration
        {
            public static string config_file = "CameraCalibration.json";
        }
        #endregion

        #region // convert api： ToBitmap(), ToMat()
        /// <summary>
        /// Mat to Image
        /// </summary>
        /// <param name="mat"></param>
        /// <returns></returns>
        public static Bitmap ToBitmap(this Mat mat)
        {
            try
            {
                if (mat == null || mat.Size().Width == 0)
                    return null;

                return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat);
            }
            catch (Exception)
            {
                return null;
                //throw;
            }


        }

        /// <summary>
        /// Image to Mat
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static Mat ToMat(this Image image)
        {
            return OpenCvSharp.Extensions.BitmapConverter.ToMat(new Bitmap(image));
        }
        /// <summary>
        /// Bitmap to Mat
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static Mat ToMat(this Bitmap image)
        {
            return OpenCvSharp.Extensions.BitmapConverter.ToMat(image);
        }

        /// <summary>
        /// Point2f to System.Drawing.Point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static System.Drawing.Point ToPoint(this Point2f point)
        {
            return new System.Drawing.Point { X = Convert.ToInt32(point.X), Y = Convert.ToInt32(point.Y) };
        }

        /// <summary>
        /// OpenCvSharp.Point to System.Drawing.Point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static System.Drawing.Point ToPoint(this OpenCvSharp.Point point)
        {
            return new System.Drawing.Point { X = Convert.ToInt32(point.X), Y = Convert.ToInt32(point.Y) };
        }
        #endregion

        #region //FormControl Extention methods
        public static void AppendingText(this RichTextBox box, string text, Color ForeColor, Color BackColor, Font font = null)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;
            //box.SelectionLength = text.Length;

            box.SelectionColor = ForeColor;
            box.SelectionFont = font;
            box.SelectionBackColor = BackColor;
            box.AppendText(text);

            box.SelectionBackColor = box.BackColor;
            box.SelectionColor = box.ForeColor;

        }
        #endregion

        #region //SetTimeout, SetInterval
        /// <summary> 
        /// 在指定时间过后执行指定的表达式 
        /// </summary> 
        /// <param name="interval">事件之间经过的时间（以毫秒为单位）</param> 
        /// <param name="action">要执行的表达式</param> 
        public static void SetTimeout(double interval, Action action)
        {
            System.Timers.Timer timer = new System.Timers.Timer(interval);
            timer.Elapsed += delegate (object sender, System.Timers.ElapsedEventArgs e)
            {
                timer.Enabled = false;
                action();
            };
            timer.Enabled = true;
        }
        /// <summary> 
        /// 在指定时间周期重复执行指定的表达式 
        /// </summary> 
        /// <param name="interval">事件之间经过的时间（以毫秒为单位）</param> 
        /// <param name="action">要执行的表达式</param> 
        public static void SetInterval(double interval, Action<ElapsedEventArgs> action)
        {
            System.Timers.Timer timer = new System.Timers.Timer(interval);
            timer.Elapsed += delegate (object sender, System.Timers.ElapsedEventArgs e)
            {
                action(e);
            };
            timer.Enabled = true;
        }
        #endregion
    }
}
