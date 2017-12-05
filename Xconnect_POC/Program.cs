using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sitecore.XConnect;
using Sitecore.XConnect.Client;
using Sitecore.XConnect.Client.WebApi;
using Sitecore.XConnect.Collection.Model;
using Sitecore.XConnect.Schema;
using Sitecore.Xdb.Common.Web;


namespace Xconnect_POC
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("");
            Console.WriteLine("END OF PROGRAM.");
            Console.ReadKey();
        }

        private static async Task MainAsync(string[] args)
        {
            CertificateWebRequestHandlerModifierOptions options =
            CertificateWebRequestHandlerModifierOptions.Parse("StoreName=My;StoreLocation=LocalMachine;FindType=FindByThumbprint;FindValue=‎DE12BF44CAC821F2E5AD5C0C8E6158B65C98C4D4");

            var certificateModifier = new CertificateWebRequestHandlerModifier(options);

            List<IHttpClientModifier> clientModifiers = new List<IHttpClientModifier>();
            var timeoutClientModifier = new TimeoutHttpClientModifier(new TimeSpan(0, 0, 20));
            clientModifiers.Add(timeoutClientModifier);

            var collectionClient = new CollectionWebApiClient(new Uri("https://Sc91.xconnect/odata"), clientModifiers, new[] { certificateModifier });
            var searchClient = new SearchWebApiClient(new Uri("https://Sc91.xconnect/odata"), clientModifiers, new[] { certificateModifier });
            var configurationClient = new ConfigurationWebApiClient(new Uri("https://Sc91.xconnect/configuration"), clientModifiers, new[] { certificateModifier });

            var cfg = new XConnectClientConfiguration(
                new XdbRuntimeModel(CollectionModel.Model), collectionClient, searchClient, configurationClient);

            try
            {
                await cfg.InitializeAsync();

                // Print xConnect if configuration is valid
                var arr = new[]
                {
                        @"            ______                                                       __     ",
                        @"           /      \                                                     |  \    ",
                        @" __    __ |  $$$$$$\  ______   _______   _______    ______    _______  _| $$_   ",
                        @"|  \  /  \| $$   \$$ /      \ |       \ |       \  /      \  /       \|   $$ \  ",
                        @"\$$\/  $$| $$      |  $$$$$$\| $$$$$$$\| $$$$$$$\|  $$$$$$\|  $$$$$$$ \$$$$$$   ",
                        @" >$$  $$ | $$   __ | $$  | $$| $$  | $$| $$  | $$| $$    $$| $$        | $$ __  ",
                        @" /  $$$$\ | $$__/  \| $$__/ $$| $$  | $$| $$  | $$| $$$$$$$$| $$_____   | $$|  \",
                        @"|  $$ \$$\ \$$    $$ \$$    $$| $$  | $$| $$  | $$ \$$     \ \$$     \   \$$  $$",
                        @" \$$   \$$  \$$$$$$   \$$$$$$  \$$   \$$ \$$   \$$  \$$$$$$$  \$$$$$$$    \$$$$ "
                    };
                Console.WindowWidth = 160;
                foreach (string line in arr)
                    Console.WriteLine(line);

            }
            catch (XdbModelConflictException ce)
            {
                Console.WriteLine("ERROR:" + ce.Message);
                return;
            }

            // Initialize a client using the validated configuration
            using (var client = new XConnectClient(cfg))
            {
                try
                {
                    // Identifier for a 'known' contact
                    var identifier = new ContactIdentifier[]
                    {
                                new ContactIdentifier("twitter", "myrtlesitecore" + Guid.NewGuid().ToString("N"), ContactIdentifierType.Known)
                    };

                    // Print out the identifier that is going to be used
                    Console.WriteLine("Identifier:" + identifier[0].Identifier);

                    // Create a new contact with the identifier
                    Contact knownContact = new Contact(identifier);

                    client.AddContact(knownContact);

                    // Submit contact and interaction - a total of two operations
                    await client.SubmitAsync();

                    // Get the last batch that was executed
                    var operations = client.LastBatch;

                    Console.WriteLine("RESULTS...");

                    // Loop through operations and check status
                    foreach (var operation in operations)
                    {
                        Console.WriteLine(operation.OperationType + operation.Target.GetType().ToString() + " Operation: " + operation.Status);
                    }

                    Console.ReadLine();
                }
                catch (XdbExecutionException ex)
                {
                    // Deal with exception
                }
            }
        }
    }
}
