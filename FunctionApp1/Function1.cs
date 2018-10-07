using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Avro.File;
using Avro.Generic;
using IOTDemo;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EventProcessor
{
    public static class Function1
    {
        private const string StorageConnectionString =
            @"DefaultEndpointsProtocol=https;AccountName=eventhubdemo1234;AccountKey=scMHMTW+Uwai6ZpVPAFwHlbAO3ZXgcibcVcxVnZXT6grweBt06jB5o6tXlxaomGU+3300jfe9C64s13s5R3LOA==;EndpointSuffix=core.windows.net";

        [FunctionName("EventGridTriggerMigrateData")]
        public static void Run([EventGridTrigger] JObject eventGridEvent, TraceWriter log)
        {
            log.Info("C# EventGrid trigger function processed a request.");
            log.Info(eventGridEvent.ToString(Formatting.Indented));

            try
            {
                // Copy to a static Album instance
                var ehEvent = eventGridEvent.ToObject<EventGridEHEvent>();
                var uri = new Uri(ehEvent.data.fileUrl);
                var measurements = Dump(uri);
            }
            catch (Exception e)
            {
                var s = string.Format(CultureInfo.InvariantCulture,
                    "Error processing request. Exception: {0}, Request: {1}", e, eventGridEvent.ToString());
                log.Error(s);
            }
        }

        /// <summary>
        ///     Dumps the data from the Avro blob to the data warehouse (DW).
        ///     Before running this, ensure that the DW has the required <see cref="TableName" /> table created.
        /// </summary>
        private static IEnumerable<DeviceMeasurement> Dump(Uri fileUri)
        {
            // Get the blob reference
            var storageAccount = CloudStorageAccount.Parse(StorageConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var blob = blobClient.GetBlobReferenceFromServer(fileUri);

            // Parse the Avro File
            using (var avroReader = DataFileReader<GenericRecord>.OpenReader(blob.OpenRead()))
            {
                while (avroReader.HasNext())
                {
                    var r = avroReader.Next();

                    var body = (byte[])r["Body"];

                    var payload = Encoding.ASCII.GetString(body);
                    var measurement = JsonConvert.DeserializeObject<DeviceMeasurement>(payload);
                    yield return measurement;
                }
            }
        }
    }
}