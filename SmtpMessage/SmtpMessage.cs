using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using Toolbelt.Net.Smtp.Internal;

namespace Toolbelt.Net.Smtp
{
    public class SmtpMessage
    {
        protected MIMEPart _MIMEPart;

        public List<SmtpHeader> Headers { get { return _MIMEPart.Headers; } }
        
        public List<string> Data { get { return _MIMEPart.Data; } }

        public Guid Id { get; set; }

        public string MailFrom { get; set; }

        public List<string> RcptTo { get; set; }

        public string Body
        {
            get
            {
                var contentType = this.Headers.GetContentType();
                switch (contentType.ToLower())
                {
                    case "text/plain":
                        return MIMEDecoder.DecodeTransfer(this.Headers, this.Data);
                    case "multipart/mixed":
                        var bodyPart = _MIMEPart.GetMultiParts().First(p => p.Headers.GetContentDisposition() == "");
                        return MIMEDecoder.DecodeTransfer(bodyPart.Headers, bodyPart.Data);
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private SmtpAttachment[] _Attachments;

        public SmtpAttachment[] Attachments
        {
            get
            {
                if (_Attachments == null)
                {
                    _Attachments = _MIMEPart.GetMultiParts()
                        .Where(p => p.Headers.GetContentDisposition() != "")
                        .Select(p => p.ToAttachment())
                        .ToArray();
                }
                return _Attachments;
            }
        }

        public string Subject { get { return MIMEDecoder.DecodeText(this.Headers.ValueOf("Subject")); } }

        public MailAddress From { get { return new MailAddress(MIMEDecoder.DecodeText(this.Headers.ValueOf("From"))); } }

        public MailAddress[] To { get { return HeaderToMailAddresses("To"); } }

        public MailAddress[] CC { get { return HeaderToMailAddresses("CC"); } }

        public MailAddress[] ReplyTo { get { return HeaderToMailAddresses("Reply-To"); } }

        private MailAddress[] HeaderToMailAddresses(string headerName)
        {
            return MIMEDecoder.DecodeText(this.Headers.ValueOf(headerName))
                .SplitAndTrim(',')
                .Select(a => new MailAddress(a))
                .ToArray();
        }

        public SmtpMessage()
        {
            this._MIMEPart = new MIMEPart();
            this.Id = Guid.NewGuid();
            this.RcptTo = new List<string>();
        }

        public void SaveAs(string saveToPath)
        {
            this._MIMEPart.SaveAs(saveToPath);
        }

        public void Load(string loadFromPath)
        {
            this._MIMEPart.Load(loadFromPath);
        }
    }
}
