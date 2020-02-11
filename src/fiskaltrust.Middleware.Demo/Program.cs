using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using fiskaltrust.ifPOS.v1;
using fiskaltrust.Middleware.Demo.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace fiskaltrust.Middleware.Demo
{
    public static class Program
    {
                private const string _RECEIPTEXAMPLEFOLDER = "ReceiptExamples";

        private static IPOS pos = null;
        private static Guid _cashBoxId;

        private static readonly Dictionary<string, ReceiptRequest> Examples = new Dictionary<string, ReceiptRequest>();

        /// <param name="cashboxId">The cashboxid for the Middleware.</param>
        /// <param name="url">The url that is used for sending requests to the Middleware(Default: grpc://localhost:10103).</param>
        /// <returns></returns>
        public static async Task Main(string cashboxId, string url = "grpc://localhost:10103")
        {
            try
            {
                if (Guid.TryParse(cashboxId, out var parsedCashBoxId))
                {
                    _cashBoxId = parsedCashBoxId;
                    LoadExamples();
                    pos = GetPosClientForUrl(url);
                    await EchoAsync();


                    while (true)
                    {
                        await MenuAsync();
                    }
                }
                else
                {
                    ConsoleHelper.WriteError("Please provide a valid CashBoxId");
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError(ex.ToString());

            }
            finally
            {
                Console.ReadLine();
            }
        }

        private static void LoadExamples()
        {
            foreach (var file in Directory.GetFiles(_RECEIPTEXAMPLEFOLDER, "*.json", SearchOption.AllDirectories).OrderBy(f => f))
            {
                var content = File.ReadAllText(file);
                var req = JsonConvert.DeserializeObject<ReceiptRequest>(content);
                req.ftCashBoxID = _cashBoxId.ToString();
                Examples.Add(file, req);
            }
        }

        private static IPOS GetPosClientForUrl(string url)
        {
            if (url.StartsWith("grpc://"))
            {
                var uri = new Uri(url);
                return GrpcHelper.GetClient<IPOS>(uri.Host, uri.Port);
            }
            else
            {
#if WCF
                    return WcfHelper.GetClient<IPOS>(url);
#else
                throw new NotSupportedException($"The url {url} is not supported in .NET Core. Please provide a valid one. If you want to use WCF for connection make sure you are running the net461 version of this application.");
#endif
            }
        }

        private static async Task EchoAsync()
        {
            try
            {
                var message = await pos.EchoAsync(new EchoRequest
                {
                    Message = "message"
                });
                if (message.Message != "message")
                {
                    throw new Exception("echo failed");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        private static async Task MenuAsync()
        {
            PrintOptions();
            var input = Console.ReadLine();
            if (!int.TryParse(input, out var inputInt) || inputInt > Examples.Keys.Count)
            {
                Console.WriteLine($"\"{input}\" nicht erkannt.");
            }
            else
            {
                var req = Examples.Values.ToList()[inputInt - 1];
                await PerformPosRequest(req);
                Console.WriteLine("Please press enter to continue.");
                Console.ReadLine();
                Console.Clear();
            }
            await MenuAsync();
        }

        private static void PrintOptions()
        {
            var i = 1;
            foreach (var example in Examples)
            {
                Console.WriteLine($"{i}: {example.Key} - ({example.Value.ftReceiptCase:X})");
                i++;
            }
            Console.WriteLine("exit: Program beenden");
        }

        private static async Task PerformPosRequest(ReceiptRequest req)
        {
            try
            {
                PrintRequest(req);
                var resp = await pos.SignAsync(req);
                PrintResponse(resp);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("An error occured when trying to send the request.");
                Console.Error.WriteLine(ex);
            }
        }

        private static void PrintRequest(ReceiptRequest receiptRequest)
        {
            Console.WriteLine("{0:G} ReceiptRequest:", DateTime.Now);
            Console.WriteLine(JsonConvert.SerializeObject(receiptRequest, Formatting.Indented));
        }

        private static void PrintResponse(ReceiptResponse data)
        {
            if (data != null)
            {
                Console.WriteLine("{0:G} ReceiptResponse:", DateTime.Now);
                Console.WriteLine(JsonConvert.SerializeObject(data, Formatting.Indented));
                Console.WriteLine("========== n: {0} CashBoxIdentificateion:{1} ReceiptIdentification:{2} ==========", data.cbReceiptReference, data.ftCashBoxIdentification, data.ftReceiptIdentification);
                foreach (var item in data.ftSignatures.Where(x => x.ftSignatureFormat != 0x03))
                {
                    Console.WriteLine("{0}:{1}", item.Caption, item.Data);
                }
                foreach (var item in data.ftSignatures.Where(x => x.ftSignatureFormat == 0x03))
                {
                    QrCodeHelper.PrintQrCode(item);
                }
            }
            else
            {
                Console.WriteLine("null-result!!!");
            }
        }
    }
}
