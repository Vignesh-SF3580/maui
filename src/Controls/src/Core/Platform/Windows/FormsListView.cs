#nullable disable
using System;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using UwpApp = Microsoft.UI.Xaml.Application;
using UwpControlTemplate = Microsoft.UI.Xaml.Controls.ControlTemplate;
using UwpScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility;
using WVisibility = Microsoft.UI.Xaml.Visibility;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class FormsListView : Microsoft.UI.Xaml.Controls.ListView, IEmptyView
	{
		ContentControl _emptyViewContentControl;
		StackPanel _emptyViewHeaderPanel;
		Microsoft.UI.Xaml.Controls.ContentPresenter _headerContentPresenter;
		FrameworkElement _emptyView;
		View _formsEmptyView;

		public FormsListView()
		{
			Template = (UwpControlTemplate)UwpApp.Current.Resources["FormsListViewTemplate"];

			ScrollViewer.SetHorizontalScrollBarVisibility(this, UwpScrollBarVisibility.Disabled);
			ScrollViewer.SetVerticalScrollBarVisibility(this, UwpScrollBarVisibility.Auto);
		}

		public static readonly DependencyProperty EmptyViewVisibilityProperty =
			DependencyProperty.Register(nameof(EmptyViewVisibility), typeof(Visibility),
				typeof(FormsListView), new PropertyMetadata(WVisibility.Collapsed, EmptyViewVisibilityChanged));

		static void EmptyViewVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is FormsListView listView)
			{
				// Update this manually; normally we'd just bind this, but TemplateBinding doesn't seem to work
				// for WASDK right now.
				listView.UpdateEmptyViewVisibility((WVisibility)e.NewValue);
			}
		}

		public WVisibility EmptyViewVisibility
		{
			get
			{
				return (WVisibility)GetValue(EmptyViewVisibilityProperty);
			}
			set
			{
				SetValue(EmptyViewVisibilityProperty, value);
			}
		}

		public void SetEmptyView(FrameworkElement emptyView, View formsEmptyView)
		{
			_emptyView = emptyView;
			_formsEmptyView = formsEmptyView;

			if (_emptyViewContentControl != null)
			{
				_emptyViewContentControl.Content = emptyView;
				UpdateEmptyViewVisibility(EmptyViewVisibility);
			}
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_emptyViewContentControl = GetTemplateChild("EmptyViewContentControl") as ContentControl;
			_emptyViewHeaderPanel = GetTemplateChild("EmptyViewHeaderPanel") as StackPanel;
			_headerContentPresenter = GetTemplateChild("HeaderContentPresenter") as Microsoft.UI.Xaml.Controls.ContentPresenter;

			if (_emptyViewHeaderPanel != null)
			{
				_emptyViewHeaderPanel.Orientation = ScrollViewer.GetVerticalScrollMode(this) == Microsoft.UI.Xaml.Controls.ScrollMode.Disabled
					? Orientation.Horizontal : Orientation.Vertical;
			}

			if (_emptyViewContentControl != null)
			{
				_emptyViewContentControl.Content = _emptyView;
				UpdateEmptyViewVisibility(EmptyViewVisibility);
			}
		}

		protected override global::Windows.Foundation.Size MeasureOverride(global::Windows.Foundation.Size availableSize)
		{
			var isHorizontal = ScrollViewer.GetVerticalScrollMode(this) == Microsoft.UI.Xaml.Controls.ScrollMode.Disabled;
			if (_emptyViewHeaderPanel != null)
			{
				_emptyViewHeaderPanel.Orientation = isHorizontal ? Orientation.Horizontal : Orientation.Vertical;
			}

			var measuredSize = base.MeasureOverride(availableSize);
			if (_emptyViewContentControl?.Visibility != WVisibility.Visible)
			{
				return measuredSize;
			}

			var availableExtent = isHorizontal
				? availableSize.Width - BorderThickness.Left - BorderThickness.Right - Padding.Left - Padding.Right
				: availableSize.Height - BorderThickness.Top - BorderThickness.Bottom - Padding.Top - Padding.Bottom;
			var headerExtent = Header is null ? 0 : (isHorizontal
				? _headerContentPresenter?.DesiredSize.Width ?? 0
				: _headerContentPresenter?.DesiredSize.Height ?? 0);
			// A minimum preserves viewport fill without preventing larger content from scrolling.
			var minimumExtent = double.IsInfinity(availableExtent) ? 0 : Math.Max(0, availableExtent - headerExtent);
			var minWidth = isHorizontal ? minimumExtent : 0;
			var minHeight = isHorizontal ? 0 : minimumExtent;
			if (_emptyViewContentControl.MinWidth != minWidth || _emptyViewContentControl.MinHeight != minHeight)
			{
				_emptyViewContentControl.MinWidth = minWidth;
				_emptyViewContentControl.MinHeight = minHeight;
				return base.MeasureOverride(availableSize);
			}

			return measuredSize;
		}

		protected override global::Windows.Foundation.Size ArrangeOverride(global::Windows.Foundation.Size finalSize)
		{
			var arrangedSize = base.ArrangeOverride(finalSize);

			if (_emptyViewContentControl?.Visibility == WVisibility.Visible)
			{
				_formsEmptyView?.Arrange(new Rect(0, 0,
					_emptyViewContentControl.ActualWidth, _emptyViewContentControl.ActualHeight));
			}

			return arrangedSize;
		}

		protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
		{
			GroupFooterItemTemplateContext.EnsureSelectionDisabled(element, item);
			base.PrepareContainerForItemOverride(element, item);
		}

		void UpdateEmptyViewVisibility(WVisibility visibility)
		{
			if (_emptyViewContentControl is null)
			{
				return;
			}

			_emptyViewContentControl.Visibility = visibility;
		}
	}
}