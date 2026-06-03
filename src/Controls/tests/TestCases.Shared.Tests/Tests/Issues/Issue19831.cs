#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19831 : _IssuesUITest
{
	public override string Issue => "[Android] Action mode menu doesn't disappear when switch on another tab";

	public Issue19831(TestDevice device)
		: base(device)
	{ }

	[Test]
	[Category(UITestCategories.ListView)]
	public void ActionModeMenuShouldNotBeVisibleAfterSwitchingTab()
	{
		// This test only validates the new Android ShellHandler behavior; on the legacy
		// ShellRenderer path the behavior differs.
		if (App is AppiumAndroidApp androidApp && !HelperExtensions.IsAndroidShellHandlerEnabled(androidApp))
		{
			Assert.Ignore("This test only runs on Android with the new ShellHandler (UseAndroidShellHandlers=true).");
		}

		_ = App.WaitForElement("Item1");

		// 1. Open a context menu.
		App.LongPress("Item1");

		// 2. Navigate to a different tab.
		App.Click("button");

		// 3. The test passes if the action mode menu is not visible.
		VerifyScreenshot();
	}
}
#endif