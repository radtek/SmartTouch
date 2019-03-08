using AutoMapper;
using SmartTouch.CRM.Domain.Enterprises;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Repositories
{
   public  class EnterpriseServicesRepository : Repository<InvalidCouponsEngagedContact, int, InvalidCouponsEngagedContactsDb>, IEnterpriseServicesRepository
    {

        public EnterpriseServicesRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory)
        { }
        /// <summary>
        /// Getting All Invalid Coupons
        /// </summary>
        /// <param name="pagenumber"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public IEnumerable<ReportedCoupons> GetAllReportedCoupons(int pageNumber, int pageSize)
        {
            var procedureName = "[dbo].[GetGrabOnInvalidCouponResponses]";

            var parms = new List<SqlParameter> {
                   new SqlParameter{ParameterName ="@PageNumber", Value=pageNumber },
                    new SqlParameter{ParameterName ="@PageSize", Value=pageSize } };
            CRMDb context = new CRMDb();
            var objectContext = (context as IObjectContextAdapter).ObjectContext;
            objectContext.CommandTimeout = 400;
            return context.ExecuteStoredProcedure<ReportedCoupons>(procedureName, parms);
        }

        public void BulkInvalidCouponEngagedContacts(IList<InvalidCouponEnagedFrom> invalidData)
        {
            var db = ObjectContextFactory.Create();
            IList<InvalidCouponsEngagedContactsDb> invalidCouponData = new List<InvalidCouponsEngagedContactsDb>();
            foreach(InvalidCouponEnagedFrom data in invalidData)
            {
                InvalidCouponsEngagedContactsDb couponData = new InvalidCouponsEngagedContactsDb();
                couponData.ContactID = data.ContactId;
                couponData.FormSubmissionID = data.FormSubmissionId;
                couponData.LastUpdatedDate = DateTime.Now.ToUniversalTime();
                invalidCouponData.Add(couponData);
            }

            db.InvalidaCouponsEngagedContacts.AddRange(invalidCouponData);
            db.SaveChanges();
        }

        public override InvalidCouponsEngagedContact FindBy(int id)
        {
            throw new NotImplementedException();
        }

        public override InvalidCouponsEngagedContactsDb ConvertToDatabaseType(InvalidCouponsEngagedContact domainType, CRMDb context)
        {
            throw new NotImplementedException();
        }

        public override InvalidCouponsEngagedContact ConvertToDomain(InvalidCouponsEngagedContactsDb databaseType)
        {
            throw new NotImplementedException();
        }

        public override void PersistValueObjects(InvalidCouponsEngagedContact domainType, InvalidCouponsEngagedContactsDb dbType, CRMDb context)
        {
            throw new NotImplementedException();
        }
    }
}
  