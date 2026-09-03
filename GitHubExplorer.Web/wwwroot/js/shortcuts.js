document.addEventListener('keydown', function (e) {
    if (e.key !== '/') return;
    var tag = document.activeElement && document.activeElement.tagName;
    if (tag === 'INPUT' || tag === 'TEXTAREA') return;
    var box = document.getElementById('repo-search');
    if (box) {
        e.preventDefault();
        box.focus();
    }
});
