using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19628 : _IssuesUITest
{
	public override string Issue => "CollectionView Header and EmptyView overlap when there are no items";

	public Issue19628(TestDevice device) : base(device)
	{
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void HeaderAndEmptyViewDoNotOverlap()
	{
		App.WaitForElement("Issue19628EmptyView");
		VerifyScreenshot();
	}
}
