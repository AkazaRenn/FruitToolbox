# Fruit Toolbox

A tool to mimic the famous you-know-who fruit's language switcher on Windows.

[![Microsoft Store Link](https://camo.githubusercontent.com/34f495b817d32130aac0e9d20193b89a6f92b7ef119c6fbd536a876213dcbba8/68747470733a2f2f6765742e6d6963726f736f66742e636f6d2f696d616765732f656e2d55532532306c696768742e737667)](https://apps.microsoft.com/detail/fruit-language-switcher/9NJD8G4V1G1K)

## What it does

It separates the Windows keyboard layouts `win + space` into two
categories: keyboard languages and IME languages, and adds a hotkey (Caps Lock)
to switch within or between categories.

Currently, `capslock` is used to switch between the categories, so the original `capslock` function will be blocked.
To toggle the actual `capslock` state, hold `capslock` for more than 500ms.
`lwin + space` will be used to select any language and the app will update itself accordingly.

For example, as a user with `[en-US, zh-TW, fr-CA, ja-JP]` as the
available languages for the OS, if current language is `en-US`, hitting `capslock` will swtich from `en-US ([en-US, fr-CA])` to `zh-TW ([zh-TW, ja-JP])`.
