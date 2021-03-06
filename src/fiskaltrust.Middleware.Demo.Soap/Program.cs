﻿using System;
using System.Threading.Tasks;
using fiskaltrust.Middleware.Demo.Shared;

namespace fiskaltrust.Middleware.Demo.Soap
{
    public static class Program
    {
        private const string RECEIPT_EXAMPLES_DIR_AT = "ReceiptExamples/at";
        private const string RECEIPT_EXAMPLES_DIR_DE = "ReceiptExamples/de";
        private const string RECEIPT_EXAMPLES_DIR_FR = "ReceiptExamples/fr";

        /// <summary>A demo application to demonstrate how to connect to the fiskaltrust.Middleware via WCF/SOAP</summary>
        /// <remarks>
        /// This application uses the fiskaltrust.Middleware.Interface.Soap client package that encapsulates communication.
        /// Examples are loaded from the ReceiptExamples directory. 
        /// Currently, the v1 interface is not supported for Austrian and French instances. Hence, v0 is used for them in this example, while the German example uses v1.
        /// </remarks>
        /// <param name="url">The URL that is used to connect to the Middleware.</param>
        /// <param name="cashboxId">The cashboxid for the Middleware.</param>
        /// <param name="market">The country that will be used to select the respective receipt examples. Either AT, DE or FR.</param>
        public static async Task Main(string url, Guid cashboxId, Market market)
        {
            try
            {
                if (url == null)
                    url = ConsoleHelper.ReadFromConsole<string>("Middleware URL");
                if (cashboxId == default)
                    cashboxId = ConsoleHelper.ReadFromConsole<Guid>("CashboxID");
                if (market == Market.Undefined)
                    market = ConsoleHelper.ReadFromConsole<Market>("Market (AT, DE or FR)", false);

                switch (market)
                {
                    case Market.AT:
                        DemoV0.Run(url, cashboxId, RECEIPT_EXAMPLES_DIR_AT);
                        break;
                    case Market.FR:
                        DemoV0.Run(url, cashboxId, RECEIPT_EXAMPLES_DIR_FR);
                        break;
                    case Market.DE:
                        await Demo.RunAsync(url, cashboxId, RECEIPT_EXAMPLES_DIR_DE);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
            finally
            {
                Console.WriteLine("Press Enter to exit.");
                Console.ReadLine();
            }
        }
    }

    public enum Market
    {
        Undefined,
        AT,
        DE,
        FR
    }
}
