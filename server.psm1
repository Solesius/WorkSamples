#a part of a small http framework written in powershell
using namespace System.Net
using namespace System.Collections
using namespace System.Management.Automation
using namespace System.Security.Cryptography.X509Certificates
using module ".\IO.psm1"
using module ".\RouteImpl.psm1"
using module ".\HeaderBuilder.psm1"

class HttpServer
{
    [int]$port
    [String]$ip
    [String]$origin
    hidden [ArrayList]$routes = [ArrayList]::new()
    hidden [IO]$IO = [IO]::new()
    hidden [Logger]$Logger = [Logger]::new()
    hidden [Sockets.TcpListener]$tcp_listener
    hidden [HeaderBuilder] $hb = [HeaderBuilder]::new()

    #empty constructor
    HttpServer()
    {

    }

    HttpServer($_ip, $_port,$accepted_origin)
    {
        $this.ip = $_ip;
        $this.port = $_port;
        $this.origin = $accepted_origin
    }

    [Void]
    Listen([Int]$max_connections)
    {
        [IPHostEntry]$ip_host_entry = [System.Net.Dns]::Resolve((hostname))
        $bound_address = $ip_host_entry.AddressList.Where({
            $_.IPAddressToString -eq $this.ip
        })[0]
        try{
            $this.tcp_listener = [Sockets.TcpListener]::new($bound_address,$this.port);
            $this.tcp_listener.Start();
            $this.Logger.Log("Server listening on port $($this.port)", 0)
        }
        catch {
            $this.tcp_listener.Stop();
        }      
    }
    [Void]ServeFile($filename,$socket,$mime) {

        $found_headers = $this.Bytes( ( [String]::Join("", @(
                "HTTP/1.1 200 OK`r`n",
                "Access-Control-Allow-Origin: $($this.origin)`r`n"
                "Access-Control-Allow-Methods: GET, POST, OPTIONS`r`n"
                "Access-Control-Allow-Headers: Content-Type,Origin`r`n"
                "Content-Type: $($mime)`r`n",
                "Vary: Origin`r`n",
                "Cache-Control: max-age=15`r`n",
                "Server: Powershell-Http-0.1BETA`r`n"
            )
        ) + "`r`n") )
        
        $not_found_headers = $this.Bytes( ( [String]::Join("", @(
                "HTTP/1.1 404 Not Found`r`n",
                "Access-Control-Allow-Origin: $($this.origin)`r`n"
                "Access-Control-Allow-Methods: GET, POST, OPTIONS`r`n"
                "Access-Control-Allow-Headers: Content-Type,Origin`r`n"
                "Content-Type: text/html`r`n",
                "Vary: Origin`r`n",
                "Cache-Control: no-cache`r`n",
                "Server: Powershell-Http-0.1BETA`r`n"
            )
        ) + "`r`n") )

        $error_page = (
                        "<h1>
                            404 NOT FOUND
                        </h1>
                        <p>
                            Sorry the file you're looking for is gone
                        </p>"
                )

        if([System.IO.File]::Exists($filename)) {
            $handle = [System.IO.FileStream]::new($filename,[System.IO.FileMode]::Open)

            [Byte[]]$bytes = [Byte[]]::new($handle.Length)
            $handle.Read($bytes,0,$handle.Length)
            $handle.Close()
            $handle.Dispose()
            
            $socket.Write($found_headers)
            $socket.Write($bytes)
            $socket.Flush()
            $socket.Close()
        }
        else {
            $socket.write($not_found_headers)
            $socket.Write($this.Bytes($error_page))
            $socket.Flush()
            $socket.Close()
        }
    }

    [Void]AddRoute([Route]$route){
        $this.routes.Add($route);
    }
    [Byte[]]Bytes([String]$s){
        return [System.Text.Encoding]::UTF8.GetBytes($s);
    }

    [Void]
    HandleRequests()
    {

        while ($true) {
    
            $incoming_connection_raw = $this.tcp_listener.AcceptSocket();
            $incoming_connection = [Sockets.NetworkStream]::new($incoming_connection_raw,$true)
            [String]$in = ""

            $bytes = [Byte[]]::new(1024);
            #data received from the client socket, read up until at most 1024 bytes, this can be done in a while loop.
            #all client side data will be < 1024bytes so this is fine for now. 
            $bytesRec = $incoming_connection.Read($bytes, 0, $bytes.Length)

            #append our placeholder string with amount of bytes read from the client socket. 
         
            $in += [Text.Encoding]::UTF8.GetString($bytes, 0, $bytesRec);
            [String[]]$request_headers = $in.split("`n");
            #setup our worker thread for the incoming socket. 

            $work = {
                param($reqH)

                [String]$route = $reqH[0].Split(" ")[1]
                [String]$http_method = $reqH[0].Split(" ")[0]
                [String]$remote_address = $incoming_connection_raw.RemoteEndPoint
                [Int]$delim = $reqH.IndexOf(
                    [System.Text.Encoding]::utf8.getstring(13)
                    );
                
                [String[]]$approved_IP =  @(
                   #ip allowlist goes here
                )

                #parse our JSON request body per IETF specification
                $body = ""
                ($delim..($reqH.Count - 1)) | ForEach-Object {
                    $body += $reqH[$_]
                }
                
                $body = $body | ConvertFrom-Json

                [hashtable]$route_work = @{
                    socket =  $incoming_connection;
                    request_body = $body;
                }
                $this.Logger.Log("$http_method request from $( $remote_address ) for $( $route )", 0);

                #IP Filtering middleware
                switch ( $approved_IP.Contains(([String]$incoming_connection_raw.RemoteEndPoint).Split(":")[0]) ) {
                    $true {
                        try {
                            #static file handler.
                            #can set to something like wwwroot and then test-path on that. 

                            if($route.contains(".")) {
                                
                                $real_path = [String]::Join(
                                            "/",
                                            @(
                                                #enter desired static file path here
                                                (pwd),
                                                $route.substring(1)
                                            )
                                    )

                                $map = [hashtable]@{
                                    js = "text/javascript";
                                    #"map" = "application/octect-stream"
                                    css = "text/css";
                                    json = "application/json";
                                    html = "text/html";
                                    ico = "image/x-icon"
                                }
                                
                                $extension = ([String]$route).substring($route.lastindexof(".")).replace(".","")

                                $this.ServeFile(
                                    $real_path,
                                    $incoming_connection,
                                    $map[$extension]
                                )

                                [Console]::WriteLine($route + "Served")
                            }else {
                                $this.
                                routes.
                                        Where({
                                            ($_.http_method -eq $http_method) -and
                                            ($_.route_path -eq $route)
                                        })[0].handler.InvokeReturnAsIs($route_work);
                            }
     
                        }
                        catch {
                            $incoming_connection.Write($this.hb.Error($this.origin))
                            $incoming_connection.Write($this.Bytes("API ERROR`nRequest Failed"))
                            $incoming_connection.Flush()
                            $incoming_connection.Close()
                            $incoming_connection.Dispose();
                        }
                        break
                    }
                    $false {
                        $incoming_connection.Write($this.hb.Unauthorized($this.origin))
                        $incoming_connection.Write($this.Bytes("API Permission Denied"))
                        $incoming_connection.Flush()
                        $incoming_connection.Close()
                        $incoming_connection.Dispose(); 
                    }
                }                
            }

            $req_handler = [IO]::new($work)
            $req_handler.AddSessionVariable($this,"this","")
            $req_handler.AddSessionVariable($this.IO, "IO", "")
            $req_handler.AddSessionVariable($this.hb, "hb", "")
            $req_handler.AddSessionVariable($this.origin, "origin", "")           
            $req_handler.AddSessionVariable($this.Logger,"Logger","")
            $req_handler.AddSessionVariable($incoming_connection, "incoming_connection", "Network stream wrapper for the client connection")
            $req_handler.AddSessionVariable($incoming_connection_raw, "incoming_connection_raw", "raw socket of the client connection")

            $req_handler.ForEachParN(1, @([hashtable]@{
                "reqH" = $request_headers
            }))

            $req_handler = $null;
            $incoming_connection_raw.Dispose();
        }
    }
}
