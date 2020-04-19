using System.Text.RegularExpressions;

namespace Bitbucket
{
    class Validation
    {
        public static bool IsUUID(string text)
        {
            return Regex.IsMatch(text, @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}");
        }
    }
}
