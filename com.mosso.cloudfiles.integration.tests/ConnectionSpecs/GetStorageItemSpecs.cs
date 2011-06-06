using System;
using System.Collections.Generic;
using System.Threading;
using com.mosso.cloudfiles.domain;
using com.mosso.cloudfiles.domain.request;
using com.mosso.cloudfiles.exceptions;
using NUnit.Framework;


namespace com.mosso.cloudfiles.integration.tests.ConnectionSpecs.GetStorageItemSpecs
{
    [TestFixture]
    public class When_downloading_a_file_using_connection : TestBase
    {
        [Test]
        public void Should_return_large_file_if_predetermined_container_exists_and_file_exists()
        {
            var containers = connection.GetContainers();
            if(!containers.Contains(Constants.CONTAINER_NAME_WITH_LARGE_OBJECT) || 
                !connection.GetContainerItemList(Constants.CONTAINER_NAME_WITH_LARGE_OBJECT)
                    .Contains(Constants.StorageItemNameLarge))
            {
                Assert.Ignore(string.Format("The conventional container {0} or large file {1} does not exist.",
                              Constants.CONTAINER_NAME_WITH_LARGE_OBJECT,
                              Constants.StorageItemNameLarge));
            }

            Console.WriteLine(string.Format("Beginning download of {0} file", Constants.StorageItemNameLarge));
            connection.GetStorageItem(Constants.CONTAINER_NAME_WITH_LARGE_OBJECT, Constants.StorageItemNameLarge, Constants.StorageItemNameLarge);
            Console.WriteLine(string.Format("Finished download of {0} file", Constants.StorageItemNameLarge));

        }

        [Test]
        public void Should_return_nothing_if_a_local_file_name_is_supplied_and_the_download_is_successful()
        {
            
            try
            {
                connection.CreateContainer(Constants.CONTAINER_NAME);
                connection.PutStorageItem(Constants.CONTAINER_NAME, Constants.StorageItemName);
                connection.GetStorageItem(Constants.CONTAINER_NAME, Constants.StorageItemName, Constants.StorageItemName);
            }
            finally
            {
                connection.DeleteStorageItem(Constants.CONTAINER_NAME, Constants.StorageItemName);
                connection.DeleteContainer(Constants.CONTAINER_NAME);
            }
        }

        [Test]
        public void Should_return_a_non_null_storage_object_when_no_local_file_name_is_supplied_and_the_download_is_successful()
        {
            
            StorageItem storageItem;
            try
            {
                connection.CreateContainer(Constants.CONTAINER_NAME);
                connection.PutStorageItem(Constants.CONTAINER_NAME, Constants.StorageItemName);
                storageItem = connection.GetStorageItem(Constants.CONTAINER_NAME, Constants.StorageItemName);
            }
            finally
            {
                connection.DeleteStorageItem(Constants.CONTAINER_NAME, Constants.StorageItemName);
                connection.DeleteContainer(Constants.CONTAINER_NAME);
            }

            Assert.That(storageItem, Is.Not.Null);
            Assert.That(storageItem.ObjectStream, Is.Not.Null);
            Assert.That(storageItem.ObjectStream.CanRead, Is.EqualTo(true));
            storageItem.Dispose();
        }

        [Test]
        public void Should_throw_an_exception_when_the_file_does_not_exist()
        {
            
            bool exceptionWasThrown = false;

            try
            {
                connection.CreateContainer(Constants.CONTAINER_NAME);
                connection.GetStorageItem(Constants.CONTAINER_NAME, Constants.StorageItemName);
            }
            catch (Exception exception)
            {
                Assert.That(exception.GetType(), Is.EqualTo(typeof (StorageItemNotFoundException)));
                exceptionWasThrown = true;
            }
            finally
            {
                connection.DeleteContainer(Constants.CONTAINER_NAME);
            }
            Assert.That(exceptionWasThrown, Is.True);
        }

        [Test]
        public void Should_return_partial_content_when_optional_range_header_is_specified()
        {
            
            StorageItem si = null;
            try
            {
                var requestHeaders = new Dictionary<RequestHeaderFields, string>();
                requestHeaders.Add(RequestHeaderFields.Range, "0-5");
                connection.CreateContainer(Constants.CONTAINER_NAME);
                connection.PutStorageItem(Constants.CONTAINER_NAME, Constants.StorageItemName);
                si = connection.GetStorageItem(Constants.CONTAINER_NAME, Constants.StorageItemName, requestHeaders);
            }
            finally
            {
                if (si != null) si.Dispose();
                connection.DeleteStorageItem(Constants.CONTAINER_NAME, Constants.StorageItemName);
                connection.DeleteContainer(Constants.CONTAINER_NAME);
            }
        }
    }

    [TestFixture]
    public class When_getting_storage_item : TestBase
    {
        [Test, Ignore]
        public void Should_return_a_storage_item()
        {
            

            Dictionary<string, string> metadata = new Dictionary<string, string>
                                                      {
                                                          {Constants.MetadataKey, Constants.MetadataValue}
                                                      };

            StorageItem storageItem;
            try
            {
                connection.CreateContainer(Constants.CONTAINER_NAME);
                connection.PutStorageItem(Constants.CONTAINER_NAME, Constants.StorageItemName);
                connection.SetStorageItemMetaInformation(Constants.CONTAINER_NAME, Constants.StorageItemName, metadata);
                storageItem = connection.GetStorageItem(Constants.CONTAINER_NAME, Constants.StorageItemName);
            }
            finally
            {
                connection.DeleteStorageItem(Constants.CONTAINER_NAME, Constants.StorageItemName);
                connection.DeleteContainer(Constants.CONTAINER_NAME);
            }

            Assert.That(storageItem, Is.Not.Null);
            Assert.That(storageItem.Metadata[Constants.MetadataKey], Is.EqualTo(Constants.MetadataValue));
        }


    }

    [TestFixture]
    public class When_getting_storage_item_and_item_does_not_exist : TestBase
    {
        [Test]
        public void Should_throw_a__storage_item_not_found_exception()
        {
            
            connection.CreateContainer(Constants.CONTAINER_NAME);

            try
            {
                connection.GetStorageItem(Constants.CONTAINER_NAME, Constants.StorageItemName);
            }
            catch (Exception ex)
            {
                Assert.That(ex.GetType(), Is.EqualTo(typeof(StorageItemNotFoundException)));
            }
            finally
            {
                connection.DeleteContainer(Constants.CONTAINER_NAME);
            }
        }
    }

    [TestFixture]
    public class When_getting_a_storage_item_asynchronously : TestBase
    {
        private bool operationCompleted;
        private bool gotProgress;

        [Test]
        public void Should_receive_progress_events_when_the_transfer_is_successful()
        {
            try
            {
                var progressCallback = new Connection.ProgressCallback(callback);
                var operationCompleteCallback = new Connection.OperationCompleteCallback(operationComplete);

                ((Connection)connection).OperationComplete += operationCompleteCallback;
                ((Connection)connection).AddProgressWatcher(progressCallback);

                connection.CreateContainer(Constants.CONTAINER_NAME);
                connection.PutStorageItem(Constants.CONTAINER_NAME, Constants.StorageItemName);
                connection.GetStorageItemAsync(Constants.CONTAINER_NAME, Constants.StorageItemName, Constants.StorageItemName);

                Thread.Sleep(3000);
            }
            catch (Exception)
            {
                connection.DeleteStorageItem(Constants.CONTAINER_NAME, Constants.StorageItemName);
                connection.DeleteContainer(Constants.CONTAINER_NAME);
            }
        }

        [TearDown]
        public void Teardown()
        {
            Assert.IsTrue(gotProgress);
            Assert.IsTrue(operationCompleted);
        }

        private void operationComplete()
        {
            connection.DeleteStorageItem(Constants.CONTAINER_NAME, Constants.StorageItemName);
            connection.DeleteContainer(Constants.CONTAINER_NAME);
            operationCompleted = true;
        }

        private void callback(int xfer)
        {
            if (xfer > 0)
            {
                gotProgress = true;
            }
        }
    }
}