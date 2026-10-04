const canvas=document.querySelector('#unity-canvas');
const status=document.querySelector('#ai-status');
const loading=document.querySelector('#loading');
const progress=document.querySelector('#progress');
const full=document.querySelector('#fullscreen');
let game;
let view={};
const node=id=>document.getElementById(id);
const clean=text=>String(text||'').replace(/<[^>]*>/g,'');
window.maskedDiceViewState=json=>{
 view=JSON.parse(json);
 node('round-status').textContent=clean(view.status)||'请入座';
 node('bid-status').textContent=view.quantity?`当前叫牌：${view.quantity} 个 ${view.point} 点`:'尚未叫牌';
 node('hand').textContent=view.started&&view.hand?'你的手牌：'+view.hand+`（末 ${view.specialCount||1} 位为特殊骰）`:'';
 node('peek').textContent=view.peek?'透视：'+view.peek:'';
 node('micro').textContent=clean(view.micro);
 node('decision').textContent=clean(view.decision);
 node('begin').hidden=view.started&&!view.gameOver;
 node('begin').textContent=view.gameOver?'再来一局':'进入牌桌';
 node('bid-controls').hidden=!view.started||view.gameOver||view.afterBid;
 node('special-controls').hidden=!view.afterBid||!view.canAct;
 node('bid').disabled=!view.canAct||view.afterBid;
 node('challenge').disabled=!view.canAct||view.afterBid||!view.quantity;
};
node('begin').onclick=()=>game?.SendMessage('WebGameBridge','BeginGame');
node('bid').onclick=()=>{
 const quantity=Number(node('quantity').value),point=Number(node('point').value);
 if(!Number.isInteger(quantity)||quantity<1||quantity>60)return;
 if(quantity<view.quantity||(quantity===view.quantity&&point<=view.point)){node('decision').textContent='叫牌需要比上一家更大。';return;}
 game?.SendMessage('WebGameBridge','PlaceBid',JSON.stringify({quantity,point,chat:''}));
};
node('challenge').onclick=()=>game?.SendMessage('WebGameBridge','Challenge');
const special=(target,point)=>game?.SendMessage('WebGameBridge','SpecialAction',JSON.stringify({quantity:target,point,chat:''}));
node('skip').onclick=()=>special(0,0);
node('xray').onclick=()=>special(Number(node('target').value),0);
node('steal').onclick=()=>special(Number(node('target').value),Number(node('guess').value));
window.maskedDiceAIStatus=state=>{status.textContent=({ready:'AI 已连接',thinking:'AI 正在观察',unavailable:'AI 连接暂时不可用',unconfigured:'AI 尚未连接'})[state]||'准备中';};
function failed(message){document.querySelector('#loading-title').textContent='牌桌加载失败';document.querySelector('#loading-message').textContent=message;progress.hidden=true;document.querySelector('#retry').hidden=false;status.textContent='等待重试';}
document.querySelector('#retry').onclick=()=>location.reload();
const script=document.createElement('script');script.src='/Build/site.loader.js';script.onerror=()=>failed('游戏文件未能加载，请检查网络后重试。');
script.onload=()=>createUnityInstance(canvas,{
 dataUrl:'/Build/site.data.unityweb',frameworkUrl:'/Build/site.framework.js.unityweb',codeUrl:'/Build/site.wasm.unityweb',streamingAssetsUrl:'/StreamingAssets',companyName:'DiscElysium',productName:"Masked Liar's Dice",productVersion:'0.1',matchWebGLToCanvasSize:false,devicePixelRatio:1,
 showBanner:(message,type)=>{if(type==='error')failed('浏览器无法运行游戏，请使用支持 WebGL 2 的桌面浏览器。');else console.warn(message);}
},value=>{progress.value=value;}).then(instance=>{game=instance;loading.hidden=true;full.disabled=false;node('begin').disabled=false;full.onclick=()=>{if(document.fullscreenElement)document.exitFullscreen();else document.documentElement.requestFullscreen();};instance.SendMessage('WebGameBridge','Configure',new URL('/api/analyze',location.href).href);canvas.focus();}).catch(()=>failed('请使用支持 WebGL 2 的桌面浏览器，或重新加载。'));
document.body.appendChild(script);
