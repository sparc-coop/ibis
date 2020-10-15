using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Kuvio.Kernel.Core.Utils
{
    public static class StringUtils
    {
        public static string RemoveSpecialCharacters(string str)
        {
            return Regex.Replace(str, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
        }

        public static string StripHtml(string str)
        {
            string noHTML = Regex.Replace(str, @"<[^>]+>|&nbsp;", "").Trim();
            string noHTMLNormalised = Regex.Replace(noHTML, @"\s{2,}", " ");
            return noHTMLNormalised;
        }

        public static bool ContainsNullPropagating(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }
    }
}
