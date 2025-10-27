namespace Microsoft.Maui.Platform
{
	public static class LayoutViewExtensions
	{
		public static void UpdateClipsToBounds(this LayoutView layoutView, ILayout layout)
		{
			layoutView.ClipsToBounds = layout.ClipsToBounds;
			
			// Try a more aggressive approach to ensure clipping changes take effect
			layoutView.SetNeedsDisplay();
			
			// Force the superview to re-layout as well since clipping affects child positioning
			layoutView.Superview?.SetNeedsLayout();
			layoutView.Superview?.LayoutIfNeeded();
		}
	}
}