const recent = new Map();
const json = (body, status = 200) => new Response(JSON.stringify(body), { status, headers: {"Content-Type":"application/json; charset=utf-8","Cache-Control":"private, no-store"} });
export async function analyze(request, env, fetchAI = fetch) {
  if (request.method !== "POST") return json({error:"method_not_allowed"},405);
  const origin = new URL(request.url).origin;
  if ((request.headers.get("Origin") && request.headers.get("Origin") !== origin) || request.headers.get("Sec-Fetch-Site") === "cross-site") return json({error:"forbidden"},403);
  if (!request.headers.get("Content-Type")?.startsWith("application/json")) return json({error:"expected_json"},415);
  if (Number(request.headers.get("Content-Length")) > 32000) return json({error:"too_large"},413);
  const reader=request.body?.getReader();
  if (!reader) return json({error:"invalid_request"},400);
  const chunks=[];let size=0;
  while(true){const {done,value}=await reader.read();if(done)break;size+=value.length;if(size>32000){await reader.cancel();return json({error:"too_large"},413);}chunks.push(value);}
  const bytes=new Uint8Array(size);let offset=0;for(const c of chunks){bytes.set(c,offset);offset+=c.length;}
  let input;
  try {input=JSON.parse(new TextDecoder().decode(bytes));} catch {return json({error:"invalid_json"},400);}
  if (typeof input.prompt!=="string" || input.prompt.length<10 || input.prompt.length>12000 || typeof input.persona!=="string" || input.persona.length>8000) return json({error:"invalid_request"},400);
  if (!env.DEEPSEEK_API_KEY) return json({error:"ai_not_configured"},503);
  // Sites enforces owner-only access before this Worker. This per-isolate burst
  // guard does not replace a durable account-wide spending quota.
  const user=request.headers.get("oai-authenticated-user-id") || "owner";
  const now=Date.now();const calls=(recent.get(user)||[]).filter(t=>now-t<60000);
  if(calls.length>=12)return json({error:"rate_limited"},429);
  calls.push(now);recent.set(user,calls);
  if(recent.size>1000)recent.clear();
  let phase="request";
  try {
    const upstream=await fetchAI("https://api.deepseek.com/chat/completions", {
      method:"POST",headers:{"Authorization":"Bearer "+env.DEEPSEEK_API_KEY,"Content-Type":"application/json"},signal:AbortSignal.timeout(38000),
      body:JSON.stringify({model:env.DEEPSEEK_MODEL||"deepseek-flash",stream:false,thinking:{type:"disabled"},temperature:0.7,max_tokens:1200,response_format:{type:"json_object"},messages:[
        {role:"system",content:input.persona+"\n你在虚构的骰子游戏中扮演 NPC。用简短中文输出 JSON，不要 Markdown。保留角色性格及局势判断。必须包含 risk_factor（0到1的数字）、micro_expression（可见的细微动作，最多80字）、analysis_summary（最多200字）。若局势要求决策，保留 action（BID 或 CHALLENGE）、bid_quantity、bid_point。"},
        {role:"user",content:input.prompt}
      ]})
    });
    if(!upstream.ok){console.warn("AI upstream failed",upstream.status);return json({error:"ai_service_unavailable"},502);}
    phase="response";
    const data=await upstream.json();const content=data.choices?.[0]?.message?.content;
    if(typeof content!=="string")throw new Error("missing_content");
    phase="json";
    const result=JSON.parse(content.replace(/^\s*```(?:json)?\s*|\s*```\s*$/g,""));
    phase="schema";
    if(typeof result.risk_factor==="string" && result.risk_factor.trim())result.risk_factor=Number(result.risk_factor);
    if(typeof result.risk_factor!=="number" || !Number.isFinite(result.risk_factor) || typeof result.micro_expression!=="string" || !result.micro_expression.trim())throw new Error("invalid_analysis");
    return json({risk_factor:Math.max(0,Math.min(1,result.risk_factor)),micro_expression:result.micro_expression.slice(0,160),analysis_summary:String(result.analysis_summary||"").slice(0,400),action:["BID","CHALLENGE"].includes(result.action)?result.action:"",bid_quantity:Number.isInteger(result.bid_quantity)?result.bid_quantity:0,bid_point:Number.isInteger(result.bid_point)?result.bid_point:0});
  }catch{console.warn("AI response failed",phase);return json({error:"ai_service_unavailable"},502);}
}
