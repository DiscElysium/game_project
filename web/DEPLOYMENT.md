# 假面骰子网页版

原版 `gamejamFinal` 场景的 Unity 2022.3 WebGL 构建。游戏继续使用原始 1-bit Shader Graph 明暗算法、深棕色/白色材质和阈值纹理。网页副本将 Shader Graph 计算精度设为 Single；Bayer 阈值纹理采用不压缩、无 Mipmap、Point 采样，以避免浏览器端阈值失真。页面不对画面进行滤镜或重新抖动。

## 运行结构

- `public/Build/` 是真实 WebGL 构建，约 16 MB。采用 Gzip 与 Unity 解压回退，不需要额外的 Content-Encoding 配置。
- `public/play.html`、`play.css`、`play.js` 是全屏游戏入口。内部渲染分辨率 1600×900，页面按 16:9 缩放。
- `worker/analyze.js` 只在服务端调用 DeepSeek。它保留场景里三位 NPC 的原始性格提示，以及游戏传来的手牌和局势。
- `build/sites-worker.ts` 将 `/api/analyze` 交给服务端，其余请求由 Sites/Vinext 处理。
- `unity/BrowserAI` 是移植用的桥接脚本，用来替换本地 llama.cpp 插件。
- 网页操作栏调用原版叫牌、质疑、透视和猜测特殊骰子的接口，并镜像游戏的状态及微表情文字，解决 WebGL 中文字体显示问题。

Sites 部署保持仅所有者可访问。必须登录创建这个 Site 时使用的 ChatGPT 账号。GitHub 存放部署源代码，不承担私有游戏托管。

## AI 配置

在 Sites 的服务器环境变量中设置 `DEEPSEEK_API_KEY`（Secret）和 `DEEPSEEK_MODEL=deepseek-flash`。不要把密钥放入 Unity、浏览器代码、构建目录或 GitHub。接口限制输入大小、拒绝跨站请求，并设置每个服务器实例每分钟 12 次的临时频率限制。它不是账号总消费额度限制。

AI 请求失败时会明确显示连接不可用，对局继续使用原游戏的决策算法；不会用预制文本冒充 AI 微表情。

## 更新构建

在原工程的独立副本中替换 `LLMUnity` 本地推理组件。保留原始 Shader Graph 和材质 GUID，以及 `LLMUnity/texture` 里的 Bayer/蓝噪声纹理。这些纹理属于游戏美术，移除推理插件时仍须保留。

将场景里 `LLMCharacter`/`LLM` 的脚本引用更新到桥接脚本的实际 GUID，保留 `prompt` 和 `npcCharacters` 引用。给 `LiarGameManager` 加入下面三个只读访问方法，网页通过原游戏的逻辑手牌同步特殊骰子转移：

```csharp
public bool WebCanAct => isPlayerTurnActive && !hasPlayerActed;
public string GetWebHand(int index) => allHands.TryGetValue(index, out var hand) ? string.Join("  ", hand) : "";
public int GetWebSpecialCount(int index) => playerSpecialDiceCounts.TryGetValue(index, out var count) ? count : 0;
```

将网页副本的两个 Shader Graph 的 `m_GraphPrecision` 设为 `0`（Single）。通过 `WebGLBuild.Build` 构建，把输出 `Build/` 四个文件复制到 `public/Build/`。大模型和桌面原生库不参与浏览器构建。

安装：`npm run install:ci`。验证：`node scripts/check-ai.mjs`。构建：`npm run build`。发布使用 Sites 的源代码推送与私有发布流程，构建输出是 `dist/`。

本次验证包含真实浏览器启动和对局，以及真实 DeepSeek 结构化响应。原版游戏的一些背景/UI 素材使用灰色，这些素材按实机保留。
