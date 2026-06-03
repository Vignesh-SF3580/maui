namespace Maui.Controls.Sample
{
	internal class CoreRootPage : ContentPage
	{
		public CoreRootPage(Page rootPage)
		{
			Title = "Controls TestCases";

			var corePageView = new CorePageView(rootPage);

			var searchBar = new Entry()
			{
				AutomationId = "SearchBar"
			};

			searchBar.TextChanged += (sender, e) =>
			{
				corePageView.FilterPages(e.NewTextValue);
			};

			var testCasesButton = new Button
			{
				Text = "Go to Test Cases",
				AutomationId = "GoToTestButton",
				Command = new Command(async () =>
				{
					if (!string.IsNullOrEmpty(searchBar.Text))
					{
						await corePageView.NavigateToTest(searchBar.Text);
					}
					else
					{
						await Navigation.PushModalAsync(TestCases.GetTestCases());
					}
				})
			};

			var rootLayout = new Grid();
			rootLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			rootLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			rootLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			rootLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });


			rootLayout.Add(testCasesButton);
			Grid.SetRow(testCasesButton, 0);

			rootLayout.Add(searchBar);
			Grid.SetRow(searchBar, 1);

			var gcButton = new Button
			{
				Text = "Click to Force GC",
				Command = new Command(() =>
				{
					GC.Collect();
					GC.WaitForPendingFinalizers();
					GC.Collect();
				})
			};
			rootLayout.Add(gcButton);
			Grid.SetRow(gcButton, 2);

			rootLayout.Add(corePageView);
			Grid.SetRow(corePageView, 3);

			// Probe element so UI tests can detect at runtime which Android Shell architecture
			// the app was built with (new ShellHandler vs legacy ShellRenderer). Read by
			// HelperExtensions.IsAndroidShellHandlerEnabled. The probe sits on this gallery
			// page which every test traverses at startup (see _IssuesUITest.NavigateToIssue),
			// so its value is captured before tests navigate away.
			// Visually inert (1px tall, transparent text) but kept in Android's accessibility
			// tree — do NOT set Opacity=0 or InputTransparent=true, both cause Android to
			// strip the view from the a11y tree and Appium can no longer find it.
			var shellModeProbe = new Label
			{
				AutomationId = "MauiAndroidShellMode",
				Text = AppContext.TryGetSwitch("Microsoft.Maui.RuntimeFeature.UseAndroidShellHandlers", out var useHandler) && useHandler
					? "Handler"
					: "Renderer",
				HeightRequest = 1,
				TextColor = Colors.Transparent,
				BackgroundColor = Colors.Transparent
			};
			rootLayout.Add(shellModeProbe);
			Grid.SetRow(shellModeProbe, 0);

			AutomationId = "Gallery";

			Content = rootLayout;
		}
	}
}