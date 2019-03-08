using AutoMapper;
using SmartTouch.CRM.Domain.Forms;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class FormSubmissionRepository : Repository<FormSubmission, int, FormSubmissionDb>, IFormSubmissionRepository
    {
        public FormSubmissionRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory) { }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override FormSubmission FindBy(int id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Convert FormSubmission from domain to database type
        /// </summary>
        /// <param name="domainType"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override FormSubmissionDb ConvertToDatabaseType(FormSubmission domainType, CRMDb context)
        {
            FormSubmissionDb formSubmissionDb;
            if (domainType.Id > 0)
            {
                formSubmissionDb = context.FormSubmissions.SingleOrDefault(c => c.FormID == domainType.Id);

                if (formSubmissionDb == null)
                    throw new ArgumentException("Invalid form id has been passed. Suspected Id forgery.");

                formSubmissionDb = Mapper.Map<FormSubmission, FormSubmissionDb>(domainType, formSubmissionDb);
            }
            else
            {
                formSubmissionDb = Mapper.Map<FormSubmission, FormSubmissionDb>(domainType);
            }
            return formSubmissionDb;
        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="databaseType">Type of the database.</param>
        /// <returns></returns>
        public override FormSubmission ConvertToDomain(FormSubmissionDb databaseType)
        {
            FormSubmission submission = new FormSubmission();
            if (databaseType != null)
                return Mapper.Map<FormSubmissionDb, FormSubmission>(databaseType);
            else
                return submission;
        }

        /// <summary>
        /// Persists the value objects.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="context">The context.</param>
        public override void PersistValueObjects(FormSubmission domainType, FormSubmissionDb dbType, CRMDb context)
        {
            //for future use
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FormSubmission> FindAll()
        {
            var db = ObjectContextFactory.Create();

            //var sub = db.FormSubmissions.Where(w => w.LeadSourceID != null && w.StatusID != null);
            //var subnull = sub.Where(w => w.LeadSourceID == null);

            var formSubmissions = db.FormSubmissions.Where(w => w.LeadSourceID != null && w.StatusID != default(short)).Select(s => new
            {
                FormSubmissionId = s.FormSubmissionID,
                FormId = s.FormID,
                ContactId = s.ContactID,
                StatusId = s.StatusID,
                SubmittedOn = s.SubmittedOn,
                LeadSourceId = s.LeadSourceID
            }).Select(s => new FormSubmission
            {
                LeadSourceID = s.LeadSourceId == null ? default(short) : s.LeadSourceId,
                ContactId = s.ContactId,
                FormId = s.FormId,
                Id = s.FormSubmissionId,
                StatusID = (FormSubmissionStatus)s.StatusId,
                SubmittedOn = s.SubmittedOn
            });
            
            return formSubmissions;
        }

        /// <summary>
        /// Gets the form submission.
        /// </summary>
        /// <param name="formSubmissionId">The form submission identifier.</param>
        /// <returns></returns>
        public FormData GetFormSubmission(int formSubmissionId)
        {
            FormData formSubmission = new FormData();
            if (formSubmissionId != 0)
            {
                var db = ObjectContextFactory.Create();
                var formSubmissionDb = db.FormSubmissions.Where(fs => fs.FormSubmissionID == formSubmissionId).Join(db.Forms, f => f.FormID, fs => fs.FormID, (fs, f) => new
                {
                    formName = f.Name,
                    formData = fs.SubmittedData
                }).Select(s => new FormData() { FormName = s.formName , SubmittedData = s.formData}).FirstOrDefault();
                formSubmission = formSubmissionDb;
            }
            return formSubmission;
        }

        /// <summary>
        /// Getting The Form Name
        /// </summary>
        /// <param name="formSubmissionId"></param>
        /// <returns></returns>
        public string GetFormName(int formSubmissionId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"select f.Name from FormSubmissions fs (nolock) 
                        inner join Forms f (nolock) on f.FormID = fs.FormID
                        where fs.FormSubmissionID=@formSubmissionID";
            string formName = db.Get<string>(sql, new { formSubmissionID = formSubmissionId }).FirstOrDefault();
            return formName;
        }
    }
}
