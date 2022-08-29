# Fruit Language Switcher

A tool to mimic the famous you-know-who fruit's language switcher on Windows.
A GUI will finally be added but for now it's only a console project.

## What does it do

It separates the Windows keyboard layouts `win + space` into two
categories: keyboard languages and IME languages, and adds hotkeys
to switch within or between categories.

Currently, `win + shift + ctrl + alt + A` is used to switch between the categories, and
`win + shift + ctrl + alt + B` and `win + shift + ctrl + alt + C` is used to swtich within a category.

For example, as a user with `[en-US, zh-TW, fr-CA, ja-JP]` as the
available languages for the OS, if current language is `en-US`, hitting `win + shift + ctrl + alt + B` will switch to
`fr-CA [en-US, fr-CA]`, while hitting `win + shift + ctrl + alt + A` will swtich from `en-US ([en-US, fr-CA])` to `zh-TW ([zh-TW, ja-JP])`.
