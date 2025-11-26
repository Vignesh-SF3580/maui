namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Bugzilla, 45743, "[iOS] Calling DisplayAlert via BeginInvokeOnMainThread blocking other calls on iOS", PlatformAffected.iOS)]
public class Bugzilla45743 : TestNavigationPage
{
	protected override void Init()
	{
		PushAsync(new ContentPage
		{
			Content = new StackLayout
			{
				AutomationId = "Page1",
				Children =
				{
					new Label { Text = "Page 1" }
				}
			}
		});

		MainThread.BeginInvokeOnMainThread(async () =>
		{
			var page2 = new ContentPage
			{
				AutomationId = "Page2",
				Content = new StackLayout
				{
					Children =
					{
						new Label { Text = "Page 2", AutomationId = "Page 2" }
					}
				}
			};
			page2.Loaded += Page2_Loaded;

			await PushAsync(page2);
		});
	}

	private void Page2_Loaded(object sender, EventArgs e)
	{
		MainThread.BeginInvokeOnMainThread(async () =>
		{
			await DisplayAlertAsync("Title", "Message", "Accept", "Cancel");
			await DisplayAlertAsync("Title 2", "Message", "Accept", "Cancel");
			await DisplayActionSheetAsync("ActionSheet Title", "Cancel", "Close", new string[] { "Test", "Test 2" });
		});
	}
}