using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CommonWPF
{
    public class BitmapGridDrawing
    {
        public BitmapDrawing Drawing;

        public int Rows { get; set; }
        public int Columns { get; set; }

        public int Size { get; set; }

        public int GridLineThickness { get; set; }
        public Color GridLineColor { get; set; } = Color.FromRgb(255, 255, 255);

        public Color BackgroundColor { get; set; } = Color.FromRgb(0, 0, 0);

        public WriteableBitmap Picture => Drawing.Picture;


        public BitmapGridDrawing(int rows, int columns, int size, int gridLineThickness)
        {
            Rows = rows;
            Columns = columns;
            Size = size;
            GridLineThickness = gridLineThickness;

            Drawing = new BitmapDrawing(Columns * Size + (Columns - 1) * GridLineThickness,
                                            Rows * Size + (Rows - 1) * GridLineThickness);
        }

        public BitmapGridDrawing(BitmapGridDrawing drawing)
        {
            Rows = drawing.Rows;
            Columns = drawing.Columns;
            Size = drawing.Size;
            GridLineThickness = drawing.GridLineThickness;
            GridLineColor = drawing.GridLineColor;
            BackgroundColor = drawing.BackgroundColor;

            Drawing = new BitmapDrawing(drawing.Drawing);
        }

        public void DrawBackGround()
        {
            // draw grid lines
            if (GridLineThickness > 0)
            {
                for (int x = 0; x < Columns - 1; x++)
                {
                    Drawing.DrawRectangle(Size + x * (Size + GridLineThickness), 0, 
                        GridLineThickness, Drawing.Picture.PixelHeight, 
                        GridLineColor);
                }

                for (int y = 0; y < Rows - 1; y++)
                {
                    Drawing.DrawRectangle(0, Size + y * (Size + GridLineThickness),
                        Drawing.Picture.PixelWidth, GridLineThickness,
                        GridLineColor);
                }
            }

            // fill cells
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    FillGridCell(x, y, BackgroundColor);
                }
            }
        }


        /// <summary>
        /// Fill grid cell with color (zero based counting
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        public void FillGridCell(int x, int y, System.Windows.Media.Color color)
        {
            // start positions on bitmap
            var cellCoordinates = GetCellPosition(x, y);

            Drawing.DrawRectangle((int)cellCoordinates.X, (int)cellCoordinates.Y, Size, Size, color);
        }



        /// <summary>
        /// Draw a series of connected lines from center of one grid cell to the next
        /// </summary>
        /// <param name="linepositions">List of grid cells to be connected</param>
        /// <param name="color">RGB color</param>
        public void DrawConnectedLines(List<Vector2> linepositions, System.Windows.Media.Color color)
        {
            var half = (int)(Size / 2);

            // line start position on bitmap
            for (int i = 0; i < linepositions.Count - 1; i++)
            {
                var startMapPos = linepositions[i];
                var endMappos = linepositions[i + 1];

                var lineStart = startMapPos * Size + new Vector2(half, half);
                var lineEnd = endMappos * Size + new Vector2(half, half);
                
                Drawing.DrawLine(lineStart, lineEnd, color);
            }
        }

        /// <summary>
        /// Draw an "X" into the specified grid cell
        /// </summary>
        /// <param name="x">X position of the grid cell</param>
        /// <param name="y">Y position of the grid cell</param>
        /// <param name="size">Size in pixels of the X</param>
        /// <param name="color">RGB color</param>
        public void DrawXInGridcell(int x, int y, int drawSize, System.Windows.Media.Color color)
        {
            var cellCoordinates = GetCellPosition(x, y);
            // center X in in grid cell
            var sizediff = Size - drawSize;

            var startX = (int)(cellCoordinates.X + sizediff / 2);
            var startY = (int)(cellCoordinates.Y + sizediff / 2);

            Drawing.DrawX(startX, startY, drawSize, color);
        }

        /// <summary>
        /// Pixel coordinates of the upper left corner of the grid cell
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private Vector2 GetCellPosition(int x, int y)
        {
            return new Vector2(x * (Size + GridLineThickness), y * (Size + GridLineThickness));
        }

    }
}
