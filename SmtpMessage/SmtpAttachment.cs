using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Toolbelt.Net.Smtp.Internal;

namespace Toolbelt.Net.Smtp
{
    public class SmtpAttachment
    {
        public string Name { get; set; }

        public string MediaType { get; set; }

        public byte[] ContentBytes { get; set; }

        public SmtpAttachment()
        {
            this.Name = "";
            this.MediaType = "";
            this.ContentBytes = new byte[0];
        }

        public SmtpAttachment(IEnumerable<byte> contentBytes, string name, string mediaType)
        {
            this.Name = MIMEDecoder.DecodeText(name);
            this.MediaType = mediaType;
            this.ContentBytes = contentBytes.ToArray();
        }
    }
}
