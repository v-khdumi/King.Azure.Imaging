﻿namespace King.Azure.Imaging.Models
{
    using System;

    /// <summary>
    /// Raw Uploaded Data
    /// </summary>
    public class RawData
    {
        #region Properties
        /// <summary>
        /// Image Contents
        /// </summary>
        public byte[] Contents
        {
            get;
            set;
        }

        /// <summary>
        /// Content Type
        /// </summary>
        public string ContentType
        {
            get;
            set;
        }

        /// <summary>
        /// File Name
        /// </summary>
        public string FileName
        {
            get;
            set;
        }

        /// <summary>
        /// Original File Name
        /// </summary>
        public string OriginalFileName
        {
            get;
            set;
        }

        /// <summary>
        /// File Size
        /// </summary>
        public long FileSize
        {
            get;
            set;
        }

        /// <summary>
        /// Identifier
        /// </summary>
        public Guid Identifier
        {
            get;
            set;
        }
        #endregion
    }
}