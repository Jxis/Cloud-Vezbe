using Common;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Models;

namespace Bookstore
{
    public class BookService : IBookStore
    {
        IReliableStateManager StateManager { get; set; }
        string Book { get; set; }
        int Count { get; set; }
        public BookService(IReliableStateManager StateManager)
        {
            this.StateManager = StateManager;
        }

        public async Task<Book> ListAvailableItems()
        {
            try
            {
                var booksDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, Book>>("books");

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var book = await booksDictionary.TryGetValueAsync(tx, "1");


                    await tx.CommitAsync();
                    return book.Value;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<bool> Prepare()
        {

            this.Book = "1";
            this.Count = 1;
            Book book = await ListAvailableItems();

            if (book != null && book.Count > this.Count)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public Task Commit()
        {
            throw new NotImplementedException();
        }

        public Task Rollback()
        {
            throw new NotImplementedException();
        }
    }
}
