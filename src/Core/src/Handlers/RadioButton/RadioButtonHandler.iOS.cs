using System;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class RadioButtonHandler : ViewHandler<IRadioButton, ContentView>
	{
		readonly RadioButtonEventProxy _proxy = new();

		protected override ContentView CreatePlatformView()
		{
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a {nameof(ContentView)}");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} cannot be null");

			return new SemanticSwitchContentView(VirtualView);
		}

		protected override void ConnectHandler(ContentView platformView)
		{
			base.ConnectHandler(platformView);
			_proxy.Connect(VirtualView, platformView);
		}

		protected override void DisconnectHandler(ContentView platformView)
		{
			base.DisconnectHandler(platformView);
			_proxy.Disconnect(platformView);
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			PlatformView.View = view;
			PlatformView.CrossPlatformLayout = VirtualView;
		}

		static void UpdateContent(IRadioButtonHandler handler)
		{
			_ = handler.PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = handler.VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			// Cleanup the old view when reused
			handler.PlatformView.ClearSubviews();

			if (handler.VirtualView.PresentedContent is IView view)
				handler.PlatformView.AddSubview(view.ToPlatform(handler.MauiContext));
		}

		public static void MapContent(IRadioButtonHandler handler, IContentView page)
		{
			UpdateContent(handler);
		}

		public static void MapIsChecked(IRadioButtonHandler handler, IRadioButton radioButton)
		{
			if (radioButton.IsChecked)
				handler.PlatformView.AccessibilityValue = "1";
			else
				handler.PlatformView.AccessibilityValue = "0";
		}

		[MissingMapper]
		public static void MapTextColor(IRadioButtonHandler handler, ITextStyle textStyle) { }

		[MissingMapper]
		public static void MapCharacterSpacing(IRadioButtonHandler handler, ITextStyle textStyle) { }

		[MissingMapper]
		public static void MapFont(IRadioButtonHandler handler, ITextStyle textStyle) { }

		[MissingMapper]
		public static void MapStrokeColor(IRadioButtonHandler handler, IRadioButton radioButton) { }

		[MissingMapper]
		public static void MapStrokeThickness(IRadioButtonHandler handler, IRadioButton radioButton) { }

		[MissingMapper]
		public static void MapCornerRadius(IRadioButtonHandler handler, IRadioButton radioButton) { }

		class RadioButtonEventProxy
		{
			WeakReference<IRadioButton>? _virtualView;
			UITapGestureRecognizer? _tapGestureRecognizer;

			IRadioButton? VirtualView => _virtualView is not null && _virtualView.TryGetTarget(out var v) ? v : null;

			public void Connect(IRadioButton virtualView, ContentView platformView)
			{
				_virtualView = new(virtualView);

				// Add tap gesture recognizer to detect focus events
				_tapGestureRecognizer = new UITapGestureRecognizer(OnTapped);
				_tapGestureRecognizer.ShouldRecognizeSimultaneously = (recognizer, otherRecognizer) => true;
				platformView.AddGestureRecognizer(_tapGestureRecognizer);
				platformView.UserInteractionEnabled = true;
			}

			public void Disconnect(ContentView platformView)
			{
				_virtualView = null;

				if (_tapGestureRecognizer != null)
				{
					platformView.RemoveGestureRecognizer(_tapGestureRecognizer);
					_tapGestureRecognizer?.Dispose();
					_tapGestureRecognizer = null;
				}
			}

			void OnTapped()
			{
				// Handle focus event following Editor/Entry pattern with proper focus management
				if (VirtualView is IView view)
					ViewHandler.SetViewFocused(view);
			}
		}
	}
}