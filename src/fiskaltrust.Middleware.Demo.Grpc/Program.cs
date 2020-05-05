using System;
using System.Threading.Tasks;
using fiskaltrust.Middleware.Demo.Shared;

namespace fiskaltrust.Middleware.Demo.Grpc
{
    public static class Program
    {
        private const string RECEIPT_EXAMPLES_DIR_DE = "ReceiptExamples/de";

        /// <summary>A demo application to demonstrate how to connect to the fiskaltrust.Middleware via HTTP</summary>
        /// <remarks>
        /// This application uses the fiskaltrust.Middleware.Interface.Http client package that encapsulates communication.
        /// Examples are loaded from the ReceiptExamples directory. 
        /// gRPC is only supported in v1, which is not yet implemented for Austrian and French queues. This example is thus only supporting German instances.
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
                    case Market.FR:
                        throw new NotImplementedException("gRPC communication is currently not supported for Austrian and French queues. Please have a look at the HTTP and SOAP examples.");
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
