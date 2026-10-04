import http from 'node:http';
import {readFile} from 'node:fs/promises';
import {resolve,extname,sep} from 'node:path';
import {analyze} from '../worker/analyze.js';
const root=resolve('public');
console.log('Awaiting private AI configuration on stdin.');
const config=await new Promise(resolveInput=>{
 let input='';if(process.stdin.isTTY)process.stdin.setRawMode(true);
 process.stdin.setEncoding('utf8');
 const receive=chunk=>{input+=chunk;if(/[\r\n]/.test(input)){process.stdin.off('data',receive);process.stdin.pause();resolveInput(input.trim());}};
 process.stdin.on('data',receive);process.stdin.resume();
});
const env=JSON.parse(config);
try{
 const modelReply=await fetch('https://api.deepseek.com/models',{headers:{Authorization:'Bearer '+env.DEEPSEEK_API_KEY},signal:AbortSignal.timeout(15000)});
 console.log(JSON.stringify({modelServiceStatus:modelReply.status,models:modelReply.ok?(await modelReply.json()).data.map(m=>m.id):[]}));
}catch{console.log('Model discovery unavailable; continuing preview.');}
const types={'.html':'text/html; charset=utf-8','.css':'text/css','.js':'application/javascript','.woff2':'font/woff2','.svg':'image/svg+xml','.unityweb':'application/octet-stream'};
http.createServer(async(req,res)=>{
 try{
  const url=new URL(req.url,'http://127.0.0.1:4177');
  if(url.pathname==='/api/analyze'){
   const chunks=[];let length=0;for await(const c of req){length+=c.length;if(length>32000){res.writeHead(413).end();return;}chunks.push(c);}
   const request=new Request(url,{method:req.method,headers:req.headers,body:Buffer.concat(chunks)});
   const start=Date.now();const response=await analyze(request,env);
   console.log(JSON.stringify({aiStatus:response.status,durationMs:Date.now()-start}));
   res.writeHead(response.status,Object.fromEntries(response.headers));res.end(Buffer.from(await response.arrayBuffer()));return;
  }
  const filename=resolve(root,'.'+(url.pathname==='/'?'/play.html':decodeURIComponent(url.pathname)));
  if(!filename.startsWith(root+sep)){res.writeHead(403).end();return;}
  const bytes=await readFile(filename);res.writeHead(200,{'Content-Type':types[extname(filename)]||'application/octet-stream','Content-Length':bytes.length});res.end(bytes);
 }catch{res.writeHead(404).end('Not found');}
}).listen(4177,'127.0.0.1',()=>console.log('Game preview ready: http://127.0.0.1:4177'));
