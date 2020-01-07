// Custom Desktop Logo 1.0 - By: 2008 Eric Wong
// September 20th, 2008
// Custom Desktop Logo is open source software licensed under GNU GENERAL PUBLIC LICENSE V3. 
// Use it as you wish, but you must share your source code under the terms of use of the license.

// Custom Desktop Logo allows you to create custom static and animated logos from PNG images.

// Copyright (C) 2008 by Eric Wong. 
// VideoInPicture@gmail.com
// http://customdesktoplogo.wikidot.com
// http://easyunicodepaster.wikidot.com
// http://circledock.wikidot.com
// http://videoinpicture.wikidot.com
// http://webcamsignature.wikidot.com
// http://windowextractor.wikidot.com

// Uses AMS.Profile from http://www.codeproject.com/KB/cs/readwritexmlini.aspx for .ini file operations (Open source, non-specific license)
// Uses hotkey selector component from http://www.codeproject.com/KB/miscctrl/systemhotkey.aspx (Open source, non-specific license)

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace ImageOperations
{
    /// <summary> 
    /// Collection of bitmap operation routines.
    /// </summary>
    class BitmapOperations
    {
        /// <summary> 
        /// Returns a bitmap scaled by a factor of ScaleFactorX and ScaleFactorY. Uses high quality scaling.
        /// Returns null if an error is encountered.
        /// </summary>
        /// <param name="Bitmap">Image to scale.</param>
        /// <param name="ScaleFactorX">Factor to scale the x-size by.</param>
        /// <param name="ScaleFactorY">Factor to scale the y-size by.</param>
        public static Bitmap ScaleByFactors(ref Bitmap Bitmap, float ScaleFactorX, float ScaleFactorY)
        {
            try
            {
                var scaleWidth = (int)Math.Max(Bitmap.Width * ScaleFactorX, 1.0f);
                var scaleHeight = (int)Math.Max(Bitmap.Height * ScaleFactorY, 1.0f);

                var scaledBitmap = new Bitmap(scaleWidth, scaleHeight);

                // Scale the bitmap in high quality mode.
                using (var gr = Graphics.FromImage(scaledBitmap))
                {
                    gr.SmoothingMode = SmoothingMode.HighSpeed;
                    gr.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                    gr.CompositingQuality = CompositingQuality.HighSpeed;
                    gr.InterpolationMode = InterpolationMode.High;

                    gr.DrawImage(Bitmap, new Rectangle(0, 0, scaleWidth, scaleHeight), new Rectangle(0, 0, Bitmap.Width, Bitmap.Height), GraphicsUnit.Pixel);
                }

                return scaledBitmap;
            }
            catch (Exception)
            {
                return null;
            }
        }


        /// <summary> 
        /// Returns a bitmap resized to NewWidth and NewHeight. Uses high quality scaling.
        /// Returns null if an error is encountered.
        /// </summary>
        /// <param name="Bitmap">Image to resize.</param>
        /// <param name="NewWidth">Width of the resized bitmap.</param>
        /// <param name="NewHeight">Height of the resized bitmap.</param>
        public static Bitmap ScaleBySize(ref Bitmap Bitmap, int NewWidth, int NewHeight)
        {
            try
            {
                var scaledBitmap = new Bitmap(NewWidth, NewHeight);

                // Scale the bitmap in high quality mode.
                using (var gr = Graphics.FromImage(scaledBitmap))
                {
                    gr.SmoothingMode = SmoothingMode.HighSpeed;
                    gr.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                    gr.CompositingQuality = CompositingQuality.HighSpeed;
                    gr.InterpolationMode = InterpolationMode.High;

                    gr.DrawImage(Bitmap, new Rectangle(0, 0, NewWidth, NewHeight), new Rectangle(0, 0, Bitmap.Width, Bitmap.Height), GraphicsUnit.Pixel);
                }

                return scaledBitmap;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary> 
        /// Returns a bitmap resized to NewWidth and NewHeight. Uses high quality scaling.
        /// Returns null if an error is encountered.
        /// </summary>
        /// <param name="Bitmap">Image to resize.</param>
        /// <param name="NewWidth">Width of the resized bitmap.</param>
        /// <param name="NewHeight">Height of the resized bitmap.</param>
        public static Bitmap ScaleBySize(Bitmap Bitmap, int NewWidth, int NewHeight)
        {
            try
            {
                var scaledBitmap = new Bitmap(NewWidth, NewHeight);

                // Scale the bitmap in high quality mode.
                using (var gr = Graphics.FromImage(scaledBitmap))
                {
                    gr.SmoothingMode = SmoothingMode.HighSpeed;
                    gr.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                    gr.CompositingQuality = CompositingQuality.HighSpeed;
                    gr.InterpolationMode = InterpolationMode.High;

                    gr.DrawImage(Bitmap, new Rectangle(0, 0, NewWidth, NewHeight), new Rectangle(0, 0, Bitmap.Width, Bitmap.Height), GraphicsUnit.Pixel);
                }

                return scaledBitmap;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary> 
        /// Returns a bitmap resized to fit inside NewWidth and NewHeight without aspect ratio change. Uses high quality scaling.
        /// Returns null if an error is encountered.
        /// </summary>
        /// <param name="Bitmap">Image to resize.</param>
        /// <param name="NewWidth">Width of the resized bitmap.</param>
        /// <param name="NewHeight">Height of the resized bitmap.</param>
        public static Bitmap ScaleBySizeBestFit(ref Bitmap Bitmap, int NewWidth, int NewHeight)
        {
            try
            {
                var scaledBitmap = new Bitmap(NewWidth, NewHeight);

                // Scale the bitmap in high quality mode.
                using (var gr = Graphics.FromImage(scaledBitmap))
                {
                    gr.SmoothingMode = SmoothingMode.HighSpeed;
                    gr.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                    gr.CompositingQuality = CompositingQuality.HighSpeed;
                    gr.InterpolationMode = InterpolationMode.High;

                    var scaledBitmapAspectRatio = scaledBitmap.Width / (double)scaledBitmap.Height;
                    var sourceBitmapAspectRatio = Bitmap.Width / (double)Bitmap.Height;

                    int bestLeft, bestRight, bestTop, bestBottom, calcWidth, calcHeight;

                    if (sourceBitmapAspectRatio > scaledBitmapAspectRatio)      // The source bitmap is wider
                    {
                        bestLeft = 0;
                        bestRight = scaledBitmap.Width;

                        calcHeight = (int)(scaledBitmap.Width / sourceBitmapAspectRatio);
                        bestTop = (scaledBitmap.Height - calcHeight) / 2;
                        bestBottom = calcHeight;
                    }
                    else
                    {
                        bestTop = 0;
                        bestBottom = scaledBitmap.Height;

                        calcWidth = (int)(scaledBitmap.Height * sourceBitmapAspectRatio);
                        bestLeft = (scaledBitmap.Width - calcWidth) / 2;
                        bestRight = calcWidth;
                    }

                    gr.DrawImage(Bitmap, new Rectangle(bestLeft, bestTop, bestRight, bestBottom), new Rectangle(0, 0, Bitmap.Width, Bitmap.Height), GraphicsUnit.Pixel);
                }

                return scaledBitmap;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Class to generate Fancy text.
    /// </summary>
    public class FancyText
    {
        /// <summary>
        /// Make fancy blurred text image used in Snaptune One UI.
        /// </summary>
        /// <param name="strText">text to make fancy, multiple line is OK</param>
        /// <param name="fnt">font to use</param>
        /// <param name="clrFore">foreground color of text</param>
        /// <param name="clrBack">background color of text</param>
        /// <param name="blurAmount">Amount to blur text (ie. number of pixels to smear around)</param>
        /// <param name="blurAlpha">Alpha value to use when drawing shadow. Recommended is 16. (0-255)</param>
        /// <returns>fancy image of text (transparent background)</returns>
        /// <owner>warrenb</owner>  <reviewed>00/00/00</reviewed>
        public static Image ImageFromText(string strText, Font fnt, Color clrFore, Color clrBack, int blurAmount, int blurAlpha)
        {
            if (blurAlpha > 255)
                blurAlpha = 255;
            else if (blurAlpha < 0)
                blurAlpha = 0;

            Bitmap bmpOut = null; // bitmap we are creating and will return from this function.

            using (var g = Graphics.FromHwnd(IntPtr.Zero))
            {
                var sz = g.MeasureString(strText, fnt);
                using (var bmp = new Bitmap((int)sz.Width, (int)sz.Height))
                using (var gBmp = Graphics.FromImage(bmp))
                using (var brBack = new SolidBrush(Color.FromArgb(blurAlpha, clrBack.R, clrBack.G, clrBack.B)))
                using (var brFore = new SolidBrush(clrFore))
                {
                    gBmp.SmoothingMode = SmoothingMode.HighSpeed;
                    gBmp.InterpolationMode = InterpolationMode.HighQualityBilinear;
                    gBmp.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                    gBmp.DrawString(strText, fnt, brBack, 0, 0);

                    // make bitmap we will return.
                    bmpOut = new Bitmap(bmp.Width + blurAmount, bmp.Height + blurAmount);
                    using (var gBmpOut = Graphics.FromImage(bmpOut))
                    {
                        gBmpOut.SmoothingMode = SmoothingMode.HighSpeed;
                        gBmpOut.InterpolationMode = InterpolationMode.HighQualityBilinear;
                        gBmpOut.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                        // smear image of background of text about to make blurred background "halo"
                        for (var x = 0; x <= blurAmount; x++)
                            for (var y = 0; y <= blurAmount; y++)
                                gBmpOut.DrawImageUnscaled(bmp, x, y);

                        // draw actual text
                        gBmpOut.DrawString(strText, fnt, brFore, blurAmount / 2, blurAmount / 2);
                    }
                }
            }

            return bmpOut;
        }
    }
}
