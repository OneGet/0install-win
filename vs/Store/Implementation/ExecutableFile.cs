﻿using System;
using System.Globalization;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// An immutable executable file-entry in a <see cref="Manifest"/>.
    /// </summary>
    public sealed class ExecutableFile : FileBase, IEquatable<ExecutableFile>
    {
        #region Constructor
        /// <summary>
        /// Creates a new executable file entry.
        /// </summary>
        /// <param name="hash">The hash of the content of the file calculated using the selected digest algorithm.</param>
        /// <param name="modifiedTime">The time this file was last modified in the number of seconds since the epoch.</param>
        /// <param name="size">The size of the file in bytes.</param>
        /// <param name="fileName">The name of the file without the containing directory.</param>
        public ExecutableFile(string hash, long modifiedTime, long size, string fileName) : base(hash, modifiedTime, size, fileName)
        {}
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the string representation of this node for the manifest format.
        /// </summary>
        /// <returns><code>"X", space, hash, space, mtime, space, size, space, file name, newline</code></returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "X {0} {1} {2} {3}", Hash, Size, ModifiedTime, FileName);
        }

        /// <summary>
        /// Creates a new node from a string representation as created by <see cref="ToString"/>.
        /// </summary>
        /// <param name="line">The string representation to parse.</param>
        /// <returns>The newly created node.</returns>
        public static ExecutableFile FromString(string line)
        {
            string[] parts = line.Split(new[] { ' ' }, 5);
            if (parts.Length != 5) throw new ArgumentException(Resources.InvalidNumberOfLineParts, "line");
            return new ExecutableFile(parts[1], long.Parse(parts[2]), long.Parse(parts[3]), parts[4]);
        }
        #endregion

        #region Compare
        public bool Equals(ExecutableFile other)
        {
            return base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(obj, this)) return true;
            return obj.GetType() == typeof(ExecutableFile) && Equals((ExecutableFile)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(ExecutableFile left, ExecutableFile right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ExecutableFile left, ExecutableFile right)
        {
            return !Equals(left, right);
        }
        #endregion
    }
}
