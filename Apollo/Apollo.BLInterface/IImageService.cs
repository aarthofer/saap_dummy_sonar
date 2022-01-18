using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.BLInterface
{
    public interface IImageService
    {
        Task GetImageAsync(string name, BinaryWriter writer);
        Task GetDefaultImage(BinaryWriter writer);
        void StoreImage(string fileName, byte[] vs);
    }
}
