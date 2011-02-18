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

using System.Globalization;
using NUnit.Framework;
using ZeroInstall.Model;

namespace ZeroInstall.Injector.Solver
{
    /// <summary>
    /// Contains test methods for <see cref="Requirements"/>.
    /// </summary>
    [TestFixture]
    public class RequirementsTest
    {
        #region Helpers
        /// <summary>
        /// Creates test <see cref="Requirements"/>.
        /// </summary>
        public static Requirements CreateTestRequirements()
        {
            return new Requirements
            {
                InterfaceID = "http://0install.de/feeds/test/test1.xml", CommandName = "command", Architecture = new Architecture(OS.Windows, Cpu.I586),
                NotBeforeVersion = new ImplementationVersion("1.0"), BeforeVersion = new ImplementationVersion("2.0")
            };
        }
        #endregion

        /// <summary>
        /// Ensures that setting <see cref="Requirements.InterfaceID"/> produces the correct exceptions.
        /// </summary>
        [Test]
        public void TestInterfaceID()
        {
            var requirements = new Requirements();
            Assert.Throws<InvalidInterfaceIDException>(() => requirements.InterfaceID = "http://0install.de", "Should not accept URIs without slash after hostname");
            Assert.Throws<InvalidInterfaceIDException>(() => requirements.InterfaceID = "ftp://0install.de/feeds/test.xml", "Should not accept protocols other than HTTP(S)");
            Assert.Throws<InvalidInterfaceIDException>(() => requirements.InterfaceID = "test.xml", "Should not accept relative paths");

            Assert.DoesNotThrow(() => requirements.InterfaceID = "http://0install.de/feeds/test/test1.xml", "Should accept HTTP URIs");
            Assert.DoesNotThrow(() => requirements.InterfaceID = "https://0install.de/feeds/test.xml", "Should accept HTTPS URIs");
            Assert.DoesNotThrow(() => requirements.InterfaceID = "/feeds/test.xml", "Should absolute paths");
        }

        /// <summary>
        /// Ensures that the class can be correctly cloned.
        /// </summary>
        [Test]
        public void TestClone()
        {
            var requirements1 = CreateTestRequirements();
            requirements1.Languages.Add(new CultureInfo("de"));
            requirements1.Languages.Add(new CultureInfo("en"));
            var requirements2 = requirements1.CloneRequirements();

            // Ensure data stayed the same
            Assert.AreEqual(requirements1, requirements2, "Cloned objects should be equal.");
            Assert.AreEqual(requirements1.GetHashCode(), requirements2.GetHashCode(), "Cloned objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(requirements1, requirements2), "Cloning should not return the same reference.");
        }
    }
}
