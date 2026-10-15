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
			_headerContentPresenter = GetTemplateChild("HeaderContentPresenter") as Microsoft.UI.Xaml.Controls.ContentPresenter;

			if (_emptyViewContentControl != null)
			{
				_emptyViewContentControl.Content = _emptyView;
				UpdateEmptyViewVisibility(EmptyViewVisibility);
			}
		}

		protected override global::Windows.Foundation.Size ArrangeOverride(global::Windows.Foundation.Size finalSize)
		{
			UpdateEmptyViewMargin(finalSize);
			var arrangedSize = base.ArrangeOverride(finalSize);

			if (_emptyViewContentControl?.Visibility == WVisibility.Visible)
			{
				_formsEmptyView?.Arrange(new Rect(0, 0,
					_emptyViewContentControl.ActualWidth, _emptyViewContentControl.ActualHeight));
			}

			return arrangedSize;
		}

		void UpdateEmptyViewMargin(global::Windows.Foundation.Size finalSize)
		{
			if (_emptyViewContentControl is null)
			{
				return;
			}

			if (ScrollViewer.GetVerticalScrollMode(this) == Microsoft.UI.Xaml.Controls.ScrollMode.Disabled)
			{
				var headerWidth = Header is null ? 0 : Math.Min(_headerContentPresenter?.DesiredSize.Width ?? 0, finalSize.Width);
				_emptyViewContentControl.Margin = FlowDirection == Microsoft.UI.Xaml.FlowDirection.RightToLeft
					? new Microsoft.UI.Xaml.Thickness(0, 0, headerWidth, 0)
					: new Microsoft.UI.Xaml.Thickness(headerWidth, 0, 0, 0);
			}
			else
			{
				var headerHeight = Header is null ? 0 : Math.Min(_headerContentPresenter?.DesiredSize.Height ?? 0, finalSize.Height);
				_emptyViewContentControl.Margin = new Microsoft.UI.Xaml.Thickness(0, headerHeight, 0, 0);
			}
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