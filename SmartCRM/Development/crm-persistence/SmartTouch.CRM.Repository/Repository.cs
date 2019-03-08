using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace SmartTouch.CRM.Repository
{
    public abstract class Repository<DomainType, IdType, DatabaseType>
        : IUnitOfWorkRepository
        where DomainType : IAggregateRoot
        where DatabaseType : class
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IObjectContextFactory objectContextFactory;

        public Repository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
        {
            if (unitOfWork == null)
                throw new ArgumentNullException("unitOfWork");
            if (objectContextFactory == null)
                throw new ArgumentNullException("objectContextFactory");

            this.unitOfWork = unitOfWork;
            this.objectContextFactory = objectContextFactory;
        }

        public IObjectContextFactory ObjectContextFactory
        {
            get { return objectContextFactory; }
        }

        public void Update(DomainType aggregate)
        {
            this.unitOfWork.RegisterUpdate(aggregate, this);
        }

        public void Insert(DomainType aggregate)
        {
            this.unitOfWork.RegisterInsertion(aggregate, this);
        }

        public void Delete(DomainType aggregate)
        {
            this.unitOfWork.RegisterDeletion(aggregate, this);
        }

        public abstract DomainType FindBy(IdType id);

        public abstract DatabaseType ConvertToDatabaseType(DomainType domainType, CRMDb context);

        public abstract DomainType ConvertToDomain(DatabaseType databaseType);

        public abstract void PersistValueObjects(DomainType domainType, DatabaseType dbType, CRMDb context);

        public IAggregateRoot PersistInsertion(IAggregateRoot aggregateRoot)
        {
            var result = default(DomainType);
            try
            {
                var db = this.objectContextFactory.Create();
                var databaseType = RetrieveDatabaseTypeFrom(aggregateRoot, db);
                var dbSet = db.Set(typeof(DatabaseType));
                var newObject = dbSet.Add(databaseType);
                PersistValueObjects((DomainType)aggregateRoot, (DatabaseType)newObject, db);
                db.SaveChanges();
                result = ConvertToDomain((DatabaseType)newObject);
            }
            catch (Exception ex)
            {
                var rethrowException = ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_AND_RETHROW_POLICY);
                if (rethrowException) throw;
            }
            return result;
        }

        int getNewlyGeneratedId(DatabaseType dbType, CRMDb db)
        {
            ObjectContext objectContext = ((IObjectContextAdapter)db).ObjectContext;
            ObjectSet<DatabaseType> set = objectContext.CreateObjectSet<DatabaseType>();
            IEnumerable<string> keyNames = set.EntitySet.ElementType
                                                        .KeyMembers
                                                        .Select(k => k.Name);
            if (keyNames != null && keyNames.Any())
            {
                string idValue = dbType.GetType().GetProperty(keyNames.FirstOrDefault()).GetValue(dbType, null).ToString();
                int id = 0;
                if (!int.TryParse(idValue, out id))
                    throw new InvalidOperationException("More than once key configured to the entity: " + dbType.GetType().ToString());
                else
                    return id;
            }
            else
                throw new InvalidOperationException("More than once key configured to the entity: " + dbType.GetType().ToString());
        }

        public IAggregateRoot PersistUpdate(IAggregateRoot aggregateRoot)
        {
            var result = default(DomainType);

            var db = this.objectContextFactory.Create();
            DatabaseType databaseType = RetrieveDatabaseTypeFrom(aggregateRoot, db);
            var dbSet = db.Set(typeof(DatabaseType));

            var newObject = dbSet.Attach(databaseType);
            var entry = db.Entry(databaseType);
            entry.State = EntityState.Modified;

            PersistValueObjects((DomainType)aggregateRoot, databaseType, db);
            db.SaveChanges();
            result = ConvertToDomain((DatabaseType)newObject);

            return result;
        }

        public void PersistDeletion(IAggregateRoot aggregateRoot)
        {
            try
            {
                var db = this.objectContextFactory.Create();
                DatabaseType databaseType = RetrieveDatabaseTypeFrom(aggregateRoot, db);
                db.Set(typeof(DatabaseType)).Remove(databaseType);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                var rethrowException = ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_AND_RETHROW_POLICY);
                if (rethrowException) throw;
            }
        }

        DatabaseType RetrieveDatabaseTypeFrom(IAggregateRoot aggregateRoot, dynamic context)
        {
            var result = default(DatabaseType);
            try
            {
                DomainType domainType = (DomainType)aggregateRoot;
                result = ConvertToDatabaseType(domainType, context);
            }
            catch (Exception ex)
            {
                var rethrowException = ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_AND_RETHROW_POLICY);
                if (rethrowException) throw;
            }
            return result;
        }
    }
}
