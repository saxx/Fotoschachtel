using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Fotoschachtel.Common.ViewModels
{
    public class PicturesViewModel : BaseViewModel
    {
        public IEnumerable<Picture> Pictures { get; set; }
        public string Event { get; private set; }
        private Settings.SasToken _sasToken;


        public async Task Fill()
        {
            if (IsBusy)
            {
                return;
            }
            IsBusy = true;

            Event = Settings.Event;
            _sasToken = await Settings.GetSasToken();

            string xmlString;
            try
            {
                using (var httpClient = new HttpClient())
                {
                    xmlString = await httpClient.GetStringAsync(_sasToken.SasListUrl);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to list files in storage: " + ex.Message);
            }

            var pictures = new List<Picture>();
            try
            {
                var xml = XDocument.Parse(xmlString);
                // ReSharper disable PossibleNullReferenceException
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var blobNode in xml.Element("EnumerationResults").Element("Blobs").Elements("Blob"))
                {
                    var fileName = blobNode.Element("Name").Value;
                    if (fileName.StartsWith("thumbnails-small"))
                    {
                        fileName = fileName.Substring("thumbnails-small/".Length);
                        pictures.Add(new Picture(fileName)
                        {
                            SmallThumbnailUrl = $"{_sasToken.ContainerUrl}/thumbnails-small/{fileName}{_sasToken.SasQueryString}",
                            MediumThumbnailUrl = $"{_sasToken.ContainerUrl}/thumbnails-medium/{fileName}{_sasToken.SasQueryString}",
                            DateTime = DateTime.Parse(blobNode.Element("Properties").Element("Last-Modified").Value)
                        });
                    }
                }
                // ReSharper restore PossibleNullReferenceException
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to parse XML with files list: " + ex.Message);
            }
            Pictures = pictures.OrderByDescending(x => x.DateTime);

            IsBusy = false;
        }


        public class Picture
        {
            public Picture(string fileName)
            {
                FileName = fileName;
            }


            public string FileName { get; }
            public string SmallThumbnailUrl { get; set; }
            public string MediumThumbnailUrl { get; set; }
            public DateTime DateTime { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as Picture;
                if (other == null)
                {
                    return false;
                }
                return other.FileName == FileName;
            }

            public override int GetHashCode()
            {
                return FileName.GetHashCode();
            }
        }
    }
}

