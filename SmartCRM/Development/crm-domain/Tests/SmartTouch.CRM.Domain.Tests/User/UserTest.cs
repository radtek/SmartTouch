using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DU = SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.ValueObjects;
using Moq;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Domain.Tests.Tour;


namespace SmartTouch.CRM.Domain.Tests.User
{
    [TestClass]
    public class UserTest
    {

        DU.User validUser = new DU.User()
        {
            FirstName = "First Name",
            LastName = "Last Name",
            Addresses = Enumerable.Empty<Address>(),
            UserName = "User Name",
            Email = null
        };



        #region Users
        /* this is for testing the required first name for the users */
        [TestMethod]
        public void GetBrokenRules_FirstNameShouldNotBeNull_Failure()
        {

            validUser.FirstName = null;
            var brokenRulesCount = validUser.GetBrokenRules().Count();
            Assert.AreEqual(1,brokenRulesCount);
        }


        /* this is for testing the required last name for the users */
        [TestMethod]
        public void GetBrokenRules_LastNameShouldNotBeNull_Failure()
        {
            validUser.LastName = null;
            var brokenRulesCount = validUser.GetBrokenRules().Count();
            Assert.AreEqual(1, brokenRulesCount);
        }


        /* this is for testing the not required home phone for the users */
        [TestMethod]
        public void GetBrokenRules_HomePhoneCustomValidation_Failure()
        {
            validUser.HomePhone = new Phone() { Number = "12345" };
            var brokenRulesCount = validUser.GetBrokenRules().Count();
            Assert.AreEqual(1, brokenRulesCount);
        }


      


        /* this is for testing the not required mobile phone for the users */
        [TestMethod]
        public void GetBrokenRules_MobilePhoneCustomValidation_Failure()
        {
            validUser.MobilePhone = new Phone() { Number = "12345" };
            var brokenRulesCount = validUser.GetBrokenRules().Count();
            Assert.AreEqual(1, brokenRulesCount);
        }



        /* this is for testing the not required work phone for the users */
        [TestMethod]
        public void GetBrokenRules_WorkPhoneCustomValidation_Failure()
        {
            validUser.WorkPhone = new Phone() { Number = "12345" };
            var brokenRulesCount = validUser.GetBrokenRules().Count();
            Assert.AreEqual(1, brokenRulesCount);
        }


        /* this is for testing the email is not valid  */
        [TestMethod]
        public void GetBrokenRules_EmailNotValid_Success()
        {          
            var emailValid = validUser.IsValidEmail("srigmail.com");
            Assert.IsFalse(emailValid);
        }


        /* this is for testing the email is valid  */
        [TestMethod]
        public void GetBrokenRules_EmailValid_Success()
        {        
            var emailValid = validUser.IsValidEmail("sri@gmail.com");
            Assert.IsTrue(emailValid);
        }


        /* this is for testing the address is valid  */
        [TestMethod]
        public void GetBrokenRules_AddressNotValid_Success()
        {
            validUser.Addresses = new List<Address>() {
                new Address(){
                    Country = new Country { Name = "" },
                    State = new State { Name = "" } 
                }            
            };

            var brokenRulesCount = validUser.GetBrokenRules().Count();
            Assert.AreEqual(2, brokenRulesCount);
        }


        /* this is for testing the address is valid  */
        [TestMethod]
        public void GetBrokenRules_USZipCodeNotValid_Success()
        {
            validUser.Addresses = new List<Address>() {
                new Address(){
                    Country = new Country { Name = "", Code="US" },
                    State = new State { Name = "", Code="US" } ,
                    ZipCode = "524rret"
                }            
            };

            var brokenRulesCount = validUser.GetBrokenRules().Count();
            Assert.AreEqual(1, brokenRulesCount);
        }

        /* this is for testing the address is valid  */
        [TestMethod]
        public void GetBrokenRules_USZipCodeValid_Success()
        {
            validUser.Addresses = new List<Address>() {
                new Address(){
                    Country = new Country { Name = "", Code="US" },
                    State = new State { Name = "", Code="US" } ,
                    ZipCode = "52456"
                }            
            };

            var brokenRulesCount = validUser.GetBrokenRules().Count();
            Assert.AreEqual(0, brokenRulesCount);
        }


        /* this is for testing the address is valid  */
        [TestMethod]
        public void GetBrokenRules_CAZipCodeNotValid_Success()
        {
            validUser.Addresses = new List<Address>() {
                new Address(){
                    Country = new Country { Name = "", Code="CA" },
                    State = new State { Name = "", Code="CA" } ,
                    ZipCode = "524rret"
                }            
            };

            var brokenRulesCount = validUser.GetBrokenRules().Count();
            Assert.AreEqual(1, brokenRulesCount);
        }

        /* this is for testing the address is valid  */
        [TestMethod]
        public void GetBrokenRules_CAZipCodeValid_Success()
        {
            validUser.Addresses = new List<Address>() {
                new Address(){
                    Country = new Country { Name = "", Code="CA" },
                    State = new State { Name = "", Code="CA" } ,
                    ZipCode = "T0E 1S2"
                }            
            };

            var brokenRulesCount = validUser.GetBrokenRules().Count();
            Assert.AreEqual(0, brokenRulesCount);
        }



        /* this is for testing the email ids are valid */
        [TestMethod]
        public void GetBrokenRules_SecondaryMailsValid_Success()
        {
            validUser.SecondaryEmails = new List<Email>() {
                new Email(){
                    EmailId = "srinivas@gmail.com"
                },
                new Email(){
                 EmailId  = "sr@lmit.co.in"
                }
            };

            var brokenRulesCount = validUser.GetBrokenRules().Count();           
            var compare = brokenRulesCount == 0 ? false : true;
            Assert.IsFalse(compare);
        }


        /* this is for testing the email ids is not valid */
        [TestMethod]
        public void GetBrokenRules_SecondaryMailsValid_Failure()
        {
            validUser.SecondaryEmails = new List<Email>() {
                new Email(){
                    EmailId = "srinivasgmail.com"
                },
                new Email(){
                 EmailId  = "sr@lmit.co.in.gafa.arar"
                },
                 new Email(){
                 EmailId  = "asrlmit.co.in.gafa.arar"
                }
            };

            var brokenRulesCount = validUser.GetBrokenRules().Count();
            var compare = brokenRulesCount == 0 ? false : true;
            Assert.IsTrue(compare);
        }



        /* this is for testing the url provided for facebook is correct */
        [TestMethod]
        public void GetBrokenRules_ValidFaceBookURL_Success()
        {
            validUser.FacebookUrl = new Url { URL = "www.facebook.com/srinivas" };

            var brokenRulesCount = validUser.GetBrokenRules().Count();            
            Assert.AreEqual(0,brokenRulesCount);
        }



        /* this is for testing the url provided for facebook is not correct */
        [TestMethod]
        public void GetBrokenRules_ValidFaceBookURL_Failure()
        {
            validUser.FacebookUrl = new Url { URL = "www.fb.com/srinivas" };

            var brokenRulesCount = validUser.GetBrokenRules().Count();
            Assert.AreEqual(1, brokenRulesCount);
        }


        /* this is for testing the url provided for twitter is correct */
        [TestMethod]
        public void GetBrokenRules_ValidTwitterURL_Success()
        {
            validUser.TwitterUrl = new Url { URL = "www.twitter.com/srinivas" };

            var brokenRulesCount = validUser.GetBrokenRules().Count();
            Assert.AreEqual(0, brokenRulesCount);
        }



        /* this is for testing the url provided for Twitter is not correct */
        [TestMethod]
        public void GetBrokenRules_ValidTwitterURL_Failure()
        {
            validUser.TwitterUrl = new Url { URL = "www.tt.com/srinivas" };

            var brokenRulesCount = validUser.GetBrokenRules().Count();
            Assert.AreEqual(1, brokenRulesCount);
        }




        /* this is for testing the url provided for GooglePlusUrl is correct */
        [TestMethod]
        public void GetBrokenRules_ValidGooglePlusURL_Success()
        {
            validUser.GooglePlusUrl = new Url { URL = "www.plus.google.com/srinivas" };

            var brokenRulesCount = validUser.GetBrokenRules().Count();
            Assert.AreEqual(0, brokenRulesCount);
        }



        /* this is for testing the url provided for GooglePlusUrl is not correct */
        [TestMethod]
        public void GetBrokenRules_ValidGooglePlusURL_Failure()
        {
            validUser.GooglePlusUrl = new Url { URL = "www.+goog.com/srinivas" };

            var brokenRulesCount = validUser.GetBrokenRules().Count();
            Assert.AreEqual(1, brokenRulesCount);
        }



        /* this is for testing the url provided for LinkedIn is correct */
        [TestMethod]
        public void GetBrokenRules_ValidLinkedInURL_Success()
        {
            validUser.LinkedInUrl = new Url { URL = "www.linkedin.com/srinivas" };

            var brokenRulesCount = validUser.GetBrokenRules().Count();
            Assert.AreEqual(0, brokenRulesCount);
        }



        /* this is for testing the url provided for LinkedIn is not correct */
        [TestMethod]
        public void GetBrokenRules_ValidLinkedInURL_Failure()
        {
            validUser.LinkedInUrl = new Url { URL = "www.notlinked.com/srinivas" };

            var brokenRulesCount = validUser.GetBrokenRules().Count();
            Assert.AreEqual(1, brokenRulesCount);
        }





        /* this is for testing the url provided for blog url is correct */
        [TestMethod]
        public void GetBrokenRules_ValidBlogURL_Success()
        {
            validUser.BlogUrl = new Url { URL = "www.blog.com/srinivas" };

            var brokenRulesCount = validUser.GetBrokenRules().Count();
            Assert.AreEqual(0, brokenRulesCount);
        }



        /* this is for testing the url provided for blog url is not correct */
        [TestMethod]
        public void GetBrokenRules_ValidBlogURL_Failure()
        {
            validUser.BlogUrl = new Url { URL = "wwwaracom@gaar/srinivas" };

            var brokenRulesCount = validUser.GetBrokenRules().Count();
            Assert.AreEqual(1, brokenRulesCount);
        }




        /* this is for testing the url provided for blog url is correct */
        [TestMethod]
        public void GetBrokenRules_ValidWebSiteURL_Success()
        {
            validUser.WebsiteUrl = new Url { URL = "www.blog.com/srinivas" };

            var brokenRulesCount = validUser.GetBrokenRules().Count();
            Assert.AreEqual(0, brokenRulesCount);
        }



        /* this is for testing the url provided for blog url is not correct */
        [TestMethod]
        public void GetBrokenRules_ValidWebSiteURL_Failure()
        {
            validUser.WebsiteUrl = new Url { URL = "wwwaracom@gaar/srinivas" };

            var brokenRulesCount = validUser.GetBrokenRules().Count();
            Assert.AreEqual(1, brokenRulesCount);
        }


        #endregion
    }
}
