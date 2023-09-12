﻿using Common.Entities;
using Common.Experiment;
using Common.Infra;
using Common.Workload;
using Common.Services;
using Common.Workers;
using Common.Workers.Seller;
using Dapr.Workload;
using Common.Ingestion;
using Microsoft.Extensions.Logging;
using Orleans.Workers;
using Common.Workers.Customer;
using Common.Http;
using Orleans.Metric;

namespace Orleans.Workload;

public class ActorExperimentManager : ExperimentManager
{

    private readonly IHttpClientFactory httpClientFactory;

    private readonly SellerService sellerService;
    private readonly CustomerService customerService;
    private readonly DeliveryService deliveryService;

    private readonly Dictionary<int, ISellerWorker> sellerThreads;
    private readonly Dictionary<int, AbstractCustomerThread> customerThreads;
    private readonly DeliveryThread deliveryThread;

    private int numSellers;

    private readonly ActorWorkloadManager workloadManager;
    private readonly ActorMetricManager metricManager;

    public ActorExperimentManager(IHttpClientFactory httpClientFactory, ExperimentConfig config) : base(config)
    {
        this.httpClientFactory = httpClientFactory;

        this.deliveryThread = DeliveryThread.BuildDeliveryThread(httpClientFactory, config.deliveryWorkerConfig);
        this.deliveryService = new DeliveryService(this.deliveryThread);

        this.sellerThreads = new Dictionary<int, ISellerWorker>();
        this.sellerService = new SellerService(this.sellerThreads);
        this.customerThreads = new Dictionary<int, AbstractCustomerThread>();
        this.customerService = new CustomerService(this.customerThreads);

        this.numSellers = 0;

        this.workloadManager = new ActorWorkloadManager(
            sellerService, customerService, deliveryService,
            config.transactionDistribution,
            // set in the base class
            this.customerRange,
            config.concurrencyLevel,
            config.executionTime,
            config.delayBetweenRequests);

        this.metricManager = new ActorMetricManager(sellerService, customerService, deliveryService);
    }

    protected override async void PreExperiment()
    {
        // reset microservice states
        var resps_ = new List<Task<HttpResponseMessage>>();
        foreach (var task in config.postRunTasks)
        {
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Patch, task.url);
            logger.LogInformation("Pre experiment task to URL {0}", task.url);
            resps_.Add(HttpUtils.client.SendAsync(message));
        }
        await Task.WhenAll(resps_);

        for (int i = this.customerRange.min; i <= this.customerRange.max; i++)
        {
            this.customerThreads.Add(i, ActorCustomerThread.BuildCustomerThread(httpClientFactory, sellerService, config.numProdPerSeller, config.customerWorkerConfig, customers[i-1]));
        }
    }

    protected override async void RunIngestion()
    {
        var ingestionOrchestrator = new IngestionOrchestrator(config.ingestionConfig);
        await ingestionOrchestrator.Run(connection);
    }

    protected override void PreWorkload(int runIdx)
    {
        this.numSellers = (int)DuckDbUtils.Count(connection, "sellers");

        for (int i = 1; i <= numSellers; i++)
        {
            List<Product> products = DuckDbUtils.SelectAllWithPredicate<Product>(connection, "products", "seller_id = " + i);
            if (!sellerThreads.ContainsKey(i))
            {
                sellerThreads[i] = ActorSellerThread.BuildSellerThread(i, httpClientFactory, config.sellerWorkerConfig);
                sellerThreads[i].SetUp(products, config.runs[runIdx].keyDistribution);
            }
            else
            {
                sellerThreads[i].SetUp(products, config.runs[runIdx].keyDistribution);
            }
        }

        Interval sellerRange = new Interval(1, this.numSellers);
        for (int i = customerRange.min; i <= customerRange.max; i++)
        {
            this.customerThreads[i].SetUp(this.config.runs[runIdx].sellerDistribution, sellerRange, this.config.runs[runIdx].keyDistribution);
        }

    }

    protected override WorkloadManager SetUpManager(int runIdx)
    {
        this.workloadManager.SetUp(config.runs[runIdx].sellerDistribution, new Interval(1, this.numSellers));
        return workloadManager;
    }

    protected override void Collect(int runIdx, DateTime startTime, DateTime finishTime)
    {
        string ts = new DateTimeOffset(startTime).ToUnixTimeMilliseconds().ToString();
        this.metricManager.SetUp(numSellers, config.numCustomers);
        this.metricManager.Collect(startTime, finishTime, config.epoch, string.Format("{0}#{1}_{2}_{3}_{4}_{5}", ts, runIdx, config.concurrencyLevel,
                    config.runs[runIdx].numProducts, config.runs[runIdx].sellerDistribution, config.runs[runIdx].keyDistribution));
    }

    protected override async void PostExperiment()
    {
        // cleanup microservice states
        var resps_ = new List<Task<HttpResponseMessage>>();
        foreach (var task in config.postExperimentTasks)
        {
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Patch, task.url);
            logger.LogInformation("Pre experiment task to URL {0}", task.url);
            resps_.Add(HttpUtils.client.SendAsync(message));
        }
        await Task.WhenAll(resps_);
    }

    protected override async void PostRunTasks(int runIdx, int lastRunIdx)
    {
        // reset microservice states
        var resps_ = new List<Task<HttpResponseMessage>>();
        foreach (var task in config.postRunTasks)
        {
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Patch, task.url);
            logger.LogInformation("Post run task to URL {0}", task.url);
            resps_.Add(HttpUtils.client.SendAsync(message));
        }
        await Task.WhenAll(resps_);
    }

    protected override void TrimStreams()
    {
        // nothing to do for orleans
    }
}