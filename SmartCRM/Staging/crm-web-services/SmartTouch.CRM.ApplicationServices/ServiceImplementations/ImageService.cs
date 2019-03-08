using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.Image;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class ImageService : IImageService
    {
        public ImageService()
        {

        }

        public DownloadImageResponse DownloadImage(DownloadImageRequest request)
        {
            string imageUrl = string.Empty;
            string storagePath = string.Empty;
            var extension = Path.GetExtension(request.ImageInputUrl) == "" ? ".JPEG" : Path.GetExtension(request.ImageInputUrl);
            string storageName = Guid.NewGuid().ToString() + extension;
            string fileOriginalName = Path.GetFileName(request.ImageInputUrl);
            var imagePhysicalPath = ConfigurationManager.AppSettings["IMAGE_PHYSICAL_PATH"].ToString();

            DownloadImageResponse downloadResponse = new DownloadImageResponse();
            ImageViewModel imageViewModel = new ImageViewModel();

            //Checking for EditMode
            if (!System.IO.File.Exists(imagePhysicalPath + "/" + request.ImageInputUrl))
            {
                try
                {
                    WebRequest req = WebRequest.Create(request.ImageInputUrl);
                    req.Timeout = 10000;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                                        | SecurityProtocolType.Tls11
                                                        | SecurityProtocolType.Tls12
                                                        | SecurityProtocolType.Ssl3;

                    WebResponse response = req.GetResponse();

                    Stream stream = response.GetResponseStream();
                    Image Image = Image.FromStream(stream);
                    if (!System.IO.Directory.Exists(imagePhysicalPath + "/" + request.AccountId))
                        Directory.CreateDirectory(imagePhysicalPath + "/" + request.AccountId);
                    switch (request.ImgCategory)
                    {
                        case ImageCategory.ContactProfile:
                            storagePath = request.AccountId + "/" + "pi";
                            imageViewModel.FriendlyName = fileOriginalName;
                            break;
                        case ImageCategory.Campaigns:
                            storagePath = request.AccountId + "/" + "ci";
                            break;
                        default:
                            break;
                    }
                    imagePhysicalPath = imagePhysicalPath + "/" + storagePath;
                    if (!System.IO.Directory.Exists(imagePhysicalPath))
                        System.IO.Directory.CreateDirectory(imagePhysicalPath);
                    imagePhysicalPath = Path.Combine(imagePhysicalPath, storageName);
                    if (!System.IO.File.Exists(imagePhysicalPath))
                        Image.Save(imagePhysicalPath, ImageFormat.Jpeg);
                    imageUrl = storageName;
                    imageViewModel.ImageContent = imageUrl;
                    imageViewModel.OriginalName = fileOriginalName;
                    imageViewModel.ImageType = Path.GetExtension(request.ImageInputUrl) == "" ? ".JPEG" : Path.GetExtension(request.ImageInputUrl);
                    imageViewModel.StorageName = storageName;
                    imageViewModel.ImageCategoryID = request.ImgCategory;
                    imageViewModel.AccountID = Convert.ToInt16(request.AccountId);
                }
                catch (Exception ex)
                {
                    Logger.Current.Error("An error occured while downloading image from specified path : " + request.ImageInputUrl + " AccountID : " + request.AccountId, ex);
                }
            }

            else { imageUrl = request.ImageInputUrl; }
            downloadResponse.ImageViewModel = imageViewModel;
            return downloadResponse;
        }

        public SaveImageResponse SaveImage(SaveImageRequest request)
        {
            SaveImageResponse response = new SaveImageResponse();
            if (request != null)
            {
                ImageViewModel viewModel = request.ViewModel;
                if (viewModel.ImageContent != null)
                {
                    if (!string.IsNullOrEmpty(viewModel.StorageName) && viewModel.ImageContent.Contains(viewModel.StorageName))
                    {
                        response.ImageViewModel = viewModel;
                        return response;
                    }
                    if (viewModel.OriginalName == null)
                    {
                        response.ImageViewModel = null;
                        return response;
                    }
                    Image Image;
                    string storageName = string.Empty;
                    if (viewModel.ImageID != 0) { storageName = viewModel.StorageName; }
                    else { storageName = Guid.NewGuid().ToString() + viewModel.ImageType; }
                    string imagePath = CreateFolder(request.ImageCategory, request.AccountId.ToString());
                    imagePath = Path.Combine(imagePath, storageName);

                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                    if (!System.IO.File.Exists(imagePath))
                    {
                        if (viewModel.ImageContent.Split(',').Length == 2)
                        {
                            string imageContent = viewModel.ImageContent.Split(',')[1];
                            byte[] imageData = Convert.FromBase64String(imageContent);
                            using (MemoryStream ms = new MemoryStream(imageData))
                            {
                                Image = Image.FromStream(ms);
                                Image.Save(imagePath);
                            }
                            viewModel.ImageContent = null;
                            viewModel.StorageName = storageName;
                            viewModel.FriendlyName = viewModel.OriginalName;
                            viewModel.ImageCategoryID = request.ImageCategory;
                            viewModel.AccountID = Convert.ToInt16(request.AccountId);
                        }
                        else
                            viewModel = null;
                    }
                    response.ImageViewModel = viewModel;
                }
            }
            else
            {
                response.ImageViewModel = null;
            }
            return response;
        }

        string CreateFolder(ImageCategory ImageCategory, string accountId)
        {
            var imagePhysicalPath = ConfigurationManager.AppSettings["IMAGE_PHYSICAL_PATH"].ToString();
            imagePhysicalPath = Path.Combine(imagePhysicalPath, accountId);
            //Account Folder
            if (!System.IO.Directory.Exists(imagePhysicalPath))
            {
                System.IO.Directory.CreateDirectory(imagePhysicalPath);
            }
            switch (ImageCategory)
            {
                case (ImageCategory.ContactProfile):
                    imagePhysicalPath = Path.Combine(imagePhysicalPath, "pi");
                    break;
                case (ImageCategory.OpportunityProfile):
                    imagePhysicalPath = Path.Combine(imagePhysicalPath, "pi");
                    break;
                case ImageCategory.Campaigns:
                    imagePhysicalPath = Path.Combine(imagePhysicalPath, "ci");
                    break;
                case ImageCategory.AccountLogo:
                    imagePhysicalPath = Path.Combine(imagePhysicalPath, "ai");
                    break;
                default:
                    break;
            }
            //ImageCategory Folder
            if (!System.IO.Directory.Exists(imagePhysicalPath))
            {
                System.IO.Directory.CreateDirectory(imagePhysicalPath);
            }
            return imagePhysicalPath;
        }
    }
}
