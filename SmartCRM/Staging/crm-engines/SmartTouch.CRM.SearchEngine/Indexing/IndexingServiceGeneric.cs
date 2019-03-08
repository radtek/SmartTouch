//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//using SmartTouch.CRM.Infrastructure.Domain;

//namespace SmartTouch.CRM.SearchEngine
//{
//    public class IndexingServiceGenric : IIndexingService
//    {
//        public int ReIndexAll<T>(IEnumerable<T> documents) where T : EntityBase<int>
//        {
//            var indxr = new GenericIndexer<T>();
//            return indxr.ReIndexAll(documents);
//        }

//        public int Index<T>(T document)
//        {
//            throw new NotImplementedException();
//        }

//        public int Update<T>(T document)
//        {
//            throw new NotImplementedException();
//        }

//        public int Remove<T>(T document)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
