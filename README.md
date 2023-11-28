# Cloud-Vezbe

![image](https://github.com/Jxis/Cloud-Vezbe/assets/24139683/b400c3f6-2c39-465c-8a25-8f48be103dd1)


ServiceFabricApplication\
-> Stateless  ASP.Net Framerowork - Client\
-> Stateless service .Net framework - Validator\
-> Class Library - Common\
-> Statefull service - TransactionCoordinator\
-> Statefull service - BookStore\
-> Statefull service - Bank
##
Stateless\
Stateless listeners - ima svoj ServiceEndpoint (Manifest koji sluzi za komunikaciju to obtain the port on which to listen.)
```
protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
{
    return new List<ServiceInstanceListener>(1)
    {
        new ServiceInstanceListener(context => this.CreateWcfCommunication(context),"ServiceEndpoint")
    };
}
```
```
 private ICommunicationListener CreateWcfCommunication(StatelessServiceContext context)
 {
     string host = context.NodeContext.IPAddressOrFQDN;
     var serviceEndPoint = context.CodePackageActivationContext.GetEndpoint("ServiceEndpoint");
     int port = serviceEndPoint.Port;
     var scheme = serviceEndPoint.Protocol.ToString();

     string uri = string.Format("net.{0}://{1}:{2}/ServiceEndpoint", scheme, host, port);

     var listener = new WcfCommunicationListener<IValidator>(
         serviceContext: context,
         wcfServiceObject: new ValidatorService(),
         listenerBinding: WcfUtility.CreateTcpListenerBinding(),
         address: new System.ServiceModel.EndpointAddress(uri));
     return listener;
 }
```
Servisna komunikacija - Validator komunicira sa Transaction_Coordinator
```
ServicePartitionClient<WcfCommunicationClient<ITransaction>> servicePartitionClient = new
    ServicePartitionClient<WcfCommunicationClient<ITransaction>>(new WcfCommunicationClientFactory<ITransaction>(clientBinding: binding),
    new Uri("fabric:/Cloud_Zadatak/Transaction_Cordinator"),
    new ServicePartitionKey());
```
Logovanje - ServiceEventSource - svaki mikroservis mora da ima ovu klasu!

ServiceManifest xml file ima : name, protocol: http,tcp.. , port, type: input

ApplicationManifest xml file :\
-> Statefull : TargetReplicaSetSize, MinReplicaSetSize, PartitionCount\
-> Stateless : InstanceCount\

Client - Stateless web, isto kao ASP.NET Web Api, ne koristi se IIS, podela na WEB API aplikaciju i Host
#
Stateful - cuvaju stanje mikroservisa preko RELIABLE kolekcije, transakcija zavrsi tek nakon sto se celoukupna transakcija evidentira na svim replikama, cak i na primarnoj instanci mikroservisa\
-> Reliable Dictionary - replicirana, transakciona i asinhrona kolekcija parova kljuc/vrednost\
-> Reliable Queue - replicirani, transakcioni i asinhroni red koji je FIFO\
-> Reliable concurent queue - replicirani, transakcioni i asinhroni redi koji je FIFO i sluzi za visoku propusnost

Stateful listeners
```
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
```
Komunikacija izmedju mikroservisa
```
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
```

Reliable kolekcije:
```
var bankDictionary = await this.ReliableStateManager.GetOrAddAsync<IReliableDictionary<string, Common.Models.Bank>>("banks");
using (var transaction = this.ReliableStateManager.CreateTransaction())
{
    var bank = await bankDictionary.TryGetValueAsync(transaction, "1");


    await transaction.CommitAsync();
    return bank.Value;
}
```



