# Host（房主）流程

Create Lobby
↓
顯示 Lobby UI（Player List）
↓
等待玩家加入
↓
【Host 按 Start Game】
↓
Create Relay Allocation
↓
把 Relay Join Code 存進 Lobby Data
↓
StartHost（Relay Transport）

# Client（加入者）流程

Join Lobby
↓
顯示 Lobby UI（Player List）
↓
等待 Host 按 Start Game
↓
Lobby Data 更新（拿到 Relay Join Code）
↓
Join Relay
↓
StartClient
