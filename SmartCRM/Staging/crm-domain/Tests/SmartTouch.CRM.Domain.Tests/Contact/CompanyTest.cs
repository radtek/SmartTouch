using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.ValueObjects;
using System.Linq;
using System.Collections.Generic;

namespace SmartTouch.CRM.Domain.Tests
{
    [TestClass]
    public class CompanyTest
    {
        Company company1 = new Company();
        Company company2 = new Company();
        Company company3 = new Company();
        Company company4 = new Company();
        Company company5 = new Company();
        Company company6 = new Company();
        Company company7 = new Company();
        Company company8 = new Company();
        Company company9 = new Company();
        Company company10 = new Company();

        [TestInitialize]
        public void Initialize()
        {
            List<Address> addresses = new List<Address>();
            for (int i = 1; i <= 10; i++)
            {
                Address address = new Address();
                address.AddressLine1 = "Add 1" + i.ToString();
                address.AddressLine2 = "Add 2" + i.ToString();
                address.City = "City " + i.ToString();
                address.State = new State { Code = "US-AK" };
                address.Country= new Country { Code = "US" };
                addresses.Add(address);
            };

            company1.Addresses = addresses;
        }



        [TestMethod]
        public void Company_Is_Valid_Company_Return_True_If_Pass()
        {
        }

        [TestMethod]
        public void Company_PhoneNumber_Length_Greaterthan_Or_EqualTo_10_Return_True_If_Pass()
        {
            Assert.AreEqual(true, company1.IsValidPhoneNumberLength("2345678900"));
            //Assert.AreEqual(true, company1.IsValidPhoneNumberLength("0019885021320"));
            Assert.AreEqual(true, company1.IsValidPhoneNumberLength("(212)897-0067"));
            Assert.AreEqual(true, company1.IsValidPhoneNumberLength("1(212)897-0067"));
            //Assert.AreEqual(true, company1.IsValidPhoneNumberLength("001(212)897-0067"));
            //Assert.AreEqual(true, company1.IsValidPhoneNumberLength("001.212.897.0067"));
            //Assert.AreEqual(true, company1.IsValidPhoneNumberLength("001(212)897.00671234"));
            //Assert.AreEqual(true, company1.IsValidPhoneNumberLength("001 212 897 0067"));
        }

        [TestMethod]
        public void Company_PhoneNumber_Length_Greaterthan_Or_EqualTo_10__Return_False_If_Fail()
        {
            Assert.AreEqual(false, company1.IsValidPhoneNumberLength("(234)567-891"));
            Assert.AreEqual(false, company1.IsValidPhoneNumberLength("001(234)-567-891"));
            Assert.AreEqual(false, company1.IsValidPhoneNumberLength("10408123456"));
            Assert.AreEqual(false, company1.IsValidPhoneNumberLength("100000000000000000001"));
            Assert.AreEqual(false, company1.IsValidPhoneNumberLength("200000000"));
            Assert.AreEqual(false, company1.IsValidPhoneNumberLength("040404004"));
            Assert.AreEqual(false, company1.IsValidPhoneNumberLength("DFUASDFU0E11234567890"));
        }

        [TestMethod]
        public void Company_PhoneNumber_Has_Alpha_And_Specials_If_Extracted_Number_IsValid_Succeed()
        {
            Assert.AreNotEqual(true, company1.IsValidPhoneNumberLength("0111409885021320SmartTouch"));
            Assert.AreNotEqual(true, company1.IsValidPhoneNumberLength("DFUASDFU8E83214164637"));
        }

        [TestMethod]
        public void Company_PhoneNumber_Has_Alpha_And_Specials_If_Extracted_Number_IsInvalid_Fail()
        {
            Assert.AreEqual(false, company1.IsValidPhoneNumberLength("DFUASDFDASFSD"));
            Assert.AreEqual(false, company1.IsValidPhoneNumberLength("98850DFASF152"));
        }

        [TestMethod]
        public void Company_FacebookURL_IsValid_Return_True_If_Pass()
        {
            company1.FacebookUrl = new Url() { URL = "facebook.com/adamsandler" };
            company2.FacebookUrl = new Url() { URL = "www.facebook.com/adamsandler" };
            company3.FacebookUrl = new Url() { URL = "https://facebook.com/adamsandler" };
            company4.FacebookUrl = new Url() { URL = "http://facebook.com/adamsandler" };
            company5.FacebookUrl = new Url() { URL = "https://www.facebook.com/adamsandler" };
            company6.FacebookUrl = new Url() { URL = "h://facebook.com/adamsandler" };
            company7.FacebookUrl = new Url() { URL = "s://www.facebook.com/adamsandler" };
            Assert.IsTrue(company1.IsFacebookURLValid(company1.FacebookUrl));
            Assert.IsTrue(company2.IsFacebookURLValid(company2.FacebookUrl));
            Assert.IsTrue(company3.IsFacebookURLValid(company3.FacebookUrl));
            Assert.IsTrue(company4.IsFacebookURLValid(company4.FacebookUrl));
            Assert.IsTrue(company5.IsFacebookURLValid(company5.FacebookUrl));
        }

        [TestMethod]
        public void Company_FacebookURL_IsValid_Return_False_If_Fail()
        {
            company1.FacebookUrl = new Url() { URL = "facebok.com/adamsandler" };
            company2.FacebookUrl = new Url() { URL = ".faebook.com/adamsandler" };
            company3.FacebookUrl = new Url() { URL = "https://facebook.co.in/adamsandler" };
            
            Assert.IsFalse(company1.IsFacebookURLValid(company1.FacebookUrl));
            Assert.IsFalse(company2.IsFacebookURLValid(company2.FacebookUrl));
            Assert.IsFalse(company3.IsFacebookURLValid(company3.FacebookUrl));
            Assert.IsFalse(company4.IsFacebookURLValid(company4.FacebookUrl));
            Assert.IsFalse(company5.IsFacebookURLValid(company5.FacebookUrl));
        }
        
        //[TestMethod]
        //public void Company_FacebookURL_IsValid_Return_False_If_Fail()
        //{
        //    Assert.IsFalse(company1.IsFacebookURLValid("http:"));
        //}

        //[TestMethod]
        //public void Company_LinkedInURL_IsValid_Return_True_If_Pass()
        //{
        //    Assert.IsTrue(company1.IsLinkedInURLValid("http://www.linkedin.com/pub/alan-daniel/7/31/8b7"));
        //}

        //[TestMethod]
        //public void Company_LinkedInURL_IsValid_Return_False_If_Fail()
        //{
        //    Assert.IsFalse(company1.IsLinkedInURLValid("http://www.linked.com/pub/alan-daniel/7/31/8b7"));
        //}

        //[TestMethod]
        //public void Company_GooglePlusURL_IsValid_Return_True_If_Pass()
        //{
        //    Assert.IsTrue(company1.IsGooglePlusURLValid("http://www.plus.google.com/pub/alan-daniel/7/31/8b7"));
        //}

        //[TestMethod]
        //public void Company_GooglePlusURL_IsValid_Return_False_If_Fail()
        //{
        //    Assert.IsFalse(company1.IsGooglePlusURLValid("http://www.google.com/pub/alan-daniel/7/31/8b7"));
        //}


        [TestMethod]
        public void Company_Email_IsValid_Return_True_If_Pass()
        {
            Assert.IsTrue(company1.IsValidEmail("abcd@gmail.com"));
            Assert.IsTrue(company1.IsValidEmail("kumar.amarapuram@landmarkit.co.in"));
        }

        [TestMethod]
        public void Company_Email_IsValid_Return_False_If_Fail()
        {
            Assert.IsFalse(company1.IsValidEmail("abcd@com"));
            Assert.IsFalse(company1.IsValidEmail("@landmarkit.co.in"));
            Assert.IsFalse(company1.IsValidEmail("sample@landmarkit"));
        }

        [TestMethod]
        public void Company_USZipCode_IsValid_Return_True_If_Pass()
        {
            Assert.IsTrue(company1.IsValidUSZipCode("07071 - 1234"));
            Assert.IsTrue(company1.IsValidUSZipCode("07071"));
            Assert.IsTrue(company1.IsValidUSZipCode("07071-1234"));
            Assert.IsTrue(company1.IsValidUSZipCode("07071 -1234"));
            Assert.IsTrue(company1.IsValidUSZipCode("07071 1234"));
            Assert.IsTrue(company1.IsValidUSZipCode("070711234"));
            Assert.IsTrue(company1.IsValidUSZipCode("07071 1234"));
        }

        [TestMethod]
        public void Company_USZipCode_IsValid_Return_False_If_Fail()
        {
            Assert.IsFalse(company1.IsValidUSZipCode("0707115"));
            Assert.IsFalse(company1.IsValidUSZipCode("00707115"));
            Assert.IsFalse(company1.IsValidUSZipCode("070711Smart"));
            Assert.IsFalse(company1.IsValidUSZipCode("A1A 1A1"));
        }

        [TestMethod]
        public void Company_CanadianPostalCode_IsValid_Return_True_If_Pass()
        {
            Assert.IsTrue(company1.IsValidCanadianPostalCode("A1A 1A1"));
            Assert.IsTrue(company1.IsValidCanadianPostalCode("A1A1A1"));
            Assert.IsTrue(company1.IsValidCanadianPostalCode("a1a 1b1"));
            Assert.IsTrue(company1.IsValidCanadianPostalCode("Z1Z1z1"));
            Assert.IsTrue(company1.IsValidCanadianPostalCode(" Z1Z 1z1"));
            Assert.IsTrue(company1.IsValidCanadianPostalCode(" Z1Z-1z1"));
            Assert.IsTrue(company1.IsValidCanadianPostalCode("Z1Z-1z1"));
            Assert.IsTrue(company1.IsValidCanadianPostalCode("Z1Z-1z1"));
            Assert.IsTrue(company1.IsValidCanadianPostalCode("Z1Z-1-z1"));
            Assert.IsTrue(company1.IsValidCanadianPostalCode("---------     Z1Z       -1-z1"));
            Assert.IsTrue(company1.IsValidCanadianPostalCode("Z-1-Z-1-s-1-"));
        }

        [TestMethod]
        public void Company_CanadianPostalCode_IsValid_Return_False_If_Fail()
        {
            Assert.IsFalse(company1.IsValidCanadianPostalCode("A1AA11"));
            Assert.IsFalse(company1.IsValidCanadianPostalCode("A1AA111"));
            Assert.IsFalse(company1.IsValidCanadianPostalCode("aA1AA1"));
            Assert.IsFalse(company1.IsValidCanadianPostalCode("07071"));
            Assert.IsFalse(company1.IsValidCanadianPostalCode("870711"));
            Assert.IsFalse(company1.IsValidCanadianPostalCode("A1A12A"));
        }

        [TestMethod]
        public void Company_ImageType_IsValid_Return_True_If_Pass()
        {
            company1.ImageUrl = @"C:\Users\Public\Pictures\Sample Pictures\Chrysanthemum.jpg";
            company2.ImageUrl = @"http://www.smarttouchinteractive.com/newsblog/wp-content/uploads/2013/06/photo.jpeg";
            Assert.IsTrue(company1.IsValidImageType());
            Assert.IsTrue(company1.IsValidImageType());
        }

        [TestMethod]
        public void Company_ImageType_IsValid_Return_False_If_Fail()
        {
            company1.ImageUrl = @"C:\Users\Public\Pictures\Sample Pictures\Chrysanthemum.exe";
            company2.ImageUrl = @"C:\Users\Public\Pictures\Sample Pictures\Chrysanthemum";
            Assert.IsFalse(company1.IsValidImageType());
            Assert.IsFalse(company2.IsValidImageType());
        }
    }
}

