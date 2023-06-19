using System.Security;

namespace mqcat.Util;

public class Utils
{
    public static void LogError(string text)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"ERROR :: {text}");
        Console.ResetColor();
    }
    
    public static void LogInfo(string text)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(text);
        Console.ResetColor();
    }
    
    public static void LogSuccess(string text)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    // public SecureString GetPassword()
    // {
    //     var pwd = new SecureString();
    //     while (true)
    //     {
    //         ConsoleKeyInfo i = Console.ReadKey(true);
    //         if (i.Key == ConsoleKey.Enter)
    //         {
    //             break;
    //         }
    //         else if (i.Key == ConsoleKey.Backspace)
    //         {
    //             if (pwd.Length > 0)
    //             {
    //                 pwd.RemoveAt(pwd.Length - 1);
    //                 Console.Write("\b \b");
    //             }
    //         }
    //         else if (i.KeyChar != '\u0000' ) // KeyChar == '\u0000' if the key pressed does not correspond to a printable character, e.g. F1, Pause-Break, etc
    //         {
    //             pwd.AppendChar(i.KeyChar);
    //             Console.Write("*");
    //         }
    //     }
    //     return pwd;
    // }
    public static string ReadPassword()
    {
        var pass = string.Empty;
        ConsoleKey key;
        do
        {
            var keyInfo = Console.ReadKey(intercept: true);
            key = keyInfo.Key;

            if (key == ConsoleKey.Backspace && pass.Length > 0)
            {
                Console.Write("\b \b");
                pass = pass[0..^1];
            }
            else if (!char.IsControl(keyInfo.KeyChar) )
            {
                Console.Write("*");
                pass += keyInfo.KeyChar;
            }
        } while (key != ConsoleKey.Enter);

        return pass;
    }
}