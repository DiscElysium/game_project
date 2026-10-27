(() => {
  'use strict';
  const container=document.getElementById('embedded-player'),cover=document.getElementById('play-cover'),load=document.getElementById('load-game'),expand=document.getElementById('expand-game'),status=document.getElementById('play-status'),caption=document.getElementById('player-caption');
  const configured=document.querySelector('meta[name="game-origin"]')?.content;
  const gameURL=new URL('game/play.html',configured||document.baseURI),origin=gameURL.origin;
  for(const link of document.querySelectorAll('[data-game-link]'))link.href=gameURL.href;
  const t=key=>window.SiteI18n.t(key);
  let frame,phase='idle';
  function render(){
    load.textContent=t(phase==='loading'?'player.loading':phase==='error'?'player.retry':'player.play');
    status.textContent=t(phase==='loading'?'player.firstLoad':phase==='error'?'player.error':'player.hint');
    caption.textContent=t(phase==='ready'?'player.ready':'player.caption');
    expand.textContent=t(document.fullscreenElement?'player.exitFullscreen':'player.fullscreen');
    if(frame)frame.title=t('player.frame');
  }
  function focusGame(){frame?.contentWindow?.postMessage({type:'MASKED_DICE_FOCUS'},origin);}
  function openGame(){
    if(frame){focusGame();return;}
    phase='loading';load.disabled=true;render();
    frame=document.createElement('iframe');frame.title=t('player.frame');frame.allow='fullscreen; autoplay';frame.allowFullscreen=true;
    frame.src=gameURL.href+'?embed=1&release=2026.10.25-r3-web';
    frame.addEventListener('load',()=>{cover.hidden=true;expand.hidden=false;expand.disabled=false;focusGame();});
    frame.addEventListener('error',()=>{frame.remove();frame=null;phase='error';load.disabled=false;cover.hidden=false;expand.hidden=true;render();});
    container.appendChild(frame);
  }
  load.addEventListener('click',openGame);
  for(const link of document.querySelectorAll('[data-start-game]'))link.addEventListener('click',event=>{
    event.preventDefault();openGame();
    const url=new URL(location.href);url.hash='play';history.replaceState(null,'',url);
    container.scrollIntoView({block:'start',behavior:matchMedia('(prefers-reduced-motion: reduce)').matches?'auto':'smooth'});
  });
  expand.addEventListener('click',async()=>{if(frame){try{if(document.fullscreenElement)await document.exitFullscreen();else await container.requestFullscreen();focusGame();}catch{frame.focus();}}});
  window.addEventListener('message',event=>{if(event.origin===origin&&event.source===frame?.contentWindow&&event.data?.type==='MASKED_DICE_READY'){phase='ready';cover.hidden=true;expand.hidden=false;expand.disabled=false;render();focusGame();}});
  document.addEventListener('fullscreenchange',render);
  document.addEventListener('site:languagechange',render);
  render();
})();
