# ParkourGame Constitution

## Core Principles

### Unity/C# Best Practices

1. 腳本類別命名使用 PascalCase，變數與方法使用 camelCase。
2. 每個 MonoBehaviour 僅負責單一職責。
3. Update() 內不得有重度運算，需用協程或事件處理。
4. 欄位必須標明存取修飾詞，不可預設 public。
5. 事件註冊與解除必須成對，避免記憶體洩漏。
6. 禁用魔法數字，常數必須 const 或 readonly。
7. Prefab 與資源引用必須 Inspector 注入，不可硬編碼。
8. 所有例外必須捕捉並記錄，避免遊戲崩潰。
9. 關鍵邏輯需有單元測試（非 MonoBehaviour 可用標準 C# 測試框架）。
10. 每個腳本檔案只能有一個 public class，且檔名需與類別名一致。
11. 禁止在協程中無限 yield return null，需設計合理終止條件。
12. UI 物件更新必須集中於單一控制器，禁止分散於多個腳本。
13. 物件池必須用於重複生成/銷毀的遊戲物件，避免 GC 壓力。
14. 事件/委派必須避免循環引用，需明確解除訂閱。
15. 所有 public API 必須有 XML 註解，描述用途、參數與回傳值。
16. 禁止直接操作 transform hierarchy，需用 API（如 SetParent）並檢查 null。
17. 禁止在 Awake/Start 內進行資源載入，需用 async/await 或預載。
18. 禁止在 FixedUpdate 內進行非物理運算。
19. 禁止在 Editor 腳本內存取遊戲邏輯。
20. 禁止在 ScriptableObject 內存取場景物件。

### 社群主流慣例

1. 重大架構調整需先徵詢意見。
2. 組件化設計：遊戲邏輯應拆分為可重用組件，避免巨型類別。
3. 事件驅動：遊戲狀態變化、UI 更新、物件互動應以事件或委派實現，避免直接耦合。
4. ScriptableObject 配置：遊戲參數、設定、資料表應以 ScriptableObject 管理，避免硬編碼。
5. Commit message 必須採用 Conventional Commits 格式（如 feat:、fix:、docs:、refactor: 等），並簡明描述變更內容。
6. 任何違反上述慣例的程式碼，需於 Pull Request 中說明理由並取得團隊共識。

## 開發流程與約束

1. 新功能需提出設計規格，經審查通過後實作。
2. 重大變更需同步更新文件與測試。
3. 違反原則者需於 PR 說明理由並取得共識。

## Governance

- 本憲章優先於其他開發慣例，修訂需團隊共識並記錄於版本控管。
- Pull Request 或審查需檢查是否符合本憲章。
- 憲章修訂時須同步檢查並更新相關模板與開發文件。

**Version**: 1.0.0 | **Ratified**: 2026-01-09 | **Last Amended**: 2026-01-09
