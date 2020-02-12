using fiskaltrust.ifPOS.v0;
using fiskaltrust.ifPOS.v1;
using fiskaltrust.Middleware.Demo.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace fiskaltrust.Middleware.Demo
{
    public class RestPos : ifPOS.v1.IPOS
    {
        private string _url;
        public RestPos(string url)
        {
            _url = url;
        }

        IAsyncResult ifPOS.v0.IPOS.BeginEcho(string message, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        IAsyncResult ifPOS.v0.IPOS.BeginJournal(long ftJournalType, long from, long to, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        IAsyncResult ifPOS.v0.IPOS.BeginSign(ifPOS.v0.ReceiptRequest data, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        string ifPOS.v0.IPOS.Echo(string message)
        {
            throw new NotImplementedException();
        }

        async Task<EchoResponse> ifPOS.v1.IPOS.EchoAsync(EchoRequest message)
        {
            if (_url.StartsWith("rest://"))
            {
                return await JsonEchoAsync(message);
            }
            else
            {
                return await XmlEchoAsync(message);
            }

        }

        private async Task<EchoResponse> XmlEchoAsync(EchoRequest message)
        {
            var jsonstring = JsonConvert.SerializeObject(message);
            var xmlContent = new StringContent(jsonstring, Encoding.UTF8, "application/xml");

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_url);

                using (var response = await client.PostAsync("v1/Echo", xmlContent))
                {
                    var content = await response.Content.ReadAsStringAsync();

                    var xml = XElement.Parse(content);
                    string jsonText = JsonConvert.SerializeXNode(xml);
                    return JsonConvert.DeserializeObject<EchoResponse>(jsonText);
                }
            }
        }

        private async Task<EchoResponse> JsonEchoAsync(EchoRequest message)
        {
            var jsonstring = JsonConvert.SerializeObject(message);
            var jsonContent = new StringContent(jsonstring, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_url);

                using (var response = await client.PostAsync("v1/Echo", jsonContent))
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<EchoResponse>(content.ToString());
                }
            }
        }

        string ifPOS.v0.IPOS.EndEcho(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        Stream ifPOS.v0.IPOS.EndJournal(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        ifPOS.v0.ReceiptResponse ifPOS.v0.IPOS.EndSign(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        Stream ifPOS.v0.IPOS.Journal(long ftJournalType, long from, long to)
        {
            throw new NotImplementedException();
        }

        IAsyncEnumerable<JournalResponse> ifPOS.v1.IPOS.JournalAsync(JournalRequest request)
        {
            throw new NotImplementedException();
        }

        ifPOS.v0.ReceiptResponse ifPOS.v0.IPOS.Sign(ifPOS.v0.ReceiptRequest data)
        {
            throw new NotImplementedException();
        }

        async Task<ifPOS.v1.ReceiptResponse> ifPOS.v1.IPOS.SignAsync(ifPOS.v1.ReceiptRequest request)
        {
            if (_url.StartsWith("rest://"))
            {
                return await JsonSignAsync(request);
            }
            else
            {
                return await XmlSignAsync(request);
            }
        }

        private async Task<ifPOS.v1.ReceiptResponse> JsonSignAsync(ifPOS.v1.ReceiptRequest request)
        {
            var jsonstring = JsonConvert.SerializeObject(request);
            var jsonContent = new StringContent(jsonstring, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_url);

                using (var response = await client.PostAsync("v1/Sign", jsonContent))
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<ifPOS.v1.ReceiptResponse>(content.ToString());
                }
            }
        }

        private async Task<ifPOS.v1.ReceiptResponse> XmlSignAsync(ifPOS.v1.ReceiptRequest request)
        {
            var jsonstring = JsonConvert.SerializeObject(request);
            var xmlContent = new StringContent(jsonstring, Encoding.UTF8, "application/xml");

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_url);

                using (var response = await client.PostAsync("v1/Sign", xmlContent))
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var xml = XElement.Parse(content);
                    string jsonText = JsonConvert.SerializeXNode(xml);
                    return JsonConvert.DeserializeObject<ifPOS.v1.ReceiptResponse>(jsonText);
                }
            }
        }
    }
}

