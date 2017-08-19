using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Filters {
    public class AuthFilter : IAuthorizationFilter {

        public void OnAuthorization(AuthorizationFilterContext context) {
            try {
                if (context.HttpContext.Connection.RemoteIpAddress.ToString() != "127.0.0.1" && context.HttpContext.Connection.RemoteIpAddress.ToString() != "::1" && context.HttpContext.Connection.RemoteIpAddress.ToString() != "localhost") {
                    context.Result = new UnauthorizedResult();
                }
            } catch { }
        }
    }
}
