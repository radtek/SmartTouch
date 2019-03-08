using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;


namespace htmlreader.Controllers
{

    public class CampaignController : Controller
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        Campaigns Campaign = new Campaigns();
        CampaignLinks cmpLinks = new CampaignLinks();
        ClassicCampaigns classiccampaign = new ClassicCampaigns();
        Images images = new Images();
        SmartCRMLoadTestEntities db = new SmartCRMLoadTestEntities();

        // GET: Campaign
        public ActionResult Index()
        {
            var linkUrl = ConfigurationManager.AppSettings["LinkUrl"];
            var ImgUrl = ConfigurationManager.AppSettings["ImageURL"];
            ViewBag.LinkUrl = linkUrl;
            ViewBag.ImgURL = ImgUrl;
            return View();
        }

        public ActionResult GetCampaigns(int AccountID)
        {
            WebClient client = new WebClient();
            HtmlWeb web = new HtmlWeb();
            var Imagepath = ConfigurationManager.AppSettings["ImageLocation"] + AccountID + "\\ci\\";
            var fileLocation = ConfigurationManager.AppSettings["FileLog"];
            var fileLog = fileLocation + "ErrorList.txt";
          
            var classicCampaign = (from c in db.ClassicCampaigns where c.status == false & c.AccountID == AccountID select c).FirstOrDefault();
            int ImgCount = 0;
            try
            {
                if (!Directory.Exists(Imagepath))
                {
                    Directory.CreateDirectory(Imagepath);
                }

                if (classicCampaign != null)
                {
                    logger.Info(System.Environment.NewLine);
                    logger.Info("Classic campaign id : " + classicCampaign.ClassicCampaignID);
                    var CampaignURL = ConfigurationManager.AppSettings["CampaignUrl"];
                    string CampUrl = CampaignURL + classicCampaign.AID + "&cid=" + classicCampaign.cid;
                    var campaign = web.Load(CampUrl);
                    var html = campaign.DocumentNode.InnerHtml;
                    classicCampaign.InvalidImages = new List<string>();

                    if (html != null && !html.Contains("Campaign File not found"))
                    {
                        try
                        {
                            var ImgSrc = campaign.DocumentNode.SelectNodes("//img");
                            if (ImgSrc != null)
                            {
                                var url = ImgSrc.Select(s => s.Attributes["src"].Value).ToList();
                                    
                                for (int j = 0; j < url.Count; j++)
                                {
                                    string decodedUrl = System.Net.WebUtility.HtmlDecode(url[j]);
                                    Uri uri = new Uri(decodedUrl);
                                    var filename = System.IO.Path.GetFileName(uri.LocalPath);
                                    filename = filename.Replace(" ", "_");

                                    try
                                    {
                                        logger.Info("Classic campaign id : " + classicCampaign.ClassicCampaignID + " - Url Found: " + url[j]);
                                        if (!(System.IO.File.Exists((Imagepath + filename))))
                                        {
                                            logger.Info("Classic campaign id : " + classicCampaign.ClassicCampaignID + " - Attempting to download");
                                            client.DownloadFile(uri, Imagepath + filename);
                                            logger.Info("Classic campaign id : " + classicCampaign.ClassicCampaignID + " - Image downloaded");
                                            ImgCount = ImgCount+1;
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        classicCampaign.InvalidImages.Add(filename);
                                        logger.Error("Classic campaign id : " + classicCampaign.ClassicCampaignID + " - Unable to download: " + filename + " - Error: " + e.Message);                                       
                                        continue;
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error("Error Message : " + e.Message);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("Error Message : " + e.Message);
                var jsonResult = Json(new { success = false, response = e.Message }, JsonRequestBehavior.AllowGet);
                return jsonResult;
            }

            if (ImgCount > 0)
                logger.Info("Classic campaign id : " + classicCampaign.ClassicCampaignID + " - Image downloaded Count: " + ImgCount);                
            var jsonResult1 = Json(new { success = true, response = classicCampaign }, JsonRequestBehavior.AllowGet);
            jsonResult1.MaxJsonLength = int.MaxValue;
            return jsonResult1;
        }

        public static IList<KeyValuePair<int, string>> DownloadedImages { get; set; }

        public ActionResult UpdateCampaign(Campaigns campaigns, List<Images> images)
        {
            Campaign = (from c in db.Campaigns where c.CampaignID == campaigns.CampaignID select c).FirstOrDefault();

            if (images == null)
                images = new List<Images>();

            var DbImages = (from i in db.Images where i.AccountID == campaigns.AccountID select new { i.OriginalName }).ToList();
            var databaseImages = DbImages.Select(c => c.OriginalName).ToList();
            try
            {
                if (images != null)
                {
                    for (int i = 0; i < images.Count; i++)
                    {
                        var InsertImage = images[i].OriginalName;
                        var duplicate = databaseImages.Find(c => c == images[i].OriginalName);

                        if (duplicate == null)
                        {
                            databaseImages.Add(images[i].OriginalName);
                            images[i].CreatedBy = Campaign.CreatedBy;
                            db.Images.Add(images[i]);
                            db.SaveChanges();
                        }
                    }
                }

                var oldCampaignLinksDB = (from cl in db.CampaignLinks where cl.CampaignID == Campaign.CampaignID select cl).ToList();
                db.CampaignLinks.RemoveRange(oldCampaignLinksDB); 
                Campaign.HTMLContent = campaigns.HTMLContent;
                Campaign.CampaignLinks = campaigns.CampaignLinks;
                db.Entry<Campaigns>(Campaign).State = System.Data.Entity.EntityState.Modified;
               // db.SaveChanges();

                var classiccampaign = (from c in db.ClassicCampaigns where c.CampaignID == campaigns.CampaignID select c).FirstOrDefault();
                classiccampaign.status = true;
                db.Entry(classiccampaign).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception e)
            {
                logger.Error("Error Message : " + e.Message);
            }

            var jsonResult = Json(images, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
    }
}