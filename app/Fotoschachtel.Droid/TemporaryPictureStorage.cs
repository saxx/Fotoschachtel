using System;
using System.IO;
using Fotoschachtel.Common;
using Fotoschachtel.Droid;
using Xamarin.Forms;

[assembly: Dependency(typeof(TemporaryPictureStorage))]

namespace Fotoschachtel.Droid
{
    public class TemporaryPictureStorage : ITemporaryPictureStorage
    {
        public void Save(string fileName, Stream stream)
        {
            using (var fileStream = File.Create(GetFullPath(fileName)))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
            }
        }


        public Stream Load(string fileName)
        {
            return File.Open(GetFullPath(fileName), FileMode.Open, FileAccess.Read);
        }


        public void Delete(string fileName)
        {
            File.Delete(GetFullPath(fileName));
        }


        public string GetFullPath(string fileName)
        {
            var directory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            return Path.Combine(directory, fileName);
        }
    }
}