# data-model.md

## 主要實體

### 1. Room（房間）

- 欄位：
  - id: string
  - players: 玩家清單（陣列）
  - status: RoomStatus (等待中/遊戲中/結束)
  - maxPlayers: int
- 驗證：players 數量 <= maxPlayers
- 狀態轉換：等待中 <-> 遊戲中 <-> 結束

### 2. Player（玩家）

- 欄位：
  - id: string
  - name: string
  - position: Vector3
  - velocity: Vector3
  - score: int
  - isAlive: bool
- 驗證：名稱不可為空，分數 >= 0
- 狀態轉換：死亡、復活、過關

### 3. Level（關卡）

- 欄位：
  - id: string
  - name: string
  - obstacles: 障礙物清單（陣列）
  - startPosition: Vector3
  - endPosition: Vector3
- 驗證：障礙物數量 >= 1

### 4. Obstacle（障礙物）

- 欄位：
  - id: string
  - type: ObstacleType
  - position: Vector3
  - isActive: bool
- 狀態轉換：啟用、停用

### 5. ScoreRecord（分數紀錄）

- 欄位：
  - playerId: string
  - levelId: string
  - score: int
  - timestamp: DateTime
- 驗證：分數 >= 0

## 關聯

- Room 與 Player 一對多
- Player 與 ScoreRecord 一對多
- Level 與 Obstacle 一對多

## 狀態轉換

- Player: isAlive <-> 死亡/復活
- Obstacle: isActive <-> 啟用/停用
