using System;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 5, "[iOS] Shell page Unloaded event fires when returning to it from another tab", PlatformAffected.iOS)]
	public class Issue5 : Shell
	{
		public Issue5()
		{
			var tab1 = new ShellContent
			{
				Title = "Tab 1",
				Content = new Issue5_Tab1Page()
			};

			var tab2 = new ShellContent
			{
				Title = "Tab 2", 
				Content = new Issue5_Tab2Page()
			};

			Items.Add(new TabBar
			{
				Items = { tab1, tab2 }
			});
		}

		public class Issue5_Tab1Page : ContentPage
		{
			static int unloadedCount = 0;
			static int loadedCount = 0;
			Label statusLabel;
			Button navigateButton;

			public Issue5_Tab1Page()
			{
				Title = "Tab 1";
				
				statusLabel = new Label
				{
					Text = $"Loaded: {loadedCount}, Unloaded: {unloadedCount}",
					AutomationId = "StatusLabel"
				};

				navigateButton = new Button
				{
					Text = "Navigate to Second Page",
					AutomationId = "NavigateButton"
				};
				navigateButton.Clicked += OnNavigateClicked;

				Content = new StackLayout
				{
					Children = { statusLabel, navigateButton }
				};

				Loaded += OnPageLoaded;
				Unloaded += OnPageUnloaded;
			}

			private async void OnNavigateClicked(object sender, EventArgs e)
			{
				await Navigation.PushAsync(new Issue5_SecondPage());
			}

			private void OnPageLoaded(object sender, EventArgs e)
			{
				loadedCount++;
				UpdateStatus();
			}

			private void OnPageUnloaded(object sender, EventArgs e)
			{
				unloadedCount++;
				UpdateStatus();
			}

			private void UpdateStatus()
			{
				if (statusLabel != null)
				{
					statusLabel.Text = $"Loaded: {loadedCount}, Unloaded: {unloadedCount}";
				}
			}
		}

		public class Issue5_Tab2Page : ContentPage
		{
			public Issue5_Tab2Page()
			{
				Title = "Tab 2";
				Content = new StackLayout
				{
					Children = {
						new Label { Text = "This is Tab 2", AutomationId = "Tab2Label" }
					}
				};
			}
		}

		public class Issue5_SecondPage : ContentPage
		{
			static int unloadedCount = 0;
			static int loadedCount = 0;
			Label statusLabel;

			public Issue5_SecondPage()
			{
				Title = "Second Page";
				
				statusLabel = new Label
				{
					Text = $"Second Page - Loaded: {loadedCount}, Unloaded: {unloadedCount}",
					AutomationId = "SecondPageStatusLabel"
				};

				var backButton = new Button
				{
					Text = "Go Back",
					AutomationId = "BackButton"
				};
				backButton.Clicked += async (s, e) => await Navigation.PopAsync();

				Content = new StackLayout
				{
					Children = { statusLabel, backButton }
				};

				Loaded += OnPageLoaded;
				Unloaded += OnPageUnloaded;
			}

			private void OnPageLoaded(object sender, EventArgs e)
			{
				loadedCount++;
				UpdateStatus();
			}

			private void OnPageUnloaded(object sender, EventArgs e)
			{
				unloadedCount++;
				UpdateStatus();
			}

			private void UpdateStatus()
			{
				if (statusLabel != null)
				{
					statusLabel.Text = $"Second Page - Loaded: {loadedCount}, Unloaded: {unloadedCount}";
				}
			}
		}
	}
}