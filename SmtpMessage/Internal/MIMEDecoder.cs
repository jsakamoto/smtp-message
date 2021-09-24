using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Toolbelt.Net.Smtp.Internal
{
    public static class MIMEDecoder
    {
        public static string DecodeTransfer(IEnumerable<SmtpHeader> headers, IEnumerable<string> data)
        {
            var charset = Encoding.GetEncoding(headers.GetCharSet());
            var decodedBytes = DecodeTransferToBytes(headers, data);
            return charset.GetString(decodedBytes);
        }

        public static byte[] DecodeTransferToBytes(IEnumerable<SmtpHeader> headers, IEnumerable<string> data)
        {
            var transferEncoding = headers.GetTransferEncoding();
            return DecodeTransferToBytes(transferEncoding, data);
        }

        public static byte[] DecodeTransferToBytes(string transferEncoding, IEnumerable<string> data)
        {
            switch (transferEncoding.ToLower())
            {
                case "q":
                case "quoted-printable":
                    return DecodeTransferToBytes_QuotedPrintable(data);
                case "b":
                case "base64":
                    return DecodeTransferToBytes_Base64(data);
                case "7bit":
                    return DecodeTransferToBytes_7bitAscii(data);
                default:
                    throw new Exception();
            }
        }

        private static byte[] DecodeTransferToBytes_QuotedPrintable(IEnumerable<string> data)
        {
            var translated = string.Concat(data
                .Select(_ => Regex.Replace(_, @"^\.\.", "."))
                .Select(_ => Regex.Replace(_, "=$", "")));
            var decodedBytes = Regex.Matches(translated, "(?<g1>(=[0-9a-fA-F]{2})+)|(?<g2>[^=]+)")
                .Cast<Match>()
                .SelectMany(m =>
                {
                    var g1 = m.Groups["g1"];
                    return g1.Success ?
                        g1.Value.SplitAndTrim('=').Select(s => byte.Parse(s, NumberStyles.HexNumber)) :
                        Encoding.ASCII.GetBytes(m.Groups["g2"].Value);
                });
            return decodedBytes.ToArray();
        }

        private static byte[] DecodeTransferToBytes_Base64(IEnumerable<string> data)
        {
            return Convert.FromBase64String(string.Concat(data));
        }

        private static byte[] DecodeTransferToBytes_7bitAscii(IEnumerable<string> data)
        {
            return Encoding.ASCII.GetBytes(string.Join("\r\n", data));
        }

        public static string DecodeText(string text)
        {
            var matches = Regex
                .Matches(text, @"(?<g1>=\?(?<char>[^\?]+)\?(?<enc>[^\?]+)\?(?<body>[^\?]+)\?=)|(?<g2>.+?)")
                .Cast<Match>();
            var m1 = matches.FirstOrDefault(m => m.Groups["g1"].Success);
            var charset = m1 != null ? m1.Groups["char"].Value : "us-ascii";
            var encoding = m1 != null ? m1.Groups["enc"].Value : "";

            var decodedBytes = matches.SelectMany(m =>
            {
                var g1 = m.Groups["g1"];
                return g1.Success ?
                    DecodeTransferToBytes(encoding, new[] { m.Groups["body"].Value }) :
                    Encoding.ASCII.GetBytes(m.Groups["g2"].Value);
            });

            return Encoding.GetEncoding(charset).GetString(decodedBytes.ToArray());
        }
    }
}
