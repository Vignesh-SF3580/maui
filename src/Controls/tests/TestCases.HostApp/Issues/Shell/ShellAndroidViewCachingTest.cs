using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues.Shell
{
	[Issue(IssueTracker.Github, 11, "Shell flyout navigation lag on Android with complex pages")]
	public class ShellAndroidViewCachingTest : TestShell
	{
		public ShellAndroidViewCachingTest()
		{
			// Create a shell with tabs that contain complex pages
			var tab1 = new Tab()
			{
				Title = "Complex Page 1",
				Route = "tab1"
			};
			tab1.Items.Add(new ShellContent()
			{
				Title = "Page 1",
				Route = "page1",
				ContentTemplate = new DataTemplate(() => new ComplexPageWithManyControls("Page 1"))
			});

			var tab2 = new Tab()
			{
				Title = "Complex Page 2", 
				Route = "tab2"
			};
			tab2.Items.Add(new ShellContent()
			{
				Title = "Page 2",
				Route = "page2",
				ContentTemplate = new DataTemplate(() => new ComplexPageWithManyControls("Page 2"))
			});

			Items.Add(new TabBar()
			{
				Items = { tab1, tab2 }
			});
		}
	}

	/// <summary>
	/// A page with many controls to reproduce the Android Shell flyout navigation lag
	/// </summary>
	public class ComplexPageWithManyControls : ContentPage
	{
		public ComplexPageWithManyControls(string title)
		{
			Title = title;
			
			var stack = new StackLayout();
			
			// Add many Entry controls to simulate complex layout that causes lag
			for (int i = 0; i < 30; i++)
			{
				stack.Children.Add(new Entry
				{
					Text = $"Entry {i + 1} on {title}",
					AutomationId = $"entry_{i}"
				});
			}
			
			// Add some Labels as well
			for (int i = 0; i < 20; i++)
			{
				stack.Children.Add(new Label
				{
					Text = $"Label {i + 1} on {title}",
					AutomationId = $"label_{i}"
				});
			}
			
			Content = new ScrollView
			{
				Content = stack,
				AutomationId = "scroll_view"
			};
		}
	}
}