using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla45743 : _IssuesUITest
{
	const string CancelBtn = "Cancel";

	public Bugzilla45743(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] Calling DisplayAlert via BeginInvokeOnMainThread blocking other calls on iOS";

	[Test]
	[Category(UITestCategories.DisplayAlert)]
	public void Bugzilla45743Test()
	{
		App.WaitForElementTillPageNavigationSettled("Title");
		App.TapDisplayAlertButton(CancelBtn);
		App.WaitForElementTillPageNavigationSettled("Title 2");
		App.TapDisplayAlertButton(CancelBtn);
		App.WaitForElementTillPageNavigationSettled("ActionSheet Title");
		App.TapDisplayAlertButton(CancelBtn);
	}
}