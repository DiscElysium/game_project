mergeInto(LibraryManager.library, {
    MaskedDiceViewState: function(ptr) {
        if (window.maskedDiceViewState) window.maskedDiceViewState(UTF8ToString(ptr));
    },
  MaskedDiceAIStatus: function (state) {
    if (typeof window.maskedDiceAIStatus === 'function') {
      window.maskedDiceAIStatus(UTF8ToString(state));
    }
  }
});
