﻿namespace King.Azure.Imaging
{
    using System;
    using System.Linq;

    /// <summary>
    /// Image Naming Convensions
    /// </summary>
    public class Naming : INaming
    {
        #region Members
        /// <summary>
        /// File Name Format
        /// </summary>
        public const string FileNameFormat = "{0}_{1}.{2}";

        /// <summary>
        /// Dynamic Version Format
        /// </summary>
        public const string DynamicVersionFormat = "{0}_{1}_{2}x{3}";

        /// <summary>
        /// Original
        /// </summary>
        public const string Original = "original";

        /// <summary>
        /// Default Extension
        /// </summary>
        public const string DefaultExtension = "jpeg";

        /// <summary>
        /// Path Format
        /// </summary>
        public const string PathFormat = "{0}/{1}";
        #endregion

        #region Methods
        /// <summary>
        /// Naming for dynamic versions, enforce file uniqueness
        /// </summary>
        /// <param name="extension">Extension</param>
        /// <param name="quality">Quality</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <returns>Name</returns>
        public virtual string DynamicVersion(string extension, int quality, int width, int height)
        {
            return string.Format(DynamicVersionFormat, extension, quality, width, height).ToLowerInvariant();
        }

        /// <summary>
        /// File name
        /// </summary>
        /// <param name="identifier">Identifier</param>
        /// <param name="version">Version</param>
        /// <param name="extension">Extension</param>
        /// <returns>Name</returns>
        public virtual string FileName(Guid identifier, string version, string extension)
        {
            return string.Format(FileNameFormat, identifier, version, extension).ToLowerInvariant();
        }

        /// <summary>
        /// Partial File Name
        /// </summary>
        /// <param name="identifier">Identifier</param>
        /// <returns>Partial Name Format</returns>
        public virtual string FileNamePartial(Guid identifier)
        {
            return string.Format(FileNameFormat, identifier, "{0}", "{1}").ToLowerInvariant();
        }

        /// <summary>
        /// Identifier From File Name
        /// </summary>
        /// <param name="fileName">File Name</param>
        /// <returns>Identifier</returns>
        public virtual Guid FromFileName(string fileName)
        {
            return Guid.Parse(fileName.Substring(0, fileName.IndexOf('_')));
        }

        /// <summary>
        /// Extension
        /// </summary>
        /// <param name="fileName">File Name</param>
        /// <returns>Extension</returns>
        public virtual string Extension(string fileName)
        {
            return fileName.Contains('.') ? fileName.Substring(fileName.LastIndexOf('.') + 1).ToLowerInvariant() : DefaultExtension;
        }

        /// <summary>
        /// Relative Path
        /// </summary>
        /// <param name="folder">Folder</param>
        /// <param name="file">File</param>
        /// <returns>Relative Path</returns>
        public virtual string RelativePath(string folder, string file)
        {
            return string.Format(PathFormat, folder, file).ToLowerInvariant();
        }
        #endregion
    }
}