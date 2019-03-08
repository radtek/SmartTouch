using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web.Http;

namespace SmartTouch.CRM.WebService.Controllers
{
    /// <summary>
    /// Creating smartTouchApi Controller
    /// </summary>
    [SmartTouchApiActionFilter]
    public abstract class SmartTouchApiController : ApiController
    {       
        /// <summary>
        /// For Account ID
        /// </summary>
        #region Claims
        protected int AccountId
        {
            get
            {
                var accountIdString = ((ClaimsPrincipal)Thread.CurrentPrincipal).Claims.Where(c => c.Type == "AccountID").Select(c => c.Value).FirstOrDefault();
                var accountId = default(int);
                int.TryParse(accountIdString, out accountId);
                return accountId;
            }
        }

        /// <summary>
        /// For User ID
        /// </summary>
        protected int UserId
        {
            get
            {
                var userIdString = ((ClaimsPrincipal)Thread.CurrentPrincipal).Claims.Where(c => c.Type == "UserID").Select(c => c.Value).FirstOrDefault();
                var userId = default(int);
                int.TryParse(userIdString, out userId);
                return userId;
            }
        }

        /// <summary>
        /// For Role ID
        /// </summary>
        protected short RoleId
        {
            get
            {
                var roleIdString = ((ClaimsPrincipal)Thread.CurrentPrincipal).Claims.Where(c => c.Type == "RoleID").Select(c => c.Value).FirstOrDefault();
                var roleId = default(short);
                short.TryParse(roleIdString, out roleId);
                return roleId;
            }
        }

        /// <summary>
        /// For Role Name
        /// </summary>
        protected string RoleName
        {
            get
            {
                return ((ClaimsPrincipal)Thread.CurrentPrincipal).Claims.Where(c => c.Type == "RoleName").Select(c => c.Value).FirstOrDefault();
            }
        }

        /// <summary>
        /// For User Name
        /// </summary>
        protected string UserName
        {
            get
            {
                return ((ClaimsPrincipal)Thread.CurrentPrincipal).Claims.Where(c => c.Type == "UserName").Select(c => c.Value).FirstOrDefault();
            }
        }

        /// <summary>
        /// For Time Zone
        /// </summary>
        protected string TimeZone
        {
            get
            {
                return ((ClaimsPrincipal)Thread.CurrentPrincipal).Claims.Where(c => c.Type == "TimeZone").Select(c => c.Value).FirstOrDefault();
            }
        }
        /// <summary>
        /// Indian Time Zone
        /// </summary>
        protected string IanaTimeZone
        {
            get
            {
                return ((ClaimsPrincipal)Thread.CurrentPrincipal).Claims.Where(c => c.Type == "IanaTimeZone").Select(c => c.Value).FirstOrDefault();
            }
        }
        #endregion
    }
}