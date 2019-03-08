using AutoMapper;
using SmartTouch.CRM.Domain.ImageDomains;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class ImageDomainRepository : Repository<ImageDomain, byte, ImageDomainsDb>, IImageDomainRepository
    {
        public ImageDomainRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory)
        {

        }

        public IEnumerable<ImageDomain> ImageDomainsList()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ImageDomain> FindAll()
        {
            var db = ObjectContextFactory.Create();
            var imageDomainsDb = db.ImageDomains.ToList();
            return Mapper.Map<IEnumerable<ImageDomainsDb>, IEnumerable<ImageDomain>>(imageDomainsDb);
        }

        /// <summary>
        /// Gets the active image domains.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ImageDomain> GetActiveImageDomains()
        {
            var db = ObjectContextFactory.Create();
            var imageDomainsDb = db.ImageDomains.ToList();
            return Mapper.Map<IEnumerable<ImageDomainsDb>, IEnumerable<ImageDomain>>(imageDomainsDb);
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="status">if set to <c>true</c> [status].</param>
        /// <returns></returns>
        public IEnumerable<ImageDomain> FindAll(string name, int limit, int pageNumber, bool status)
        {
            var records = (pageNumber - 1) * limit;
            IEnumerable<ImageDomainsDb> imageDomains = findImageDomains(records, limit);
            foreach (ImageDomainsDb da in imageDomains)
            {
                yield return ConvertToDomain(da);
            }
        }

        /// <summary>
        /// Finds the image domains.
        /// </summary>
        /// <param name="records">The records.</param>
        /// <param name="limit">The limit.</param>
        /// <returns></returns>
        IEnumerable<ImageDomainsDb> findImageDomains(int records, int limit)
        {
            IEnumerable<ImageDomainsDb> imageDomains = ObjectContextFactory.Create().ImageDomains.OrderByDescending(i=>i.LastModifiedOn).Skip(records).Take(limit);                                   
                
            return imageDomains;
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override ImageDomain FindBy(byte id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the type of to database.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Invalid image domain id has been passed. Suspected Id forgery.</exception>
        public override ImageDomainsDb ConvertToDatabaseType(ImageDomain domainType, CRMDb context)
        {
            ImageDomainsDb imageDomainsDb;
            if (domainType.Id > 0)
            {
                imageDomainsDb = context.ImageDomains.SingleOrDefault(c => c.ImageDomainID == domainType.Id);

                if (imageDomainsDb == null)
                    throw new ArgumentException("Invalid image domain id has been passed. Suspected Id forgery.");

                imageDomainsDb = Mapper.Map<ImageDomain, ImageDomainsDb>(domainType, imageDomainsDb);
            }
            else
            {
                imageDomainsDb = Mapper.Map<ImageDomain, ImageDomainsDb>(domainType);
            }
            return imageDomainsDb;
        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="databaseType">Type of the database.</param>
        /// <returns></returns>
        public override ImageDomain ConvertToDomain(ImageDomainsDb databaseType)
        {
            return Mapper.Map<ImageDomainsDb,ImageDomain>(databaseType);
        }

        /// <summary>
        /// Persists the value objects.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="context">The context.</param>
        public override void PersistValueObjects(ImageDomain domainType, ImageDomainsDb dbType, CRMDb context)
        {
            //for future use            
        }

        /// <summary>
        /// Determines whether [is duplicate image domain] [the specified domain].
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <returns></returns>
        public bool IsDuplicateImageDomain(ImageDomain domain)
        {
            var db= ObjectContextFactory.Create();
            var domainDb = db.ImageDomains.Where(c=>c.ImageDomain==domain.Domain).FirstOrDefault();
            var isDuplicate = false;
            if (domain.Id > 0)
                isDuplicate = domainDb != null && domainDb.ImageDomainID != domain.Id;
            else
                isDuplicate = domainDb != null;

            return isDuplicate;
        }

        /// <summary>
        /// Gets the image domain.
        /// </summary>
        /// <param name="imageDomainId">The image domain identifier.</param>
        /// <returns></returns>
        public ImageDomain GetImageDomain(byte imageDomainId)
        {
            var db = ObjectContextFactory.Create();
            var imageDomainDb = db.ImageDomains.Where(i => i.ImageDomainID==imageDomainId).FirstOrDefault();
            ImageDomain imageDomain = Mapper.Map<ImageDomainsDb, ImageDomain>(imageDomainDb);
            return imageDomain;
        }

        /// <summary>
        /// Updates the image domain.
        /// </summary>
        /// <param name="imageDomain">The image domain.</param>
        /// <returns></returns>
        public ImageDomain UpdateImageDomain(ImageDomain imageDomain)
        {
            var db = ObjectContextFactory.Create();
            var imageDomainDb = db.ImageDomains.Where(i => i.ImageDomainID == imageDomain.Id).FirstOrDefault();
            imageDomainDb = Mapper.Map<ImageDomain, ImageDomainsDb>(imageDomain);
            db.Entry<ImageDomainsDb>(imageDomainDb).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return imageDomain;
        }

        /// <summary>
        /// Deletes the image domain.
        /// </summary>
        /// <param name="imageDomainId">The image domain identifier.</param>
        public void DeleteImageDomain(byte imageDomainId)
        {
            var db = ObjectContextFactory.Create();
            var imageDomainDb = db.ImageDomains.Where(i => i.ImageDomainID == imageDomainId).FirstOrDefault();
            db.Entry<ImageDomainsDb>(imageDomainDb).State = System.Data.Entity.EntityState.Deleted;
            db.SaveChanges();
        }

        /// <summary>
        /// Gets the image domains count.
        /// </summary>
        /// <returns></returns>
        public byte GetImageDomainsCount()
        {
            var db = ObjectContextFactory.Create();
            byte imageDomainsCount = (byte)db.ImageDomains.Count();
            return imageDomainsCount;
        }

        /// <summary>
        /// Determines whether [is configured with vmta] [the specified image domain identifier].
        /// </summary>
        /// <param name="imageDomainID">The image domain identifier.</param>
        /// <returns></returns>
        public bool IsConfiguredWithVMTA(byte imageDomainID)
        {
            var db = ObjectContextFactory.Create();
            bool isconfiguredwithvmta = db.ServiceProviders.Where(i => i.ImageDomainID == imageDomainID).Count() > 0;
            return isconfiguredwithvmta;
        }
    }
}
