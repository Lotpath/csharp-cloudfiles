using System;
using com.mosso.cloudfiles.domain.request;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace com.mosso.cloudfiles.unit.tests.Domain.request.GetStorageItemInformationSpecs
{
    [TestFixture]
    public class when_getting_information_of_a_storage_item_and_storage_url_is_null
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void should_throw_argument_null_exception()
        {
            new GetStorageItemInformation(null, "storagetoken", "containername", "storageitemname");
        }
    }

    [TestFixture]
    public class when_getting_information_of_a_storage_item_and_storage_url_is_emptry_string
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void should_throw_argument_null_exception()
        {
            new GetStorageItemInformation("", "storagetoken", "containername", "storageitemname");
        }
    }

    [TestFixture]
    public class when_getting_information_of_a_storage_item_and_storage_token_is_null
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void should_throw_argument_null_exception()
        {
            new GetStorageItemInformation("http://storageurl", null ,"containername", "storageitemname");
        }
    }

    [TestFixture]
    public class when_getting_information_of_a_storage_item_and_storage_token_is_emptry_string
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void should_throw_argument_null_exception()
        {
            new GetStorageItemInformation("http://storageurl", "", "containername", "storageitemname");
        }
    }

    [TestFixture]
    public class when_getting_information_of_a_storage_item_and_container_name_is_null
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void should_throw_argument_null_exception()
        {
            new GetStorageItemInformation("http://storageurl", "storagetoken", null, "storageitemname");
        }
    }

    [TestFixture]
    public class when_getting_information_of_a_storage_item_and_container_name_is_emptry_string
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void should_throw_argument_null_exception()
        {
            new GetStorageItemInformation("http://storageurl", "storagetoken", "", "storageitemname");
        }
    }

    [TestFixture]
    public class when_getting_information_of_a_storage_item_and_storage_item_name_is_null
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void should_throw_argument_null_exception()
        {
            new GetStorageItemInformation("http://storageurl", "storagetoken", "containername", null);
        }
    }

    [TestFixture]
    public class when_getting_information_of_a_storage_item_and_storage_item_name_is_emptry_string
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void should_throw_argument_null_exception()
        {
            new GetStorageItemInformation("http://storageurl", "storagetoken", "containername", "");
        }
    }

    [TestFixture]
    public class when_getting_information_of_a_storage_item
    {
        private GetStorageItemInformation getStorageItemInformation;

        [SetUp]
        public void setup()
        {
            getStorageItemInformation = new GetStorageItemInformation("http://storageurl", "storagetoken", "containername", "storageitemname");
        }

        [Test]
        public void should_have_properly_formmated_request_url()
        {
            Assert.That(getStorageItemInformation.Uri.ToString(), Is.EqualTo("http://storageurl/containername/storageitemname"));
        }

        [Test]
        public void should_have_a_http_head_method()
        {
            Assert.That(getStorageItemInformation.Method, Is.EqualTo("HEAD"));
        }

        [Test]
        public void should_have_an_storage_token_in_the_headers()
        {
            Assert.That(getStorageItemInformation.Headers[cloudfiles.Constants.X_STORAGE_TOKEN], Is.EqualTo("storagetoken"));
        }
    }

}