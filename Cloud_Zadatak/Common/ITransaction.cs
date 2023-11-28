using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [ServiceContract]
    public interface ITransaction
    {
        [OperationContract]
        Task<bool> Prepare();

        [OperationContract]
        Task Commit();

        [OperationContract]
        Task Rollback();
    }
}
