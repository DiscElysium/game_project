# 假面骰子 · 最新公开网页版

[介绍与在线游玩](https://masked-liars-dice.ngakeo1990hl.chatgpt.site/#play) · [独立全屏入口](https://masked-liars-dice-play.ngakeo1990hl.chatgpt.site/play.html)

当前版本 **2026.10.10-r3-web**，来自最新的 `gamejam1bitDeepSeek`，替换旧 `gamejamFinal` 移植。访客可以直接在介绍网页中游玩，无需登录或填写 API Key。

- 原生深棕色／纸色 1bit 像素与 Bayer 抖动，1280×720 画布；网页不再处理游戏截图。
- 悬崖议庭、三张独立面具、桌面数量选择器与实体道具交互。
- 服务端 DeepSeek 流式推理、英文对白、微表情和叫牌判断；F3 查看实际推理、输出、请求和应用后的决策。
- F2 调整画面，TAB 观察／回到牌桌，右键或方向键环顾。游戏区支持全屏。

`docs/` 为介绍页静态源代码，游戏 iframe 指向公开游戏服务。`web/` 为游戏加载器、AI 服务及 Unity WebGL 移植代码。完整模型与构建文件保留在本地工程及已发布的游戏服务，密钥只在服务端 Secret 中。

实际验证：浏览器内的数量／点数选择、叫牌、透视与全屏正常；三名 AI 均产生真实 HTTP 200 回复，并将结构化决策应用到对局。
