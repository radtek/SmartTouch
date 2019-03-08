using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.Messaging.Tags;
using SmartTouch.CRM.Domain.Tag;

using Moq;
using Moq.Linq;

namespace SmartTouch.CRM.ApplicationServices.Tests.Tags
{
    public class TagMockData
    {
        public static IEnumerable<Tag> GetMockTags(MockRepository mockRepository, int objectCount)
        {
            IList<Tag> mockTags = new List<Tag>();
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                var mockTag = mockRepository.Create<Tag>();
                mockTags.Add(mockTag.Object);
            }
            return mockTags;
        }

        public static IEnumerable<Mock<Tag>> GetMockTagsWithSetups(MockRepository mockRepository, int objectCount)
        {
            IList<Mock<Tag>> mockTags = new List<Mock<Tag>>();
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                var mockTag = mockRepository.Create<Tag>();
                mockTag.Setup<int>(c => c.Id).Returns(i);
                mockTags.Add(mockTag);
            }
            return mockTags;
        }

        //public static IEnumerable<Mock<Tag>> GetMockTagsWithSetups(MockRepository mockRepository, int objectCount)
        //{
        //    IList<Mock<Tag>> mockTags = new List<Mock<Tag>>();
        //    foreach (int i in Enumerable.Range(1, objectCount))
        //    {
        //        var mockTag = mockRepository.Create<Tag>();
        //        mockTag.Setup<int>(t => t.Id).Returns(i);
        //        mockTags.Add(mockTag);
        //    }
        //    return mockTags;
        //}

        //public static IEnumerable<Mock<Tag>> GetMockTagNames(MockRepository MockRepository)
        //{
        //    IList<Mock<Tag>> mockTags = new List<Mock<Tag>>();
        //}
    }
}
