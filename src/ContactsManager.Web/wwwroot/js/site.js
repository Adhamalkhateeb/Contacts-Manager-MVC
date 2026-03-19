/**
 * Contact Manager — site.js
 * Minimal, focused JavaScript. Only what's needed.
 */
(function () {
    'use strict';

    // Auto-dismiss any flash/alert messages after 5s
    document.querySelectorAll('[data-auto-dismiss]').forEach(function (el) {
        setTimeout(function () {
            el.style.transition = 'opacity .4s';
            el.style.opacity = '0';
            setTimeout(function () { el.remove(); }, 400);
        }, 5000);
    });

    // Keyboard shortcut: / to focus search input
    document.addEventListener('keydown', function (e) {
        if (e.key === '/' && !e.ctrlKey && !e.metaKey && !e.target.matches('input,textarea,select')) {
            e.preventDefault();
            var search = document.getElementById('searchValueText');
            if (search && !search.classList.contains('hidden')) {
                search.focus();
                search.select();
            }
        }
    });

    // Confirm before navigating away from dirty forms
    document.querySelectorAll('form[data-confirm-leave]').forEach(function (form) {
        var dirty = false;
        form.addEventListener('input', function () { dirty = true; });
        form.addEventListener('submit', function () { dirty = false; });
        window.addEventListener('beforeunload', function (e) {
            if (dirty) {
                e.preventDefault();
                e.returnValue = '';
            }
        });
    });

})();