#nullable disable
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class Editor
	{
		public static void MapText(EditorHandler handler, Editor editor) =>
			MapText((IEditorHandler)handler, editor);

		public static void MapText(IEditorHandler handler, Editor editor)
		{
			Platform.TextExtensions.UpdateText(handler.PlatformView, editor);

			// Any text changes in the editor field require recalculating the CharacterSpacing by regenerating the attributed string to properly apply the spacing and override the current text formatting.
			handler?.UpdateValue(nameof(CharacterSpacing));
		}

		protected override void OnHandlerChanged()
		{
			base.OnHandlerChanged();
			SyncAllowAutoGrowthToPlatform();
		}

		protected override void OnPropertyChanged(string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == nameof(AutoSize))
			{
				SyncAllowAutoGrowthToPlatform();
			}
		}

		void SyncAllowAutoGrowthToPlatform()
		{
			if (Handler?.PlatformView is MauiTextView textView)
			{
				textView.AllowAutoGrowth = AutoSize == EditorAutoSizeOption.TextChanges;
			}
		}
	}
}
