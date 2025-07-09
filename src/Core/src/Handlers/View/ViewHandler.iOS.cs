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
		static bool _notificationObserversSet = false;

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
				OnViewBecameFirstResponder(platformView);
			}
		}

		static void OnTextControlDidEndEditing(NSNotification notification)
		{
			if (notification.Object is PlatformView platformView)
			{
				OnViewResignedFirstResponder(platformView);
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
			}
		}

		partial void DisconnectingHandler(PlatformView platformView)
		{
			FocusManagerMapping.Remove(platformView);
			UpdateIsFocused(false);
		}

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