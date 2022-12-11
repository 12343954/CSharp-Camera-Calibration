using System.Text;

namespace Camera.Calibration.Extensions
{
    public static class MatrixExtenstions
    {
        public static string ToStringMatrix<T>(this T[,] matrix, string delimiter = "\t")
        {
            var s = new StringBuilder("");
            s.Append("[");

            for (var i = 0; i < matrix.GetLength(0); i++)
            {
                s.Append("[");
                for (var j = 0; j < matrix.GetLength(1); j++)
                {
                    s.Append(matrix[i, j]).Append(", ").Append(delimiter);
                }

                s.AppendLine("],");
            }
            s.AppendLine("]");

            return s.ToString();
        }

        /// <summary>
        /// OpenCvSharp.Point to string
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static string ToStringPoint(this OpenCvSharp.Point point)
        {
            return $"[{point.X}, {point.Y}]";
        }

        /// <summary>
        /// OpenCvSharp.LineSegmentPoint to string
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static string ToString(this OpenCvSharp.LineSegmentPoint line)
        {
            return $"[[{line.P1.ToStringPoint()}], [{line.P2.ToStringPoint()}]]";
        }

        public static string ToString2(this System.Drawing.Point point)
        {
            return $"[{point.X}, {point.Y}]";
        }
    }
}
