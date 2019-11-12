using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
namespace Gyomu.Test.Common.Archive
{
    public class ZipArchiveTest
    {
        [Fact]
        public void ZipArchiveSimpleTest()
        {
            archiveSimpleTest();
        }
        [Fact]
        public void ZipArchiveDirectoryTest()
        {
            archiveDirectoryTest();
        }
        private static void archiveSimpleTest()
        {
            //Create sample file (text)
            string RootPath = System.IO.Path.GetTempPath();
            string TestFilePath = RootPath + System.IO.Path.DirectorySeparatorChar + "zipFolder";
            if (System.IO.Directory.Exists(TestFilePath) == false)
                System.IO.Directory.CreateDirectory(TestFilePath);
            string TestFile1Name = "zipTest.txt";
            string TestFile1FullPath = TestFilePath + System.IO.Path.DirectorySeparatorChar + TestFile1Name;
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


            string ZipFilename = System.IO.Path.GetTempPath() + "ZipTest.zip";
            if (System.IO.File.Exists(ZipFilename))
                System.IO.File.Delete(ZipFilename);
            //Create zip file from this sample file
            Models.FileTransportInfo transport = new Models.FileTransportInfo(RootPath, "zipFolder", TestFile1Name,null,null);
            Gyomu.Common.Archive.ZipArchive.Create(ZipFilename,new List<Models.FileTransportInfo>() { transport });
            System.IO.File.Delete(TestFile1FullPath);
            //Extract from the zip file and compare with original text file
            using (Gyomu.Common.Archive.ZipArchive archive = new Gyomu.Common.Archive.ZipArchive(ZipFilename))
            {
                System.IO.Stream stream = archive.GetEntryFileFromFile(transport);
                using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
                    Assert.Equal(OriginalContent, reader.ReadToEnd());
            }
            System.IO.File.Delete(ZipFilename);
        }
        private static void archiveDirectoryTest()
        {
            //Create sample file (text)
            string RootPath = System.IO.Path.GetTempPath();
            string TestFilePath = RootPath + System.IO.Path.DirectorySeparatorChar + "zipFolder2";
            if (System.IO.Directory.Exists(TestFilePath) == false)
                System.IO.Directory.CreateDirectory(TestFilePath);
            string TestFile1Name = "zipTest1.txt";
            string TestFile1FullPath = TestFilePath + System.IO.Path.DirectorySeparatorChar + TestFile1Name;
            if (System.IO.File.Exists(TestFile1FullPath))
                System.IO.File.Delete(TestFile1FullPath);
            string TestFile2Name = "zipTest2.txt";
            string TestFile2FullPath = TestFilePath + System.IO.Path.DirectorySeparatorChar + TestFile2Name;
            if (System.IO.File.Exists(TestFile2FullPath))
                System.IO.File.Delete(TestFile2FullPath);

            string TestFileSubPath = TestFilePath + System.IO.Path.DirectorySeparatorChar + "zipSubFolder";
            if (System.IO.Directory.Exists(TestFileSubPath) == false)
                System.IO.Directory.CreateDirectory(TestFileSubPath);
            string TestFile3Name = "zipTest3.txt";
            string TestFile3FullPath = TestFileSubPath + System.IO.Path.DirectorySeparatorChar + TestFile3Name;
            if (System.IO.File.Exists(TestFile3FullPath))
                System.IO.File.Delete(TestFile3FullPath);

            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(TestFile1FullPath))
            {
                writer.WriteLine("TEST Data aabcdfkdfaa");
                writer.WriteLine("1234566");
                writer.Flush();
            }
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(TestFile2FullPath))
            {
                writer.WriteLine("TEST Data a22343afda");
                writer.WriteLine("1234566");
                writer.Flush();
            }
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(TestFile3FullPath))
            {
                writer.WriteLine("TEST Data 322343afda");
                writer.WriteLine("1234566");
                writer.Flush();
            }
            List<string> OriginalContents = new List<string>();
            using (System.IO.StreamReader reader = new System.IO.StreamReader(TestFile1FullPath))
            {
                OriginalContents.Add( reader.ReadToEnd());
            }
            using (System.IO.StreamReader reader = new System.IO.StreamReader(TestFile2FullPath))
            {
                OriginalContents.Add(reader.ReadToEnd());
            }
            using (System.IO.StreamReader reader = new System.IO.StreamReader(TestFile3FullPath))
            {
                OriginalContents.Add(reader.ReadToEnd());
            }

            string ZipFilename = System.IO.Path.GetTempPath() + "ZipFolderTest.zip";
            if (System.IO.File.Exists(ZipFilename))
                System.IO.File.Delete(ZipFilename);
            //Create zip file from this sample file
            Models.FileTransportInfo transport = new Models.FileTransportInfo(RootPath, "zipFolder2", null, null, null);
            Gyomu.Common.Archive.ZipArchive.Create(ZipFilename, new List<Models.FileTransportInfo>() { transport });

            using (Gyomu.Common.Archive.ZipArchive archive = new Gyomu.Common.Archive.ZipArchive(ZipFilename))
            {
                List<string> entryNames = archive.GetEntryFileNamesFromDirectory(transport);
               foreach(string entryName in entryNames)
                {
                    System.IO.Stream stream = archive.GetEntryFileFromName(entryName);
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
                        Assert.Contains(reader.ReadToEnd(), OriginalContents);
                }
            }
            //System.IO.File.Delete(TestFile1FullPath);
            //System.IO.File.Delete(ZipFilename);
        }
    }
}
