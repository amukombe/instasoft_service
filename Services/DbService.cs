using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace TransactionNotifier.Services
{
    class DbService
    {
        private readonly string conStr;

        public DbService(IConfiguration config)
        {
            conStr = config.GetConnectionString("Constr")?? throw new InvalidOperationException("Missing connection string: Constr");
        }

        public async Task<DataTable> GetPendingTransactionsAsync()
        {
            var dt = new DataTable();

            await using var conn = new SqlConnection(conStr);
            await using var cmd = new SqlCommand("GetPendingTransactions", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 40000
            };

            await conn.OpenAsync();

            // DataTable needs DataAdapter (ADO.NET standard)
            using var adapter = new SqlDataAdapter(cmd);
            adapter.Fill(dt);

            return dt;
        }

        public async Task<int> UpdateTransactionStatusAsync(string tranId,string status,string? statusCode,string? telecomId)
        {
            await using var conn = new SqlConnection(conStr);
            await using var cmd = new SqlCommand("UpdateTransactionStatus", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 40000
            };

            cmd.Parameters.AddWithValue("@TranId", tranId);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@TelecomId", (object?)telecomId ?? DBNull.Value);

            await conn.OpenAsync();

            int result = await cmd.ExecuteNonQueryAsync();
            return result;
        }

        public async Task<int> PostTransaction(string tranId)
        {
            int result = 0;
            try
            {
                await using var conn = new SqlConnection(conStr);
                await using var cmd = new SqlCommand("PostTXNToGL", conn)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 40000
                };

                cmd.Parameters.AddWithValue("@TranId", tranId);

                await conn.OpenAsync();

                result = await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {

            }
            return result;
        }
    }
}
