using System;

namespace IP2Region
{
    public class IPInValidException : Exception
    {
        const string ERROR_MSG = "IP Illigel. Please input a valid IP.";
        public IPInValidException() : base(ERROR_MSG) { }
    }
}
