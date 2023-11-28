using Common;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System;
using System.Fabric;
using System.Threading.Tasks;
using ITransaction = Common.ITransaction;


namespace Transaction_Cordinator
{
    public class TransactionService : ITransaction
    {
        IReliableStateManager StateManager { get; set; }

        public TransactionService(IReliableStateManager stateManager)
        {
            StateManager = stateManager;
        }

        public async Task Commit()
        {

            var commitDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, string>>("myStateCommit");
            using (var transaction = this.StateManager.CreateTransaction())
            {
                await commitDictionary.TryAddAsync(transaction, "1", "state 2");
                await transaction.CommitAsync();
            }
        }

        public async Task<bool> Prepare()
        {

            FabricClient fabricClient = new FabricClient();

            var partitionNumber = (await fabricClient.QueryManager.GetApplicationListAsync(new Uri("fabric:/Cloud_Zadatak/BookStore"))).Count;
            var binding = WcfUtility.CreateTcpClientBinding();

            int index = 0;

            ServicePartitionClient<WcfCommunicationClient<IBookStore>> servicePartitionClient = new
                ServicePartitionClient<WcfCommunicationClient<IBookStore>>(
                    new WcfCommunicationClientFactory<IBookStore>(clientBinding: binding),
                    new Uri("fabric:/Cloud_Zadatak/BookStore"),
                    new ServicePartitionKey(0)); //ovde se menja kasnije


            int partition_number1 = (await fabricClient.QueryManager.GetApplicationListAsync(new Uri("fabric:/Cloud_Zadatak/Bank"))).Count;

            int index1 = 0;

            ServicePartitionClient<WcfCommunicationClient<IBank>> servicePartitionClient2 = new
                ServicePartitionClient<WcfCommunicationClient<IBank>>(
                    new WcfCommunicationClientFactory<IBank>(clientBinding: binding),
                    new Uri("fabric:/Cloud_Zadatak/Bank"),
                    new ServicePartitionKey(0)); //ovde se menja kasnije

            //ovde commit samo provera treba da se obavi
            bool book = await servicePartitionClient.InvokeWithRetryAsync(client => client.Channel.Prepare());
            bool user = await servicePartitionClient2.InvokeWithRetryAsync(client => client.Channel.Prepare());
            if (book && user)
            {
                await Commit();
                return true;
            }
            else
            {
                await Rollback();
                return false;
            }

        }

        public async Task Rollback()
        {
            var rollb_dict = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, string>>("myStateRollback");
            using (var tx = this.StateManager.CreateTransaction())
            {
                await rollb_dict.TryAddAsync(tx, "1", "ret_state 1");
                await tx.CommitAsync();
            }
        }
    }
}
