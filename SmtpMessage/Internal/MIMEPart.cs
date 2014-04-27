using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace Toolbelt.Net.Smtp.Internal
{
    public class MIMEPart
    {
        public List<SmtpHeader> Headers { get; set; }

        public List<string> Data { get; set; }

        public MIMEPart()
        {
            this.Headers = new List<SmtpHeader>();
            this.Data = new List<string>();
        }

        public MIMEPart(IEnumerable<SmtpHeader> headers, IEnumerable<string> data)
        {
            this.Headers = headers.ToList();
            this.Data = data.ToList();
        }

        public IEnumerable<MIMEPart> GetMultiParts()
        {
            if (this.Headers.GetContentType().ToLower() != "multipart/mixed") return Enumerable.Empty<MIMEPart>();
            var boundary = this.Headers.GetBoundary();
            return SplitByBoundary(this.Data, "--" + boundary, "--" + boundary + "--")
             .Select(lines => new MIMEPart(
                 SmtpHeaderExtension.CreateHeaders(lines.TakeWhile(l => l != "")),
                 lines.SkipWhile(l => l != "").Skip(1)
             ));
        }

        public SmtpAttachment ToAttachment()
        {
            var contentBytes = default(byte[]);
            var charSet = this.Headers.GetCharSet();
            var contentType = this.Headers.GetContentType();
            switch (contentType)
            {
                case "text/plain":
                    contentBytes = Encoding.GetEncoding(charSet).GetBytes(MIMEDecoder.DecodeTransfer(this.Headers, this.Data));
                    break;
                default:
                    contentBytes = MIMEDecoder.DecodeTransferToBytes(this.Headers, this.Data);
                    break;
            }

            var contentName = this.Headers.GetContentName();
            return new SmtpAttachment(contentBytes, contentName, contentType);
        }

        protected IEnumerable<string[]> SplitByBoundary(IEnumerable<string> texts, params string[] separators)
        {
            Func<string, bool> predicate = t => separators.Contains(t) == false;
            while (texts.Any())
            {
                var bound = texts.TakeWhile(predicate).ToArray();
                if (bound.Any(b => b != "")) yield return bound;
                texts = texts.SkipWhile(predicate).Skip(1);
            }
        }

        public void SaveAs(string saveToPath)
        {
            using (var writer = new StreamWriter(saveToPath, append: false, encoding: Encoding.ASCII))
            {
                foreach (var header in this.Headers)
                {
                    writer.WriteLine("{0}: {1}", header.Key, header.RawValues.First());
                    foreach (var v in header.RawValues.Skip(1)) writer.WriteLine(" " + v);
                }

                writer.WriteLine("");

                foreach (var data in this.Data)
                {
                    writer.WriteLine(data);
                }
            }
        }

        public void Load(string loadFromPath)
        {
            this.Headers.Clear();
            this.Data.Clear();

            Action<string> handler = (headerLine) =>
            {
                if (headerLine.IsNotNullOrEmpty()) this.Headers.Add(headerLine);
                else handler = (dataLine) => { this.Data.Add(dataLine); };
            };

            using (var reader = new StreamReader(loadFromPath, Encoding.ASCII))
            {
                while (reader.EndOfStream == false)
                {
                    var line = reader.ReadLine();
                    handler(line);
                }
            }
        }
    }
}
