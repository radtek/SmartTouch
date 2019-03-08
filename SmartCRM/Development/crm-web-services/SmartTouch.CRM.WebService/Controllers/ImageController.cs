using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using System;
using System.Configuration;
using System.IO;
using System.Web.Http;

namespace SmartTouch.CRM.WebService.Controllers
{
    /// <summary>
    /// Creating image controller for Image module
    /// </summary>
    public class ImageController : SmartTouchApiController
    {
        readonly ICampaignService campaignService;
        /// <summary>
        ///  Creating constructor for image controller for accessing
        /// </summary>
        /// <param name="campaignService">campaignService</param>
        public ImageController(ICampaignService campaignService)
        {
            this.campaignService = campaignService;
        }


        /// <summary>
        /// Upload campaign images.
        /// </summary>
        /// <param name="request">Data of uploaded image.</param>
        /// <returns></returns>

        [Route("Image/UploadImage")]
        [HttpPost]
        public void UploadCampaignImages(ImageViewModel request)
        {
            var AccountID = "1";
            const string ExpectedImagePrefix = "data:image/jpeg;base64,";
            if (request != null)
            {
                System.Drawing.Image Image;
                string storageName = Guid.NewGuid().ToString() + "." + request.ImageType;
                string fileOriginalName = request.OriginalName;
                var imagePhysicalPath = ConfigurationManager.AppSettings["IMAGE_PHYSICAL_PATH"].ToString();
                ImageViewModel imageViewModel = new ImageViewModel();

                string RootFolder = CreateFolder(AccountID, ImageCategory.Campaigns);
                string imageContent = request.ImageContent.Substring(ExpectedImagePrefix.Length);
                byte[] imageData = Convert.FromBase64String(imageContent);
                using (MemoryStream ms = new MemoryStream(imageData))
                {
                    Image = System.Drawing.Image.FromStream(ms);
                    Image.Save(imagePhysicalPath);

                }
                imagePhysicalPath = Path.Combine(RootFolder, storageName);
                if (!System.IO.File.Exists(imagePhysicalPath))
                {
                    Image.Save(imagePhysicalPath);
                }
                string imageUrl = storageName;
                imageViewModel.ImageContent = imageUrl;
                imageViewModel.OriginalName = fileOriginalName;
                imageViewModel.ImageType = request.ImageType;
                imageViewModel.StorageName = storageName;
                imageViewModel.ImageCategoryID = ImageCategory.Campaigns;
            }

        }

        /// <summary>
        /// Geting Image Physical Path
        /// </summary>
        /// <param name="Account">Account Name</param>
        /// <param name="ImageCategory"> Image Category</param>
        /// <returns>Image Pysical Path </returns>
        private string CreateFolder(string Account, ImageCategory ImageCategory)
        {
            var imagePhysicalPath = ConfigurationManager.AppSettings["IMAGE_PHYSICAL_PATH"].ToString();
            imagePhysicalPath = Path.Combine(imagePhysicalPath, Account);
            //Account Folder
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
                default:
                    throw new NotImplementedException();
            }
            //ImageCategory Folder
            if (!System.IO.Directory.Exists(imagePhysicalPath))
            {
                System.IO.Directory.CreateDirectory(imagePhysicalPath);
            }
            return imagePhysicalPath;
        }

        /// <summary>
        /// Get image by Id.
        /// </summary>
        /// <param name="id">Id of a image.</param>
        /// <returns></returns>
        [Route("Image/Delete")]
        [HttpPost]
        public bool DeleteImage(int id)
        {
            DeleteCampaignImageRequest request = new DeleteCampaignImageRequest(id);
            campaignService.DeleteCampaignImage(request);
            return true;
        }


    }
}
