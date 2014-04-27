using System;
using System.Collections.Generic;
using System.Linq;
using Toolbelt.Net.Smtp.Internal;

namespace Toolbelt.Net.Smtp
{
    public class SmtpHeader
    {
        public string Key { get; set; }

        public List<string> RawValues { get; set; }

        public string Value { get { return string.Concat(this.RawValues); } }

        public SmtpHeader()
        {
            this.RawValues = new List<string>();
        }

        public SmtpHeader(string rawString)
        {
            var splitPos = rawString.IndexOf(':');
            this.Key = rawString.Substring(0, splitPos);
            this.RawValues = new List<string>();
            this.RawValues.Add(rawString.Substring(splitPos + 1).Trim());
        }

        public SmtpHeader(string key, params string[] values)
        {
            this.Key = key;
            this.RawValues = new List<string>(values);
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", this.Key, this.RawValues.Combine("|"));
        }

        public static readonly SmtpHeader Empty = new SmtpHeader();
    }
}
