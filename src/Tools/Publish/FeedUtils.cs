﻿/*
 * Copyright 2010-2014 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NanoByte.Common;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using ZeroInstall.Publish.Properties;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Publish
{
    /// <summary>
    /// Helper methods for manipulating <see cref="Feed"/>s.
    /// </summary>
    public static class FeedUtils
    {
        #region Stylesheet
        /// <summary>
        /// Deploys the files used by the default feed stylesheet to a directory.
        /// </summary>
        /// <param name="path">The directory to deploy the stylesheet files to.</param>
        /// <exception cref="IOException">Failed to write the sytelsheet files.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the directory is not permitted.</exception>
        public static void DeployStylesheet(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            // Write the default XSL with its accompanying CSS file unless there is already an XSL in place
            if (!File.Exists(Path.Combine(path, "feed.xsl")))
            {
                File.WriteAllText(Path.Combine(path, "feed.xsl"), GetEmbeddedResource("feed.xsl"));
                File.WriteAllText(Path.Combine(path, "feed.css"), GetEmbeddedResource("feed.css"));
            }
        }

        private static string GetEmbeddedResource(string name)
        {
            var assembly = Assembly.GetAssembly(typeof(FeedUtils));
            using (var stream = assembly.GetManifestResourceStream(typeof(FeedUtils), name))
                return stream.ReadToString();
        }
        #endregion

        #region Sign
        /// <summary>
        /// Adds a Base64 signature to a feed or catalog file and exports the appropriate public key file in the same directory.
        /// </summary>
        /// <param name="path">The feed or catalog file to sign.</param>
        /// <param name="secretKey">The secret key to use for signing the file.</param>
        /// <param name="passphrase">The passphrase to use to unlock the key.</param>
        /// <param name="openPgp">The OpenPGP-compatible system used to create signatures.</param>
        /// <exception cref="FileNotFoundException">The file could not be found.</exception>
        /// <exception cref="IOException">The OpenPGP implementation could not be launched or the file could not be read or written.</exception>
        /// <exception cref="UnauthorizedAccessException">Read or write access to the file is not permitted.</exception>
        /// <exception cref="WrongPassphraseException">Passphrase was incorrect.</exception>
        /// <remarks>
        /// The file is not parsed before signing; invalid XML files are signed as well.
        /// The existing file must end with a line break.
        /// Old signatures are not removed.
        /// </remarks>
        public static void SignFeed(string path, OpenPgpSecretKey secretKey, string passphrase, IOpenPgp openPgp)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (secretKey == null) throw new ArgumentNullException("secretKey");
            if (openPgp == null) throw new ArgumentNullException("openPgp");
            #endregion

            // Delete any pre-exisiting signature file
            string signatureFile = path + ".sig";
            if (File.Exists(signatureFile)) File.Delete(signatureFile);

            // Create a new signature file, parse it as Base64 and then delete it again
            openPgp.DetachSign(path, secretKey.Fingerprint, passphrase);
            string base64Signature = Convert.ToBase64String(File.ReadAllBytes(signatureFile));
            File.Delete(signatureFile);

            // Add the Base64 encoded signature to the end of the file
            using (var writer = new StreamWriter(path, append: true, encoding: Store.Feeds.FeedUtils.Encoding) {NewLine = "\n"})
            {
                writer.Write(Store.Feeds.FeedUtils.SignatureBlockStart);
                writer.WriteLine(base64Signature);
                writer.Write(Store.Feeds.FeedUtils.SignatureBlockEnd);
            }

            // Export the user's public key
            string feedDir = Path.GetDirectoryName(Path.GetFullPath(path));
            if (feedDir != null)
            {
                using (var atomic = new AtomicWrite(Path.Combine(feedDir, secretKey.KeyID + ".gpg")))
                {
                    File.WriteAllText(atomic.WritePath, openPgp.GetPublicKey(secretKey.Fingerprint), Encoding.ASCII);
                    atomic.Commit();
                }
            }
        }

        /// <summary>
        /// Determines the key used to sign a feed or catalog file. Only uses the first signature if more than one is present.
        /// </summary>
        /// <param name="path">The feed or catalog file to check for signatures.</param>
        /// <param name="openPgp">The OpenPGP-compatible system used to validate the signatures.</param>
        /// <returns>The key used to sign the file; <see langword="null"/> if the file was not signed.</returns>
        /// <exception cref="FileNotFoundException">The file file could not be found.</exception>
        /// <exception cref="IOException">The OpenPGP implementation could not be launched or the file could not be read.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
        public static OpenPgpSecretKey GetKey(string path, IOpenPgp openPgp)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (openPgp == null) throw new ArgumentNullException("openPgp");
            #endregion

            try
            {
                var signatures = Store.Feeds.FeedUtils.GetSignatures(openPgp, File.ReadAllBytes(path));

                foreach (var signature in signatures.OfType<ValidSignature>())
                    return openPgp.GetSecretKey(signature.Fingerprint);
            }
                #region Error handling
            catch (KeyNotFoundException)
            {
                Log.Info(Resources.SecretKeyNotInKeyring);
            }
            catch (SignatureException ex)
            {
                // Unable to parse the signature
                Log.Error(ex);
            }
            #endregion

            return null;
        }
        #endregion
    }
}
