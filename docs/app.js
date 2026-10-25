(() => {
  'use strict';
  const situations = [
    {quantity:5,face:6,expression:'「他垂下目光，额角的血管轻轻跳动。」',normal:3,special:3,explanation:'紧张未必意味着撒谎。全场有 3 颗普通 6 点，加上叫牌者的特殊 6 点计作 3 颗，共 6 颗，足以支撑叫牌。'},
    {quantity:6,face:4,expression:'「他靠回椅背，嘴角扬起，视线始终没有离开你。」',normal:3,special:0,explanation:'这一次，自信是在虚张声势。全场只有 3 颗普通 4 点，叫牌者的特殊骰子点数不匹配，不计入；有效数量不足 6 颗。'},
    {quantity:7,face:5,expression:'「他的手指敲了一下骰盅，随后停住，像是在等你开口。」',normal:4,special:3,explanation:'他在等你质疑。4 颗普通 5 点，加上叫牌者的特殊 5 点计作 3 颗，刚好达到 7 颗。看似冒险的叫牌，可能已经算准。'}
  ];
  let currentRound = 0;
  const $ = id => document.getElementById(id);
  function choose(action) {
    const s=situations[currentRound],total=s.normal+s.special,isTrue=total>=s.quantity;
    $('decision-buttons').hidden=true;
    $('reveal').hidden=false;
    $('reveal-title').textContent=action==='challenge'?(isTrue?'这次，你质疑错了。':'你揭穿了这次虚张声势。'):(isTrue?'这次，你的信任有依据。':'这次，他骗过了你。');
    $('normal-count').textContent=s.normal;
    $('special-count').textContent=s.special;
    $('total-count').textContent=total;
    $('reveal-copy').textContent=s.explanation;
    $('next-round').focus({preventScroll:true});
  }
  $('trust-button').addEventListener('click',()=>choose('trust'));
  $('challenge-button').addEventListener('click',()=>choose('challenge'));
  $('next-round').addEventListener('click',()=>{
    currentRound=(currentRound+1)%situations.length;
    const s=situations[currentRound];
    $('round-indicator').textContent=`0${currentRound+1} / 03`;
    $('bid-quantity').textContent=s.quantity;
    $('bid-die').className=`die face-${s.face}`;
    $('bid-die').setAttribute('aria-label',`${s.face} 点骰子`);
    $('bid-readable').textContent=`全场至少有 ${s.quantity} 个 ${s.face} 点`;
    $('demo-expression').textContent=s.expression;
    $('decision-buttons').hidden=false;
    $('reveal').hidden=true;
    $('trust-button').focus({preventScroll:true});
  });
})();

