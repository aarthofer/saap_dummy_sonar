using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Apollo.BL.Test
{
    public class ImageServiceTest
    {
        [Theory]
        [InlineData("images/")]
        [InlineData("images")]
        public async System.Threading.Tasks.Task TestLoadImageAsync(string rootPath)
        {
            ImageService imageService = new ImageService(rootPath);
            using (BinaryWriter bw = new BinaryWriter(new BufferedStream(new MemoryStream())))
            {
                await imageService.GetImageAsync("testimage.jpeg", bw);
                Assert.Equal(222515, bw.BaseStream.Length);

                //bw.BaseStream.Position = 0;
                //var fs = new FileStream(@"c:\tmp\testimage.jpeg", FileMode.OpenOrCreate);
                //bw.BaseStream.CopyTo(fs);
                //fs.Close();
            }
        }

        [Theory]
        [InlineData("/images/")]
        [InlineData("")]
        [InlineData(null)]
        public async System.Threading.Tasks.Task TestInvalidRootPath(string rootPath)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ImageService(rootPath));
        }

        [Theory]
        [InlineData("images/", "notExisting.jpeg")]
        [InlineData("images/", "testimage.jpg")]
        public async System.Threading.Tasks.Task TestInvalidFile(string rootPath, string filename)
        {
            ImageService imageService = new ImageService(rootPath);
            using (BinaryWriter bw = new BinaryWriter(new BufferedStream(new MemoryStream())))
            {
                await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => imageService.GetImageAsync(filename, bw));
            }
        }
    }
}
