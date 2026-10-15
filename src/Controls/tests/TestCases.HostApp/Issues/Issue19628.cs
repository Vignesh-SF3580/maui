namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19628, "CollectionView Header and EmptyView overlap when there are no items", PlatformAffected.UWP)]
public class Issue19628 : ContentPage
{
	public Issue19628()
	{
		Content = new Grid
		{
			Children =
			{
				new CollectionView2
				{
					AutomationId = "Issue19628CollectionView",
					Header = new Label { Text = "Header here!" },
					EmptyView = new Label
					{
						Text = "No record",
						AutomationId = "Issue19628EmptyView"
					}
				}
			}
		};
	}
}
