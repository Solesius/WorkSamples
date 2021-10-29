namespace microhttp
{
    class HeaderBuilder
    {
        public string _origin { get; set; }
        public HeaderBuilder() { }
        public HeaderBuilder(string origin) => (_origin) = (origin);

        public string GET(string o)
        {
            return string.Join("", new string[] {
                "HTTP/1.1 200 OK\r\n",
                $"Access-Control-Allow-Origin: {o} \r\n",
                "Access-Control-Allow-Methods: GET, POST, OPTIONS\r\n",
                "Access-Control-Allow-Headers: Content-Type,Origin\r\n",
                "Content-Type: text/plain\r\n",
                "Vary: Origin\r\n",
                "\r\n"
            });
        }
        public string OPTIONS(string o)
        {
            return string.Join("", new string[] {
                "HTTP/1.1 204 No Content\r\n",
                $"Access-Control-Allow-Origin: {o} \r\n",
                "Access-Control-Allow-Methods: GET, POST, OPTIONS\r\n",
                "Access-Control-Allow-Headers: Content-Type,Origin\r\n",
                "Vary: Origin\r\n",
                "\r\n"
            });
        }
        public string POST(string o)
        {
            return string.Join("", new string[] {
                "HTTP/1.1 200 OK\r\n",
                $"Access-Control-Allow-Origin: {o}\r\n",
                "Access-Control-Allow-Methods: GET, POST, OPTIONS\r\n",
                "Access-Control-Allow-Headers: Content-Type,Origin\r\n",
                "Vary: Origin\r\n",
                "Content-Type: application/json; charset=UTF-8\r\n",
                "Cache-Control: max-age=15\r\n",
                 "\r\n"
            });
        }
        public string ERROR(string o)
        {
            return string.Join("", new string[] {
                "HTTP/1.1 500 Internal Server Error\r\n",
                "Content-Type: text/plain; charset=UTF-8\r\n",
                 "\r\n"
            });
        }

        public string UNAUTHORIZED(string o)
        {
            return string.Join("", new string[] {
                "HTTP/1.1 401 Forbidden\r\n",
                "Content-Type: text/plain; charset=UTF-8\r\n",
                 "\r\n"
            });
        }
    }
}