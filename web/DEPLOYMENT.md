# ������ҳ�沿��

## �ѷ����汾

- ����ҳ��https://masked-liars-dice.ngakeo1990hl.chatgpt.site/
- ��Ϸ��https://masked-liars-dice-play.ngakeo1990hl.chatgpt.site/play.html
- �汾��2026.10.10-r3-web

�����������������ʡ�`docs/` �����蹹���Ľ���ҳ��������Ϊ GitHub Pages �� `/docs` ��Դ���ز����õ�ǰ�������ܷ��񡣵�ǰʵ�����߷����� Sites �йܣ�GitHub Pages ���ڲֿ������е������á�

## �ṹ

`public/play.html`��`player-v3.css`��`player-v3.js` ���� `release.json`���ٰ����ݹ�ϣ���� `public/Build/` ���ĸ� Unity �ļ���ԭ�������� 1280��720���� 16:9 ��ʾ��ʹ����������ţ�������������Ϸ���������������ϡ�

`worker/public-ai.js` ���� `/api/health` �� `/api/chat`��ͨ������� Secret ���� DeepSeek�����ת�� SSE���� `/api/analyze` ���� 410������ҳ�Ž�����������°湹����

`unity/Latest` �������������� Unity ���̵Ĵ��븲�ǲ㣬���Ƕ��� Unity ���̡��ȸ������� `gamejam1bitDeepSeek`������ģ�͡����ʡ������� .meta���ų����� LLMUnity ��ģ���ļ����ٸ���ͬ���ű�������`LatestWebGLBuild.Build` ���� `../webgl-output/latest`��`tools/stage-latest.py` ��������Ϊ���ݹ�ϣ�ļ��������� manifest��

�����ʲ����ڴ˹���Դ��ֿ��У����ɺ���� `public/Build/`����������ʹ�� Fusion Pixel ����ѭ `public/FONT-LICENSE.txt`�������ѷ�������� `/release.json` �˶԰汾���ļ�����

## ���������

���ڷ�������� Secret `DEEPSEEK_API_KEY`��ģ�ͱ��� `DEEPSEEK_MODEL=deepseek-flash`��������� Unity ������������Կ��F3 �ɵ���˼����ģ�͡������Ͷ�����ʾ�ʣ��������ʵ��ʽ�����������ʵ�ʾ��߼�������¼��

�ӿ�У�������С����ɫ����������Ϣ�ṹ����������Դ��ÿ��ʵ����ʱ����ÿ����ַÿ���� 8 ���������ܹ� 4 ���������������ǳ־õ��˺��������ޡ�

## ��֤

���� `node scripts/check-public-ai.mjs` �����������У�顢�������� SSE ת���������� `npm run build`������������ʵ���������������������������ƣ�͸�Ӽ�ȫ���������������־���ʾ `HTTP 200 / REAL_API`���ɿ�������ģ����������Ӧ�õľ��ߡ�

F2��������ƣ�F3��AI ʵ���ң�TAB���۲죯�������Ҽ�������������ˣ�R�����򱱷���T���԰ף�H����ʷ��SPACE���ع˷�Ӧ��
