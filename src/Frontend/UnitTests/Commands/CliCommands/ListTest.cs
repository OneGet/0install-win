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
using ZeroInstall.Store.Model;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Contains integration tests for <see cref="List"/>.
    /// </summary>
    [TestFixture]
    public class ListTest : CliCommandTest<List>
    {
        [Test(Description = "Ensures calling with no arguments returns all feeds in the cache.")]
        public void TestNoArgs()
        {
            FeedCacheMock.Setup(x => x.ListAll()).Returns(new[] {FeedTest.Test1Uri, FeedTest.Test2Uri});
            RunAndAssert(new[] {FeedTest.Test1Uri.ToStringRfc(), FeedTest.Test2Uri.ToStringRfc()}, ExitCode.OK);
        }

        [Test(Description = "Ensures calling with a single argument returns a filtered list of feeds in the cache.")]
        public void TestPattern()
        {
            FeedCacheMock.Setup(x => x.ListAll()).Returns(new[] {FeedTest.Test1Uri, FeedTest.Test2Uri});
            RunAndAssert(new[] {FeedTest.Test2Uri.ToStringRfc()}, ExitCode.OK, "test2");
        }
    }
}
