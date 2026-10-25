# 公开网页版部署

## 已发布版本

- 介绍页：https://masked-liars-dice.ngakeo1990hl.chatgpt.site/
- 游戏：https://masked-liars-dice-play.ngakeo1990hl.chatgpt.site/play.html
- 版本：2026.10.10-r3-web

两处均允许公开访问。`docs/` 是无需构建的介绍页，可以作为 GitHub Pages 的 `/docs` 来源；素材引用当前公开介绍服务。当前实际在线服务由 Sites 托管，GitHub Pages 需在仓库设置中单独启用。

## 结构

`public/play.html`、`player-v3.css`、`player-v3.js` 加载 `release.json`，再按内容哈希加载 `public/Build/` 中四个 Unity 文件。原生画布是 1280×720，按 16:9 显示，使用最近邻缩放，抖动保持在游戏自身的像素网格上。

`worker/public-ai.js` 处理 `/api/health` 和 `/api/chat`，通过服务端 Secret 调用 DeepSeek，逐块转发 SSE。旧 `/api/analyze` 返回 410，旧网页桥接组件不参与新版构建。

`unity/Latest` 是用于最新完整 Unity 工程的代码覆盖层，不是独立 Unity 工程。先复制完整 `gamejam1bitDeepSeek`，保留模型、材质、场景与 .meta，排除本地 LLMUnity 和模型文件，再覆盖同名脚本与插件。`LatestWebGLBuild.Build` 构建 `../webgl-output/latest`。`tools/stage-latest.py` 将构建改为内容哈希文件名并生成 manifest。

构建资产不在此公开源码仓库中；生成后放入 `public/Build/`。像素字体使用 Fusion Pixel 并遵循 `public/FONT-LICENSE.txt`。可用已发布服务的 `/release.json` 核对版本与文件名。

## 服务端配置

仅在服务端设置 Secret `DEEPSEEK_API_KEY`，模型变量 `DEEPSEEK_MODEL=deepseek-flash`。浏览器和 Unity 构建不接收密钥。F3 可调整思考、模型、参数和额外提示词，并检查真实流式推理、输出、实际决策及导出记录。

接口校验请求大小、角色、骰子与消息结构，并限制来源。每个实例临时限制每个地址每分钟 8 次请求与总共 4 个并发请求；它不是持久的账号消费上限。

## 验证

运行 `node scripts/check-public-ai.mjs` 检查服务端输入校验、错误处理和 SSE 转发，再运行 `npm run build`。本次已在真实浏览器完成桌面数量／点数／叫牌／透视及全屏操作，三名对手均显示 `HTTP 200 / REAL_API`，可看到完整模型推理和已应用的决策。

F2：画面控制；F3：AI 实验室；TAB：观察／牌桌；右键／方向键：环顾；R：面向北方；T：对白；H：历史；SPACE：回顾反应。
