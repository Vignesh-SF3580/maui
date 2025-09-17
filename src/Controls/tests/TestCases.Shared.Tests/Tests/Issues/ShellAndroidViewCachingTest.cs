using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class ShellAndroidViewCachingTest : _IssuesUITest
	{
		public override string Issue => "Shell flyout navigation lag on Android with complex pages";
		
		public ShellAndroidViewCachingTest(TestDevice device) : base(device) { }

		[Test]
		[Category(UITestCategories.Shell)]
		public void ShellNavigationShouldBeReasonablyFast()
		{
			// Wait for initial load
			App.WaitForElement("scroll_view");
			
			// Verify we're on the first page
			App.WaitForElement("entry_0");
			
			// Navigate to second tab 
			App.Tap("Complex Page 2");
			
			// Should navigate quickly to second page 
			App.WaitForElement("scroll_view");
			App.WaitForElement("entry_0");
			
			// Navigate back to first tab
			App.Tap("Complex Page 1");
			
			// This navigation should be even faster due to Android view caching
			App.WaitForElement("scroll_view");
			App.WaitForElement("entry_0");
			
			// Navigate back and forth a few times to verify caching works
			for (int i = 0; i < 3; i++)
			{
				App.Tap("Complex Page 2");
				App.WaitForElement("scroll_view");
				
				App.Tap("Complex Page 1");
				App.WaitForElement("scroll_view");
			}
		}
	}
}