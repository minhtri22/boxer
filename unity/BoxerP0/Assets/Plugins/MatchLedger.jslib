mergeInto(LibraryManager.library, {
  BoxerLedgerCommand: function (command,payload) {
    if(window.boxerLedger)window.boxerLedger.command(command,UTF8ToString(payload));
  }
});
