using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Models
{
    public class FilePath
    {
        public string DirectoryPath { get; set; }
        public Dictionary<string, string> Filenames { get; set; }

        public Dictionary<string, string> FullPathList
        {
            get
            {
                Dictionary<string, string> lstData = new Dictionary<string, string>();
                string strDir = DirectoryPath;
                if (strDir.EndsWith(@"\") || strDir.EndsWith("/"))
                { }
                else if (strDir.EndsWith(@"\") == false && strDir.Contains(@"\"))
                    strDir += @"\";
                else if (strDir.EndsWith(@"/") == false && strDir.Contains("/"))
                    strDir += "/";
                else
                    strDir += @"\";
                foreach (string key in Filenames.Keys)
                    lstData.Add(key, strDir + Filenames[key]);

                return lstData;
            }
        }
    }
}
