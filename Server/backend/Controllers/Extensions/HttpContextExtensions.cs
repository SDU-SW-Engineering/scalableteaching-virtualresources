using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ScalableTeaching.Controllers.Extensions;

public static class HttpContextExtensions
{
    // /// <summary>
    // /// Gives the value of the username claim for the current HttpContext
    // /// </summary>
    // /// <returns>Username of logged in user</returns>
    public static string GetUsername(this ControllerBase @base)
    {
        return @base.HttpContext.User.Claims.First(claim => claim.Type == "username").Value.ToLower();
    }
}