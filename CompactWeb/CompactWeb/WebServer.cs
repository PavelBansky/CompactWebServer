//------------------------------------------------------------------------------
// WebServer.cs
//
// http://bansky.net
//
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Collections.Specialized;

namespace CompactWeb
{
    /// <summary>
    /// Class implements functionality of the simple web server
    /// </summary>
    public class WebServer
    {
        #region Fields
        TcpClient tcpClient;
        TcpListener tcpListener;
        Thread mainThread;
        bool serverStop = false;
        bool running = false;
        WebServerConfiguration Configuration;
        #endregion

        /// <summary>
        /// New WebServer
        /// </summary>
        /// <param name="webServerConf">WebServer Configuration</param>
        public WebServer(WebServerConfiguration webServerConf)
        {
            this.Configuration = webServerConf;
        }

        /// <summary>
        /// Starts the WebServer thread
        /// </summary>
        public void Start()
        {
            try
            {
                tcpListener = new TcpListener(Configuration.IPAddress, Configuration.Port);
                tcpListener.Start();                
                mainThread = new Thread(new ThreadStart(StartListen));
                serverStop = false;
                mainThread.Start();
                running = true;
                RaiseLogEvent(LogEventType.ServerStart, string.Empty);
            }
            catch (Exception e)
            {
                RaiseLogEvent(LogEventType.ServerStart, e.ToString());
            }
        }

        /// <summary>
        /// Stops the WebServer thread
        /// </summary>
        public void Stop()
        {
            try
            {
                if (mainThread != null)
                {
                    serverStop = true;
                    tcpListener.Stop();
                    mainThread.Join(1000);
                    running = false;
                    RaiseLogEvent(LogEventType.ServerStop, string.Empty);
                }
            }
            catch (Exception e)
            {
                RaiseLogEvent(LogEventType.ServerException, e.ToString());
            }
        }

        /// <summary>
        /// Start listening on port
        /// </summary>
        private void StartListen()
        {
            while (!serverStop)
            {
                try { tcpClient = tcpListener.AcceptTcpClient(); }
                catch { }

                if (tcpClient != null && tcpClient.Client.Connected)
                {
                    StreamReader streamReader = new StreamReader(tcpClient.GetStream());

                    RaiseLogEvent(LogEventType.ClientConnect, tcpClient.Client.RemoteEndPoint.ToString());

                    // Read full request with client header
                    StringBuilder receivedData = new StringBuilder();
                    while (streamReader.Peek() > -1)
                        receivedData.Append(streamReader.ReadLine());

                    string request = GetRequest(receivedData.ToString());
                    WebRequest webRequest = ParseRequest(request);

                    if (!SuportedMethod(request))
                    {
                        SendError(StatusCode.BadRequest, "Only GET is supported.");
                    }
                    else if (!Directory.Exists(webRequest.Directory))
                    {
                        SendError(StatusCode.NotFound, "Directory not found.");
                    }
                    else if (Directory.Exists(webRequest.Directory) && string.IsNullOrEmpty(webRequest.FileName))
                    {
                        SendError(StatusCode.Forbiden, "Directory does not allow contents to be listed.");
                    }
                    else if (!File.Exists(webRequest.FullPath))
                    {
                        SendError(StatusCode.NotFound, "File not found.");
                    }
                    else
                    {
                        // Handle the correct request

                        string mimeType = Configuration.GetMimeType(Path.GetExtension(webRequest.FileName));

                        // Test if the filename is registered file and give control to delegate
                        if (Configuration.IsSpecialFileType(webRequest.FileName) && OnSpecialFileType != null)
                        {
                            Stream oStream;
                            OnSpecialFileType(webRequest, out oStream);
                            oStream.Position = 0;

                            SendHeader(mimeType, oStream.Length, StatusCode.OK);
                            SendStream(oStream);

                            oStream.Close();
                        }
                        else
                        {
                            // Open file on disk and stream it

                            FileStream fStream = new FileStream(webRequest.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                            SendHeader(mimeType, fStream.Length, StatusCode.OK);
                            SendStream(fStream);

                            fStream.Close();
                        }

                        RaiseLogEvent(LogEventType.ClientResult, GetStatusCode(StatusCode.OK));
                    }

                    tcpClient.Client.Close();
                    tcpClient.Close();
                }
            }
        }

        /// <summary>
        /// Sends HTTP header
        /// </summary>
        /// <param name="mimeType">Mime Type</param>
        /// <param name="totalBytes">Length of the response</param>
        /// <param name="statusCode">Status code</param>
        private void SendHeader(string mimeType, long totalBytes, StatusCode statusCode)
        {
            if (string.IsNullOrEmpty(mimeType))
            {
                mimeType = "text/html";
            }

            StringBuilder header = new StringBuilder();
            header.Append(string.Format("HTTP/1.1 {0}\r\n", GetStatusCode(statusCode)));
            header.Append(string.Format("Content-Type: {0}\r\n", mimeType));
            header.Append(string.Format("Accept-Ranges: bytes\r\n"));
            header.Append(string.Format("Server: {0}\r\n", Configuration.ServerName));
            header.Append(string.Format("Connection: close\r\n"));            
            header.Append(string.Format("Content-Length: {0}\r\n", totalBytes));
            header.Append("\r\n");

            SendToClient(header.ToString());
        }

        /// <summary>
        /// Sends error page to the client
        /// </summary>
        /// <param name="statusCode">Status code</param>
        /// <param name="message">Error message</param>
        private void SendError(StatusCode statusCode, string message)
        {
            string page = GetErrorPage(statusCode, message);
            SendHeader(null, page.Length, statusCode);
            SendToClient(page);

            RaiseLogEvent(LogEventType.ClientResult, GetStatusCode(statusCode));
        }

        /// <summary>
        /// Sends stream to the client
        /// </summary>
        /// <param name="stream"></param>
        private void SendStream(Stream stream)
        {
            byte[] buffer = new byte[10240];
            while (true)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0) SendToClient(buffer, bytesRead);
                else break;
            }
        }

        /// <summary>
        /// Send string data to client
        /// </summary>
        /// <param name="data">String data</param>
        private void SendToClient(string data)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(data);
            SendToClient(bytes, bytes.Length);
        }

        /// <summary>
        /// Sends byte array to client
        /// </summary>
        /// <param name="data">Data array</param>
        /// <param name="bytesTosend">Data length</param>
        private void SendToClient(byte[] data, int bytesTosend)
        {
            try
            {
                Socket socket = tcpClient.Client;

                if (socket.Connected)
                {
                    int sentBytes = socket.Send(data, 0, bytesTosend, 0);
                    if (sentBytes < bytesTosend)
                        Console.WriteLine("Data was not completly send.");
                    else
                        Console.WriteLine("Data was send. Total length " + sentBytes.ToString());
                }
                else
                    Console.WriteLine("Connection lost");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.ToString());
            }
        }

        /// <summary>
        /// Checks whether the method in request is supported
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>True if method is supported</returns>
        private bool SuportedMethod(string request)
        {
            return (request.Substring(0, 3) != "GET") ? false : true;
        }

        /// <summary>
        /// Gets request from input string
        /// </summary>
        /// <param name="data">Input string</param>
        /// <returns>Request</returns>
        private string GetRequest(string data)
        {
            Match m = Regex.Match(data, @"^[^\n]*");
            return m.Value;
        }

        /// <summary>
        /// Gets URI from request
        /// </summary>
        /// <param name="request">Reuqest string</param>
        /// <returns>URI</returns>
        private string GetRequestedURI(string request)
        {
            int startPos = request.IndexOf("HTTP", 1);
            request = (startPos > 0) ? request.Substring(0, startPos - 1) : request.Substring(0);

            request = request.Replace("\\", "/");

            if (request.IndexOf(".") < 1 && !request.EndsWith("/"))
                request = request + "/ ";

            return request.Substring(request.IndexOf("/")).Trim();
        }

        /// <summary>
        /// Parse request string into WebRequest
        /// </summary>
        /// <param name="request">Request string</param>
        /// <returns>WebRequest</returns>
        private WebRequest ParseRequest(string request)
        {
            string uri = GetRequestedURI(request);

            WebRequest retRequest = new WebRequest();

            int startPos = uri.LastIndexOf("/");
            retRequest.Directory = uri.Substring(0, startPos);
            retRequest.FileName = uri.Substring(startPos + 1);

            // Parse query string
            startPos = retRequest.FileName.IndexOf('?');

            if (startPos > 0)
            {
                retRequest.QueryString = retRequest.FileName.Substring(startPos + 1);
                retRequest.FileName = retRequest.FileName.Substring(0, startPos);
            }

            // Get physical path for the directory
            retRequest.Directory = GetLocalDir(retRequest.Directory);

            // Prefill default filename if is not specifieed
            if (string.IsNullOrEmpty(retRequest.FileName))
                retRequest.FileName = Configuration.GetDefaultFileName(retRequest.Directory);

            return retRequest;
        }

        /// <summary>
        /// Gets physical path for the URL path
        /// </summary>
        /// <param name="path">URL path</param>
        /// <returns>Physical path</returns>
        private string GetLocalDir(string path)
        {
            path = path.Trim();

            Match m = Regex.Match(path, @"^/?([^/]*)");            
            string firstDir = m.ToString();
            string otherDir = path.Substring(m.Length);

            // Look in virtual directory list
            string dirName = Configuration.GetVirtualDirectory(firstDir);

            otherDir = otherDir.Replace('/', Path.DirectorySeparatorChar);
            firstDir = firstDir.Replace('/', Path.DirectorySeparatorChar);

            string localDir = (string.IsNullOrEmpty(dirName)) ? Configuration.ServerRoot + firstDir + otherDir : dirName + otherDir;

            Console.WriteLine("Local dir: " + localDir);
            return localDir;
        }

        /// <summary>
        /// Generates error page
        /// </summary>
        /// <param name="statusCode">StatusCode</param>
        /// <param name="message">Message</param>
        /// <returns>ErrorPage</returns>
        private string GetErrorPage(StatusCode statusCode, string message)
        {
            string status = GetStatusCode(statusCode);

            StringBuilder errorMessage = new StringBuilder();
            errorMessage.Append("<html>\n");
            errorMessage.Append("<head>\n");
            errorMessage.Append(string.Format("<title>{0}</title>\n", status));
            errorMessage.Append("</head>\n");
            errorMessage.Append("<body>\n");
            errorMessage.Append(string.Format("<h1>{0}</h1>\n", status));
            errorMessage.Append(string.Format("<p>{0}</p>\n", message));
            errorMessage.Append("<hr>\n");
            errorMessage.Append(string.Format("<address>{0} Server at {1} Port {2} </address>\n", Configuration.ServerName, Configuration.IPAddress, Configuration.Port));
            errorMessage.Append("</body>\n");
            errorMessage.Append("</html>\n");
            return errorMessage.ToString();
        }

        /// <summary>
        /// Gets string representation for the status code
        /// </summary>
        /// <param name="statusCode">Status code</param>
        /// <returns>Status code as HTTP string</returns>
        private string GetStatusCode(StatusCode statusCode)
        {
            string code;

            switch (statusCode)
            {
                case StatusCode.OK: code = "200 OK"; break;
                case StatusCode.BadRequest: code = "400 Bad Request"; break;
                case StatusCode.Forbiden: code = "403 Forbidden"; break;
                case StatusCode.NotFound: code = "404 Not Found"; break;
                default: code = "202 Accepted"; break;
            }

            return code;
        }

        /// <summary>
        /// Raise event when something "loggable" happend
        /// </summary>
        /// <param name="eventType">Event type</param>
        /// <param name="message">Message</param>
        private void RaiseLogEvent(LogEventType eventType, string message)
        {
            if (OnLogEvent != null)
            {
                OnLogEvent(eventType, message);
            }
        }

        /// <summary>
        /// Determines whether WebServer is running or not
        /// </summary>
        public bool Running
        {
            get { return running; }
        }
    
        /// <summary>
        /// Decodes the URL query string into string
        /// </summary>
        /// <param name="encodedString">Encoded QueryString</param>
        /// <returns>Plain string</returns>
        public static string URLDecode(string encodedString)
        {
            string outStr = string.Empty;

            int i = 0;
            while (i < encodedString.Length)
            {
                switch (encodedString[i])
                {
                    case '+': outStr += " "; break;
                    case '%':
                        string tempStr = encodedString.Substring(i+1, 2);
                        outStr += Convert.ToChar(int.Parse(tempStr, System.Globalization.NumberStyles.AllowHexSpecifier));
                        i = i + 2;
                        break;
                    default:
                        outStr += encodedString[i];
                        break;
                }
                i++;
            }
            return outStr;
        }

        /// <summary>
        /// Splits query string into NameValueCollection
        /// </summary>
        /// <param name="queryString">Query string</param>
        /// <returns>NameValueCollection</returns>
        public static NameValueCollection ParseQueryString(string queryString)
        {
            NameValueCollection nameValueCollection = new NameValueCollection();
            string[] parts = queryString.Split('&');

            foreach (string part in parts)
            {
                string[] nameValue = part.Split('=');
                nameValueCollection.Add(nameValue[0], URLDecode(nameValue[1]));
            }

            return nameValueCollection;
        }

        public delegate void SpecialFileType(WebRequest webRequest, out Stream outputStream);
        /// <summary>
        /// Even is raised when SpecialFile type is requested
        /// </summary>
        public event SpecialFileType OnSpecialFileType;

        public delegate void LogEvent(LogEventType logEvent, string message);
        /// <summary>
        /// Event is Raised when log event occures
        /// </summary>
        public event LogEvent OnLogEvent;   

    }
}
