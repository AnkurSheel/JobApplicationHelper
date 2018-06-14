using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace JAH.Logger
{
    public static class WebLoggerExtensions
    {
        public static void LogWebUsage(this IJahLogger logger,
                                       string product,
                                       string layer,
                                       string activityName,
                                       HttpContext context,
                                       Dictionary<string, object> additionalInfo = null)
        {
            var details = GetWebFlogDetail(product, layer, activityName, context, additionalInfo);
            logger.WriteUsage(details);
        }

        private static LogDetail GetWebFlogDetail(string product,
                                                  string layer,
                                                  string activityName,
                                                  HttpContext context,
                                                  Dictionary<string, object> additionalInfo)
        {
            var detail = new LogDetail
            {
                Product = product,
                Layer = layer,
                Message = activityName,
                Hostname = Environment.MachineName,
                CorrelationId = Activity.Current?.Id ?? context.TraceIdentifier,
            };

            foreach (var info in additionalInfo)
            {
                detail.AdditionalInfo.Add(info.Key, info.Value);
            }

            GetUserData(detail, context.User);
            GetRequestData(detail, context.Request);
            return detail;
        }

        private static void GetUserData(LogDetail detail, ClaimsPrincipal user)
        {
            var userId = string.Empty;
            var userName = string.Empty;
            if (user != null)
            {
                var i = 1; // i included in dictionary key to ensure uniqueness
                foreach (var claim in user.Claims)
                {
                    if (claim.Type == ClaimTypes.NameIdentifier)
                    {
                        userId = claim.Value;
                    }
                    else if (claim.Type == "name")
                    {
                        userName = claim.Value;
                    }
                    else
                    {
                        detail.AdditionalInfo.Add($"UserClaim-{i++}-{claim.Type}", claim.Value);
                    }
                }
            }

            detail.UserId = userId;
            detail.UserName = userName;
        }

        private static void GetRequestData(LogDetail detail, HttpRequest request)
        {
            if (request != null)
            {
                detail.Location = request.Path;

                detail.AdditionalInfo.Add("UserAgent", request.Headers["User-Agent"]);
                detail.AdditionalInfo.Add("Languages", request.Headers["Accept-Language"]);

                Dictionary<string, StringValues> qdict = QueryHelpers.ParseQuery(request.QueryString.ToString());
                foreach (var key in qdict.Keys)
                {
                    detail.AdditionalInfo.Add($"QueryString-{key}", qdict[key]);
                }
            }
        }
    }
}
