This repo is based on my original blog post [http://bansky.net/blog/compact-web-server-in-compact-framework/](http://bansky.net/blog/compact-web-server-in-compact-framework/)


![home page 242x322](http://bansky.net/blogimages/tinyWeb04.png)

<!--more-->

## Configuring web server

Configuration for web server is stored in _WebServerConfiguration_ class. Here it's possible to setup server root, listening port, IP address etc.. Important thing is the possibility to configure default files like default.html, index.html or other. Do not forget to register MIME types for files that will be handled by the web server. Setting right mime types is necessary for correct response header of the server.
Special files are those files, that need to be treat in custom method and not just send to the client. **CompactWeb** server supports **virtual directories** so you can request directory placed outside the server root.
    
```csharp
WebServerConfiguration webConf = new WebServerConfiguration();

webConf.IPAddress = IPAddress.Any;
webConf.Port = 80;

// Folder where the web is stored
webConf.ServerRoot = @"\Inetpub";

webConf.AddDefaultFile("default.html");
webConf.AddMimeType(".htm", "text/html");
webConf.AddMimeType(".html", "text/html");
webConf.AddMimeType(".png", "image/png");
webConf.AddMimeType(".jpg", "image/jpg");
webConf.AddMimeType(".gif", "image/gif");
webConf.AddMimeType(".bmp", "image/bmp");
webConf.AddMimeType(".cgi", "text/html");

// .cgi files will be special handled
webConf.AddSpecialFileType(".cgi");

// Register virtual directory
webConf.AddVirtualDirectory("photos", @"\My Documents\Photos");
```

Necessary step is to setup the server root physically on the file system. Simply create the folder and put the website content in it.

## Start me up

Registering **OnLogEvent** is fine to track client connections, response sending and exceptions. **OnSpecialFileType** is raised whenever the file registered as special is requested by client. Delegate for this event can handle output to the client.

```csharp
webServer = new WebServer(webConf);
webServer.OnLogEvent += new WebServer.LogEvent(webServer_OnLogEvent);
webServer.OnSpecialFileType += new WebServer.SpecialFileType(webServer_OnSpecialFileType);
webServer.Start();
```

From here, your server is running and ready to serve all files placed in the server root.

## CGI and such

If it's necessary to process some files in a special way, you can use delegate to **OnSpecialFileType** and take the control over processing.
Next piece of code handles file with .cgi extension. It takes the content of the file and replace <%=RESULT%> pattern with parameter _userName_ passed in the query string. In case that you will call following URL **http://127.0.0.1/form.cgi?userNamer=Joshua** the <%=RESULT%> pattern will be replaced with _Joshua_.

```csharp
void webServer_OnSpecialFileType(CompactWeb.WebRequest webRequest, out IO.Stream outputStream)
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
```

