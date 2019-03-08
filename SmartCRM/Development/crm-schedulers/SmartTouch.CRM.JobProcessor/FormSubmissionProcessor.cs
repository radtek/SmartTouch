using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.Forms;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Forms;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.JobProcessor
{
    public class FormSubmissionProcessor : CronJobProcessor
    {
        readonly IFormService formService;
        readonly IFindSpamService findSpamService;
        readonly IFormRepository formRepository;
        private static bool isFormProcessing;
        public FormSubmissionProcessor(CronJobDb cronJob, JobService jobService, string formSubmissionProcessorCacheName)
            : base(cronJob, jobService, formSubmissionProcessorCacheName)
          {
            this.formService = IoC.Container.GetInstance<IFormService>();
            this.findSpamService = IoC.Container.GetInstance<IFindSpamService>();
            this.formRepository = IoC.Container.GetInstance<IFormRepository>();
          }

        protected override void Execute()
        {
            Logger.Current.Informational("Entering into FormSubmission processor");
            try
            {
                if (!isFormProcessing)
                {
                    isFormProcessing = true;
                    string spamRemarks = string.Empty;
                    GetFormSubmissionDataResponse responce = new GetFormSubmissionDataResponse();
                    responce = formService.GetFormSubmittedData();
                    while (responce != null && responce.SubmittedFormViewModel != null)
                    {
                        Logger.Current.Verbose("Processing SubmittedFormDataID: " + responce.SubmittedFormViewModel.SubmittedFormDataID);
                        IDictionary<string, string> fields = responce.SubmittedFormViewModel.SubmittedFormFields.ToDictionary(x => x.Key, x => x.Value);
                        bool IsSpam = findSpamService.SpamCheck(fields, responce.SubmittedFormViewModel.AccountId, responce.SubmittedFormViewModel.IPAddress,responce.SubmittedFormViewModel.FormId ,true,out spamRemarks);
                        if (IsSpam && spamRemarks == "Invalid MobilePhone")
                        {
                            IsSpam = false;
                            spamRemarks = "";
                        }

                        if (IsSpam)
                        {
                            Logger.Current.Informational("Spam Submission. SubmittedFormDataID:" + responce.SubmittedFormViewModel.SubmittedFormDataID);
                            formRepository.UpdateFormSubmissionStatus(responce.SubmittedFormViewModel.SubmittedFormDataID, SubmittedFormStatus.Spam, spamRemarks, null);
                        }
                        else
                        {
                            Logger.Current.Informational("Not a spam Submission. SubmittedFormDataID:" + responce.SubmittedFormViewModel.SubmittedFormDataID);
                            formService.InsertFormSubmittedData(responce.SubmittedFormViewModel);

                        }
                        Logger.Current.Informational("Completed processing submission, " + responce.SubmittedFormViewModel.SubmittedFormDataID);
                        responce = formService.GetFormSubmittedData();
                    }
                    isFormProcessing = false;
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Error while FormSubmission" , ex);
                isFormProcessing = false;
            }
            finally {
                isFormProcessing = false;
            }
        }
    }
}
