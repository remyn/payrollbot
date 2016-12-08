using System;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ReckonTwo.Security
{
    public class BasicAuthenticationMessageHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var authHeader = request.Headers.Authorization;

            if (authHeader == null)
            {
                return base.SendAsync(request, cancellationToken);
            }

            if (authHeader.Scheme != "Basic")
            {
                return base.SendAsync(request, cancellationToken);
            }

            var encodedUserPass = authHeader.Parameter.Trim();
            var userPass = Encoding.ASCII.GetString(Convert.FromBase64String(encodedUserPass));
            var parts = userPass.Split(":".ToCharArray());
            var email = parts[0];
            var password = parts[1];

            email = email.Replace("\0", "");
            password = password.Replace("\0", "");

            string EncPass = Helper.HashPassword(password.Trim());

            if (!Helper.ConfirmLoginDetails(EncPass, email))
            {
                return base.SendAsync(request, cancellationToken);
            }

            var identity = new GenericIdentity(email, "Basic");
            string[] roles = new string[1] { "1" };
            var principal = new GenericPrincipal(identity, roles);

            Thread.CurrentPrincipal = principal;

            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = principal;
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}