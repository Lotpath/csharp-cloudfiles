using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using com.mosso.cloudfiles.domain.request;
using NUnit.Framework;

using CreateContainer=com.mosso.cloudfiles.domain.request.CreateContainer;
using DeleteStorageItem=com.mosso.cloudfiles.domain.request.DeleteStorageItem;
using PutStorageItem=com.mosso.cloudfiles.domain.request.PutStorageItem;
using SetStorageItemMetaInformation=com.mosso.cloudfiles.domain.request.SetStorageItemMetaInformation;

namespace com.mosso.cloudfiles.integration.tests
{
    public class TestHelper : IDisposable
    {
        private readonly string authToken;
        private readonly string storageUrl;
        private readonly string containerName;

        public TestHelper(string authToken, string storageUrl, string containerName)
        {
            this.authToken = authToken;
            this.storageUrl = storageUrl;
            this.containerName = containerName;

            CreateContainer();
        }

        public TestHelper(string authToken, string storageUrl) : this(authToken, storageUrl, Constants.CONTAINER_NAME)
        {} 

        public void DeleteItemFromContainer()
        {
            DeleteItemFromContainer(Constants.StorageItemName);
        }

        public void DeleteItemFromContainer(string storageItemName)
        {
            var deleteStorageItem = new DeleteStorageItem(storageUrl, containerName, storageItemName);
            var deleteStorageItemResponse = new GenerateRequestByType().Submit(deleteStorageItem, authToken);
            Assert.That(deleteStorageItemResponse.Status, Is.EqualTo(HttpStatusCode.NoContent));
        }

        public void AddMetadataToItem(string storageItemName)
        {
            var metadata = new Dictionary<string, string> {{"Test", "test"}, {"Test2", "test2"}};
            var setStorageItemMetaInformation = new SetStorageItemMetaInformation(storageUrl, containerName, storageItemName, metadata);
            var postStorageItemResponse = new GenerateRequestByType().Submit(setStorageItemMetaInformation, authToken);
            Assert.That(postStorageItemResponse.Status, Is.EqualTo(HttpStatusCode.Accepted));
            Assert.That(Regex.Match(postStorageItemResponse.Headers["Content-Type"], "text/(plain|html)").Success, Is.True);
            var contentLength = postStorageItemResponse.Headers["Content-Length"];
            Assert.That(contentLength == "58" || contentLength == "0", Is.True);
        }

        public void AddMetadataToItem()
        {
            AddMetadataToItem(Constants.StorageItemName);
        }

        public void PutItemInContainer(string storageItemName, string remoteName)
        {
            var putStorageItem = new PutStorageItem(storageUrl, containerName, remoteName, storageItemName);
            var putStorageItemResponse = new GenerateRequestByType().Submit(putStorageItem, authToken);
            Assert.That(putStorageItemResponse.Status, Is.EqualTo(HttpStatusCode.Created));
        }

        public void PutItemInContainer(string storageItemName)
        {
            PutItemInContainer(storageItemName, storageItemName);
        }

        public void PutItemInContainer()
        {
            PutItemInContainer(Constants.StorageItemName);
        }

        private void CreateContainer()
        {
            var createContainer = new CreateContainer(storageUrl, containerName);
            var putContainerResponse = new GenerateRequestByType().Submit(createContainer, authToken);
            Assert.That(putContainerResponse.Status, Is.EqualTo(HttpStatusCode.Created));
        }

        private void DeleteContainer()
        {
            var deleteContainer = new DeleteContainer(storageUrl, containerName);
            var deleteContainerResponse = new GenerateRequestByType().Submit(deleteContainer, authToken);
            Assert.That(deleteContainerResponse.Status, Is.EqualTo(HttpStatusCode.NoContent));
        }

        public void Dispose()
        {
            DeleteContainer();
        }
    }
}