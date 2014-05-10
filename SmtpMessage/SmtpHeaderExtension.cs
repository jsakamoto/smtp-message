using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Toolbelt.Net.Smtp
{
    public static class SmtpHeaderExtension
    {
        public static SmtpHeader Find(this IEnumerable<SmtpHeader> headers, string headerName)
        {
            headerName = headerName.ToUpper();
            return (headers.FirstOrDefault(hd => hd.Key.ToUpper() == headerName) ?? SmtpHeader.Empty);
        }

        public static string ValueOf(this IEnumerable<SmtpHeader> headers, string headerName, string defaultValue = "")
        {
            var value = headers.Find(headerName).Value;
            return value != "" ? value : defaultValue;
        }

        public static void Add(this ICollection<SmtpHeader> hedaers, string key, params string[] values)
        {
            hedaers.Add(new SmtpHeader(key, values));
        }

        public static void Add(this ICollection<SmtpHeader> hedaers, string rawString)
        {
            if (rawString.StartsWith(" ") || rawString.StartsWith("\t"))
            {
                hedaers.Last().RawValues.Add(rawString.TrimStart(' ', '\t'));
            }
            else
            {
                hedaers.Add(new SmtpHeader(rawString));
            }
        }

        public static List<SmtpHeader> CreateHeaders(IEnumerable<string> rawLines)
        {
            var headers = new List<SmtpHeader>();
            foreach (var line in rawLines)
            {
                if (line.StartsWith(" ") || line.StartsWith("\t"))
                {
                    headers.Last().RawValues.Add(line.TrimStart(' ', '\t'));
                }
                else
                {
                    headers.Add(new SmtpHeader(line));
                }
            }

            return headers;
        }

        public static string GetContentType(this IEnumerable<SmtpHeader> headers)
        {
            var contentType = headers.ValueOf("Content-Type");
            var m = Regex.Match(contentType, "^(?<type>[a-z0-9/]+)", RegexOptions.IgnoreCase);
            var grp = m.Groups["type"];
            return grp.Success ? grp.Value : "text/plain";
        }

        public static string GetTransferEncoding(this IEnumerable<SmtpHeader> headers)
        {
            return headers.ValueOf("Content-Transfer-Encoding", defaultValue: "7bit");
        }

        public static string GetCharSet(this IEnumerable<SmtpHeader> headers)
        {
            var contentType = headers.ValueOf("Content-Type");
            var m = Regex.Match(contentType, "charset=\"?(?<charset>[^\";]+)\"?", RegexOptions.IgnoreCase);
            var grp = m.Groups["charset"];
            return grp.Success ? grp.Value : "us-ascii";
        }

        public static string GetBoundary(this IEnumerable<SmtpHeader> headers)
        {
            var m = Regex.Match(headers.Find("Content-Type").Value, "boundary=(?<b>[^;]+)");
            return m.Success ? m.Groups["b"].Value : "";
        }

        public static string GetContentDisposition(this IEnumerable<SmtpHeader> headers)
        {
            return headers.ValueOf("Content-Disposition");
        }

        public static string GetContentName(this IEnumerable<SmtpHeader> headers)
        {
            var contentType = headers.ValueOf("Content-Type");
            var m = Regex.Match(contentType, "name=\"?(?<name>[^;\"]+)\"?", RegexOptions.IgnoreCase);
            var grp = m.Groups["name"];
            return grp.Success ? grp.Value : "";
        }
    }
}
