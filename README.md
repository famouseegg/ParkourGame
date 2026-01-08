<!-- markdownlint-disable MD033 MD041 -->
<div align="center">

<img alt="LOGO" src="https://github.com/user-attachments/assets/864f70e5-4441-446f-a49f-aad4b8eb1d12" width="256" height="256">

# 🚀 ParkourGame

一款多人合作、充滿未知與挑戰的太空跑酷遊戲。玩家需在神秘空間中合作突破重重障礙，尋找逃脫的方法。

</div>

## 🌌 背景故事

你們是一支正在執行長程任務的太空人小隊。
任務原本只是一次例行的航道穿越與空間探測。

直到那一刻——
飛行船的感測器出現了不可能存在的數據。

在未被標記的星域邊緣，
一道不穩定的**蟲洞**突然展開，
船體還來不及進行迴避，就被強行捲入其中。

---

當系統重新啟動時，
你們已不在任何已知座標內。

這裡沒有星圖、沒有訊號回應，
空間結構扭曲，方向感失去意義。
引擎仍在運轉，卻無法離開這片區域。

彷彿整個宇宙，
只剩下你們與這片**神秘空間**。

---

有限的能源正在流失，
船體的完整性逐步下降。
唯一可以確認的是——
**這個空間並非靜止，它在觀察你們的行動。**

你們必須探索、前進，
找出空間的規則與出口。

因為停留，
只會讓你們永遠迷失在這裡。

## 🌟 遊戲特色

- 多人合作與競賽元素
- 多樣化機關與平台設計
- 物理互動與動態障礙
- 支援快速重生與關卡切換

## 🕹️ 操作方式

- **W / A / S / D**：控制角色移動
- **滑鼠移動**：轉動視角、調整視線方向
- **滑鼠右鍵**：飛撲
- **滑鼠左鍵**：攻擊
- **Shift**：衝刺
- **空白鍵**：跳躍

## 🗂️ 專案結構

- `Assets/Scenes/`：主要遊戲場景
- `Assets/Scripts/`：遊戲核心腳本（玩家、管理器、UI 等）
- `Assets/Animation/`：動畫資源
- `Assets/Settings/`：渲染與專案設定

## 🛠️ 安裝編譯指南

### 📥 原始碼取得

使用 Git 來複製專案：

```bash
git clone https://github.com/famouseegg/ParkourGame.git
```

### ⚙️ 建置與執行步驟

1. 安裝對應版本的 Unity Hub 與 Unity 編輯器（6000.3.2f1）。
2. 於 Unity Hub 中選擇「打開專案」，路徑指向本專案資料夾。
3. 開啟專案後，Unity 會自動還原相依套件。
4. 進入 `Assets/Scenes/`，選擇 `LobbyScene.unity` 或 `01-GameScene.unity` 開始遊戲。
5. 按下「播放」(Play) 按鈕即可進行測試。

### 📦 其他套件依賴

本專案使用以下套件：

- **Cinemachine**
- **Input System**
- **Netcode for GameObjects**
- **Unity Transport**

## 💾 下載遊戲

從 GitHub Release 下載最新版本：

1. 訪問 [Release 頁面](https://github.com/famouseegg/ParkourGame/releases)
2. 下載最新版本的壓縮檔案

### ▶️ 執行遊戲

1. 解壓縮下載的檔案
2. 點選 `ParkourGame.exe` 即可執行遊戲

## 🤝 貢獻指南

歡迎提交 Issue 或 Pull Request！

1. Fork 本專案
2. 建立分支 (`feature/your-feature`)
3. 提交 PR 並詳述修改內容

## 🏅 貢獻者

感謝所有為 ParkourGame 做出貢獻的開發者們！

<a href="https://github.com/famouseegg/ParkourGame/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=famouseegg/ParkourGame&max=1000" alt="Contributors"/>
</a>
