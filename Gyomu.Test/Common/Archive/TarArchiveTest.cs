using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Gyomu.Test.Common.Archive
{
    public class TarArchiveTest
    {
        [Fact]
        public void TarArchiveSimpleTest()
        {
            archiveSimpleTest();
        }
        
        private static void archiveSimpleTest()
        {
            //Create sample file (text)
            string RootPath = System.IO.Path.GetTempPath();
            string TestFilePath = RootPath + System.IO.Path.DirectorySeparatorChar + "tarFolder";
            if (System.IO.Directory.Exists(TestFilePath))
                System.IO.Directory.Delete(TestFilePath,true);
            System.IO.Directory.CreateDirectory(TestFilePath);
            string TestFile1Name = "tarTest.txt";
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


            string TarFilename = "TarTest.tar";
            string TarFullpath = System.IO.Path.GetTempPath() + TarFilename;
            if (System.IO.File.Exists(TarFullpath))
                System.IO.File.Delete(TarFullpath);
            //Create tar file from this sample file
            Models.FileTransportInfo transport = new Models.FileTransportInfo(RootPath, "tarFolder", null, RootPath, TarFilename);
            Gyomu.Common.Archive.TarArchive.Create(  transport );
            System.IO.File.Delete(TestFile1FullPath);
            //Extract from the tar file and compare with original text file

            string TestExtractRootPath = RootPath + System.IO.Path.DirectorySeparatorChar + "tarExtractFolder";
            if (System.IO.Directory.Exists(TestExtractRootPath) == false)
                System.IO.Directory.CreateDirectory(TestExtractRootPath);

            transport = new Models.FileTransportInfo(null, RootPath, TarFilename, TestExtractRootPath, null);
            Gyomu.Common.Archive.TarArchive.Extract(transport);

            List<System.IO.FileInfo> fileInfoList= Gyomu.Access.FileOperationAccess.Search(TestExtractRootPath, new List<Models.FileFilterInfo>() {
                new Models.FileFilterInfo(Models.FileFilterInfo.FilterType.FileName, Models.FileFilterInfo.CompareType.Equal,TestFile1Name) }, true);

            Assert.NotNull(fileInfoList);
            Assert.NotEmpty(fileInfoList);
            Assert.Single(fileInfoList);
            System.IO.FileInfo fileInfo = fileInfoList[0];
            using (System.IO.StreamReader reader = new System.IO.StreamReader(fileInfo.FullName))
            {
                Assert.Equal(OriginalContent, reader.ReadToEnd());
            }
        }
        
    }
}
