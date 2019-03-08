using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.ValueObjects;
using Moq;
using System.Linq;

namespace SmartTouch.CRM.Domain.Tests
{
    [TestClass]
    public class PersonTest
    {
     
        Person person1 = new Person();
        Person person2 = new Person();
        Person person3 = new Person();
        Person person4 = new Person();
        Person person5 = new Person();
        Person person6 = new Person();
        Person person7 = new Person();
        Person person8 = new Person();
        Person person9 = new Person();
        Person person10 = new Person();

        List<Person> persons = new List<Person>();
        [TestInitialize]
        public void Initialize()
        {
            
            for (int i = 1; i <= 20; i++)
            {
                Person person = new Person();
                person.FirstName = "FN" + i;
                person.LastName = "LN" + i;
                //person.Email = new Email { EmailId = "email" + i + "@st.com" };
                persons.Add(person);
            }
            List<Address> addresses = new List<Address>();
            for (int i = 1; i <= 5; i++)
            {
                Address address = new Address();
                address.AddressLine1 = "Add 1" + i.ToString();
                address.AddressLine2 = "Add 2" + i.ToString();
                address.City = "City " + i.ToString();
                address.State = new State{Code= "US-AK"};
                //address.State.Code = "US-AK";
                address.Country = new Country { Code = "US" };
                addresses.Add(address);
            };

            person1.Addresses = addresses;
        }

      
        [TestMethod]
        public void Person_PhoneNumber_Length_Greaterthan_Or_EqualTo_10_Return_True_If_Pass()
        {
            Assert.AreEqual(true, person1.IsValidPhoneNumberLength("2345678900"));
            //Assert.AreEqual(true, person1.IsValidPhoneNumberLength("0019885021320"));
            Assert.AreEqual(true, person1.IsValidPhoneNumberLength("(212)897-0067"));
            Assert.AreEqual(true, person1.IsValidPhoneNumberLength("1(212)897-0067"));
            //Assert.AreEqual(true, person1.IsValidPhoneNumberLength("001(212)897-0067"));
            //Assert.AreEqual(true, person1.IsValidPhoneNumberLength("001.212.897.0067"));
            //Assert.AreEqual(true, person1.IsValidPhoneNumberLength("001(212)897.00671234"));
            //Assert.AreEqual(true, person1.IsValidPhoneNumberLength("001 212 897 0067"));
        }

        [TestMethod]
        public void Person_PhoneNumber_Length_Greaterthan_Or_EqualTo_10__Return_False_If_Fail()
        {
            Assert.AreEqual(false, person1.IsValidPhoneNumberLength("(234)567-891"));
            Assert.AreEqual(false, person1.IsValidPhoneNumberLength("001(234)-567-891"));
            Assert.AreEqual(false, person1.IsValidPhoneNumberLength("10408123456"));
            Assert.AreEqual(false, person1.IsValidPhoneNumberLength("100000000000000000001"));
            Assert.AreEqual(false, person1.IsValidPhoneNumberLength("200000000"));
            Assert.AreEqual(false, person1.IsValidPhoneNumberLength("040404004"));
        }

        
        //[TestMethod]
        //public void Person_PhoneNumber_Has_Alpha_And_Specials_If_Extracted_Number_Is_Valid_Return_True()
        //{
        //    Assert.AreEqual(false, person1.IsValidPhoneNumberLength("01152515409885021320SmartTouch"));
        //    Assert.AreEqual(false, person1.IsValidPhoneNumberLength("DFUASDFU8E8321416464452127"));
        //}

        [TestMethod]
        public void Person_PhoneNumber_Has_Alpha_And_Specials_If_Extracted_Number_Is_Invalid_Return_False()
        {
            Assert.AreEqual(false, person1.IsValidPhoneNumberLength("DFUASDFDASFSD"));
            Assert.AreEqual(false, person1.IsValidPhoneNumberLength("98850DFASF152"));
        }

        //[TestMethod]
        //public void Person_FacebookURL_IsValid_Return_True_If_Pass()
        //{
        //    person1.FacebookUrl = new Url { URL = "http://facebook.com/smarttouch" };
        //    Assert.IsTrue(person1.IsValidFacebookURL());
        //}

        //[TestMethod]
        //public void Person_FacebookURL_IsValid_Return_False_If_Fail()
        //{
        //    person1.FacebookUrl = new Url { URL = "http:" };
        //    Assert.IsFalse(person1.IsValidFacebookURL());
        //}

        //[TestMethod]
        //public void Person_LinkedInURL_IsValid_Return_True_If_Pass()
        //{
        //    person1.LinkedInUrl = new Url { URL = "http://www.linkedin.com/pub/alan-daniel/7/31/8b7" };
        //    Assert.IsTrue(person1.IsValidLinkedInURL());
        //}

        //[TestMethod]
        //public void Person_LinkedInURL_IsValid_Return_False_If_Fail()
        //{
        //    person1.LinkedInUrl = new Url { URL = "http://www.linked.com/pub/alan-daniel/7/31/8b7" };
        //    Assert.IsFalse(person1.IsValidLinkedInURL());
        //}

        //[TestMethod]
        //public void Person_GooglePlusURL_IsValid_Return_True_If_Pass()
        //{
        //    person1.GooglePlusUrl = new Url { URL = "https://plus.google.com/u/0/111018300559763791383/posts" };
        //    Assert.IsTrue(person1.IsValidGooglePlusURL());
        //}

        //[TestMethod]
        //public void Person_GooglePlusURL_IsValid_Return_False_If_Fail()
        //{
        //    person1.GooglePlusUrl = new Url { URL = "https://googleplus.com/u/0/111018300559763791383/posts" };
        //    Assert.IsFalse(person1.IsValidGooglePlusURL());
        //}


        [TestMethod]
        public void Person_Email_IsValid_Return_True_If_Pass()
        {
            Assert.IsTrue(person1.IsValidEmail("abcd@gmail.com"));
            Assert.IsTrue(person1.IsValidEmail("kumar.amarapuram@landmarkit.co.in"));
        }

        [TestMethod]
        public void Person_Email_IsValid_Return_False_If_Fail()
        {
            Assert.IsFalse(person1.IsValidEmail("abcd@com"));
            Assert.IsFalse(person1.IsValidEmail("@landmarkit.co.in"));
            Assert.IsFalse(person1.IsValidEmail("sample@landmarkit"));
        }

        [TestMethod]
        public void Person_USZipCode_IsValid_Return_True_If_Pass()
        {
            Assert.IsTrue(person1.IsValidUSZipCode("07071 - 1234"));
            Assert.IsTrue(person1.IsValidUSZipCode("07071"));
            Assert.IsTrue(person1.IsValidUSZipCode("07071-1234"));
            Assert.IsTrue(person1.IsValidUSZipCode("07071 -1234"));
            Assert.IsTrue(person1.IsValidUSZipCode("07071 1234"));
            Assert.IsTrue(person1.IsValidUSZipCode("070711234"));
            Assert.IsTrue(person1.IsValidUSZipCode("07071 1234"));
        }

        [TestMethod]//Incomplete
        public void Person_USZipCode_IsValid_Return_False_If_Fail()
        {
            Assert.IsFalse(person1.IsValidUSZipCode("0707115"));
            Assert.IsFalse(person1.IsValidUSZipCode("00707115"));
            Assert.IsFalse(person1.IsValidUSZipCode("070711Smart"));
            Assert.IsFalse(person1.IsValidUSZipCode("A1A 1A1"));
        }

        [TestMethod]
        public void Person_CanadianPostalCode_IsValid_Return_True_If_Pass()
        {
            Assert.IsTrue(person1.IsValidCanadianPostalCode("A1A 1A1"));
            Assert.IsTrue(person1.IsValidCanadianPostalCode("A1A1A1"));
            Assert.IsTrue(person1.IsValidCanadianPostalCode("a1a 1b1"));
            Assert.IsTrue(person1.IsValidCanadianPostalCode("Z1Z1z1"));
            Assert.IsTrue(person1.IsValidCanadianPostalCode(" Z1Z 1z1"));
            Assert.IsTrue(person1.IsValidCanadianPostalCode(" Z1Z-1z1"));
            Assert.IsTrue(person1.IsValidCanadianPostalCode("Z1Z-1z1"));
            Assert.IsTrue(person1.IsValidCanadianPostalCode("Z1Z-1z1"));
            Assert.IsTrue(person1.IsValidCanadianPostalCode("Z1Z-1-z1"));
            Assert.IsTrue(person1.IsValidCanadianPostalCode("---------     Z1Z       -1-z1"));
            Assert.IsTrue(person1.IsValidCanadianPostalCode("Z-1-Z-1-s-1-"));
        }

        [TestMethod]
        public void Person_CanadianPostalCode_IsValid_Return_False_If_Fail()
        {
            Assert.IsFalse(person1.IsValidCanadianPostalCode("A1AA11"));
            Assert.IsFalse(person1.IsValidCanadianPostalCode("A1AA111"));
            Assert.IsFalse(person1.IsValidCanadianPostalCode("aA1AA1"));
            Assert.IsFalse(person1.IsValidCanadianPostalCode("07071"));
            Assert.IsFalse(person1.IsValidCanadianPostalCode("870711"));
            Assert.IsFalse(person1.IsValidCanadianPostalCode("A1A12A"));
        }



        [TestMethod]
        public void Person_SSN_IsValid_Return_True_If_Pass()
        {
            person2.SSN = "123456789";
            person3.SSN = "123-456-789";
            person4.SSN = "123-45-6789";
            person5.SSN = "123-456-789";
            person6.SSN = "123 456 789";
            person7.SSN = "123 45 6789";

            Assert.IsTrue(person2.IsValidSsnOrSin());
            Assert.IsTrue(person3.IsValidSsnOrSin());
            Assert.IsTrue(person4.IsValidSsnOrSin());
            Assert.IsTrue(person5.IsValidSsnOrSin());
            Assert.IsTrue(person6.IsValidSsnOrSin());
            Assert.IsTrue(person7.IsValidSsnOrSin());
        }

        [TestMethod]
        public void Person_SSN_IsValid_Return_False_If_Fail()
        {
            person2.SSN = "123 - 2252111 - 2585";
            person3.SSN = "1231258511" ;
            person4.SSN = "225-2111-2585" ;
            Assert.IsFalse(person2.IsValidSsnOrSin());
            Assert.IsFalse(person3.IsValidSsnOrSin());
            Assert.IsFalse(person4.IsValidSsnOrSin());
        }

        [TestMethod]
        public void Person_ImageType_IsValid_Return_True_If_Pass()
        {
            person1.ImageUrl = @"C:\Users\Public\Pictures\Sample Pictures\Chrysanthemum.jpg";
            person2.ImageUrl = @"http://www.smarttouchinteractive.com/newsblog/wp-content/uploads/2013/06/photo.jpeg";
            Assert.IsTrue(person1.IsValidImageType());
            Assert.IsTrue(person2.IsValidImageType());
        }

        [TestMethod]
        public void Person_ImageType_IsValid_Return_False_If_Fail()
        {
            person1.ImageUrl = @"C:\Users\Public\Pictures\Sample Pictures\Chrysanthemum.exe";
            person2.ImageUrl = @"C:\Users\Public\Pictures\Sample Pictures\Chrysanthemum";
            Assert.IsFalse(person1.IsValidImageType());
            Assert.IsFalse(person2.IsValidImageType());
        }

        [TestMethod]
        public void Person_Address_Has_Country_Return_True_If_Pass()
        {
            Assert.IsTrue(person1.Addresses.First().IsValidCountry());
        }
    }
}
