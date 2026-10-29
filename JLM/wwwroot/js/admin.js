/* ==========================================================================
   JLM — admin interactions
   ========================================================================== */
(function () {
  'use strict';

  function slugify(text) {
    return String(text || '')
      .toLowerCase()
      .normalize('NFD').replace(/[\u0300-\u036f]/g, '')
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/^-+|-+$/g, '')
      .slice(0, 80);
  }

  // Slug auto-generation
  document.addEventListener('input', (e) => {
    const src = e.target.closest('[data-slug-source]');
    if (!src) return;
    const form = src.closest('form') || document;
    const target = form.querySelector('[data-slug-target]');
    if (!target) return;
    if (target.dataset.touched === 'true') return;
    target.value = slugify(src.value);
  });
  document.addEventListener('input', (e) => {
    const t = e.target.closest('[data-slug-target]');
    if (t) t.dataset.touched = 'true';
  });

  // Image preview (file)
  document.addEventListener('change', (e) => {
    const input = e.target.closest('[data-image-input]');
    if (!input || !input.files || !input.files[0]) return;
    const file = input.files[0];
    const reader = new FileReader();
    reader.onload = (ev) => {
      const preview = document.querySelector('[data-image-preview]');
      const placeholder = document.querySelector('[data-image-placeholder]');
      if (preview) {
        preview.src = ev.target.result;
      } else if (placeholder) {
        const img = document.createElement('img');
        img.src = ev.target.result;
        img.alt = 'Prévia da imagem principal';
        img.setAttribute('data-image-preview', '');
        placeholder.replaceWith(img);
      }
    };
    reader.readAsDataURL(file);
  });

  // Image preview (URL)
  document.addEventListener('input', (e) => {
    const input = e.target.closest('[data-image-url]');
    if (!input) return;
    const url = input.value.trim();
    if (!url) return;
    const preview = document.querySelector('[data-image-preview]');
    if (preview) preview.src = url;
  });

  // Copy to clipboard (admin fallback; site.js also handles)
  document.addEventListener('click', async (e) => {
    const btn = e.target.closest('[data-copy]');
    if (!btn) return;
    e.preventDefault();
    try {
      await navigator.clipboard.writeText(btn.getAttribute('data-copy'));
      btn.classList.add('is-copied');
      setTimeout(() => btn.classList.remove('is-copied'), 1200);
    } catch { /* ignore */ }
  });
})();
