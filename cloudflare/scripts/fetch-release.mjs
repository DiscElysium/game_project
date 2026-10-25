// Restore release binaries from the game's own Cloudflare deployment.
import {mkdir,writeFile,readFile} from 'node:fs/promises';
import {resolve,dirname} from 'node:path';
import {createHash} from 'node:crypto';
const origin=process.env.RELEASE_ORIGIN||'https://masked-dice-tribunal.1598509191.workers.dev';
const root=resolve(import.meta.dirname,'..','public');
const manifest=JSON.parse(await readFile(resolve(root,'game/release.json'),'utf8'));
const files=['assets/latest-round.png','assets/latest-table.png','assets/pixel.woff2','game/masks-r3.png','game/pixel.woff2',...['data','framework','loader','wasm'].map(k=>'game/'+manifest[k])];
for(const name of files){
 const response=await fetch(new URL('/'+name,origin));if(!response.ok)throw Error('Release asset unavailable: '+name);
 const bytes=Buffer.from(await response.arrayBuffer());
 const hash=name.match(/\/Build\/([a-f0-9]{12})-/)?.[1];
 if(hash&&createHash('sha256').update(bytes).digest('hex').slice(0,12)!==hash)throw Error('Release hash mismatch: '+name);
 const path=resolve(root,name);await mkdir(dirname(path),{recursive:true});await writeFile(path,bytes);
 console.log('Restored '+name+' ('+bytes.length+' bytes)');
}
