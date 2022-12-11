using OpenCvSharp;

namespace Camera.Calibration.Model
{
    public class CalibrationResult
    {
        public double Ret { get; set; }

        public double[,] CameraMatrix { get; set; }

        public double[] DistCoeffs { get; set; }

        public Vec3d[] RVecs { get; set; }

        public Vec3d[] TVecs { get; set; }

    }
}
