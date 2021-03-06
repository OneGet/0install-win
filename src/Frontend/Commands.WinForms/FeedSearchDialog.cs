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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using JetBrains.Annotations;
using NanoByte.Common;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store;

namespace ZeroInstall.Commands.WinForms
{
    /// <summary>
    /// Displays the results of a feed search to the user and allows them to perform additional searches.
    /// </summary>
    public partial class FeedSearchDialog : Form
    {
        private List<SearchResult> _results;

        /// <summary>
        /// Creates a new feed search dialog.
        /// </summary>
        /// <param name="query">An already executed query to visualize.</param>
        public FeedSearchDialog([NotNull] SearchQuery query)
        {
            #region Sanity checks
            if (query == null) throw new ArgumentNullException("query");
            #endregion

            InitializeComponent();

            textKeywords.Text = query.Keywords;
            textKeywords.TextChanged += delegate
            {
                queryTimer.Stop();
                queryTimer.Start();
            };

            dataGrid.AutoGenerateColumns = false;
            dataGrid.DataSource = _results = query.Results;
        }

        private void queryTimer_Tick(object sender, EventArgs e)
        {
            if (backgroundWorker.IsBusy) return;

            queryTimer.Stop();
            backgroundWorker.RunWorkerAsync(textKeywords.Text);
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = SearchQuery.Perform(Config.Load(), (string)e.Argument);
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                errorProvider.Clear();
                dataGrid.DataSource = _results = ((SearchQuery)e.Result).Results;
            }
            else
            {
                errorProvider.SetIconAlignment(textKeywords, ErrorIconAlignment.MiddleLeft);
                errorProvider.SetError(textKeywords, e.Error.Message);
            }
        }

        #region Context menu
        private void dataGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == columnUri.Index)
                contextMenu.Show(Cursor.Position);
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            if (dataGrid.CurrentRow == null) return;
            var result = _results[dataGrid.CurrentRow.Index];

            try
            {
                ProcessUtils.Assembly(Program.ExeName, "run", "--no-wait", result.Uri.ToStringRfc()).Start();
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

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (dataGrid.CurrentRow == null) return;
            var result = _results[dataGrid.CurrentRow.Index];

            try
            {
                ProcessUtils.Assembly(Program.ExeName, "add", result.Uri.ToStringRfc()).Start();
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

        private void buttonIntegrate_Click(object sender, EventArgs e)
        {
            if (dataGrid.CurrentRow == null) return;
            var result = _results[dataGrid.CurrentRow.Index];

            try
            {
                ProcessUtils.Assembly(Program.ExeName, "integrate", result.Uri.ToStringRfc()).Start();
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

        private void buttonDetails_Click(object sender, EventArgs e)
        {
            if (dataGrid.CurrentRow == null) return;
            var result = _results[dataGrid.CurrentRow.Index];

            try
            {
                ProcessUtils.Start(result.Uri.ToStringRfc());
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
        #endregion
    }
}
