/*
 * The Following Code was developed by Dewald Esterhuizen
 * View Documentation at: http://softwarebydefault.com
 * Licensed under Ms-PL 
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Diagnostics;

namespace ImageMedianFilter
{
    // ADD
    public class Calculation
    {

        public int matrixSize; 
        public Bitmap sourceBitmap;
        public BitmapData sourceData;
        public byte[] pixelBuffer;
        public byte[] resultBuffer;
        public int thread;
        public int threadSize;

        // Constructor
        public Calculation(int matrixSize, Bitmap sourceBitmap, BitmapData sourceData, byte[] pixelBuffer, byte[] resultBuffer,
            int thread, int threadSize)
        {
            this.matrixSize = matrixSize;
            this.sourceBitmap = sourceBitmap;
            this.sourceData = sourceData;
            this.pixelBuffer = pixelBuffer;
            this.resultBuffer = resultBuffer;
            this.thread = thread;
            this.threadSize = threadSize;
        }


        public void MedianFilterCalculator()
        {
            int filterOffset = (matrixSize - 1) / 2;
            int calcOffset = 0;

            int byteOffset = 0;

            List<int> neighbourPixels = new List<int>();
            byte[] middlePixel;

            Debug.Write("Thread: " + thread);

            for (int offsetY = filterOffset; offsetY <
                sourceBitmap.Height- filterOffset; offsetY++)
            {
                for (int offsetX = filterOffset; offsetX <
                    sourceBitmap.Width - filterOffset; offsetX++)
                {
                    byteOffset = offsetY *
                                 sourceData.Stride +
                                 offsetX * 4;

                    neighbourPixels.Clear();

                    for (int filterY = -filterOffset;
                        filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset;
                            filterX <= filterOffset; filterX++)
                        {

                            calcOffset = byteOffset +
                                         (filterX * 4) +
                                         (filterY * sourceData.Stride);

                            neighbourPixels.Add(BitConverter.ToInt32(
                                             pixelBuffer, calcOffset));
                        }
                    }

                    neighbourPixels.Sort();

                    middlePixel = BitConverter.GetBytes(
                                       neighbourPixels[filterOffset]);

                    resultBuffer[byteOffset] = middlePixel[0];
                    resultBuffer[byteOffset + 1] = middlePixel[1];
                    resultBuffer[byteOffset + 2] = middlePixel[2];
                    resultBuffer[byteOffset + 3] = middlePixel[3];
                }
            }
            Debug.Write("Watek skonczył zadanie");
        }
    }

    // ADD

    public static class ExtBitmap
    {
        public static Bitmap CopyToSquareCanvas(this Bitmap sourceBitmap, int canvasWidthLenght)
        {
            float ratio = 1.0f;
            int maxSide = sourceBitmap.Width > sourceBitmap.Height ?
                          sourceBitmap.Width : sourceBitmap.Height;

            ratio = (float)maxSide / (float)canvasWidthLenght;

            Bitmap bitmapResult = (sourceBitmap.Width > sourceBitmap.Height ?
                                    new Bitmap(canvasWidthLenght, (int)(sourceBitmap.Height / ratio))
                                    : new Bitmap((int)(sourceBitmap.Width / ratio), canvasWidthLenght));

            using (Graphics graphicsResult = Graphics.FromImage(bitmapResult))
            {
                graphicsResult.CompositingQuality = CompositingQuality.HighQuality;
                graphicsResult.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphicsResult.PixelOffsetMode = PixelOffsetMode.HighQuality;

                graphicsResult.DrawImage(sourceBitmap,
                                        new Rectangle(0, 0,
                                            bitmapResult.Width, bitmapResult.Height),
                                        new Rectangle(0, 0,
                                            sourceBitmap.Width, sourceBitmap.Height),
                                            GraphicsUnit.Pixel);
                graphicsResult.Flush();
            }

            return bitmapResult;
        }

        public static Bitmap MedianFilter(this Bitmap sourceBitmap, 
                                                int matrixSize,  
                                                  int bias = 0, 
                                         bool grayscale = false) 
        {
            // liczba watkow
            int n = 4;

            BitmapData sourceData = 
                       sourceBitmap.LockBits(new Rectangle(0, 0,
                       sourceBitmap.Width, sourceBitmap.Height),
                       ImageLockMode.ReadOnly, 
                       PixelFormat.Format32bppArgb);

            byte[] pixelBuffer = new byte[sourceData.Stride * 
                                          sourceData.Height];

            byte[] resultBuffer = new byte[sourceData.Stride * 
                                           sourceData.Height];

            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, 
                                       pixelBuffer.Length);

            sourceBitmap.UnlockBits(sourceData);

            if (grayscale == true)
            {
                float rgb = 0;

                for (int k = 0; k < pixelBuffer.Length; k += 4)
                {
                    rgb = pixelBuffer[k] * 0.11f;
                    rgb += pixelBuffer[k + 1] * 0.59f;
                    rgb += pixelBuffer[k + 2] * 0.3f;


                    pixelBuffer[k] = (byte)rgb;
                    pixelBuffer[k + 1] = pixelBuffer[k];
                    pixelBuffer[k + 2] = pixelBuffer[k];
                    pixelBuffer[k + 3] = 255;
                }
            }

            // Utworzenie n watkow
            Thread[] w = new Thread[n];
            /*
            for(int i = 0; i < n; i++)
            {
                Calculation calculation = new Obliczenia(matrixSize, sourceBitmap, sourceData, pixelBuffer, resultBuffer, i, n);
                w[i] = new Thread(new ThreadStart(calculation.MedianFilterCalculator));
                w[i].Start();
            }

            for(int i = 0; i < n; i++)
            {
                w[i].Join();
            }
            */

            Calculation calculation = new Calculation(matrixSize, sourceBitmap, sourceData, pixelBuffer, resultBuffer, 0, n);
            calculation.MedianFilterCalculator();

            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width, 
                                             sourceBitmap.Height);

            BitmapData resultData = 
                       resultBitmap.LockBits(new Rectangle(0, 0,
                       resultBitmap.Width, resultBitmap.Height),
                       ImageLockMode.WriteOnly,
                       PixelFormat.Format32bppArgb);

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, 
                                       resultBuffer.Length);

            resultBitmap.UnlockBits(resultData);

            return resultBitmap;
        }
    }  
}
