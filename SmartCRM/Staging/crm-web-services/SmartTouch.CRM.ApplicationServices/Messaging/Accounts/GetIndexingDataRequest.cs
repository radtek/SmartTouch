﻿using SmartTouch.CRM.Domain.Accounts;
using System;
using System.Collections.Generic;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class GetIndexingDataRequest : ServiceRequestBase
    {
        public int ChunkSize { get; set; }
    }

    public class GetIndexingDataResponce : ServiceResponseBase
    {
        public IEnumerable<IndexingData> IndexingData { get; set; }
    }

    public class UpdateIndexingStatusRequest : ServiceRequestBase
    {
        public IEnumerable<Guid> ReferenceIds { get; set; }
        public IndexingStatus Status { get; set; }
    }

    public class UpdateIndexingStatusResponse : ServiceResponseBase
    {

    }
}
