# research.md

## 決策與依據

### 1. 技術選型

- 決策：Unity 6000.3.2f1，C# 最新 LTS
- 依據：Unity 社群主流版本，支援最新功能與最佳效能，資源豐富。
- 替代方案：Godot、Unreal Engine，但 Unity 生態系與教學資源最完整。

### 2. 主要依賴

- 決策：Input System、TextMeshPro、DOTween、Cinemachine、Odin Inspector、Netcode for GameObjects、Unity Transport
- 依據：這些套件為 Unity 3D 遊戲開發社群標配，穩定且有長期維護，Netcode/Transport 為官方多人同步解決方案。
- 替代方案：Mirror、Photon，但官方 Netcode 整合度高。

### 3. 資料儲存

- 決策：ScriptableObject 管理靜態資料，PlayerPrefs 儲存玩家進度
- 依據：ScriptableObject 易於編輯與序列化，PlayerPrefs 適合小型資料。
- 替代方案：外部資料庫（如 SQLite），但對單機/小型多人遊戲過度設計。

### 4. 測試策略

- 決策：Unity Test Framework（PlayMode/Editor）、NUnit
- 依據：官方支援，易於整合 CI/CD，覆蓋關鍵邏輯。
- 替代方案：手動測試，無法保證品質。

### 5. 效能與資源管理

- 決策：Prefab/資源分層、物件池、GC Alloc 控制、Draw Call 優化
- 依據：社群經驗證明這些做法能有效提升效能與穩定性。
- 替代方案：無分層或無物件池，容易造成效能瓶頸。

### 6. 專案結構

- 決策：Assets/ 下分 Scripts、Prefabs、Materials、Scenes、Fonts、TextMesh Pro、Settings
- 依據：Unity 官方與社群推薦結構，利於維護與擴充。
- 替代方案：混合式結構，維護困難。

### 7. 多人同步

- 決策：Netcode for GameObjects + Unity Transport
- 依據：官方支援，易於與 Unity Editor、雲端服務整合，適合 2-4 人即時連線。
- 替代方案：Photon、Mirror，功能強但授權/整合複雜。
