using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Packaging;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;

namespace Mita.GithubCopilotRtl
{
    public static class RTLHelper
    {
        // الگوی تشخیص کاراکترهای RTL (فارسی، عربی، عبری و ...)
        private static readonly Regex RtlRegex = new Regex(
            @"[\u0600-\u06FF\u0750-\u077F\u08A0-\u08FF\uFB50-\uFDFF\uFE70-\uFEFF\u0590-\u05FF]",
            RegexOptions.Compiled
        );

        // الگوی تشخیص کاراکترهای انگلیسی و کد
        private static readonly Regex EnglishRegex = new Regex(
            @"[a-zA-Z0-9{}()<>\[\]\/\\|;:'"",.?~!@#$%^&*+=_\-]",
            RegexOptions.Compiled
        );

        
        public static FlowDirection DetectFlowDirection(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return FlowDirection.LeftToRight;

            // حذف whitespace و کاراکترهای خاص برای تشخیص دقیق‌تر
            var cleanText = new string(text.Where(c => !char.IsWhiteSpace(c)).ToArray());
            //var cleanText = text.Trim();
            var firstWord = text.Trim().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[0];

            if (string.IsNullOrEmpty(cleanText))
                return FlowDirection.LeftToRight;

            // بررسی وجود کاراکترهای RTL
            bool hasRtl = RtlRegex.IsMatch(cleanText);

            // بررسی وجود کاراکترهای انگلیسی/کد
            bool hasEnglish = EnglishRegex.IsMatch(cleanText);

            // اگر هر دو نوع وجود داشته باشند، RTL اولویت دارد
            // چون معمولاً متن اصلی RTL است و کد/انگلیسی درون آن قرار دارد
            if (RtlRegex.IsMatch(firstWord))
                return FlowDirection.RightToLeft;

            return FlowDirection.LeftToRight;
        }

        
        public static void ApplySmartRTLToElement(DependencyObject element, bool enable)
        {
            if (element == null) 
                return;

            if (element is FrameworkElement fe)
            {
                string typeName = fe.GetType().Name;

                // 1. EditorViewBox یا SimplePromptBox (جعبه ورودی متن)
                if (typeName == "EditorViewBox" || typeName == "SimplePromptBox")
                {                    
                    //var direction = enable ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
                    //ApplyDirectionToEditorViewBox(fe, direction);
                }
                // 2. ChatMessageView (نمایش پیام‌ها)
                else if (typeName == "ChatMessageView")
                {
                    // پیدا کردن FlowDocument
                    var flowDoc = FindVisualChild<FlowDocument>(fe);
                    if (flowDoc != null)
                    {
                        // استخراج متن از FlowDocument
                        string fullText = GetTextFromFlowDocument(flowDoc);
                        var direction = enable ? DetectFlowDirection(fullText) : FlowDirection.LeftToRight;
                        flowDoc.FlowDirection = direction;
                        Debug.WriteLine($"Applied direction {direction} to FlowDocument");
                    }
                }
                // 3. ChatSessionView یا InlineSessionView
                else if (typeName == "ChatSessionView" || typeName == "InlineSessionView")
                {
                    // کل ویو را RTL نمی‌کنیم، چون ممکن است عناصر مختلفی داشته باشد
                    // فقط ChatMessageViewها را پردازش می‌کنیم
                    foreach (var child in FindVisualChildrenByType(fe, "ChatMessageView"))
                    {
                        if (child is FrameworkElement childFe)
                        {
                            //var doc = FindVisualChild<FlowDocument>(childFe);
                            var dataContext = childFe.GetType().GetProperty("DataContext").GetValue(childFe);
                            var fullText = dataContext.GetType().GetProperty("AutomationText").GetValue(dataContext) as string;

                            var direction = enable ? DetectFlowDirection(fullText) : FlowDirection.LeftToRight;
                            childFe.FlowDirection = direction;
                        }
                    }

                    // همچنین EditorViewBox و SimplePromptBox را پردازش کن
                    foreach (var child in FindVisualChildrenByType(fe, "EditorViewBox"))
                    {
                        ApplySmartRTLToElement(child as FrameworkElement, enable);
                    }
                    foreach (var child in FindVisualChildrenByType(fe, "SimplePromptBox"))
                    {
                        //ApplySmartRTLToElement(child as FrameworkElement, enable);
                    }
                }
                // 4. سایر المان‌های متنی
                else if (fe is System.Windows.Controls.TextBlock textBlock)
                {
                    var direction = enable ? DetectFlowDirection(textBlock.Text) : FlowDirection.LeftToRight;
                    textBlock.FlowDirection = direction;
                }
                else if (fe is System.Windows.Controls.TextBox textBox)
                {
                    var direction = enable ? DetectFlowDirection(textBox.Text) : FlowDirection.LeftToRight;
                    textBox.FlowDirection = direction;
                }
            }

            // ادامه جستجوی بازگشتی
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                ApplySmartRTLToElement(VisualTreeHelper.GetChild(element, i), enable);
            }
        }
        public static void TryDetectCopilotChatAndApplyDirection(bool enable)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            //var uiShell = package.GetServiceAsync(typeof(SVsUIShell)).Result as IVsUIShell;
            var uiShell = ServiceProvider.GlobalProvider.GetService(typeof(SVsUIShell)) as IVsUIShell;

            uiShell.GetToolWindowEnum(out IEnumWindowFrames enumFrames);

            IVsWindowFrame[] frames = new IVsWindowFrame[1];
            uint fetched;

            while (enumFrames.Next(1, frames, out fetched) == VSConstants.S_OK && fetched == 1)
            {
                var root = GetWpfRoot(frames[0]);
                if (root == null)
                    continue;

                var copilot = FindCopilotChatControl(root);

                if (copilot != null)
                {
                    MakeRtl(copilot, enable);
                    return;
                }
            }
        }


        private static void MakeRtl(FrameworkElement copilot, bool enable)
        {
            ApplySmartRTLToElement(copilot, enable);
        }
        private static FrameworkElement GetWpfRoot(IVsWindowFrame frame)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out object docViewObj);

            if (docViewObj is IVsWindowPane pane)
            {
                // در VS 2026 ViewHelper حذف شده
                // باید از WPF interop استفاده کنیم
                if (pane is WindowPane wp)
                {
                    return wp.Content as FrameworkElement;
                }
            }

            return null;
        }
        private static FrameworkElement FindCopilotChatControl(DependencyObject root)
        {
            var result = FindDescendant(root, d =>
            {
                var type = d.GetType().FullName;
                return type != null &&
                       (type.Contains("CopilotChat") ||
                        type.Contains("Copilot") ||
                        type.Contains("ChatView"));
            }) as FrameworkElement;

            return result;
        }
        private static DependencyObject FindDescendant(DependencyObject root, Func<DependencyObject, bool> predicate)
        {
            if (root == null)
                return null;

            if (predicate(root))
                return root;

            int count = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                var result = FindDescendant(child, predicate);
                if (result != null)
                    return result;
            }

            return null;
        }
        private static string GetTextFromFlowDocument(FlowDocument document)
        {
            if (document == null) return string.Empty;

            try
            {
                var textRange = new TextRange(document.ContentStart, document.ContentEnd);
                return textRange.Text;
            }
            catch
            {
                return string.Empty;
            }
        }
        private static string GetTextFromElement(FrameworkElement element)
        {
            if (element == null) return string.Empty;

            try
            {
                // 1. بررسی TextBox
                if (element is System.Windows.Controls.TextBox textBox)
                    return textBox.Text;

                // 2. بررسی TextBlock
                if (element is System.Windows.Controls.TextBlock textBlock)
                    return textBlock.Text;

                // 3. بررسی ContentControl (مثل Button, Label)
                if (element is System.Windows.Controls.ContentControl contentControl)
                {
                    if (contentControl.Content is string str)
                        return str;
                    if (contentControl.Content is System.Windows.Controls.TextBlock innerTextBlock)
                        return innerTextBlock.Text;
                }

                // 4. بررسی EditorViewBox از طریق Reflection
                var typeName = element.GetType().Name;
                if (typeName == "EditorViewBox")
                {
                    var textProp = element.GetType().GetProperty("Text");
                    if (textProp != null)
                    {
                        var value = textProp.GetValue(element);
                        if (value != null)
                            return value.ToString();
                    }
                }

                // 5. بررسی SimplePromptBox
                if (typeName == "SimplePromptBox" && element is System.Windows.Controls.TextBox simpleBox)
                    return simpleBox.Text;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting text from element: {ex.Message}");
            }

            return string.Empty;
        }
        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result) return result;
                var nested = FindVisualChild<T>(child);
                if (nested != null) return nested;
            }
            return null;
        }
        private static IEnumerable<DependencyObject> FindVisualChildrenByType(DependencyObject parent, string typeName)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child.GetType().Name == typeName)
                {
                    yield return child;
                }

                foreach (var nested in FindVisualChildrenByType(child, typeName))
                {
                    yield return nested;
                }
            }
        }
        public static void ApplyDirectionToEditorViewBox(FrameworkElement editorViewBox, FlowDirection direction)
        {
            if (editorViewBox == null) return;
            if (editorViewBox.GetType().Name != "EditorViewBox") return;

            try
            {
                // 1. تنظیم FlowDirection روی خود EditorViewBox
                editorViewBox.FlowDirection = direction;

                // 2. دریافت View (IWpfTextView) از طریق Reflection
                var viewProp = editorViewBox.GetType().GetProperty("View");
                if (viewProp != null)
                {
                    var view = viewProp.GetValue(editorViewBox);
                    if (view != null)
                    {
                        // 3. دریافت VisualElement
                        var visualElementProp = view.GetType().GetProperty("VisualElement");
                        if (visualElementProp != null)
                        {
                            var visualElement = visualElementProp.GetValue(view) as FrameworkElement;
                            if (visualElement != null)
                            {
                                // 4. تنظیم FlowDirection
                                visualElement.FlowDirection = direction;

                                // 5. تنظیم TextAlignment از طریق Reflection
                                var textAlignmentProp = visualElement.GetType().GetProperty("TextAlignment");
                                if (textAlignmentProp != null)
                                {
                                    var alignment = direction == FlowDirection.RightToLeft
                                        ? TextAlignment.Right
                                        : TextAlignment.Left;
                                    textAlignmentProp.SetValue(visualElement, alignment);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error applying direction to EditorViewBox: {ex.Message}");
            }
        }
        public static string GetTextFromEditorViewBox(FrameworkElement editorViewBox)
        {
            if (editorViewBox == null) return string.Empty;
            if (editorViewBox.GetType().Name != "EditorViewBox") return string.Empty;

            try
            {
                var textProp = editorViewBox.GetType().GetProperty("Text");
                if (textProp != null)
                {
                    var value = textProp.GetValue(editorViewBox);
                    if (value != null)
                        return value.ToString();
                }
            }
            catch { }

            return string.Empty;
        }
    }
}
