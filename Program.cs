using System.Data;
using System.Transactions;
using Microsoft.Extensions.Hosting;
using TransactionNotifier.Models;
using TransactionNotifier.Services;

var builder = Host.CreateApplicationBuilder(args);
var config = builder.Configuration;
// 🔹 Services
var tranSvc = new TransactionService(config);
var apiClient = new MomoApi(config);


while (true)
{
    try
    {

        DataTable pending = await tranSvc.GetPendingTransactionsAsync();
        Console.WriteLine("Found : " + pending.Rows.Count + " pending Phone to Wallet transactions");
        foreach (DataRow row in pending.Rows)
        {
            var tranId = row["TranId"]?.ToString();
            if (string.IsNullOrWhiteSpace(tranId))
                continue;
            try
            {
                var statusResp = await apiClient.GetTransactionStatusAsync(tranId);
                var status = statusResp.Status?.Trim().ToUpperInvariant();

                Console.WriteLine("Processed: " + tranId + " Status: " + statusResp.Status);
                if (status == "SUCCESSFUL")
                {
                    int result = 0;
                     result=await tranSvc.UpdateTransactionStatusAsync( tranId,"SUCCESS","0",
                        statusResp.FinancialTransactionId);
                    if (result == 1)
                        tranSvc.PostTransaction(tranId);
                }
                else if (status == "FAILED" || status == "REJECTED")
                {
                    await tranSvc.UpdateTransactionStatusAsync(
                        tranId,
                        "FAILED",
                        "105",
                        statusResp.FinancialTransactionId
                    );
                }
                // else still pending → do nothing
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing TranId={tranId}: {ex.Message}");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Worker loop error: {ex.Message}");
    }

    await Task.Delay(TimeSpan.FromSeconds(20));
}