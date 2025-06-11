#if TEST_FAILS_ON_WINDOWS // Issue replicates in native WinUI
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue15833 : _IssuesUITest
{
	public Issue15833(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "Unloaded event should not trigger when switching tabs";

	[Test]
	[Category(UITestCategories.Shell)]
	public void UnloadedNotFiredOnTabSwitch()
	{
		App.WaitForElement("Page1Button");
		App.Tap("Page1Button");
		App.WaitForElement("Page2Label");
		App.Tap("Second Tab");
		App.WaitForElement("First Tab");
		App.Tap("First Tab");
		var label = App.WaitForElement("Page2Label");
		Assert.That(label.GetText(), Is.EqualTo("Unloaded event not triggered"));
	}
}
#endif