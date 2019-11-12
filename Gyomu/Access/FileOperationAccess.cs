using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;

namespace Gyomu.Access
{
    public class FileOperationAccess : IDisposable
    {
        public static bool CanAccess(string filename, bool isReadonly = false)
        {
            if (System.IO.File.Exists(filename))
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(filename);
                string[] SpecialExtensions = new string[] { "XLS", "XLSM", "XLSX" };
                if (SpecialExtensions.Contains(fi.Extension.ToUpper()) && fi.Length == 0)
                    return false;
                if (isReadonly)
                    return true;

                try
                {

                    using (System.IO.FileStream fs = System.IO.File.Open(filename, System.IO.FileMode.Open, System.IO.FileAccess.Write, System.IO.FileShare.None))
                    {

                    }
                    return true;
                }
                catch (Exception) { }


            }
            return false;
        }

        private string lockFilename = null;
        private AutoResetEvent lockEvent = null;
        public static Dictionary<string, AutoResetEvent> dictLock = new Dictionary<string, AutoResetEvent>();
        public static FileOperationAccess LockInsideProcess(string filename)
        {
            FileOperationAccess fileAccess = new FileOperationAccess();
            System.IO.FileInfo fileInformation = new System.IO.FileInfo(filename);
            fileAccess.lockFilename = fileInformation.FullName.ToUpper();
            lock (dictLock)
            {
                if (dictLock.ContainsKey(fileAccess.lockFilename) == false)
                {
                    fileAccess.lockEvent = new AutoResetEvent(false);
                    dictLock.Add(fileAccess.lockFilename, fileAccess.lockEvent);
                }
                else
                {
                    fileAccess.lockEvent = dictLock[fileAccess.lockFilename];
                    fileAccess.lockEvent.WaitOne();
                }
            }
            return fileAccess;
        }
        public void Dispose()
        {
            if(lockEvent!=null)
                lockEvent.Set();
        }

        public static List<FileInfo> Search(string parentDirectory, List<Models.FileFilterInfo> filterConditions, bool recursive)
        {
            List<FileInfo> result = new List<FileInfo>();
            DirectoryInfo parent = new DirectoryInfo(parentDirectory);
            if (parent.Exists == false)
                return result;

            FileInfo[] lstFiles = null;
            if (recursive)
            {
                lstFiles = parent.GetFiles("*", SearchOption.AllDirectories);
            }
            else
            {
                lstFiles = parent.GetFiles("*", SearchOption.TopDirectoryOnly);
            }

            foreach (FileInfo fileInformation in lstFiles)
            {

                if (filterConditions == null || isFileValid(fileInformation, filterConditions))
                    result.Add(fileInformation);
            }


            return result;
        }

        private static bool isFileValid(FileInfo fileInformation, List<Models.FileFilterInfo> filterConditions)
        {
            bool isMatch = true;
            foreach (Models.FileFilterInfo filterInfo in filterConditions)
            {
                if (isFileValid(fileInformation, filterInfo) == false)
                {
                    isMatch = false;
                    break;
                }
            }
            return isMatch;
        }

        private static bool isFileValid(FileInfo fileInformation, Models.FileFilterInfo filter)
        {
            switch (filter.Kind)
            {
                case Models.FileFilterInfo.FilterType.FileName:
                    return isFileNameMatch(fileInformation.Name, filter.NameFilter, filter.Operator);
                case Models.FileFilterInfo.FilterType.CreateTimeUtc:
                    return isFileDateMatch(fileInformation.CreationTimeUtc, filter.TargetDate, filter.Operator);

                case Models.FileFilterInfo.FilterType.LastAccessTimeUtc:
                    return isFileDateMatch(fileInformation.LastAccessTimeUtc, filter.TargetDate, filter.Operator);
                case Models.FileFilterInfo.FilterType.LastWriteTimeUtc:
                    return isFileDateMatch(fileInformation.LastWriteTimeUtc, filter.TargetDate, filter.Operator);
            }
            return true;

        }

        private static bool isFileNameMatch(string fileName, string targetFilter, Models.FileFilterInfo.CompareType comparer)
        {
            switch (comparer)
            {
                case Models.FileFilterInfo.CompareType.Equal:
                    return Regex.IsMatch(fileName, targetFilter);
                case Models.FileFilterInfo.CompareType.Larger:
                    return fileName.CompareTo(targetFilter) > 0;
                case Models.FileFilterInfo.CompareType.LargerEqual:
                    return fileName.CompareTo(targetFilter) >= 0;
                case Models.FileFilterInfo.CompareType.Less:
                    return fileName.CompareTo(targetFilter) < 0;
                case Models.FileFilterInfo.CompareType.LessEqual:
                    return fileName.CompareTo(targetFilter) <= 0;
                default:
                    return false;
            }
        }

        private static bool isFileDateMatch(DateTime fileDate, DateTime targetDate, Models.FileFilterInfo.CompareType comparer)
        {
            switch (comparer)
            {
                case Models.FileFilterInfo.CompareType.Equal:
                    return fileDate.Equals(targetDate);
                case Models.FileFilterInfo.CompareType.Larger:
                    return fileDate > targetDate;
                case Models.FileFilterInfo.CompareType.LargerEqual:
                    return fileDate >= targetDate;
                case Models.FileFilterInfo.CompareType.Less:
                    return fileDate < targetDate;
                case Models.FileFilterInfo.CompareType.LessEqual:
                    return fileDate <= targetDate;
                default:
                    return false;

            }
        }

        public enum ArchiveType
        {
            Zip,
            TGZ,
            BZip2,
            GZip,
            Tar,
            GuessFromFileName
        }
        public static StatusCode Archive(string archiveFilename,ArchiveType archiveType,List<Models.FileTransportInfo> SourceFiles, Common.Configurator Config,short ApplicationID,string password=null)
        {
            if(SourceFiles==null || SourceFiles.Count==0)
                return StatusCode.InvalidArgumentStatus("Source File Not Specified to archive", Config, ApplicationID);

            FileInfo archiveInfo = new System.IO.FileInfo(archiveFilename);
            if (archiveType== ArchiveType.GuessFromFileName)
            {
                switch(archiveInfo.Extension.ToUpper())
                {
                    case ".ZIP":
                        archiveType = ArchiveType.Zip;
                        break;
                    case ".TGZ":
                        archiveType = ArchiveType.TGZ;
                        break;
                    case ".BZ2":
                        archiveType = ArchiveType.BZip2;
                        break;
                    case ".GZ":
                        archiveType = ArchiveType.GZip;
                        break;
                    case ".TAR":
                        archiveType = ArchiveType.Tar;
                        break;
                    default:
                        return StatusCode.InvalidArgumentStatus("File Extension Not supported for archiving", Config, ApplicationID);
                }
            }
            if(archiveType==ArchiveType.BZip2 || archiveType == ArchiveType.GZip)
            {
                if(SourceFiles.Count>1 || SourceFiles.Where(f=>f.SourceIsDirectory).Count()>0)
                    return StatusCode.InvalidArgumentStatus("Multiple files are not supported in this compression type", Config, ApplicationID);
            }
            if(archiveType!= ArchiveType.Zip && string.IsNullOrEmpty(password) == false)
            {
                return StatusCode.InvalidArgumentStatus("password is not supported on other than zip format", Config, ApplicationID);
            }
            Models.FileTransportInfo transport = SourceFiles[0];
            string archiveFileDirectory = archiveInfo.Directory.FullName;
            string archiveName = archiveInfo.Name;
            switch (archiveType)
            {
                case ArchiveType.Zip:
                    Common.Archive.ZipArchive.Create(archiveFilename, SourceFiles,password);
                    break;
                case ArchiveType.BZip2:
                    transport.DestinationFolderName = archiveFileDirectory;
                    transport.DestinationFileName = archiveName;
                    Common.Archive.BZip2Archive.Create(transport);
                    break;
                case ArchiveType.GZip:
                    transport.DestinationFolderName = archiveFileDirectory;
                    transport.DestinationFileName = archiveName;
                    Common.Archive.GZipArchive.Create(transport);
                    break;
                case ArchiveType.Tar:
                    transport.DestinationFolderName = archiveFileDirectory;
                    transport.DestinationFileName = archiveName;
                    Common.Archive.TarArchive.Create(transport);
                    break;
                case ArchiveType.TGZ:
                    transport.DestinationFolderName = Path.GetTempPath();
                    transport.DestinationFileName = archiveName.Substring(0, archiveName.Length - (archiveInfo.Extension.Length)) + ".tar";
                    Common.Archive.TarArchive.Create(transport);
                    Models.FileTransportInfo tgzInfo = new Models.FileTransportInfo(null, transport.DestinationFolderName, transport.DestinationFileName, archiveFileDirectory, archiveName);
                    Common.Archive.GZipArchive.Create(tgzInfo);
                    break;
            }
                



            return StatusCode.SUCCEED_STATUS;
        }
        public static StatusCode UnarchiveAllAndCreateDestinationFolderIfNotExist(string archiveFilename, ArchiveType archiveType, Models.FileTransportInfo transport, Common.Configurator Config, short ApplicationID, string password = null)
        {
            if (transport == null)
                return StatusCode.InvalidArgumentStatus("Transportation Information Not Specified to archive", Config, ApplicationID);

            FileInfo archiveInfo = new System.IO.FileInfo(archiveFilename);
            if (archiveType == ArchiveType.GuessFromFileName)
            {
                switch (archiveInfo.Extension.ToUpper())
                {
                    case ".ZIP":
                        archiveType = ArchiveType.Zip;
                        break;
                    case ".TGZ":
                        archiveType = ArchiveType.TGZ;
                        break;
                    case ".BZ2":
                        archiveType = ArchiveType.BZip2;
                        break;
                    case ".GZ":
                        archiveType = ArchiveType.GZip;
                        break;
                    case ".TAR":
                        archiveType = ArchiveType.Tar;
                        break;
                    default:
                        return StatusCode.InvalidArgumentStatus("File Extension Not supported for archiving", Config, ApplicationID);
                }
            }

            if (archiveType == ArchiveType.BZip2 || archiveType == ArchiveType.GZip)
            {
                if (transport.SourceIsDirectory)
                    return StatusCode.InvalidArgumentStatus("Multiple files are not supported in this compression type", Config, ApplicationID);
            }
            if (archiveType != ArchiveType.Zip && string.IsNullOrEmpty(password) == false)
            {
                return StatusCode.InvalidArgumentStatus("password is not supported on other than zip format", Config, ApplicationID);
            }
            string archiveFileDirectory = archiveInfo.Directory.FullName;
            string archiveName = archiveInfo.Name;

            System.IO.Directory.CreateDirectory(transport.DestinationFolderName);
            switch (archiveType)
            {
                case ArchiveType.Zip:
                    transport.SourceFolderName = archiveFileDirectory;
                    transport.SourceFileName = archiveName;
                    Common.Archive.ZipArchive.Unzip(transport);
                        break;
                case ArchiveType.BZip2:
                    transport.SourceFolderName = archiveFileDirectory;
                    transport.SourceFileName = archiveName;
                    Common.Archive.BZip2Archive.Extract(transport);
                    break;
                case ArchiveType.GZip:
                    transport.SourceFolderName = archiveFileDirectory;
                    transport.SourceFileName = archiveName;
                    Common.Archive.GZipArchive.Extract(transport);
                    break;
                case ArchiveType.Tar:
                    transport.SourceFolderName = archiveFileDirectory;
                    transport.SourceFileName = archiveName;
                    Common.Archive.TarArchive.Extract(transport);
                    break;
                case ArchiveType.TGZ:
                    transport.SourceFolderName = archiveFileDirectory;
                    transport.SourceFileName = archiveName;
                    string originalDestinationFolder = transport.DestinationFolderName;
                    string originalDestinationFile = transport.DestinationFileName;
                    transport.DestinationFolderName = Path.GetTempPath();
                    transport.DestinationFileName = archiveName.Substring(0, archiveName.Length - (archiveInfo.Extension.Length)) + ".tar";
                    Common.Archive.GZipArchive.Extract(transport);
                    Models.FileTransportInfo tarInfo = new Models.FileTransportInfo(null, transport.DestinationFolderName, transport.DestinationFileName, originalDestinationFolder, originalDestinationFile);
                    Common.Archive.TarArchive.Extract(tarInfo);
                    File.Delete(transport.DestinationFullName);
                    break;
            }
            return StatusCode.SUCCEED_STATUS;
        }
    }
}
