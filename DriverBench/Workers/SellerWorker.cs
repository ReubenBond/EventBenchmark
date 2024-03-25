﻿using Common.Entities;
using Common.Infra;
using Common.Workers.Seller;
using Common.Workload;
using Common.Workload.Metrics;
using Common.Workload.Seller;
using Microsoft.Extensions.Logging;

namespace DriverBench.Workers;

public sealed class SellerWorker : AbstractSellerWorker
{
    private SellerWorker(int sellerId, SellerWorkerConfig workerConfig, ILogger logger) : base(sellerId, workerConfig, logger)
    {
    }

    public static SellerWorker BuildSellerWorker(int sellerId, IHttpClientFactory httpClientFactory, SellerWorkerConfig workerConfig)
    {
        var logger = LoggerProxy.GetInstance("SellerThread_" + sellerId);
        return new SellerWorker(sellerId, workerConfig, logger);
    }

    public override void BrowseDashboard(string tid)
    {
        this.submittedTransactions.Add(new TransactionIdentifier(tid, TransactionType.QUERY_DASHBOARD, DateTime.UtcNow));
        Thread.Sleep(100);
        this.finishedTransactions.Add(new TransactionOutput(tid, DateTime.UtcNow));
    }

    protected override void SendProductUpdateRequest(Product product, string tid)
    {
        this.submittedTransactions.Add(new TransactionIdentifier(tid, TransactionType.UPDATE_PRODUCT, DateTime.UtcNow));
        Thread.Sleep(100);
        this.finishedTransactions.Add(new TransactionOutput(tid, DateTime.UtcNow));
    
    }

    protected override void SendUpdatePriceRequest(Product productToUpdate, string tid)
    {
        this.submittedTransactions.Add(new TransactionIdentifier(tid, TransactionType.PRICE_UPDATE, DateTime.UtcNow));
        Thread.Sleep(100);
        this.finishedTransactions.Add(new TransactionOutput(tid, DateTime.UtcNow));
    }
}
