using System;
using System.Net;
using com.mosso.cloudfiles.domain;
using com.mosso.cloudfiles.domain.request;
using com.mosso.cloudfiles.domain.response;
using com.mosso.cloudfiles.domain.response.Interfaces;
using NUnit.Framework;


namespace com.mosso.cloudfiles.integration.tests.domain.RetrieveContainerRequestSpecs
{
    [TestFixture]
    public class When_requesting_a_list_of_containers_and_containers_are_present : TestBase
    {

        [Test]
        public void Should_return_OK_status()
        {
            
            using(new TestHelper(authToken, storageUrl))
            {
                ICloudFilesResponse response = null;
                try
                {
                    GetContainers request = new GetContainers(storageUrl);
 

                    response = new GenerateRequestByType(new RequestFactoryWithAgentSupport("NASTTestUserAgent")).Submit(request, authToken);

                    Assert.That(response.Status, Is.EqualTo(HttpStatusCode.OK));
                    Assert.That(response.ContentBody, Is.Not.Null);
                }
                finally
                {
                    if(response != null)
                        response.Dispose();
                }
            }
            
        }

        [Test]
        public void Should_return_the_list_of_containers()
        {

            
            using (new TestHelper(authToken, storageUrl))
            {
                ICloudFilesResponse response = null;
                try
                {
                    GetContainers request = new GetContainers(storageUrl);
                    response = new GenerateRequestByType().Submit(request, authToken);
                    Assert.That(response.ContentBody.Count, Is.GreaterThan(0));

                }
                finally
                {
                    if (response != null)
                        response.Dispose();
                }
            }
            
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void Should_throw_an_exception_when_the_storage_url_is_null()
        {
            new GetContainers(null);
        }

       
    }

    [TestFixture]
    public class When_requesting_a_list_of_containers_and_no_containers_are_present : TestBase
    {
        [Test]
        public void Should_return_No_Content_status()
        {
            Assert.Ignore("Is returning OK instead of NoContent, need to investigate - 7/14/2010");
            var request = new GetContainers(storageUrl);
      
            var response = new GenerateRequestByType(
                new RequestFactoryWithAgentSupport("NASTTestUserAgent")).Submit(request, authToken);

            Assert.That(response.Status, Is.EqualTo(HttpStatusCode.NoContent));
            if(response.ContentBody != null)
                Assert.That(response.ContentBody.Count, Is.EqualTo(0));
            response.Dispose();
        }

    }
}