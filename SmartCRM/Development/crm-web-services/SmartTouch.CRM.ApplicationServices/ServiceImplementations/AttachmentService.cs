using AutoMapper;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Exceptions;
using SmartTouch.CRM.ApplicationServices.Messaging.Communication;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class AttachmentService : IAttachmentService
    {
        readonly IAttachmentRepository attachmentRepository;
        readonly IServiceProviderRepository serviceProviderRepository;
        readonly IUserRepository userRepository;
        readonly IUnitOfWork unitOfWork;

        public AttachmentService(IAttachmentRepository attachmentRepository, IServiceProviderRepository serviceProviderRepository, IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            if (attachmentRepository == null) throw new ArgumentNullException("attachmentRepository");
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");
            this.attachmentRepository = attachmentRepository;
            this.serviceProviderRepository = serviceProviderRepository;
            this.userRepository = userRepository;
            this.unitOfWork = unitOfWork;
        }

        public bool SaveAttachment(SaveAttachmentRequest request)
        {
            foreach (var file in request.filesViewModel)
            {
                string storageFileName = System.Guid.NewGuid().ToString();

                int FileTypeID;
                string Extension = Path.GetExtension(file.Name);
                if (!string.IsNullOrEmpty(Extension))
                {
                    if (Extension.ToLower() == ".jpg" || Extension.ToLower() == ".jpeg" || Extension.ToLower() == ".png" || Extension.ToLower() == ".bmp") { FileTypeID = (int)FileType.image; }
                    else if (Extension.ToLower() == ".doc" || Extension.ToLower() == ".docx") { FileTypeID = (int)FileType.word; }
                    else if (Extension.ToLower() == ".xls" || Extension.ToLower() == ".xlsx") { FileTypeID = (int)FileType.excel; }
                    else if (Extension.ToLower() == ".txt") { FileTypeID = (int)FileType.txt; }
                    else if (Extension.ToLower() == ".pdf") { FileTypeID = (int)FileType.pdf; }
                    else if (Extension.ToLower() == ".csv") { FileTypeID = (int)FileType.csv; }
                    else if (Extension.ToLower() == ".rtf") { FileTypeID = (int)FileType.rtf; }
                    else { FileTypeID = (int)FileType.others; }
                }
                else { FileTypeID = (int)FileType.others; }

                AttachmentViewModel viewModel = new AttachmentViewModel()
                {
                    ContactID = request.ContactId,
                    DocumentTypeID = (int)DocumentType.General,
                    FileTypeID = FileTypeID,
                    CreatedBy = request.CreatedBy,
                    CreatedDate = DateTime.Now.ToUniversalTime(),
                    FilePath = file.Link,
                    OriginalFileName = file.Name,
                    StorageFileName = storageFileName,
                    OpportunityID = request.OpportunityID,
                    StorageSource = request.StorageSource
                };

                Attachment attachment = Mapper.Map<AttachmentViewModel, Attachment>(viewModel);
                attachmentRepository.Insert(attachment);
                unitOfWork.Commit();
            }
            return true;
        }

        public AttachmentResponse GeAttachment(AttachmentRequest request)
        {
            AttachmentResponse response = new AttachmentResponse();
             Attachment attachment = attachmentRepository.FindBy(Convert.ToInt16(request.AttachmentViewModel.DocumentID));
            if (attachment != null)
            {
                AttachmentViewModel attachmentViewModel = Mapper.Map<Attachment, AttachmentViewModel>(attachment);
                response.AttachmentViewModel = attachmentViewModel;
            }
            return response;
        }


        public AttachmentResponse DeleteAttachment(AttachmentRequest request)
        {
            attachmentRepository.DeleteAttachment(request.AttachmentViewModel.DocumentID);
            return new AttachmentResponse();
        }

        public GetAttachmentsResponse GetAllAttachments(GetAttachmentsRequest request)
        {
            GetAttachmentsResponse response = new GetAttachmentsResponse();
            IEnumerable<Attachment> attachments = attachmentRepository.FindAllAttachments(request.ContactId, request.OpportunityID, request.Page, request.Limit, request.PageNumber);
            AttachmentActivityAnalyzer analyser = new AttachmentActivityAnalyzer(attachments, request.DateFormat);
            response.Attachments = Mapper.Map<IEnumerable<Attachment>, IEnumerable<AttachmentViewModel>>(analyser.GenerateAnalysis());
            response.TotalRecords = attachmentRepository.TotalNumberOfAttachments(request.ContactId, request.OpportunityID, request.Page);
            return response;
        }

        private Exception GetAttachementNotFoundException()
        {
            Logger.Current.Error("Exception occurred while performing getting all attachements.");
            return new ResourceNotFoundException("The requested attachements was not found.");
        }
    }
}
