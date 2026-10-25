// The public player sends game state, never an API credential.
const buckets = new Map();
let active = 0;
const version = '2026.10.10-r3-web';
function headers(request) {
  const own = new URL(request.url).origin;
  const origin = request.headers.get('Origin');
  const allowed = !origin || origin === own || origin === 'https://discelysium.github.io' || origin === 'https://masked-liars-dice.ngakeo1990hl.chatgpt.site';
  const h = {'Cache-Control':'no-store, no-transform','X-MaskedDice-Version':version,'Vary':'Origin'};
  if (allowed && origin) h['Access-Control-Allow-Origin'] = origin;
  h['Access-Control-Allow-Methods'] = 'GET, POST, OPTIONS';
  h['Access-Control-Allow-Headers'] = 'Content-Type';
  return {allowed,h};
}
function json(body,status,h) { return new Response(JSON.stringify(body),{status,headers:{...h,'Content-Type':'application/json; charset=utf-8'}}); }
export async function publicAI(request,env,fetchAI=fetch) {
  const {allowed,h}=headers(request); const path=new URL(request.url).pathname;
  if(!allowed) return json({error:'origin_not_allowed'},403,h);
  if(request.method==='OPTIONS') return new Response(null,{status:204,headers:h});
  if(path==='/api/health') {
    if(request.method!=='GET') return json({error:'method_not_allowed'},405,h);
    return json({configured:!!env.DEEPSEEK_API_KEY,model:env.DEEPSEEK_MODEL||'deepseek-flash',version},200,h);
  }
  if(path!=='/api/chat') return json({error:'not_found'},404,h);
  if(request.method!=='POST') return json({error:'method_not_allowed'},405,h);
  if(!request.headers.get('Content-Type')?.startsWith('application/json')) return json({error:'expected_json'},415,h);
  if(!env.DEEPSEEK_API_KEY) return json({error:'server_ai_unavailable'},503,h);
  if(Number(request.headers.get('Content-Length'))>64000) return json({error:'request_too_large'},413,h);
  let body;
  try {
    const reader=request.body?.getReader(); if(!reader) return json({error:'empty_request'},400,h);
    const chunks=[]; let size=0;
    while(true) { const {done,value}=await reader.read(); if(done) break; size+=value.length; if(size>64000) { await reader.cancel(); return json({error:'request_too_large'},413,h); } chunks.push(value); }
    const bytes=new Uint8Array(size); let offset=0; for(const chunk of chunks) { bytes.set(chunk,offset); offset+=chunk.length; }
    body=JSON.parse(new TextDecoder().decode(bytes));
  } catch { return json({error:'invalid_json'},400,h); }
  if(!Array.isArray(body.messages) || body.messages.length!==2 || body.messages[0]?.role!=='system' || body.messages[1]?.role!=='user' || body.messages.some(m=>typeof m.content!=='string' || m.content.length<10 || m.content.length>24000)) return json({error:'invalid_game_request'},400,h);
  let situation;
  try { situation=JSON.parse(body.messages[1].content); } catch { return json({error:'invalid_situation'},400,h); }
  if(!['Watcher','Weaver','Risker'].includes(situation.role) || !Array.isArray(situation.ownNormalDice) || !Array.isArray(situation.ownSpecialDice) || ![...situation.ownNormalDice,...situation.ownSpecialDice].every(n=>Number.isInteger(n) && n>=1 && n<=6)) return json({error:'invalid_game_state'},400,h);
  const model=body.model || env.DEEPSEEK_MODEL || 'deepseek-flash';
  if(typeof model!=='string' || !/^deepseek-[a-z0-9-]{1,40}$/.test(model)) return json({error:'invalid_model'},400,h);
  const id=request.headers.get('CF-Connecting-IP') || request.headers.get('oai-authenticated-user-id') || 'anonymous';
  const now=Date.now(); const recent=(buckets.get(id)||[]).filter(t=>now-t<60000);
  if(recent.length>=8 || active>=4) return json({error:'ai_busy_try_later'},429,{'Retry-After':'15',...h});
  recent.push(now); buckets.set(id,recent); if(buckets.size>2000) for(const [key,times] of buckets) if(times.at(-1)<now-60000) buckets.delete(key);
  const payload={model,messages:body.messages.map(m=>({role:m.role,content:m.content})),stream:true,stream_options:{include_usage:true},response_format:{type:'json_object'},max_tokens:Math.max(1024,Math.min(8192,Number.isInteger(body.max_tokens)?body.max_tokens:8192)),thinking:{type:body.thinking?.type==='disabled'?'disabled':'enabled'},reasoning_effort:['low','high','max'].includes(body.reasoning_effort)?body.reasoning_effort:'low'};
  const abort=new AbortController(); const timeout=setTimeout(()=>abort.abort(),65000); active++;
  let released=false; const release=()=>{if(!released) {released=true;active--;clearTimeout(timeout);}};
  let upstream;
  try { upstream=await fetchAI('https://api.deepseek.com/chat/completions',{method:'POST',headers:{'Content-Type':'application/json','Authorization':'Bearer '+env.DEEPSEEK_API_KEY},body:JSON.stringify(payload),signal:abort.signal}); }
  catch { release(); return json({error:'ai_connection_failed'},502,h); }
  if(!upstream.ok || !upstream.body) { release(); return json({error:'ai_service_unavailable'},[401,402,404,429].includes(upstream.status)?upstream.status:502,h); }
  const reader=upstream.body.getReader();
  const stream=new ReadableStream({
    async pull(controller) { try { const {done,value}=await reader.read(); if(done) {release();controller.close();} else controller.enqueue(value); } catch { release();controller.error(new Error('AI stream interrupted')); } },
    cancel() { abort.abort(); release(); return reader.cancel(); }
  });
  return new Response(stream,{status:200,headers:{...h,'Content-Type':'text/event-stream; charset=utf-8'}});
}
