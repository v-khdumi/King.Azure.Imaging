﻿namespace King.Azure.Imaging
{
    using King.Azure.Data;
    using King.Azure.Imaging.Entities;
    using King.Azure.Imaging.Models;
    using Newtonsoft.Json;
    using System;
    using King.Mapper;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage.Queue;

    public class ImagePreProcessor : IImagePreProcessor
    {
        #region Members
        /// <summary>
        /// File Name Header
        /// </summary>
        public const string FileNameHeader = "X-File-Name";

        /// <summary>
        /// Content Type Header
        /// </summary>
        public const string ContentTypeHeader = "X-File-Type";

        /// <summary>
        /// Blob Container
        /// </summary>
        private readonly IContainer container = null;

        /// <summary>
        /// Table
        /// </summary>
        private readonly ITableStorage table = null;

        /// <summary>
        /// Storage Queue
        /// </summary>
        private readonly IStorageQueue queue = null;
        #endregion

        #region Methods
        public ImagePreProcessor(string connectionString)
        {
            this.container = new Container("", connectionString);
            this.table = new TableStorage("", connectionString);
            this.queue = new StorageQueue("", connectionString);
        }
        #endregion

        #region Methods
        public async Task Process(byte[] content, string contentType, string fileName)
        {
            var data = new RawData()
            {
                Contents = content,
                Identifier = Guid.NewGuid(),
                ContentType = contentType,
                FileName = fileName,
            };
            data.FileSize = data.Contents.Length;

            var entity = data.Map<ImageEntity>();
            entity.PartitionKey = "original";
            entity.RowKey = data.Identifier.ToString();
            await table.InsertOrReplace(entity);

            var toQueue = data.Map<ImageQueued>();
            await this.queue.Save(new CloudQueueMessage(JsonConvert.SerializeObject(toQueue)));

            await container.Save(data.Identifier.ToString(), data.Contents, data.ContentType);
        }
        #endregion
    }
}