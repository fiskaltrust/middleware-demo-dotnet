using System;
using System.Threading.Tasks;
using fiskaltrust.Middleware.Demo.Shared;

namespace fiskaltrust.Middleware.Demo.Soap
{
    public static class Program
    {
        private const string RECEIPT_EXAMPLES_DIR_AT = "ReceiptExamples/at";
        private const string RECEIPT_EXAMPLES_DIR_DE = "ReceiptExamples/de";
        private const string RECEIPT_EXAMPLES_DIR_FR = "ReceiptExamples/fr";

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

                var receiptExampleDir = market switch
                {
                    Market.AT => RECEIPT_EXAMPLES_DIR_AT,
                    Market.DE => RECEIPT_EXAMPLES_DIR_DE,
                    Market.FR => RECEIPT_EXAMPLES_DIR_FR,
                    _ => throw new ArgumentException(market.ToString(), nameof(market)),
                };

                await Demo.RunAsync(url, cashboxId, receiptExampleDir);
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
