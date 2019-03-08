using AutoMapper;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.Domain.LeadScoreRules;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class LeadScoreRepository : Repository<LeadScore, int, LeadScoreDb>, ILeadScoreRepository
    {

        public LeadScoreRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory)
        { }

        /// <summary>
        /// Determines whether [is score audited] [the specified contact identifier].
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="conditionId">The condition identifier.</param>
        /// <param name="conditionValue">The condition value.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <returns></returns>
        public IEnumerable<LeadScoreRule> IsScoreAudited(int contactId, int accountId, int conditionId, string conditionValue, int entityId, IEnumerable<LeadScoreRule> rules)
        {
            var db = ObjectContextFactory.Create();
           // var rulesDb = Mapper.Map<IEnumerable<LeadScoreRule>, IEnumerable<LeadScoreRulesDb>>(rules);
            var leadScoresList = db.LeadScores.Where(l => l.ContactID == contactId && l.EntityID == entityId).Select(s => s.LeadScoreRuleID);
            List<LeadScoreRule> unAuditedRules = rules.Where(w => !leadScoresList.Contains(w.Id)).ToList();
            return unAuditedRules;           
        }

        /// <summary>
        /// Converts the type of to database.
        /// </summary>
        /// <param name="leadScore">The lead score.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Invalid note id has been passed. Suspected Id forgery.</exception>
        public override LeadScoreDb ConvertToDatabaseType(LeadScore leadScore, CRMDb context)
        {
            LeadScoreDb leadScoreDb;

            if (leadScore.Id > 0)
            {
                leadScoreDb = context.LeadScores.SingleOrDefault(l => l.LeadScoreID == leadScore.Id);
                if (leadScoreDb == null)
                    throw new ArgumentException("Invalid note id has been passed. Suspected Id forgery.");
                leadScoreDb = Mapper.Map<LeadScore, LeadScoreDb>(leadScore, leadScoreDb);
            }
            else
            {
                leadScoreDb = Mapper.Map<LeadScore, LeadScoreDb>(leadScore);
            }
            return leadScoreDb;
        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="leadScoredb">The lead scoredb.</param>
        /// <returns></returns>
        public override LeadScore ConvertToDomain(LeadScoreDb leadScoredb)
        {
            return Mapper.Map<LeadScoreDb, LeadScore>(leadScoredb);
        }

        /// <summary>
        /// Persists the value objects.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="context">The context.</param>
        public override void PersistValueObjects(LeadScore domainType, LeadScoreDb dbType, CRMDb context)
        {
            //for future use
            // throw new NotImplementedException();
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<LeadScore> FindAll()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override LeadScore FindBy(int id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adjusts the lead score.
        /// </summary>
        /// <param name="leadScore">The lead score.</param>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="workflowActionId">The workflow action identifier.</param>
        public void AdjustLeadScore(int leadScore, int contactId, int accountId, int workflowActionId)
        {
            var db = ObjectContextFactory.Create();
            if (leadScore != 0 && contactId != 0 && accountId != 0)
            {
                var leadScoreRuleId = db.LeadScoreRules.Where(r => r.AccountID == accountId && r.ConditionValue == workflowActionId.ToString()
                                    && r.ConditionID == 10).Select(s => s.LeadScoreRuleID).FirstOrDefault();

                var IsPerson = db.Contacts.Any(c => c.ContactID == contactId && c.IsDeleted == false && c.ContactType == Entities.ContactType.Person && c.AccountID == accountId);

                if (leadScoreRuleId != 0 && IsPerson)
                {
                    LeadScoreDb leadscore = new LeadScoreDb();
                    leadscore.AddedOn = DateTime.Now.ToUniversalTime();
                    leadscore.ContactID = contactId;
                    leadscore.EntityID = workflowActionId;
                    leadscore.Score = leadScore;
                    leadscore.LeadScoreRuleID = leadScoreRuleId;
                    db.LeadScores.Add(leadscore);
                    db.Database.ExecuteSqlCommand("update contacts set leadscore = leadscore + {0} where contactid = {1}", leadScore, contactId);
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Inserts the lead scores.
        /// </summary>
        /// <param name="leadScores">The lead scores.</param>
        public int InsertLeadScores(IEnumerable<LeadScore> leadScores,int accountId)
        {
            int leadScore = default(int);
            using (var db = ObjectContextFactory.Create())
            {
                if (leadScores != null && leadScores.Any())
                {
                    int totalScore = leadScores.Sum(s => s.Score);
                    int contactID = leadScores.FirstOrDefault().ContactID.HasValue ? leadScores.FirstOrDefault().ContactID.Value : default(int);
                    List<LeadScoreDb> leadScoresDb = new List<LeadScoreDb>();
                    Logger.Current.Informational("Score to be updated for a contact" + totalScore);

                    foreach (LeadScore leadscore in leadScores)
                    {
                        LeadScoreDb leadScoreDb = new LeadScoreDb()
                        {
                            AddedOn = leadscore.AddedOn,
                            ContactID = (int)leadscore.ContactID,
                            EntityID = leadscore.EntityID,
                            LeadScoreRuleID = (int)leadscore.LeadScoreRuleID,
                            Score = leadscore.Score
                        };
                        leadScoresDb.Add(leadScoreDb);
                    }
                    db.LeadScores.AddRange(leadScoresDb);
                    db.Database.ExecuteSqlCommand("update contacts set leadscore = leadscore + {0} where contactid = {1}", totalScore, contactID);
                    db.SaveChanges();
                    Logger.Current.Informational("Leadscore updated successfully for the contact, id: " + contactID, " Leadscore : " + totalScore);
                    leadScore = db.Contacts.Where(c => c.ContactID == contactID && c.IsDeleted == false && c.AccountID == accountId).Select(s => s.LeadScore).FirstOrDefault();
                    Logger.Current.Informational("Leadscore after updating : " + leadScore);
                }
            }
            return leadScore;
        }

        /// <summary>
        /// Gets lead score based on contact
        /// </summary>
        /// <param name="contactId"></param>
        /// <returns></returns>
        public int GetScore(int contactId)
        {
            int leadScore = default(int);
            if (contactId != 0)
            {
                var db = ObjectContextFactory.Create();
                leadScore = db.Contacts.Where(c => c.ContactID == contactId && c.IsDeleted == false).Select(s => s.LeadScore).FirstOrDefault();
            }
            return leadScore;
        }
    }
}
