namespace Microsoft.Maui.Platform
{
	public static class LayoutViewGroupExtensions
	{
		public static void UpdateClipsToBounds(this LayoutViewGroup layoutViewGroup, ILayout layout)
		{
			// Setting ClipsToBounds will trigger the property setter which handles all Android clipping logic
			layoutViewGroup.ClipsToBounds = layout.ClipsToBounds;
			
			// Force a layout pass to ensure the clipping changes take effect
			layoutViewGroup.RequestLayout();
		}
	}
}
