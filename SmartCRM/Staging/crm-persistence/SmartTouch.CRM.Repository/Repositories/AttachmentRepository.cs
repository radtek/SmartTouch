using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AutoMapper;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class AttachmentRepository : Repository<Attachment, int, AttachmentsDb>, IAttachmentRepository
    {
        public AttachmentRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory)
        { }

        public IEnumerable<Attachment> FindAll()
        {
            var db = ObjectContextFactory.Create();
            var varAttachements = db.Attachment.ToList();
            foreach (AttachmentsDb dc in varAttachements)
            {
                yield return Mapper.Map<AttachmentsDb, Attachment>(dc);
            }
        }

        public IEnumerable<Attachment> FindAll(int contactId, long attachmentId)
        {
            var db = ObjectContextFactory.Create();
            var varAttachments = db.Attachment.Include(c => c.Users).Where(Id => Id.DocumentID == attachmentId && Id.ContactID == contactId).ToList();
            foreach (AttachmentsDb dc in varAttachments)
            {
                yield return Mapper.Map<AttachmentsDb, Attachment>(dc);
            }
        }

        public override Attachment FindBy(int id)
        {
            try
            {
                Logger.Current.Informational("Request to fetching document by using documentid:" + id);
                var db = ObjectContextFactory.Create();
                AttachmentsDb attachmentDatabase = db.Attachment.Include(x => x.Users).SingleOrDefault(c => c.DocumentID == id);
                if (attachmentDatabase != null) return ConvertToDomain(attachmentDatabase);
                return null;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occurred while fetching document", ex);
                return null;
            }
        }

        public IEnumerable<Attachment> FindAllAttachments(int? contactId, int? opportunityID, string page, int limit, int pageNumber)
        {
            var db = ObjectContextFactory.Create();
            var records = (pageNumber - 1) * limit;
            IEnumerable<AttachmentsDb> Attachments;
            if (page == "contacts")
                Attachments = db.Attachment.Include(c => c.Users)
                                .Where(Id => Id.ContactID == contactId)
                                .OrderByDescending(c => c.CreatedDate)
                                .Skip(records).Take(limit);

            else
                Attachments = db.Attachment.Where(Id => Id.OpportunityID == opportunityID)
                                .OrderByDescending(c => c.CreatedDate)
                                .Skip(records).Take(limit);

            foreach (AttachmentsDb dc in Attachments)
                yield return Mapper.Map<AttachmentsDb, Attachment>(dc);
        }



        public IEnumerable<Attachment> FindAttachement(int contactId, string originalFilename, string storageFilename)
        {
            var db = ObjectContextFactory.Create();
            var varAttachments = db.Attachment.Where(Id => Id.ContactID == contactId && Id.OriginalFileName == originalFilename && Id.StorageFileName == storageFilename).ToList();
            foreach (AttachmentsDb dc in varAttachments)
            {
                yield return Mapper.Map<AttachmentsDb, Attachment>(dc);
            }
        }

        public int TotalNumberOfAttachments(int? contactId, int? opportunityID, string page)
        {
            int attachementCount = 0;
            Logger.Current.Verbose("Request to fetch total attachment count for related contact.");
            try
            {
                var db = ObjectContextFactory.Create();
                if (page == "contacts")
                    attachementCount = db.Attachment.Where(Id => Id.ContactID == contactId).Count();
                else
                    attachementCount = db.Attachment.Where(Id => Id.OpportunityID == opportunityID).Count();
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occurred while getting documents", ex);
            }
            return attachementCount;
        }

        public void DeleteAttachment(long attachementId)
        {
            try
            {
                Logger.Current.Informational("Request to delete for specific document by using documentid:" + attachementId);
                var db = ObjectContextFactory.Create();
                var varAttachments = db.Attachment.Where(Id => Id.DocumentID == attachementId).FirstOrDefault();
                db.Attachment.Remove(varAttachments);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occurred while deleting attachments", ex);
            }
        }

        public override Attachment ConvertToDomain(AttachmentsDb attachementDb)
        {
            Attachment attachement = new Attachment();
            Logger.Current.Verbose("Request to convet DbObject(AttachmentsDb) to DomainObjet(Attachment)");
            try
            {
                Mapper.Map<AttachmentsDb, Attachment>(attachementDb, attachement);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occurred while conveting DbObject(AttachmentsDb) to DomainObjet(Attachment)", ex);
            }
            return attachement;
        }

        public override AttachmentsDb ConvertToDatabaseType(Attachment domainType, CRMDb db)
        {
            Logger.Current.Verbose("Request to convet DomainObjet(Attachment) to DbObject(AttachmentsDb)");
            try
            {
                return Mapper.Map<Attachment, AttachmentsDb>(domainType as Attachment);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occurred while conveting DomainObjet(Attachment) to DbObject(AttachmentsDb)", ex);
                return null;
            }
        }

        public override void PersistValueObjects(Attachment domainType, AttachmentsDb docRepositorysDb, CRMDb db)
        {
            //future implementation
        }
    }
}
