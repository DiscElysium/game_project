# 当前部署位置

请使用仓库中的 [Cloudflare 部署项目](../cloudflare/README.md)。旧部署流程已停用，不用于新版发布。

当前网址：https://masked-dice-tribunal.1598509191.workers.dev/#play

介绍页与游戏同源，游戏使用 `/api/health` 和 `/api/chat`。AI 密钥保存在 Cloudflare Secret。Unity 覆盖层改为在启动时读取当前网页域名；没有写死的旧托管网址。
