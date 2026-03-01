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

	Label _checkBoxFocusedCountLabel = new Label
	{
		Text = "CheckBox Focused: 0",
		AutomationId = "CheckBoxFocusedCountLabel"
	};

	Label _checkBoxUnfocusedCountLabel = new Label
	{
		Text = "CheckBox Unfocused: 0",
		AutomationId = "CheckBoxUnfocusedCountLabel"
	};

	Label _radioButtonFocusedCountLabel = new Label
	{
		Text = "RadioButton Focused: 0",
		AutomationId = "RadioButtonFocusedCountLabel"
	};

	Label _radioButtonUnfocusedCountLabel = new Label
	{
		Text = "RadioButton Unfocused: 0",
		AutomationId = "RadioButtonUnfocusedCountLabel"
	};

	int _buttonFocusedCount;
	int _buttonUnfocusedCount;
	int _layoutFocusedCount;
	int _layoutUnfocusedCount;
	int _checkBoxFocusedCount;
	int _checkBoxUnfocusedCount;
	int _radioButtonFocusedCount;
	int _radioButtonUnfocusedCount;

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

	int CheckBoxFocusedCount
	{
		get { return _checkBoxFocusedCount; }
		set
		{
			_checkBoxFocusedCount = value;
			_checkBoxFocusedCountLabel.Text = $"CheckBox Focused: {value}";
		}
	}

	int CheckBoxUnfocusedCount
	{
		get { return _checkBoxUnfocusedCount; }
		set
		{
			_checkBoxUnfocusedCount = value;
			_checkBoxUnfocusedCountLabel.Text = $"CheckBox Unfocused: {value}";
		}
	}

	int RadioButtonFocusedCount
	{
		get { return _radioButtonFocusedCount; }
		set
		{
			_radioButtonFocusedCount = value;
			_radioButtonFocusedCountLabel.Text = $"RadioButton Focused: {value}";
		}
	}

	int RadioButtonUnfocusedCount
	{
		get { return _radioButtonUnfocusedCount; }
		set
		{
			_radioButtonUnfocusedCount = value;
			_radioButtonUnfocusedCountLabel.Text = $"RadioButton Unfocused: {value}";
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

		var testCheckBox = new CheckBox
		{
			AutomationId = "TestCheckBox"
		};

		testCheckBox.Focused += (sender, e) =>
		{
			CheckBoxFocusedCount++;
		};
		testCheckBox.Unfocused += (sender, e) =>
		{
			CheckBoxUnfocusedCount++;
		};

		var testRadioButton = new RadioButton
		{
			Content = "Test RadioButton",
			AutomationId = "TestRadioButton"
		};

		testRadioButton.Focused += (sender, e) =>
		{
			RadioButtonFocusedCount++;
		};
		testRadioButton.Unfocused += (sender, e) =>
		{
			RadioButtonUnfocusedCount++;
		};

		var instructions = new Label
		{
			Text = "Tap the controls to test focus events. Focus counts should increment on tap.",
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
		mainLayout.Children.Add(testCheckBox);
		mainLayout.Children.Add(testRadioButton);
		mainLayout.Children.Add(_buttonFocusedCountLabel);
		mainLayout.Children.Add(_buttonUnfocusedCountLabel);
		mainLayout.Children.Add(_layoutFocusedCountLabel);
		mainLayout.Children.Add(_layoutUnfocusedCountLabel);
		mainLayout.Children.Add(_checkBoxFocusedCountLabel);
		mainLayout.Children.Add(_checkBoxUnfocusedCountLabel);
		mainLayout.Children.Add(_radioButtonFocusedCountLabel);
		mainLayout.Children.Add(_radioButtonUnfocusedCountLabel);

		Content = mainLayout;
	}
}