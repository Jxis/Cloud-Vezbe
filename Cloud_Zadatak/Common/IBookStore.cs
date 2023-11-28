using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [ServiceContract]
    public interface IBookStore : ITransaction
    {
        [OperationContract]
        Task<Book> ListAvailableItems();

        //[OperationContract]
        //Task EnlistPurchase(string bookID, uint count);

        //[OperationContract]
        //Task<double> GetItemPrice(string bookID);
    }
}
