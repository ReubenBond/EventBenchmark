﻿using Common.Ingestion;
using GrainInterfaces.Ingestion;
using Orleans;
using Orleans.Concurrency;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Grains.Ingestion
{
    /**
     * The unit of parallelism for data ingestion
     */
    [StatelessWorker]
    public class IngestionWorker : Grain, IIngestionWorker
    {

        // https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=net-7.0
        private static readonly HttpClient client = new HttpClient();

        private static readonly String httpJsonContentType = "application/json";

        private static readonly Encoding encoding = Encoding.UTF8;

        public async override Task OnActivateAsync()
        {
            return;
        }

        public Task Send(IngestionBatch batch)
        {
            Task<HttpStatusCode>[] response = RunBatch(batch);
            Task.WaitAll(response);
            // return;
            return Task.CompletedTask;
        }

        public async Task Send(List<IngestionBatch> batches)
        {

            Console.WriteLine("Batches received: "+ batches.Count);

            List<Task<HttpStatusCode>[]> responses = new List<Task<HttpStatusCode>[]>();

            foreach(IngestionBatch batch in batches) 
            {
                responses.Add(RunBatch(batch));
            }

            Console.WriteLine("All http requests sent");

            foreach (Task<HttpStatusCode>[] tasks in responses)
            {
                // TimeSpan timeout = TimeSpan.FromSeconds(5.0);
                foreach (Task<HttpStatusCode> task in tasks) 
                {
                    await task; 
                }
            }

            Console.WriteLine("All responses received");
            return;
        }

        private static Task<HttpStatusCode>[] RunBatch(IngestionBatch batch)
        {
            Task<HttpStatusCode>[] responses = new Task<HttpStatusCode>[batch.data.Count];
            int idx = 0;
            foreach (string payload in batch.data)
            {
                // https://learn.microsoft.com/en-us/dotnet/orleans/grains/external-tasks-and-grains
                // https://stackoverflow.com/questions/10343632/httpclient-getasync-never-returns-when-using-await-async
                // https://blog.stephencleary.com/2012/07/dont-block-on-async-code.html
                responses[idx] = (Task.Run(() =>
                {
                    try
                    {
                        HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, batch.url);
                        message.Content = BuildPayload(payload);
                        using HttpResponseMessage response = client.Send(message);
                        // response.EnsureSuccessStatusCode();
                        // Console.WriteLine("Here we are: " + response.StatusCode);
                        return response.StatusCode;
                    }
                    catch (HttpRequestException e)
                    {
                        Console.WriteLine("\nException Caught!");
                        Console.WriteLine("Message: {0}", e.Message);
                        return HttpStatusCode.ServiceUnavailable; // e.StatusCode.Value;
                    }
                }
                ));

                idx++;

            }
            return responses;
        }

        private static StringContent BuildPayload(string item)
        {
            return new StringContent(item, encoding, httpJsonContentType);
        }

    }
}