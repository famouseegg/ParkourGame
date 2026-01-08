# Implementation Plan: ParkourGame 多人 3D 跑酷遊戲

**Branch**: main | **Date**: 2026-01-08 | **Spec**: [`specs/main/plan.md`](specs/main/plan.md)
**Input**: 本文件與 research.md、data-model.md、quickstart.md、contracts/

**Note**: 本計畫依 Unity 6000.3.2f1 / C# 最新 LTS 與社群主流多人遊戲實踐產出，所有設計細節詳見下方。

## Summary

主要需求：
主要為 3D 跑酷遊戲，玩家需操作角色穿越障礙、跳躍平台，完成關卡。需支援鍵盤與手把操作，具備 UI 介面、音效、分數與進度儲存。
技術路線：
採用 Unity 6000.3.2f1，C# 撰寫，核心邏輯模組化，資源分層管理，動畫與物理互動結合，UI 採用 TextMeshPro，流程控制與資料儲存皆以 ScriptableObject 為主，測試自動化。

## Technical Context

**Language/Version**: Unity 6000.3.2f1 / C# 最新 LTS
**Primary Dependencies**: Unity Input System、TextMeshPro、DOTween、Cinemachine、Odin Inspector
**Storage**: ScriptableObject 管理靜態資料，PlayerPrefs 儲存玩家進度，資源分層於 Assets/ 目錄（如 Prefabs、Materials、Scripts、Scenes）
**Testing**: Unity Test Framework（PlayMode/Editor）、NUnit，單元測試覆蓋關鍵邏輯，CI 自動化測試
**Target Platform**: Windows 10+、macOS、WebGL
**Project Type**: 單一 Unity 3D 遊戲專案，採用標準 Assets 結構
**Performance Goals**: 桌機 60 FPS、WebGL 30 FPS，Draw Call < 100，GC Alloc 單幀 < 1KB
**Constraints**: Prefab 與資源分層（依功能/類型），ScriptableObject 管理設定，記憶體峰值 < 1GB，啟動時間 < 5 秒，Update 內避免重運算，事件/協程管理釋放
**Scale/Scope**: 支援 2-4 人即時連線闖關，10+ 關卡，具備單人與多人模式

## Constitution Check

_GATE: Must pass before Phase 0 research. Re-check after Phase 1 design._

1. 命名規範：腳本類別 PascalCase，變數/方法 camelCase。
2. 每個 MonoBehaviour 僅負責單一職責。
3. Update 內避免重運算，需用協程或事件處理。
4. 欄位標明存取修飾詞，避免預設 public。
5. 事件註冊/解除需成對，避免記憶體洩漏。
6. 禁用魔法數字，常數需 const/readonly。
7. Prefab 與資源引用皆 Inspector 注入，不可硬編碼。
8. 例外需捕捉並記錄，避免遊戲崩潰。
9. 關鍵邏輯需有單元測試。
10. 每檔僅一 public class，檔名需與類別名一致。
11. 禁止協程無限 yield return null，需設計終止條件。
12. UI 更新集中於單一控制器，禁止分散多腳本。
13. 物件池用於重複生成/銷毀物件，減少 GC 壓力。
14. 事件/委派避免循環引用，需明確解除訂閱。
15. public API 需有 XML 註解。
16. 禁止直接操作 transform hierarchy，需用 API 並檢查 null。
17. 禁止在 Awake/Start 載入資源，需 async/await 或預載。
18. 禁止在 FixedUpdate 進行非物理運算。
19. 禁止 Editor 腳本存取遊戲邏輯。
20. 禁止 ScriptableObject 存取場景物件。
21. 重大架構調整需先徵詢意見。
22. 組件化設計，遊戲邏輯拆分為可重用組件。
23. 事件驅動，狀態變化、UI 更新、物件互動皆用事件或委派。
24. ScriptableObject 管理參數、設定、資料表。
25. Commit message 採 Conventional Commits 格式。
26. 違反上述慣例需於 PR 說明理由並取得共識。

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
# [REMOVE IF UNUSED] Option 1: Single project (DEFAULT)
src/
├── models/
├── services/
├── cli/
└── lib/

tests/
├── contract/
├── integration/
└── unit/

# [REMOVE IF UNUSED] Option 2: Web application (when "frontend" + "backend" detected)
backend/
├── src/
│   ├── models/
│   ├── services/
│   └── api/
└── tests/

frontend/
├── src/
│   ├── components/
│   ├── pages/
│   └── services/
└── tests/

# [REMOVE IF UNUSED] Option 3: Mobile + API (when "iOS/Android" detected)
api/
└── [same as backend above]

ios/ or android/
└── [platform-specific structure: feature modules, UI flows, platform tests]
```

**Structure Decision**:

- 專案結構：
  - Assets/
    - Scripts/（遊戲邏輯、組件、管理器）
    - Prefabs/（預製物件）
    - Materials/（材質）
    - Scenes/（場景）
    - Fonts/、TextMesh Pro/、Settings/（資源與設定）
  - Packages/（外部套件）
  - ProjectSettings/（專案設定）
  - specs/（設計文件）
- 測試腳本置於 Assets/Tests/，單元測試與整合測試分開
- 資料與設定以 ScriptableObject 管理，Prefab 與資源分層明確

## Complexity Tracking

無違反憲章規則，所有設計皆依社群主流最佳實踐。
