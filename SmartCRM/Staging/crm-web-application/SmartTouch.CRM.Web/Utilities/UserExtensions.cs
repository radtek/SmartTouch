using SmartTouch.CRM.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;

namespace SmartTouch.CRM.Web.Utilities
{
    public static class UserExtensions
    {
        public static int ToUserID(this IIdentity identity)
        {
            var result = default(int);
            int.TryParse(GetClaimValue(identity, Claims.UserID), out result);
            return result;
        }
        public static string ToFirstName(this IIdentity identity)
        {
            return GetClaimValue(identity, Claims.FirstName);
        }
        public static string ToLastName(this IIdentity identity)
        {
            return GetClaimValue(identity, Claims.LastName);
        }
        public static string ToUserName(this IIdentity identity)
        {
            return GetClaimValue(identity, Claims.UserName);
        }
        public static short ToRoleID(this IIdentity identity)
        {
            var result = default(short);
            short.TryParse(GetClaimValue(identity, Claims.RoleID), out result);
            return result;
        }
        public static string ToRoleName(this IIdentity identity)
        {
            return GetClaimValue(identity, Claims.RoleName);
        }
        public static int ToAccountID(this IIdentity identity)
        {
            var result = default(int);
            int.TryParse(GetClaimValue(identity, Claims.AccountID), out result);
            return result;
        }

        public static string ToDateFormat(this IIdentity identity)
        {
            return GetClaimValue(identity, Claims.DateFormat);
        }

        public static string Country(this IIdentity identity)
        {
            return GetClaimValue(identity, Claims.CountryID);
        }

        public static string ToItemsPerPage(this IIdentity identity)
        {
            return GetClaimValue(identity, Claims.ItemsPerPage);
        }

        public static string ToCurrency(this IIdentity identity)
        {
            return GetClaimValue(identity, Claims.Currency);
        }

        public static string ToTimeZone(this IIdentity identity)
        {
            var result = GetClaimValue(identity, Claims.TimeZone);
            if(string.IsNullOrWhiteSpace(result)) result = "Central America Standard Time";
            return result;
        }

        public static string ToUserEmail(this IIdentity identity)
        {
            return GetClaimValue(identity, Claims.UserEmail);
        }

        public static string ToAccountName(this IIdentity identity)
        {
            return GetClaimValue(identity, Claims.AccountName);
        }

        public static string ToAccountURL(this IIdentity identity)
        {
            return GetClaimValue(identity, Claims.AccountURL);
        }

        public static string ToIanaTimeZone(this IIdentity identity)
        {
            return GetClaimValue(identity, Claims.IanaTimeZone);
        }
        public static bool IsSTAdmin(this IIdentity identity)
        {
            bool result;
            bool.TryParse(GetClaimValue(identity, Claims.IsSTAdmin),out result);
            return result;
        }

        public static bool IsInFeature(this IIdentity identity, AppFeatures featureName)
        {
            var result = default(bool);
            var claims = (identity as System.Security.Claims.ClaimsIdentity).Claims;
            if (claims != null && claims.Where(c => c.Type == Claims.FeatureID).Any())
            {
                var operations = claims.Where(c => c.Type == Claims.FeatureID).Select(c => int.Parse(c.Value)).ToList();
                result = operations.Contains((int)featureName);
            }
            return result;
        }

        public static List<short> ToFeatureIds(this IIdentity identity)
        {
            //TODO: implement this
            return new List<short>();
        }

        public static string ToAccountPrimaryEmail(this IIdentity identity)
        {
            return GetClaimValue(identity, Claims.AccountPrimaryEmail);
        }
        #region Get Raw Claim
        private static string GetClaimValue(IIdentity identity, string claimName)
        {
            var result = default(string);
            var claimsIdentity = identity as ClaimsIdentity;
            var claims = claimsIdentity != null ? claimsIdentity.Claims : null;
            //if (claims != null && claims.Where(c => c.Type == Claims.RoleName).Any()) result = claims.Where(c => c.Type == Claims.RoleName).First().Value;
            if (claims != null && claims.Where(c => c.Type == claimName).Any()) result = claims.Where(c => c.Type == claimName).First().Value;
            return result;
        }
        #endregion
    }

    public class UserResources
    {
        int accountId;

        public UserResources(int accountId)
        {
            this.accountId = accountId;
        }

        public string UserPersmissionsCacheName
        {
            get { return "userpermissions" + accountId; }
        }

        public string AccountPersmissionsCacheName
        {
            get { return "accountpermissions" + accountId; }
        }

        public string GetDependencyFileName(string key)
        {
            return string.Format("{0}_FileDependency.txt", key);
        }
    }
}