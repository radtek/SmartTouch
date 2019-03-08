using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Tags;
using DA = SmartTouch.CRM.Domain.Actions;
using SmartTouch.CRM.Domain.Notes;
using SmartTouch.CRM.Domain.Tours;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.Opportunities;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Domain.SuppressedEmails;

namespace SmartTouch.CRM.SearchEngine.Indexing
{
    internal static class IndexerFactory
    {
        public static Indexer<T, S> GetIndexer<T, S>() where T : EntityBase<S>
        {
            if (typeof(T).Equals(typeof(Contact)))
                return new ContactIndexer<Contact>() as Indexer<T, S>;
            else if (typeof(T).Equals(typeof(Tag)))
                return new TagIndexer<Tag>() as Indexer<T, S>;
            else if (typeof(T).Equals(typeof(DA.Action)))
                return new GenericIndexer<DA.Action>() as Indexer<T, S>;
            else if (typeof(T).Equals(typeof(Note)))
                return new GenericIndexer<Note>() as Indexer<T, S>;
            else if (typeof(T).Equals(typeof(Tour)))
                return new GenericIndexer<Tour>() as Indexer<T, S>;
            else if (typeof(T).Equals(typeof(Campaign)))
                return new CampaignIndexer<Campaign>() as Indexer<T, S>;
            else if (typeof(T).Equals(typeof(Opportunity)))
                return new GenericIndexer<Opportunity>() as Indexer<T, S>;
            else if (typeof(T).Equals(typeof(SuppressionList)) || typeof(T).Equals(typeof(SuppressedEmail)) || typeof(T).Equals(typeof(SuppressedDomain)))
                return new SuppressionListIndexer<SuppressionList>() as Indexer<T, S>;
            else
                throw new NotImplementedException("Indexer for the specified Type (" + typeof(T).ToString() + ") is not implemented.");
        }
    }

    //public class Fruit<T> where T : class
    //{ }

    //public class Apple<T> : Fruit<String>
    //{ }

    //public class FruitFactory
    //{
    //    public Fruit<String> GetFruit<T>() where T : class
    //    {
    //        return new Apple<String>();
    //    }
    //}
}
