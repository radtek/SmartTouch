using AutoMapper;
using LandmarkIT.Enterprise.Utilities.Logging;
using LinqKit;
using SmartTouch.CRM.Domain.Actions;
using SmartTouch.CRM.Domain.Dropdowns;
using SmartTouch.CRM.Domain.Fields;
using SmartTouch.CRM.Domain.Forms;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class FormRepository : Repository<Form, int, FormsDb>, IFormRepository
    {

        public FormRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory) { }

        /// <summary>
        /// Gets the form by identifier.
        /// </summary>
        /// <param name="formId">The form identifier.</param>
        /// <returns></returns>
        public Form GetFormById(int formId)
        {
            var db = ObjectContextFactory.Create();
            FormsDb form = new FormsDb();
            IEnumerable<FieldsDb> fields = new List<FieldsDb>();
            db.QueryStoredProc("[dbo].[GetForm]", (reader) =>
            {
                form = reader.Read<FormsDb>().FirstOrDefault();
                if (form != null)
                {
                    form.FormTags = reader.Read<FormTagsDb>().ToList();

                    form.LeadSource = reader.Read<DropdownValueDb>().FirstOrDefault();
                    form.FormFields = reader.Read<FormFieldsDb, FieldsDb, FormFieldsDb>((ff, f) =>
                    {
                        ff.Field = f;
                        return ff;
                    }, splitOn: "FieldInputTypeID").ToList();
                    if (form.FormTags.Any())
                    {
                        var tags = reader.Read<TagsDb>().ToList();
                        foreach (var tag in form.FormTags)
                        {
                            tag.Tag = tags.Where(t => t.TagID == tag.TagID).FirstOrDefault();
                        }
                    }
                }
            }, new { FormID = formId });
            return Mapper.Map<FormsDb, Form>(form);
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override Form FindBy(int id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Convert FORM from domain type to database type
        /// </summary>
        /// <param name="domainType"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override FormsDb ConvertToDatabaseType(Form domainType, CRMDb context)
        {
            FormsDb formsDb;
            if (domainType.Id > 0)
            {
                formsDb = context.Forms.Include(f => f.FormFields).SingleOrDefault(c => c.FormID == domainType.Id);

                if (formsDb == null)
                    throw new ArgumentException("Invalid form id has been passed. Suspected Id forgery.");

                formsDb = Mapper.Map<Form, FormsDb>(domainType, formsDb);
                //context.Entry<DropdownValueDb>(formsDb.LeadSource).State = EntityState.Detached;
            }
            else
            {
                formsDb = Mapper.Map<Form, FormsDb>(domainType);
                //context.Entry<DropdownValueDb>(formsDb.LeadSource).State = EntityState.Unchanged;
            }
            return formsDb;
        }

        public int InsertSubmittedFormData(SmartTouch.CRM.Domain.Forms.SubmittedFormData submittedData, IEnumerable<SmartTouch.CRM.Domain.Forms.SubmittedFormFieldData> submittedFormFieldData)
        {
            var db = ObjectContextFactory.Create();

            SmartTouch.CRM.Repository.Database.SubmittedFormDataDb submitteddata = (Mapper.Map<SmartTouch.CRM.Domain.Forms.SubmittedFormData, SmartTouch.CRM.Repository.Database.SubmittedFormDataDb>(submittedData));
            IEnumerable<SmartTouch.CRM.Repository.Database.SubmittedFormFieldDataDb> submittedformdata = (Mapper.Map<IEnumerable<SmartTouch.CRM.Domain.Forms.SubmittedFormFieldData>, IEnumerable<SmartTouch.CRM.Repository.Database.SubmittedFormFieldDataDb>>(submittedFormFieldData));


            db.SubmittedFormData.Add(submitteddata);
            db.SaveChanges();
            foreach (SubmittedFormFieldDataDb fd in submittedformdata)
            {
                fd.SubmittedFormDataID = submitteddata.SubmittedFormDataID;
            }

            //  submittedformdata.ForEach(p => p.SubmittedFormDataID = submittedData.SubmittedFormDataID);

            db.SubmittedFormFieldData.AddRange(submittedformdata);
            db.SaveChanges();



            return submitteddata.SubmittedFormDataID;
        }

        /// <summary>
        /// Deactivate FORM by making its property 'IsDeleted' to TRUE
        /// </summary>
        /// <param name="formIds"></param>
        public void DeactivateForm(int[] formIds)
        {
            var db = ObjectContextFactory.Create();
            var formsDb = db.Forms.Where(f => formIds.Contains(f.FormID));
            formsDb.ForEach(f => { f.IsDeleted = true; });
            db.SaveChanges();
        }

        /// <summary>
        /// Convert FORM from database type to domain type
        /// </summary>
        /// <param name="formDb"></param>
        /// <returns></returns>
        public override Form ConvertToDomain(FormsDb formDb)
        {
            return Mapper.Map<FormsDb, Form>(formDb);
        }

        public SmartTouch.CRM.Domain.Forms.SubmittedFormData GetFormSubmittedData()
        {
            var db = ObjectContextFactory.Create();
            SmartTouch.CRM.Repository.Database.SubmittedFormDataDb data = db.SubmittedFormData.Where(p => p.Status == (int)SubmittedFormStatus.ReadyToProcess).FirstOrDefault();
            if (data != null)
                return Mapper.Map<SmartTouch.CRM.Repository.Database.SubmittedFormDataDb, SmartTouch.CRM.Domain.Forms.SubmittedFormData>(data);
            return null;
        }

        /// <summary>
        /// Get Submitted FormData By by formSubmissionID NEXG- 3014
        /// </summary>
        /// <param name="formSubmissionID"></param>
        /// <returns></returns>
        public SmartTouch.CRM.Domain.Forms.SubmittedFormData GetFormSubmittedData(int formSubmissionID)
        {
            var db = ObjectContextFactory.Create();
            SmartTouch.CRM.Repository.Database.SubmittedFormDataDb data = db.SubmittedFormData.Where(p => p.SubmittedFormDataID == formSubmissionID).FirstOrDefault();
            if (data != null)
                return Mapper.Map<SmartTouch.CRM.Repository.Database.SubmittedFormDataDb, SmartTouch.CRM.Domain.Forms.SubmittedFormData>(data);
            return null;
        }

        public IEnumerable<SmartTouch.CRM.Domain.Forms.SubmittedFormFieldData> GetFormSubmittedFieldData(int formDataId)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<SmartTouch.CRM.Repository.Database.SubmittedFormFieldDataDb> data = db.SubmittedFormFieldData.Where(p => p.SubmittedFormDataID == formDataId).ToArray();

            IEnumerable<SmartTouch.CRM.Domain.Forms.SubmittedFormFieldData> submittedFormFieldData
                = Mapper.Map<IEnumerable<SmartTouch.CRM.Repository.Database.SubmittedFormFieldDataDb>, IEnumerable<SmartTouch.CRM.Domain.Forms.SubmittedFormFieldData>>(data);

            return submittedFormFieldData;
        }

        public void UpdateFormSubmissionStatus(int submittedFormDataID, SubmittedFormStatus status, string spamRemarks, int? formSubmissionID)
        {
            var db = ObjectContextFactory.Create();
            var sd = db.SubmittedFormData.Where(p => p.SubmittedFormDataID == submittedFormDataID).FirstOrDefault();
            if (sd != null)
            {
                sd.Status = (int)status;
                sd.Remarks = spamRemarks;
                sd.FormSubmissionID = formSubmissionID;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Persists the value objects.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <param name="formDb">The form database.</param>
        /// <param name="db">The database.</param>
        public override void PersistValueObjects(Form form, FormsDb formDb, CRMDb db)
        {
            PersistFormTags(form, formDb, db);
            PersistFormFields(form, formDb, db);
        }

        /// <summary>
        /// Persists the form tags.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <param name="formDb">The form database.</param>
        /// <param name="db">The database.</param>
        private void PersistFormTags(Form form, FormsDb formDb, CRMDb db)
        {
            var formTags = db.FormTags.Where(a => a.FormID == form.Id).ToList();

            foreach (Tag tag in form.Tags)
            {
                var tagexist = db.Tags.Where(p => p.TagID == tag.Id && p.AccountID == tag.AccountID && p.IsDeleted != true).FirstOrDefault();
                if (tag.Id == 0 || tagexist == null)
                {
                    var tagDb = db.Tags.SingleOrDefault(t => t.TagName.Equals(tag.TagName) && t.AccountID.Equals(tag.AccountID) && t.IsDeleted != true);
                    if (tagDb == null)
                    {
                        tagDb = Mapper.Map<Tag, TagsDb>(tag);
                        tagDb.IsDeleted = false;
                        tagDb = db.Tags.Add(tagDb);
                    }
                    var formTag = new FormTagsDb()
                    {
                        Form = formDb,
                        Tag = tagDb
                    };

                    db.FormTags.Add(formTag);
                }
                else if (!formTags.Any(a => a.TagID == tag.Id))
                {
                    db.FormTags.Add(new FormTagsDb() { FormID = form.Id, TagID = tag.Id });
                    db.RefreshAnalytics.Add(new RefreshAnalyticsDb() { EntityID = tag.Id, EntityType = 5, Status = 1, LastModifiedOn = DateTime.Now.ToUniversalTime() });
                }
            }

            IList<int> tagIds = form.Tags.Where(a => a.Id > 0).Select(a => a.Id).ToList();
            var unMapActionTags = formTags.Where(a => !tagIds.Contains(a.TagID));
            foreach (FormTagsDb formTagMapDb in unMapActionTags)
            {
                db.FormTags.Remove(formTagMapDb);
                db.RefreshAnalytics.Add(new RefreshAnalyticsDb() { EntityID = formTagMapDb.TagID, EntityType = 5, Status = 1, LastModifiedOn = DateTime.Now.ToUniversalTime() });
            }
        }

        /// <summary>
        /// Persists the form fields.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <param name="formDb">The form database.</param>
        /// <param name="db">The database.</param>
        private void PersistFormFields(Form form, FormsDb formDb, CRMDb db)
        {
            if (form.Id > 0)
            {
                foreach (FormField formField in form.FormFields.Where(f => f.FormFieldId == 0))
                {
                    FormFieldsDb formFieldDb = Mapper.Map<FormField, FormFieldsDb>(formField);
                    db.FormFields.Add(formFieldDb);
                }
            }
            //else
            //{
            //    formDb.FormFields = db.FormFields.Where(f => f.FormID == formDb.FormID).ToList();

            //    foreach (FormField field in form.FormFields)
            //    {
            //        FormFieldsDb formFieldDb = formDb.FormFields.Where(f => f.FieldID == field.Id).FirstOrDefault();
            //        formFieldDb = Mapper.Map<FormField, FormFieldsDb>(field);
            //        formFieldDb.FormID = form.Id;
            //        if (formFieldDb.FormFieldID == 0)
            //            formDb.FormFields.Add(formFieldDb);
            //    }
            //}
            IList<int> formFieldIds = form.FormFields.Where(a => a.FormFieldId > 0).Select(a => a.FormFieldId).ToList();
            var unMapFormFields = db.FormFields.Where(a => !formFieldIds.Contains(a.FormFieldID) && a.FormID == form.Id);
            foreach (FormFieldsDb formFieldMapDb in unMapFormFields)
            {
                formFieldMapDb.IsDeleted = true;
                db.Entry(formFieldMapDb).State = EntityState.Deleted;
            }
        }

        /// <summary>
        /// Checks if the given FORM name is unique in the account
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public bool IsFormNameUnique(Form form)
        {
            var db = ObjectContextFactory.Create();
            var formFound = db.Forms.Where(c => c.Name == form.Name && c.AccountID == form.AccountID && c.IsDeleted == false).Select(c => c).FirstOrDefault();
            if (formFound != null && form.Id != formFound.FormID)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether [is form submission allowed] [the specified form identifier].
        /// </summary>
        /// <param name="formId">The form identifier.</param>
        /// <returns></returns>
        public bool isFormSubmissionAllowed(int formId)
        {
            var db = ObjectContextFactory.Create();
            var isFormActive = db.Forms.Where(f => f.FormID == formId).Select(f => f.Status).FirstOrDefault();
            return isFormActive == (byte)FormStatus.Active;
        }

        /// <summary>
        /// Get all possible contact fields
        /// </summary>
        /// <returns></returns>
        public IList<Field> GetAllContactFields()
        {
            /* 
              in the db and 23,26,25 are donotemail, leadscore and owner fields AND 27,28,29,41 are Created By,Created On,Last Touched,Last Touched Through*/
            int[] notrequiredFileds = new int[] { 
                (int)ContactFields.DonotEmail,
                (int)ContactFields.LeadScore,
                (int)ContactFields.CreatedBy,
                (int)ContactFields.CreatedOn,
                (int)ContactFields.LastTouchedThrough,
                (int)ContactFields.LastTouched,
                (int)ContactFields.Owner,
                (int)ContactFields.LifecycleStageField,
                (int)ContactFields.PartnerTypeField,
                (int)ContactFields.WebPage,
                (int)ContactFields.WebPageDuration,
                (int)ContactFields.ContactTag,
                (int)ContactFields.FormName,
                (int)ContactFields.FormsubmittedOn,
                (int)ContactFields.FirstSourceType,
                (int)ContactFields.LeadAdapter,
                (int)ContactFields.FirstLeadSource,
                (int)ContactFields.FirstLeadSourceDate,
                (int)ContactFields.LeadSourceDate,
                (int)ContactFields.LastNoteDate,
                (int)ContactFields.LastNote,
                (int)ContactFields.NoteSummary,
                (int)ContactFields.TourType,
                (int)ContactFields.TourDate,
                (int)ContactFields.TourCreator,
                (int)ContactFields.EmailStatus,
                (int)ContactFields.TourAssignedUsers,
                (int)ContactFields.ActionCreatedDate,
                (int)ContactFields.ActionType,
                (int)ContactFields.ActionDate,
                (int)ContactFields.ActionStatus,
                (int)ContactFields.ActionAssignedTo
            };

            var db = ObjectContextFactory.Create();
            IList<FieldsDb> contactFieldsDb = db.Fields.Where(c => c.AccountID == null
                                                                && c.StatusID == (short)FieldStatus.Active
                                                                && !notrequiredFileds.Contains(c.FieldID))
                                                .Include(c => c.CustomFieldValueOptions).ToList();
            IList<Field> contactFields = Mapper.Map<IList<FieldsDb>, IList<Field>>(contactFieldsDb);
            return contactFields;
        }

        /// <summary>
        /// Get the list of Value options of the field by passing FieldId
        /// </summary>
        /// <param name="fieldId"></param>
        /// <returns></returns>
        public IList<FieldValueOption> GetAllFieldValueOptions(int fieldId)
        {
            var db = ObjectContextFactory.Create();
            IList<CustomFieldValueOptionsDb> customFieldsValueOptionsDb = db.CustomFieldValueOptions.Where(c => c.CustomFieldID == fieldId).ToList();
            IList<FieldValueOption> fieldValueOptions = Mapper.Map<IList<CustomFieldValueOptionsDb>, IList<FieldValueOption>>(customFieldsValueOptionsDb);
            return fieldValueOptions;
        }

        /// <summary>
        /// Get a particular field option by passing its FieldValueOptionId.
        /// </summary>
        /// <param name="fieldValueOptionId"></param>
        /// <returns></returns>
        public FieldValueOption GetFieldValueOption(int fieldValueOptionId)
        {
            var db = ObjectContextFactory.Create();
            CustomFieldValueOptionsDb customFieldsValueOptionDb = db.CustomFieldValueOptions.Where(c => c.CustomFieldValueOptionID == fieldValueOptionId).FirstOrDefault();
            FieldValueOption fieldValueOption = new FieldValueOption();
            if (customFieldsValueOptionDb != null)
            {
                fieldValueOption = Mapper.Map<CustomFieldValueOptionsDb, FieldValueOption>(customFieldsValueOptionDb);
            }
            return fieldValueOption;
        }

        /// <summary>
        /// Find all FORMs
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Form> FindAll()
        {
            // var predicate = PredicateBuilder.True<FormsDb>();

            //IEnumerable<FormsDb> forms = ObjectContextFactory.Create().Forms.Where(f => f.IsDeleted == false);

            var db = ObjectContextFactory.Create();

            var forms = db.Forms.Where(f => f.IsDeleted == false).Select(a =>
                  new
                  {
                      AccountID = a.AccountID,
                      Acknowledgement = a.Acknowledgement,
                      AllSubmissions = db.FormSubmissions.Where(i => i.FormID == a.FormID && !i.Contact.IsDeleted).Select(i => i.ContactID).Count(),
                      AcknowledgementType = a.AcknowledgementType,
                      CreatedBy = a.CreatedBy,
                      FormID = a.FormID,
                      FormFields = a.FormFields,
                      HTMLContent = a.HTMLContent,
                      IsDeleted = a.IsDeleted,
                      LastModifiedBy = a.LastModifiedBy,
                      LastModifiedOn = a.LastModifiedOn,
                      Name = a.Name,
                      Status = a.Status,
                      CreatedOn = a.CreatedOn,
                      UniqueSubmissions = db.FormSubmissions.Where(i => i.FormID == a.FormID && !i.Contact.IsDeleted).Select(i => i.ContactID).Distinct().Count()
                  }).AsEnumerable().Select(a => new FormsDb
                  {
                      AccountID = a.AccountID,
                      Acknowledgement = a.Acknowledgement,
                      AllSubmissions = a.AllSubmissions,
                      AcknowledgementType = a.AcknowledgementType,
                      CreatedBy = a.CreatedBy,
                      CreatedOn = a.CreatedOn,
                      FormID = a.FormID,
                      FormFields = a.FormFields,
                      HTMLContent = a.HTMLContent,
                      IsDeleted = a.IsDeleted,
                      LastModifiedBy = a.LastModifiedBy,
                      LastModifiedOn = a.LastModifiedOn,
                      Name = a.Name,
                      Status = a.Status,
                      UniqueSubmissions = a.UniqueSubmissions
                  });

            foreach (FormsDb form in forms)
            {
                yield return Mapper.Map<FormsDb, Form>(form);
            }
        }

        /// <summary>
        /// Find all FORMs
        /// </summary>
        /// <param name="name"></param>
        /// <param name="limit"></param>
        /// <param name="pageNumber"></param>
        /// <param name="status"></param>
        /// <param name="AccountID"></param>
        /// <returns></returns>
        public IEnumerable<Form> FindAll(string name, int limit, int pageNumber, byte status, int AccountID)
        {
            var predicate = PredicateBuilder.True<FormsDb>();
            var records = (pageNumber - 1) * limit;
            if (!string.IsNullOrEmpty(name))
            {
                name = name.ToLower();
                predicate = predicate.And(a => a.Name.Contains(name));
            }
            if (status != 0)
            {
                predicate = predicate.And(a => a.Status == status);
            }
            predicate = predicate.And(a => a.IsDeleted == false);
            predicate = predicate.And(a => a.AccountID == AccountID);
            IEnumerable<FormsDb> forms = findFormsSummary(predicate).Skip(records).Take(limit);
            foreach (FormsDb form in forms)
            {
                yield return ConvertToDomain(form);
            }
        }

        /// <summary>
        /// Find all FORMs
        /// </summary>
        /// <param name="name"></param>
        /// <param name="status"></param>
        /// <param name="AccountID"></param>
        /// <returns></returns>
        public IEnumerable<Form> FindAll(string name, byte status, int AccountID)
        {
            var predicate = PredicateBuilder.True<FormsDb>();
            if (!string.IsNullOrEmpty(name))
            {
                name = name.ToLower();
                predicate = predicate.And(a => a.Name.Contains(name));
            }
            if (status != 0)
            {
                predicate = predicate.And(a => a.Status == status);
            }
            predicate = predicate.And(a => a.IsDeleted == false);
            predicate = predicate.And(a => a.AccountID == AccountID);

            IEnumerable<FormsDb> forms = findFormsSummary(predicate);
            foreach (FormsDb form in forms)
            {
                yield return ConvertToDomain(form);
            }
        }

        /// <summary>
        /// Finds the forms summary.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        IEnumerable<FormsDb> findFormsSummary(System.Linq.Expressions.Expression<Func<FormsDb, bool>> predicate)
        {
            IEnumerable<FormsDb> forms = ObjectContextFactory.Create().Forms
                .AsExpandable()
                .Where(predicate).OrderByDescending(c => c.FormID).Select(a =>
                    new
                    {
                        FormID = a.FormID,
                        Name = a.Name,
                        Status = a.Status
                    }).ToList().Select(x => new FormsDb
                    {
                        FormID = x.FormID,
                        Name = x.Name,
                        Status = x.Status
                    });
            return forms;
        }

        /// <summary>
        /// Gets the contacts by form identifier.
        /// </summary>
        /// <param name="FormID">The form identifier.</param>
        /// <returns></returns>
        public IEnumerable<int> GetContactsByFormID(int FormID)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<int> formSubmissions = db.FormSubmissions.Where(i => i.FormID == FormID).Select(i => i.ContactID);
            return formSubmissions;
        }

        /// <summary>
        /// Gets the form submission by identifier.
        /// </summary>
        /// <param name="FormSubmissionID">The form submission identifier.</param>
        /// <returns></returns>
        public FormSubmission GetFormSubmissionByID(int FormSubmissionID)
        {
            var db = ObjectContextFactory.Create();
            FormSubmissionDb formsubmissiondb = db.FormSubmissions.Where(i => i.FormSubmissionID == FormSubmissionID).FirstOrDefault();
            if (formsubmissiondb == null)
                return null;
            FormSubmission formsubmission = Mapper.Map<FormSubmissionDb, FormSubmission>(formsubmissiondb);
            return formsubmission;
        }

        /// <summary>
        /// Determines whether [is associated with workflows] [the specified form identifier].
        /// </summary>
        /// <param name="FormID">The form identifier.</param>
        /// <returns></returns>
        public bool isAssociatedWithWorkflows(int[] FormID)
        {
            var db = ObjectContextFactory.Create();
            return db.WorkflowTriggers.Where(c => c.FormID.HasValue && FormID.Contains(c.FormID.Value)).Count() > 0;
        }

        /// <summary>
        /// Determines whether [is associated with lead score rules] [the specified form identifier].
        /// </summary>
        /// <param name="FormID">The form identifier.</param>
        /// <returns></returns>
        public bool isAssociatedWithLeadScoreRules(int[] FormID)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<string> FormIDs = FormID.Select(x => x.ToString());
            return db.LeadScoreRules.Where(c => c.ConditionID == 3
                                            && c.IsActive == true
                                            && FormIDs.Contains(c.ConditionValue))
                                            .Count() > 0;

        }

        /// <summary>
        /// Determines whether [is linked to workflows] [the specified form identifier].
        /// </summary>
        /// <param name="FormID">The form identifier.</param>
        /// <returns></returns>
        public bool isLinkedToWorkflows(int FormID)
        {
            using (var db = ObjectContextFactory.Create())
            {
                return db.WorkflowTriggers.Where(i => i.FormID == FormID)
                         .Join(db.Workflows.Where(i => i.IsDeleted != true),
                          i => i.WorkflowID, j => j.WorkflowID,
                          (i, j) => new
                          {
                              i.WorkflowID
                          }).Count() > 0;
            }
        }

        /// <summary>
        /// Gets all fields.
        /// </summary>
        /// <param name="AccountID">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Field> GetAllFields(int AccountID)
        {
            var db = ObjectContextFactory.Create();
            /* 
             in the db and 23,26,25 are donotemail, leadscore and owner fields AND 27,28,29,41 are Created By,Created On,Last Touched,Last Touched Through*/
            int[] notrequiredFileds = new int[] { 
                (int)ContactFields.DonotEmail,
                (int)ContactFields.LeadScore,
                (int)ContactFields.CreatedBy,
                (int)ContactFields.CreatedOn,
                (int)ContactFields.LastTouchedThrough,
                (int)ContactFields.LastTouched,
                (int)ContactFields.Owner,
                (int)ContactFields.LifecycleStageField,
                (int)ContactFields.LeadSource,
                (int)ContactFields.PartnerTypeField,
                (int)ContactFields.HomePhoneField,
                (int)ContactFields.MobilePhoneField,
                (int)ContactFields.WorkPhoneField,
                (int)ContactFields.Community,
                (int)ContactFields.FirstSourceType,
                (int)ContactFields.WebPage,
                (int)ContactFields.WebPageDuration,
                (int)ContactFields.ContactTag,
                (int)ContactFields.FormName,
                (int)ContactFields.FormsubmittedOn,
                (int)ContactFields.LeadAdapter,
                (int)ContactFields.FirstLeadSource,
                (int)ContactFields.FirstLeadSourceDate,
                (int)ContactFields.LeadSourceDate,
                (int)ContactFields.LastNoteDate,
                (int)ContactFields.LastNote,
                (int)ContactFields.NoteSummary,
                (int)ContactFields.TourType,
                (int)ContactFields.TourDate,
                (int)ContactFields.TourCreator,
                (int)ContactFields.EmailStatus,
                (int)ContactFields.TourAssignedUsers,
                (int)ContactFields.ActionCreatedDate,
                (int)ContactFields.ActionType,
                (int)ContactFields.ActionDate,
                (int)ContactFields.ActionStatus,
                (int)ContactFields.ActionAssignedTo
            };

            IEnumerable<FieldsDb> Fields = db.Fields.Where(c => (c.AccountID == AccountID || c.AccountID == null)
                                                             && c.StatusID == (short?)FieldStatus.Active
                                                             && !notrequiredFileds.Contains(c.FieldID))
                                             .Include(b => b.CustomFieldValueOptions);
            IEnumerable<Field> customFields = Mapper.Map<IEnumerable<FieldsDb>, IEnumerable<Field>>(Fields);
            return customFields;
        }

        /// <summary>
        /// Gets the filed data by identifier.
        /// </summary>
        /// <param name="filedId">The filed identifier.</param>
        /// <returns></returns>
        public Field GetFiledDataById(int fieldId)
        {
            var result = default(Field);
            using (var db = ObjectContextFactory.Create())
            {
                var field = db.Fields.Where(c => c.FieldID == fieldId).Include(b => b.FieldInputTypes).FirstOrDefault();
                if (field != null) result = Mapper.Map<FieldsDb, Field>(field);
            }
            return result;
        }

        /// <summary>
        /// Getting all Spam KeyWords
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllSpamKeyWords()
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<string> SpamKeyWords = db.SpamKeyWords.Select(s => s.Value).ToList();
            return SpamKeyWords;
        }

        /// <summary>
        /// Getting All Spam Validators
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SpamValidator> GetAllSpamValidators(int accountId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @";with svCTE as 
                        (SELECT * FROM SpamValidators (nolock) where AccountID = @accountID
                        union
                        SELECT * FROM SpamValidators (nolock) where AccountID IS NULL),
                         svCTE1 as (select *, row_number() over(partition by [order] order by [Order] asc, accountid desc) row_num from svCTE)
                         select * from svCTE1 where row_num = 1 and RunValidation = 1";

            IEnumerable<SpamValidator> spamValidators = db.Get<SpamValidator>(sql, new { accountID = accountId });
            return spamValidators;
        }

        /// <summary>
        /// Checking Is Spam Or not.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="accountId"></param>
        /// <param name="isSpam"></param>
        /// <returns></returns>
        public bool CheckForSpamIP(string ipAddress, int accountId, bool isSpam)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT COUNT(1)  FROM SpamIPAddresses (nolock) IP where (IP.AccountID=@AccountID or IP.AccountID is null) and IP.IPAddress = dbo.IpToBinary4(@ip) AND IP.IsSpam = @isSpam";
            int spam = db.Get<int>(sql, new { AccountID = accountId, ip = ipAddress, isSpam = isSpam }).FirstOrDefault();
            return (spam > 0) ? true : false;
        }

        /// <summary>
        /// Checking For Exclusion Ips
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public bool CheckForIPExclusion(string ipAddress, int accountId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT COUNT(1)  FROM SpamIPAddresses (nolock) IP where (IP.AccountID=@AccountID or IP.AccountID is null) and IP.IPAddress = dbo.IpToBinary4(@ip) AND IP.IsSpam = 0";
            int spam = db.Get<int>(sql, new { AccountID = accountId, ip = ipAddress }).FirstOrDefault();
            return (spam > 0) ? true : false;
        }

        /// <summary>
        /// Inserting Spam IP
        /// </summary>
        /// <param name="IPAddress"></param>
        /// <param name="Spam"></param>
        /// <param name="AccountID"></param>
        public void InsertSpamIPAddress(string IPAddress, bool Spam, int AccountID)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"insert into spamipaddresses values(@IPAddress,@IsSpam,@AccountID)";
            db.Execute(sql, new { IPAddress = IPAddress, IsSpam = Spam, AccountID = AccountID });
        }

        /// <summary>
        /// Getting Form IP Submission Count
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public int GettingFormIPSubmissionCount(int accountId, string ipAddress, int timeLimit, bool isFormSubmission)
        {
            var db = ObjectContextFactory.Create();
            var sql = string.Empty;
            if (isFormSubmission)
            {
                sql = @"SELECT COUNT(1) FROM SubmittedFormData (nolock) fd 
                        where fd.AccountID=@AccountID  and DATEDIFF(MINUTE, fD.CreatedOn, GETUTCDATE()) < @Time and fd.IPAddress = @IPAddress";
            }
            else
            {
                sql = @"SELECT COUNT(1) FROM APILeadSubmissions (NOLOCK) WHERE AccountID=@AccountID AND DATEDIFF(MINUTE, SubmittedOn, GETUTCDATE()) < @Time AND IPAddress=@IPAddress";
            }
           
            int count = db.Get<int>(sql, new { AccountID = accountId, IPAddress = ipAddress, Time = timeLimit }).FirstOrDefault();
            return count;
        }

        public IEnumerable<Phone> GetPhoneFields(int contactId)
        {
            IEnumerable<Phone> phones = new List<Phone>();
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT ContactPhoneNumberID, ContactID, PhoneNumber AS Number, IsPrimary, PhoneType, DropdownValueTypeID FROM [dbo].[ContactPhoneNumbers] (NOLOCK) CP
                        JOIN DropdownValues (NOLOCK) dv on dv.DropdownValueID = CP.PhoneType
                        WHERE contactid = @contactId AND CP.IsDeleted = 0";
            phones = db.Get<Phone>(sql, new { contactId = contactId });
            return phones;
        }

        public string GetFormNameById(int formId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT f.Name  FROM Forms (nolock) F WHERE F.FormID=@formId";
            string formName = db.Get<string>(sql, new { formId = formId }).FirstOrDefault();
            return formName;
        }

        public IEnumerable<int> GetFormFieldIDs(int formId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"select fieldid from formfields (nolock) where formid = @formId";
                IEnumerable<int> formFields = db.Get<int>(sql, new { formId = formId }).ToList();
                return formFields;

            }
        }

        public IEnumerable<FormSubmission> GetFormSubmissions(int formId, DateTime? startDate, DateTime? endDate, int pageLimit, int pageNumber)
        {
            using (var db = ObjectContextFactory.Create())
            {
                IEnumerable<FormSubmission> formSubmissions = null;
                pageNumber -= 1;
                db.QueryStoredProc("dbo.Get_FormSubmissionsByForm", (reader) =>
                {
                    formSubmissions = reader.Read<FormSubmission>().ToList();
                    //totalHits = reader.Read<int>().First();
                }, new { FormID = formId, StartDate = startDate, EndDate = endDate, PageNumber = pageNumber, PageSize = pageLimit }, commandTimeout: 600);
                return formSubmissions;
            }
        }

        public FormAcknowledgement GetFormAcknowledgement(int formId)
        {
            Logger.Current.Verbose("Fetching Acknowledgement for form: " + formId);
            var sql = @"Select AcknowledgementType, Acknowledgement from Forms (nolock) where formid = @formId";
            FormAcknowledgement formAckowledgement = ObjectContextFactory.Create().Get<FormAcknowledgement>(sql, new { formId = formId }).FirstOrDefault();
            return formAckowledgement;
        }

        public int CreateAPIForm(string formName, int accountId, int userId)
        {
            var db = ObjectContextFactory.Create();
            FormsDb formDb = new FormsDb()
            {
                Name = formName,
                Acknowledgement = "Thank you",
                AcknowledgementType = AcknowledgementType.Message,
                HTMLContent = "<div></div>",
                Status = 201,
                AccountID = accountId,
                CreatedBy = userId,
                CreatedOn = DateTime.UtcNow,
                LastModifiedOn = DateTime.UtcNow,
                LastModifiedBy = userId,
                IsDeleted = false
            };
            db.Forms.Add(formDb);
            db.SaveChanges();
            return formDb.FormID;
        }

        public bool UpdateFormName(int formId, string formName, int modifiedBy)
        {
            bool hasUpdated = false;
            var db = ObjectContextFactory.Create();
            var form = db.Forms.Where(w => w.FormID == formId && w.IsAPIForm).FirstOrDefault();
            if (form != null && !string.IsNullOrEmpty(formName))
            {
                form.Name = formName;
                form.LastModifiedBy = modifiedBy;
                form.LastModifiedOn = DateTime.UtcNow;
                db.Forms.Attach(form);
                db.Entry(form).State = EntityState.Modified;
                db.SaveChanges();
                hasUpdated = true;
            }
            return hasUpdated;
        }

        public bool IsValidAPISubmission(int formId, int accountId)
        {
            var db = ObjectContextFactory.Create();
            return db.Forms.Where(w => w.FormID == formId && w.IsAPIForm && w.AccountID == accountId).Any();
        }

        /// <summary>
        /// Getting DropDownValueTypeIds By PhoneTypes
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public List<short> GetDropdownValueTypeIdsByPhoneTypes(List<short> ids, int accountId)
        {
            var db = ObjectContextFactory.Create();
            List<short> dropdownValueIds = new List<short>();
            dropdownValueIds = db.DropdownValues.Where(d => ids.Contains(d.DropdownValueID) && d.AccountID == accountId).Select(s => s.DropdownValueTypeID).ToList();
            return dropdownValueIds;
        }

        /// <summary>
        ///  for Indexing contact inserting data to IndexData table.
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="indexType"></param>
        /// <param name="isPerculationNeeded"></param>
        public void ScheduleIndexing(int entityId, IndexType indexType, bool isPerculationNeeded)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"INSERT INTO IndexData VALUES(NEWID(),@EntityID,@IndexTypeID,GETUTCDATE(),NULL,1,@IsPerculationNeeded)";
                db.Execute(sql, new { EntityID = entityId, IndexTypeID = (int)indexType, IsPerculationNeeded = isPerculationNeeded });
            }
        }

        public List<ApproveLeadsQueue> GetLeadsQueue(int accountId, int pageNumber, int limit, short dateRange)
        {
            var db = ObjectContextFactory.Create();
            List<ApproveLeadsQueue> queue = new List<ApproveLeadsQueue>();
            db.QueryStoredProc("[dbo].[Failed_Forms_Submissions_Report]", (r) =>
            {
                queue = r.Read<ApproveLeadsQueue>().ToList();
            }, new { AccountID = accountId, PageNumber = pageNumber, Limit = limit, DateRange = dateRange });

            return queue;
        }

        public void UpdateFormData(ApproveLeadsQueue queue, int userId)
        {
            if (queue != null)
            {
                var db = ObjectContextFactory.Create();
                if (queue.LeadSourceType == "Forms")
                {
                    IEnumerable<SubmittedFormFieldDataDb> submittedFormFieldData = db.SubmittedFormFieldData.Where(w => w.SubmittedFormDataID == queue.SubmittedFormDataID);
                    if (submittedFormFieldData != null && submittedFormFieldData.Any())
                    {
                        var firstNameField = submittedFormFieldData.Where(w => w.Field == "1").FirstOrDefault();
                        if (firstNameField != null)
                            firstNameField.Value = queue.FirstName;
                        var lastNameField = submittedFormFieldData.Where(w => w.Field == "2").FirstOrDefault();
                        if (lastNameField != null)
                            lastNameField.Value = queue.LastName;
                        var emailField = submittedFormFieldData.Where(w => w.Field == "7").FirstOrDefault();
                        if (emailField != null)
                            emailField.Value = queue.Email;
                        //var mobilePhoneField = submittedFormFieldData.Where(w => w.Field == "4").FirstOrDefault();
                        //if (mobilePhoneField != null)
                        //    mobilePhoneField.Value = queue.MobilePhone;

                        foreach (var entity in submittedFormFieldData)
                        {
                            db.Entry<SubmittedFormFieldDataDb>(entity).State = EntityState.Modified;
                        }

                        SubmittedFormDataDb submission = db.SubmittedFormData.Where(w => w.SubmittedFormDataID == queue.SubmittedFormDataID).FirstOrDefault();
                        if (submission != null)
                        {
                            submission.Remarks = "";
                            submission.Status = 1;
                            submission.FieldUpdatedOn = DateTime.UtcNow;
                            submission.FieldUpdatedBy = userId;
                            db.Entry<SubmittedFormDataDb>(submission).State = EntityState.Modified;
                        }
                        db.SaveChanges();
                    }
                }
            }
        }

        public string GetAPIJson(int apiLeadSubmissionID)
        {
            var db = ObjectContextFactory.Create();
            return db.APILeadSubmissions.Where(w => w.APILeadSubmissionID == apiLeadSubmissionID).Select(s => s.SubmittedData).FirstOrDefault();
        }

        public void UpdateAPIData(string json, int userId, int apiLeadSubmissionId)
        {
            var db = ObjectContextFactory.Create();
            var apiSubmission = db.APILeadSubmissions.Where(w => w.APILeadSubmissionID == apiLeadSubmissionId).FirstOrDefault();
            if (apiSubmission != null)
            {
                apiSubmission.SubmittedData = json;
                apiSubmission.IsProcessed = 1;
                apiSubmission.Remarks = "";
                apiSubmission.FieldUpdatedOn = DateTime.UtcNow;
                apiSubmission.FieldUpdatedBy = userId;

                db.Entry<APILeadSubmissionsDb>(apiSubmission).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public List<int> GetActiveAcountIds()
        {
            var db = ObjectContextFactory.Create();
            return db.Accounts.Where(w => !w.IsDeleted && w.Status == 1).Select(s => s.AccountID).ToList();
        }

        public List<UserSummary> GetUserDetails(int accountId)
        {
            var db = ObjectContextFactory.Create();
            List<UserSummary> summary = db.Users.Where(w => w.AccountID == accountId && !w.IsDeleted && w.Status == Status.Active)
                                        .Join(db.RoleModules.Where(w => w.ModuleID == (byte)AppModules.ApproveLeads), u => u.RoleID, rm => rm.RoleID, (u, rm) => 
                                            new UserSummary() {
                                                UserEmail = u.PrimaryEmail,
                                                UserId = u.UserID,
                                                AccountId = u.AccountID
                                            }).ToList();
            return summary;
        }

        public IEnumerable<ActionContactsSummary> GetFailedFormsLeads(int accountId)
        {
            List<ActionContactsSummary> leads = new List<ActionContactsSummary>();
            var db = ObjectContextFactory.Create();
            return new List<ActionContactsSummary>();
        }
    }
}
