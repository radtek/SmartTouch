using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.ApplicationTour;
using SmartTouch.CRM.Repository.Database;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using AutoMapper;
using LandmarkIT.Enterprise.Utilities.Logging;
using LinqKit;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class ApplicationTourRepository : Repository<ApplicationTourDetails, int, ApplicationTourDetailsDb>, IApplicationTourDetailsRepository 
    {
        public ApplicationTourRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory)
        { }

        public IEnumerable<ApplicationTourDetails> FindAll()
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<ApplicationTourDetails> applicationTourDetails = new List<ApplicationTourDetails>();
            var tourDetails = db.ApplicationTourDetails.Include(i => i.Division).Include(i => i.Section).Where(w => w.Status == true).OrderBy(o => o.DivisionID).ThenBy(t => t.order).ToList();
            if (tourDetails != null)
                applicationTourDetails = Mapper.Map<IEnumerable<ApplicationTourDetailsDb>, IEnumerable<ApplicationTourDetails>>(tourDetails);
            return applicationTourDetails;
        }

        public override ApplicationTourDetails FindBy(int id)
        {
            var db = ObjectContextFactory.Create();
            var tourDetail = db.ApplicationTourDetails.Include(i => i.Division).Include(i => i.Section).Where(w => w.ApplicationTourDetailsID == id).FirstOrDefault();
            if (tourDetail != null)
                return ConvertToDomain(tourDetail);
            else
                return null;
        }

        public override ApplicationTourDetailsDb ConvertToDatabaseType(ApplicationTourDetails domainType, CRMDb context)
        {
            return Mapper.Map<ApplicationTourDetails, ApplicationTourDetailsDb>(domainType);
        }

        public override ApplicationTourDetails ConvertToDomain(ApplicationTourDetailsDb databaseType)
        {
            return Mapper.Map<ApplicationTourDetailsDb, ApplicationTourDetails>(databaseType);
        }

        public override void PersistValueObjects(ApplicationTourDetails domainType, ApplicationTourDetailsDb dbType, CRMDb context)
        {
            throw new NotImplementedException();
        }

        public void UpdateDetails(int applicationTourId, string title, string content, int userId)
        {
            if (applicationTourId != 0)
            {
                var db = ObjectContextFactory.Create();
                var dbObject = db.ApplicationTourDetails.Where(w => w.ApplicationTourDetailsID == applicationTourId).FirstOrDefault();
                dbObject.Content = content;
                dbObject.Title = title;
                dbObject.LastUpdatedBy = userId;
                dbObject.LastUpdatedOn = DateTime.UtcNow;
                db.SaveChanges();
            }
        }

        public IEnumerable<ApplicationTourDetails> GetByDivision(int divisionId)
        {
            IEnumerable<ApplicationTourDetails> details = new List<ApplicationTourDetails>();
            if (divisionId != 0)
            {
                var db = ObjectContextFactory.Create();
                IEnumerable<ApplicationTourDetailsDb> appTourDb = db.ApplicationTourDetails.Where(w => w.DivisionID == divisionId && !string.IsNullOrEmpty(w.Title) && !string.IsNullOrEmpty(w.Content) && w.Status).OrderBy(o => o.order);
                if (appTourDb != null && appTourDb.Any())
                    details = Mapper.Map<IEnumerable<ApplicationTourDetailsDb>, IEnumerable<ApplicationTourDetails>>(appTourDb);
            }
            return details;
        }

        public void UpdateTourVisit(int userId)
        {
            var db = ObjectContextFactory.Create();
            UsersDb user = db.Users.Where(w => w.UserID == userId).FirstOrDefault();
            if (user != null)
            {
                user.HasTourCompleted = true;
                db.SaveChanges();
            }
        }
    }
}
