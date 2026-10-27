(() => {
  'use strict';
  const situations=[{quantity:5,face:6,normal:3,special:3},{quantity:6,face:4,normal:3,special:0},{quantity:7,face:5,normal:4,special:3}];
  let currentRound=0,selectedAction=null;
  const $=id=>document.getElementById(id),t=(key,values)=>window.SiteI18n.t(key,values);
  function render(){
    const s=situations[currentRound],total=s.normal+s.special,isTrue=total>=s.quantity;
    $('round-indicator').textContent=`0${currentRound+1} / 03`;
    $('bid-quantity').textContent=s.quantity;
    $('bid-die').className=`die face-${s.face}`;
    $('bid-die').setAttribute('aria-label',t('demo.die',{face:s.face}));
    $('bid-readable').textContent=t('demo.readable',s);
    $('demo-expression').textContent=t('demo.expression.'+currentRound);
    $('decision-buttons').hidden=selectedAction!==null;
    $('reveal').hidden=selectedAction===null;
    if(selectedAction!==null){
      $('reveal-title').textContent=t('demo.'+selectedAction+(isTrue?'True':'False'));
      $('normal-count').textContent=s.normal;$('special-count').textContent=s.special;$('total-count').textContent=total;
      $('reveal-copy').textContent=t('demo.explanation.'+currentRound);
    }
  }
  function choose(action){selectedAction=action;render();$('next-round').focus({preventScroll:true});}
  $('trust-button').addEventListener('click',()=>choose('trust'));
  $('challenge-button').addEventListener('click',()=>choose('challenge'));
  $('next-round').addEventListener('click',()=>{currentRound=(currentRound+1)%situations.length;selectedAction=null;render();$('trust-button').focus({preventScroll:true});});
  document.addEventListener('site:languagechange',render);
  render();
})();
