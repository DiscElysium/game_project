mergeInto(LibraryManager.library, {
  MD_BrowserState: function (value) {
    if (window.maskedDice) window.maskedDice.state = JSON.parse(UTF8ToString(value));
  },
  MD_SyncStorage: function () {
    if (window.mdSyncing) return;
    window.mdSyncing = true;
    FS.syncfs(false, function () { window.mdSyncing = false; });
  },
  MD_Download: function (name, text) {
    var url = URL.createObjectURL(new Blob([UTF8ToString(text)], {type: 'application/x-ndjson'}));
    var a = document.createElement('a'); a.href = url; a.download = UTF8ToString(name);
    document.body.appendChild(a); a.click(); a.remove(); setTimeout(function () { URL.revokeObjectURL(url); }, 1000);
  },
  MD_AICancel: function (id) {
    if (window.mdRequests && window.mdRequests[id]) window.mdRequests[id].abort();
  },
  MD_AIStart: function (target, id, url, body, timeout) {
    var object = UTF8ToString(target), endpoint = UTF8ToString(url), payload = UTF8ToString(body);
    var controller = new AbortController();
    window.mdRequests = window.mdRequests || {}; window.mdRequests[id] = controller;
    var deadline = setTimeout(function () { controller.abort(); }, timeout);
    var buffer = '', flushTimer = null;
    function send(kind, data, status) {
      window.maskedDice.instance.SendMessage(object, 'ReceiveBrowserAI', JSON.stringify({id:id,kind:kind,data:data || '',status:status || 0}));
    }
    function flush() { clearTimeout(flushTimer); flushTimer = null; if (buffer) { send('chunk',buffer,200); buffer = ''; } }
    fetch(endpoint, {method:'POST',headers:{'Content-Type':'application/json'},body:payload,signal:controller.signal})
      .then(async function (response) {
        send('headers','',response.status);
        if (!response.ok || !response.body) { send('error','',response.status); return; }
        var reader = response.body.getReader(), decoder = new TextDecoder();
        while (true) {
          var packet = await reader.read();
          if (packet.done) break;
          buffer += decoder.decode(packet.value,{stream:true});
          if (buffer.length >= 32768) flush(); else if (!flushTimer) flushTimer = setTimeout(flush,50);
        }
        buffer += decoder.decode(); flush(); send('done','',response.status);
      }).catch(function () { flush(); send('error','',0); })
      .finally(function () { clearTimeout(deadline); clearTimeout(flushTimer); delete window.mdRequests[id]; });
  }
});
