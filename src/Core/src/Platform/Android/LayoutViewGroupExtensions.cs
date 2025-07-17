namespace Microsoft.Maui.Platform
{
	public static class LayoutViewGroupExtensions
	{
		public static void UpdateClipsToBounds(this LayoutViewGroup layoutViewGroup, ILayout layout)
		{
			layoutViewGroup.ClipsToBounds = layout.ClipsToBounds;
			// Also update SetClipChildren to ensure consistent clipping behavior
			layoutViewGroup.SetClipChildren(layout.ClipsToBounds);
			layoutViewGroup.RequestLayout();
		}
	}
}
