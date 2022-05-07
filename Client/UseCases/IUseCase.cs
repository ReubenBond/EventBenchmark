﻿using System;
using System.Collections.Generic;
using Client.Configuration;

namespace Client.UseCases.eShop
{
    public interface IUseCaseConfig
    {

        List<String> GetTransactions();

        List<int> GetPercentageOfTransactions();

        List<TimeSpan> GetPeriodBetweenRequestsOfSameTransaction();

        List<int> GetNumberOfRequestsPerTransaction(); // 0 if no limit

        TimeSpan? TimeLimit(); // limit of time if applicable

        Distribution GetDistribution();

    }
}
