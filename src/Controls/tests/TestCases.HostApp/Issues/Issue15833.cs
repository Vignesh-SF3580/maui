namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 15833, "Unloaded event should not trigger when switching tabs", PlatformAffected.iOS)]
public class Issue15833 : Shell
{
	public Issue15833()
	{
		var tabBar = new TabBar { Route = "Main" };
		tabBar.Items.Add(new ShellContent
		{
			Title = "First Tab",
			ContentTemplate = new DataTemplate(typeof(Issue15833Page1))
		});
		tabBar.Items.Add(new ShellContent
		{
			Title = "Second Tab",
			ContentTemplate = new DataTemplate(typeof(Issue15833Page2))
		});
		Items.Add(tabBar);
		Routing.RegisterRoute(nameof(Issue15833Page1), typeof(Issue15833Page1));
		Routing.RegisterRoute(nameof(Issue15833Page2), typeof(Issue15833Page2));
	}
}

public class Issue15833Page1 : ContentPage
{
	public Issue15833Page1()
	{
		var stack = new StackLayout();
		var button = new Button
		{
			Text = "Go To Page2",
			AutomationId = "Page1Button"
		};
		button.Clicked += Button_OnClicked;
		stack.Children.Add(button);
		Content = stack;
	}

	private void Button_OnClicked(object sender, EventArgs e)
	{
		Shell.Current.GoToAsync("Issue15833Page2");
	}
}

public class Issue15833Page2 : ContentPage
{
	private Label statusLabel;

	public Issue15833Page2()
	{
		Title = "Second Tab Page";
		statusLabel = new Label
		{
			Text = "Unloaded event not triggered",
			AutomationId = "Page2Label"
		};
		var stack = new VerticalStackLayout
		{
			Children = { statusLabel }
		};
		Content = stack;
		Unloaded += Issue15833Page2_Unloaded;
	}

	private void Issue15833Page2_Unloaded(object sender, EventArgs e)
	{
		if (statusLabel is not null)
		{
			statusLabel.Text = "Unloaded event triggered";
		}
	}
}
