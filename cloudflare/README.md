# Masked Dice — Cloudflare deployment

This is the standalone introduction, embedded Unity WebGL game and DeepSeek service. All frontend resources are local static assets. The game calls `/api/health` and `/api/chat` on its own origin. No OpenAI or ChatGPT hosting is required.

## Local preview

Install with `npm ci`. When cloning source from GitHub, restore release binaries with `node scripts/fetch-release.mjs`. Run `npm run check`, then `npm run dev`. The introduction is `/`, and the embedded game is `/game/play.html`.

For real AI preview, place `DEEPSEEK_API_KEY` in an ignored `.dev.vars` file, or use the workspace's encrypted-credential launcher. Never commit that key. A missing server key is reported by the health endpoint and game AI panel.

## Deployment

1. Authenticate this computer using `npx wrangler login`.
2. Upload the existing DeepSeek key as a server-side secret using `npx wrangler secret put DEEPSEEK_API_KEY`.
3. Run `npm run deploy`.
4. Use the returned Workers URL. Current production: https://masked-dice-tribunal.1598509191.workers.dev/#play

The Worker serves static assets directly, and only `/api/*` invokes AI code. Build files use content hashes and immutable caching. The largest Unity asset is under Cloudflare's 25 MiB per-file limit. AI requests use SSE and server-side credentials; the temporary per-instance rate and concurrency controls are not a durable spending quota.

`public/game/Build/` contains the locally generated game binaries and is excluded from ordinary source commits. Include these files in the deployment upload. Generate them from the latest Unity project before staging. `scripts/stage-assets.mjs` accepts three directories: introduction output, player source, and Unity Build output. The complete deployment folder is also packaged locally for manual upload.
