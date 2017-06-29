using System;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using CompactWeb;

namespace TinyWebServer
{
    public partial class MainForm : Form
    {
        WebServer webServer;        
        WebServerConfiguration webConf = new WebServerConfiguration();

        public MainForm()
        {
            InitializeComponent();

            // Show IP address
            string strHostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
            IPAddress[] addr = ipEntry.AddressList;
            lblIpAddr.Text = addr[0].ToString();
        }

        /// <summary>
        /// Start web server
        /// </summary>
        private void mnStart_Click(object sender, EventArgs e)
        {
            webConf.IPAddress = IPAddress.Any;
            webConf.Port = 80;
            // Path to the server root
            webConf.ServerRoot = @"\Inetpub";
            webConf.AddDefaultFile("default.html");
            webConf.AddMimeType(".htm", "text/html");
            webConf.AddMimeType(".html", "text/html");
            webConf.AddMimeType(".png", "image/png");
            webConf.AddMimeType(".jpg", "image/jpg");
            webConf.AddMimeType(".gif", "image/gif");
            webConf.AddMimeType(".bmp", "image/bmp");
            webConf.AddMimeType(".cgi", "text/html");
            // Register special file
            webConf.AddSpecialFileType(".cgi");

            // Register virtual directory
            //webConf.AddVirtualDirectory("photos", @"\SDMMC\Photos");

            webServer = new WebServer(webConf);
            webServer.OnLogEvent += new WebServer.LogEvent(webServer_OnLogEvent);
            webServer.OnSpecialFileType += new WebServer.SpecialFileType(webServer_OnSpecialFileType);
            webServer.Start();

            mnStart.Enabled = false;
        }

        private void mnStop_Click(object sender, EventArgs e)
        {
            StopServer();
        }

        private void mnClose_Click(object sender, EventArgs e)
        {
            StopServer();
            Close();
        }

        /// <summary>
        /// Stop the web server
        /// <param name="e"></param>
        void StopServer()
        {
            if (webServer != null && webServer.Running)
            {
                webServer.Stop();
                mnStart.Enabled = true;
            }
        }

        /// <summary>
        /// Handle the special file
        /// </summary>
        /// <param name="webRequest">Web request</param>
        /// <param name="outputStream">Output stream</param>
        void webServer_OnSpecialFileType(CompactWeb.WebRequest webRequest, out System.IO.Stream outputStream)
        {
            outputStream = new MemoryStream();           

            if (webRequest.FileName == "form.cgi")
            {
                // Read the requested file template
                FileStream sourceFile = new FileStream(webRequest.FullPath, FileMode.Open);
                StreamReader streamReader = new StreamReader(sourceFile);
                string response = streamReader.ReadToEnd();
                streamReader.Close();
                sourceFile.Close();
                
                // Parse query string into NameValueCollection
                NameValueCollection query = WebServer.ParseQueryString(webRequest.QueryString);

                // Replace <%=RESULT%> with the username
                string userName = query["userName"];
                response = response.Replace("<%=RESULT%>", userName);

                // Creat response
                byte[] buffer = Encoding.ASCII.GetBytes(response);
                outputStream.Write(buffer, 0, buffer.Length);
            }
        }

        /// <summary>
        /// Event occurs when log event is raised
        /// </summary>
        /// <param name="logEvent"></param>
        /// <param name="message"></param>
        void webServer_OnLogEvent(LogEventType logEvent, string message)
        {
            LogMessage logMsg = new LogMessage();
            logMsg.EventType = logEvent;
            logMsg.EventMessage = message;

            ReportProgress(logMsg);
        }

        /// <summary>
        /// Thread safe UI report progress 
        /// </summary>
        /// <param name="state"></param>
        void ReportProgress(object state)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new WaitCallback(ReportProgress), new object[] { state });
                return;
            }

            if (listBoxLog.Items.Count > 14) listBoxLog.Items.Clear();

            LogMessage logMsg = (LogMessage)state;

            listBoxLog.Items.Add(string.Format(">>{0}", logMsg.EventType.ToString()));
            listBoxLog.Items.Add(string.Format("{0}", logMsg.EventMessage));
        }

    }
}