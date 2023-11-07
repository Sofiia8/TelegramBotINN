using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TelegramBotINNConsole
{
    internal static class CheckRequest
    {
        internal static bool CheckFormatMainInfo(string text)
        {
            var pattern = @"^(\d+,)*\d+$";
            return Regex.IsMatch(text, pattern);
        }
        internal static bool CheckFormatFullInfo(string text, out string outputString)
        {
            var pattern = @"^#(\d+$)";
            var regex = Regex.Match(text, pattern); 
            outputString = regex.Groups[1].Value;
            return Regex.IsMatch(text, pattern);
        }
    }
}
