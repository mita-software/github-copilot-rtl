# Mita.GithubCopilotRtl

GitHub Copilot RTL Support for Visual Studio

English / فارسی

## Overview

This Visual Studio extension adds simple Right-to-Left (RTL) support toggles for GitHub Copilot UI elements inside Visual Studio. It provides menu commands and keyboard shortcuts to enable and disable RTL layout mode.

Features
- Tools > Enable GitHub Copilot RTL Support
- Tools > Disable GitHub Copilot RTL Support
- Shortcut: Ctrl + RShift to enable RTL
- Shortcut: Ctrl + LShift to disable RTL

## Quick start
1. Requirements: Visual Studio, .NET Framework 4.7.2, VSIX packaging tools.
2. Open solution `Mita.GithubCopilotRtl.slnx` in Visual Studio.
3. Build the VSIX project and install the generated `.vsix` file.
4. Restart Visual Studio, then use Tools menu or shortcuts.

## Known issues
- Prompt Box does not flow right to left. When `FlowDirection=RightToLeft` is set on the input control, it causes the text to be mirrored. (This feature is currently disabled)

## Contributing
Please read CONTRIBUTING.md. Open issues and pull requests are welcome.

---

<div dir="rtl">

## نمای کلی (فارسی)

این افزونه برای Visual Studio امکانات پایه‌ای جهت فعال/غیرفعال‌سازی حالت راست‌به‌چپ (RTL) برای عناصر UI مرتبط با GitHub Copilot فراهم می‌کند. دستورات منو و کلیدهای میانبر برای فعال و غیرفعال کردن حالت RTL اضافه شده‌اند.

ویژگی‌ها
- منو Tools > Enable GitHub Copilot RTL Support
- منو Tools > Disable GitHub Copilot RTL Support
- شورتکات: Ctrl + RShift برای فعال‌سازی RTL
- شورتکات: Ctrl + LShift برای غیرفعال‌سازی RTL

شروع سریع
1. نیازمندی‌ها: Visual Studio، .NET Framework 4.7.2، ابزارهای VSIX.
2. فایل راه‌حل `Mita.GithubCopilotRtl.slnx` را در Visual Studio باز کنید.
3. پروژه VSIX را بیلد و فایل `.vsix` تولیدشده را نصب کنید.
4. Visual Studio را ری‌استارت کنید و از منوی Tools یا شورتکات‌ها استفاده کنید.

مشکلات شناخته‌شده
- Prompt Box راست به چپ نمی شود. هنگامی که `FlowDirection=RightToLeft` روی کنترل ورودی تنظیم شده باشد باعث آینه شدن متن شود. (این بخش فعلا غیر فعال شده است)
مشارکت
لطفاً CONTRIBUTING.md را بخوانید. باز شدن issue و pull request خوش‌آمدید.

</div>
