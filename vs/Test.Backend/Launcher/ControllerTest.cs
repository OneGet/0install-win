﻿/*
 * Copyright 2010 Bastian Eicher
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
using Common.Collections;
using Common.Storage;
using NUnit.Framework;
using ZeroInstall.Fetchers;
using ZeroInstall.Launcher.Solver;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Feed;

namespace ZeroInstall.Launcher
{
    /// <summary>
    /// Contains test methods for <see cref="Controller"/>.
    /// </summary>
    [TestFixture]
    public class ControllerTest
    {
        /// <summary>
        /// Ensures the <see cref="Controller"/> constructor, <see cref="Controller.GetSelections"/> and <see cref="Controller.GetExecutor"/> throw the correct exceptions.
        /// </summary>
        [Test]
        public void TestExceptions()
        {
            Assert.Throws<ArgumentException>(() => new Controller("invalid", SolverProvider.Default, Policy.CreateDefault(new SilentHandler())), "Invalid paths should be detected");

            var controller = new Controller("http://nothing", SolverProvider.Default, Policy.CreateDefault(new SilentHandler()));
            Assert.Throws<InvalidOperationException>(() => controller.GetSelections(), "GetSelections should depend on Solve being called first");
            Assert.Throws<InvalidOperationException>(() => controller.GetExecutor(), "GetRun should depend on Solve being called first");
        }

        /// <summary>
        /// Ensures that <see cref="Controller.ListUncachedImplementations"/> correctly finds <see cref="Implementation"/>s not cached in a <see cref="IStore"/>.
        /// </summary>
        // Test deactivated because it uses an external process
        //[Test]
        public void TestListUncachedImplementations()
        {
            // Look inside a temporary (empty) store
            IEnumerable<Implementation> implementations;
            using (var temp = new TemporaryDirectory("0install-unit-tests"))
            {
                var policy = new Policy(new FeedManager(FeedCacheProvider.Default, new SilentHandler()), new Fetcher(new SilentHandler(), new DirectoryStore(temp.Path)));
                var controller = new Controller("http://afb.users.sourceforge.net/zero-install/interfaces/seamonkey2.xml", SolverProvider.Default, policy);
                controller.Solve();
                implementations = controller.ListUncachedImplementations();
            }

            // Check the first (and only) entry of the "missing list" is the correct implementation
            Assert.AreEqual("sha1new=91dba493cc1ff911df9860baebb6136be7341d38", EnumUtils.GetFirst(implementations).ManifestDigest.BestDigest, "The actual Implementation should have the same digest as the selection information.");
        }

        /// <summary>
        /// Ensures <see cref="Controller.GetSelections"/> correctly provides results from a <see cref="ZeroInstall.Launcher.Solver"/>.
        /// </summary>
        // Test deactivated because it uses an external process
        //[Test]
        public void TestGetSelections()
        {
            var controller = new Controller("http://afb.users.sourceforge.net/zero-install/interfaces/seamonkey2.xml", SolverProvider.Default, Policy.CreateDefault(new SilentHandler()));
            controller.Solve();
            Assert.AreEqual(controller.GetSelections().InterfaceID, "http://afb.users.sourceforge.net/zero-install/interfaces/seamonkey2.xml");
        }

        /// <summary>
        /// Ensures <see cref="Controller.GetExecutor"/> correctly provides an application that can be launched.
        /// </summary>
        // Test deactivated because it uses an external process
        //[Test]
        public void TestGetExecutor()
        {
            var controller = new Controller("http://afb.users.sourceforge.net/zero-install/interfaces/seamonkey2.xml", SolverProvider.Default, Policy.CreateDefault(new SilentHandler()));
            controller.Solve();
            controller.DownloadUncachedImplementations();
            var executor = controller.GetExecutor();
            var startInfo = executor.GetStartInfo("--help");
            Assert.AreEqual("--help", startInfo.Arguments);
        }
    }
}
