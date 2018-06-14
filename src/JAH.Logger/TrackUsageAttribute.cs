using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc.Filters;

namespace JAH.Logger
{
    [AttributeUsage(AttributeTargets.Method)]
    public class TrackUsageAttribute : ActionFilterAttribute
    {
        private readonly IJahLogger _logger;

        private readonly string _product;

        private readonly string _layer;

        private readonly string _activityName;

        public TrackUsageAttribute(IJahLogger logger, string product, string layer, string activityName)
        {
            _logger = logger;
            _product = product;
            _layer = layer;
            _activityName = activityName;
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var dict = new Dictionary<string, object>();
            foreach (var key in context.RouteData.Values?.Keys)
            {
                dict.Add($"RouteData-{key}", (string)context.RouteData.Values[key]);
            }

            _logger.LogWebUsage(_product, _layer, _activityName, context.HttpContext, dict);
        }
    }
}
