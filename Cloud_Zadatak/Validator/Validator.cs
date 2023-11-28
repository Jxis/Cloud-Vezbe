using Common;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Validator
{

    internal sealed class Validator : StatelessService
    {
        public Validator(StatelessServiceContext context)
            : base(context)
        { }

        // Stateless Listener
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new List<ServiceInstanceListener>(1)
            {
                new ServiceInstanceListener(context => this.CreateWcfCommunication(context),"ServiceEndpoint")
            };
        }


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

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {

            long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
