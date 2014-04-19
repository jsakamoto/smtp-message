using System;
using System.Collections.Generic;
using System.Linq;
using Toolbelt.Net.Smtp.Internal;

namespace Toolbelt.Net.Smtp
{
    public class SmtpHeader
    {
        public string Key { get; set; }

        public ICollection<string> RawValues { get; protected set; }

        public string Value { get { return string.Concat(this.RawValues); } }

        public SmtpHeader()
        {
            this.RawValues = new List<string>();
        }

        public SmtpHeader(string rawString)
        {
            var pair = rawString.SplitAndTrim(':');
            this.Key = pair.First();
            this.RawValues = new List<string>();
            this.RawValues.Add(pair.Last());
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
