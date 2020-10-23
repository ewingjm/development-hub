﻿namespace DevelopmentHub.Repositories
{
    using System;
    using System.Net;

    /// <summary>
    /// Web client optimised for use in the Common Data Service sandbox.
    /// <see href="https://docs.microsoft.com/en-us/powerapps/developer/common-data-service/best-practices/business-logic/set-keepalive-false-interacting-external-hosts-plugin"/>.
    /// </summary>
    public class SandboxWebClient : WebClient
    {
        /// <inheritdoc/>
        protected override WebRequest GetWebRequest(Uri address)
        {
            var req = (HttpWebRequest)base.GetWebRequest(address);
            req.KeepAlive = false;

            return req;
        }
    }
}
