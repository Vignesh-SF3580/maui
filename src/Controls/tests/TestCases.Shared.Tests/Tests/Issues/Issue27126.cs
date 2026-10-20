using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue27126 : _IssuesUITest
{
	public Issue27126(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Tap gesture on a button inside an AbsoluteLayout does not respond on iOS";

	[Test]
	[Category(UITestCategories.Gestures)]
	public void ButtonTapGestureUpdatesTapCount()
	{
		App.WaitForElement("ButtonWithTapGesture");
		App.Tap("ButtonWithTapGesture");

		var tapCount = App.WaitForElement("ButtonTapCount").GetText();
		Assert.That(tapCount, Is.EqualTo("Button tapped: 1 times"));
	}
}
