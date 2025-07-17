namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, "99999", "IsClippedToBounds Property is not working for Android, MAC and IOS", PlatformAffected.iOS | PlatformAffected.Android | PlatformAffected.macOS)]
public class Issue99999 : TestContentPage
{
	const string ToggleClippingButton = "ToggleClippingButton";
	const string ParentLayout = "ParentLayout";
	const string ChildView = "ChildView";
	const string StatusLabel = "StatusLabel";

	private AbsoluteLayout? _parentLayout;
	private ContentView? _childView;
	private Label? _statusLabel;

	protected override void Init()
	{
		// Create a parent layout with smaller bounds
		_parentLayout = new AbsoluteLayout
		{
			AutomationId = ParentLayout,
			BackgroundColor = Colors.LightBlue,
			WidthRequest = 300,
			HeightRequest = 50,
			IsClippedToBounds = true // Start with clipping enabled
		};

		// Create a child view that overflows the parent bounds
		_childView = new ContentView
		{
			AutomationId = ChildView,
			BackgroundColor = Colors.Red,
			WidthRequest = 300,
			HeightRequest = 100, // Taller than parent to test overflow
			Content = new Label
			{
				Text = ".NET Multi-platform App UI (.NET MAUI) lets you build native apps",
				BackgroundColor = Colors.Yellow,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			}
		};

		// Add child to parent layout
		_parentLayout.Children.Add(_childView);
		AbsoluteLayout.SetLayoutBounds(_childView, new Rect(0, 0, 300, 100));

		_statusLabel = new Label
		{
			AutomationId = StatusLabel,
			Text = "Clipping: ON (child should be clipped to parent bounds)",
			Margin = new Thickness(10)
		};

		var toggleButton = new Button
		{
			AutomationId = ToggleClippingButton,
			Text = "Toggle IsClippedToBounds",
			Margin = new Thickness(10)
		};

		toggleButton.Clicked += OnToggleClippingClicked;

		Content = new StackLayout
		{
			Spacing = 20,
			Margin = new Thickness(20),
			Children =
			{
				new Label
				{
					Text = "IsClippedToBounds Test",
					FontSize = 18,
					FontAttributes = FontAttributes.Bold
				},
				_statusLabel,
				toggleButton,
				new Label
				{
					Text = "Parent Layout (300x50, LightBlue):",
					FontSize = 14
				},
				_parentLayout,
				new Label
				{
					Text = "The red child view (300x100) should overflow when clipping is OFF, and be clipped when clipping is ON.",
					FontSize = 12,
					TextColor = Colors.Gray
				}
			}
		};
	}

	private void OnToggleClippingClicked(object? sender, EventArgs e)
	{
		if (_parentLayout != null && _statusLabel != null)
		{
			_parentLayout.IsClippedToBounds = !_parentLayout.IsClippedToBounds;
			_statusLabel.Text = _parentLayout.IsClippedToBounds 
				? "Clipping: ON (child should be clipped to parent bounds)" 
				: "Clipping: OFF (child should overflow parent bounds)";
		}
	}
}