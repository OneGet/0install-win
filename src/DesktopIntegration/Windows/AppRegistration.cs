﻿/*
 * Copyright 2010-2011 Bastian Eicher
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
using System.Globalization;
using System.IO;
using System.Net;
using Common;
using Common.Collections;
using Common.Tasks;
using Microsoft.Win32;
using ZeroInstall.Model;
using ZeroInstall.Model.Capabilities;
using Capabilities = ZeroInstall.Model.Capabilities;

namespace ZeroInstall.DesktopIntegration.Windows
{
    /// <summary>
    /// Contains control logic for applying <see cref="Capabilities.AppRegistration"/> on Windows systems.
    /// </summary>
    public static class AppRegistration
    {
        #region Constants
        /// <summary>Prepended before <see cref="Capabilities.AppRegistration.CapabilityRegPath"/>. This prevents conflicts with non-Zero Install installations.</summary>
        private const string CapabilityPrefix = @"Zero Install\Applications\";

        /// <summary>The HKLM registry key for registering applications as candidates for default programs.</summary>
        public const string RegKeyMachineRegisteredApplications = @"SOFTWARE\RegisteredApplications";

        /// <summary>The registry value name for the application name.</summary>
        public const string RegValueAppName = "ApplicationName";

        /// <summary>The registry value name for the application description.</summary>
        public const string RegValueAppDescription = "ApplicationDescription";

        /// <summary>The registry value name for the application icon.</summary>
        public const string RegValueAppIcon = "ApplicationIcon";

        /// <summary>The registry subkey containing <see cref="Capabilities.FileType"/> references.</summary>
        public const string RegSubKeyFileAssocs = "FileAssociations";

        /// <summary>The registry subkey containing <see cref="Capabilities.UrlProtocol"/> references.</summary>
        public const string RegSubKeyUrlAssocs = "URLAssociations";

        /// <summary>The registry subkey containing <see cref="Capabilities.DefaultProgram"/> references.</summary>
        public const string RegSubKeyStartMenu = "StartMenu";
        #endregion

        #region Apply
        /// <summary>
        /// Applies application registration to the current Windows system. This can only be applied system-wide, not per user.
        /// </summary>
        /// <param name="target">The application being integrated.</param>
        /// <param name="appRegistration">The registration information to be applied.</param>
        /// <param name="verbCapabilities">The capabilities that the application is to be registered with.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <exception cref="UserCancelException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if the data in <paramref name="appRegistration"/> or <paramref name="verbCapabilities"/> is invalid.</exception>
        public static void Apply(InterfaceFeed target, Capabilities.AppRegistration appRegistration, IEnumerable<VerbCapability> verbCapabilities, ITaskHandler handler)
        {
            #region Sanity checks
            if (appRegistration == null) throw new ArgumentNullException("appRegistration");
            if (verbCapabilities == null) throw new ArgumentNullException("verbCapabilities");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            if (string.IsNullOrEmpty(appRegistration.ID)) throw new InvalidDataException("Missing ID");
            if (string.IsNullOrEmpty(appRegistration.CapabilityRegPath)) throw new InvalidDataException("Invalid CapabilityRegPath");

            // ToDo: Handle appRegistration.X64
            using (var capabilitiesKey = Registry.LocalMachine.CreateSubKey(CapabilityPrefix + appRegistration.CapabilityRegPath))
            {
                capabilitiesKey.SetValue(RegValueAppName, target.Feed.Name ?? "");
                capabilitiesKey.SetValue(RegValueAppDescription, target.Feed.Descriptions.GetBestLanguage(CultureInfo.CurrentCulture) ?? "");

                // Set icon if available
                try
                {
                    var icon = target.Feed.GetIcon(Icon.MimeTypeIco, null);
                    capabilitiesKey.SetValue(RegValueAppIcon, IconProvider.GetIconPath(icon, true, handler) + ",0");
                }
                catch (KeyNotFoundException)
                {}

                using (var fileAssocsKey = capabilitiesKey.CreateSubKey(RegSubKeyFileAssocs))
                {
                    foreach (var fileType in EnumerableUtils.OfType<Capabilities.FileType>(verbCapabilities))
                    {
                        foreach (var extension in fileType.Extensions)
                        {
                            if (!string.IsNullOrEmpty(extension.Value) && !string.IsNullOrEmpty(fileType.ID))
                                fileAssocsKey.SetValue(extension.Value, FileType.RegKeyPrefix + fileType.ID);
                        }
                    }
                }

                using (var urlAssocsKey = capabilitiesKey.CreateSubKey(RegSubKeyUrlAssocs))
                {
                    foreach (var urlProtocol in EnumerableUtils.OfType<Capabilities.UrlProtocol>(verbCapabilities))
                    {
                        foreach (var prefix in urlProtocol.KnownPrefixes)
                            urlAssocsKey.SetValue(prefix.Value, FileType.RegKeyPrefix + urlProtocol.ID);
                    }
                }

                using (var startMenuKey = capabilitiesKey.CreateSubKey(RegSubKeyStartMenu))
                {
                    foreach (var defaultProgram in EnumerableUtils.OfType<Capabilities.DefaultProgram>(verbCapabilities))
                    {
                        if (!string.IsNullOrEmpty(defaultProgram.ID) && !string.IsNullOrEmpty(defaultProgram.Service))
                            startMenuKey.SetValue(defaultProgram.Service, defaultProgram.ID);
                    }
                }
            }

            using (var regAppsKey = Registry.LocalMachine.CreateSubKey(RegKeyMachineRegisteredApplications))
                regAppsKey.SetValue(appRegistration.ID, CapabilityPrefix + appRegistration.CapabilityRegPath);
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes application registration from the current Windows system. This can only be applied system-wide, not per user.
        /// </summary>
        /// <param name="appRegistration">The registration information to be removed.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if the data in <paramref name="appRegistration"/>.</exception>
        public static void Remove(Capabilities.AppRegistration appRegistration)
        {
            #region Sanity checks
            if (appRegistration == null) throw new ArgumentNullException("appRegistration");
            #endregion

            if (string.IsNullOrEmpty(appRegistration.ID)) throw new InvalidDataException("Missing ID");
            if (string.IsNullOrEmpty(appRegistration.CapabilityRegPath)) throw new InvalidDataException("Invalid CapabilityRegPath");

            using (var regAppsKey = Registry.LocalMachine.CreateSubKey(RegKeyMachineRegisteredApplications))
                regAppsKey.DeleteValue(appRegistration.ID, false);

            // ToDo: Handle appRegistration.X64
            try
            {
                Registry.LocalMachine.DeleteSubKeyTree(CapabilityPrefix + appRegistration.CapabilityRegPath);
            }
            catch (ArgumentException)
            {} // Ignore missing registry keys
        }
        #endregion
    }
}