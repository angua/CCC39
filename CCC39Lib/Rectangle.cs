using System.Numerics;

namespace CCC39Lib;

public class Rectangle
{
    public Rectangle(Vector2 upperLeftCorner, Vector2 lowerRightCorner)
    {
        LeftX = (int)upperLeftCorner.X;
        TopY = (int)upperLeftCorner.Y;
        RightX = (int)lowerRightCorner.X;
        BottomY = (int)lowerRightCorner.Y;

        SetProperties();
    }

    public Rectangle(int leftX, int rightX, int topY, int bottomY)
    {
        LeftX = leftX;
        TopY = topY;
        RightX = rightX;
        BottomY = bottomY;

        SetProperties();
    }


    private void SetProperties()
    {
        Width = RightX - LeftX + 1;
        Height = BottomY - TopY + 1;

        Area = Width * Height;
    }


    public Rectangle(Rectangle rect)
    {
        LeftX = rect.LeftX;
        TopY = rect.TopY;
        RightX = rect.RightX;
        BottomY = rect.BottomY;

        Width = rect.Width;
        Height = rect.Height;

        Area = rect.Area;
    }

    public int LeftX { get; private set; }
    public int TopY { get; private set; }
    public int RightX { get; private set; }
    public int BottomY { get; private set; }

    public int Width { get; private set; }
    public int Height { get; private set; }
    public int Area { get; private set; }


    internal bool Intersect(Vector2 position)
    {
        return position.X >= LeftX &&
               position.X <= RightX &&
               position.Y >= TopY &&
               position.Y <= BottomY;
    }

    internal bool Intersect(Rectangle rec2)
    {
        return LeftX <= rec2.RightX &&
            RightX >= rec2.LeftX &&
            TopY <= rec2.BottomY &&
            BottomY >= rec2.TopY;
    }

}
