using Common;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank
{
    public class BankService : IBank
    {
        IReliableStateManager ReliableStateManager { get; set; }
        string Bank { get; set; }
        int Year { get; set; }
        public BankService(IReliableStateManager reliableStateManager) 
        {
            ReliableStateManager = reliableStateManager;
        }

        public async Task<Common.Models.Bank> GetBankAsync()
        {
            try
            {
                var bankDictionary = await this.ReliableStateManager.GetOrAddAsync<IReliableDictionary<string, Common.Models.Bank>>("banks");
                using (var transaction = this.ReliableStateManager.CreateTransaction())
                {
                    var bank = await bankDictionary.TryGetValueAsync(transaction, "1");


                    await transaction.CommitAsync();
                    return bank.Value;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Task Commit()
        {
            throw new NotImplementedException();
        }

        public Task EnlistMoneyTransfer(string userID, double amount)
        {
            throw new NotImplementedException();
        }

        public Task ListClients()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Prepare()
        {
            Bank = "1";
            Year = 50;
            Common.Models.Bank bank = await GetBankAsync();

            if (bank != null && bank.Year > Year)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public Task Rollback()
        {
            throw new NotImplementedException();
        }
    }
}
