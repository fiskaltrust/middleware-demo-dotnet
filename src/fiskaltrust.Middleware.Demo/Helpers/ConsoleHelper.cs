using System;
using System.Collections.Generic;
using System.Text;

namespace fiskaltrust.Middleware.Demo.Helpers
{
    public static class ConsoleHelper
    {
        public static void WriteError(string message)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(message);
            Console.ForegroundColor = color;
        }
    }
}
