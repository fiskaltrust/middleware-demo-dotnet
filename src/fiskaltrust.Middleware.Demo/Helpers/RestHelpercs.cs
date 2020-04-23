using fiskaltrust.ifPOS.v1;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace fiskaltrust.Middleware.Demo.Helpers
{
    public static class RestHelper
    {
        public static IPOS GetClient(string url)
        {
            return new RestPos(url);
        }
    }
}
