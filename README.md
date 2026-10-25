# 假面骰子 · Cloudflare 公开网页版

[介绍页内直接游玩](https://masked-dice-tribunal.1598509191.workers.dev/#play) · [独立游戏入口](https://masked-dice-tribunal.1598509191.workers.dev/game/play.html)

当前版本 **2026.10.25-r3-web**，基于最新版 `gamejam1bitDeepSeek`。介绍页、游戏资源、字体与 DeepSeek 流式接口都运行在用户自己的 Cloudflare 账号中，不依赖 GPT 托管。

- 深棕色／纸色 1bit 原生像素与 Bayer 抖动，1280×720 游戏画布。
- 新版悬崖议庭、三张独立面具、实体桌面数量／点数选择与透视道具。
- DeepSeek 实时对白、微表情与判断；F3 查看实际推理、输出、参数和应用后的决策。
- F2 调整画面，TAB 观察／牌桌，右键或方向键环顾，支持全屏。

**当前部署代码：[cloudflare](cloudflare/README.md)**。源码在 GitHub，公开游戏与 AI 服务在 Cloudflare。密钥只保存在 Cloudflare Secret，浏览器无需输入密钥。

`docs/` 保留可用于 GitHub Pages 的介绍页，它嵌入同一个 Cloudflare 游戏。旧 `web/` 是历史移植代码；当前发布请使用 `cloudflare/`。

从源码恢复发布文件：在 `cloudflare/` 内运行 `npm ci`、`node scripts/fetch-release.mjs`，再执行 `npm run check` 和 `npm run deploy`。二进制资源来自该游戏自己的 Cloudflare 地址，并核验构建哈希；本地也保留完整发布包与 Unity 工程。
