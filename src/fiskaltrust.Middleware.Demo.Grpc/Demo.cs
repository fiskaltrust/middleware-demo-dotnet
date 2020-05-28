using fiskaltrust.ifPOS.v1;
using fiskaltrust.Middleware.Demo.Shared;
using fiskaltrust.Middleware.Interface.Client;
using fiskaltrust.Middleware.Interface.Client.Grpc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fiskaltrust.Middleware.Demo.Grpc
{
    public static class Demo
    {
        private static IPOS _pos = null;
        private static Guid _cashBoxId;
        private static Dictionary<string, ReceiptRequest> _examples;
        private static Dictionary<int, int> _journalOptions = new Dictionary<int, int>();

        public static async Task RunAsync(string url, Guid cashboxId, string receiptExampleDirectory)
        {
            _cashBoxId = cashboxId;
            var retryOptions = new RetryPolicyOptions { ClientTimeout = TimeSpan.FromSeconds(90), DelayBetweenRetries = TimeSpan.FromSeconds(5), Retries = 3 };
            _pos = await GrpcPosFactory.CreatePosAsync(new GrpcClientOptions { Url = new Uri(url), RetryPolicyOptions = retryOptions });
            _examples = LoadExamples(receiptExampleDirectory, cashboxId);

            await ExecuteEchoAsync("Test");

            while (true)
            {
                await MenuAsync();
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

        private static async Task ExecuteEchoAsync(string message)
        {
            try
            {
                var result = await _pos.EchoAsync(new EchoRequest
                {
                    Message = message
                });
                if (result.Message != message)
                {
                    throw new Exception("The Echo request did not return the expected result.");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        private static async Task ExecuteSignAsync(ReceiptRequest req)
        {
            try
            {
                ConsoleHelper.PrintRequest(req);
                var resp = await _pos.SignAsync(req);
                ConsoleHelper.PrintResponse(resp);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("An error occured when trying to send the request.");
                Console.Error.WriteLine(ex);
            }
        }

        private static async Task ExecuteJournalAsync(string input)
        {
            Console.Clear();
            if (!int.TryParse(input, out var inputInt) || inputInt > 9)
            {
                Console.WriteLine($"\"{input}\" is not a valid input.");
            }
            else
            {
                switch (inputInt)
                {
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                        var journal = await GetJournalAsync(inputInt - 1);
                        var result = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<dynamic>(journal), Formatting.Indented);
                        Console.WriteLine(result);
                        break;
                    case 5:
                        journal = await GetJournalAsync(0x4154);
                        result = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<dynamic>(journal), Formatting.Indented);
                        Console.WriteLine(result);
                        break;
                    case 6:
                        journal = await GetJournalAsync(0x4445);
                        result = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<dynamic>(journal), Formatting.Indented);
                        Console.WriteLine(result);
                        break;
                    case 7:
                        journal = await GetJournalAsync(0x4652);
                        result = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<dynamic>(journal), Formatting.Indented);
                        Console.WriteLine(result);
                        break;
                    case 8:
                        journal = await GetJournalAsync(0x4445000000000000);
                        result = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<dynamic>(journal), Formatting.Indented);
                        Console.WriteLine(result);
                        break;
                    case 9:
                        var fileName = await SaveJournalToFileAsync(0x4445000000000001);
                        Console.WriteLine($"Successfully exported TAR file to -> {fileName}");
                        break;
                }
        
                Console.WriteLine("Please press enter to continue.");
                Console.ReadLine();
            }
        }

        private static async Task<string> GetJournalAsync(long inputInt)
        {
            using var memoryStream = new MemoryStream();
            await foreach (var chunk in _pos.JournalAsync(new JournalRequest
            {
                ftJournalType = inputInt
            }))
            {
                var write = chunk.Chunk.ToArray();
                await memoryStream.WriteAsync(write, 0, write.Length);
            }
            
            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }

        private static async Task<string> SaveJournalToFileAsync(long inputInt)
        {
            var fileName = $"exportTSE_{DateTime.Now.Ticks}.tar";
            using var fileStream = File.OpenWrite(fileName);
            await foreach (var chunk in _pos.JournalAsync(new JournalRequest
            {
                ftJournalType = inputInt
            }))
            {
                var write = chunk.Chunk.ToArray();
                await fileStream.WriteAsync(write, 0, write.Length);
            }
            return fileName;
        }

        private static async Task MenuAsync()
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
            else if (inputInt > _examples.Keys.Count)
            {
                Console.Clear();
                Console.WriteLine("Please select a Journal:");
                Console.WriteLine($"<1>: Journal 0x0000000000000000 Version information");
                Console.WriteLine($"<2>: Journal 0x0000000000000001 ActionJournals in internal format");
                Console.WriteLine($"<3>: Journal 0x0000000000000002 ReceiptJournals in internal format");
                Console.WriteLine($"<4>: Journal 0x0000000000000003 QueueItems in internal format");
                Console.WriteLine($"<5>: Journal 0x0000000000004154 JournalAT in internal format");
                Console.WriteLine($"<6>: Journal 0x0000000000004445 JournalDE in internal format");
                Console.WriteLine($"<7>: Journal 0x0000000000004652 JournalFR in internal format");
                Console.WriteLine($"<8>: Journal 0x4445000000000000 QueueDE Status");
                Console.WriteLine($"<9>: Journal 0x4445000000000001 TSE-TAR File export (Creates file with contents at {Directory.GetCurrentDirectory()})");
                await ExecuteJournalAsync(Console.ReadLine());
            }
            else
            {
                var req = _examples.Values.ToList()[inputInt - 1];
                await ExecuteSignAsync(req);
                Console.WriteLine("Please press enter to continue.");
                Console.ReadLine();
                Console.Clear();
            }
            await MenuAsync();
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
