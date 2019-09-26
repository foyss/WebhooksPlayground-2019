using Microsoft.AspNet.WebHooks;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace FoysalWebhook.Webhooks
{
    public class TestWebhookSender : DataflowWebHookSender
    {
        public TestWebhookSender(ILogger logger)
            : base(logger)
        {
        }

        /// <summary>
        /// By overriding this method you can shape the WebHook request URI exactly as you want, both 
        /// in terms of HTTP headers and HTTP body.
        /// </summary>
        protected override HttpRequestMessage CreateWebHookRequest(WebHookWorkItem workItem)
        {
            return base.CreateWebHookRequest(workItem);
        }

        /// <summary>
        /// By overriding this method you can control just the WebHook request body. The rest of the 
        /// WebHook request follows the default model including the HTTP header containing a signature of the 
        /// body.
        /// </summary>
        protected override JObject CreateWebHookRequestBody(WebHookWorkItem workItem)
        {
            return base.CreateWebHookRequestBody(workItem);
        }

        /// <summary>
        /// By overriding this method you can control just the WebHook header containing a signature of the 
        /// body.
        /// </summary>
        protected override void SignWebHookRequest(WebHookWorkItem workItem, HttpRequestMessage request, JObject body)
        {
            base.SignWebHookRequest(workItem, request, body);
        }
    }
}