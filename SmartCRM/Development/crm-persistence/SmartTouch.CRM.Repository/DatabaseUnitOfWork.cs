using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using System.Collections.Generic;
using System.Transactions;

namespace SmartTouch.CRM.Repository
{
    public class DatabaseUnitOfWork : IUnitOfWork
    {
        private Dictionary<IAggregateRoot, IUnitOfWorkRepository> insertedAggregates;
        private Dictionary<IAggregateRoot, IUnitOfWorkRepository> updatedAggregates;
        private Dictionary<IAggregateRoot, IUnitOfWorkRepository> deletedAggregates;

        public DatabaseUnitOfWork()
        {
            insertedAggregates = new Dictionary<IAggregateRoot,IUnitOfWorkRepository>();
            updatedAggregates = new Dictionary<IAggregateRoot,IUnitOfWorkRepository>();
            deletedAggregates = new Dictionary<IAggregateRoot,IUnitOfWorkRepository>();
        }

        public void RegisterUpdate(Infrastructure.Domain.IAggregateRoot aggregateRoot, IUnitOfWorkRepository repository)
        {
            if(!updatedAggregates.ContainsKey(aggregateRoot))
                updatedAggregates.Add(aggregateRoot, repository);
        }

        public void RegisterInsertion(Infrastructure.Domain.IAggregateRoot aggregateRoot, IUnitOfWorkRepository repository)
        {
            if(!insertedAggregates.ContainsKey(aggregateRoot))
                insertedAggregates.Add(aggregateRoot, repository);
        }

        public void RegisterDeletion(Infrastructure.Domain.IAggregateRoot aggregateRoot, IUnitOfWorkRepository repository)
        {
            if(!deletedAggregates.ContainsKey(aggregateRoot))
                deletedAggregates.Add(aggregateRoot, repository);
        }

        /// <summary>
        /// Persists all the actions performed on Aggregate root. Returns the persisted Aggregate Root object.
        /// </summary>
        /// <returns></returns>
        public IAggregateRoot Commit()
        {
            try
            {
                IAggregateRoot persistedAggregateRoot = null;
                using (TransactionScope scope = new TransactionScope())
                {
                    foreach (IAggregateRoot aggregateRoot in insertedAggregates.Keys)
                    {
                        persistedAggregateRoot = insertedAggregates[aggregateRoot].PersistInsertion(aggregateRoot);
                    }

                    foreach (IAggregateRoot aggregateRoot in updatedAggregates.Keys)
                    {
                        persistedAggregateRoot = updatedAggregates[aggregateRoot].PersistUpdate(aggregateRoot);
                    }

                    foreach (IAggregateRoot aggregateRoot in deletedAggregates.Keys)
                    {
                        deletedAggregates[aggregateRoot].PersistDeletion(aggregateRoot);
                    }
                    scope.Complete();
                }
                return persistedAggregateRoot;
            }
            finally
            {
                insertedAggregates.Clear();
                updatedAggregates.Clear();
                deletedAggregates.Clear();
            }
        }
    }
}
