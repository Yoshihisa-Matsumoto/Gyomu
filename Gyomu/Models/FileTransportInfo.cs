using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Models
{
    public class FileTransportInfo
    {

        internal string SourceFileName { get; set; }
        internal string SourceFolderName { get; set; }
        internal string BasePath { get; private set; }
        internal bool SourceIsDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(SourceFileName))
                    return true;
                return false;

            }
        }

        internal string DestinationFileName { get; set; }
        internal string DestinationFolderName { get; set; }
        internal bool DestinationIsDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(DestinationFileName))
                    return true;
                return false;

            }
        }
        internal bool DestinationIsRoot
        {
            get
            {
                if (string.IsNullOrEmpty(BasePath))
                {
                    if (string.IsNullOrEmpty(SourceFolderName) && string.IsNullOrEmpty(DestinationFolderName))
                        return true;

                }
                else
                {
                    if (string.IsNullOrEmpty(DestinationFolderName) && string.IsNullOrEmpty(SourceFolderName))
                        return true;
                }
                return false;
            }
        }

        internal string SourceFullName
        {
            get
            {
                string part2 = string.IsNullOrEmpty(SourceFolderName)
                    ? "" : SourceFolderName + System.IO.Path.DirectorySeparatorChar;

                return part2 + SourceFileName;
            }
        }
        internal string SourceFullNameWithBasePath
        {
            get
            {
                string part1 = string.IsNullOrEmpty(BasePath)
                                  ? ""
                                  : BasePath + System.IO.Path.DirectorySeparatorChar;
                return part1 + SourceFullName;
            }
        }

        internal string DestinationFullName
        {
            get
            {
                if (string.IsNullOrEmpty(DestinationFileName) == false && string.IsNullOrEmpty(DestinationFolderName) == false)
                {
                    return DestinationFolderName + System.IO.Path.DirectorySeparatorChar + DestinationFileName;
                }
                else if (string.IsNullOrEmpty(DestinationFolderName) == false)
                {
                    if (SourceIsDirectory)
                        return DestinationFolderName;
                    else
                    {
                        return DestinationFolderName + System.IO.Path.DirectorySeparatorChar + SourceFileName;
                    }
                }
                else if (string.IsNullOrEmpty(DestinationFileName) == false)
                {
                    if (DestinationIsRoot)
                        return DestinationFileName;
                    else
                        return SourceFolderName + System.IO.Path.DirectorySeparatorChar + DestinationFileName;
                }
                else
                {
                    if (DestinationIsRoot)
                    {
                        if (SourceIsDirectory)
                            return null;
                        else
                            //return SourceFileName;
                            return null;
                    }

                    else
                    {
                        if (SourceIsDirectory)
                            return SourceFolderName;
                        else
                            return SourceFolderName;
                    }
                }            }
        }
        internal string DestinationFullNameWithBasePath
        {
            get {
                string part1 = string.IsNullOrEmpty(BasePath)
                                ? ""
                                : BasePath + System.IO.Path.DirectorySeparatorChar;
                return part1 + DestinationFullName;
            }
        }

        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="strBasePath"></param>
        ///<param name="srcPath"></param>
        ///<param name="srcFile"></param>
        ///<param name="destPath"></param>
        ///<param name="destFile"></param>
        /// <remarks>
        /// Base	SrcPath	SrcFile	DestPath	DestFile	Where to Where
        /// @	    @	    @	    @	        @	        Base\SrcPath\SrcFile->DestPath\DestFile
        /// @	    @	    @	    @	        	        Base\SrcPath\SrcFile->DestPath
        /// @	    @	    @		            @	        Base\SrcPath\SrcFile->SrcPath\DestFile
        /// @	    @	    @		            	        Base\SrcPath\SrcFile->SrcPath
        /// @	    @		        @	        	        Base\SrcPath->DestFolder
        /// @	    @			                	        Base\SrcPath->SrcPath
        /// @		    	        	        	        Base->Root?
        /// @		    	        @	        	        Base->DestFolder
        /// @		        @	    @	        @	        Base\SrcFile->DestPath\DestFile
        /// @		        @	    @	        	        Base\SrcFile->DestPath
        /// @		        @	    	        @	        Base\SrcFile->Root.DestFile
        /// @		        @	    	        	        Base\SrcFile->Root
        /// 			    		
	    ///         @	    @	    @	        @	        SrcPath\SrcFile->DestPath\DestFile
	    ///         @	    @	    @	        	        SrcPath\SrcFile->DestPath
	    ///         @	    @	    	        @	        SrcPath\SrcFile->Root.DestFile
	    ///         @		        @	        	        SrcPath->DestPath
	    ///         @			                	        SrcPath->Root
	    ///                 @	    @	        @	        SrcFile->DestPath\DestFile
	    ///                 @	    @	        	        SrcFile->DestPath
	    ///                 @	    	        @	        SrcFile->DestFile
		///                 @	    	        	        SrcFile->Root
		///                 @	    @	        @	        SrcFile->DestPath\DestFile
		///                 @	    	        @	        SrcFile->Root\DestFile
        ///                 @	    @	        	        SrcFile->DestPath
        /// </remarks>
        public FileTransportInfo(string strBasePath, string srcPath, string srcFile, string destPath, string destFile,bool deleteSourceAfterProcess=false,bool overwriteDestination=false)
        {
            BasePath = strBasePath;
            SourceFileName = srcFile;
            SourceFolderName = srcPath;
            DestinationFileName = destFile;
            DestinationFolderName = destPath;
            DeleteSourceAfterProcess = deleteSourceAfterProcess;
            OverwriteDestination = overwriteDestination;

            if (string.IsNullOrEmpty(DestinationFolderName))
                DestinationFolderName = SourceFolderName;
            if (string.IsNullOrEmpty(DestinationFileName))
                DestinationFileName = SourceFileName;
        }
        public bool DeleteSourceAfterProcess { get; private set; }
        public bool OverwriteDestination { get; private set; }
    }
}
