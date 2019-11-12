using System;
using System.Collections.Generic;
using System.Text;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.GZip;
using System.Linq;

namespace Gyomu.Common.Archive
{
    public class GZipArchive
    {
        internal static void Create(Models.FileTransportInfo fileTransferInfo)
        {
            GZip.Compress(System.IO.File.OpenRead(fileTransferInfo.SourceFullNameWithBasePath), System.IO.File.Create(fileTransferInfo.DestinationFullName), true);
        }
        internal static System.IO.Stream Extract(string gzipFilePath)
        {
            return new GZipInputStream(System.IO.File.OpenRead(gzipFilePath));
        }
        internal static void Extract(Models.FileTransportInfo fileTransferInfo)
        {
            GZip.Decompress(System.IO.File.OpenRead(fileTransferInfo.SourceFullName), System.IO.File.OpenWrite(fileTransferInfo.DestinationFullName), true);
        }
    }
}
