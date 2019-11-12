using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Gyomu.Test.Common.Archive
{
    public class GZipArchiveTest
    {
        [Fact]
        public void GZipSimpleArchive()
        {
            //Create sample file (text)
            string RootPath = System.IO.Path.GetTempPath();
            string TestFile1Name = "gzipTest.txt";
            string TestFile1FullPath = RootPath + TestFile1Name;
            if (System.IO.File.Exists(TestFile1FullPath))
                System.IO.File.Delete(TestFile1FullPath);

            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(TestFile1FullPath))
            {
                writer.WriteLine("TEST Data aabcdfkdfaa");
                writer.WriteLine("1234566");
                writer.Flush();
            }
            string OriginalContent = null;
            using (System.IO.StreamReader reader = new System.IO.StreamReader(TestFile1FullPath))
            {
                OriginalContent = reader.ReadToEnd();
            }


            string GZipFilename = "GZipTest.gz";
            string GZipFullpath = RootPath + GZipFilename;
            if (System.IO.File.Exists(GZipFullpath))
                System.IO.File.Delete(GZipFullpath);
            //Create zip file from this sample file
            Models.FileTransportInfo transport = new Models.FileTransportInfo(null, RootPath, TestFile1Name, RootPath, GZipFilename);
            Gyomu.Common.Archive.GZipArchive.Create(transport);
            System.IO.File.Delete(TestFile1FullPath);
            //Extract from the zip file and compare with original text file
            transport = new Models.FileTransportInfo(null, RootPath, GZipFilename, RootPath, TestFile1Name);
            Gyomu.Common.Archive.GZipArchive.Extract(transport);
            using (System.IO.StreamReader reader = new System.IO.StreamReader(TestFile1FullPath))
            {
                Assert.Equal(OriginalContent, reader.ReadToEnd());
            }
            System.IO.File.Delete(TestFile1FullPath);
            using (System.IO.Stream st = Gyomu.Common.Archive.GZipArchive.Extract(GZipFullpath))
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(st))
                {
                    Assert.Equal(OriginalContent, reader.ReadToEnd());
                }
            }

            System.IO.File.Delete(GZipFullpath);

        }
    }
}
