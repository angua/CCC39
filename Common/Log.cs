namespace Common;

public static class Log
{
    public static bool IsActive = false;


    public static void WriteLine(string value)
    {
        if (IsActive)
        {
            Console.WriteLine(value);
        }
    }
    public static void WriteLine(FormattableString value)
    {
        if (IsActive)
        {
            Console.WriteLine(value);
        }
    }

    public static void WriteLine(Func<string> value)
    {
        if (IsActive)
        {
            Console.WriteLine(value());
        }
    }

    public static void Write(string value)
    {
        if (IsActive)
        {
            Console.Write(value);
        }
    }

    public static void Write(FormattableString value)
    {
        if (IsActive)
        {
            Console.Write(value);
        }
    }

    public static void WriteLine()
    {
        if (IsActive)
        {
            Console.WriteLine();
        }
    }

    public static void ReadLine()
    {
        if (IsActive)
        {
            Console.ReadLine();
        }
    }
}
