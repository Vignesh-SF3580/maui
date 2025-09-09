using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue9 : _IssuesUITest
{
	const string TestButton = "TestButton";
	const string TestLayout = "TestLayout";
	const string TestCheckBox = "TestCheckBox";
	const string TestRadioButton = "TestRadioButton";
	const string ButtonFocusedCountLabel = "ButtonFocusedCountLabel";
	const string ButtonUnfocusedCountLabel = "ButtonUnfocusedCountLabel";
	const string LayoutFocusedCountLabel = "LayoutFocusedCountLabel";
	const string LayoutUnfocusedCountLabel = "LayoutUnfocusedCountLabel";
	const string CheckBoxFocusedCountLabel = "CheckBoxFocusedCountLabel";
	const string CheckBoxUnfocusedCountLabel = "CheckBoxUnfocusedCountLabel";
	const string RadioButtonFocusedCountLabel = "RadioButtonFocusedCountLabel";
	const string RadioButtonUnfocusedCountLabel = "RadioButtonUnfocusedCountLabel";

	public Issue9(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Focus events not working in MAC and iOS";

	[Test]
	[Category(UITestCategories.Button)]
	[Category(UITestCategories.Layout)]
	public void FocusEventsWorkOnButtonAndLayout()
	{
		App.WaitForElement(TestButton);
		App.WaitForElement(TestLayout);
		
		// Test button focus events
		App.Tap(TestButton);
		var buttonFocusedText = App.FindElement(ButtonFocusedCountLabel).GetText();
		Assert.That(buttonFocusedText, Is.EqualTo("Button Focused: 1"), "Button focused event should fire on tap");
		
		// Test layout focus events
		App.Tap(TestLayout);
		var layoutFocusedText = App.FindElement(LayoutFocusedCountLabel).GetText();
		Assert.That(layoutFocusedText, Is.EqualTo("Layout Focused: 1"), "Layout focused event should fire on tap");
		
		// Tap button again to test unfocus/focus cycle
		App.Tap(TestButton);
		var buttonFocusedText2 = App.FindElement(ButtonFocusedCountLabel).GetText();
		var layoutUnfocusedText = App.FindElement(LayoutUnfocusedCountLabel).GetText();
		
		Assert.That(buttonFocusedText2, Is.EqualTo("Button Focused: 2"), "Button focused event should fire again");
		Assert.That(layoutUnfocusedText, Is.EqualTo("Layout Unfocused: 1"), "Layout unfocused event should fire when button is tapped");
	}

	[Test]
	[Category(UITestCategories.CheckBox)]
	[Category(UITestCategories.RadioButton)]
	public void FocusEventsWorkOnCheckBoxAndRadioButton()
	{
		App.WaitForElement(TestCheckBox);
		App.WaitForElement(TestRadioButton);
		
		// Test checkbox focus events
		App.Tap(TestCheckBox);
		var checkBoxFocusedText = App.FindElement(CheckBoxFocusedCountLabel).GetText();
		Assert.That(checkBoxFocusedText, Is.EqualTo("CheckBox Focused: 1"), "CheckBox focused event should fire on tap");
		
		// Test radiobutton focus events
		App.Tap(TestRadioButton);
		var radioButtonFocusedText = App.FindElement(RadioButtonFocusedCountLabel).GetText();
		Assert.That(radioButtonFocusedText, Is.EqualTo("RadioButton Focused: 1"), "RadioButton focused event should fire on tap");
		
		// Tap checkbox again to test unfocus/focus cycle
		App.Tap(TestCheckBox);
		var checkBoxFocusedText2 = App.FindElement(CheckBoxFocusedCountLabel).GetText();
		var radioButtonUnfocusedText = App.FindElement(RadioButtonUnfocusedCountLabel).GetText();
		
		Assert.That(checkBoxFocusedText2, Is.EqualTo("CheckBox Focused: 2"), "CheckBox focused event should fire again");
		Assert.That(radioButtonUnfocusedText, Is.EqualTo("RadioButton Unfocused: 1"), "RadioButton unfocused event should fire when checkbox is tapped");
	}
}