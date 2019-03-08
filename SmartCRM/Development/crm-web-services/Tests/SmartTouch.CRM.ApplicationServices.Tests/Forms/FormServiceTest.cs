using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SmartTouch.CRM.ApplicationServices.Messaging.Forms;
using SmartTouch.CRM.ApplicationServices.ServiceImplementations;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Forms;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.CustomFields;
using SmartTouch.CRM.Domain.Dropdowns;
using SmartTouch.CRM.MessageQueues;
using SmartTouch.CRM.SearchEngine.Indexing;
using SmartTouch.CRM.SearchEngine.Search;
using SmartTouch.CRM.Domain.LeadScoreRules;
using SmartTouch.CRM.Domain.Communication;

namespace SmartTouch.CRM.ApplicationServices.Tests.Forms
{
    [TestClass]
    public class FormServiceTest
    {
        MockRepository mockRepository;
        IFormService formService;
        Mock<IFormRepository> mockFormRepository;
        Mock<IFormSubmissionRepository> mockFormSubmissionRepository;
        Mock<ITagRepository> mockTagRepository;
        IUnitOfWork mockUnitofWork;
        Mock<IContactRepository> mockContactRepository;
        Mock<ICustomFieldRepository> mockCustomFieldRepository;
        Mock<ICachingService> cachingService;
        Mock<IDropdownRepository> mockDropdownRepository;
        Mock<IContactService> contactService;
        Mock<ICustomFieldService> customFieldService;
        Mock<IUrlService> urlService;
        Mock<IIndexingService> indexingService;
        Mock<ISearchService<Form>> searchService;
        Mock<ISearchService<Contact>> contactSearchService;
        Mock<ILeadScoreRuleRepository> leadScoreRulesRepository;
        Mock<IMessageService> mockMessageService;
        Mock<IGeoService> mockGeoService;
        Mock<IAccountService> mockAccountService;
        Mock<IUserService> mockUserService;
        Mock<IServiceProviderRepository> mockServiceProvider;
        Mock<IFindSpamService> findSpamService; //NEXG-3014
        [TestInitialize]
        public void Initialize()
        {
            InitializeAutoMapper.Initialize();
            mockRepository = new MockRepository(MockBehavior.Default);
            mockTagRepository = mockRepository.Create<ITagRepository>();
            mockUnitofWork = mockRepository.Create<IUnitOfWork>().Object;
            mockFormRepository = mockRepository.Create<IFormRepository>();
            mockFormSubmissionRepository = mockRepository.Create<IFormSubmissionRepository>();
            mockContactRepository = mockRepository.Create<IContactRepository>();
            mockCustomFieldRepository = mockRepository.Create<ICustomFieldRepository>();
            cachingService = mockRepository.Create<ICachingService>();
            mockDropdownRepository = mockRepository.Create<IDropdownRepository>();
            contactService = mockRepository.Create<IContactService>();
            customFieldService = mockRepository.Create<ICustomFieldService>();
            urlService = mockRepository.Create<IUrlService>();
            indexingService = mockRepository.Create<IIndexingService>();
            searchService = mockRepository.Create<ISearchService<Form>>();
            contactSearchService = mockRepository.Create<ISearchService<Contact>>();
            mockMessageService = mockRepository.Create<IMessageService>();
            leadScoreRulesRepository = mockRepository.Create<ILeadScoreRuleRepository>();
            mockGeoService = mockRepository.Create<IGeoService>();
            mockAccountService = mockRepository.Create<IAccountService>();
            mockUserService = mockRepository.Create<IUserService>();
            mockServiceProvider = mockRepository.Create<IServiceProviderRepository>();
            findSpamService = mockRepository.Create<IFindSpamService>(); //NEXG-3014

            formService = new FormService(
                 mockFormRepository.Object,
                 mockFormSubmissionRepository.Object,
                 mockUnitofWork,
                 mockTagRepository.Object,
                 mockContactRepository.Object,
                 mockCustomFieldRepository.Object,
                 cachingService.Object,
                 mockDropdownRepository.Object, 
                 contactService.Object, 
                 customFieldService.Object,
                 urlService.Object, 
                 indexingService.Object,
                 searchService.Object,
                 contactSearchService.Object,
                 leadScoreRulesRepository.Object,
                 mockMessageService.Object,mockGeoService.Object,
                 mockAccountService.Object,
                 mockUserService.Object,
                 mockServiceProvider.Object
                 , findSpamService.Object //NEXG-3014
                 );
        }

        [TestCleanup]
        public void CleanUp()
        {
        }

        [TestMethod]
        public void InsertForm_ValidForm_Succeed()
       { 
            FormViewModel viewModel = FormMockData.GetFormViewModel("create");
            mockFormRepository.Setup(mnt => mnt.IsFormNameUnique(It.IsAny<Form>())).Returns(true);
            mockFormRepository.Setup(mnt => mnt.Insert(It.IsAny<Form>())).Verifiable();
            InsertFormRequest request = new InsertFormRequest() { FormViewModel = viewModel };
            InsertFormResponse response = formService.InsertForm(request);            
            mockRepository.VerifyAll();
        }


        [TestMethod]
        public void UpdateForm_ValidForm_Succeed()
        {
            FormViewModel viewModel = FormMockData.GetFormViewModel("update");
            mockFormRepository.Setup(mnt => mnt.IsFormNameUnique(It.IsAny<Form>())).Returns(true);
            mockFormRepository.Setup(mnt => mnt.Update(It.IsAny<Form>())).Verifiable();
            UpdateFormRequest request = new UpdateFormRequest() { FormViewModel = viewModel };
            UpdateFormResponse response = formService.UpdateForm(request);
            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void DeleteForm_ValidForm_Succeed()
        {
            int[] DeleteFormAray = FormMockData.GetDeleteFormRequest();            
            mockFormRepository.Setup(mnt => mnt.DeactivateForm(It.IsAny<int[]>())).Verifiable();
            DeleteFormRequest req = new DeleteFormRequest() { FormIDs = DeleteFormAray };
            DeleteFormResponse response = formService.DeleteForm(req);
            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void GetAllContactFields_ReturnAllContactFields_Succeed()
        {
            mockFormRepository.Setup(c => c.GetAllContactFields()).Verifiable();
            GetAllContactFieldsRequest request = new GetAllContactFieldsRequest();
            GetAllContactFieldsResponse response = formService.GetAllContactFields(request);
            mockRepository.VerifyAll();
            Assert.AreEqual(null, response.Exception);
        }

        [TestMethod]
        public void ProcessFormSubmission_SubmitValidForm_CreateANewContactIfNotFoundDuplicate()
        {
            mockFormSubmissionRepository.Setup(s => s.Insert(new FormSubmission())).Verifiable();
            mockContactRepository.Setup(c => c.Insert(new Person())).Verifiable();
            SubmitFormRequest request = new SubmitFormRequest() { SubmittedFormViewModel = FormMockData.GenerateSubmittedFormViewModel() };
            SubmitFormResponse response = formService.SubmitForm(request);
            mockFormRepository.VerifyAll();
            Assert.AreEqual(null, response.Exception); 
        }
    }
}
