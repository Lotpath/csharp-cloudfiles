///
/// See COPYING file for licensing information
///

using System;
using System.Net;
using System.Threading;
using com.mosso.cloudfiles.domain.request.Interfaces;
using com.mosso.cloudfiles.domain.response;
using com.mosso.cloudfiles.domain.response.Interfaces;
using com.mosso.cloudfiles.utils;

namespace com.mosso.cloudfiles.domain.request
{
    /// <summary>
    /// Wraps requests to optionally handle proxy credentials and ssl
    /// </summary>
    public class CloudFilesRequest : ICloudFilesRequest
    {
        private readonly IRequest request;
        private readonly ProxyCredentials proxyCredentials;

        /// <summary>
        /// Constructor without proxy credentials provided
        /// </summary>
        /// <param name="request">The request being sent to the server</param>
        public CloudFilesRequest(IRequest request) : this(request, null)
        {
        }

        /// <summary>
        /// Constructor with proxy credentials provided
        /// </summary>
        /// <param name="request">The request being sent to the server</param>
        /// <param name="proxyCredentials">Proxy credentials</param>
        /// <exception cref="System.ArgumentNullException">Thrown when any of the reference arguments are null</exception>
        public CloudFilesRequest(IRequest request, ProxyCredentials proxyCredentials)
        {
            if (request == null) throw new ArgumentNullException();

            this.request = request;
            this.proxyCredentials = proxyCredentials;
        }
        
        /// <summary>
        /// RequestType
        /// </summary>
        /// <returns>the type of the request</returns>
        public Type RequestType
        {
            get { return request.GetType(); }
        }

        /// <summary>
        /// GetRequest
        /// </summary>
        /// <returns>a HttpWebRequest object that has all the information to make a request against CloudFiles</returns>
        
        public ICloudFilesResponse GetResponse()
        {
            var httpWebRequest = (HttpWebRequest)System.Net.WebRequest.Create(request.Uri);
            if (request.Headers != null) httpWebRequest.Headers.Add(request.Headers);

            httpWebRequest.Method = request.Method;
            httpWebRequest.Timeout = Constants.CONNECTION_TIMEOUT;
            httpWebRequest.UserAgent = Constants.USER_AGENT;

            HandleIsModifiedSinceHeaderRequestFieldFor(httpWebRequest);
            HandleRangeHeader(httpWebRequest);
            HandleRequestBodyFor(httpWebRequest);
            HandleProxyCredentialsFor(httpWebRequest);
            return new CloudFilesResponse((HttpWebResponse)httpWebRequest.GetResponse());

        }

        public string RequestUri
        {
            get { throw new AbandonedMutexException(); }
        }

        public string Method
        {
            get { throw new NotImplementedException(); }
        }

        public WebHeaderCollection Headers
        {
            get { throw new NotImplementedException(); }
        }

        private void HandleRangeHeader(HttpWebRequest webrequest)
        {
            if (!(request is IRangedRequest)) return;
            var rangedRequest = (IRangedRequest) request;
            if (rangedRequest.RangeFrom != 0 && rangedRequest.RangeTo == 0)
                webrequest.AddRange("bytes", rangedRequest.RangeFrom);
            else if (rangedRequest.RangeFrom == 0 && rangedRequest.RangeTo != 0)
                webrequest.AddRange("bytes", rangedRequest.RangeTo);
            else if (rangedRequest.RangeFrom != 0 && rangedRequest.RangeTo != 0)
                webrequest.AddRange("bytes", rangedRequest.RangeFrom, rangedRequest.RangeTo);
        }

        private void HandleIsModifiedSinceHeaderRequestFieldFor(HttpWebRequest webrequest)
        {
            if (!(request is IModifiedSinceRequest)) return;
            webrequest.IfModifiedSince = ((IModifiedSinceRequest)request).ModifiedSince;
        }

        private void HandleProxyCredentialsFor(HttpWebRequest httpWebRequest)
        {
            if (proxyCredentials == null) return;
            
            var loProxy = new WebProxy(proxyCredentials.ProxyAddress, true);

            if (proxyCredentials.ProxyUsername.Length > 0)
                loProxy.Credentials = new NetworkCredential(proxyCredentials.ProxyUsername, proxyCredentials.ProxyPassword, proxyCredentials.ProxyDomain);
            httpWebRequest.Proxy = loProxy;
        }

        private void HandleRequestBodyFor(HttpWebRequest httpWebRequest)
        {
            if (!(request is IRequestWithContentBody)) return;

            var requestWithContentBody = (IRequestWithContentBody) request;
            httpWebRequest.ContentLength = requestWithContentBody.ContentLength;
            httpWebRequest.AllowWriteStreamBuffering = false;
            if(httpWebRequest.ContentLength < 1)
                httpWebRequest.SendChunked = true;

            var requestMimeType = request.ContentType;
            httpWebRequest.ContentType = String.IsNullOrEmpty(requestMimeType) 
                ? "application/octet-stream" : requestMimeType;

            var stream = httpWebRequest.GetRequestStream();
            requestWithContentBody.ReadFileIntoRequest(stream);
        }

        
    }
}