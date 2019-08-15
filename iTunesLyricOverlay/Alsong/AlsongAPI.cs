using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace iTunesLyricOverlay.Alsong
{
    internal static class AlsongAPI
    {
        private const string EncData = "88cb6ef6ec728841b111f5a037ca2bff03549eb20a47b92a74d5fd52abda38eb81291d6043a68ee80494a040d512c8482b3cda866b5dfc6b6cc9b4c11e41fff552a1e2182039258b7818ea03d04bc75825a14a1a58132c3f3fe083bfa6110a6f7fae02ae9502495455f415b2a8c0c9fabb5357e4de9c5322f7304f94b4909b72";

        private static XmlDocument CallSoap(string soapAction, byte[] data)
        {
            var req = WebRequest.Create("http://lyrics.alsong.co.kr/alsongwebservice/service1.asmx") as HttpWebRequest;
            req.Method      = "POST";
            req.UserAgent   = "gSOAP/2.7";
            req.ContentType = "application/soap+xml; charset=utf-8";
            req.Headers.Set("SOAPAction", $"\"ALSongWebServer/{soapAction}\"");

            req.GetRequestStream().Write(data, 0, data.Length);

            using (var res = req.GetResponse())
            {
                using (var stream = res.GetResponseStream())
                {
                    var xml = new XmlDocument();
                    xml.Load(stream);

                    return xml;
                }
            }
        }

        public static AlsongLyric[] SearchByText(string artist, string title, int page)
        {
            try
            {
                var data = Encoding.UTF8.GetBytes($@"<?xml version=""1.0"" encoding=""UTF-8""?>
<SOAP-ENV:Envelope
xmlns:SOAP-ENV=""http://www.w3.org/2003/05/soap-envelope""
xmlns:SOAP-ENC=""http://www.w3.org/2003/05/soap-encoding""
xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
xmlns:ns2=""ALSongWebServer/Service1Soap""
xmlns:ns1=""ALSongWebServer""
xmlns:ns3=""ALSongWebServer/Service1Soap12"">
    <SOAP-ENV:Body>
        <ns1:GetResembleLyricList2>
            <ns1:encData>{EncData}</ns1:encData>
            <ns1:title>{new XText(title).ToString()}</ns1:title>
            <ns1:artist>{new XText(artist).ToString()}</ns1:artist>
            <ns1:pageNo>{page}</ns1:pageNo>
        </ns1:GetResembleLyricList2>
    </SOAP-ENV:Body>
</SOAP-ENV:Envelope>");

                return CallSoap("GetResembleLyricList2", data).SelectNodes("//*[local-name()='ST_SEARCHLYRIC_LIST']")?.Cast<XmlNode>().Select(e => AlsongLyric.Parse(e)).Where(e => e != null).ToArray();
            }
            catch
            {
                return null;
            }
        }
        
        public static AlsongLyric[] SearchByFile(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                    return null;

                var ext = Path.GetExtension(filePath);
                if (ext != ".mp3" && ext != ".ogg" && ext != ".wma" && ext != ".flac")
                    return null;

                var checkSum = GetChecksum(filePath);
                if (checkSum == null)
                    return null;

                Console.WriteLine(checkSum);

                var data = Encoding.UTF8.GetBytes($@"<?xml version=""1.0"" encoding=""UTF-8""?>
<SOAP-ENV:Envelope
xmlns:SOAP-ENV=""http://www.w3.org/2003/05/soap-envelope""
xmlns:SOAP-ENC=""http://www.w3.org/2003/05/soap-encoding""
xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
xmlns:ns2=""ALSongWebServer/Service1Soap""
xmlns:ns1=""ALSongWebServer""
xmlns:ns3=""ALSongWebServer/Service1Soap12"">
    <SOAP-ENV:Body>
        <ns1:GetLyric7>
            <ns1:encData>{EncData}</ns1:encData>
            <ns1:stQuery>
                <ns1:strChecksum>{checkSum}</ns1:strChecksum>
                <ns1:strVersion>3.46</ns1:strVersion>
                <ns1:strMACAddress></ns1:strMACAddress>
                <ns1:strIPAddress></ns1:strIPAddress>
            </ns1:stQuery>
        </ns1:GetLyric7>
    </SOAP-ENV:Body>
</SOAP-ENV:Envelope>");

                var lyric = AlsongLyric.Parse(CallSoap("GetLyric7", data).SelectSingleNode("//*[local-name()='GetLyric7Result']"));
                return lyric == null ? null : new AlsongLyric[] { lyric };
            }
            catch
            {
                return null;
            }
        }

        private static string GetChecksum(string filePath)
        {
            using (var fs = File.OpenRead(filePath))
            {
                var buff = new byte[4096];

                if (fs.Read(buff, 0, 20) != 20)
                    return null;

                if (
                    buff[0] == 'I' &&
                    buff[1] == 'D' &&
                    buff[2] == '3')
                {
                    if (!SkipID3Data(fs))
                        return null;
                }
                else
                    fs.Position = 0;

                using (var md5 = new MD5CryptoServiceProvider())
                {
                    int remain = 163840;
                    int read;
                    
                    while (remain > 0)
                    {
                        read = fs.Read(buff, 0, Math.Min(remain, 4096));
                        if (read == 0)
                            break;

                        remain -= read;

                        md5.TransformBlock(buff, 0, read, buff, 0);
                    }
                    md5.TransformFinalBlock(buff, 0, 0);

                    var sb = new StringBuilder();
                    foreach (byte b in md5.Hash)
                        sb.Append($"{b:x2}");
                    return sb.ToString();
                }
            }
        }

        private static bool SkipID3Data(Stream fs)
        {
            var id3Header = new byte[10];

            fs.Position = 0;
            if (fs.Read(id3Header, 0, 10) != 10)
                return false;
           
            fs.Position = 10 + (id3Header[6] << 21 | id3Header[7] << 14 | id3Header[8] << 7 | id3Header[9]);
            
            return true;
        }
        
        public static string GetRawLyric(string lyricID)
        {
            try
            {
                var data = Encoding.UTF8.GetBytes($@"<?xml version=""1.0"" encoding=""UTF-8""?>
<SOAP-ENV:Envelope
xmlns:SOAP-ENV=""http://www.w3.org/2003/05/soap-envelope""
xmlns:SOAP-ENC=""http://www.w3.org/2003/05/soap-encoding""
xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
xmlns:ns2=""ALSongWebServer/Service1Soap""
xmlns:ns1=""ALSongWebServer""
xmlns:ns3=""ALSongWebServer/Service1Soap12"">
    <SOAP-ENV:Body>
        <ns1:GetLyricByID2>
            <ns1:encData>{EncData}</ns1:encData>
            <ns1:lyricID>{lyricID}</ns1:lyricID>
        </ns1:GetLyricByID2>
    </SOAP-ENV:Body>
</SOAP-ENV:Envelope>");

                var xml = CallSoap("GetLyricByID2", data);

                if (xml.SelectSingleNode("//*[local-name()='GetLyricByID2Result']").InnerText != "true")
                    return null;

                return xml.SelectSingleNode("//*[local-name()='lyric']").InnerText;
            }
            catch
            {
                return null;
            }
        }
    }
}
