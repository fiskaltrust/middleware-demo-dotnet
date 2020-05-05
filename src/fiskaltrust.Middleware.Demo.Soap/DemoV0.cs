using fiskaltrust.ifPOS.v0;
using fiskaltrust.Middleware.Demo.Shared;
using fiskaltrust.Middleware.Interface.Soap;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace fiskaltrust.Middleware.Demo.Soap
{
    public static class DemoV0
    {
        private static IPOS _pos = null;
        private static Guid _cashBoxId;
        private static Dictionary<string, ReceiptRequest> _examples;
        private static Dictionary<int, int> _journalOptions = new Dictionary<int, int>();

        public static void Run(string url, Guid cashboxId, string receiptExampleDirectory)
        {
            _cashBoxId = cashboxId;
            _pos = new SoapPosFactory().CreatePosAsync(new SoapPosOptions { Url = url });
            _examples = LoadExamples(receiptExampleDirectory, cashboxId);

            ExecuteEcho("Test");

            while (true)
            {
                Menu();
            }
        }

        private static Dictionary<string, ReceiptRequest> LoadExamples(string receiptExampleDirectory, Guid cashboxId)
        {
            var examples = new Dictionary<string, ReceiptRequest>();
            foreach (var file in Directory.GetFiles(receiptExampleDirectory, "*.json", SearchOption.AllDirectories).OrderBy(f => f))
            {
                var content = File.ReadAllText(file);
                var req = JsonConvert.DeserializeObject<ReceiptRequest>(content);
                req.ftCashBoxID = cashboxId.ToString();
                examples.Add(file.Replace("\\", "/"), req);
            }

            return examples;
        }

        private static void ExecuteEcho(string message)
        {
            try
            {
                var result = _pos.Echo(message);
                if (result != message)
                {
                    throw new Exception("The Echo request did not return the expected result.");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        private static void ExecuteSign(ReceiptRequest req)
        {
            try
            {
                ConsoleHelper.PrintRequest(req);
                var resp = _pos.Sign(req);
                ConsoleHelper.PrintResponse(resp);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("An error occured when trying to send the request.");
                Console.Error.WriteLine(ex);
            }
        }

        private static void ExecuteJournal(string input)
        {
            Console.Clear();
            if (!int.TryParse(input, out var inputInt) || inputInt > 4)
            {
                Console.WriteLine($"\"{input}\" is not a valid input.");
            }
            else
            {
                var journal = GetJournal(inputInt);
                var result = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<dynamic>(journal), Formatting.Indented);
                Console.WriteLine(result);
                Console.WriteLine("Please press enter to continue.");
                Console.ReadLine();
            }
        }

        private static string GetJournal(int inputInt)
        {
            using var streamReader = new StreamReader(_pos.Journal(inputInt, 0, int.MaxValue));
            return streamReader.ReadToEnd();
        }

        private static void Menu()
        {
            Console.WriteLine();
            PrintOptions();
            var input = Console.ReadLine();
            if (input.Trim() == "exit")
            {
                Environment.Exit(0);
            }
            else if (!int.TryParse(input, out var inputInt))
            {
                Console.WriteLine($"\"{input}\" is not a valid input.");
            }
            else if (inputInt > _examples.Keys.Count - 1)
            {
                Console.Clear();
                Console.WriteLine("Please select a Journal:");
                Console.WriteLine($"<1>: Journal 0x0000000000000000 Version information");
                Console.WriteLine($"<2>: Journal 0x0000000000000001 ActionJournal in internal format");
                Console.WriteLine($"<3>: Journal 0x0000000000000002 ReceiptJournal in internal format");
                Console.WriteLine($"<4>: Journal 0x0000000000000003 QueueItemJournal in internal format");
                ExecuteJournal(Console.ReadLine());
            }
            else
            {
                var req = _examples.Values.ToList()[inputInt - 1];
                ExecuteSign(req);
                Console.WriteLine("Please press enter to continue.");
                Console.ReadLine();
                Console.Clear();
            }
            Menu();
        }

        private static void PrintOptions()
        {
            var i = 1;
            foreach (var example in _examples)
            {
                Console.WriteLine($"<{i}>: {example.Key} - ({example.Value.ftReceiptCase:X})");
                i++;
            }

            Console.WriteLine($"<{i}>: Journal");
            Console.WriteLine("exit: Program beenden");
        }
    }
}
