/*
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
using System.IO;
using System.Net;
using System.Text;
using Gtk;
using NanoByte.Common;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Services.Injector;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Commands.Gtk
{
    /// <summary>
    /// Launches a GTK# tool for managing caches of Zero Install implementations.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The canonical EXE name (without the file ending) for this binary.
        /// </summary>
        public const string ExeName = "0install-gtk";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static int Main(string[] args)
        {
            ProgramUtils.Init();
            Application.Init();
            return (int)Run(args);
        }

        /// <summary>
        /// Runs the application (called by main method or by embedding process).
        /// </summary>
        public static ExitCode Run(string[] args)
        {
            Log.Debug("Zero Install Command GTK GUI started with: " + args.JoinEscapeArguments());

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
                    handler.DisableUI();
                    Log.Error(ex);
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    return ExitCode.InvalidArguments;
                }
                catch (WebException ex)
                {
                    handler.DisableUI();
                    Log.Error(ex);
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    return ExitCode.WebError;
                }
                catch (NotSupportedException ex)
                {
                    handler.DisableUI();
                    Log.Error(ex);
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    return ExitCode.NotSupported;
                }
                catch (IOException ex)
                {
                    handler.DisableUI();
                    Log.Error(ex);
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    return ExitCode.IOError;
                }
                catch (UnauthorizedAccessException ex)
                {
                    handler.DisableUI();
                    Log.Error(ex);
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    return ExitCode.AccessDenied;
                }
                catch (InvalidDataException ex)
                {
                    handler.DisableUI();
                    Log.Error(ex);
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    return ExitCode.InvalidData;
                }
                catch (SignatureException ex)
                {
                    handler.DisableUI();
                    Log.Error(ex);
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    return ExitCode.InvalidSignature;
                }
                catch (DigestMismatchException ex)
                {
                    handler.DisableUI();
                    Log.Error(ex);
                    Msg.Inform(null, Resources.DownloadDamaged, MsgSeverity.Error);
                    return ExitCode.DigestMismatch;
                }
                catch (SolverException ex)
                {
                    handler.DisableUI();
                    Log.Error(ex);
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    return ExitCode.SolverError;
                }
                catch (ExecutorException ex)
                {
                    handler.DisableUI();
                    Log.Error(ex);
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    return ExitCode.ExecutorError;
                }
                catch (ConflictException ex)
                {
                    handler.DisableUI();
                    Log.Error(ex);
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    return ExitCode.Conflict;
                }
                    #endregion

                finally
                {
                    handler.CloseUI();
                }
            }
        }
    }
}
