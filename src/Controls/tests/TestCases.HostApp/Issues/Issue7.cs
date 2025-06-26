using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
    [Issue(IssueTracker.Github, 7, 
        "[regression/8.0.3] [iOS] Layout issue with HorizontalStackLayout visibility and BindableLayout", 
        PlatformAffected.iOS)]
    public class Issue7 : TestContentPage
    {
        const string HideAndAddButton = "HideAndAddButton";
        const string ShowBarButton = "ShowBarButton";
        const string TopBar = "TopBar";
        const string TestLabel = "TestLabel";

        public ObservableCollection<string> TestData { get; } = new ObservableCollection<string> { "item", "item" };

        protected override void Init()
        {
            var topBarGrid = new Grid
            {
                AutomationId = TopBar
            };

            var horizontalStackLayout = new HorizontalStackLayout();
            
            var bindableLayout = BindableLayout.SetItemsSource(horizontalStackLayout, TestData);
            BindableLayout.SetItemTemplate(horizontalStackLayout, new DataTemplate(() =>
            {
                var verticalStack = new VerticalStackLayout
                {
                    WidthRequest = 25,
                    VerticalOptions = LayoutOptions.Center,
                    Spacing = 2
                };

                var ellipse = new Ellipse
                {
                    Fill = Brush.Black,
                    HeightRequest = 13,
                    WidthRequest = 13,
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 5, 0, 0)
                };

                var label = new Label
                {
                    Text = "Lbl",
                    TextColor = Colors.Black,
                    HorizontalTextAlignment = TextAlignment.Center
                };

                verticalStack.Children.Add(ellipse);
                verticalStack.Children.Add(label);

                return verticalStack;
            }));

            var testLabel = new Label
            {
                Text = "Test",
                AutomationId = TestLabel,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center,
                HeightRequest = 58
            };

            horizontalStackLayout.Children.Add(testLabel);
            topBarGrid.Children.Add(horizontalStackLayout);

            var scrollView = new ScrollView();
            var mainStack = new VerticalStackLayout
            {
                Padding = new Thickness(30, 0),
                Spacing = 25
            };

            var contentLabel = new Label { Text = "Content" };

            var hideAndAddButton = new Button
            {
                Text = "Hide and add item",
                AutomationId = HideAndAddButton
            };
            hideAndAddButton.Clicked += (sender, e) =>
            {
                topBarGrid.IsVisible = false;
                TestData.Add("item");
            };

            var showBarButton = new Button
            {
                Text = "Show Bar",
                AutomationId = ShowBarButton
            };
            showBarButton.Clicked += (sender, e) => topBarGrid.IsVisible = true;

            mainStack.Children.Add(contentLabel);
            mainStack.Children.Add(hideAndAddButton);
            mainStack.Children.Add(showBarButton);
            scrollView.Content = mainStack;

            var mainGrid = new Grid
            {
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Star }
                }
            };

            mainGrid.Children.Add(topBarGrid);
            Grid.SetRow(scrollView, 1);
            mainGrid.Children.Add(scrollView);

            Content = mainGrid;
        }
    }
}