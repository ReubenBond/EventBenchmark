﻿using System;
using System.Collections.Generic;
using Common.Ingestion.Worker;

namespace Common.Scenario
{

    public enum SubmissionEnum
    {
        QUANTITY,
        TIME_IN_MILLI
    }

    public class ScenarioConfiguration
    {

        public SubmissionStrategy submissionStrategy = SubmissionStrategy.BURST_THEN_CONTROL;

        //
        public SubmissionEnum submissionType = SubmissionEnum.TIME_IN_MILLI;

        // how much time a window or burst may remain. in milliseconds
        public int windowOrBurstValue = 1000;

        // a timer is configured to notify the orchestrator grain about the termination
        public TimeSpan timeSpan = TimeSpan.FromSeconds(60);

        // e.g. 10 entries, new order has 7 entries and price update 3, meaning 70% probability of new order
        public TransactionType[] weight;

        // usually the same as the ingestion
        // but as new microservices might be added here
        // we have this attribute in the config
        public Dictionary<string, string> mapTableToUrl;

        // map kafka topic to orleans stream Guid
        public Dictionary<string, Guid> mapTopicToStreamGuid;

    }
}