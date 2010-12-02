//----------------------------------------------
// See COPYING file for licensing information
//----------------------------------------------

namespace com.mosso.cloudfiles.domain.request
{
    #region Using
    using System;
    using com.mosso.cloudfiles.domain.request.Interfaces;
    using com.mosso.cloudfiles.exceptions;
    using com.mosso.cloudfiles.utils;
    #endregion

    /// <summary>
    /// A class to represent deleting a storage item in a web request
    /// </summary>
    public class DeleteStorageItem : IAddToWebRequest
    {
        private readonly string _storageUrl;
        private readonly string _containerName;
        private readonly string _storageItemName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteStorageItem"/> class.
        /// </summary>
        /// <param name="storageUrl">the customer unique url to interact with cloudfiles</param>
        /// <param name="containerName">the name of the container where the storage item is located</param>
        /// <param name="storageItemName">the name of the storage item to add meta information too</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the reference parameters are null</exception>
        /// <exception cref="ContainerNameException">Thrown when the container name is invalid</exception>
        /// <exception cref="StorageItemNameException">Thrown when the object name is invalid</exception>
        public DeleteStorageItem(string storageUrl,  string containerName, string storageItemName)
        {
            if (string.IsNullOrEmpty(storageUrl)
            || string.IsNullOrEmpty(containerName)
            || string.IsNullOrEmpty(storageItemName))
            {
                throw new ArgumentNullException();
            }

            if (!ContainerNameValidator.Validate(containerName))
            {
                throw new ContainerNameException();
            }

            if (!ObjectNameValidator.Validate(storageItemName))
            {
                throw new StorageItemNameException();
            }

            _storageUrl = storageUrl;
            _containerName = containerName;
            _storageItemName = storageItemName;
        }

        /// <summary>
        /// Creates the URI.
        /// </summary>
        /// <returns>A new URI for this container</returns>
        public Uri CreateUri()
        {
            return new Uri(_storageUrl + "/" + _containerName.Encode() + "/" + _storageItemName.StripSlashPrefix().Encode());
        }

        /// <summary>
        /// Applies the apropiate method to the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        public void Apply(ICloudFilesRequest request)
        {
            request.Method = "DELETE";
        }
    }
}