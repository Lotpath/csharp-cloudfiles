using com.mosso.cloudfiles.domain;
using com.mosso.cloudfiles.domain.request;
using com.mosso.cloudfiles.domain.response;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace com.mosso.cloudfiles.integration.tests
{
    public class TestBase
    {
        protected string storageUrl;
        protected string authToken;
        protected AbstractConnection connection;

        [SetUp]
        public void SetUpBase()
        {
            var request = new GetAuthentication(new UserCredentials(Credentials.USERNAME, Credentials.API_KEY));
            var response = new ResponseFactory().Create(new CloudFilesRequest(request));

            storageUrl = response.Headers[Constants.XStorageUrl];
            authToken = response.Headers[Constants.XAuthToken];
            Assert.That(authToken.Length, Is.EqualTo(36));
            connection = new Connection(new UserCredentials(Credentials.USERNAME, Credentials.API_KEY));
            SetUp();
        }


        protected virtual void SetUp()
        {
        }
    }
}