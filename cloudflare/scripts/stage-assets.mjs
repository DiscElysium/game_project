import {mkdir,readFile,writeFile,copyFile,readdir,stat} from 'node:fs/promises';
import {resolve,join} from 'node:path';
import {createHash} from 'node:crypto';
const [sitePath,playerPath,buildPath]=process.argv.slice(2).map(p=>resolve(p));
if(!sitePath||!playerPath||!buildPath)throw Error('Pass introduction, player and Unity Build directories.');
const root=resolve(import.meta.dirname,'..'),pub=join(root,'public'),game=join(pub,'game');
await mkdir(join(pub,'assets'),{recursive:true});await mkdir(join(game,'Build'),{recursive:true});
for(const name of ['index.html','design.css','frames.css','frames.js','app.js','play-embed.css','play-embed.js','creator-note.css','bilingual.css','i18n.js']) {
  let content=await readFile(join(sitePath,name),'utf8');
  if(name==='index.html')content=content.replace(/<meta property="og:url"[^>]*>\s*/g,'').replace(/<link rel="canonical"[^>]*>\s*/g,'');
  if(name==='play-embed.js') {
    content=content.replace(/const local=.*?;\s*const origin=.*?;/s,'const origin=location.origin;');
    content=content.replace("origin+(local?'/game/play.html':'/play.html')","new URL('game/play.html',document.baseURI).href");
    content=content.replace('2026.10.10-r3-web','2026.10.25-r3-web');
  }
  await writeFile(join(pub,name),content);
}
for(const name of ['latest-round.png','latest-table.png','pixel.woff2','screen-shadow.svg','FONT-LICENSE.txt'])await copyFile(join(sitePath,'assets',name),join(pub,'assets',name));
for(const name of ['play.html','player-v3.js','player-v3.css','masks-r3.png','pixel.woff2','FONT-LICENSE.txt']) {
  let bytes=await readFile(join(playerPath,name));
  if(name==='player-v3.js') {
    let text=bytes.toString('utf8');
    text=text.replace(/const local=.*?;\s*instance.SendMessage\('BrowserGameBridge','ConfigureAI',.*?\);/s,"instance.SendMessage('BrowserGameBridge','ConfigureAI',location.origin);");
    text=text.replace(/const parents=new Set\(\[.*?\]\);/,"const parents=new Set([location.origin,'https://discelysium.github.io']);");
    bytes=Buffer.from(text);
  }
  await writeFile(join(game,name),bytes);
}
const manifest={version:'2026.10.25-r3-web'};
for(const name of await readdir(buildPath)) {
  const kind=name.endsWith('.loader.js')?'loader':name.includes('.framework.js')?'framework':name.includes('.data.')?'data':name.includes('.wasm.')?'wasm':null;
  if(!kind)continue;
  const bytes=await readFile(join(buildPath,name));if(bytes.length>25*1024*1024)throw Error('Cloudflare asset exceeds 25 MiB: '+name);
  const file=createHash('sha256').update(bytes).digest('hex').slice(0,12)+'-'+name;
  await writeFile(join(game,'Build',file),bytes);manifest[kind]='Build/'+file;
}
if(!['loader','framework','data','wasm'].every(k=>manifest[k]))throw Error('Incomplete Unity build.');
await writeFile(join(game,'release.json'),JSON.stringify(manifest,null,2)+'\n');
await writeFile(join(pub,'_headers'),`/game/Build/*\n  Cache-Control: public, max-age=31536000, immutable\n/game/release.json\n  Cache-Control: no-cache\n/*\n  X-Content-Type-Options: nosniff\n  Referrer-Policy: strict-origin-when-cross-origin\n`);
console.log(JSON.stringify({version:manifest.version,publicDirectory:pub,build:manifest}));
