using System;
using System.Text.RegularExpressions;
using System.Xml;
using NUnit.Framework;

using System.Collections.Generic;
using Rackspace.CloudFiles.Domain;
using Rackspace.CloudFiles.Exceptions;

namespace Rackspace.CloudFiles.Integration.Tests.ConnectionSpecs.GetContainerInformationSpecs
{
    [TestFixture]
    public class When_requesting_information_on_a_container_using_connection : TestBase
    {
        [Test]
        public void Should_return_container_information_when_the_container_exists()
        {

            string containerName = Guid.NewGuid().ToString();
            try
            {
                connection.CreateContainer(containerName);
                Container containerInformation = connection.GetContainerInformation(containerName);

                Assert.That(containerInformation.Name, Is.EqualTo(containerName));
                Assert.That(containerInformation.ByteCount, Is.EqualTo(0));
                Assert.That(containerInformation.ObjectCount, Is.EqualTo(0));
                if(connection.HasCDN())
                    Assert.That(containerInformation.CdnUri, Is.EqualTo(""));
            }
            finally
            {
                connection.DeleteContainer(containerName);
            }
        }

        [Test]
        public void Should_throw_an_exception_when_the_container_does_not_exist()
        {
            Assert.Throws<ContainerNotFoundException>(() => connection.GetContainerInformation(Constants.CONTAINER_NAME));
        }
    }

    [TestFixture]
    public class When_getting_serialized_container_information_for_a_container_in_json_format_and_objects_exist : TestBase
    {
		private string jsonReturnValue;
	 
		protected override void  SetUp()
		{
			  connection.CreateContainer(Constants.CONTAINER_NAME);

            try
            {
                connection.PutStorageItem(Constants.CONTAINER_NAME, Constants.StorageItemNameJpg);
                jsonReturnValue = connection.GetContainerInformationJson(Constants.CONTAINER_NAME);
              
            }
            finally
            {
                connection.DeleteStorageItem(Constants.CONTAINER_NAME, Constants.StorageItemNameJpg);
                connection.DeleteContainer(Constants.CONTAINER_NAME);
            }
		}
        [Test]
        public void Should_get_serialized_json_format()
        {
            var expectedSubString = "[{\"name\":[ ]?\"" + Constants.StorageItemNameJpg + "\",[ ]?\"hash\":[ ]?\"b44a59383b3123a747d139bd0e71d2df\",[ ]?\"bytes\":[ ]?\\d+,[ ]?\"content_type\":[ ]?\"image.*jpeg\",[ ]?\"last_modified\":[ ]?\"" + String.Format("{0:yyyy-MM}", DateTime.Now);
            Assert.That(Regex.Match(jsonReturnValue, expectedSubString).Success, Is.True);
        }
    }

    [TestFixture]
    public class When_getting_serialized_container_information_for_a_container_in_xml_format_and_objects_exist : TestBase
    {
		private XmlDocument xmlReturnValue;
		 
		protected override void  SetUp()
        {
            connection.CreateContainer(Constants.CONTAINER_NAME);

            try
            {
				var dict = new Dictionary<string,string> 
                {{"X-User-Agent-ACL", "Mozilla"}, {"X-Referrer-ACL", "testdomain.com"}};
                connection.PutStorageItem(Constants.CONTAINER_NAME, Constants.StorageItemNameJpg, dict);
                xmlReturnValue = connection.GetContainerInformationXml(Constants.CONTAINER_NAME);
                Console.WriteLine(xmlReturnValue.InnerXml);

            }
            finally
            {
                connection.DeleteStorageItem(Constants.CONTAINER_NAME, Constants.StorageItemNameJpg);
                connection.DeleteContainer(Constants.CONTAINER_NAME);
            }
		
		}
		[Test]
		public void should_have_serialized_xml()
		{

            var expectedSubString = "<container name=\"" + Constants.CONTAINER_NAME + "\"><object><name>" + Constants.StorageItemNameJpg + "</name><hash>b44a59383b3123a747d139bd0e71d2df</hash><bytes>105542</bytes><content_type>image/jpeg</content_type><last_modified>" + String.Format("{0:yyyy-MM}", DateTime.Now);

            Assert.That(Regex.Match(xmlReturnValue.InnerXml, expectedSubString).Success, Is.True);
		
		}

    }

    [TestFixture]
    public class When_creating_a_new_container_with_metadata : TestBase
    {
        [Test]
        public void Should_create_container()
        {

            Container container;

            try
            {
                connection.CreateContainer(Constants.CONTAINER_NAME, new Dictionary<string, string>
                                                                         {
                                                                             {Constants.MetadataKey, Constants.MetadataValue}
                                                                         });
                container = connection.GetContainerInformation(Constants.CONTAINER_NAME);

            }
            finally
            {
                connection.DeleteContainer(Constants.CONTAINER_NAME);
            }

            Assert.That(container.Metadata.Count, Is.GreaterThan(1));  //can't be exact due to caching
            Assert.That(container.Metadata[Constants.MetadataKey], Is.EqualTo(Constants.MetadataValue));
        }

        [Test, Ignore("The Keys are being capitalized (first letter capital, remaining lowercase).  Issue #42")]
        public void Should_get_metadata_keys_with_same_casing_that_was_submitted_originally()
        {

            Container container;

            try
            {
                connection.CreateContainer(Constants.CONTAINER_NAME, new Dictionary<string, string>
                                                                         {
                                                                             {"UserID", Constants.MetadataValue},
                                                                             {"UserFriendlyName", Constants.MetadataValue},
                                                                             {"ALLCAPSKEY", Constants.MetadataValue}
                                                                         });
                container = connection.GetContainerInformation(Constants.CONTAINER_NAME);

            }
            finally
            {
                connection.DeleteContainer(Constants.CONTAINER_NAME);
            }

            Assert.That(container.Metadata.Count, Is.EqualTo(3));
            Assert.That(container.Metadata["UserID"], Is.EqualTo(Constants.MetadataValue));
            Assert.That(container.Metadata["UserFriendlyName"], Is.EqualTo(Constants.MetadataValue));
            Assert.That(container.Metadata["ALLCAPSKEY"], Is.EqualTo(Constants.MetadataValue));
        }
    }
	 
}