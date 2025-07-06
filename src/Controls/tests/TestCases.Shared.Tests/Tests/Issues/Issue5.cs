using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue5 : _IssuesUITest
	{
		public Issue5(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] Shell page Unloaded event fires when returning to it from another tab";

		[Test]
		[Category(UITestCategories.Shell)]
		public void ShellTabUnloadedEventShouldNotFireOnTabSwitch()
		{
			// Arrange - we start on Tab 1
			App.WaitForElement("StatusLabel");
			var initialStatus = App.GetText("StatusLabel");
			
			// Navigate to second page within Tab 1
			App.Tap("NavigateButton");
			App.WaitForElement("SecondPageStatusLabel");
			
			// Switch to Tab 2
			App.Tap("Tab 2");
			App.WaitForElement("Tab2Label");
			
			// Switch back to Tab 1 - this should NOT trigger Unloaded event for SecondPage
			App.Tap("Tab 1");
			App.WaitForElement("SecondPageStatusLabel");
			
			// Check that SecondPage is still showing and Unloaded event wasn't triggered
			var secondPageStatus = App.GetText("SecondPageStatusLabel");
			
			// The unloaded count should still be 0 since we just switched tabs, not navigated away
			Assert.That(secondPageStatus, Does.Contain("Unloaded: 0"), 
				"Unloaded event should not fire when switching between Shell tabs");
			
			// Go back to Tab 1 main page
			App.Tap("BackButton");
			App.WaitForElement("StatusLabel");
			
			// Now the SecondPage should have properly fired its Unloaded event
			App.Tap("NavigateButton");
			App.WaitForElement("SecondPageStatusLabel");
			var finalSecondPageStatus = App.GetText("SecondPageStatusLabel");
			
			// This time the unloaded count should be 1 since we actually navigated away
			Assert.That(finalSecondPageStatus, Does.Contain("Unloaded: 1"), 
				"Unloaded event should fire when actually navigating away from page");
		}
	}
}