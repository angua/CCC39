using System.Numerics;

namespace CCC39Lib;

public class Rectangle
{
    public Rectangle(Vector2 upperLeftCorner, Vector2 lowerRightCorner)
    {
        UpperLeftCorner = upperLeftCorner;
        LowerRightCorner = lowerRightCorner;

        SetProperties();
    }

    private void SetProperties()
    {
        UpperLeftCornerX = (int)UpperLeftCorner.X;
        UpperLeftCornerY = (int)UpperLeftCorner.Y;
        LowerRightCornerX = (int)LowerRightCorner.X;
        LowerRightCornerY = (int)LowerRightCorner.Y;

        Width = LowerRightCornerX - UpperLeftCornerX + 1;
        Height = LowerRightCornerY - UpperLeftCornerY + 1;

        Area = Width * Height;
    }


    public Rectangle(Rectangle rect)
    {
        UpperLeftCorner = rect.UpperLeftCorner;
        LowerRightCorner = rect.LowerRightCorner;

        SetProperties();
    }

    public Vector2 UpperLeftCorner { get; private set; }
    public Vector2 LowerRightCorner { get; private set; }

    public int UpperLeftCornerX { get; private set; }
    public int UpperLeftCornerY { get; private set; }
    public int LowerRightCornerX { get; private set; }
    public int LowerRightCornerY { get; private set; }

    public int Width { get; private set; }
    public int Height { get; private set; }
    public int Area { get; private set; }



    internal bool Intersect(Vector2 position)
    {
        return position.X >= UpperLeftCornerX &&
               position.X <= LowerRightCornerX &&
               position.Y >= UpperLeftCornerY &&
               position.Y <= LowerRightCornerY;
    }

    internal bool Intersect(Rectangle rec2)
    {
        return UpperLeftCornerX <= rec2.LowerRightCornerX &&
            LowerRightCornerX >= rec2.UpperLeftCornerX &&
            UpperLeftCornerY <= rec2.LowerRightCornerY &&
            LowerRightCornerY >= rec2.UpperLeftCornerY;
    }



}
