using AutoMapper;
using SmartTouch.CRM.Domain.SeedList;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class SeedListRepository : Repository<SeedEmail, int, SeedEmailDb>, ISeedListRepository
    {
        public SeedListRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory)
        {

        }

        /// <summary>
        /// Gets the seed list.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SeedEmail> GetSeedList()
        {
            var db = ObjectContextFactory.Create();
            var seedEmailDb = db.SeedEmail.ToList();
            return Mapper.Map<IEnumerable<SeedEmailDb>, IEnumerable<SeedEmail>>(seedEmailDb);
        }

        /// <summary>
        /// Saves the seed list.
        /// </summary>
        /// <param name="seedEmails">The seed emails.</param>
        /// <param name="userId">The user identifier.</param>
        public void SaveSeedList(IEnumerable<SeedEmail> seedEmails, int userId)
        {

            List<SeedEmailDb> seedEmailsDb = new List<SeedEmailDb>();
            if (seedEmails.Any())
            {
                foreach (var seedEmail in seedEmails)
                {
                    SeedEmailDb seedEmailDb = new SeedEmailDb();
                    seedEmailDb.CreatedBy = userId;
                    seedEmailDb.CreatedDate = DateTime.Now.ToUniversalTime();
                    seedEmailDb.Email = seedEmail.Email;
                    seedEmailsDb.Add(seedEmailDb);
                }
            }
            var db = ObjectContextFactory.Create();

            var seedList = (from s in db.SeedEmail select s).ToList();
            db.SeedEmail.RemoveRange(seedList);

            db.SeedEmail.AddRange(seedEmailsDb);
            db.SaveChanges();
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override SeedEmail FindBy(int id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the type of to database.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override SeedEmailDb ConvertToDatabaseType(SeedEmail domainType, CRMDb context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="databaseType">Type of the database.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override SeedEmail ConvertToDomain(SeedEmailDb databaseType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Persists the value objects.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="context">The context.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void PersistValueObjects(SeedEmail domainType, SeedEmailDb dbType, CRMDb context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Saves the seed list.
        /// </summary>
        /// <param name="seedEmail">The seed email.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void SaveSeedList(IEnumerable<SeedEmail> seedEmail)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<SeedEmail> FindAll()
        {
            throw new NotImplementedException();
        }
    }
}
