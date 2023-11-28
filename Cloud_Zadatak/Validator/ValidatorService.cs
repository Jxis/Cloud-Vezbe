using Common;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System;
using System.Fabric;
using System.Threading.Tasks;
using ITransaction = Common.ITransaction;

namespace Validator
{
    public class ValidatorService : IValidator
    {
        public async Task<bool> Check(string user, string book, int count)
        {
            if (count > 0)
            {
                // Kreiramo FabricClient da komunicira sa Service Fabric
                FabricClient fabricClient = new FabricClient();

                // Uzmemo broj particija za aplikaciju, da bi komunicirao sa TransactionCoordiantor
                int partitionNumber = (await fabricClient.QueryManager.GetApplicationListAsync(new Uri("fabric:/Cloud_Zadatak/Transaction_Cordinator"))).Count;

                // kreiramo TCP za WCF
                var binding = WcfUtility.CreateTcpListenerBinding();

                ServicePartitionClient<WcfCommunicationClient<ITransaction>> servicePartitionClient = new
                    ServicePartitionClient<WcfCommunicationClient<ITransaction>>(new WcfCommunicationClientFactory<ITransaction>(clientBinding: binding),
                    new Uri("fabric:/Cloud_Zadatak/Transaction_Cordinator"),
                    new ServicePartitionKey());


            }
            return false;
        }
    }
}
