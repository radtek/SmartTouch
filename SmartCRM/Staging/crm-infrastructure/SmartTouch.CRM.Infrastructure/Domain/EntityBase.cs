using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nest;

namespace SmartTouch.CRM.Infrastructure.Domain
{
    [ElasticType(IdProperty = "Id")]
    public abstract class EntityBase<IdType>
    {
        private List<BusinessRule> brokenRules = new List<BusinessRule>();

        public virtual IdType Id { get; set; }

        protected abstract void Validate();

        protected void AddBrokenRule(BusinessRule businessRule)
        {
            brokenRules.Add(businessRule);
        }

        public IEnumerable<BusinessRule> GetBrokenRules()
        {
            brokenRules.Clear();
            Validate();
            return brokenRules;
        }

        public override bool Equals(object entity)
        {
            return entity != null
               && entity is EntityBase<IdType>
               && this == (EntityBase<IdType>)entity;
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        public static bool operator ==(EntityBase<IdType> entity1, EntityBase<IdType> entity2)
        {
            if ((object)entity1 == null && (object)entity2 == null)
            {
                return true;
            }

            if ((object)entity1 == null || (object)entity2 == null)
            {
                return false;
            }

            if (entity1.Id.ToString() == entity2.Id.ToString())
            {
                return true;
            }

            return false;
        }

        public static bool operator !=(EntityBase<IdType> entity1,
            EntityBase<IdType> entity2)
        {
            return (!(entity1 == entity2));
        }
    }
}
