using LandmarkIT.Enterprise.CommunicationManager.Contracts;
using LandmarkIT.Enterprise.CommunicationManager.Repositories;
using LandmarkIT.Enterprise.CommunicationManager.Requests;
using LandmarkIT.Enterprise.CommunicationManager.Responses;
using LandmarkIT.Enterprise.Utilities.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;


namespace LandmarkIT.Enterprise.CommunicationManager.Extensions
{
    public static class StoredProcedureExtensions
    {
        public static List<SendSingleMailRequest> ExecuteProcessQueue(this EfUnitOfWork context, short queueSize = 30, List<SendSingleMailRequest> requests = null, List<SendMailResponse> responses = null, int instance=1)
        {
            Logger.Current.Verbose("Request received for executing mail stored-procedure");
            //TODO: correct this
            var queueTime = DateTime.Now;

            Logger.Current.Verbose("Creating data-table for MailQueue");

            try
            {
                var dataTable = new DataTable();
                dataTable.Columns.Add("TokenGuid", System.Type.GetType("System.Guid"));
                dataTable.Columns.Add("RequestGuid", System.Type.GetType("System.Guid"));
                dataTable.Columns.Add("From", typeof(string));
                dataTable.Columns.Add("PriorityID", typeof(byte));
                dataTable.Columns.Add("ScheduledTime", typeof(DateTime));
                dataTable.Columns.Add("QueueTime", typeof(DateTime));
                dataTable.Columns.Add("DisplayName", typeof(string));
                dataTable.Columns.Add("ReplyTo", typeof(string));
                dataTable.Columns.Add("To", typeof(string));
                dataTable.Columns.Add("CC", typeof(string));
                dataTable.Columns.Add("BCC", typeof(string));
                dataTable.Columns.Add("Subject", typeof(string));
                dataTable.Columns.Add("Body", typeof(string));
                dataTable.Columns.Add("IsBodyHtml", typeof(bool));
                dataTable.Columns.Add("StatusID", typeof(byte));
                dataTable.Columns.Add("ServiceResponse", typeof(string));

                if (requests != null && responses != null)
                {
                    Logger.Current.Informational("No of requests count : " + requests.Count + "  " + "No of responses count : " + responses.Count);
                    requests.ForEach(item =>
                    {
                        Logger.Current.Informational("Request GUID : " + item.RequestGuid);
                        var response = responses.Where(i => i.RequestGuid == Guid.NewGuid()).FirstOrDefault();
                        if (response == null)
                        {
                            Logger.Current.Informational("Request GUID : " + item.RequestGuid);
                            response = new SendMailResponse();
                            response.StatusID = CommunicationStatus.Failed;
                            response.ServiceResponse = item.RequestGuid + " response not found.";
                        }
                        dataTable.Rows.Add(new object[] { item.TokenGuid, item.RequestGuid, item.From, (byte)item.PriorityID, item.ScheduledTime, queueTime, 
                                item.DisplayName, item.ReplyTo, item.To, item.CC, item.BCC
                                ,item.Subject, item.Body, item.IsBodyHtml, (byte)response.StatusID, response.ServiceResponse});

                        dataTable.AcceptChanges();
                    });
                }

                var procedureName = "[dbo].[updateAndGetNextQueue]";
                var parms = new List<SqlParameter>
                {   
                    new SqlParameter{ParameterName ="@MailQueue", Value = dataTable, SqlDbType= SqlDbType.Structured, TypeName="[dbo].[MailQueue]" },                
                    new SqlParameter{ParameterName ="@noOfRecords", Value = queueSize}   ,             
                    new SqlParameter{ParameterName ="@instance", Value = instance}   
                };

                return context.ExecuteStoredProcedure<SendSingleMailRequest>(procedureName, parms).ToList();
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured while executing stored procedure -> updateAndGetNextQueue " + ex);
                return new List<SendSingleMailRequest>();
            }
        }

        public static List<SendSingleTextRequest> ExecuteTextQueue(this EfUnitOfWork context, short queueSize = 30, List<SendSingleTextRequest> requests = null, List<SendTextResponse> responses = null)
        {
            Logger.Current.Verbose("Request received for executing text stored-procedure");            

            try
            {
                var dataTable = new DataTable();
                dataTable.Columns.Add("TextResponseID", typeof(int));
                dataTable.Columns.Add("From", typeof(string));
                dataTable.Columns.Add("To", typeof(string));
                dataTable.Columns.Add("SenderID", typeof(string));
                dataTable.Columns.Add("Message", typeof(string));
                dataTable.Columns.Add("Status", typeof(byte));
                dataTable.Columns.Add("ServiceResponse", typeof(string));
                dataTable.Columns.Add("RequestGuid", System.Type.GetType("System.Guid"));
                dataTable.Columns.Add("TokenGuid", System.Type.GetType("System.Guid"));
                dataTable.Columns.Add("ScheduledTime", typeof(DateTime));

                if (requests != null && responses != null)
                {
                    requests.ForEach(item =>
                    {
                        Logger.Current.Informational("Request GUID :" + item.RequestGuid);
                        var response = responses.First(i => i.RequestGuid == item.RequestGuid);
                        dataTable.Rows.Add(new object[] { 1,item.From, item.To,1,item.Message,(byte)response.StatusID,response.ServiceResponse,
                        item.RequestGuid, item.TokenGuid,item.ScheduledTime});
                        dataTable.AcceptChanges();
                    });
                }

                var procedureName = "[dbo].[GET_SendTextQueue]";
                var parms = new List<SqlParameter>
                {   
                    new SqlParameter{ParameterName="@TextQueue", Value=dataTable, SqlDbType= SqlDbType.Structured, TypeName="[dbo].[TextQueue]" },                
                    new SqlParameter{ParameterName="@noOfRecords", Value=queueSize}                
                };
                return context.ExecuteStoredProcedure<SendSingleTextRequest>(procedureName, parms).ToList();
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured while executing stored procedure -> GET_SendTextQueue " + ex);
                return new List<SendSingleTextRequest>();
            }
        }

        public static Dictionary<string, Dictionary<string,string>> GetMergeFieldValues(this EfUnitOfWork context, int sentMailDetailId)
        {
            Dictionary<string, Dictionary<string, string>> mergeFields = new Dictionary<string, Dictionary<string, string>>();
            try
            {
                var parms = new List<SqlParameter>
                {
                    new SqlParameter{ParameterName="@SentMailDetailID", Value=sentMailDetailId}
                };
                var result = context.ExecuteStoredProcedure<SendMailMergeFieldsRequest>("dbo.GetSendMailMergefieldValues", parms).ToList();


                if (result != null && result.Count > 0)
                {
                    var distinctContacts = result.Select(s => s.Email).Distinct().ToList();
                    distinctContacts.ForEach(c =>
                    {
                        var mf = result.Where(r => r.Email == c).ToDictionary(k => k.FieldCode, v => v.FieldValue);
                        if (!mergeFields.ContainsKey(c))
                        {
                            mergeFields.Add(c, mf);
                        }
                    });
                }
            }
            catch(Exception ex)
            {
                ex.Data.Clear();
                ex.Data.Add("SendMailDetailId", sentMailDetailId);
                Logger.Current.Error("Error while getting sendmail merge field values", ex);
                throw;
            }
            return mergeFields;
        }
       
    }
}
