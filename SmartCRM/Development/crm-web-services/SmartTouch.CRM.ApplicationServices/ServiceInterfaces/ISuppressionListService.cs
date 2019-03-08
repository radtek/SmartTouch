using SmartTouch.CRM.ApplicationServices.Messaging.SuppressionList;
using SmartTouch.CRM.Domain.SuppressedEmails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface ISuppressionListService
    {
        InsertSuppressedEmailResponse InsertSuppressedEmails(InsertSuppressedEmailRequest request);
        InsertSuppressedDomainResponse InertSuppressedDomains(InsertSuppressedDomainRequest request);
        GetSuppressionEmailsResponse GetSuppressionEmails(GetSuppressionEmailsRequest request);
        ReIndexSuppressionListResponse ReIndexSuppressionList(ReIndexSuppressionListRequest request);
        DeleteSuppressionEmailResponse RemoveSuppressionEmail(DeleteSuppressionEmailRequest request);
        CheckSuppressionEmailsResponse CheckSuppressionEmails(CheckSuppressionEmailsRequest request);
        SearchSuppressionListResponse<T> SearchSuppressionList<T>(SearchSuppressionListRequest request) where T : SuppressionList;
    }
}
