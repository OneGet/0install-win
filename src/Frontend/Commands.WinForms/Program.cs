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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Controls;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Services.Injector;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Commands.WinForms
{
    /// <summary>
    /// A WinForms-based GUI for Zero Install, for installing and launching applications, managing caches, etc.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The canonical EXE name (without the file ending) for this binary.
        /// </summary>
        public const string ExeName = "0install-win";

        /// <summary>
        /// The application user model ID used by the Windows 7 taskbar. Encodes <see cref="Locations.InstallBase"/> and the name of this sub-app.
        /// </summary>
        public static readonly string AppUserModelID = "ZeroInstall." + Locations.InstallBase.GetHashCode() + ".Commands";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        // NOTE: No [STAThread] here, because it could block .NET remoting callbacks
        private static int Main(string[] args)
        {
            ProgramUtils.Init();
            WindowsUtils.SetCurrentProcessAppID(AppUserModelID);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ErrorReportForm.SetupMonitoring(new Uri("https://0install.de/error-report/"));
            return (int)Run(args);
        }

        /// <summary>
        /// Runs the application (called by main method or by embedding process).
        /// </summary>
        [STAThread] // Required for WinForms
        public static ExitCode Run(string[] args)
        {
            Log.Debug("Zero Install Command WinForms GUI started with: " + args.JoinEscapeArguments());

            using (var handler = new GuiCommandHandler())
            {
                try
                {
                    var command = CommandFactory.CreateAndParse(args, handler);
                    return command.Execute();
                }
                    #region Error handling
                catch (OperationCanceledException)
                {
                    return ExitCode.UserCanceled;
                }
                catch (NotAdminException ex)
                {
                    handler.DisableUI();
                    if (WindowsUtils.IsWindowsNT)
                    {
                        try
                        {
                            return (ExitCode)ProcessUtils.Assembly(ExeName, args).AsAdmin().Run();
                        }
                            #region Error handling
                        catch (OperationCanceledException)
                        {
                            return ExitCode.UserCanceled;
                        }
                        catch (IOException ex2)
                        {
                            Log.Error(ex2);
                            return ExitCode.IOError;
                        }
                        #endregion
                    }
                    else
                    {
                        Log.Error(ex);
                        Msg.Inform(null, ex.Message, MsgSeverity.Warn);
                        return ExitCode.AccessDenied;
                    }
                }
                catch (OptionException ex)
                {
                    var builder = new StringBuilder(ex.Message);
                    if (ex.InnerException != null) builder.Append("\n" + ex.InnerException.Message);
                    builder.Append("\n" + string.Format(Resources.TryHelp, ExeName));
                    Msg.Inform(null, builder.ToString(), MsgSeverity.Warn);
                    return ExitCode.InvalidArguments;
                }
                catch (FormatException ex)
                {
                    Log.Error(ex);
                    handler.DisableUI();
                    ErrorBox.Show(null, ex.Message, handler.ErrorLog);
                    return ExitCode.InvalidArguments;
                }
                catch (WebException ex)
                {
                    Log.Error(ex);
                    handler.DisableUI();
                    ErrorBox.Show(null, ex.Message, handler.ErrorLog);
                    return ExitCode.WebError;
                }
                catch (NotSupportedException ex)
                {
                    Log.Error(ex);
                    handler.DisableUI();
                    ErrorBox.Show(null, ex.Message, handler.ErrorLog);
                    return ExitCode.NotSupported;
                }
                catch (IOException ex)
                {
                    Log.Error(ex);
                    handler.DisableUI();
                    ErrorBox.Show(null, ex.Message, handler.ErrorLog);
                    return ExitCode.IOError;
                }
                catch (UnauthorizedAccessException ex)
                {
                    Log.Error(ex);
                    handler.DisableUI();
                    ErrorBox.Show(null, ex.Message, handler.ErrorLog);
                    return ExitCode.AccessDenied;
                }
                catch (InvalidDataException ex)
                {
                    Log.Error(ex);
                    handler.DisableUI();
                    ErrorBox.Show(null, ex.Message, handler.ErrorLog);
                    return ExitCode.InvalidData;
                }
                catch (SignatureException ex)
                {
                    Log.Error(ex);
                    handler.DisableUI();
                    ErrorBox.Show(null, ex.Message, handler.ErrorLog);
                    return ExitCode.InvalidSignature;
                }
                catch (DigestMismatchException ex)
                {
                    Log.Error(ex);
                    handler.DisableUI();
                    ErrorBox.Show(null, Resources.DownloadDamaged, handler.ErrorLog);
                    return ExitCode.DigestMismatch;
                }
                catch (SolverException ex)
                {
                    Log.Error(ex);
                    handler.DisableUI();
                    ErrorBox.Show(null, ex.Message.GetLeftPartAtFirstOccurrence(Environment.NewLine), handler.ErrorLog);
                    return ExitCode.SolverError;
                }
                catch (ExecutorException ex)
                {
                    Log.Error(ex);
                    handler.DisableUI();
                    ErrorBox.Show(null, ex.Message, handler.ErrorLog);
                    return ExitCode.ExecutorError;
                }
                catch (ConflictException ex)
                {
                    Log.Error(ex);
                    handler.DisableUI();
                    ErrorBox.Show(null, ex.Message, handler.ErrorLog);
                    return ExitCode.Conflict;
                }
                    #endregion

                finally
                {
                    handler.CloseUI();
                }
            }
        }

        #region Taskbar
        /// <summary>
        /// Configures the Windows 7 taskbar for a specific window.
        /// </summary>
        /// <param name="form">The window to configure.</param>
        /// <param name="name">The name for the taskbar entry.</param>
        /// <param name="subCommand">The name to add to the <see cref="AppUserModelID"/> as a sub-command; can be <see langword="null"/>.</param>
        /// <param name="arguments">Additional arguments to pass to <see cref="ExeName"/> when restarting to get back to this window; can be <see langword="null"/>.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Taskbar operations are always per-window.")]
        public static void ConfigureTaskbar([NotNull] Form form, [NotNull] string name, [CanBeNull] string subCommand = null, [CanBeNull] string arguments = null)
        {
            #region Sanity checks
            if (form == null) throw new ArgumentNullException("form");
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            #endregion

            string appUserModelID = AppUserModelID;
            if (!string.IsNullOrEmpty(subCommand)) appUserModelID += "." + subCommand;
            string exePath = Path.Combine(Locations.InstallBase, ExeName + ".exe");
            WindowsTaskbar.SetWindowAppID(form.Handle, appUserModelID, exePath.EscapeArgument() + " " + arguments, exePath, name);
        }
        #endregion
    }
}
