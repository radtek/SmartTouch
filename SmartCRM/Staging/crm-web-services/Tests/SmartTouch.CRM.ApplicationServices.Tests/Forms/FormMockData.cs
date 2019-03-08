using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Fields;
using SmartTouch.CRM.Domain.Tags;

namespace SmartTouch.CRM.ApplicationServices.Tests.Forms
{
    public class FormMockData
    {
        public static FormViewModel GetFormViewModel(string type)
        {
            IList<FieldViewModel> fields = new List<FieldViewModel>() { };
            IList<FormFieldViewModel> formFields = new List<FormFieldViewModel>() { };
            IList<TagViewModel> tag = new List<TagViewModel>() { new TagViewModel() { AccountID = 1, Count = 2, Description = "Tag name", TagID = 1001, TagName = "New Tag" } };

            FormViewModel formViewModel = new FormViewModel();
            formViewModel.AccountId = 1;
            formViewModel.Acknowledgement = "This is the acknowledgement message";
            formViewModel.AcknowledgementType = Entities.AcknowledgementType.Message;
            formViewModel.TagsList = tag;
            formViewModel.CreatedBy = 1;
            formViewModel.CreatedDate = DateTime.Now;
            formViewModel.Fields = fields;
            formViewModel.FormFields = formFields;
            if (type == "create")
            {
                formViewModel.FormId = 0;
            }
            else
            {
                formViewModel.FormId = 1;
            }
            formViewModel.HTMLContent = "<div><p>This is the welcome message for every one</p></div>";
            formViewModel.LastModifiedBy = 1;
            formViewModel.LastModifiedOn = DateTime.Now;
            formViewModel.Name = "Form name contains un even data for asdfar";
            formViewModel.Status = Entities.FormStatus.Active;
            formViewModel.Submissions = 10;
            return formViewModel;
        }

        public static int[] GetDeleteFormRequest()
        {
            int[] arr = new int[] { 1, 2 };
            return arr;
        }

        public static SubmittedFormViewModel GenerateSubmittedFormViewModel()
        {
            SubmittedFormViewModel newForm = new SubmittedFormViewModel();
            newForm.IPAddress = "192.168.1.1";
            newForm.AccountId = 1;
            newForm.SubmittedOn = new DateTime();
            newForm.SubmittedFormFields = new List<SubmittedFormFieldViewModel>();
            for (byte i = 1; i <= 2; i++)
            {
                newForm.SubmittedFormFields.Add(new SubmittedFormFieldViewModel() 
                { 
                    Key = i.ToString(), 
                    Value = i.ToString() 
                });
            }
            return newForm;
        }

    }
}
