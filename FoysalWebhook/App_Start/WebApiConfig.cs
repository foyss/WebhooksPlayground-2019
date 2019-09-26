using FoysalWebhook.Webhooks;
using Microsoft.AspNet.WebHooks;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.AspNet.WebHooks.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Web.Http;
using Microsoft.AspNet.WebHooks.Controllers;

namespace FoysalWebhook
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            var controllerType = typeof(WebHookReceiversController);

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new
                {
                    id = RouteParameter.Optional
                }
            );

            // Load Azure Storage or SQL for persisting subscriptions
            // config.InitializeCustomWebHooksAzureStorage();
            // config.InitializeCustomWebHooksSqlStorage();

            // Load Azure Queued Sender for enqueueing outgoing WebHooks to an Azure Storage Queue
            // config.InitializeCustomWebHooksAzureQueueSender();

            // Uncomment the following to set a custom WebHook sender where you can control how you want 
            // the outgoing WebHook request to look.
            ILogger logger = CommonServices.GetLogger();
            IWebHookSender sender = new TestWebhookSender(logger);
            CustomServices.SetSender(sender);
                       
            // Load basic support for sending WebHooks
            config.InitializeCustomWebHooks();

            // Load Web API controllers for managing subscriptions
            config.InitializeCustomWebHooksApis();

            config.InitializeReceiveCustomWebHooks();

            config.EnsureInitialized();

            var webhookRoutes = config.Routes;

            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/octet-stream"));
        }
    }
}
