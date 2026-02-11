using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace TransactionNotifier.Services
{
    class TransactionService
    {
        private readonly DbService dblogic;

        public TransactionService(IConfiguration config)
        {
            dblogic = new DbService(config);
        }

        public Task<DataTable> GetPendingTransactionsAsync() => dblogic.GetPendingTransactionsAsync();

        public Task<int> UpdateTransactionStatusAsync( string tranId, string status,string? statusCode, string? telecomId)
            => dblogic.UpdateTransactionStatusAsync(tranId, status, statusCode, telecomId);
        public Task<int> PostTransaction(string tranId) => dblogic.PostTransaction(tranId);
    }
}
