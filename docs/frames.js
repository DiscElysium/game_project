(() => {
  'use strict';
  const bayer = [0,32,8,40,2,34,10,42,48,16,56,24,50,18,58,26,12,44,4,36,14,46,6,38,60,28,52,20,62,30,54,22,3,35,11,43,1,33,9,41,51,19,59,27,49,17,57,25,15,47,7,39,13,45,5,37,63,31,55,23,61,29,53,21];
  const ink = [44,27,23], paper = [238,220,186];

  function inside(x,y,points) {
    let result = false;
    for (let i=0,j=points.length-1;i<points.length;j=i++) {
      const [ax,ay]=points[i], [bx,by]=points[j];
      if ((ay>y)!==(by>y) && x<(bx-ax)*(y-ay)/(by-ay)+ax) result=!result;
    }
    return result;
  }

  function draw(panel,canvas) {
    const width=panel.clientWidth, height=panel.clientHeight;
    if (!width || !height) return;
    const pixel=width>700?2:1;
    const w=Math.ceil(width/pixel), h=Math.ceil(height/pixel);
    const scale=v=>Math.round(v/pixel);
    const b=scale(parseFloat(getComputedStyle(panel).getPropertyValue('--facet-size'))||24);
    const depth=scale(width>700?8:5), cut=b+scale(4);
    const W=w-depth-1, H=h-depth-1;
    canvas.width=w; canvas.height=h;
    const ctx=canvas.getContext('2d');
    const image=ctx.createImageData(w,h);
    const data=image.data;

    function paint(x,y,color) {
      const i=(y*w+x)*4;
      data[i]=color[0]; data[i+1]=color[1]; data[i+2]=color[2]; data[i+3]=255;
    }
    function face(points,density,outline=true) {
      const xs=points.map(p=>p[0]), ys=points.map(p=>p[1]);
      const left=Math.max(0,Math.floor(Math.min(...xs))), right=Math.min(w-1,Math.ceil(Math.max(...xs)));
      const top=Math.max(0,Math.floor(Math.min(...ys))), bottom=Math.min(h-1,Math.ceil(Math.max(...ys)));
      for(let y=top;y<=bottom;y++) for(let x=left;x<=right;x++) {
        if (!inside(x+.5,y+.5,points)) continue;
        const edge=outline && (!inside(x-.5,y+.5,points)||!inside(x+1.5,y+.5,points)||!inside(x+.5,y-.5,points)||!inside(x+.5,y+1.5,points));
        paint(x,y,edge||density>(bayer[(y%8)*8+x%8]+.5)/64?ink:paper);
      }
    }
    const outline=[[0,cut],[cut,0],[W-cut,0],[W,cut],[W,H-cut],[W-cut,H],[cut,H],[0,H-cut]];
    face(outline.map(([x,y])=>[x+depth,y+depth]),.86);
    face(outline,0);
    const tx=Math.round(W*.58), ly=Math.round(H*.58), ry=Math.round(H*.42), bx=Math.round(W*.42);
    // Each face is a separate lighting plane. Bayer thresholds express its shade.
    face([[cut,0],[tx,0],[tx-b,b],[b,b]],.06);
    face([[tx,0],[W-cut,0],[W-b,b],[tx-b,b]],.19);
    face([[0,cut],[b,b],[b,ly-b],[0,ly]],.16);
    face([[0,ly],[b,ly-b],[b,H-b],[0,H-cut]],.28);
    face([[W-b,b],[W,cut],[W,ry],[W-b,ry+b]],.62);
    face([[W-b,ry+b],[W,ry],[W,H-cut],[W-b,H-b]],.78);
    face([[b,H-b],[bx+b,H-b],[bx,H],[cut,H]],.43);
    face([[bx+b,H-b],[W-b,H-b],[W-cut,H],[bx,H]],.61);
    face([[0,cut],[cut,0],[b,b]],.28);
    face([[W-cut,0],[W,cut],[W-b,b]],.46);
    face([[W,H-cut],[W-cut,H],[W-b,H-b]],.88);
    face([[cut,H],[0,H-cut],[b,H-b]],.54);
    ctx.putImageData(image,0,0);
  }

  const observer=new ResizeObserver(entries=>{
    for (const entry of entries) draw(entry.target,entry.target.querySelector('.facet-surface'));
  });
  for (const panel of document.querySelectorAll('.game-frame, .facet-panel')) {
    const canvas=document.createElement('canvas');
    canvas.className='facet-surface';
    canvas.setAttribute('aria-hidden','true');
    panel.prepend(canvas);
    panel.classList.add('has-facets');
    observer.observe(panel);
  }
})();
