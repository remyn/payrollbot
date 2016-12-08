using System;
using System.Security.Principal;

namespace ReckonTwo.Security
{
    public class CustomPrincipal : IPrincipal
    {
        public CustomPrincipal(IIdentity idty, String _Roles)
        {
            identity = idty;
            TheRoles = _Roles;
        }

        IIdentity identity;
        private String TheRoles;

        public IIdentity Identity
        {
            get { return identity; }
        }

        public bool IsInRole(string role)
        {
            return TheRoles.Contains(role);
        }

        public String Roles { get { return TheRoles; } }
    }
}