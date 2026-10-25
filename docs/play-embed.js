(() => {
  'use strict';
  const container=document.getElementById('embedded-player'),cover=document.getElementById('play-cover'),load=document.getElementById('load-game'),expand=document.getElementById('expand-game'),status=document.getElementById('play-status');
  const origin='https://masked-dice-tribunal.1598509191.workers.dev';
  let frame;
  load.addEventListener('click',()=>{
    if(frame)return;
    load.disabled=true;load.textContent='正在打开牌桌…';status.textContent='首次需要下载游戏资源，请稍候。';
    frame=document.createElement('iframe');frame.title='Masked Dice — 最新版在线游玩';frame.allow='fullscreen; autoplay';frame.allowFullscreen=true;
    frame.src=origin+'/game/play.html'+'?embed=1&release=2026.10.25-r3-web';
    frame.addEventListener('load',()=>{cover.hidden=true;expand.disabled=false;container.scrollIntoView({block:'start'});frame.contentWindow?.postMessage({type:'MASKED_DICE_FOCUS'},origin);});
    container.appendChild(frame);
  });
  expand.addEventListener('click',async()=>{if(frame){try{if(document.fullscreenElement)await document.exitFullscreen();else await container.requestFullscreen();frame.contentWindow?.postMessage({type:'MASKED_DICE_FOCUS'},origin);}catch{frame.focus();}}});
  window.addEventListener('message',event=>{if(event.origin===origin && event.source===frame?.contentWindow && event.data?.type==='MASKED_DICE_READY'){cover.hidden=true;expand.disabled=false;}});
  document.addEventListener('fullscreenchange',()=>{expand.textContent=document.fullscreenElement?'退出全屏 ↙':'全屏游玩 ↗';});
})();
