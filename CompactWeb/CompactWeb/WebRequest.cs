//------------------------------------------------------------------------------
// WebRequest.cs
//
// http://bansky.net
//
//------------------------------------------------------------------------------
using System;
using System.IO;

namespace CompactWeb
{
    /// <summary>
    /// Class implements web request to the web server
    /// </summary>
    public struct WebRequest
    {
        #region Fields
        private string _directory;
        private string _fileName;
        private string _queryString;
        #endregion

        /// <summary>
        /// Directory of the requested file
        /// </summary>
        public string Directory
        {
            get { return _directory; }
            set { _directory = value; }
        }

        /// <summary>
        /// Requested file name
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }

        /// <summary>
        /// Query string
        /// </summary>
        public string QueryString
        {
            get { return _queryString; }
            set { _queryString = value; }
        }

        /// <summary>
        /// Full physical path to the requested file
        /// </summary>
        public string FullPath
        {
            get { return Path.Combine(Directory, FileName); }
        }
    }
}
