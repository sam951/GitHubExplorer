(function () {
    var saved = null;
    try { saved = localStorage.getItem('theme'); } catch (e) { }
    if (saved === 'dark' || saved === 'light')
        document.documentElement.setAttribute('data-theme', saved);

    document.addEventListener('click', function (e) {
        if (!e.target.closest('#theme-toggle')) return;
        var current = document.documentElement.getAttribute('data-theme')
            || (window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light');
        var next = current === 'dark' ? 'light' : 'dark';
        document.documentElement.setAttribute('data-theme', next);
        try { localStorage.setItem('theme', next); } catch (e) { }
    });
})();
