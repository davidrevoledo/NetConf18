using System;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;

namespace DeviceEventProcessing
{
    internal class Program
    {
        private const string EventHubName = "temperature1";
        private const string StorageContainerName = "checkpoints";

        private const string StorageAccountKey =
            "DefaultEndpointsProtocol=https;AccountName=eventhubdemo1234;AccountKey=scMHMTW+Uwai6ZpVPAFwHlbAO3ZXgcibcVcxVnZXT6grweBt06jB5o6tXlxaomGU+3300jfe9C64s13s5R3LOA==;EndpointSuffix=core.windows.net";

        private const string EventHubConnectionString =
            @"Endpoint=sb://netconfar2018.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=byPgdEE3mUYh9BSlUkcI1fyUBwlTplXcvcvEbyut9M8=";

        private static void Main(string[] args)
        {
            MainAsync()
                .GetAwaiter()
                .GetResult();
        }

        private static async Task MainAsync()
        {
            Console.WriteLine("Registering EventProcessor...");

            var eventProcessorHost = new EventProcessorHost(
                EventHubName,
                PartitionReceiver.DefaultConsumerGroupName,
                EventHubConnectionString,
                StorageAccountKey,
                StorageContainerName);

            // Registers the Event Processor Host and starts receiving messages
            await eventProcessorHost.RegisterEventProcessorAsync<TemperatureMeasurementProccesor>();

            await EventStorage.Start(90, 5);

            Console.WriteLine("Receiving. Press ENTER to stop worker.");
            Console.ReadLine();

            // Disposes of the Event Processor Host
            await eventProcessorHost.UnregisterEventProcessorAsync();
        }
    }
}