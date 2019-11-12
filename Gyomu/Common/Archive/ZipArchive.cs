using System;
using System.Collections.Generic;
using System.Text;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System.Linq;
using System.IO;

namespace Gyomu.Common.Archive
{
    internal class ZipArchive:IDisposable
    {
        public ZipArchive(string zipFilePath, string password = null)
        {
            zipObject = new ZipFile(zipFilePath);
            zipObject.UseZip64 = UseZip64.Dynamic;
            if (!string.IsNullOrEmpty(password))
            {
                zipObject.Password = password;
            }
        }
        private ZipArchive() { }
        ZipFile zipObject { get; set; }
        internal static void Create(string zipFilePath,List<Models.FileTransportInfo> fileTransferInfoList, string password = null)
        {
            ZipArchive archive = new ZipArchive()
            {
                zipObject = ZipFile.Create(zipFilePath)
            };
            using (archive)
            {
                archive.zipObject.UseZip64 = UseZip64.Dynamic;
                if (!string.IsNullOrEmpty(password))
                {
                    archive.zipObject.Password = password;
                }
                archive.zipObject.BeginUpdate();

                foreach (Models.FileTransportInfo fileInfo in fileTransferInfoList)
                    archive.Archive(fileInfo);
                archive.zipObject.CommitUpdate();
            }
        }
        internal static void Unzip( Models.FileTransportInfo fileTransferInfo, string password = null)
        {            
            ZipArchive archive = new ZipArchive()
            {
                zipObject = new ZipFile(fileTransferInfo.SourceFullNameWithBasePath)
            };
            using (archive)
            {
                archive.zipObject.UseZip64 = UseZip64.Dynamic;
                if (!string.IsNullOrEmpty(password))
                {
                    archive.zipObject.Password = password;
                }
                //if (fileTransferInfo.SourceFullName == null) { 
                    foreach (ZipEntry zipEntry in archive.zipObject)
                    {
                        if (!zipEntry.IsFile)
                            continue;

                        string entryFilename = zipEntry.Name;
                        var fullZipToPath = System.IO.Path.Combine(fileTransferInfo.DestinationFolderName, entryFilename);
                        var directoryName = System.IO.Path.GetDirectoryName(fullZipToPath);
                        if (directoryName.Length > 0)
                            System.IO.Directory.CreateDirectory(directoryName);

                        var buffer = new byte[4096];

                        using (Stream fsOutput = File.Create(fullZipToPath))
                        {
                            StreamUtils.Copy(archive.GetEntryFileFromName(entryFilename), fsOutput, buffer);
                        }
                            
                    }
                //}
            }
        }
        internal System.IO.Stream GetEntryFileFromFile(Models.FileTransportInfo info)
        {
            if (info == null)
                return null;
            if (info.SourceIsDirectory)
                return null;
            return GetEntryFileFromName(getEntryFileFromSourceZip(info));
        }
        internal System.IO.Stream GetEntryFileFromName(string entryName)
        {
            if (zipObject == null)
                return null;
            ZipEntry entry = zipObject.GetEntry(entryName);
            if (entry == null)
                return null;
            return zipObject.GetInputStream(entry);
        }
        internal List<string> GetEntryFileNamesFromDirectory(Models.FileTransportInfo info)
        {

            List<string> lstEntry = new List<string>();
            if (info.SourceIsDirectory == false)
                return lstEntry;
            if (zipObject == null)
                return lstEntry;

            string searchEntryPrefix = ZipEntry.CleanName(info.SourceFolderName) + "/";
            foreach(ZipEntry entry in zipObject)
            {
                if (entry.IsDirectory)
                    continue;
                if (entry.Name.StartsWith(searchEntryPrefix))
                    lstEntry.Add(entry.Name);
            }
            return lstEntry;
        }
        private static string getEntryFileFromSourceZip(Models.FileTransportInfo info)
        {
            return ZipEntry.CleanName(info.SourceFullName);
        }
        internal bool FileExists(string targetFilename)
        {
            string entryName = ZipEntry.CleanName(targetFilename);
            if (zipObject != null)
                return zipObject.GetEntry(entryName) != null;
            foreach(ZipEntry entry in zipObject)
            {
                if (!entry.IsFile)
                    continue;
                
            }
            return false;
        }
        internal bool DirectoryExists(string entryPath)
        {
            return false;
        }
        internal void Archive(Models.FileTransportInfo fileTransportInfo) {
            
            if (fileTransportInfo.SourceIsDirectory == false)
            {
                string entryName = ZipEntry.CleanName(fileTransportInfo.DestinationFolderName + @"\" + fileTransportInfo.DestinationFileName);
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(fileTransportInfo.SourceFullNameWithBasePath);

                zipObject.Add(fileInfo.FullName, entryName);
            }
            else
            {
                string[] files = System.IO.Directory.GetFiles(fileTransportInfo.SourceFullNameWithBasePath, "*", System.IO.SearchOption.AllDirectories);
                ZipNameTransform nameTransform = new ZipNameTransform(fileTransportInfo.BasePath);
                foreach(string filename in files)
                {
                    string entryName = nameTransform.TransformFile(filename);
                    zipObject.Add(filename, entryName);
                }
                
            }

        }
        public void Dispose()
        {
            using (zipObject) { }
        }
    }
}
