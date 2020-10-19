using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Transcriber.Core;
using Transcriber.Core.Results;

namespace IbisTranscriber.AzureFunctions
{
    public static class TriggerSpeechToText
    {
        [FunctionName("TriggerSpeechToText")]
        public static async Task Run([BlobTrigger("projects/{name}.wav", Connection = "AzureWebJobsStorage")]Stream myBlob, 
            [Blob("transcribedfiles/{name}.txt",FileAccess.Write, Connection = "TextStorage")] Stream textBlob,
            [CosmosDB(
                databaseName: "transcriber-db",
                collectionName: "Result",
                ConnectionStringSetting = "CosmosDBConnection")]IAsyncCollector<Result> results,
            [CosmosDB(
                databaseName: "transcriber-db",
                collectionName: "Project",
                ConnectionStringSetting = "CosmosDBConnection")]DocumentClient client,
            string name, ILogger log)
        {
            var config = SpeechConfig.FromSubscription(Environment.GetEnvironmentVariable("ApiKey", EnvironmentVariableTarget.Process),
                Environment.GetEnvironmentVariable("Region", EnvironmentVariableTarget.Process));

            config.RequestWordLevelTimestamps();
            //string fromLanguage = "en-US";
            //config.SpeechRecognitionLanguage = fromLanguage;
            //config.OutputFormat = OutputFormat.Simple;

            var projectId = name.Split('/')[0];
            Uri driverCollectionUri = UriFactory.CreateDocumentCollectionUri(databaseId: "transcriber-db", collectionId: "Project");

            var options = new FeedOptions { EnableCrossPartitionQuery = true }; // Enable cross partition query

            Project project = client.CreateDocumentQuery<Project>(driverCollectionUri, options)
                                        .AsEnumerable()
                                        .First(p => p.Id == projectId);

            

            var completionSource = new TaskCompletionSource<int>();

            using(var audioInput = AudioConfig.FromStreamInput(new AudioStreamReader(myBlob)))
            using (var recognizer = new SpeechRecognizer(config, audioInput))
            {
                var streamWriter = new StreamWriter(textBlob);

                recognizer.Recognized += async (s, e) =>
                {
                    var r = e.Result;
                    //log.LogInformation("Recognized");
                    //Result result = new Result(projectId);
                    //result.Id = r.ResultId;
                    //result.RegisterDate = DateTime.UtcNow;
                    //result.DisplayText = r.Text;
                    //result.RecognitionStatus = r.Reason.ToString();

                    var resultstring = r.ToString();

                    var jsonIndex = resultstring.IndexOf("Json:");
                    var stringt = resultstring.Substring(jsonIndex + 5, resultstring.Length - (jsonIndex+5));
                    //var t = r.Properties.GetProperty("DisplayText");

                    Result rs = JsonSerializer.Deserialize<Result>(stringt);
                    rs.PartitionKey = projectId;
                    rs.ProjectID = projectId;
                    rs.Id = r.ResultId;
                    rs.RecognitionStatus = r.Reason.ToString();
                    rs.RegisterDate = DateTime.UtcNow;
                    rs.DisplayText = r.Text;

                    await results.AddAsync(rs);

                    streamWriter.Write(e.Result);
                };



                recognizer.SessionStopped += async (s, e) =>
                {
                    //todo update project status
                    project.Status = "Processed";
                    await client.UpsertDocumentAsync(driverCollectionUri, project);
                    //log.LogInformation("session stopped");
                    streamWriter.Flush();
                    streamWriter.Dispose();
                    completionSource.TrySetResult(0);
                };

                await recognizer.StartContinuousRecognitionAsync()
                    .ConfigureAwait(false);

                await Task.WhenAny(new[] { completionSource.Task });
                //log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
                await recognizer.StopContinuousRecognitionAsync()
                    .ConfigureAwait(false);


            }
            
        }

       
    }
}
