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

using NUnit.Framework;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Contains test methods for <see cref="FeedElement"/>.
    /// </summary>
    [TestFixture]
    public class FeedElementTest
    {
        [Test]
        public void TestFilterMismatch()
        {
            Assert.IsFalse(FeedElement.FilterMismatch(new EntryPoint()));
            Assert.IsFalse(FeedElement.FilterMismatch(new EntryPoint {IfZeroInstallVersion = new VersionRange("0..")}));
            Assert.IsTrue(FeedElement.FilterMismatch(new EntryPoint {IfZeroInstallVersion = new VersionRange("..!0")}));
        }
    }
}
