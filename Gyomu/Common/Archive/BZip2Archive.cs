using System;
using System.Collections.Generic;
using System.Text;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.BZip2;
using System.Linq;

namespace Gyomu.Common.Archive
{
    internal class BZip2Archive 
    {
        internal static void Create( Models.FileTransportInfo fileTransferInfo)
        {            
            BZip2.Compress(System.IO.File.OpenRead(fileTransferInfo.SourceFullName), System.IO.File.Create(fileTransferInfo.DestinationFullName), true, 9);
        }
        internal static System.IO.Stream Extract(string bzipFilePath)
        {
             return new BZip2InputStream(System.IO.File.OpenRead(bzipFilePath));
        }
        internal static void Extract(Models.FileTransportInfo fileTransferInfo)
        {
            BZip2.Decompress(System.IO.File.OpenRead(fileTransferInfo.SourceFullName), System.IO.File.OpenWrite(fileTransferInfo.DestinationFullName), true);
        }
    }
}
