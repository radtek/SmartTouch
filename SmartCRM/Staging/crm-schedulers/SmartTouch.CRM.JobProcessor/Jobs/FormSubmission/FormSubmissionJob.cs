using System.Linq;
using Quartz;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Forms;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.JobProcessor.QuartzScheduler.Scheduler;

namespace SmartTouch.CRM.JobProcessor.Jobs.FormSubmission
{
    public class FormSubmissionJob : BaseJob
    {
        private readonly IFormService _formService;
        private readonly IFindSpamService _findSpamService;
        private readonly IFormRepository _formRepository;

        public FormSubmissionJob(
            IFormService formService,
            IFindSpamService findSpamService,
            IFormRepository formRepository)
        {
            _formService = formService;
            _findSpamService = findSpamService;
            _formRepository = formRepository;
        }

        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            Log.Informational("Entering into FormSubmission processor");

            var formSubmissions = _formService.GetFormSubmittedData();
            while (formSubmissions.SubmittedFormViewModel != null)
            {
                Log.Informational("Processing SubmittedFormDataID: " + formSubmissions.SubmittedFormViewModel.SubmittedFormDataID);
                var fields = formSubmissions.SubmittedFormViewModel.SubmittedFormFields.ToDictionary(x => x.Key, x => x.Value);
                string spamRemarks;
                var isSpam = _findSpamService.SpamCheck(fields, formSubmissions.SubmittedFormViewModel.AccountId, formSubmissions.SubmittedFormViewModel.IPAddress, formSubmissions.SubmittedFormViewModel.FormId, true, out spamRemarks);
                if (isSpam)
                {
                    Log.Informational("Spam Submission. SubmittedFormDataID:" + formSubmissions.SubmittedFormViewModel.SubmittedFormDataID);
                    _formRepository.UpdateFormSubmissionStatus(formSubmissions.SubmittedFormViewModel.SubmittedFormDataID, SubmittedFormStatus.Spam, spamRemarks, null);
                }
                else
                {
                    Log.Informational("Not a spam Submission. SubmittedFormDataID:" + formSubmissions.SubmittedFormViewModel.SubmittedFormDataID);
                    _formService.InsertFormSubmittedData(formSubmissions.SubmittedFormViewModel);

                }
                Log.Informational("Completed processing submission, " + formSubmissions.SubmittedFormViewModel.SubmittedFormDataID);
                formSubmissions = _formService.GetFormSubmittedData();
            }
        }
    }
}
