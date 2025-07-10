using Microsoft.Maui.TestCases.Tests;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;
using System;
using System.Diagnostics;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public partial class ShellContentPreloadTests : _IssuesUITest
	{
		public ShellContentPreloadTests(TestDevice device) : base(device) { }

		public override string Issue => "Shell flyout navigation performance with content preloading";

		protected override bool ResetAfterEachTest => true;

		[Test]
		[Category(UITestCategories.Shell)]
		[Category(UITestCategories.Performance)]
		public void ValidateShellContentPreloading()
		{
			// Verify we start on the main page
			App.WaitForElement("MainPageLabel");

			// Test manual preload trigger
			App.WaitForElement("TriggerPreloadButton");
			App.Tap("TriggerPreloadButton");

			// Wait a moment for preloading to complete
			System.Threading.Thread.Sleep(1000);

			// Navigate to preloaded test page via flyout
			App.WaitForElement("ComplexPageLabel", timeout: TimeSpan.FromSeconds(10), postTimeout: TimeSpan.FromSeconds(1));
		}

		[Test]
		[Category(UITestCategories.Shell)]
		[Category(UITestCategories.Performance)]
		public void ValidatePreloadedNavigationIsEfficient()
		{
			// Start on main page
			App.WaitForElement("MainPageLabel");

			// Trigger preloading
			App.WaitForElement("TriggerPreloadButton");
			App.Tap("TriggerPreloadButton");

			// Wait for preloading to complete
			System.Threading.Thread.Sleep(1000);

			// Measure navigation time to preloaded page
			var stopwatch = Stopwatch.StartNew();
			
			// Navigate to preloaded test page - this should be fast
			App.WaitForElement("ComplexPageLabel", timeout: TimeSpan.FromSeconds(5));
			
			stopwatch.Stop();

			// Navigation should be reasonably fast (less than 3 seconds)
			// This is a basic performance check - in real scenarios the improvement would be more dramatic
			Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(3000), 
				"Navigation to preloaded content took too long");

			// Verify the complex page loaded properly with all controls
			App.WaitForElement("Entry1");
			App.WaitForElement("Entry10");
			App.WaitForElement("Entry20");
			App.WaitForElement("Entry30");
			App.WaitForElement("TestButton");
			App.WaitForElement("TestSlider");
			App.WaitForElement("TestProgressBar");
		}

		[Test]
		[Category(UITestCategories.Shell)]
		public void ValidateShellContentBasicFunctionality()
		{
			// Verify main page loads
			App.WaitForElement("MainPageLabel");

			// Navigate to complex test page to ensure it loads completely
			App.WaitForElement("ComplexPageLabel", timeout: TimeSpan.FromSeconds(10));

			// Verify some of the Entry controls loaded
			App.WaitForElement("Entry1");
			App.WaitForElement("Entry15");
			App.WaitForElement("Entry30");

			// Verify other controls loaded
			App.WaitForElement("TestButton");
			App.WaitForElement("TestSlider");

			// Test that controls are functional
			App.Tap("TestButton"); // Should not throw
		}
	}
}