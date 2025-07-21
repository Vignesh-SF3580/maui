namespace Microsoft.Maui.Platform
{
	public static class LayoutViewExtensions
	{
		public static void UpdateClipsToBounds(this LayoutView layoutView, ILayout layout)
		{
			layoutView.ClipsToBounds = layout.ClipsToBounds;
			
			// Force a visual update by marking the view as needing display
			layoutView.SetNeedsDisplay();
		}
	}
}