using Common;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Bank
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    public sealed class Bank : StatefulService
    {
        public Bank(StatefulServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[]{
                new ServiceReplicaListener(context =>
                {
                    return new  WcfCommunicationListener<IBank>(context,
                            new BankService(this.StateManager),
                            WcfUtility.CreateTcpListenerBinding(),
                            "ServiceEndpoint"

                        );
                },"ServiceEndpoint")
            };
        }

        private async Task SetBooks()
        {
            try
            {
                var banksDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, Common.Models.Bank>>("banks");
                using (var transaction = this.StateManager.CreateTransaction())
                {
                    await banksDictionary.TryAddAsync(transaction, "1", new Common.Models.Bank() { Name = "OTP", Year = 99 });
                    await transaction.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.
            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");
            var banksDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, Common.Models.Bank>>("banks");

            await SetBooks();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var result = await myDictionary.TryGetValueAsync(tx, "Counter");

                    ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}",
                        result.HasValue ? result.Value.ToString() : "Value does not exist.");

                    await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

                    // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                    // discarded, and nothing is saved to the secondary replicas.
                    await tx.CommitAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
