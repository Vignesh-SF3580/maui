namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 13, "IsClippedToBounds Property is not working for Android, MAC and IOS", PlatformAffected.iOS | PlatformAffected.Android | PlatformAffected.macOS)]
public class Issue13 : TestContentPage
{
	const string ToggleClippingButton = "ToggleClippingButton";
	const string ParentLayoutDirect = "ParentLayoutDirect";
	const string ParentLayoutCustomAbsolute = "ParentLayoutCustomAbsolute";
	const string ParentLayoutCustomStack = "ParentLayoutCustomStack";
	const string ChildView = "ChildView";
	const string StatusLabel = "StatusLabel";

	private AbsoluteLayout? _directLayout;
	private CustomAbsoluteLayout? _customAbsoluteLayout;
	private CustomStackLayout? _customStackLayout;
	private Label? _statusLabel;

	protected override void Init()
	{
		// Test 1: Direct AbsoluteLayout usage (this should work)
		_directLayout = new AbsoluteLayout
		{
			AutomationId = ParentLayoutDirect,
			BackgroundColor = Colors.LightBlue,
			WidthRequest = 300,
			HeightRequest = 50,
			IsClippedToBounds = true
		};

		var directChild = CreateTestChild("Direct");
		_directLayout.Children.Add(directChild);
		AbsoluteLayout.SetLayoutBounds(directChild, new Rect(0, 0, 300, 100));

		// Test 2: Custom AbsoluteLayout (this reproduces the issue)
		_customAbsoluteLayout = new CustomAbsoluteLayout
		{
			AutomationId = ParentLayoutCustomAbsolute,
			BackgroundColor = Colors.LightGreen,
			WidthRequest = 300,
			HeightRequest = 50,
			IsClippedToBounds = true
		};

		var customAbsoluteChild = CreateTestChild("Custom Absolute");
		_customAbsoluteLayout.Children.Add(customAbsoluteChild);
		AbsoluteLayout.SetLayoutBounds(customAbsoluteChild, new Rect(0, 0, 300, 100));

		// Test 3: Custom StackLayout (this also reproduces the issue)
		_customStackLayout = new CustomStackLayout
		{
			AutomationId = ParentLayoutCustomStack,
			BackgroundColor = Colors.LightCoral,
			WidthRequest = 300,
			HeightRequest = 50,
			IsClippedToBounds = true
		};

		var customStackChild = CreateTestChild("Custom Stack");
		_customStackLayout.Children.Add(customStackChild);

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
					Text = "IsClippedToBounds Test - Custom Layout Issue",
					FontSize = 18,
					FontAttributes = FontAttributes.Bold
				},
				_statusLabel,
				toggleButton,
				new Label
				{
					Text = "1. Direct AbsoluteLayout (should work):",
					FontSize = 14,
					FontAttributes = FontAttributes.Bold
				},
				_directLayout,
				new Label
				{
					Text = "2. Custom AbsoluteLayout (reproduces issue):",
					FontSize = 14,
					FontAttributes = FontAttributes.Bold
				},
				_customAbsoluteLayout,
				new Label
				{
					Text = "3. Custom StackLayout (reproduces issue):",
					FontSize = 14,
					FontAttributes = FontAttributes.Bold
				},
				_customStackLayout,
				new Label
				{
					Text = "The red child views should overflow when clipping is OFF, and be clipped when clipping is ON. Custom layouts may not behave correctly.",
					FontSize = 12,
					TextColor = Colors.Gray
				}
			}
		};
	}

	private ContentView CreateTestChild(string labelText)
	{
		return new ContentView
		{
			BackgroundColor = Colors.Red,
			WidthRequest = 300,
			HeightRequest = 100, // Taller than parent to test overflow
			Content = new Label
			{
				Text = $"{labelText}: .NET Multi-platform App UI (.NET MAUI) lets you build native apps",
				BackgroundColor = Colors.Yellow,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				FontSize = 10
			}
		};
	}

	private void OnToggleClippingClicked(object? sender, EventArgs e)
	{
		if (_directLayout != null && _customAbsoluteLayout != null && _customStackLayout != null && _statusLabel != null)
		{
			var newClippingValue = !_directLayout.IsClippedToBounds;
			
			_directLayout.IsClippedToBounds = newClippingValue;
			_customAbsoluteLayout.IsClippedToBounds = newClippingValue;
			_customStackLayout.IsClippedToBounds = newClippingValue;
			
			_statusLabel.Text = newClippingValue 
				? "Clipping: ON (child should be clipped to parent bounds)" 
				: "Clipping: OFF (child should overflow parent bounds)";
		}
	}
}

// Custom layouts to reproduce the issue
public class CustomAbsoluteLayout : AbsoluteLayout
{
	// This inherits from AbsoluteLayout and should reproduce the issue
}

public class CustomStackLayout : StackLayout
{
	// This inherits from StackLayout and should reproduce the issue
}