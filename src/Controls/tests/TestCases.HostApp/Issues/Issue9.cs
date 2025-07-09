namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 9, "Focus events not working in MAC and iOS", PlatformAffected.macOS | PlatformAffected.iOS)]
public class Issue9 : TestContentPage
{
	Label _buttonFocusedCountLabel = new Label
	{
		Text = "Button Focused: 0",
		AutomationId = "ButtonFocusedCountLabel"
	};
	
	Label _buttonUnfocusedCountLabel = new Label
	{
		Text = "Button Unfocused: 0",
		AutomationId = "ButtonUnfocusedCountLabel"
	};
	
	Label _layoutFocusedCountLabel = new Label
	{
		Text = "Layout Focused: 0",
		AutomationId = "LayoutFocusedCountLabel"
	};
	
	Label _layoutUnfocusedCountLabel = new Label
	{
		Text = "Layout Unfocused: 0",
		AutomationId = "LayoutUnfocusedCountLabel"
	};

	int _buttonFocusedCount;
	int _buttonUnfocusedCount;
	int _layoutFocusedCount;
	int _layoutUnfocusedCount;

	int ButtonFocusedCount
	{
		get { return _buttonFocusedCount; }
		set
		{
			_buttonFocusedCount = value;
			_buttonFocusedCountLabel.Text = $"Button Focused: {value}";
		}
	}

	int ButtonUnfocusedCount
	{
		get { return _buttonUnfocusedCount; }
		set
		{
			_buttonUnfocusedCount = value;
			_buttonUnfocusedCountLabel.Text = $"Button Unfocused: {value}";
		}
	}

	int LayoutFocusedCount
	{
		get { return _layoutFocusedCount; }
		set
		{
			_layoutFocusedCount = value;
			_layoutFocusedCountLabel.Text = $"Layout Focused: {value}";
		}
	}

	int LayoutUnfocusedCount
	{
		get { return _layoutUnfocusedCount; }
		set
		{
			_layoutUnfocusedCount = value;
			_layoutUnfocusedCountLabel.Text = $"Layout Unfocused: {value}";
		}
	}

	protected override void Init()
	{
		var button = new Button
		{
			Text = "Test Button",
			AutomationId = "TestButton"
		};
		
		button.Focused += (sender, e) =>
		{
			ButtonFocusedCount++;
		};
		button.Unfocused += (sender, e) =>
		{
			ButtonUnfocusedCount++;
		};

		var testLayout = new StackLayout
		{
			BackgroundColor = Colors.LightBlue,
			HeightRequest = 100,
			AutomationId = "TestLayout"
		};
		
		testLayout.Focused += (sender, e) =>
		{
			LayoutFocusedCount++;
		};
		testLayout.Unfocused += (sender, e) =>
		{
			LayoutUnfocusedCount++;
		};

		var instructions = new Label
		{
			Text = "Tap the button and layout to test focus events. Focus counts should increment on tap.",
			AutomationId = "Instructions"
		};

		var mainLayout = new StackLayout
		{
			Padding = 20,
			Spacing = 10
		};
		
		mainLayout.Children.Add(instructions);
		mainLayout.Children.Add(button);
		mainLayout.Children.Add(testLayout);
		mainLayout.Children.Add(_buttonFocusedCountLabel);
		mainLayout.Children.Add(_buttonUnfocusedCountLabel);
		mainLayout.Children.Add(_layoutFocusedCountLabel);
		mainLayout.Children.Add(_layoutUnfocusedCountLabel);

		Content = mainLayout;
	}
}