using System;
using System.Net;
using Rackspace.CloudFiles.Domain;
using NUnit.Framework;
using Rackspace.CloudFiles.Domain.Request;
using Rackspace.CloudFiles.Domain.Response;
using Rackspace.CloudFiles.Exceptions;


namespace Rackspace.CloudFiles.Integration.Tests.domain.DeleteContainerSpecs
{
    [TestFixture]
    public class When_deleting_a_container : TestBase
    {
        [Test]
        public void Should_return_no_content_when_the_container_exists()
        {
            PutContainer(storageUrl, Constants.CONTAINER_NAME);
            var deleteContainer = new DeleteContainer(storageUrl, Constants.CONTAINER_NAME);
            var response = new GenerateRequestByType().Submit(deleteContainer, authToken);
            Assert.That(response.Status, Is.EqualTo(HttpStatusCode.NoContent));
        }

        [Test]
        [ExpectedException(typeof (WebException))]
        public void Should_return_404_when_container_does_not_exist()
        {
            var deleteContainer = new DeleteContainer(storageUrl,  Guid.NewGuid().ToString());

             new GenerateRequestByType().Submit(deleteContainer, authToken);
            Assert.Fail("404 Not found exception expected");
        }

        [Test]
        public void Should_return_conflict_status_when_the_container_exists_and_is_not_empty()
        {
            
            try
            {
                using (new TestHelper(authToken, storageUrl))
                {
                    var putStorageItem = new PutStorageItem(storageUrl, Constants.CONTAINER_NAME, Constants.StorageItemName, Constants.StorageItemName);
                    
                  var putStorageItemResponse=  new GenerateRequestByType().Submit(putStorageItem, authToken);
                    Assert.That(putStorageItemResponse.Status, Is.EqualTo(HttpStatusCode.Created));
                }
                Assert.Fail("409 conflict expected");
            }
            catch (WebException we)
            {
                var response = (HttpWebResponse)we.Response;
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            }
            finally
            {
                var genrequest = new GenerateRequestByType();
                  genrequest.Submit( new DeleteStorageItem(storageUrl, Constants.CONTAINER_NAME, Constants.StorageItemName),authToken);
                genrequest.Submit(new DeleteContainer(storageUrl, Constants.CONTAINER_NAME), authToken);
            }
        }

        [Test]
        public void Should_throw_an_exception_when_the_container_name_exceeds_the_maximum_length()
        {
            string containerName = new string('a', Constants.MaximumContainerNameLength + 1);
            try
            {
                using (new TestHelper(authToken, storageUrl, containerName))
                {
                    var putStorageItem = new PutStorageItem(storageUrl, containerName, Constants.StorageItemName, Constants.StorageItemName);
                 //   Assert.That(putStorageItem.ContentLength, Is.GreaterThan(0));

                    var putStorageItemResponse = new GenerateRequestByType().Submit(putStorageItem, authToken);
                    Assert.That(putStorageItemResponse.Status, Is.EqualTo(HttpStatusCode.Created));
                }
                Assert.Fail("ContainerNameException expected");
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.TypeOf(typeof (ContainerNameException)));
            }
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void Should_throw_an_exception_when_the_storage_url_is_null()
        {
            new DeleteContainer(null, "a");
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void Should_throw_an_exception_when_the_container_name_is_null()
        {
            new DeleteContainer("a", null);
        }

        

        private void PutContainer(string storageUri, String containerName)
        {
            var createContainer = new CreateContainer(storageUri, containerName);

            IResponse response = new GenerateRequestByType().Submit(createContainer, authToken);
            Assert.That(response.Status, Is.EqualTo(HttpStatusCode.Created));
        }
    }
}