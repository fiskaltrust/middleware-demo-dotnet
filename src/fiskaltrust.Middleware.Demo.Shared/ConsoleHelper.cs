using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Linq;

namespace fiskaltrust.Middleware.Demo.Shared
{
    public static class ConsoleHelper
    {
        public static T ReadFromConsole<T>(string label, bool printType = true)
        {
            while (true)
            {
                try
                {
                    if (printType)
                        Console.Write($"Please provide a value for '{label}' ({typeof(T).Name}): ");
                    else
                        Console.Write($"Please provide a value for '{label}': ");

                    string input = Console.ReadLine();
                    return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(input);
                }
                catch (Exception)
                {
                    Console.WriteLine("Input not valid, please try again.");
                }
            }
        }

        public static void PrintRequest(ifPOS.v1.ReceiptRequest receiptRequest)
        {
            Console.WriteLine("{0:G} ReceiptRequest:", DateTime.Now);
            Console.WriteLine(JsonConvert.SerializeObject(receiptRequest, Formatting.Indented));
        }

        public static void PrintResponse(ifPOS.v1.ReceiptResponse response)
        {
            if (response != null)
            {
                Console.WriteLine("{0:G} ReceiptResponse:", DateTime.Now);
                Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
                Console.WriteLine("========== n: {0} CashBoxIdentificateion:{1} ReceiptIdentification:{2} ==========", response.cbReceiptReference, response.ftCashBoxIdentification, response.ftReceiptIdentification);
                foreach (var item in response.ftSignatures.Where(x => x.ftSignatureFormat != 0x03))
                {
                    Console.WriteLine("{0}:{1}", item.Caption, item.Data);
                }
                foreach (var item in response.ftSignatures.Where(x => x.ftSignatureFormat == 0x03))
                {
                    QrCodeHelper.PrintQrCode(item.Data);
                }
            }
            else
            {
                Console.WriteLine("null-result!!!");
            }
        }

        public static void PrintRequest(ifPOS.v0.ReceiptRequest receiptRequest)
        {
            Console.WriteLine("{0:G} ReceiptRequest:", DateTime.Now);
            Console.WriteLine(JsonConvert.SerializeObject(receiptRequest, Formatting.Indented));
        }

        public static void PrintResponse(ifPOS.v0.ReceiptResponse response)
        {
            if (response != null)
            {
                Console.WriteLine("{0:G} ReceiptResponse:", DateTime.Now);
                Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
                Console.WriteLine("========== n: {0} CashBoxIdentificateion:{1} ReceiptIdentification:{2} ==========", response.cbReceiptReference, response.ftCashBoxIdentification, response.ftReceiptIdentification);
                foreach (var item in response.ftSignatures.Where(x => x.ftSignatureFormat != 0x03))
                {
                    Console.WriteLine("{0}:{1}", item.Caption, item.Data);
                }
                foreach (var item in response.ftSignatures.Where(x => x.ftSignatureFormat == 0x03))
                {
                    QrCodeHelper.PrintQrCode(item.Data);
                }
            }
            else
            {
                Console.WriteLine("null-result!!!");
            }
        }
    }
}
