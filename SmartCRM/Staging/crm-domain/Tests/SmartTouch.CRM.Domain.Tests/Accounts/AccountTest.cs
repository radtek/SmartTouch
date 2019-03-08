using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.ValueObjects;
using Moq;
using System.Linq;

namespace SmartTouch.CRM.Domain.Tests.Accounts
{
    [TestClass]
    public class AccountTest
    {
        Account account = new Account();

        List<Account> persons = new List<Account>();
        [TestInitialize]
        public void Initialize()
        {

            for (int i = 1; i <= 20; i++)
            {
                Account person = new Account();
                person.FirstName = "FN" + i;
                person.LastName = "LN" + i;
                persons.Add(person);
            }
            List<Address> addresses = new List<Address>();
            for (int i = 1; i <= 5; i++)
            {
                Address address = new Address();
                address.AddressLine1 = "Add 1" + i.ToString();
                address.AddressLine2 = "Add 2" + i.ToString();
                address.City = "City " + i.ToString();
                address.State = new State { Code = "US-AK" };
                address.Country = new Country { Code = "US" };
                addresses.Add(address);
            };
            account.Addresses = addresses;
        }

        [TestMethod]
        public void Account_Email_IsValid_Return_True_If_Pass()
        {
            Assert.IsTrue(account.IsValidEmail("abcd@gmail.com"));
            Assert.IsTrue(account.IsValidEmail("kumar.amarapuram@landmarkit.co.in"));
        }

        [TestMethod]
        public void Account_Email_IsValid_Return_False_If_Fail()
        {
            Assert.IsFalse(account.IsValidEmail("abcd@com"));
            Assert.IsFalse(account.IsValidEmail("@landmarkit.co.in"));
            Assert.IsFalse(account.IsValidEmail("sample@landmarkit"));
        }

        [TestMethod]
        public void Account_USZipCode_IsValid_Return_True_If_Pass()
        {
            Assert.IsTrue(account.IsValidUSZipCode("07071 - 1234"));
            Assert.IsTrue(account.IsValidUSZipCode("07071"));
            Assert.IsTrue(account.IsValidUSZipCode("07071-1234"));
            Assert.IsTrue(account.IsValidUSZipCode("07071 -1234"));
            Assert.IsTrue(account.IsValidUSZipCode("07071 1234"));
            Assert.IsTrue(account.IsValidUSZipCode("070711234"));
            Assert.IsTrue(account.IsValidUSZipCode("07071 1234"));
        }

        [TestMethod]
        public void Account_USZipCode_IsValid_Return_False_If_Fail()
        {
            Assert.IsFalse(account.IsValidUSZipCode("0707115"));
            Assert.IsFalse(account.IsValidUSZipCode("00707115"));
            Assert.IsFalse(account.IsValidUSZipCode("070711Smart"));
            Assert.IsFalse(account.IsValidUSZipCode("A1A 1A1"));
        }

        [TestMethod]
        public void Account_CanadianPostalCode_IsValid_Return_True_If_Pass()
        {
            Assert.IsTrue(account.IsValidCanadianPostalCode("A1A 1A1"));
            Assert.IsTrue(account.IsValidCanadianPostalCode("A1A1A1"));
            Assert.IsTrue(account.IsValidCanadianPostalCode("a1a 1b1"));
            Assert.IsTrue(account.IsValidCanadianPostalCode("Z1Z1z1"));
            Assert.IsTrue(account.IsValidCanadianPostalCode(" Z1Z 1z1"));
            Assert.IsTrue(account.IsValidCanadianPostalCode(" Z1Z-1z1"));
            Assert.IsTrue(account.IsValidCanadianPostalCode("Z1Z-1z1"));
            Assert.IsTrue(account.IsValidCanadianPostalCode("Z1Z-1z1"));
            Assert.IsTrue(account.IsValidCanadianPostalCode("Z1Z-1-z1"));
            Assert.IsTrue(account.IsValidCanadianPostalCode("---------     Z1Z       -1-z1"));
            Assert.IsTrue(account.IsValidCanadianPostalCode("Z-1-Z-1-s-1-"));
        }

        [TestMethod]
        public void Account_CanadianPostalCode_IsValid_Return_False_If_Fail()
        {
            Assert.IsFalse(account.IsValidCanadianPostalCode("A1AA11"));
            Assert.IsFalse(account.IsValidCanadianPostalCode("A1AA111"));
            Assert.IsFalse(account.IsValidCanadianPostalCode("aA1AA1"));
            Assert.IsFalse(account.IsValidCanadianPostalCode("07071"));
            Assert.IsFalse(account.IsValidCanadianPostalCode("870711"));
            Assert.IsFalse(account.IsValidCanadianPostalCode("A1A12A"));
        }

        [TestMethod]
        public void Person_Address_Has_Country_Return_True_If_Pass()
        {
            Assert.IsTrue(account.Addresses.First().IsValidCountry());
        }

        [TestMethod]
        public void Person_PhoneNumber_Length_Greaterthan_Or_EqualTo_10_Return_True_If_Pass()
        {
            Assert.AreEqual(true, account.IsValidPhoneNumberLength("2345678900"));
            Assert.AreEqual(true, account.IsValidPhoneNumberLength("(212)897-0067"));
            Assert.AreEqual(true, account.IsValidPhoneNumberLength("1(212)897-0067"));
        }

        [TestMethod]
        public void Person_PhoneNumber_Length_Greaterthan_Or_EqualTo_10__Return_False_If_Fail()
        {
            Assert.AreEqual(false, account.IsValidPhoneNumberLength("(234)567-891"));
            Assert.AreEqual(false, account.IsValidPhoneNumberLength("001(234)-567-891"));
            Assert.AreEqual(false, account.IsValidPhoneNumberLength("10408123456"));
            Assert.AreEqual(false, account.IsValidPhoneNumberLength("100000000000000000001"));
            Assert.AreEqual(false, account.IsValidPhoneNumberLength("200000000"));
            Assert.AreEqual(false, account.IsValidPhoneNumberLength("040404004"));
        }

        [TestMethod]
        public void Person_PhoneNumber_Has_Alpha_And_Specials_If_Extracted_Number_Is_Invalid_Return_False()
        {
            Assert.AreEqual(false, account.IsValidPhoneNumberLength("DFUASDFDASFSD"));
            Assert.AreEqual(false, account.IsValidPhoneNumberLength("98850DFASF152"));
        }
    }
}
