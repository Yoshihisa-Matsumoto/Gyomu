using System;
using System.Collections.Generic;
using System.Text;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Tar;
using System.Linq;

namespace Gyomu.Common.Archive
{
    public class TarArchive
    {
        internal static void Create(Models.FileTransportInfo fileTransferInfo)
        {
            
            if (fileTransferInfo.SourceIsDirectory == false)
                throw new InvalidOperationException("File archive is not supported. Need to support folder only");
            using (System.IO.Stream outStream = System.IO.File.Create(fileTransferInfo.DestinationFullName))
            {
                using (ICSharpCode.SharpZipLib.Tar.TarArchive tarArchive = ICSharpCode.SharpZipLib.Tar.TarArchive.CreateOutputTarArchive(outStream))
                {
                    tarArchive.RootPath = fileTransferInfo.BasePath.Replace(@"\", "/");
                    if (tarArchive.RootPath.EndsWith("/"))
                        tarArchive.RootPath = tarArchive.RootPath.Remove(tarArchive.RootPath.Length - 1);

                    AddDirectoryFilesToTar(tarArchive, fileTransferInfo.SourceFullNameWithBasePath, true);

                    tarArchive.Close();
                }
            }
        }
        private static void AddDirectoryFilesToTar(ICSharpCode.SharpZipLib.Tar.TarArchive tarArchive, string sourceDirectory, bool recurse)
        {
            TarEntry tarEntry = TarEntry.CreateEntryFromFile(sourceDirectory);
            tarArchive.WriteEntry(tarEntry, false);

            string[] filenames = System.IO.Directory.GetFiles(sourceDirectory);
            foreach (string filename in filenames)
            {
                tarEntry = TarEntry.CreateEntryFromFile(filename);
                tarArchive.WriteEntry(tarEntry, true);
            }

            if (recurse)
            {
                string[] directories = System.IO.Directory.GetDirectories(sourceDirectory);
                foreach (string directory in directories)
                    AddDirectoryFilesToTar(tarArchive, directory, recurse);
            }
        }

        internal static void Extract(System.IO.Stream tarStream, Models.FileTransportInfo fileTransferInfo)
        {
            using (TarInputStream tarIn = new TarInputStream(tarStream))
            {
                TarEntry tarEntry = null;
                while ((tarEntry = tarIn.GetNextEntry()) != null)
                {
                    if (tarEntry.IsDirectory)
                        continue;

                    string name = tarEntry.Name.Replace('/', System.IO.Path.DirectorySeparatorChar);
                    if (System.IO.Path.IsPathRooted(name))
                        name = name.Substring(System.IO.Path.GetPathRoot(name).Length);

                    string outName = System.IO.Path.Combine(fileTransferInfo.DestinationFullNameWithBasePath, name);
                    string directoryName = System.IO.Path.GetDirectoryName(outName);
                    System.IO.Directory.CreateDirectory(directoryName);

                    System.IO.FileStream outStr = new System.IO.FileStream(outName, System.IO.FileMode.Create);
                    tarIn.CopyEntryContents(outStr);
                    outStr.Close();
                    DateTime myDt = DateTime.SpecifyKind(tarEntry.ModTime, DateTimeKind.Utc);
                    System.IO.File.SetLastWriteTimeUtc(outName, myDt);
                }
                tarIn.Close();
            }
        }
        internal static void Extract(Models.FileTransportInfo fileTransferInfo)
        {
            using (System.IO.FileStream fsIn = new System.IO.FileStream(fileTransferInfo.SourceFullNameWithBasePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                Extract(fsIn, fileTransferInfo);
            }
        }
    }
}
