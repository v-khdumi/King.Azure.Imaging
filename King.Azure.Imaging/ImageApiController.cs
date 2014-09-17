﻿namespace King.Azure.Imaging
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Web.Http;

    /// <summary>
    /// Image Api Controller
    /// </summary>
    public class ImageApiController : ApiController
    {
        #region Members
        /// <summary>
        /// Image Preprocessor
        /// </summary>
        protected readonly IPreprocessor preprocessor = null;

        /// <summary>
        /// Imaging
        /// </summary>
        protected readonly IImaging imaging = null;

        /// <summary>
        /// Image Store
        /// </summary>
        protected readonly IDataStore store = null;

        /// <summary>
        /// Image Naming
        /// </summary>
        protected readonly INaming naming = null;
        #endregion

        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        public ImageApiController(string connectionString)
            : this(connectionString, new Preprocessor(connectionString), new StorageElements(), new Naming())
        {

        }

        /// <summary>
        /// Mockable Constructor
        /// </summary>
        public ImageApiController(string connectionString, IPreprocessor preprocessor, IStorageElements elements, INaming naming)
            : this(preprocessor, new Imaging(), new DataStore(connectionString, elements), naming)
        {
        }

        /// <summary>
        /// Mockable Constructor
        /// </summary>
        public ImageApiController(IPreprocessor preprocessor, IImaging imaging, IDataStore store, INaming naming)
        {
            if (null == preprocessor)
            {
                throw new ArgumentException("preprocessor");
            }
            if (null == imaging)
            {
                throw new ArgumentException("imaging");
            }
            if (null == store)
            {
                throw new ArgumentException("store");
            }
            if (null == naming)
            {
                throw new ArgumentException("naming");
            }

            this.preprocessor = preprocessor;
            this.imaging = imaging;
            this.store = store;
            this.naming = naming;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Post new file to the server
        /// </summary>
        /// <returns>Task</returns>
        [HttpPost]
        public virtual async Task<HttpResponseMessage> Post()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return new HttpResponseMessage(HttpStatusCode.UnsupportedMediaType);
            }

            var provider = new MultipartMemoryStreamProvider();
            await this.Request.Content.ReadAsMultipartAsync(provider);

            foreach (var file in provider.Contents)
            {
                var bytes = await file.ReadAsByteArrayAsync();

                await this.preprocessor.Process(bytes
                    , file.Headers.ContentType.MediaType
                    , file.Headers.ContentDisposition.FileName.Trim('\"'));
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        /// <summary>
        /// Get a specific file from Blob storage
        /// </summary>
        /// <param name="file">file name</param>
        /// <returns>File</returns>
        [HttpGet]
        public virtual async Task<HttpResponseMessage> Get(string file)
        {
            if (string.IsNullOrWhiteSpace(file))
            {
                return new HttpResponseMessage(HttpStatusCode.PreconditionFailed)
                {
                    ReasonPhrase = "file must be specified",
                };
            }

            var streamer = this.store.Streamer;
            var stream = await streamer.Get(file);
            var response = new HttpResponseMessage
            {
                Content = new StreamContent(stream),
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue(streamer.ContentType);
            return response;
        }

        /// <summary>
        /// Resize Image on the fly
        /// </summary>
        /// <remarks>
        /// Format and Cache are not wired in yet, soon
        /// </remarks>
        /// <returns>Image (Resized)</returns>
        [HttpGet]
        public virtual async Task<HttpResponseMessage> Resize(string file, int width, int height = 0, string format = Naming.DefaultExtension, int quality = 85, bool cache = false)
        {
            if (string.IsNullOrWhiteSpace(file))
            {
                return new HttpResponseMessage(HttpStatusCode.PreconditionFailed)
                {
                    ReasonPhrase = "file must be specified",
                };
            }
            if (0 > width)
            {
                return new HttpResponseMessage(HttpStatusCode.PreconditionFailed)
                {
                    ReasonPhrase = "width less than 0",
                };
            }
            if (0 > height)
            {
                return new HttpResponseMessage(HttpStatusCode.PreconditionFailed)
                {
                    ReasonPhrase = "height less than 0",
                };
            }
            if (0 >= width && 0 >= height)
            {
                return new HttpResponseMessage(HttpStatusCode.PreconditionFailed)
                {
                    ReasonPhrase = "width and height less than or equal to 0",
                };
            }

            var wasCached = false;
            var imgFormat = this.imaging.Get(format, quality);

            var identifier = this.naming.FromFileName(file);
            var versionName = this.naming.DynamicVersion(imgFormat.DefaultExtension, quality, width, height);
            var cachedFileName = this.naming.FileName(identifier, versionName, imgFormat.DefaultExtension);

            byte[] resized = null;
            var streamer = this.store.Streamer;

            if (cache)
            {
                resized = await streamer.GetBytes(cachedFileName);
                wasCached = null != resized;
            }

            if (!wasCached)
            {
                var version = new ImageVersion
                {
                    Height = height,
                    Width = width,
                    Format = imgFormat,
                };

                var toResize = await streamer.GetBytes(file);
                resized = this.imaging.Resize(toResize, version);
            }

            var response = new HttpResponseMessage
            {
                Content = new StreamContent(new MemoryStream(resized)),
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(imgFormat.MimeType);

            if (cache && !wasCached)
            {
                await this.store.Save(cachedFileName, resized, versionName, imgFormat.MimeType, identifier, false, null, quality);
            }

            return response;
        }
        #endregion
    }
}