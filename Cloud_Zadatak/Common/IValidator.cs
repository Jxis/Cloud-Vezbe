using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [ServiceContract]
    public interface IValidator
    {
        [OperationContract]
        Task<bool> Check(string user, string book, int count);
    }
}
