namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 27126, "Tap gesture on a button inside an AbsoluteLayout does not respond on iOS", PlatformAffected.iOS)]
public class Issue27126 : ContentPage
{
	public Issue27126()
	{
		var tapCount = 0;
		var resultLabel = new Label
		{
			Text = "Button tapped: 0 times",
			AutomationId = "ButtonTapCount"
		};

		var button = new Button
		{
			Text = "Tap Me",
			AutomationId = "ButtonWithTapGesture",
			BackgroundColor = Colors.Green,
			TextColor = Colors.White
		};
		button.GestureRecognizers.Add(new TapGestureRecognizer
		{
			Command = new Command(() =>
			{
				tapCount++;
				resultLabel.Text = $"Button tapped: {tapCount} times";
			})
		});

		var layout = new AbsoluteLayout
		{
			HeightRequest = 400,
			BackgroundColor = Colors.LightGray
		};

		AbsoluteLayout.SetLayoutBounds(button, new Rect(10, 50, 200, 50));
		AbsoluteLayout.SetLayoutBounds(resultLabel, new Rect(10, 120, 250, 50));
		layout.Children.Add(button);
		layout.Children.Add(resultLabel);

		Content = layout;
	}
}
