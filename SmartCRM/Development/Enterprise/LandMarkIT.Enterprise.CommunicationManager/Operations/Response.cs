using LandmarkIT.Enterprise.CommunicationManager.DatabaseEntities;
using LandmarkIT.Enterprise.CommunicationManager.Repository;
using LandmarkIT.Enterprise.CommunicationManager.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandmarkIT.Enterprise.CommunicationManager.Operations
{
  public  class Response
    {
      public Guid TextResponse(SendTextResponse request)
      {
          CMDataAccess datatAccess = new CMDataAccess();
          TextResponse requestData = new TextResponse();
          if (request != null)
          {
              requestData.RequestGuid = request.RequestGuid;
              requestData.Token = request.Token;
              requestData.CreatedDate = DateTime.Now;
              foreach (var varItem in request.Details)
              {
                  TextResponseDetails textresponsedetails = new TextResponseDetails();
                  textresponsedetails.SenderId = varItem.SenderId;
                  textresponsedetails.From = varItem.From;
                  textresponsedetails.ServiceResponse = varItem.ServiceResponse;
                  textresponsedetails.Message = varItem.Message;
                  textresponsedetails.Status = varItem.Status;
                  textresponsedetails.To = string.Join(",", varItem.To.ToArray());
                  textresponsedetails.TextResponse = requestData;
                  requestData.TextResponseDetails.Add(textresponsedetails);
              }
            return  datatAccess.TextResponse(requestData);
     
          }
          else
          {
              return Guid.Empty;
          }
          //return Guid.NewGuid();
      }

      public Guid MailResponse(SendMailResponse request)
      {
          CMDataAccess datatAccess = new CMDataAccess();
          MailResponse requestData = new MailResponse();
          if (request != null)
          {
              requestData.RequestGuid = request.RequestGuid;
              requestData.Token = request.Token;
              requestData.CreatedDate = DateTime.Now;
              MailResponseDetails mailresponsedetails = null;
              foreach (var varItem in request.Details)
              {
                 mailresponsedetails= new MailResponseDetails();
                  mailresponsedetails.MailGuid = varItem.MailGuid;
                  mailresponsedetails.To = varItem.To;
                  mailresponsedetails.From = varItem.From;
                  mailresponsedetails.CC = varItem.CC;
                  mailresponsedetails.BCC = varItem.BCC;
                  mailresponsedetails.Status = varItem.Status;
                  mailresponsedetails.ServiceResponse = varItem.ServiceResponse;
                  mailresponsedetails.CreatedDate = DateTime.Now;
                  mailresponsedetails.MailResponse = requestData;
                  requestData.MailResponseDetails.Add(mailresponsedetails);
              }
              return datatAccess.MailResponse(requestData);

          }
          return Guid.NewGuid();
      }
    }
}
