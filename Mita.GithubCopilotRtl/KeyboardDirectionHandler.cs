using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics;
using System.IO.Packaging;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Mita.GitHubCopilotRtl
{
    public static class KeyboardDirectionHandler
    {
        private static bool _isInitialized = false;
        
        public static void Initialize()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (_isInitialized) 
                return;

            // ثبت رویداد برای همه پنجره‌ها
            EventManager.RegisterClassHandler(
                typeof(Window),
                Window.KeyDownEvent,
                new KeyEventHandler(OnKeyDown),
                true
            );

            //// همچنین برای همه پنجره‌های جدید
            //Application.Current.Windows.Changed += OnWindowsCollectionChanged;

            _isInitialized = true;
            Debug.WriteLine("KeyboardDirectionHandler initialized.");
        }

        private static void OnKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
                {
                    var enable = e.Key == Key.RightShift ? true : false;

                    RTLHelper.TryDetectCopilotChatAndApplyDirection(enable);
                    e.Handled = true;
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnKeyDown: {ex.Message}");
            }
        }
    }
}

