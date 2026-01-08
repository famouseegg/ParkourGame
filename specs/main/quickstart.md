# quickstart.md

## Unity 3D 跑酷遊戲專案快速啟動指南

### 1. 環境需求

- Unity 6000.3.2f1
- .NET 最新 LTS
- 建議安裝 VSCode 或 Rider

### 2. 專案結構

- Assets/Scripts：遊戲邏輯與組件
- Assets/Prefabs：預製物件
- Assets/Materials：材質
- Assets/Scenes：場景
- Assets/Fonts、TextMesh Pro、Settings：資源與設定
- specs/：設計文件

### 3. 套件安裝

- 於 Unity Package Manager 安裝：
  - Input System
  - TextMeshPro
  - DOTween
  - Cinemachine
  - Odin Inspector
  - Netcode for GameObjects（多人同步）
  - Unity Transport

### 4. 執行步驟

1. 於 Unity Hub 開啟專案
2. 確認上述套件已安裝
3. 開啟 Assets/Scenes/Main.unity
4. 按下 Play 測試
5. 多人測試：可於 Editor 啟動多執行緒或建構多個 client 連線

### 5. 測試

- 於 Unity Test Runner 執行 PlayMode/Editor 測試
- 測試腳本於 Assets/Tests/

### 6. 進階

- 修改 ScriptableObject 以調整遊戲參數
- 擴充 Prefab 與關卡
