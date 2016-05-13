using System.Diagnostics.Contracts;
using System.IO;

namespace Fotoschachtel.Common
{
    public interface ITemporaryPictureStorage
    {
        void Save(string fileName, Stream stream);
        Stream Load(string fileName);
        void Delete(string fileName);
        string GetFullPath(string fileName);
    }
}
