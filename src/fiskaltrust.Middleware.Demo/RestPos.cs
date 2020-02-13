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
using System.Xml.Serialization;

namespace fiskaltrust.Middleware.Demo
{
    public class RestPos : ifPOS.v1.IPOS
    {
        private string _url;
        private string _requestType;
        public RestPos(string url)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
            };

            _url = url.Replace("rest://", "http://");
            _requestType = url;
        }

        IAsyncResult ifPOS.v0.IPOS.BeginEcho(string message, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        IAsyncResult ifPOS.v0.IPOS.BeginJournal(long ftJournalType, long from, long to, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        IAsyncResult ifPOS.v0.IPOS.BeginSign(ifPOS.v0.ReceiptRequest data, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        string ifPOS.v0.IPOS.Echo(string message)
        {       

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_url);
                var jsonstring = JsonConvert.SerializeObject("message");
                var jsonContent = new StringContent(jsonstring, Encoding.UTF8, "application/json");                                            

                using (var response = client.PostAsync("v0/Echo", jsonContent).Result)
                {
                    var reponse = response.Content.ReadAsStringAsync();
                    return reponse.ToString();
                }
            }
        }       

        async Task<EchoResponse> ifPOS.v1.IPOS.EchoAsync(EchoRequest message)
        {
            if (_requestType.StartsWith("rest://"))
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
            var xmlString = Serialize(message);
            var xmlContent = new StringContent(xmlString, Encoding.UTF8, "application/xml");

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

                using (var response = await client.PostAsync("v1/echo", jsonContent))
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<EchoResponse>(content.ToString());
                }
            }
        }

        string ifPOS.v0.IPOS.EndEcho(IAsyncResult result)
        {
            throw new NotSupportedException();
        }

        Stream ifPOS.v0.IPOS.EndJournal(IAsyncResult result)
        {
            throw new NotSupportedException();
        }

        ifPOS.v0.ReceiptResponse ifPOS.v0.IPOS.EndSign(IAsyncResult result)
        {
            throw new NotSupportedException();
        }

        Stream ifPOS.v0.IPOS.Journal(long ftJournalType, long from, long to)
        {
            using (var client = new HttpClient())
            {
                var data = new { type = ftJournalType, from= from, to= to };
                var jsonstring = JsonConvert.SerializeObject(data);
                var jsonContent = new StringContent(jsonstring, Encoding.UTF8, "application/json");
                client.BaseAddress = new Uri(_url);               

                using (var response =  client.PostAsync("v0/journal", jsonContent).Result)
                {
                    var stream = response.Content.ReadAsStreamAsync().Result;
                    return stream;
                }
            }
        }                   

        IAsyncEnumerable<JournalResponse> ifPOS.v1.IPOS.JournalAsync(JournalRequest request)
        {
            throw new NotSupportedException("Async Streaming are not supported in Http");
        }

        ifPOS.v0.ReceiptResponse ifPOS.v0.IPOS.Sign(ifPOS.v0.ReceiptRequest data)
        {
            throw new NotImplementedException();
        }

        async Task<ifPOS.v1.ReceiptResponse> ifPOS.v1.IPOS.SignAsync(ifPOS.v1.ReceiptRequest request)
        {
            if (_requestType.StartsWith("rest://"))
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
            var xmlString = Serialize(request);
            var xmlContent = new StringContent(xmlString, Encoding.UTF8, "application/xml");

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

        private string Serialize(Object inputObject)
        {
            using (var writer = new StringWriter())
            {
                new XmlSerializer(inputObject.GetType()).Serialize(writer, inputObject);
                return writer.ToString();
            }
        }
    }
}

