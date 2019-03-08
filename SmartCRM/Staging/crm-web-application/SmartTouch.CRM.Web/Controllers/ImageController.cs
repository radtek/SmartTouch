using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using System.Net;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using System.Security.Claims;
using System.Threading;
using System.Text;
using System.Drawing;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using Kendo.Mvc.UI;

namespace SmartTouch.CRM.Web.Controllers
{
    public class ImageController : Controller
    {
        readonly ICampaignService campaignService;

        string accountId = "";

        public ImageController(ICampaignService campaignService)
        {
            this.campaignService = campaignService;
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            accountId = identity.Claims.Where(c => c.Type == "AccountID").Select(c => c.Value).SingleOrDefault();
        }

        //TODO:Once we will get the accountname from identity then 
        public ImageViewModel SaveImages(ImageViewModel viewModel, ImageCategory ImageCategory)
        {
            if (viewModel.ImageContent != null)
            {
                if (!string.IsNullOrEmpty(viewModel.StorageName) && viewModel.ImageContent.Contains(viewModel.StorageName))
                {
                    return null;
                }
                System.Drawing.Image Image;
                string storageName = string.Empty;
                if (viewModel.ImageID != 0)
                {
                    storageName = viewModel.StorageName;
                }
                else
                {
                    storageName = Guid.NewGuid().ToString() + viewModel.ImageType;
                }
                try
                {
                    string imagePath = CreateFolder(ImageCategory);
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
                                Image = System.Drawing.Image.FromStream(ms);
                                Image.Save(imagePath);
                            }
                            viewModel.ImageContent = null;
                            viewModel.StorageName = storageName;
                            viewModel.FriendlyName = viewModel.OriginalName;
                            viewModel.ImageCategoryID = ImageCategory;
                            viewModel.AccountID = Convert.ToInt16(accountId);
                        }
                        else
                            viewModel = null;
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                viewModel = null;
            }
            return viewModel;
        }

        public static string GetImage(string ImagePath)
        {
            return ConfigurationManager.AppSettings["ImagesHostedPath"].ToString() + ImagePath;
        }

        [HttpDelete, ActionName("DeleteImage")]
        public JsonResult DeleteImage(string ImagePath)
        {
            bool result = true;
            try
            {
                if (System.IO.File.Exists(ImagePath))
                {
                    System.IO.File.Delete(ImagePath);
                }
            }
            catch (Exception)
            {
                result = false;
                throw;
            }
            return Json(new
            {
                valid = true,
                success = result,
                JsonRequestBehavior.AllowGet
            });
        }

        public ImageViewModel DownloadImage(string ImageInputUrl, ImageCategory ImgCategory)
        {
            string imageUrl = string.Empty;
            string storagePath = string.Empty;
            string storageName = Guid.NewGuid().ToString() + Path.GetExtension(ImageInputUrl);
            string fileOriginalName = Path.GetFileName(ImageInputUrl);
            var imagePhysicalPath = ConfigurationManager.AppSettings["IMAGE_PHYSICAL_PATH"].ToString();
            ImageViewModel imageViewModel = new ImageViewModel();
            if (!System.IO.File.Exists(imagePhysicalPath + "/" + ImageInputUrl))
            {
                WebRequest req = WebRequest.Create(ImageInputUrl);
                req.Timeout = 10000;
                WebResponse response = req.GetResponse();
                Stream stream = response.GetResponseStream();
                System.Drawing.Image Image = System.Drawing.Image.FromStream(stream);
                if (!System.IO.Directory.Exists(imagePhysicalPath + "/" + accountId))
                    Directory.CreateDirectory(imagePhysicalPath + "/" + accountId);
                switch (ImgCategory)
                {
                    case ImageCategory.ContactProfile:
                        storagePath = accountId + "/" + "pi";
                        imageViewModel.FriendlyName = fileOriginalName;
                        break;
                    case ImageCategory.Campaigns:
                        storagePath = accountId + "/" + "ci";
                        break;
                    default:
                        storagePath = string.Empty;
                        break;
                }
                imagePhysicalPath = imagePhysicalPath + "/" + storagePath;
                if (!System.IO.Directory.Exists(imagePhysicalPath))
                    System.IO.Directory.CreateDirectory(imagePhysicalPath);
                imagePhysicalPath = Path.Combine(imagePhysicalPath, storageName);
                if (!System.IO.File.Exists(imagePhysicalPath))
                    Image.Save(imagePhysicalPath);
                imageUrl = storageName;
                imageViewModel.ImageContent = imageUrl;
                imageViewModel.OriginalName = fileOriginalName;
                imageViewModel.ImageType = Path.GetExtension(ImageInputUrl);
                imageViewModel.StorageName = storageName;
                imageViewModel.ImageCategoryID = ImgCategory;
                imageViewModel.AccountID = Convert.ToInt16(accountId);
            }
            return imageViewModel;
        }

        //TODO:Build the common code for Folder creation(Inprogress).Need to remove unnecessary above lines in above code
        private string CreateFolder(ImageCategory ImageCategory)
        {
            var imagePhysicalPath = ConfigurationManager.AppSettings["IMAGE_PHYSICAL_PATH"].ToString();
            imagePhysicalPath = Path.Combine(imagePhysicalPath, accountId);
            if (!System.IO.Directory.Exists(imagePhysicalPath))
            {
                System.IO.Directory.CreateDirectory(imagePhysicalPath);
            }
            switch (ImageCategory)
            {
                case ImageCategory.ContactProfile:
                    imagePhysicalPath = Path.Combine(imagePhysicalPath, "pi");
                    break;
                case ImageCategory.Campaigns:
                    imagePhysicalPath = Path.Combine(imagePhysicalPath, "ci");
                    break;
                case ImageCategory.AccountLogo:
                    imagePhysicalPath = Path.Combine(imagePhysicalPath, "ai");
                    break;
                default:
                    imagePhysicalPath = string.Empty;
                    break;
            }
            if (!System.IO.Directory.Exists(imagePhysicalPath))
            {
                System.IO.Directory.CreateDirectory(imagePhysicalPath);
            }
            return imagePhysicalPath;
        }

        [HttpPost]
        public JsonResult UploadCampaignImages(IEnumerable<HttpPostedFileBase> files, string FriendlyName)
        {
            int imageId = 0;
            bool result = true;
            string message = string.Empty;
            string imageHostedUrl = string.Empty;
            if (!string.IsNullOrEmpty(FriendlyName))
            {
                InsertCampaignImageRequest request = new InsertCampaignImageRequest();
                InsertCampaignImageResponse response = new InsertCampaignImageResponse();
                ImageViewModel imageViewModel = new ImageViewModel();
                string RootFolder = string.Empty;
                string storageName = string.Empty;
                string imagePhysicalPath = string.Empty;
                ViewBag.Image = null;
                try
                {
                    if (files != null)
                    {
                        foreach (var file in files)
                        {
                            imageHostedUrl = ConfigurationManager.AppSettings["IMAGE_HOSTING_SERVICE_URL"].ToString();
                            storageName = Guid.NewGuid().ToString() + "." + file.FileName.Split('.')[1];
                            byte[] fileData = null;
                            System.Drawing.Image Image;
                            RootFolder = CreateFolder(ImageCategory.Campaigns);
                            using (var binaryReader = new BinaryReader(file.InputStream))
                            {
                                fileData = binaryReader.ReadBytes(file.ContentLength);
                            }
                            imagePhysicalPath = Path.Combine(RootFolder, storageName);
                            using (MemoryStream ms = new MemoryStream(fileData))
                            {
                                Image = System.Drawing.Image.FromStream(ms);
                                Image.Save(imagePhysicalPath);
                            }
                            if (!System.IO.File.Exists(imagePhysicalPath))
                            {
                                Image.Save(imagePhysicalPath);
                            }
                            imageViewModel.AccountID = Convert.ToInt16(accountId);
                            imageViewModel.FriendlyName = FriendlyName;
                            imageViewModel.ImageContent = string.Empty;
                            imageViewModel.OriginalName = file.FileName;
                            imageViewModel.ImageType = file.ContentType;
                            imageViewModel.StorageName = storageName;
                            imageViewModel.ImageCategoryID = ImageCategory.Campaigns;
                            request.ImageViewModel = imageViewModel;
                            response = campaignService.InsertCampaignImage(request);
                            if (response != null)
                            {
                                if (response.ImageViewModel != null)
                                {
                                    imageId = response.ImageViewModel.ImageID;
                                    imageHostedUrl = Path.Combine(imageHostedUrl + "/" + accountId + "/ci" + "/" + storageName);
                                }
                                else if (response.Exception != null)
                                {
                                    message = response.Exception.Message;
                                    result = false;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                    result = false;
                }
            }
            return Json(new
            {
                success = result,
                message = message,
                response = imageHostedUrl,
                Id = imageId
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UploadImages(IEnumerable<HttpPostedFileBase> files, string FriendlyName)
        {
            int imageId = 0;
            bool result = true;
            string message = string.Empty;
            string imageHostedUrl = string.Empty;
            if (!string.IsNullOrEmpty(FriendlyName))
            {
                InsertCampaignImageRequest request = new InsertCampaignImageRequest();
                InsertCampaignImageResponse response = new InsertCampaignImageResponse();
                ImageViewModel imageViewModel = new ImageViewModel();
                string RootFolder = string.Empty;
                string storageName = string.Empty;
                string imagePhysicalPath = string.Empty;
                ViewBag.Image = null;
                try
                {
                    if (files != null)
                    {
                        foreach (var file in files)
                        {
                            imageHostedUrl = ConfigurationManager.AppSettings["IMAGE_HOSTING_SERVICE_URL"].ToString();
                            storageName = Guid.NewGuid().ToString() + "." + file.FileName.Split('.')[1];
                            byte[] fileData = null;
                            System.Drawing.Image Image;
                            RootFolder = CreateFolder(ImageCategory.Campaigns);
                            using (var binaryReader = new BinaryReader(file.InputStream))
                            {
                                fileData = binaryReader.ReadBytes(file.ContentLength);
                            }
                            imagePhysicalPath = Path.Combine(RootFolder, storageName);
                            using (MemoryStream ms = new MemoryStream(fileData))
                            {
                                Image = System.Drawing.Image.FromStream(ms);
                                Image.Save(imagePhysicalPath);
                            }
                            if (!System.IO.File.Exists(imagePhysicalPath))
                            {
                                Image.Save(imagePhysicalPath);
                            }
                            imageViewModel.AccountID = Convert.ToInt16(accountId);
                            imageViewModel.FriendlyName = FriendlyName;
                            imageViewModel.ImageContent = string.Empty;
                            imageViewModel.OriginalName = file.FileName;
                            imageViewModel.ImageType = file.ContentType;
                            imageViewModel.StorageName = storageName;
                            imageViewModel.ImageCategoryID = ImageCategory.Campaigns;
                            request.ImageViewModel = imageViewModel;
                            response = campaignService.InsertCampaignImage(request);
                            if (response != null)
                            {
                                if (response.ImageViewModel != null)
                                {
                                    imageId = response.ImageViewModel.ImageID;
                                    imageHostedUrl = Path.Combine(imageHostedUrl + "/" + accountId + "/ci" + "/" + storageName);
                                }
                                else if (response.Exception != null)
                                {
                                    message = response.Exception.Message;
                                    result = false;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return new JsonResult()
                    {
                        Data = new
                        {
                            success = false,
                            message = ex.Message
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                return Json(new
                {
                    success = result,
                    message = message,
                    response = imageHostedUrl,
                    Id = imageId
                }, JsonRequestBehavior.AllowGet);
            }
            else
                return new JsonResult()
                {
                    Data = new
                    {
                        success = false,
                        error = "Please specify friendly name for Image",
                        message = "Please specify friendly name for Image"
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
        }
    }
}
