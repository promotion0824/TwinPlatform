using static System.Console;

namespace Common;

public static class ConsoleHelper
{
    internal static void ClearNotifications()
    {
        SetCursorPosition(0, 0);
        string space = new(' ', WindowWidth);
        WriteLine(space);
        WriteLine(space);
        WriteLine(space);
        WriteLine(space);
    }

    public static void Clear(int startLine, int numberOfLines)
    {
        SetCursorPosition(0, startLine);
        string space = new(' ', WindowWidth);

        for (int i = 0; i < numberOfLines; i++)
        {
            WriteLine(space);
        }
        SetCursorPosition(0, startLine);
    }
}
