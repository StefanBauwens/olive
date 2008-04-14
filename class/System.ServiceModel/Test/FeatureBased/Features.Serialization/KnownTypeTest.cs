﻿using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Proxy.MonoTests.Features.Client;

namespace MonoTests.Features.Serialization
{
	[TestFixture]
	[Category ("NotWorking")]
    public class KnownTypeTest : TestFixtureBase<KnownTypeTesterContractClient, MonoTests.Features.Contracts.KnownTypeTester, MonoTests.Features.Contracts.IKnownTypeTesterContract>
	{
		[Test]
		public void TestKnownType ()
		{
			Point2D p1 = new Point2D ();
			p1.X = 1;
			p1.Y = 1;

			Point2D p2 = new Point2D ();
			p2.X = 2;
			p2.Y = 3;

			Point2D r = Client.Move (p1, p2);
			Assert.IsNotNull (r, "#1");
			Assert.IsTrue (r is AdvPoint2D, "#2");
			Assert.AreEqual (((AdvPoint2D) r).ZeroDistance, 5, "#3");

		}
	}
}
