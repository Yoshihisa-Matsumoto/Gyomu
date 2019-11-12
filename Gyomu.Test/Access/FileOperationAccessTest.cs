using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Linq;

namespace Gyomu.Test.Access
{
    public class FileOperationAccessTest
    {
        [Fact]
        public void FileLockTest()
        {
            string RootPath = System.IO.Path.GetTempPath();
            string TestFile1Name = "fileLockTest.txt";
            string TestFile1FullPath = RootPath  + TestFile1Name;
            if (System.IO.File.Exists(TestFile1FullPath))
                System.IO.File.Delete(TestFile1FullPath);

            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(TestFile1FullPath))
            {
                writer.WriteLine("TEST Data aabcdfkdfaa");
                writer.WriteLine("1234566");
                writer.Flush();
            }
            DateTime dateBeforeLock = DateTime.Now;
            System.Threading.Tasks.Task.Run(() =>
            {
                using (Gyomu.Access.FileOperationAccess access = Gyomu.Access.FileOperationAccess.LockInsideProcess(TestFile1FullPath))
                {
                    System.Threading.Thread.Sleep(3000);
                }
            });
            System.Threading.Thread.Sleep(1000);
            using (Gyomu.Access.FileOperationAccess access = Gyomu.Access.FileOperationAccess.LockInsideProcess(TestFile1FullPath))
            {
                DateTime dateAfterLock = DateTime.Now;
                Assert.True(dateAfterLock.Subtract(dateBeforeLock).TotalSeconds > 1);
            }
            if (System.IO.File.Exists(TestFile1FullPath))
                System.IO.File.Delete(TestFile1FullPath);
        }

        [Fact]
        public void FileAccessibilityTest()
        {
            string RootPath = System.IO.Path.GetTempPath();
            string TestFile1Name = "fileAccessibilityTest.txt";
            string TestFile1FullPath = RootPath + TestFile1Name;
            if (System.IO.File.Exists(TestFile1FullPath))
                System.IO.File.Delete(TestFile1FullPath);

            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(TestFile1FullPath))
            {
                writer.WriteLine("TEST Data aabcdfkdfaa");
                writer.WriteLine("1234566");
                writer.Flush();
            }

            Assert.True(Gyomu.Access.FileOperationAccess.CanAccess(TestFile1FullPath));
            System.Threading.Tasks.Task.Run(() =>
            {
                using (System.IO.Stream stream = System.IO.File.OpenRead(TestFile1FullPath))
                {
                    System.Threading.Thread.Sleep(3000);
                }
            });
            System.Threading.Thread.Sleep(1000);
            Assert.False(Gyomu.Access.FileOperationAccess.CanAccess(TestFile1FullPath));
            Assert.True(Gyomu.Access.FileOperationAccess.CanAccess(TestFile1FullPath,true));
        }
        [Fact]
        public void FileArchiveTest()
        {
            string RootPath = System.IO.Path.GetTempPath();
            string TestFileDirectory = "archiveFolder";
            string TestFilePath = RootPath + System.IO.Path.DirectorySeparatorChar + TestFileDirectory;
            if (System.IO.Directory.Exists(TestFilePath) == false)
                System.IO.Directory.CreateDirectory(TestFilePath);
            string TestFile1Name = "archiveTest1.txt";
            string TestFile1FullPath = TestFilePath + System.IO.Path.DirectorySeparatorChar + TestFile1Name;
            if (System.IO.File.Exists(TestFile1FullPath))
                System.IO.File.Delete(TestFile1FullPath);
            Gyomu.Common.Configurator config = Gyomu.Common.BaseConfigurator.GetInstance();
            config.ApplicationID = Common.GyomuDataAccessTest.testApplicationId;
            short testApplicationID = Common.GyomuDataAccessTest.testApplicationId;
            #region Init part
            string TestFile2Name = "archiveTest2.txt";
            string TestFile2FullPath = TestFilePath + System.IO.Path.DirectorySeparatorChar + TestFile2Name;
            if (System.IO.File.Exists(TestFile2FullPath))
                System.IO.File.Delete(TestFile2FullPath);

            string TestFileSubPath = TestFilePath + System.IO.Path.DirectorySeparatorChar + "zipSubFolder";
            if (System.IO.Directory.Exists(TestFileSubPath) == false)
                System.IO.Directory.CreateDirectory(TestFileSubPath);
            string TestFile3Name = "archiveTest3.txt";
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
            #endregion
            
            Gyomu.Models.FileTransportInfo transport = new Models.FileTransportInfo(TestFilePath, null, null, null, null);
            string ZipFilename = RootPath + "zipArchive.zip";
            if (System.IO.File.Exists(ZipFilename))
                System.IO.File.Delete(ZipFilename);
            StatusCode retVal =  Gyomu.Access.FileOperationAccess.Archive(ZipFilename, Gyomu.Access.FileOperationAccess.ArchiveType.GuessFromFileName, new List<Models.FileTransportInfo>() { transport }, config, testApplicationID);
            Assert.True(retVal.IsSucceeded);

            string ZipExtractPath = RootPath + System.IO.Path.DirectorySeparatorChar + "zipArchiveFolder";
            if (System.IO.Directory.Exists(ZipExtractPath))
                System.IO.Directory.Delete(ZipExtractPath, true);
            Models.FileTransportInfo zipTransport = new Models.FileTransportInfo(null, null, null, ZipExtractPath, null);

            retVal = Gyomu.Access.FileOperationAccess.UnarchiveAllAndCreateDestinationFolderIfNotExist(ZipFilename, Gyomu.Access.FileOperationAccess.ArchiveType.GuessFromFileName, zipTransport, config, testApplicationID);
            Assert.True(retVal.IsSucceeded);

            compareFile(TestFilePath, ZipExtractPath);

            #region Tar

            transport = new Models.FileTransportInfo(TestFilePath, null, null, null, null);
            string TarFilename = RootPath + "tarArchive.tar";
            if (System.IO.File.Exists(TarFilename))
                System.IO.File.Delete(TarFilename);
            retVal = Gyomu.Access.FileOperationAccess.Archive(TarFilename, Gyomu.Access.FileOperationAccess.ArchiveType.GuessFromFileName, new List<Models.FileTransportInfo>() { transport }, config, testApplicationID);
            Assert.True(retVal.IsSucceeded);

            string TarExtractPath = RootPath + System.IO.Path.DirectorySeparatorChar + "tarArchiveFolder";
            if (System.IO.Directory.Exists(TarExtractPath))
                System.IO.Directory.Delete(TarExtractPath, true);
            Models.FileTransportInfo tarTransport = new Models.FileTransportInfo(null, null, null, TarExtractPath, null);

            retVal = Gyomu.Access.FileOperationAccess.UnarchiveAllAndCreateDestinationFolderIfNotExist(TarFilename, Gyomu.Access.FileOperationAccess.ArchiveType.GuessFromFileName, tarTransport, config, testApplicationID);
            Assert.True(retVal.IsSucceeded);

            compareFile(TestFilePath, TarExtractPath);

            #endregion
            #region TGZ

            transport = new Models.FileTransportInfo(TestFilePath, null, null, null, null);
            string TgzFilename = RootPath + "tgzArchive.tgz";
            if (System.IO.File.Exists(TgzFilename))
                System.IO.File.Delete(TgzFilename);
            retVal = Gyomu.Access.FileOperationAccess.Archive(TgzFilename, Gyomu.Access.FileOperationAccess.ArchiveType.GuessFromFileName, new List<Models.FileTransportInfo>() { transport }, config, testApplicationID);
            Assert.True(retVal.IsSucceeded);

            string TgzExtractPath = RootPath + System.IO.Path.DirectorySeparatorChar + "tgzArchiveFolder";
            if (System.IO.Directory.Exists(TgzExtractPath))
                System.IO.Directory.Delete(TgzExtractPath, true);
            Models.FileTransportInfo tgzTransport = new Models.FileTransportInfo(null, null, null, TgzExtractPath, null);

            retVal = Gyomu.Access.FileOperationAccess.UnarchiveAllAndCreateDestinationFolderIfNotExist(TgzFilename, Gyomu.Access.FileOperationAccess.ArchiveType.GuessFromFileName, tgzTransport, config, testApplicationID);
            Assert.True(retVal.IsSucceeded);

            compareFile(TestFilePath, TgzExtractPath);

            #endregion

            #region GZip

            transport = new Models.FileTransportInfo(TestFilePath, null, TestFile1Name, null, null);
            string GzipFilename = RootPath + "gzArchive.gz";
            if (System.IO.File.Exists(GzipFilename))
                System.IO.File.Delete(GzipFilename);
            retVal = Gyomu.Access.FileOperationAccess.Archive(GzipFilename, Gyomu.Access.FileOperationAccess.ArchiveType.GuessFromFileName, new List<Models.FileTransportInfo>() { transport }, config, testApplicationID);
            Assert.True(retVal.IsSucceeded);

            string GzipExtractPath = RootPath + System.IO.Path.DirectorySeparatorChar + "gzArchiveFolder";
            if (System.IO.Directory.Exists(GzipExtractPath))
                System.IO.Directory.Delete(GzipExtractPath, true);
            Models.FileTransportInfo gzipTransport = new Models.FileTransportInfo(null, null, GzipFilename, GzipExtractPath, TestFile1Name);

            retVal = Gyomu.Access.FileOperationAccess.UnarchiveAllAndCreateDestinationFolderIfNotExist(GzipFilename, Gyomu.Access.FileOperationAccess.ArchiveType.GuessFromFileName, gzipTransport, config, testApplicationID);
            Assert.True(retVal.IsSucceeded);

            compareFile(TestFile1FullPath, GzipExtractPath + System.IO.Path.DirectorySeparatorChar+ TestFile1Name);

            #endregion

            #region BZip2

            transport = new Models.FileTransportInfo(TestFilePath, null, TestFile1Name, null, null);
            string BZip2Filename = RootPath + "bzip2Archive.gz";
            if (System.IO.File.Exists(BZip2Filename))
                System.IO.File.Delete(BZip2Filename);
            retVal = Gyomu.Access.FileOperationAccess.Archive(BZip2Filename, Gyomu.Access.FileOperationAccess.ArchiveType.GuessFromFileName, new List<Models.FileTransportInfo>() { transport }, config, testApplicationID);
            Assert.True(retVal.IsSucceeded);

            string Bzip2ExtractPath = RootPath + System.IO.Path.DirectorySeparatorChar + "bzip2ArchiveFolder";
            if (System.IO.Directory.Exists(Bzip2ExtractPath))
                System.IO.Directory.Delete(Bzip2ExtractPath, true);
            Models.FileTransportInfo bzip2Transport = new Models.FileTransportInfo(null, null, BZip2Filename, Bzip2ExtractPath, TestFile1Name);

            retVal = Gyomu.Access.FileOperationAccess.UnarchiveAllAndCreateDestinationFolderIfNotExist(BZip2Filename, Gyomu.Access.FileOperationAccess.ArchiveType.GuessFromFileName, bzip2Transport, config, testApplicationID);
            Assert.True(retVal.IsSucceeded);

            compareFile(TestFile1FullPath, Bzip2ExtractPath + System.IO.Path.DirectorySeparatorChar + TestFile1Name);

            #endregion

        }

        private void compareFile(string source,string destination)
        {
            bool isSourceDirectory = System.IO.File.GetAttributes(source).HasFlag(System.IO.FileAttributes.Directory);
            bool isDestinationDirectory = System.IO.File.GetAttributes(destination).HasFlag(System.IO.FileAttributes.Directory);
            Assert.Equal(isSourceDirectory, isDestinationDirectory);

            if (isSourceDirectory == false)
            {
                string sourceContent = null;
                using (System.IO.StreamReader reader = new System.IO.StreamReader(source))
                {
                    sourceContent = reader.ReadToEnd();
                }
                using (System.IO.StreamReader reader = new System.IO.StreamReader(destination))
                {
                    Assert.Equal(sourceContent, reader.ReadToEnd());
                }
            }
            else
            {
                List<string> sourceFiles = System.IO.Directory.GetFiles(source, "*", System.IO.SearchOption.AllDirectories).ToList();
                List<string> destinationFiles = System.IO.Directory.GetFiles(destination, "*", System.IO.SearchOption.AllDirectories).ToList();
                foreach(string sourceFilename in sourceFiles)
                {
                    string sourceFilenameonly = new System.IO.FileInfo(sourceFilename).Name;
                    string destinationFile = destinationFiles.Where(f => f.EndsWith(sourceFilenameonly)).FirstOrDefault();
                    Assert.NotNull(destinationFile);
                    compareFile(sourceFilename, destinationFile);
                }
            }

        }
    }
}
