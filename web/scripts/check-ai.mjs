import assert from 'node:assert/strict';
import {analyze} from '../worker/analyze.js';
const request=(body,headers={})=>new Request('https://game.example/api/analyze',{method:'POST',headers:{'Content-Type':'application/json',...headers},body:JSON.stringify(body)});
const valid={persona:'一个沉着谨慎的赌徒。',prompt:'当前对方叫了两个六，你手中有1,2,3,6,6。输出风险判断和可观察的微表情。'};
let called=false;
const mock=async()=>{called=true;return Response.json({choices:[{message:{content:JSON.stringify({risk_factor:.4,micro_expression:'指尖在桌沿停顿了一瞬。',analysis_summary:'局势尚可。'})}}]});};
assert.equal((await analyze(request(valid,{Origin:'https://another.example'}),{DEEPSEEK_API_KEY:'test'},mock)).status,403);
assert.equal(called,false);
assert.equal((await analyze(request({...valid,prompt:'x'.repeat(13000)}),{DEEPSEEK_API_KEY:'test'},mock)).status,400);
assert.equal((await analyze(request(valid),{},mock)).status,503);
assert.equal((await analyze(request(valid),{DEEPSEEK_API_KEY:'test'},async()=>Response.json({choices:[]}))).status,502);
const good=await analyze(request(valid),{DEEPSEEK_API_KEY:'test'},mock);assert.equal(good.status,200);assert.equal((await good.json()).risk_factor,.4);
console.log('AI origin, input, secret configuration, invalid upstream and valid response checks passed.');
if(process.argv.includes('--live')){
 const live=await fetch('http://127.0.0.1:4177/api/analyze',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify(valid)});
 console.log(JSON.stringify({liveStatus:live.status,result:await live.json()}));assert.equal(live.status,200);
}
