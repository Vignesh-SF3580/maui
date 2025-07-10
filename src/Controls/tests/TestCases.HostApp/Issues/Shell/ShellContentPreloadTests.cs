using System;
using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.None, 0, "Shell flyout navigation performance with content preloading")]
	public class ShellContentPreloadTests : Shell
	{
		public ShellContentPreloadTests()
		{
			// Enable content preloading to improve navigation performance
			IsContentPreloadEnabled = true;

			var mainPageContent = new ShellContent()
			{
				Title = "Main Page",
				Route = "MainPage",
				ContentTemplate = new DataTemplate(typeof(MainTestPage))
			};

			var testPageContent = new ShellContent()
			{
				Title = "Test Page",
				Route = "TestPage",
				ContentTemplate = new DataTemplate(typeof(ComplexTestPage)),
				// Enable individual preloading for this content
				IsContentPreloadEnabled = true
			};

			var testPageWithoutPreloadContent = new ShellContent()
			{
				Title = "Test Page No Preload",
				Route = "TestPageNoPreload", 
				ContentTemplate = new DataTemplate(typeof(ComplexTestPage)),
				// Disable preloading for comparison
				IsContentPreloadEnabled = false
			};

			Items.Add(new FlyoutItem()
			{
				Title = "Main Page",
				Items =
				{
					new ShellSection()
					{
						Items = { mainPageContent }
					}
				}
			});

			Items.Add(new FlyoutItem()
			{
				Title = "Test Page (Preloaded)",
				Items =
				{
					new ShellSection()
					{
						Items = { testPageContent }
					}
				}
			});

			Items.Add(new FlyoutItem()
			{
				Title = "Test Page (No Preload)",
				Items =
				{
					new ShellSection()
					{
						Items = { testPageWithoutPreloadContent }
					}
				}
			});
		}
	}

	public class MainTestPage : ContentPage
	{
		public MainTestPage()
		{
			Title = "Main Test Page";
			Content = new VerticalStackLayout
			{
				Spacing = 10,
				Padding = 30,
				Children =
				{
					new Label
					{
						Text = "Shell Content Preloading Test",
						FontSize = 20,
						HorizontalOptions = LayoutOptions.Center,
						AutomationId = "MainPageLabel"
					},
					new Label
					{
						Text = "Use the flyout to navigate to test pages.\nPreloaded pages should navigate faster.",
						HorizontalOptions = LayoutOptions.Center
					},
					new Button
					{
						Text = "Trigger Manual Preload",
						AutomationId = "TriggerPreloadButton",
						Command = new Command(() =>
						{
							// Test manual preloading
							if (Parent is Shell shell)
							{
								foreach (var item in shell.Items)
								{
									foreach (var section in item.Items)
									{
										foreach (var content in section.Items)
										{
											if (content is ShellContent shellContent)
											{
												shellContent.PreloadContent();
											}
										}
									}
								}
							}
						})
					}
				}
			};
		}
	}

	public class ComplexTestPage : ContentPage
	{
		public ComplexTestPage()
		{
			Title = "Complex Test Page";
			
			// Create a page with many controls to simulate the performance issue
			var stackLayout = new VerticalStackLayout
			{
				Spacing = 10,
				Padding = 30
			};

			// Add welcome label
			stackLayout.Children.Add(new Label
			{
				Text = "Complex Test Page - Many Controls",
				FontSize = 18,
				HorizontalOptions = LayoutOptions.Center,
				AutomationId = "ComplexPageLabel"
			});

			// Add many Entry controls to simulate the original issue
			for (int i = 1; i <= 30; i++)
			{
				stackLayout.Children.Add(new Entry
				{
					Text = $"Entry{i}",
					Placeholder = $"Placeholder for Entry {i}",
					AutomationId = $"Entry{i}"
				});
			}

			// Add some additional complex controls
			stackLayout.Children.Add(new Button
			{
				Text = "Test Button",
				AutomationId = "TestButton"
			});

			stackLayout.Children.Add(new Slider
			{
				Minimum = 0,
				Maximum = 100,
				Value = 50,
				AutomationId = "TestSlider"
			});

			stackLayout.Children.Add(new ProgressBar
			{
				Progress = 0.75,
				AutomationId = "TestProgressBar"
			});

			Content = new ScrollView
			{
				Content = stackLayout
			};
		}
	}
}