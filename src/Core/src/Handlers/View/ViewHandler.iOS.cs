using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;
using PlatformView = UIKit.UIView;

namespace Microsoft.Maui.Handlers
{
	public partial class ViewHandler
	{
		readonly static ConditionalWeakTable<PlatformView, ViewHandler> FocusManagerMapping = new();
		readonly static ConditionalWeakTable<PlatformView, UITapGestureRecognizer> FocusGestureMapping = new();
		readonly static ConditionalWeakTable<PlatformView, object> ButtonFocusEventMapping = new();
		static bool _notificationObserversSet = false;
		static WeakReference<PlatformView>? _currentFocusedView = null;

		static ViewHandler()
		{
			SetupGlobalFocusNotifications();
		}

		static void SetupGlobalFocusNotifications()
		{
			if (_notificationObserversSet)
				return;

			_notificationObserversSet = true;

			// Listen for text field/text view focus changes
			NSNotificationCenter.DefaultCenter.AddObserver(
				UITextField.TextDidBeginEditingNotification,
				OnTextControlDidBeginEditing);

			NSNotificationCenter.DefaultCenter.AddObserver(
				UITextField.TextDidEndEditingNotification,
				OnTextControlDidEndEditing);

			NSNotificationCenter.DefaultCenter.AddObserver(
				UITextView.TextDidBeginEditingNotification,
				OnTextControlDidBeginEditing);

			NSNotificationCenter.DefaultCenter.AddObserver(
				UITextView.TextDidEndEditingNotification,
				OnTextControlDidEndEditing);
		}

		static void OnTextControlDidBeginEditing(NSNotification notification)
		{
			if (notification.Object is PlatformView platformView)
			{
				HandleViewFocusedInternal(platformView);
			}
		}

		static void OnTextControlDidEndEditing(NSNotification notification)
		{
			if (notification.Object is PlatformView platformView)
			{
				HandleViewUnfocusedInternal(platformView);
			}
		}

		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
		public static void MapContextFlyout(IViewHandler handler, IView view)
		{
#if MACCATALYST
			if (view is IContextFlyoutElement contextFlyoutContainer)
			{
				MapContextFlyout(handler, contextFlyoutContainer);
			}
#endif
		}

#if MACCATALYST
		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
		internal static void MapContextFlyout(IElementHandler handler, IContextFlyoutElement contextFlyoutContainer)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"The handler's {nameof(handler.MauiContext)} cannot be null.");

			if (handler.PlatformView is PlatformView uiView)
			{
				MauiUIContextMenuInteraction? currentInteraction = null;

				foreach (var interaction in uiView.Interactions)
				{
					if (interaction is MauiUIContextMenuInteraction menuInteraction)
						currentInteraction = menuInteraction;
				}

				if (contextFlyoutContainer.ContextFlyout != null)
				{
					if (currentInteraction == null)
						uiView.AddInteraction(new MauiUIContextMenuInteraction(handler));
				}
				else if (currentInteraction != null)
				{
					uiView.RemoveInteraction(currentInteraction);
				}
			}
		}
#endif

		partial void ConnectingHandler(PlatformView? platformView)
		{
			if (platformView is not null)
			{
				FocusManagerMapping.Add(platformView, this);
				SetupFocusGestureRecognizer(platformView);
			}
		}

		partial void DisconnectingHandler(PlatformView platformView)
		{
			CleanupFocusGestureRecognizer(platformView);
			FocusManagerMapping.Remove(platformView);
			UpdateIsFocused(false);
		}

		static void SetupFocusGestureRecognizer(PlatformView platformView)
		{
			// Don't add gesture recognizer to text input controls - they handle focus through first responder
			if (platformView is UITextField or UITextView)
				return;

			// Handle UIButton controls (including MauiCheckBox) differently using their TouchUpInside event
			if (platformView is UIButton button)
			{
				SetupButtonFocusHandling(button);
				return;
			}

			// Add tap gesture recognizer to detect focus events for other controls
			var tapGesture = new UITapGestureRecognizer(HandleViewTapped);
			tapGesture.ShouldRecognizeSimultaneously = (recognizer, otherRecognizer) => true;
			platformView.AddGestureRecognizer(tapGesture);
			platformView.UserInteractionEnabled = true;
			
			FocusGestureMapping.Add(platformView, tapGesture);
		}

		static void SetupButtonFocusHandling(UIButton button)
		{
			// Use button's TouchUpInside event to handle focus instead of gesture recognizer
			EventHandler focusHandler = (sender, e) => HandleButtonFocused(button);
			button.TouchUpInside += focusHandler;
			
			// Store the handler so we can remove it later
			ButtonFocusEventMapping.Add(button, focusHandler);
		}

		static void HandleButtonFocused(UIButton button)
		{
			HandleViewFocusedInternal(button);
		}

		static void CleanupFocusGestureRecognizer(PlatformView platformView)
		{
			// Cleanup button focus handler if it's a UIButton
			if (platformView is UIButton button && ButtonFocusEventMapping.TryGetValue(button, out var handlerObj))
			{
				if (handlerObj is EventHandler focusHandler)
				{
					button.TouchUpInside -= focusHandler;
				}
				ButtonFocusEventMapping.Remove(button);
				return;
			}

			// Cleanup gesture recognizer for other controls
			if (FocusGestureMapping.TryGetValue(platformView, out var gestureRecognizer))
			{
				platformView.RemoveGestureRecognizer(gestureRecognizer);
				FocusGestureMapping.Remove(platformView);
				gestureRecognizer?.Dispose();
			}
		}

		static void HandleViewTapped(UITapGestureRecognizer tapGesture)
		{
			if (tapGesture.View is PlatformView platformView)
			{
				HandleViewFocusedInternal(platformView);
			}
		}

		private static void HandleViewFocusedInternal(PlatformView platformView)
		{
			// Unfocus the previously focused view
			if (_currentFocusedView?.TryGetTarget(out var previousView) == true && previousView != platformView)
			{
				HandleViewUnfocusedInternal(previousView);
			}

			// Focus the new view
			_currentFocusedView = new WeakReference<PlatformView>(platformView);
			OnViewBecameFirstResponder(platformView);
		}

		private static void HandleViewUnfocusedInternal(PlatformView platformView)
		{
			OnViewResignedFirstResponder(platformView);
		}

		// Make these methods accessible from ViewExtensions
		internal static void HandleViewFocused(PlatformView platformView) => 
			HandleViewFocusedInternal(platformView);
		
		internal static void HandleViewUnfocused(PlatformView platformView) => 
			HandleViewUnfocusedInternal(platformView);

		private protected void UpdateIsFocused(bool isFocused)
		{
			if (VirtualView is not { } virtualView)
			{
				return;
			}

			bool updateIsFocused = (isFocused && !virtualView.IsFocused) || (!isFocused && virtualView.IsFocused);

			if (updateIsFocused)
			{
				virtualView.IsFocused = isFocused;
			}
		}

		internal static void OnViewBecameFirstResponder(PlatformView platformView)
		{
			if (FocusManagerMapping.TryGetValue(platformView, out ViewHandler? handler))
			{
				handler.UpdateIsFocused(true);
			}
		}

		internal static void OnViewResignedFirstResponder(PlatformView platformView)
		{
			if (FocusManagerMapping.TryGetValue(platformView, out ViewHandler? handler))
			{
				handler.UpdateIsFocused(false);
			}
		}

		static partial void MappingFrame(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapTranslationX(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapTranslationY(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapScale(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapScaleX(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapScaleY(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapRotation(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapRotationX(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapRotationY(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapAnchorX(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapAnchorY(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		internal static void UpdateTransformation(IViewHandler handler, IView view)
		{
			handler.ToPlatform().UpdateTransformation(view);
		}
	}
}