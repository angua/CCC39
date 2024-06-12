using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace CommonWPF;

public class BitmapDrawing
{
    public WriteableBitmap Picture { get; private set; }

    /// <summary>
    /// Create writeable bitmap
    /// </summary>
    /// <param name="width">Width in pixels</param>
    /// <param name="height">Height in pixels</param>
    public BitmapDrawing(int width, int height)
    {
        Picture = new WriteableBitmap(width, height,
              96,
              96,
              PixelFormats.Bgr32,
              null);
    }

    public BitmapDrawing(BitmapDrawing input)
    {
        Picture = new WriteableBitmap(input.Picture);
    }

    /// <summary>
    /// Draw rectangle on bitmap
    /// </summary>
    /// <param name="startX">Start x position in pixels</param>
    /// <param name="startY">Start y position in pixels</param>
    /// <param name="width">Width in pixels</param>
    /// <param name="height">Height in pixels</param>
    /// <param name="color">RGB color</param>
    public void DrawRectangle(int startX, int startY, int width, int height, Color color)
    {
        try
        {
            // Reserve the back buffer for updates.
            Picture.Lock();

            // rectangle
            for (var deltaX = 0; deltaX < width; deltaX++)
            {
                for (var deltaY = 0; deltaY < height; deltaY++)
                {
                    var posX = startX + deltaX;
                    var posY = startY + deltaY;
                    SetPixel(posX, posY, color);
                }
            }

            // Specify the area of the bitmap that changed.
            Picture.AddDirtyRect(new Int32Rect(0, 0, Picture.PixelWidth, Picture.PixelHeight));
        }
        finally
        {
            // Release the back buffer and make it available for display.
            Picture.Unlock();
        }

    }



    public void DrawLine(Vector2 startPosition, Vector2 endPosition, Color color)
    {
        var linePoints = GetPointsOnLine(startPosition, endPosition);

        try
        {
            Picture.Lock();

            foreach (var point in linePoints)
            {
                SetPixel((int)point.X, (int)point.Y, color);
            }

            // Specify the area of the bitmap that changed.
            Picture.AddDirtyRect(new Int32Rect(0, 0, Picture.PixelWidth, Picture.PixelHeight));
        }
        finally
        {
            Picture.Unlock();
        }
    }




    /// <summary>
    /// Draws an "X" on the image. Perfect for marking your treasures on a map
    /// </summary>
    /// <param name="startX">X pixel coordínate of the upper left corner</param>
    /// <param name="startY">Y pixel coordínate of the upper left corner</param>
    /// <param name="size">Diameter (in pixels) of the "X"/// </param>
    /// <param name="color">RGB color</param>
    public void DrawX(int startX, int startY, int size, Color color)
    {
        var half = (int)(size / 2);

        try
        {
            Picture.Lock();

            SetPixel(startX + half, startY + half, color);

            for (var i = 1; i < half; i++)
            {
                SetPixel(startX + i, startY + i, color);
                SetPixel(startX + half + i, startY + half + i, color);

                SetPixel(startX + half - i, startY + half + i, color);
                SetPixel(startX + half + i, startY + half - i, color);
            }

            // Specify the area of the bitmap that changed.
            Picture.AddDirtyRect(new Int32Rect(0, 0, Picture.PixelWidth, Picture.PixelHeight));
        }
        finally
        {
            Picture.Unlock();
        }
    }


    /// <summary>
    /// Sets the color of the specified pixel
    /// </summary>
    /// <param name="x">X coordinate of the pixel</param>
    /// <param name="y">Y coordinate of the pixel</param>
    /// <param name="color">RGB color of the pixel</param>
    private void SetPixel(int x, int y, Color color)
    {
        var bytesPerPixel = (Picture.Format.BitsPerPixel + 7) / 8;
        var stride = Picture.PixelWidth * bytesPerPixel;

        int posX = x * bytesPerPixel;
        int posY = y * stride;

        unsafe
        {
            // Get a pointer to the back buffer.
            var backBuffer = Picture.BackBuffer;

            // Find the address of the pixel to draw.
            backBuffer += y * Picture.BackBufferStride;
            backBuffer += x * 4;

            // Compute the pixel's color.
            int color_data = color.R << 16; // R
            color_data |= color.G << 8;   // G
            color_data |= color.B << 0;   // B

            // Assign the color data to the pixel.
            *((int*)backBuffer) = color_data;
        }

    }

    /// <summary>
    /// Return the rasterized line from start to end, including the start / end points themselves.
    /// </summary>
    public static IEnumerable<Vector2> GetPointsOnLine(Vector2 start, Vector2 end)
    {
        var delta = end - start;

        if (Math.Abs(delta.X) > Math.Abs(delta.Y))
        {
            var numSteps = Math.Abs(delta.X);
            var deltaX = delta.X < 1 ? -1 : 1;
            for (var i = 0; i < numSteps; ++i)
            {
                var yStep = delta.Y / numSteps;
                yield return new Vector2(start.X + deltaX * i, (int)(start.Y + yStep * i));
            }
        }
        else
        {
            var numSteps = Math.Abs(delta.Y);
            var deltaY = delta.Y < 1 ? -1 : 1;
            for (var i = 0; i < numSteps; ++i)
            {
                var xStep = delta.X / numSteps;
                yield return new Vector2((int)(start.X + xStep * i), start.Y + deltaY * i);
            }
        }

        yield return end;
    }


}