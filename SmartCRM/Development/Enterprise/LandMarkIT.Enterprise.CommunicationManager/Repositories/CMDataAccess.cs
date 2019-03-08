using LandmarkIT.Enterprise.CommunicationManager.DatabaseEntities;
using LandmarkIT.Enterprise.CommunicationManager.Responses;
using System;
using System.Data.Entity;
using System.Linq;

namespace LandmarkIT.Enterprise.CommunicationManager.Repository
{
    internal class CMDataAccess
    {
        #region Registraions
        public Guid RegisterMail(MailRegistration item)
        {
            var result = default(Guid);
            using (var db = new RegistrationContext())
            {
                item.Guid = (item.Guid == default(Guid)) ? Guid.NewGuid() : item.Guid;

                var mailRegistration = db.MailRegistrations.Where(mr => mr.Guid == item.Guid).SingleOrDefault();

                if (mailRegistration == null)
                {
                    item.MailRegistrationId = default(int);
                    db.Entry<MailRegistration>(item).State = EntityState.Added;
                }
                else
                {
                    item.MailRegistrationId = mailRegistration.MailRegistrationId;
                    db.Entry<MailRegistration>(item).State = EntityState.Modified;
                }

                db.SaveChanges();
                result = item.Guid;
            }
            return result;
        }
        public Guid RegisterText(TextRegistration item)
        {
            var result = default(Guid);
            using (var db = new RegistrationContext())
            {
                item.Guid = (item.Guid == default(Guid)) ? Guid.NewGuid() : item.Guid;

                var textRegistration = db.TextRegistrations.Where(mr => mr.Guid == item.Guid).SingleOrDefault();

                if (textRegistration == null)
                {
                    item.TextRegistrationId = default(int);
                    db.Entry<TextRegistration>(item).State = EntityState.Added;
                }
                else
                {
                    item.TextRegistrationId = textRegistration.TextRegistrationId;
                    db.Entry<TextRegistration>(item).State = EntityState.Modified;
                }

                db.SaveChanges();
                result = item.Guid;
            }
            return result;
        }
        public Guid RegisterSocial(SocialRegistration item)
        {
            var result = default(Guid);
            using (var db = new RegistrationContext())
            {
                item.Guid = (item.Guid == default(Guid)) ? Guid.NewGuid() : item.Guid;

                var socialRegistration = db.SocialRegistrations.Where(mr => mr.Guid == item.Guid).SingleOrDefault();

                if (socialRegistration == null)
                {
                    item.SocialRegistrationId = default(int);
                    db.Entry<SocialRegistration>(item).State = EntityState.Added;
                }
                else
                {
                    item.SocialRegistrationId = socialRegistration.SocialRegistrationId;
                    db.Entry<SocialRegistration>(item).State = EntityState.Modified;
                }

                db.SaveChanges();
                result = socialRegistration.Guid;
            }
            return result;
        }
        public Guid RegisterStorage(StorageRegistration item)
        {
            var result = default(Guid);
            using (var db = new RegistrationContext())
            {
                item.Guid = (item.Guid == default(Guid)) ? Guid.NewGuid() : item.Guid;

                var storageRegistration = db.StorageRegistrations.Where(mr => mr.Guid == item.Guid).SingleOrDefault();

                if (storageRegistration == null)
                {
                    item.StorageRegistrationId = default(int);
                    db.Entry<StorageRegistration>(item).State = EntityState.Added;
                }
                else
                {
                    item.StorageRegistrationId = storageRegistration.StorageRegistrationId;
                    db.Entry<StorageRegistration>(item).State = EntityState.Modified;
                }

                db.SaveChanges();
                result = item.Guid;
            }
            return result;
        } 
        #endregion

        #region Get
        public MailRegistration GetMailRegistration(Guid guid)
        {
            using (var db = new RegistrationContext())
            {
                return db.MailRegistrations.Where(mr => mr.Guid == guid).SingleOrDefault();
            }
        }
        public SocialRegistration GetSocialRegistration(Guid guid)
        {
            using (var db = new RegistrationContext())
            {
                return db.SocialRegistrations.Where(mr => mr.Guid == guid).SingleOrDefault();
            }
        }
        public TextRegistration GetTextRegistration(Guid guid)
        {
            using (var db = new RegistrationContext())
            {
                return db.TextRegistrations.Where(mr => mr.Guid == guid).SingleOrDefault();
            }
        }
        public StorageRegistration GetStorageRegistration(Guid guid)
        {
            using (var db = new RegistrationContext())
            {
                return db.StorageRegistrations.Where(mr => mr.Guid == guid).SingleOrDefault();
            }
        }
        #endregion

        #region Registraions
        public Guid TextResponse(TextResponse item)
        {
            var result = default(Guid);
            using (var db = new RegistrationContext())
            {

                    item.TextResponseID = default(int);
                    db.Entry<TextResponse>(item).State = EntityState.Added;
                    db.SaveChanges();
                    result = item.RequestGuid;
            }
            return result;
        }
        public Guid MailResponse(MailResponse item)
        {
            var result = default(Guid);
            using (var db = new RegistrationContext())
            {
                    item.MailResponseID = default(int);
                    db.Entry<MailResponse>(item).State = EntityState.Added;
                    db.SaveChanges();
                    result = item.RequestGuid;
            }
            return result;
        } 
        #endregion
    }
}
