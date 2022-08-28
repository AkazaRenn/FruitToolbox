# Fruit Language Switcher

## What does it do

It separates the Windows keyboard layouts `win + space` into two
categories: keyboard languages and IME languages, and adds hotkeys
to switch within or between categories.

Currently, `alt + B` is used to switch between the categories, and
`alt + N` is used to swtich within a category.

For example, as a user with `[en-US, zh-TW, fr-CA, ja-JP]` as the
available languages for the OS, if current language is `en-US`, hitting `alt + N` will switch between
`[en-US, fr-CA]`, while hitting `alt + B` will swtich from `en-US ([en-US, fr-CA])` to `zh-TW ([zh-TW, ja-JP])`.
