# Fruit Language Switcher

A tool to mimic the famous you-know-who fruit's language switcher on Windows.
A GUI will finally be added but for now it only has a tray icon (no actual icon).

## What does it do

It separates the Windows keyboard layouts `win + space` into two
categories: keyboard languages and IME languages, and adds hotkeys
to switch within or between categories.

Currently, `capslock` is used to switch between the categories, so the original `capslock` function will be blocked.
To use the toggle the actual `capslock` state, use `alt + capslock` instead.
`lwin + space` will be used to switch to the next language in current category, while `lctrl + lwin + space` will switch to the previous one.

For example, as a user with `[en-US, zh-TW, fr-CA, ja-JP]` as the
available languages for the OS, if current language is `en-US`, hitting `lwin + space` will switch to
`fr-CA [en-US, fr-CA]`, while hitting `capslock` will swtich from `en-US ([en-US, fr-CA])` to `zh-TW ([zh-TW, ja-JP])`.

The advantage of using this tool instead of the stock `win + space` is the it **does not** need to wait for the GUI to respond before changing a language.
