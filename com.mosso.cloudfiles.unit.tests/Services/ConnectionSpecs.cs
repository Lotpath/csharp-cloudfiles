using System;
using com.mosso.cloudfiles.domain;
using NUnit.Framework;


namespace com.mosso.cloudfiles.unit.tests.Services.ConnectionSpecs
{
    [TestFixture]
    public class When_instantiating_a_connection_object
    {
        [Test]
        public void Should_instantiate_engine_without_throwing_exception_when_authentication_passes()
        {
            var userCreds = new UserCredentials(
                new Uri(Constants.AUTH_URL), 
                Constants.CREDENTIALS_USER_NAME, 
                Constants.CREDENTIALS_PASSWORD, 
                Constants.CREDENTIALS_CLOUD_VERSION, 
                Constants.CREDENTIALS_ACCOUNT_NAME);
            var conection = new MockConnection(userCreds);

            Assert.That(conection.AuthenticationSuccessful, Is.True);
        }
    }

    internal class MockConnection : Connection
    {
        public MockConnection(UserCredentials userCreds) : base(userCreds){}

        public bool AuthenticationSuccessful { get; private set; }

        protected override void VerifyAuthentication()
        {
            AuthenticationSuccessful = true;
        }
    }
}