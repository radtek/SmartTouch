using AutoMapper;
using LandmarkIT.Enterprise.Utilities.Logging;
using LinqKit;
using SmartTouch.CRM.Domain.CustomFields;
using SmartTouch.CRM.Domain.Fields;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class CustomFieldRepository : Repository<CustomFieldTab, int, CustomFieldTabDb>, ICustomFieldRepository
    {
        public CustomFieldRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory) { }

        /// <summary>
        /// Determines whether [is custom field name unique] [the specified custom field].
        /// </summary>
        /// <param name="customField">The custom field.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool IsCustomFieldNameUnique(Field customField)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deactivates the custom field.
        /// </summary>
        /// <param name="customFieldIds">The custom field ids.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void DeactivateCustomField(int[] customFieldIds)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets all custom field tabs.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<CustomFieldTab> GetAllCustomFieldTabs(int accountId)
        {
            IEnumerable<CustomFieldTabDb> customFieldTabsDb = ObjectContextFactory.Create().CustomFieldTabs.Where(c => c.AccountID == accountId && c.StatusID == (short)CustomFieldTabStatus.Active)
                .Include(c => c.CustomFieldSections)
                .Include(c => c.CustomFieldSections
                    .Select(a => a.CustomFields.Select(b => b.CustomFieldValueOptions))).ToList();

            IEnumerable<CustomFieldTab> customFieldTabs = Mapper.Map<IEnumerable<CustomFieldTabDb>, IEnumerable<CustomFieldTab>>(customFieldTabsDb);
            return customFieldTabs.OrderBy(c => c.SortId);
        }

        /// <summary>
        /// Gets all custom fields by account identifier.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Field> GetAllCustomFieldsByAccountId(int accountId)
        {
            var customFieldsDb = ObjectContextFactory.Create().Fields.Where(c => c.AccountID == accountId).ToList();
            IEnumerable<Field> customFields = Mapper.Map<IEnumerable<FieldsDb>, IEnumerable<Field>>(customFieldsDb);
            return customFields;
        }

        /// <summary>
        /// Gets all active custom fields by account identifier.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Field> GetAllActiveCustomFieldsByAccountID(int accountId)
        {
            var customFieldsDb = ObjectContextFactory.Create().Fields.Where(c => c.AccountID == accountId && c.StatusID == (short?)FieldStatus.Active).ToList();
            IEnumerable<Field> customFields = Mapper.Map<IEnumerable<FieldsDb>, IEnumerable<Field>>(customFieldsDb);
            return customFields;
        }

        /// <summary>
        /// Gets all custom fields for forms.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Field> GetAllCustomFieldsForForms(int accountId)
        {
            var customFieldsDb = ObjectContextFactory.Create().Fields.Where(c => c.AccountID == accountId
                && c.StatusID == (short?)FieldStatus.Active)
                .Include(b => b.CustomFieldValueOptions).ToList();

            IEnumerable<Field> customFields = Mapper.Map<IEnumerable<FieldsDb>, IEnumerable<Field>>(customFieldsDb);
            return customFields;
        }

        /// <summary>
        /// Gets all custom fields for imports.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Field> GetAllCustomFieldsForImports(int accountId)
        {
            /* 4,5,6 are the phone fields that exist as the contacts field initially 
               in the db and 23,26,25 are donotemail, leadscore and owner fields AND 27,28,29,41 are Created By,Created On,Last Touched,Last Touched Through*/
            // int[] notrequiredFileds = new int[10] { 4, 5, 6,     };

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
                (int)ContactFields.HomePhoneField,
                (int)ContactFields.MobilePhoneField,
                (int)ContactFields.WorkPhoneField,
                (int)ContactFields.Community,
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
                (int)ContactFields.TourType,
                (int)ContactFields.TourDate,
                (int)ContactFields.TourCreator,
                (int)ContactFields.TourAssignedUsers,
                (int)ContactFields.ActionCreatedDate,
                (int)ContactFields.ActionType,
                (int)ContactFields.ActionDate,
                (int)ContactFields.ActionStatus,
                (int)ContactFields.ActionAssignedTo,
                (int)ContactFields.LastNoteDate,
                (int)ContactFields.LastNote,
                (int)ContactFields.NoteSummary,


            };

            var customFieldsDb = ObjectContextFactory.Create().Fields
                                                     .Where(c => (c.AccountID == accountId || c.AccountID == null)
                                                              && c.StatusID == (short?)FieldStatus.Active
                                                              && !notrequiredFileds.Contains(c.FieldID)).ToList();

            IEnumerable<Field> customFields = Mapper.Map<IEnumerable<FieldsDb>, IEnumerable<Field>>(customFieldsDb);
            return customFields;
        }

        /// <summary>
        /// Gets the custom fields by section identifier.
        /// </summary>
        /// <param name="sectionID">The section identifier.</param>
        /// <returns></returns>
        public ICollection<FieldsDb> GetCustomFieldsBySectionId(int sectionID)
        {
            var oCF = ObjectContextFactory.Create().Fields;
            ICollection<FieldsDb> customFieldsDb = oCF.Include(c => c.CustomFieldValueOptions).Where(c => c.CustomFieldSectionID == sectionID).ToList();
            return customFieldsDb;
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="status">The status.</param>
        /// <param name="AccountID">The account identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<CustomFieldTab> FindAll(string name, int limit, int pageNumber, byte status, int AccountID)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="status">The status.</param>
        /// <param name="AccountID">The account identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<CustomFieldTab> FindAll(string name, byte status, int AccountID)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the custom field by identifier.
        /// </summary>
        /// <param name="customFieldId">The custom field identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public CustomFieldTab GetCustomFieldById(int customFieldId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override CustomFieldTab FindBy(int id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CustomFieldTab> FindAll()
        {
            IEnumerable<CustomFieldTabDb> customFieldTabs = ObjectContextFactory.Create().CustomFieldTabs;
            foreach (CustomFieldTabDb customFieldTab in customFieldTabs)
            {
                yield return Mapper.Map<CustomFieldTabDb, CustomFieldTab>(customFieldTab);
            }
        }

        /// <summary>
        /// Persists the value objects.
        /// </summary>
        /// <param name="customFieldTab">The custom field tab.</param>
        /// <param name="customFieldTabDb">The custom field tab database.</param>
        /// <param name="db">The database.</param>
        public override void PersistValueObjects(CustomFieldTab customFieldTab, CustomFieldTabDb customFieldTabDb, CRMDb db)
        {
            if (customFieldTab.Id > 0)
                PersistCustomFieldSections(customFieldTab, customFieldTabDb, db);
        }

        /// <summary>
        /// Persists the custom field sections.
        /// </summary>
        /// <param name="customFieldTab">The custom field tab.</param>
        /// <param name="customFieldTabDb">The custom field tab database.</param>
        /// <param name="db">The database.</param>
        private void PersistCustomFieldSections(CustomFieldTab customFieldTab, CustomFieldTabDb customFieldTabDb, CRMDb db)
        {
            IEnumerable<CustomFieldSectionDb> customFieldSectionsDb = customFieldTabDb.CustomFieldSections.Where(c => c.TabID == customFieldTab.Id).ToList();

            foreach (CustomFieldSection customFieldSection in customFieldTab.Sections)
            {
                CustomFieldSectionDb customFieldSectionDb = customFieldSection.Id == 0 ?
                    Mapper.Map<CustomFieldSection, CustomFieldSectionDb>(customFieldSection) :
                    customFieldSectionsDb.Where(c => c.CustomFieldSectionID == customFieldSection.Id).FirstOrDefault();
                PersistCustomFields(customFieldSection, customFieldSectionDb, db);
            }

            IList<int> customFieldSectionIds = customFieldTab.Sections.Where(c => c.Id > 0).Select(c => c.Id).ToList();
            var unMapCustomFieldSections = db.CustomFieldSections.Where(a => !customFieldSectionIds.Contains(a.CustomFieldSectionID)
                && a.StatusID != (short)CustomFieldSectionStatus.Deleted && a.TabID == customFieldTab.Id);
            foreach (CustomFieldSectionDb customFieldSectionMapDb in unMapCustomFieldSections)
            {
                db.CustomFieldSections.Remove(customFieldSectionMapDb);
            }
        }

        /// <summary>
        /// Persists the custom fields.
        /// </summary>
        /// <param name="customFieldSection">The custom field section.</param>
        /// <param name="sectionDb">The section database.</param>
        /// <param name="db">The database.</param>
        private void PersistCustomFields(CustomFieldSection customFieldSection, CustomFieldSectionDb sectionDb, CRMDb db)
        {

            IList<int> customFieldIds = customFieldSection.CustomFields.Where(c => c.Id > 0).Select(c => c.Id).ToList();
            var unMapCustomFields = db.Fields.Where(cc => !customFieldIds.Contains(cc.FieldID)
                && cc.CustomFieldSectionID == customFieldSection.Id && cc.StatusID != (short?)FieldStatus.Deleted)
                .Select(cc => cc).Distinct().ToList();

            foreach (FieldsDb customFieldMapDb in unMapCustomFields)
            {
                customFieldMapDb.StatusID = (short?)FieldStatus.Deleted;
            }

        }

        /// <summary>
        /// Persists the custom field value options.
        /// </summary>
        /// <param name="customField">The custom field.</param>
        /// <param name="customFieldDb">The custom field database.</param>
        /// <param name="db">The database.</param>
        private void PersistCustomFieldValueOptions(CustomField customField, FieldsDb customFieldDb, CRMDb db)
        {
            foreach (FieldValueOption customFieldValueOption in customField.ValueOptions.Where(v => v.Id == 0))
            {
                //valueOptionDb = customFieldDb.CustomFieldValueOptions.Where(c => c.CustomFieldValueOptionID == customFieldValueOption.Id).FirstOrDefault();
                CustomFieldValueOptionsDb valueOptionDb = Mapper.Map<FieldValueOption, CustomFieldValueOptionsDb>(customFieldValueOption);
            }

            IList<int> customFieldValueOptionIds = customField.ValueOptions.Where(c => c.Id > 0).Select(c => c.Id).ToList();
            var unMapCustomValueOptions = db.CustomFieldValueOptions.Where(cc => !customFieldValueOptionIds.Contains(cc.CustomFieldValueOptionID)
                && cc.CustomFieldID == customField.Id && cc.IsDeleted == false)
                .Select(cc => cc).Distinct().ToList();

            foreach (CustomFieldValueOptionsDb customFieldValueOptionMapDb in unMapCustomValueOptions)
            {
                customFieldValueOptionMapDb.IsDeleted = true;
            }
        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="customFieldDb">The custom field database.</param>
        /// <returns></returns>
        public override CustomFieldTab ConvertToDomain(CustomFieldTabDb customFieldDb)
        {
            return Mapper.Map<CustomFieldTabDb, CustomFieldTab>(customFieldDb);
        }

        /// <summary>
        /// Converts the type of to database.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Invalid form id has been passed. Suspected Id forgery.</exception>
        public override CustomFieldTabDb ConvertToDatabaseType(CustomFieldTab domainType, CRMDb context)
        {
            CustomFieldTabDb customFieldTabDb;
            if (domainType.Id > 0)
            {
                var tabs = context.CustomFieldTabs.Where(t => t.CustomFieldTabID == domainType.Id)
                     .GroupJoin(context.CustomFieldSections.Where(cs => cs.StatusID == (short)CustomFieldSectionStatus.Active),
                     t => t.CustomFieldTabID, cs => cs.TabID, (tab, cs) => new
                     {
                         Tab = tab,
                         Sections = cs.GroupJoin(
                             context.Fields.Where(cf => cf.StatusID != (short?)FieldStatus.Deleted),
                                 cfs => cfs.CustomFieldSectionID, fld => fld.CustomFieldSectionID, (cfs, fld) => new
                                 {
                                     Section = cfs,
                                     CustomFields = fld.GroupJoin(
                                         context.CustomFieldValueOptions.Where(cfv => cfv.IsDeleted == false), cff => cff.FieldID, cfv => cfv.CustomFieldID,
                                         (cff, cfv) => new { CustomField = cff, FieldValueOptions = cfv })
                                 })
                     });

                tabs.Select(t => t.Sections.ToList()).First(); //do not remove this line.

                customFieldTabDb = tabs.Select(t => t.Tab).First();

                if (customFieldTabDb == null)
                    throw new ArgumentException("Invalid form id has been passed. Suspected Id forgery.");

                Mapper.Map<CustomFieldTab, CustomFieldTabDb>(domainType, customFieldTabDb);
            }
            else
            {
                customFieldTabDb = Mapper.Map<CustomFieldTab, CustomFieldTabDb>(domainType);
            }
            return customFieldTabDb;
        }

        /// <summary>
        /// Determines whether [is custom field tab name unique] [the specified custom field tab].
        /// </summary>
        /// <param name="customFieldTab">The custom field tab.</param>
        /// <returns></returns>
        public bool IsCustomFieldTabNameUnique(CustomFieldTab customFieldTab)
        {
            var db = ObjectContextFactory.Create();
            var customFieldTabFound = db.CustomFieldTabs.Where(c => c.Name.ToString().ToLower() == customFieldTab.Name.ToString().ToLower() && c.AccountID == customFieldTab.AccountId
                && c.StatusID == (short)CustomFieldTabStatus.Active).Select(c => c).FirstOrDefault();
            if (customFieldTabFound != null && customFieldTab.Id != customFieldTabFound.CustomFieldTabID)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gets the custom field values of the contact
        /// </summary>
        /// <param name="contactId"></param>
        /// <returns></returns>
        public IEnumerable<ContactCustomField> ContactCustomFields(int contactId)
        {
            var contactCustomFieldsDb = ObjectContextFactory.Create().ContactCustomFields.Where(c => c.ContactID == contactId).ToList();
            IEnumerable<ContactCustomField> contactCustomFields = Mapper.Map<IEnumerable<ContactCustomFieldsDb>, IEnumerable<ContactCustomField>>(contactCustomFieldsDb);
            return contactCustomFields;

        }

        /// <summary>
        /// Deactivates the custom field tab.
        /// </summary>
        /// <param name="tabId">The tab identifier.</param>
        public void DeactivateCustomFieldTab(int tabId)
        {
            var db = ObjectContextFactory.Create();
            var tab = db.CustomFieldTabs.Include(c => c.CustomFieldSections).Include(c => c.Status).SingleOrDefault(c => c.CustomFieldTabID == tabId);
            if (tab == null)
                return;
            tab.StatusID = (short)Entities.CustomFieldTabStatus.Deleted;
            if (tab.CustomFieldSections != null)
            {
                IList<int?> associatedSections = tab.CustomFieldSections.Select(c => (int?)c.CustomFieldSectionID).ToList();
                var associatedCustomFields = db.Fields.Where(c => associatedSections.Contains(c.CustomFieldSectionID));
                foreach (var field in associatedCustomFields)
                {
                    field.StatusID = (short?)FieldStatus.Deleted;
                }
            }
            db.SaveChanges();
        }

        /// <summary>
        /// Gets the custom fields value options.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<FieldValueOption> GetCustomFieldsValueOptions(int accountId)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            var db = ObjectContextFactory.Create();
            IEnumerable<CustomFieldValueOptionsDb> customFieldsValueOptionsDb = new List<CustomFieldValueOptionsDb>();
            IEnumerable<FieldValueOption> customFieldValueOptions = new List<FieldValueOption>();

           
            var sql = @"SELECT CVO.* FROM Fields(NOLOCK) F
                        INNER JOIN CustomFieldValueOptions(NOLOCK) CVO ON CVO.CustomFieldID = F.FieldID
                        WHERE AccountID = @accountId";
            customFieldsValueOptionsDb = db.Get<CustomFieldValueOptionsDb>(sql, new { accountId = accountId });
            if (customFieldsValueOptionsDb != null && customFieldsValueOptionsDb.Count() > 0)
                customFieldValueOptions = Mapper.Map<IEnumerable<CustomFieldValueOptionsDb>, IEnumerable<FieldValueOption>>(customFieldsValueOptionsDb);

            sw.Stop();
            var timeelapsed = sw.Elapsed;
            Logger.Current.Informational("Time elapsed to fetch custom-fields value options including mapping" + timeelapsed);
            return customFieldValueOptions;
        }

        /// <summary>
        /// Gets the name of the custom field tab by.
        /// </summary>
        /// <param name="TabName">Name of the tab.</param>
        /// <param name="AccountID">The account identifier.</param>
        /// <returns></returns>
        public CustomFieldTab GetCustomFieldTabByName(string TabName, int AccountID)
        {
            using (var db = ObjectContextFactory.Create())
            {
                CustomFieldTabDb customfieldTab = db.CustomFieldTabs.Where(i => i.Name == TabName && i.AccountID == AccountID)
                                                    .Include(c => c.CustomFieldSections.Select(a => a.CustomFields)).FirstOrDefault();

                if (customfieldTab != null)
                    return Mapper.Map<CustomFieldTabDb, CustomFieldTab>(customfieldTab);
                return null;
            }
        }

        /// <summary>
        /// Gets the lead adapter custom fields.
        /// </summary>
        /// <param name="LeadAdapterType">Type of the lead adapter.</param>
        /// <param name="AccountID">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<CustomField> GetLeadAdapterCustomFields(LeadAdapterTypes LeadAdapterType, int AccountID)
        {
            var db = ObjectContextFactory.Create();
            var fields = db.LeadAdapterCustomFields.Where(i => i.LeadAdapterType == (byte)LeadAdapterType)
                            .Select(x => new CustomField
                            {
                                AccountID = AccountID,
                                DisplayName = x.Title,
                                FieldInputTypeId = (SmartTouch.CRM.Entities.FieldType)x.FieldInputTypeID,
                                IsCustomField = true,
                                Title = x.Title,
                                StatusId = FieldStatus.Active,
                                IsDropdownField = false
                            });
            return fields;
        }

        /// <summary>
        /// Gets the lead adapter custom field tab.
        /// </summary>
        /// <param name="AccountID">The account identifier.</param>
        /// <returns></returns>
        public CustomFieldTab GetLeadAdapterCustomFieldTab(int AccountID)
        {
            using (var db = ObjectContextFactory.Create())
            {
                CustomFieldTabDb customfieldTab = db.CustomFieldTabs.Where(i => i.IsLeadAdapterTab && i.AccountID == AccountID)
                                                    .Include(c => c.CustomFieldSections.Select(a => a.CustomFields)).FirstOrDefault();

                if (customfieldTab != null)
                    return Mapper.Map<CustomFieldTabDb, CustomFieldTab>(customfieldTab);
                return null;
            }
        }

        /// <summary>
        /// Gets the saved searchs count for custom field by identifier.
        /// </summary>
        /// <param name="fieldId">The field identifier.</param>
        /// <returns>savedSearchesCount</returns>
        public int GetSavedSearchsCountForCustomFieldById(int fieldId, int? valueOptionId)
        {
            var db = ObjectContextFactory.Create();
            var sql = string.Empty;

            if (fieldId != 0 && !valueOptionId.HasValue)
            {
                sql = @"select count(*) from SearchDefinitions (nolock) s
                        left outer join SearchFilters (nolock) sf on sf.SearchDefinitionID = s.SearchDefinitionID
                        left outer join fields (nolock) f on f.FieldID = sf.FieldID
                        where sf.FieldID in (@FieldID) and s.SelectAllSearch = 0 
                        group by s.SearchDefinitionID";

                var count = db.Get<int>(sql, new { FieldID = fieldId }).Count();
                return count;
            }
            else if (fieldId != 0 && valueOptionId.HasValue)
            {
                var count = db.SearchDefinitions.Where(p=>p.SelectAllSearch == false).Join(db.SearchFilters.Where(sfi => sfi.FieldID == fieldId && sfi.SearchText.Contains(valueOptionId.ToString())),
                            s => s.SearchDefinitionID, sf => sf.SearchDefinitionID, (s, sf) => new { FieldId = sf.FieldID, Sid = s.SearchDefinitionID }).
                            Join(db.Fields, g => g.FieldId, f => f.FieldID, (g, f) => new { searchId = g.Sid }).GroupBy(g => g.searchId).Count();
                return count;
            }
            return 0;

        }

        /// <summary>
        /// Gets the name of the custom field value.
        /// </summary>
        /// <param name="fieldId">The field identifier.</param>
        /// <param name="valueOptions">The value options.</param>
        /// <returns></returns>
        public  string GetCustomFieldValueName(int fieldId, string valueOptions)
        { 
            string ValueName = string.Empty;
            if (fieldId != 0 && valueOptions != null)
            {
                using (var db = ObjectContextFactory.Create())
                {
                    List<int> ValueOptions = new List<int>();
                    if (valueOptions.Contains('|'))
                        ValueOptions.AddRange(valueOptions.Split('|').Select(s => Convert.ToInt32(s)));
                    if (ValueOptions.Count > 0)
                       ValueName = string.Join(", ", db.CustomFieldValueOptions.Where(c => c.CustomFieldID == fieldId && ValueOptions.Contains(c.CustomFieldValueOptionID)).Select(s => s.Value));
                    else
                    {
                       int optionID = Convert.ToInt32(valueOptions);
                       ValueName = db.CustomFieldValueOptions.Where(c => c.CustomFieldID == fieldId && c.CustomFieldValueOptionID == optionID).Select(s => s.Value).FirstOrDefault();
                    }
                }
            }
            return ValueName;
        }

        /// <summary>
        /// Getting all Field Names.
        /// </summary>
        /// <param name="fieldIds"></param>
        /// <returns></returns>
        public string[] GetAllFieldTitles(List<int> fieldIds)
        {
            var db = ObjectContextFactory.Create();
            string[] fieldNames = db.Fields.Where(f => fieldIds.Contains(f.FieldID)).Select(s => s.Title).ToArray();
            return fieldNames;
        }
    }
}
