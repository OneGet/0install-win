﻿/*
 * Copyright 2010-2015 Bastian Eicher
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
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows.Forms;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Tasks;
using ZeroInstall.Publish.Properties;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Publish.WinForms.Wizards
{
    internal partial class DownloadPage : UserControl
    {
        public event Action AsArchive;
        public event Action AsSingleFile;
        public event Action AsInstaller;

        private readonly FeedBuilder _feedBuilder;
        private readonly InstallerCapture _installerCapture;

        public DownloadPage([NotNull] FeedBuilder feedBuilder, [NotNull] InstallerCapture installerCapture)
        {
            InitializeComponent();

            _feedBuilder = feedBuilder;
            _installerCapture = installerCapture;
        }

        private void ToggleControls(object sender, EventArgs e)
        {
            groupLocalCopy.Enabled = checkLocalCopy.Checked;

            buttonNext.Enabled =
                (textBoxUrl.Text.Length > 0) && textBoxUrl.IsValid &&
                (!checkLocalCopy.Checked || textBoxLocalPath.Text.Length > 0);
        }

        private void buttonSelectPath_Click(object sender, EventArgs e)
        {
            openFileDialog.FileName = textBoxLocalPath.Text;
            openFileDialog.ShowDialog(this);
        }

        private void openFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            textBoxLocalPath.Text = openFileDialog.FileName;
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            var fileName = checkLocalCopy.Checked ? textBoxLocalPath.Text : textBoxUrl.Text;

            try
            {
                if (AsInstaller == null) OnArchive();
                else if (fileName.EndsWithIgnoreCase(@".exe"))
                {
                    switch (Msg.YesNoCancel(this, Resources.AskInstallerEXE, MsgSeverity.Info, Resources.YesInstallerExe, Resources.NoSingleExecutable))
                    {
                        case DialogResult.Yes:
                            OnInstaller();
                            break;
                        case DialogResult.No:
                            OnSingleFile();
                            break;
                    }
                }
                else
                {
                    switch (Archive.GuessMimeType(fileName))
                    {
                        case Archive.MimeTypeMsi:
                            OnInstaller();
                            break;
                        case null:
                            OnSingleFile();
                            break;
                        default:
                            OnArchive();
                            break;
                    }
                }
            }
                #region Error handling
            catch (OperationCanceledException)
            {}
            catch (ArgumentException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
            }
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            catch (WebException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
            }
            catch (NotSupportedException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
            }
            #endregion
        }

        private void OnSingleFile()
        {
            Retrieve(new SingleFile {Href = textBoxUrl.Uri});
            _feedBuilder.ImplementationDirectory = _feedBuilder.TemporaryDirectory;
            using (var handler = new GuiTaskHandler(this))
            {
                _feedBuilder.DetectCandidates(handler);
                _feedBuilder.CalculateDigest(handler);
            }
            if (_feedBuilder.MainCandidate == null) Msg.Inform(this, Resources.NoEntryPointsFound, MsgSeverity.Warn);
            else AsSingleFile();
        }

        private void OnArchive()
        {
            Retrieve(new Archive {Href = textBoxUrl.Uri});
            AsArchive();
        }

        private void Retrieve(DownloadRetrievalMethod retrievalMethod)
        {
            _feedBuilder.RetrievalMethod = retrievalMethod;

            using (var handler = new GuiTaskHandler(this))
            {
                _feedBuilder.TemporaryDirectory = checkLocalCopy.Checked
                    ? retrievalMethod.LocalApply(textBoxLocalPath.Text, handler)
                    : retrievalMethod.DownloadAndApply(handler);
            }
        }

        private void OnInstaller()
        {
            if (checkLocalCopy.Checked)
                _installerCapture.SetLocal(textBoxUrl.Uri, textBoxLocalPath.Text);
            else
            {
                using (var handler = new GuiTaskHandler(this))
                    _installerCapture.Download(textBoxUrl.Uri, handler);
            }
            AsInstaller();
        }
    }
}
