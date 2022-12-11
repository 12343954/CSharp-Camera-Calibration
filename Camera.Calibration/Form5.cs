using Camera.Calibration.Business;
using Camera.Calibration.Extensions;
using Camera.Calibration.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Camera.Calibration
{
    public partial class Form5 : Form
    {
        Calibrate BLL_Calibrate;

        public Form5()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = Application.StartupPath;

            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                var files = Directory.GetFiles(folderBrowserDialog1.SelectedPath, "image_*.jpg", SearchOption.TopDirectoryOnly);
                if (files.Length > 0)
                {
                    var list = files.Select(x => new CalibrateImage { FileName = x }).ToList();
                    dataGridView1.DataSource = list;
                    pictureBox1.ImageLocation = list.FirstOrDefault()?.FileName;

                    _ = ProcessImage(list);
                }
            }
        }

        private CalibrateImage GetCurrentImage()
        {
            var item = dataGridView1.CurrentRow?.DataBoundItem as CalibrateImage;
            return item;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var imagePath = GetCurrentImage();
            pictureBox1.ImageLocation = imagePath.FileName;

            // call calibrate image method
            var image = BLL_Calibrate.CalibrateImageByUndistort(imagePath.FileName);
            pictureBox2.Image = image;
        }

        /// <summary>
        /// Multi-threading to process calibrating
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        async Task ProcessImage(List<CalibrateImage> list)
        {
            await Task.Run(() =>
            { 
                BLL_Calibrate = new Calibrate(list);
                BLL_Calibrate.CalibrateCamera();

                //after calibration , print result into textbox1
                textBox1.Invoke(new Action(() =>
                {
                    textBox1.Clear();
                   
                    textBox1.AppendText($"ret: {BLL_Calibrate.Ret}{Environment.NewLine}{Environment.NewLine}");
                    textBox1.AppendText($"cameraMatrix:{Environment.NewLine}{BLL_Calibrate.CameraMatrix.ToStringMatrix()}{Environment.NewLine}");
                    textBox1.AppendText($"distCoeffs:{Environment.NewLine}[{string.Join(",", BLL_Calibrate.DistCoeffs)}]{Environment.NewLine}{Environment.NewLine}");
                    textBox1.AppendText($"\nrvecs: {string.Join(",", BLL_Calibrate.RVecs)}");
                    textBox1.AppendText($"\ntvecs: {string.Join(",", BLL_Calibrate.TVecs)}");
                    textBox1.AppendText($"total error: {BLL_Calibrate.TotalError}{Environment.NewLine}");
                    textBox1.AppendText($"mean error: {BLL_Calibrate.MeanError}{Environment.NewLine}");
                    textBox1.AppendText(Environment.NewLine);
                }));
            });
        }

        private void Form5_Resize(object sender, EventArgs e)
        {
            splitContainer2.SplitterDistance = this.Width / 2;
            splitContainer3.SplitterDistance = this.Width / 2;
        }

       
    }
}
