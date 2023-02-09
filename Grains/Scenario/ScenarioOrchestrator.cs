﻿using System;
using Orleans;
using GrainInterfaces.Scenario;
using Common.Customer;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace Grains.Scenario
{
	public class ScenarioOrchestrator : Grain, IScenarioOrchestrator
    {


        public async override Task OnActivateAsync()
        {
            return;
        }

        public Task Run(CustomerConfiguration config)
        {

            // what do I need? the transactions. dictionary of name of transaction and percentage
            // for each transaction, the distribution of keys. checkout will create many customer workers with this distribution.

            // setup timer according to the config passed. the timer defines the end of the experiment

            // each type of transaction is submitted independently by a corresponding stateless? worker

            // on timer, this actor messages them

            // defines an initial rate of transaction submission. messathe workers if that changes over the workload.

            // sets up the service grain. for each external service, properly set the event listener

            return Task.CompletedTask;
        }

    }
}
