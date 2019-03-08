using System;
using System.Linq;
using System.Collections.Generic;

using Moq;

namespace SmartTouch.CRM.Tests
{    
    public class MockData
    {
        public static IEnumerable<Mock<T>> CreateMockList<T>(MockRepository repository) where T : class
        {
            IList<Mock<T>> mockContacts = new List<Mock<T>>();
            foreach (int i in Enumerable.Range(1, 10))
            {
                var mockContact = repository.Create<T>();
                mockContacts.Add(mockContact);
            }
            return mockContacts;
        }
    }
}
