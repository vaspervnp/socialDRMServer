using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks.Dataflow;
using System.IO.Compression;
using System.Xml;
using System.Dynamic;
using Newtonsoft.Json;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Reflection.Metadata;
using System.IO.Pipes;

namespace socialDRMServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EpubController : ControllerBase
    {


        public class epubZipFile
        {
            public byte[] fileBytes;
            public string fileName;
            public string fullName;
            public string comment;
            public bool isDir = false;
        }


        [HttpPost("AddSocialDrm")]
        public string AddSocialDrm([FromForm] string epubSource, [FromForm] string socialName)
        {
            List<epubZipFile> FileList = new List<epubZipFile>();
            string epubFinal = "";
            Byte[] bytes = Convert.FromBase64String(epubSource);
            var epubStream = new MemoryStream(bytes);

            using (ZipArchive zip = new ZipArchive(epubStream, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    epubZipFile zf = new epubZipFile();
                    zf.fileName = entry.Name;
                    zf.comment = entry.Comment;
                    zf.fullName = entry.FullName;
                    if (zf.fullName.EndsWith("/")) { zf.isDir = true; }
                    if (entry.Name == "content.opf")
                    {

                        Stream s = entry.Open();
                        StreamReader sr = new StreamReader(s);
                        XmlDocument doc = new XmlDocument();
                        doc.Load(sr);
                        XmlNodeList nodes = doc.GetElementsByTagName("dc:title");
                        nodes[0].InnerText = nodes[0].InnerText + ", Owner:" + socialName;
                        MemoryStream xmlStream = new MemoryStream();
                        doc.Save(xmlStream);
                        zf.fileBytes = xmlStream.ToArray();
                    }
                    else
                    if (entry.Name == "titlepage.xhtml")
                    {
                        MemoryStream ms = new MemoryStream();
                        Stream s = entry.Open();
                        StreamReader sr = new StreamReader(s);
                        sr.BaseStream.CopyTo(ms);
                        string titleHtml = System.Text.Encoding.UTF8.GetString(ms.ToArray());
                        titleHtml = titleHtml.Replace("<body>", "<body>\r\n\t\t<div>\r\n\t\t<center><a>Owner: "+ socialName + "</a></center><br>\r\n\t\t<center><a>Social DRM https://socialdrm.vasper.eu</a></center>\r\n\t\t</div>\r\n\t\t").Replace("height: 100%", "height: 90%");
                        zf.fileBytes = System.Text.Encoding.UTF8.GetBytes(titleHtml);
                    }                    
                    else
                    {
                        MemoryStream ms = new MemoryStream();
                        Stream s = entry.Open();
                        StreamReader sr = new StreamReader(s);
                        sr.BaseStream.CopyTo(ms);
                        zf.fileBytes = ms.ToArray();
                    }
                    FileList.Add(zf);
                };
            }
            var epubStream2 = new MemoryStream();
            using (ZipArchive zip2 = new ZipArchive(epubStream2, ZipArchiveMode.Create, true))
            {
                foreach (epubZipFile zf in FileList)
                {
                    ZipArchiveEntry fileInArchive;
                    if (zf.isDir)
                    {
                        fileInArchive = zip2.CreateEntry(zf.fullName);
                    }
                    else
                    {
                        fileInArchive = zip2.CreateEntry(zf.fullName, CompressionLevel.Optimal);
                    }
                    using (Stream entryStream = fileInArchive.Open())
                    {
                        using (MemoryStream fileToCompressStream = new MemoryStream(zf.fileBytes))
                        {
                            fileToCompressStream.CopyTo(entryStream);
                        }
                        fileInArchive.Comment = zf.comment;
                    }

                }
            }
            epubFinal = Convert.ToBase64String(epubStream2.ToArray());
            return epubFinal;
        }

        [HttpPost("GetBookTitle")]
        public string GetBookTitle([FromForm] string epubSource)
        {
            Byte[] bytes = Convert.FromBase64String(epubSource);
            var epubStream = new MemoryStream(bytes);
            string title = "";
            using (ZipArchive zip = new ZipArchive(epubStream))
            {
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    Console.WriteLine(entry.Name);
                    string myStr = "";
                    if (entry.Name == "content.opf")
                    {
                        Stream s = entry.Open();
                        var sr = new StreamReader(s);
                        myStr = sr.ReadToEnd();
                        XDocument doc = XDocument.Parse(myStr);

                        string jsonText = JsonConvert.SerializeXNode(doc);
                        dynamic dyn = JsonConvert.DeserializeObject<dynamic>(jsonText);
                        title = dyn.package["opf:metadata"]["dc:title"].Value;
                    }
                };
            }
            return title;
        }
    }
}