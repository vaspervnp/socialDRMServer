using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks.Dataflow;
using System.IO.Compression;
using System.Xml;
using System.Dynamic;
using Newtonsoft.Json;
using System.Xml.Linq;

namespace socialDRMServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EpubController : ControllerBase
    {

        
        [HttpPost("AddSocialDrm")]
        public string AddSocialDrm([FromForm] string epubSource, [FromForm] string socialName)
        {
            string epubFinal = "test";
            Byte[] bytes = Convert.FromBase64String(epubSource);
            var epubStream = new MemoryStream(bytes);
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
                        //XElement tempElement = doc.Descendants("opf" + "metadata").FirstOrDefault();
                        
                        
                    }
                    Console.WriteLine(myStr);
                };
            }


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