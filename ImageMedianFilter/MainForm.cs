/*
 * The Following Code was developed by Dewald Esterhuizen
 * View Documentation at: http://softwarebydefault.com
 * Licensed under Ms-PL 
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace ImageMedianFilter
{
    public partial class MainForm : Form
    {
        private Bitmap originalBitmap = null;
        private Bitmap previewBitmap = null;
        private Bitmap resultBitmap = null;
        private float time = 0;
        
        public MainForm()
        {
            InitializeComponent();

            cmbEdgeDetection.SelectedIndex = 0;
            cmbThreads.SelectedIndex = 0;
            lblTime.Text = "";
        }

        private void btnOpenOriginal_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select an image file.";
            ofd.Filter = "Png Images(*.png)|*.png|Jpeg Images(*.jpg)|*.jpg";
            ofd.Filter += "|Bitmap Images(*.bmp)|*.bmp";

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StreamReader streamReader = new StreamReader(ofd.FileName);
                originalBitmap = (Bitmap)Bitmap.FromStream(streamReader.BaseStream);
                streamReader.Close();

                previewBitmap = originalBitmap.CopyToSquareCanvas(picPreview.Width);
                picPreview.Image = previewBitmap;

                ApplyFilter(true);
            }
        }

        private void btnSaveNewImage_Click(object sender, EventArgs e)
        {
            ApplyFilter(false);

            if (resultBitmap != null)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Title = "Specify a file name and file path";
                sfd.Filter = "Png Images(*.png)|*.png|Jpeg Images(*.jpg)|*.jpg";
                sfd.Filter += "|Bitmap Images(*.bmp)|*.bmp";

                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string fileExtension = Path.GetExtension(sfd.FileName).ToUpper();
                    ImageFormat imgFormat = ImageFormat.Png;

                    if (fileExtension == "BMP")
                    {
                        imgFormat = ImageFormat.Bmp;
                    }
                    else if (fileExtension == "JPG")
                    {
                        imgFormat = ImageFormat.Jpeg;
                    }

                    StreamWriter streamWriter = new StreamWriter(sfd.FileName, false);
                    resultBitmap.Save(streamWriter.BaseStream, imgFormat);
                    streamWriter.Flush();
                    streamWriter.Close();

                    resultBitmap = null;
                }
            }
        }

        private void ApplyFilter(bool preview)
        {
            if (previewBitmap == null || cmbEdgeDetection.SelectedIndex == -1)
            {
                return;
            }

            Bitmap selectedSource = null;
            Bitmap bitmapResult = null;

            if (preview == true)
            {
                selectedSource = previewBitmap;
            }
            else
            {
                selectedSource = originalBitmap;
            }

            if (selectedSource != null)
            {
                //  Optimization
                int threads = 1;
                switch (cmbThreads.SelectedIndex.ToString())
                {
                    case "1":
                        threads = 1;
                        break;
                    case "2":
                        threads = 2;
                        break;
                    case "4":
                        threads = 4;
                        break;
                    case "6":
                        threads = 6;
                        break;
                    case "8":
                        threads = 8;
                        break;
                    default:
                        threads = 1;
                        break;
                }
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                switch (cmbEdgeDetection.SelectedItem.ToString())
                {
                    case "None":
                        break;
                    case "Median 3x3":
                        bitmapResult = selectedSource.MedianFilter(3, threads);
                        break;
                    case "Median 5x5":
                        bitmapResult = selectedSource.MedianFilter(5, threads);
                        break;
                    case "Median 7x7":
                        bitmapResult = selectedSource.MedianFilter(7, threads);
                        break;
                    case "Median 9x9":
                        bitmapResult = selectedSource.MedianFilter(9, threads);
                        break;
                    case "Median 11x11":
                        bitmapResult = selectedSource.MedianFilter(11, threads);
                        break;
                    case "Median 13x13":
                        bitmapResult = selectedSource.MedianFilter(12, threads);
                        break;
                    default:
                        bitmapResult = selectedSource;
                        break;
                }
                stopwatch.Stop();
                time = stopwatch.ElapsedTicks;
                //
            }

            if (bitmapResult != null)
            {
                if (preview == true)
                {
                    picPreview.Image = bitmapResult;
                }
                else
                {
                    resultBitmap = bitmapResult;
                }
            }
            lblTime.Text = "Ticks: " + time.ToString();
        }

        private void NeighbourCountValueChangedEventHandler(object sender, EventArgs e)
        {
            ApplyFilter(true);
        }

        private void cmbThreads_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilter(true);
        }
    }
}
