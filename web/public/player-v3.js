(() => {
  'use strict';
  const canvas=document.querySelector('#unity-canvas'),loading=document.querySelector('#loading'),message=document.querySelector('#loading-message'),progress=document.querySelector('#progress'),full=document.querySelector('#fullscreen');
  window.maskedDice={state:null,instance:null,errors:[]};
  function status(text){message.textContent=text;}
  function fail(text){loading.hidden=false;status(text);progress.hidden=true;document.querySelector('#retry').hidden=false;window.maskedDice.errors.push(text);}
  document.querySelector('#retry').onclick=()=>location.reload();
  full.onclick=()=>{canvas.focus();window.maskedDice.instance?.SetFullscreen(1);};
  async function start(){
    if(!document.createElement('canvas').getContext('webgl2')){fail('WebGL 2 is required. Please use a desktop browser with graphics acceleration.');return;}
    const manifest=await fetch('release.json',{cache:'no-store'}).then(r=>{if(!r.ok)throw Error('manifest');return r.json();});
    window.maskedDice.release=manifest;
    status('Loading the latest tribunal…');
    const script=document.createElement('script');script.src=manifest.loader;
    await new Promise((resolve,reject)=>{script.onload=resolve;script.onerror=reject;document.body.appendChild(script);});
    const instance=await createUnityInstance(canvas,{
      dataUrl:manifest.data,frameworkUrl:manifest.framework,codeUrl:manifest.wasm,streamingAssetsUrl:'StreamingAssets',
      companyName:'DiscElysium',productName:'Masked Dice — The Masked Tribunal',productVersion:manifest.version,
      matchWebGLToCanvasSize:false,devicePixelRatio:1,
      showBanner:(text,type)=>{if(type==='error')fail('The game could not start. Please reload or use a desktop browser.');else console.warn(text);}
    },value=>{progress.value=value;status('Loading the tribunal — '+Math.round(value*100)+'%');});
    window.maskedDice.instance=instance;
    const local=['127.0.0.1','localhost'].includes(location.hostname);
    instance.SendMessage('BrowserGameBridge','ConfigureAI',local?location.origin:'https://masked-liars-dice-play.ngakeo1990hl.chatgpt.site');
    loading.hidden=true;full.hidden=false;canvas.focus();
    const parents=new Set(['https://discelysium.github.io','https://masked-liars-dice.ngakeo1990hl.chatgpt.site',location.origin]);
    window.addEventListener('message',event=>{if(parents.has(event.origin)&&event.data?.type==='MASKED_DICE_FOCUS')canvas.focus();});
    if(window.parent!==window)window.parent.postMessage({type:'MASKED_DICE_READY',version:manifest.version},'*');
    window.maskedDice.command=command=>instance.SendMessage('BrowserGameBridge','Command',command);
    window.addEventListener('beforeunload',()=>instance.Quit());
  }
  start().catch(()=>fail('The game files could not load. Check your connection and try again.'));
})();
