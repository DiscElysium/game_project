(() => {
  'use strict';
  const situations = [
    {quantity:5,face:6,expression:'��������Ŀ�⣬��ǵ�Ѫ��������������',normal:3,special:3,explanation:'����δ����ζ�����ѡ�ȫ���� 3 ����ͨ 6 �㣬���Ͻ����ߵ����� 6 ����� 3 �ţ��� 6 �ţ�����֧�Ž��ơ�'},
    {quantity:6,face:4,expression:'���������α��������������ʼ��û���뿪�㡣��',normal:3,special:0,explanation:'��һ�Σ����������������ơ�ȫ��ֻ�� 3 ����ͨ 4 �㣬�����ߵ��������ӵ�����ƥ�䣬�����룻��Ч�������� 6 �š�'},
    {quantity:7,face:5,expression:'��������ָ����һ�����ѣ����ͣס�������ڵ��㿪�ڡ���',normal:4,special:3,explanation:'���ڵ������ɡ�4 ����ͨ 5 �㣬���Ͻ����ߵ����� 5 ����� 3 �ţ��պôﵽ 7 �š�����ð�յĽ��ƣ������Ѿ���׼��'}
  ];
  let currentRound = 0;
  const $ = id => document.getElementById(id);
  function choose(action) {
    const s=situations[currentRound],total=s.normal+s.special,isTrue=total>=s.quantity;
    $('decision-buttons').hidden=true;
    $('reveal').hidden=false;
    $('reveal-title').textContent=action==='challenge'?(isTrue?'��Σ������ɴ��ˡ�':'��Ҵ�������������ơ�'):(isTrue?'��Σ�������������ݡ�':'��Σ���ƭ�����㡣');
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
    $('bid-die').setAttribute('aria-label',`${s.face} ������`);
    $('bid-readable').textContent=`ȫ�������� ${s.quantity} �� ${s.face} ��`;
    $('demo-expression').textContent=s.expression;
    $('decision-buttons').hidden=false;
    $('reveal').hidden=true;
    $('trust-button').focus({preventScroll:true});
  });
})();

