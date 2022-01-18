﻿using System.Data.Common;
using System.Threading.Tasks;

namespace Apollo.Core.Dal.Factory
{
    public interface IConnectionFactory
    {
        string ConnectionString { get; }

        string ProviderName { get; }

        Task<DbConnection> CreateConnection();

    }
}