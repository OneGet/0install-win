﻿/*
 * Copyright 2010 Simon E. Silva Lauinger
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
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using Common;
using Common.Controls;
using Common.Tasks;
using Common.Utils;
using Common.Values.Design;
using ZeroInstall.Store.Implementation.Archive;
using ZeroInstall.Model;

namespace ZeroInstall.Publish.WinForms.Controls
{
    public partial class ArchiveEditor : UserControl, IEditorControlContainerRef<Archive, Implementation>
    {
        #region Properties
        private Archive _target;

        /// <inheritdoc/>
        public Archive Target
        {
            get { return _target; }
            set
            {
                _target = value;
                Refresh();
            }
        }

        /// <inheritdoc/>
        public Implementation ContainerRef { get; set; }

        /// <inheritdoc/>
        public Common.Undo.ICommandExecutor CommandExecutor { get; set; }
        #endregion

        #region Attributes
        /// <summary>
        /// The <see cref="Archive" /> to be displayed and edited by this form.
        /// </summary>
        private Archive _archive;

        /// <summary>
        /// List with supported archive types by 0install. If more types become supported, add them to this list.
        /// </summary>
        private readonly List<ArchiveMimeTypes> _supportedMimeTypes = new List<ArchiveMimeTypes> {ArchiveMimeTypes.Zip, ArchiveMimeTypes.Tar, ArchiveMimeTypes.TarGz, ArchiveMimeTypes.TarBz2, ArchiveMimeTypes.TarLzma};
        #endregion

        #region Properties
        /// <summary>
        /// Path to the extracted archive. <see langword="null"/> when archive isn't extracted yet.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ExtractedArchivePath { get; private set; }

        /// <summary>
        /// The <see cref="Archive" /> to be displayed and edited by this form.
        /// When setted, the form controls will be filled with its values.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Archive Archive
        {
            get { return _archive; }
            set
            {
                _archive = (value ?? new Archive());
                UpdateControlValues();
                SetStartState();
            }
        }

        /// <summary>
        /// The <see cref="ManifestDigest"/> of the <see cref="Archive"/> edited by this form.
        /// <see langworde="null"/> when Archive isn't extracted.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ManifestDigest ManifestDigest { get; private set; }
        #endregion

        #region Data structures

        #region ArchiveMimeTypes
        /// <summary>
        /// Mime types of archives 0install supports.
        /// </summary>
        [TypeConverter(typeof(EnumDescriptionConverter<ArchiveMimeTypes>))]
        private enum ArchiveMimeTypes
        {
            [Description("(auto detect)")]
            Auto,

            [Description("application/x-rpm")]
            Rpm,

            [Description("application/x-deb")]
            Deb,

            [Description("application/x-tar")]
            Tar,

            [Description("application/x-bzip-compressed-tar")]
            TarBz2,

            [Description("application/x-lzma-compressed-tar")]
            TarLzma,

            [Description("application/x-compressed-tar")]
            TarGz,

            [Description("application/zip")]
            Zip,

            [Description("application/vnd.ms-cab-compressed")]
            Cab
        }

        /// <summary>
        /// Contains a <see cref="ArchiveMimeTypes"/> and overrides the toString method.
        /// </summary>
        private sealed class ArchiveMimeType
        {
            public ArchiveMimeTypes MimeType { get; set; }

            /// <summary>
            /// Tries to create a <see cref="ArchiveMimeTypes"/> by the mime type of the archive.
            /// </summary>
            /// <param name="mimeTypeDescription">The mime type from which a <see cref="ArchiveMimeTypes"/> shall be created.</param>
            /// <param name="toCreate">Created <see cref="ArchiveMimeTypes"/> when returned <see langword="true"/>, else <see langword="null"/>.</param>
            /// <returns><see langword="true"/> when the creation succeeded, else <see langword="false"/>.</returns>
            public static bool TryCreateFromDescription(string mimeTypeDescription, out ArchiveMimeType toCreate)
            {
                toCreate = (
                    from ArchiveMimeTypes archiveMimeType in Enum.GetValues(typeof(ArchiveMimeTypes))
                    where archiveMimeType.ConvertToString() == mimeTypeDescription
                    select new ArchiveMimeType {MimeType = archiveMimeType}).FirstOrDefault();
                return (toCreate != null);
            }

            /// <summary>
            /// Returns the description of <see cref="MimeType"/>.
            /// </summary>
            /// <returns>Description of <see cref="MimeType"/>.</returns>
            public override string ToString()
            {
                return MimeType.ConvertToString();
            }
        }
        #endregion

        #endregion

        #region Events
        /// <summary>
        /// Raised when NO valid <see cref="Archive"/> was created.
        /// </summary>
        public event Action NoValidArchive;

        /// <summary>
        /// Raised when a vaild <see cref="Archive"/> was created.
        /// </summary>
        public event Action ValidArchive;
        #endregion

        #region Initialization
        public ArchiveEditor()
        {
            InitializeComponent();
            InitializeComboBoxArchiveFormat();
            ClearControlValues();
        }

        private void InitializeComboBoxArchiveFormat()
        {
            foreach (ArchiveMimeTypes imageMimeType in Enum.GetValues(typeof(ArchiveMimeTypes)))
                comboBoxArchiveFormat.Items.Add(new ArchiveMimeType {MimeType = imageMimeType});
        }
        #endregion

        #region Controls management
        private void ClearControlValues()
        {
            comboBoxArchiveFormat.SelectedIndex = 0;
            hintTextBoxStartOffset.Text = "";
            uriTextBoxArchiveUrl.Text = "";
            hintTextBoxLocalArchive.Text = "";
            treeViewSubDirectory.Nodes[0].Nodes.Clear();
        }

        private void UpdateControlValues()
        {
            ClearControlValues();

            // set comboBoxArchiveFormat
            if (!string.IsNullOrEmpty(_archive.MimeType))
            {
                ArchiveMimeType mimeType;
                if (ArchiveMimeType.TryCreateFromDescription(_archive.MimeType, out mimeType))
                    comboBoxArchiveFormat.SelectedIndex = (int)mimeType.MimeType;
                else
                    comboBoxArchiveFormat.SelectedIndex = 0;
            }
            // set other hintTextBoxes
            if (_archive.StartOffset != default(long)) hintTextBoxStartOffset.Text = _archive.StartOffset.ToString(CultureInfo.CurrentCulture);
            if (!string.IsNullOrEmpty(_archive.HrefString)) uriTextBoxArchiveUrl.Text = _archive.HrefString;

            // build treeViewSubDirectory
            if (string.IsNullOrEmpty(_archive.Extract)) return;
            var splittedPath = new LinkedList<string>(_archive.Extract.Split('/'));
            splittedPath.RemoveFirst();
            var currentNode = treeViewSubDirectory.Nodes[0];
            treeViewSubDirectory.BeginUpdate();
            splittedPath.Aggregate(currentNode, (current, folder) => current.Nodes.Add(folder));
            treeViewSubDirectory.ExpandAll();
            treeViewSubDirectory.EndUpdate();

            SetStartState();
        }
        #endregion

        #region Control events
        /// <summary>
        /// Opens a dialog to ask the user where to download the archive from
        /// <see cref="uriTextBoxArchiveUrl"/>.Text, downloads the archive and sets
        /// <see cref="hintTextBoxLocalArchive"/> with the chosen path.<br/>
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ButtonDownloadClick(object sender, EventArgs e)
        {
            var url = uriTextBoxArchiveUrl.Uri;
            if (url == null) return;

            // show dialog to choose download folder
            string absoluteFilePath = ShowSaveFileDialog(url.Segments[url.Segments.Length - 1]);
            if (absoluteFilePath == null) return;

            if (!SetArchiveMimeType(absoluteFilePath)) return;

            try
            {
                TrackingDialog.Run(this, new DownloadFile(url, absoluteFilePath), null);
            }
                #region Error handling
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                SetArchiveUrlChosenState();
                return;
            }
            catch (OperationCanceledException)
            {
                File.Delete(absoluteFilePath);
                SetArchiveUrlChosenState();
                return;
            }
            catch (WebException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                File.Delete(absoluteFilePath);
                SetArchiveUrlChosenState();
                return;
            }
            #endregion

            hintTextBoxLocalArchive.Text = absoluteFilePath;
            SetArchiveDownloadedState();
        }

        /// <summary>
        /// Shows the <see cref="saveFileDialogDownloadFile"/> with <paramref name="fileName"/> as presetted file name.
        /// If the file allready exists it will be removed.
        /// </summary>
        /// <param name="fileName">The presetted file name.</param>
        /// <returns>The chosen file path or null when the user cancels the dialog.</returns>
        private string ShowSaveFileDialog(string fileName)
        {
            saveFileDialogDownloadFile.FileName = fileName;
            if (saveFileDialogDownloadFile.ShowDialog() == DialogResult.Cancel) return null;
            string chosenFilePath = saveFileDialogDownloadFile.FileName;
            try
            {
                if (File.Exists(chosenFilePath))
                    File.Delete(chosenFilePath);
            }
            catch (IOException exception)
            {
                Msg.Inform(this, exception.Message, MsgSeverity.Error);
                return null;
            }
            return chosenFilePath;
        }

        /// <summary>
        /// If in <see cref="comboBoxArchiveFormat"/> is chosen "(auto detect)", the mime type
        /// of <paramref name="fileName"/> will be guessed and setted into 
        /// <see cref="comboBoxArchiveFormat"/>.
        /// If the mime type is not supported by 0install, a <see cref="MessageBox"/> with an
        /// error message will be shown to the user.
        /// </summary>
        /// <param name="fileName">File to guess the mime type for.</param>
        /// <returns><see langword="true"/>, if mime type is supported by 0install, else
        /// <see langword="false"/></returns>
        private bool SetArchiveMimeType(string fileName)
        {
            if (comboBoxArchiveFormat.SelectedIndex == 0)
            {
                var guessMimeType = Archive.GuessMimeType(fileName);
                ArchiveMimeType archiveMimeType;
                var archiveMimeTypeSucceeded = ArchiveMimeType.TryCreateFromDescription(guessMimeType, out archiveMimeType);

                string errorMessage = null;
                if (!archiveMimeTypeSucceeded)
                {
                    errorMessage = "The archive type of this file is not supported by " +
                        "0install. Downloading archive abort.";
                }
                else if (!_supportedMimeTypes.Contains(archiveMimeType.MimeType))
                {
                    errorMessage = string.Format("The extraction of the {0} format is not" +
                        " yet supported. Please extract the file yourself (e.g with 7zip) and" +
                        " set the path to the extracted archive in the \"Locale archive\"" +
                        " text box.\nATTENTION: DO THIS ONLY AND REALLY ONLY, IF THE FILES" +
                        " IN YOUR ARCHIVE DOESN'T USE UNIX X-BIT!", archiveMimeType);
                }
                if (errorMessage == null)
                {
                    comboBoxArchiveFormat.SelectedIndex = (int)archiveMimeType.MimeType;
                    return true;
                }
                else
                {
                    Msg.Inform(this, errorMessage, MsgSeverity.Error);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Shows a dialog where the user can choose a local archive and sets the the text of
        /// <see cref="hintTextBoxLocalArchive"/> with the chosen archive path.
        /// Enables <see cref="buttonExtractArchive"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ButtonLocalArchiveClick(object sender, EventArgs e)
        {
            if (openFileDialogLocalArchive.ShowDialog() == DialogResult.Cancel) return;
            if (!SetArchiveMimeType(openFileDialogLocalArchive.FileName)) return;
            hintTextBoxLocalArchive.Text = openFileDialogLocalArchive.FileName;

            SetArchiveDownloadedState();
        }

        /// <summary>
        /// Extracts the archive from <see cref="hintTextBoxLocalArchive"/> and fills
        /// <see cref="treeViewSubDirectory"/> with the folders in the archive.
        /// Shows a <see cref="MessageBox"/> with a error message if extracting is not
        /// possible.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ButtonExtractArchiveClick(object sender, EventArgs e)
        {
            if (!SetArchiveMimeType(openFileDialogLocalArchive.FileName)) return;

            var archive = hintTextBoxLocalArchive.Text;
            long startOffset = GetValidStartOffset(hintTextBoxStartOffset.Text);
            if (startOffset < 0) startOffset = 0;

            var extractedArchivePath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(archive)) ?? "", Path.GetFileName(archive) + "_extracted");
            try
            {
                if (Directory.Exists(extractedArchivePath)) Directory.Delete(extractedArchivePath, true);
            }
                #region Error handling
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                return;
            }
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                return;
            }
            #endregion

            try
            {
                using (var extractor = Extractor.CreateExtractor(comboBoxArchiveFormat.Text, archive, startOffset, extractedArchivePath))
                    TrackingDialog.Run(this, extractor, null);
            }
                #region Error handling
            catch (OperationCanceledException)
            {
                return;
            }
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                return;
            }
            #endregion

            treeViewSubDirectory.Nodes[0].Nodes.Clear();
            FillTreeViewExtract(new DirectoryInfo(extractedArchivePath), treeViewSubDirectory.Nodes[0]);
            treeViewSubDirectory.ExpandAll();

            ExtractedArchivePath = extractedArchivePath;
            SetArchiveProperty();

            try
            {
                SetManifestDigestProperty();
            }
                #region Error handling
            catch (OperationCanceledException)
            {
                return;
            }
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                return;
            }
            #endregion

            SetArchiveExtractedState();
        }

        /// <summary>
        /// Tries to parse <paramref name="startOffset"/> to a <see langword="long"/> value >= 0 .
        /// </summary>
        /// <param name="startOffset">Will be parsed.</param>
        /// <returns>The number containing in <paramref name="startOffset"/> if >= 0 , -1 else </returns>
        private static long GetValidStartOffset(string startOffset)
        {
            if (string.IsNullOrEmpty(startOffset)) return 0;

            long parsedStartOffset;
            if (!long.TryParse(startOffset, out parsedStartOffset)) return -1;
            if (parsedStartOffset < 0) return -1;
            return parsedStartOffset;
        }

        /// <summary>
        /// Inserts a folder tree in a node of <see cref="treeViewSubDirectory"/>.
        /// </summary>
        /// <param name="extractedDirectory">Top folder of a folder tree</param>
        /// <param name="currentNode">Node to insert the folder tree</param>
        private static void FillTreeViewExtract(DirectoryInfo extractedDirectory, TreeNode currentNode)
        {
            var folderList = extractedDirectory.GetDirectories();
            if (folderList.Length == 0) return;
            foreach (var folder in folderList)
                FillTreeViewExtract(folder, currentNode.Nodes.Add(folder.Name));
        }

        private void SetArchiveProperty()
        {
            long startOffset = GetValidStartOffset(hintTextBoxStartOffset.Text);
            if (startOffset >= 0) _archive.StartOffset = startOffset;
            if (comboBoxArchiveFormat.SelectedIndex != 0) _archive.MimeType = comboBoxArchiveFormat.Text;
            Uri uri;
            if (Uri.TryCreate(uriTextBoxArchiveUrl.Text, UriKind.RelativeOrAbsolute, out uri)) _archive.Href = uri;
            _archive.Size = new FileInfo(hintTextBoxLocalArchive.Text).Length;
        }

        private void SetManifestDigestProperty()
        {
            ManifestDigest = ManifestUtils.CreateDigest(this, ExtractedArchivePath);
        }

        /// <summary>
        /// Checks whether the the text of <see cref="uriTextBoxArchiveUrl"/> is empty.
        /// If yes, the control will be set to the "start state", elst to the "archive chose state".
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void UriTextBoxArchiveUrlValidated(object sender, EventArgs e)
        {
            if (uriTextBoxArchiveUrl == null)
                SetStartState();
            else
                SetArchiveUrlChosenState();
        }

        /// <summary>
        /// Sets the forcolor to <see cref="Color.Red"/>
        /// if the text in <see cref="hintTextBoxStartOffset"/> is NOT valid.
        /// Enables the buttons and sets the forecolor to <see cref="Color.Green"/> if the text
        /// in <see cref="hintTextBoxStartOffset"/> IS valid
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void HintTextBoxStartOffsetTextChanged(object sender, EventArgs e)
        {
            long startOffset = GetValidStartOffset(hintTextBoxStartOffset.Text);
            if (startOffset >= 0)
            {
                SetAllowedStartOffsetState();
                hintTextBoxStartOffset.ForeColor = Color.Green;
            }
            else
            {
                SetNotAllowedStartOffsetState();

                hintTextBoxStartOffset.ForeColor = Color.Red;
            }
        }

        /// <summary>
        /// Sets the Extract value of the <see cref="Archive"/> property.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void TreeViewSubDirectoryAfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeViewSubDirectory.SelectedNode != null && treeViewSubDirectory.SelectedNode != treeViewSubDirectory.Nodes[0])
            {
                string extractPath = treeViewSubDirectory.SelectedNode.FullPath.Substring("Top folder/".Length);
                _archive.Extract = extractPath;
                string combinedPath = Path.Combine(ExtractedArchivePath, extractPath);
                try
                {
                    ManifestDigest = ManifestUtils.CreateDigest(this, combinedPath);
                }
                    #region Error handling
                catch (OperationCanceledException)
                {}
                catch (IOException ex)
                {
                    Msg.Inform(this, ex.Message, MsgSeverity.Error);
                }
                #endregion
            }
            else _archive.Extract = null;
        }
        #endregion

        #region Control State
        private void SetStartState()
        {
            Button[] toDisable = {buttonExtractArchive};
            foreach (var button in toDisable) button.Enabled = false;
            ExtractedArchivePath = null;
            ManifestDigest = new ManifestDigest();
            if (NoValidArchive != null) NoValidArchive();
        }

        private void SetArchiveUrlChosenState()
        {
            Button[] toEnable = {buttonDownload};
            Button[] toDisable = {buttonExtractArchive};
            foreach (var button in toEnable) button.Enabled = true;
            foreach (var button in toDisable) button.Enabled = false;
        }

        private void SetArchiveDownloadedState()
        {
            Button[] toEnable = {buttonDownload, buttonExtractArchive};
            foreach (var button in toEnable) button.Enabled = true;
            ExtractedArchivePath = null;
            ManifestDigest = new ManifestDigest();
            if (NoValidArchive != null) NoValidArchive();
        }

        private void SetAllowedStartOffsetState()
        {
            if (!string.IsNullOrEmpty(hintTextBoxLocalArchive.Text)) buttonExtractArchive.Enabled = true;
            ExtractedArchivePath = null;
            ManifestDigest = new ManifestDigest();
            if (NoValidArchive != null) NoValidArchive();
        }

        private void SetNotAllowedStartOffsetState()
        {
            buttonExtractArchive.Enabled = true;
            ExtractedArchivePath = null;
            ManifestDigest = new ManifestDigest();
            if (NoValidArchive != null) NoValidArchive();
        }

        private void SetArchiveExtractedState()
        {
            Button[] toEnable = {buttonDownload, buttonExtractArchive};
            foreach (var button in toEnable) button.Enabled = true;
            if (ValidArchive != null) ValidArchive();
        }
        #endregion
    }
}
