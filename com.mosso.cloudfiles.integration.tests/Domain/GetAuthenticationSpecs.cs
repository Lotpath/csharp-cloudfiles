using System;
using System.Net;
using System.Text.RegularExpressions;
using com.mosso.cloudfiles.domain;
using com.mosso.cloudfiles.domain.request;
using com.mosso.cloudfiles.domain.response;
using NUnit.Framework;


namespace com.mosso.cloudfiles.integration.tests.domain.AuthenticationRequestSpecs
{
    [TestFixture]
    public class When_requesting_client_authentication
    {
        private GetAuthentication request;
        private GenerateRequestByType factory;
        private const string STORAGE_TOKEN = "5d8f3dca-7eb9-4453-aa79-2eea1b980353";

        [SetUp]
        public void Setup()
        {
            request =
                new GetAuthentication(new UserCredentials(Credentials.USERNAME, Credentials.API_KEY));
            factory = new GenerateRequestByType();
        }

        [Test]
        public void Should_get_204_response_when_authenticated_correctly()
        {

            IResponse response = factory.Submit(request);
            Assert.That(response.Status, Is.EqualTo(HttpStatusCode.NoContent));
        }

        [Test]
        public void Should_get_storage_url_when_authenticated_correctly()
        {
            var response = factory.Submit(request, null);
            Assert.That(response.Headers[Constants.XStorageUrl].Length, Is.GreaterThan(0));
            var storageUri = new Uri(response.Headers[Constants.XStorageUrl]);
            Assert.That(Regex.Match(storageUri.AbsolutePath, "/v1/MossoCloudFS_.*").Success, Is.True);
        }

        [Test]
        public void Should_get_auth_token_when_authenticated_correctly()
        {
            var response = factory.Submit(request);
            var authToken = response.Headers[Constants.XAuthToken];
            Assert.That(authToken.Length, Is.GreaterThan(0));
            Assert.That(authToken.Length, Is.EqualTo(STORAGE_TOKEN.Length));
        }

        [Test]
        public void Should_get_content_when_authenticated_correctly()
        {
            var response = factory.Submit(request, null);
            Assert.That(response.Headers["Content-Length"], Is.EqualTo("0"));
        }

        [Test]
        public void Should_return_a_cdn_management_url_header()
        {
            var response =
                factory.Submit(request, null);
            Assert.That(response.Headers[Constants.XCdnManagementUrl], Is.Not.Null);
        }

        [Test]
        public void Should_return_an_authorization_token_header()
        {
            var response =
               factory.Submit(request, null);
            Assert.That(response.Headers[Constants.XAuthToken], Is.Not.Null);
        }

        [Test]
        public void Should_get_401_response_when_authenticated_incorrectly()
        {
            request =
                new GetAuthentication(new UserCredentials("EPIC", "FAIL"));

            try
            {
                factory.Submit(request, null);
                Assert.Fail("Should throw WebException");
            }
            catch (WebException we)
            {
                //It's a protocol error that is usually a result of a 401 (Unauthorized)
                //Still trying to figure way to get specific httpstatuscode
                Assert.That(((HttpWebResponse) we.Response).StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            }
        }

    }
}