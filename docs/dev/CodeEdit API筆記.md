# CodeEdit API 筆記

這份文件記錄 Godot `CodeEdit` 補全 API 的實測行為。這些內容是開發筆記，不放在主要補全概念文件中。

## 啟用補全

```text
ScriptEditor.CodeCompletionEnabled = true
```

常見事件：

```text
ScriptEditor.TextChanged
ScriptEditor.CodeCompletionRequested
ScriptEditor.CodeCompletionOptionConfirmed
```

## 建議流程

文字變更時，用 analyzer / provider 判斷是否有候選：

```text
TextChanged
  -> 取得目前行與 caret column
  -> CompletionAnalyzer.Analyze(line, caretColumn)
  -> CompletionProvider.Provide(result)
  -> options.Count > 0 時 RequestCodeCompletion(true)
```

收到補全請求時加入候選：

```text
CodeCompletionRequested
  -> AddCodeCompletionOption(...)
  -> UpdateCodeCompletionOptions(true)
```

選項確認後處理特殊 caret 行為：

```text
CodeCompletionOptionConfirmed
  -> 如果插入 「」 ，把 caret 移到引號中間
```

## 實測行為

- `CodeCompletionRequested` 觸發後，在 handler 內呼叫 `AddCodeCompletionOption()` 與 `UpdateCodeCompletionOptions(true)`。
- `UpdateCodeCompletionOptions()` 會依 `GetTextForCodeCompletion()` 的 query text 過濾候選。
- 如果 query text 與候選文字不匹配，候選可能會被過濾到 0，popup 不會顯示。
- 內建 completion 較適合 prefix-based completion，例如輸入 `def` 後補 `default/毛豆`。
- `CodeCompletionOptionConfirmed` 可用來處理選項套用後的 caret 位置調整。

## 對話引號

不要使用 `"` 作為劇本對話 quote。

`CodeEdit` 是程式碼編輯器，會把 `"` 視為程式語言的 string literal 邊界。實測在 `""` 後方的空 token 位置，即使 analyzer / provider 已經產生正確候選，CodeEdit popup 仍可能不顯示：

```text
default/毛豆 "" |
  analyzer/provider -> 表情, 動作
  CodeEdit popup    -> 不顯示
```

改用劇本語言自己的 quote pair 後，CodeEdit 不再把對話內容當成內建 string token，空 slot 補全可以正常顯示：

```text
default/毛豆 「」 |
  -> 表情, 動作
```

因此 dialogue quote 統一由 `LanguageSchema` 定義：

```text
LanguageSchema.DialogueQuoteStart = 「
LanguageSchema.DialogueQuoteEnd   = 」
LanguageSchema.EmptyDialogueText  = 「」
```

## 可能的限制

若未來要支援「空行或行首直接顯示全部候選」，可能需要自訂 popup/list UI，而不是完全依賴 `CodeEdit` 內建 completion。
