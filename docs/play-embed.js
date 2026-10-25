(() => {
  'use strict';
  const container=document.getElementById('embedded-player'),cover=document.getElementById('play-cover'),load=document.getElementById('load-game'),expand=document.getElementById('expand-game'),status=document.getElementById('play-status');
  const local=['127.0.0.1','localhost'].includes(location.hostname);
  const origin=local?location.origin:'https://masked-liars-dice-play.ngakeo1990hl.chatgpt.site';
  let frame;
  load.addEventListener('click',()=>{
    if(frame)return;
    load.disabled=true;load.textContent='���ڴ�������';status.textContent='�״���Ҫ������Ϸ��Դ�����Ժ�';
    frame=document.createElement('iframe');frame.title='Masked Dice �� ���°���������';frame.allow='fullscreen; autoplay';frame.allowFullscreen=true;
    frame.src=origin+(local?'/game/play.html':'/play.html')+'?embed=1&release=2026.10.10-r3-web';
    frame.addEventListener('load',()=>{cover.hidden=true;expand.disabled=false;container.scrollIntoView({block:'start'});frame.contentWindow?.postMessage({type:'MASKED_DICE_FOCUS'},origin);});
    container.appendChild(frame);
  });
  expand.addEventListener('click',async()=>{if(frame){try{if(document.fullscreenElement)await document.exitFullscreen();else await container.requestFullscreen();frame.contentWindow?.postMessage({type:'MASKED_DICE_FOCUS'},origin);}catch{frame.focus();}}});
  window.addEventListener('message',event=>{if(event.origin===origin && event.source===frame?.contentWindow && event.data?.type==='MASKED_DICE_READY'){cover.hidden=true;expand.disabled=false;}});
  document.addEventListener('fullscreenchange',()=>{expand.textContent=document.fullscreenElement?'�˳�ȫ�� �L':'ȫ������ �J';});
})();
