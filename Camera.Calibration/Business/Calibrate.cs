using Camera.Calibration.Extensions;
using Camera.Calibration.Model;
using Newtonsoft.Json;
using NumSharp;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Camera.Calibration.Business
{
    /// <summary>
    /// Camera and Image Distortion calibration via OpencvSharp & NumSharp
    /// </summary>
    public class Calibrate
    {
        //# Set the parameters for finding sub-pixel corner points, and the stop criterion
        //# adopted is the maximum number of cycles of 30
        //# and the maximum error tolerance of 0.001
        // PYTHON:  criteria = (cv2.TERM_CRITERIA_MAX_ITER | cv2.TERM_CRITERIA_EPS, 30, 0.001)
        TermCriteria criteria = new TermCriteria(CriteriaTypes.MaxIter | CriteriaTypes.Eps, 30, 0.001);

        //# number of inner corner points, not the number of black and white boxes
        int PAT_ROW = 9, PAT_COL = 6;

        int PAT_SIZE;

        /// <summary> 
        /// PAT_ROW x PAT_COL
        /// </summary>
        OpenCvSharp.Size PatternSize;

        List<CalibrateImage> CelibrateImages;

        /// <summary>
        /// 
        /// </summary>
        public double Ret { get; set; }

        /// <summary>
        //  Output 3x3 floating-point camera matrix. If CV_CALIB_USE_INTRINSIC_GUESS and/or
        //  CV_CALIB_FIX_ASPECT_RATIO are specified, some or all of fx, fy, cx, cy must be
        //  initialized before calling the function. 
        /// </summary>
        public double[,] CameraMatrix { get; set; }

        /// <summary>
        //  Output vector of distortion coefficients (k_1, k_2, p_1, p_2[, k_3[, k_4, k_5,k_6]]) of 4, 5, or 8 elements. 
        /// </summary>
        public double[] DistCoeffs { get; set; }

        /// <summary>
        //  Output vector of rotation vectors (see Rodrigues() ) estimated for each pattern
        //  view. That is, each k-th rotation vector together with the corresponding k-th
        //  translation vector (see the next output parameter description) brings the calibration
        //  pattern from the model coordinate space (in which object points are specified)
        //  to the world coordinate space, that is, a real position of the calibration pattern
        //  in the k-th pattern view (k=0.. M -1) 
        /// </summary>
        public Vec3d[] RVecs { get; set; }

        /// <summary>
        /// Output vector of translation vectors estimated for each pattern view.
        /// </summary>
        public Vec3d[] TVecs { get; set; }

        public double TotalError { get; set; }
        public double MeanError { get; set; }


        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="images"></param>
        public Calibrate(List<CalibrateImage> images)
        {
            PAT_SIZE = PAT_ROW * PAT_COL;

            PatternSize = new OpenCvSharp.Size(PAT_COL, PAT_ROW);

            CelibrateImages = images;

        }

        /// <summary>
        /// Calibration Camera via images
        /// </summary>
        /// <returns>
        /// </returns>
        public bool CalibrateCamera()
        {
            if (CelibrateImages?.Count() == 0) return false;

            // PYTHON:  objp = np.zeros((ROW * COL, 3), np.float32)
            var objp = np.zeros((PAT_SIZE, 3), np.float32); // using numpysharp

            // create a np.array [9x6,3], 
            // python result is not suitable for C#
            // PYTHON:  objp[:, :2] = np.mgrid[0:ROW, 0:COL].T.reshape(-1, 2)

            // C#
            //(var v1, var v2) = np.mgrid(np.arange(0, PAT_COL), np.arange(0, PAT_ROW));
            //var v3 = np.dstack(v2, v1); //Deep Combination
            //objp[$":,:2"] = v3.reshape(-1, 2);

            // C#
            (var v1, var v2) = np.mgrid(np.arange(0, PAT_ROW), np.arange(0, PAT_COL));
            var v3 = np.dstack(v1, v2); //Deep Combination
            objp[$":,:2"] = v3.reshape(-1, 2);

            // NumSharp can not use in OpencvSharp, so need to convert
            var objpl = NDArrayToList(objp);

            // PYTHON: 
            // # Arrays to store object points and image points from all the images.
            // objpoints = [] # 3d points
            // imgpoints = [] # 2d points

            // C#
            List<List<Point3f>> obj_points = new List<List<Point3f>>(); // # 3d points
            List<List<Point2f>> img_points = new List<List<Point2f>>(); // # 2d points

            var size = new OpenCvSharp.Size(800, 600);

            // map the images, find & store the Chessboard Corners
            foreach (var imgfile in CelibrateImages)
            {
                var path = Path.GetFullPath(imgfile.FileName);

                // PYTHON: fileName, fileExt = os.path.splitext(imgfilepath)
                var fileName = Path.GetFileNameWithoutExtension(imgfile.FileName);
                var fileExt = Path.GetExtension(imgfile.FileName);

                // PYTHON: img = cv2.imread(fname)
                var img = Cv2.ImRead(imgfile.FileName, ImreadModes.AnyColor);

                // PYTHON: gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
                var gray = img.CvtColor(ColorConversionCodes.BGR2GRAY);

                // PYTHON: size = gray.shape[::-1]
                size = img.Size();


                Point2f[] corners;
                // PYTHON: ret, corners = cv2.findChessboardCorners(gray, (ROW, COL), None)
                var found = Cv2.FindChessboardCorners(gray, PatternSize, out corners);

                //If found, add object points, image points (after refining them)
                if (found)
                {
                    obj_points.Add(objpl);

                    // PYTHON: corners2 = cv2.cornerSubPix(gray, corners, (6, 6), (-1, -1), criteria)
                    //        # keep finding sub pix
                    var corners2 = Cv2.CornerSubPix(gray, corners, new OpenCvSharp.Size(6, 6),
                                                    new OpenCvSharp.Size(-1, -1), criteria);

                    // PYTHON: if corners2.all:
                    if (corners2?.Length > 0)
                    {
                        // PYTHON:  img_points.append(corners2)
                        img_points.Add(corners2.ToList());
                    }
                    else
                    {
                        img_points.Add(corners.ToList());
                    }

                    // PYTHON: cv2.drawChessboardCorners(img, (ROW, COL), corners, ret)
                    Cv2.DrawChessboardCorners(img, PatternSize, corners, found);

                    var save_path = Path.Combine(path,
                        $"{fileName}_CC_{DateTime.Now.ToString("ss_ffff")}{fileExt}");

                    // PYTHON:  cv2.imwrite(save_path, img)
                    Cv2.ImWrite(save_path, img);
                }

                gray.Dispose();
                img.Dispose();
            }

            #region begin calibrate
            double[,] cameraMatrix = new double[3, 3];
            double[] distCoeffs = new double[5];

            Vec3d[] rvecs = new Vec3d[] { };
            Vec3d[] tvecs = new Vec3d[] { };

            //PYTHON:   ret, mtx, dist, rvecs, tvecs = cv2.calibrateCamera(obj_points, img_points, size, None, None)
            var ret = Cv2.CalibrateCamera(obj_points, img_points, size,
                                            cameraMatrix, distCoeffs, out rvecs, out tvecs);
            #endregion end calibrate

            //record into global var
            Ret = ret;

            //   cameraMatrix:
            //      Output 3x3 floating-point camera matrix. If CV_CALIB_USE_INTRINSIC_GUESS and/or
            //      CV_CALIB_FIX_ASPECT_RATIO are specified, some or all of fx, fy, cx, cy must be
            //      initialized before calling the function.
            CameraMatrix = cameraMatrix;


            //   distCoeffs:
            //      Output vector of distortion coefficients (k_1, k_2, p_1, p_2[, k_3[, k_4, k_5,
            //      k_6]]) of 4, 5, or 8 elements.
            DistCoeffs = distCoeffs;


            //   rvecs:
            //      Output vector of rotation vectors (see Rodrigues() ) estimated for each pattern
            //      view. That is, each k-th rotation vector together with the corresponding k-th
            //      translation vector (see the next output parameter description) brings the calibration
            //      pattern from the model coordinate space (in which object points are specified)
            //      to the world coordinate space, that is, a real position of the calibration pattern
            //      in the k-th pattern view (k=0.. M -1)
            RVecs = rvecs;


            //   tvecs:
            //      Output vector of translation vectors estimated for each pattern view.
            TVecs = tvecs;


            Debug.WriteLine($"\nret: {ret}");
            Debug.WriteLine($"\ncameraMatrix:\n {cameraMatrix.ToStringMatrix()}");
            Debug.WriteLine($"\ndistCoeffs: {string.Join(",", distCoeffs)}");
            Debug.WriteLine($"\nrvecs: {string.Join(",", rvecs)}");
            Debug.WriteLine($"\ntvecs: {string.Join(",", tvecs)}");
            Debug.WriteLine($"\n");

            // calculate total & mean error
            TotalError = 0.0;

            // PYTHON: for i in range(len(obj_points)):
            for (var i = 0; i < obj_points.Count; i++)
            {
                var imagePoints = new Mat();
                // PYTHON: img_points2, _ = cv2.projectPoints(obj_points[i], rvecs[i], tvecs[i], mtx, dist)
                Cv2.ProjectPoints(InputArray.Create(obj_points[i]),
                    rvecs[i], tvecs[i],
                    InputArray.Create(cameraMatrix),
                    InputArray.Create(distCoeffs),
                    imagePoints);

                // PYTHON: error = cv2.norm(img_points[i], img_points2, cv2.NORM_L2)/len(img_points2)
                var error = Cv2.Norm(InputArray.Create(img_points[i]),
                                imagePoints, NormTypes.L2) / imagePoints.Size().Height;
                TotalError += error;
            }

            //record into class member vars
            MeanError = TotalError / obj_points.Count();

            Debug.WriteLine($"\ntotal_error: {TotalError}");
            Debug.WriteLine($"\nmean_error: {MeanError}");
            Debug.WriteLine($"\n");
            Debug.WriteLine($"\n");

            string json = JsonConvert.SerializeObject(new CalibrationResult
            {
                Ret = Ret,
                CameraMatrix = CameraMatrix,
                DistCoeffs = DistCoeffs,
                RVecs = RVecs,
                TVecs = TVecs
            }, Formatting.Indented);

            File.WriteAllText("CameraCalibration.json", json);


            return true;


        }

        /// <summary>
        /// Calibrate Image via Cv2.Undistort(), after CalibrateCamera()
        /// </summary>
        /// <param name="filepath"></param>
        public Image CalibrateImageByUndistort(string filepath)
        {
            if (!File.Exists(filepath)) return null;


            var image = Cv2.ImRead(filepath, ImreadModes.AnyColor);

            //   validPixROI:
            //      Optional output rectangle that outlines all-good-pixels region in the undistorted
            //      image. See roi1, roi2 description in stereoRectify() .
            Rect validPixROI;

            // PYTHON: newcameramtx, roi = cv2.getOptimalNewCameraMatrix(mtx, dist, (w, h), 1, (w, h))
            var newCameraMatrix = Cv2.GetOptimalNewCameraMatrix(CameraMatrix, DistCoeffs,
                                    image.Size(), 1, image.Size(), out validPixROI);

            //undistort
            var dst = new Mat();

            // PYTHON: dst = cv2.undistort(img, mtx, dist, None, newcameramtx)
            Cv2.Undistort(image, dst, InputArray.Create(CameraMatrix), InputArray.Create(DistCoeffs),
                InputArray.Create(newCameraMatrix));

            // PYTHON:
            //      x, y, w, h = roi
            //      dst1 = dst[y: y + h, x: x + w]
            var newDst = dst[validPixROI];

            return newDst?.ToBitmap();
        }

        /// <summary>
        /// Calibrate Image via Cv2.remap(), after CalibrateCamera()
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public Image CalibrateImageByRemap(string filepath)
        {
            if (!File.Exists(filepath)) return null;

            var image = Cv2.ImRead(filepath, ImreadModes.AnyColor);


            //   validPixROI:
            //      Optional output rectangle that outlines all-good-pixels region in the undistorted
            //      image. See roi1, roi2 description in stereoRectify() .
            Rect validPixROI;

            // PYTHON: newcameramtx, roi = cv2.getOptimalNewCameraMatrix(mtx, dist, (w, h), 1, (w, h))
            var newCameraMatrix = Cv2.GetOptimalNewCameraMatrix(CameraMatrix, DistCoeffs, image.Size(), 1, image.Size(), out validPixROI);

            var map1 = new Mat();
            var map2 = new Mat();

            // PYTHON:  mapx, mapy = cv2.initUndistortRectifyMap(mtx, dist, None, newcameramtx, (w, h), 5)
            Cv2.InitUndistortRectifyMap(InputArray.Create(CameraMatrix), InputArray.Create(DistCoeffs), null, InputArray.Create(newCameraMatrix),
                image.Size(), MatType.CV_32FC1, map1, map2);


            var dst = new Mat();

            // PYTHON:  dst = cv2.remap(img, mapx, mapy, cv2.INTER_CUBIC)
            Cv2.Remap(image, dst, map1, map2, InterpolationFlags.Cubic);

            // PYTHON:
            //      x, y, w, h = roi
            //      dst2 = dst[y: y + h, x: x + w]
            var newDst = dst[validPixROI];

            return newDst?.ToBitmap();

        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        List<Point3f> NDArrayToList(NDArray array)
        {
            if (array == null) return null;

            var length = array.shape[0];

            List<Point3f> target = new List<Point3f>();
            for (var i = 0; i < length; i++)
            {
                var item1 = (float)array[i][0];
                var item2 = (float)array[i][1];
                var item3 = (float)array[i][2];

                var point3f = new Point3f() { X = item1, Y = item2, Z = item3 };
                target.Add(point3f);
            }

            return target;
        }

    }
}
