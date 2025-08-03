mergeInto(LibraryManager.library, {
  IsMobile: function() {
    var ua = navigator.userAgent || navigator.vendor || window.opera;

    // Android
    if (/android/i.test(ua)) return 1;

    // iPadOS(iOS 13+)
    if (/iPad|iPhone|iPod/.test(ua)) return 1;
    if (navigator.platform === 'MacIntel' && navigator.maxTouchPoints > 1) return 1;

    // その他タブレット
    if (/tablet|ipad|playbook|silk/i.test(ua)) return 1;

    return 0;
  }
});
