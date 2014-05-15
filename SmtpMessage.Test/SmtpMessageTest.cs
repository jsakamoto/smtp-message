using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Toolbelt.Net.Smtp.Test
{
    [TestClass]
    public class SmtpMessageTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void CharSet_Test()
        {
            var msg = new SmtpMessage();
            msg.Headers.Add("Content-Type", "text/plain; charset=us-ascii");
            msg.Headers.GetCharSet().Is("us-ascii");

            msg = new SmtpMessage();
            msg.Headers.Add("Content-Type", "text/plain; charset=\"ISO-2022-JP\"");
            msg.Headers.GetCharSet().Is("ISO-2022-JP");
        }

        [TestMethod]
        public void Body_Ascii_QuotedPrintable_Test()
        {
            var msg = new SmtpMessage();
            msg.Headers.Add("Content-Type", "text/plain; charset=us-ascii");
            msg.Headers.Add("Content-Transfer-Encoding", "quoted-printable");
            msg.Data.Add("Lorem \"ipsum\" 'dolor' =0D=0A=");
            msg.Data.Add("\tmagna aliqua=");
            msg.Data.Add(".. laborum.=20");

            msg.Body.Is(
                "Lorem \"ipsum\" 'dolor' \r\n" +
                "\tmagna aliqua. laborum. ");
        }

        [TestMethod]
        public void Body_Utf8_Base64_Test()
        {
            var msg = new SmtpMessage();
            msg.Headers.Add("Content-Type", "text/plain; charset=\"utf-8\"");
            msg.Headers.Add("Content-Transfer-Encoding", "base64");
            msg.Data.Add("44Gf44Go44GI44Gw44CB5LiA6Ie044GZ44KL6KGo57SZ44CB44OY44OD44OA44O844CB44K144Kk44OJ44OQ44O844KS6L+95Yqg");
            msg.Data.Add("44Gn44GN44G+44GZ44CCW+aMv+WFpV0g44KS44Kv44Oq44OD44Kv44GX44Gm44GL44KJ44CB44Gd44KM44Ge44KM44Gu44Ku44Oj");
            msg.Data.Add("44Op44Oq44O844Gn55uu55qE44Gu6KaB57Sg44KS6YG444KT44Gn44GP44Gg44GV44GE44CCDQrjg4bjg7zjg57jgajjgrnjgr/j");
            msg.Data.Add("gqTjg6vjgpLkvb/jgaPjgabjgIHmlofmm7jlhajkvZPjga7ntbHkuIDmhJ/jgpLlh7rjgZnjgZPjgajjgoLjgafjgY3jgb7jgZnj");
            msg.Data.Add("gII=");

            msg.Body.Is(
                "たとえば、一致する表紙、ヘッダー、サイドバーを追加できます。[挿入] をクリックしてから、それぞれのギャラリーで目的の要素を選んでください。\r\n" +
                "テーマとスタイルを使って、文書全体の統一感を出すこともできます。");
        }

        [TestMethod]
        public void Body_Iso2022jp_7bit_Test()
        {
            var msg = new SmtpMessage();
            msg.Headers.Add("Content-Type", "text/plain; charset=\"ISO-2022-JP\"");
            msg.Headers.Add("Content-Transfer-Encoding", "7bit");
            msg.Data.Add("\x1b$B$?$H$($P!\"0lCW$9$kI=;f!\"%X%C%@!<!\"%5%$%I%P!<$rDI2C$G$-$^$9!#\x1b(B[\x1b$BA^F~\x1b(B] \x1b$B$r%/%j%C%/$7$F$+$i");
            msg.Data.Add("!\"$=$l$>$l$N%.%c%i%j!<$GL\\E*$NMWAG$rA*$s$G$/$@$5$$!#\x1b(B\r\n\x1b$B%F!<%^$H%9%?%$%k$r;H$C$F!\"J8=qA4BN$NE}0l");
            msg.Data.Add("46$r=P$9$3$H$b$G$-$^$9!#\x1b(B");

            msg.Body.Is(
                "たとえば、一致する表紙、ヘッダー、サイドバーを追加できます。[挿入] をクリックしてから、それぞれのギャラリーで目的の要素を選んでください。\r\n" +
                "テーマとスタイルを使って、文書全体の統一感を出すこともできます。");
        }

        [TestMethod]
        public void Body_MultiPart_Test()
        {
            var msg = new SmtpMessage();
            msg.Headers.Add("Content-Type", "multipart/mixed;", "boundary=--boundary_0_73bba9c5-516c-4f50-b880-a0d19568b806");
            msg.Data.AddRange(new[] { 
                "",
                "----boundary_0_73bba9c5-516c-4f50-b880-a0d19568b806",
                "Content-Type: text/plain; charset=iso-2022-jp",
                "Content-Transfer-Encoding: quoted-printable",
                "",
                "=1B$BF|K\\8l=1B(B",
                "----boundary_0_73bba9c5-516c-4f50-b880-a0d19568b806",
                "Content-Type: text/plain; charset=us-ascii; name=mytext.txt",
                "Content-Disposition: attachment",
                "Content-Transfer-Encoding: quoted-printable",
                "",
                "This is sample text",
                "----boundary_0_73bba9c5-516c-4f50-b880-a0d19568b806--",
                "",
                ""});
            msg.Body.Is("日本語");
        }

        [TestMethod]
        public void Subject_Iso2022jp_Base64_Test()
        {
            var msg = new SmtpMessage();
            msg.Headers.Add("Subject", "=?iso-2022-jp?B?GyRCJDMkcyRLJEEk?=", "=?iso-2022-jp?B?T0AkMyYbKEI=?=");
            msg.Subject.Is("こんにちは世界");
        }

        [TestMethod]
        public void Subject_Iso2022jp_QuotedPrintable_Test()
        {
            var msg = new SmtpMessage();
            msg.Headers.Add("Subject", "=?iso-2022-jp?Q?=1B=24B=243=24s=24K=24A=24O=40=243=26=1B=28B?=");
            msg.Subject.Is("こんにちは世界");
        }

        [TestMethod]
        public void From_AddressOnly_Blacket_Test()
        {
            var msg = new SmtpMessage();
            msg.Headers.Add("From", "<anderson@example.com>");
            msg.From.DisplayName.Is("");
            msg.From.Address.Is("anderson@example.com");
        }

        [TestMethod]
        public void From_AddressOnly_NoBlacket_Test()
        {
            var msg = new SmtpMessage();
            msg.Headers.Add("From", "anderson@example.com");
            msg.From.DisplayName.Is("");
            msg.From.Address.Is("anderson@example.com");
        }

        [TestMethod]
        public void From_Ascii_7bit_Test()
        {
            var msg = new SmtpMessage();
            msg.Headers.Add("From", "Mr.Anderson <anderson@example.com>");
            msg.From.DisplayName.Is("Mr.Anderson");
            msg.From.Address.Is("anderson@example.com");
        }

        [TestMethod]
        public void From_Iso2022jp_Base64_Test()
        {
            var msg = new SmtpMessage();
            msg.Headers.Add("From", "=?iso-2022-jp?B?GyRCRnxLXDhsGyhCIBskQkJATzobKEI=?= <taro@example.com>");
            msg.From.DisplayName.Is("日本語 太郎");
            msg.From.Address.Is("taro@example.com");
        }

        [TestMethod]
        public void To_Test()
        {
            var msg = new SmtpMessage();
            msg.Headers.Add("To", @"""Mr.Anderson"" <anderson@example.com>, ""Mrs.Oracle"" <oracle@example.com>");
            msg.To.Select(_ => _.DisplayName)
                .Is("Mr.Anderson", "Mrs.Oracle");
            msg.To.Select(_ => _.Address)
                .Is("anderson@example.com", "oracle@example.com");
        }

        [TestMethod]
        public void To_Empty_Test()
        {
            var msg = new SmtpMessage();
            msg.To.Count().Is(0);
        }

        [TestMethod]
        public void CC_Test()
        {
            var msg = new SmtpMessage();
            msg.Headers.Add("CC", @"""Mr.Anderson"" <anderson@example.com>, ""Mrs.Oracle"" <oracle@example.com>");
            msg.CC.Select(_ => _.DisplayName)
                .Is("Mr.Anderson", "Mrs.Oracle");
            msg.CC.Select(_ => _.Address)
                .Is("anderson@example.com", "oracle@example.com");
        }

        [TestMethod]
        public void CC_Empty_Test()
        {
            var msg = new SmtpMessage();
            msg.CC.Count().Is(0);
        }

        [TestMethod]
        public void Date_Test()
        {
            var msg = new SmtpMessage();
            msg.Date.IsNull();
            msg.Headers.Add("Date", "2 Jan 2014 20:23:13 +0900");
            msg.Date.Is(DateTime.Parse("2014/1/2 20:23:13"));
        }

        [TestMethod]
        public void Attachment_Empty_Test()
        {
            var msg = new SmtpMessage();
            msg.Attachments.Count().Is(0);
        }

        private static SmtpMessage CreateTestMessage1()
        {
            var msg = new SmtpMessage();
            msg.Id = Guid.Parse("0a09ebcdd3544646b72325c743474a26");
            msg.MailFrom = "taro@example.com";
            msg.Headers.Add("Content-Type", "multipart/mixed; charset=us-ascii; boundary=--boundary--");
            msg.Headers.Add("Content-Transfer-Encoding", "7bit");
            msg.Headers.Add("From", "taro@example.com");
            msg.Headers.Add("To", @"""Hanako"" <hanako@example.com>, ""Jiro"" <jiro@example.com>");
            msg.Headers.Add("Date", "2 Jan 2014 20:23:13 +0900");
            msg.Headers.Add("Subject", "[HELLO WORLD]");
            msg.MailFrom = "taro@example.com";
            msg.RcptTo.Add("hanako@example.com");
            msg.RcptTo.Add("jiro@example.com");
            msg.Data.AddRange(new[]{
                "----boundary--",
                "Content-Type: text/plain; charset=us-ascii",
                "Content-Transfer-Encoding: 7bit",
                "",
                "Hello, World.",
                "----boundary--",
                "Content-Type: text/plain; charset=us-ascii; name=hello-world.txt",
                "Content-Transfer-Encoding: 7bit",
                "Content-Disposition: -",
                "",
                "Hello, World.",
                "----boundary----"
            });
            msg.Attachments.Count().Is(1);
            return msg;
        }

        [TestMethod]
        public void Can_Serialize_Deserialize_as_Json_by_JsonNET_Minimum_Test()
        {
            var msg = new SmtpMessage();
            msg.Headers.Add("Content-Type", "text/plain; charset=us-ascii");
            msg.Headers.Add("Content-Transfer-Encoding", "7bit");

            // Can serialize.
            var json = JsonConvert.SerializeObject(msg);
            Debug.WriteLine(json);

            // Can deserialize.
            var msg2 = JsonConvert.DeserializeObject<SmtpMessage>(json);
            msg2.MailFrom.IsNull();
            msg2.RcptTo.Count().Is(0);
            msg2.Headers.OrderBy(h => h.Key).Select(h => new { h.Key, h.Value }.ToString()).Is(
                "{ Key = Content-Transfer-Encoding, Value = 7bit }",
                "{ Key = Content-Type, Value = text/plain; charset=us-ascii }");
            msg2.Attachments.Count().Is(0);
        }

        [TestMethod]
        public void Can_Serialize_Deserialize_as_Json_by_JsonNET_Test()
        {
            var msg = CreateTestMessage1();

            // Can serialize.
            var json = JsonConvert.SerializeObject(msg);
            Debug.WriteLine(json);

            // Can deserialize.
            var msg2 = JsonConvert.DeserializeObject<SmtpMessage>(json);
            msg2.MailFrom.Is("taro@example.com");
            msg2.RcptTo.Is("hanako@example.com", "jiro@example.com");
            Assert_SmtpMessage(msg2);
        }

        private static void Assert_SmtpMessage(SmtpMessage msg)
        {
            msg.Headers.Select(h => h.Key).OrderBy(k => k).Is(
                "Content-Transfer-Encoding",
                "Content-Type",
                "Date",
                "From",
                "Subject",
                "To");
            msg.Headers.ValueOf("Content-Type").Is("multipart/mixed; charset=us-ascii; boundary=--boundary--");
            msg.Headers.ValueOf("Content-Transfer-Encoding").Is("7bit");
            msg.Headers.ValueOf("From").Is("taro@example.com");
            msg.Headers.ValueOf("To").Is(@"""Hanako"" <hanako@example.com>, ""Jiro"" <jiro@example.com>");
            msg.Headers.ValueOf("Date").Is("2 Jan 2014 20:23:13 +0900");
            msg.Headers.ValueOf("Subject").Is("[HELLO WORLD]");
            msg.Data.Is(
                "----boundary--",
                "Content-Type: text/plain; charset=us-ascii",
                "Content-Transfer-Encoding: 7bit",
                "",
                "Hello, World.",
                "----boundary--",
                "Content-Type: text/plain; charset=us-ascii; name=hello-world.txt",
                "Content-Transfer-Encoding: 7bit",
                "Content-Disposition: -",
                "",
                "Hello, World.",
                "----boundary----");
        }

        [TestMethod]
        public void SaveAs_Test()
        {
            var msg = CreateTestMessage1();
            var saveToPath = Path.Combine(this.TestContext.ResultsDirectory, "actual.eml");
            msg.SaveAs(saveToPath);

            var expected = File.ReadLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_Deploy", "expected.eml"));
            var actual = File.ReadLines(saveToPath);

            expected.Is(actual);
        }

        public static string PathOf(string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_Deploy", fileName);
        }

        [TestMethod]
        public void Load_Test()
        {
            var msg = new SmtpMessage();
            msg.Load(PathOf("expected.eml"));
            
            Assert_SmtpMessage(msg);
        }

        [TestMethod]
        public void Load_with_Attachment_Test()
        {
            var msg = SmtpMessage.CreateFrom(PathOf("mail-with-attachment.eml"));

            var body = msg.Body;
            msg.Attachments.Count().Is(2);
            msg.Attachments[0].Name.Is("Markdown Presenter.pdf");
            msg.Attachments[0].ContentBytes
                .Is(File.ReadAllBytes(PathOf("Markdown Presenter.pdf")));
            msg.Attachments[1].Name.Is("花.jpg");
            msg.Attachments[1].ContentBytes
                .Is(File.ReadAllBytes(PathOf("花.jpg")));
        }
    }
}
