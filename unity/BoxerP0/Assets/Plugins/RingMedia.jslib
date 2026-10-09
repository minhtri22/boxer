mergeInto(LibraryManager.library, {
  BoxerRingCommand: function (command, token) {
    if (!window.boxerRingMedia) return;
    if (command === 1) window.boxerRingMedia.start(token);
    else if (command === 2) window.boxerRingMedia.finish();
    else if (command === 3) window.boxerRingMedia.setSound(true);
    else if (command === 4) window.boxerRingMedia.setSound(false);
    else window.boxerRingMedia.stop();
  }
});
