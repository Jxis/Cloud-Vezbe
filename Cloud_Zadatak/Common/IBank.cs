using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [ServiceContract]
    public interface IBank : ITransaction
    {
        [OperationContract]
        Task ListClients();

        [OperationContract]
        Task EnlistMoneyTransfer(string userID, double amount);
    }
}
