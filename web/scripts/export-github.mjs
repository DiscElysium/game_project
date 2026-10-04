import {readdir,readFile} from 'node:fs/promises';
import {resolve,relative} from 'node:path';
const root=process.cwd();const entries=[];
const folders=new Set(['app','build','lib','scripts','worker','unity','components','hooks','vendor']);
const rootFiles=new Set(['package.json','package-lock.json','vite.config.ts','tsconfig.json','eslint.config.mjs','postcss.config.mjs','next.config.ts','components.json','README.md','DEPLOYMENT.md','.gitignore']);
async function walk(dir){for(const entry of await readdir(dir,{withFileTypes:true})){
 const absolute=resolve(dir,entry.name);const name=relative(root,absolute).replaceAll('\\','/');
 if(entry.isDirectory()){if(dir===root&&!folders.has(entry.name)&&entry.name!=='public')continue;if(name==='public/Build')continue;await walk(absolute);}
 else if(entry.isFile()){
  if(dir===root&&!rootFiles.has(entry.name))continue;
  if(name.startsWith('public/')&&!/\.(html|css|js|svg|txt)$/.test(entry.name))continue;
  let content=await readFile(absolute,'utf8');
  if(name==='.gitignore')content+='\n/public/Build/\n/public/pixel.woff2\n';
  if(name==='README.md')content+='\n此公开仓库包含移植和部署代码。完整游戏构建和素材留在私有托管及本地工程；运行前请按部署说明生成 `public/Build/`，并从 Fusion Pixel 项目获取符合许可的字体。\n';
  entries.push({path:'web/'+name,mode:'100644',type:'blob',content});
 }
}}
await walk(root);
entries.push({path:'web/.openai/hosting.json',mode:'100644',type:'blob',content:'{\n  "d1": null,\n  "r2": null\n}\n'});
console.log(JSON.stringify(entries));
