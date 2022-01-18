using Apollo.BLInterface;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.BL
{
    public class ImageService : IImageService
    {
        private const string DEFAULT_IMAGE_NAME = "default.png";
        private readonly Microsoft.Extensions.Configuration.IConfiguration configuration;
        private DirectoryInfo rootPath;

        public ImageService(string rootPath)
        {
            if (string.IsNullOrEmpty(rootPath))
            {
                throw new ArgumentOutOfRangeException($"'{nameof(rootPath)}' cannot be null or empty", nameof(rootPath));
            }

            // remove if last char is \ makes problems in docker container
            if(rootPath[rootPath.Length-1] == '\\')
            {
                rootPath = rootPath.Substring(0, rootPath.Length - 1);
            }
            this.rootPath = new DirectoryInfo(rootPath);
            if (!this.rootPath.Exists)
            {
                throw new ArgumentOutOfRangeException("Invalid image root path" + this.rootPath.FullName);
            }
        }

        public async Task GetDefaultImage(BinaryWriter writer)
        {
            var fi = new FileInfo(Path.Combine(rootPath.FullName, DEFAULT_IMAGE_NAME));
            if (!fi.Exists)
            {
                throw new ArgumentOutOfRangeException($"File not found {DEFAULT_IMAGE_NAME}");
            }

            using (var fs = fi.OpenRead())
            {
                fs.CopyTo(writer.BaseStream);
            }
        }

        public async Task GetImageAsync(string name, BinaryWriter writer)
        {
            var fi = new FileInfo(Path.Combine(rootPath.FullName, name));
            if (!fi.Exists)
            {
                throw new ArgumentOutOfRangeException($"File not found {name}");
            }

            using (var fs = fi.OpenRead())
            {
                fs.CopyTo(writer.BaseStream);
            }
        }

        public void StoreImage(string fileName, byte[] content)
        {
            var fi = new FileInfo(Path.Combine(rootPath.FullName, fileName));
            if (fi.Exists)
            {
                throw new ArgumentOutOfRangeException($"File already exists {fileName}");
            }

            using (var fs = fi.Create())
            {
                fs.Write(content);
            }
        }
    }
}
