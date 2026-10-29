/* ==========================================================================
   JLM — public interactions
   ========================================================================== */
(function () {
  'use strict';

  const $ = (sel, ctx = document) => ctx.querySelector(sel);
  const $$ = (sel, ctx = document) => Array.from(ctx.querySelectorAll(sel));

  /* ---------- Mobile menu ---------- */
  const mobileMenu = $('#mobile-menu');
  const openMenu = () => { if (mobileMenu) { mobileMenu.setAttribute('aria-hidden', 'false'); mobileMenu.classList.add('is-open'); document.body.classList.add('menu-open'); } };
  const closeMenu = () => { if (mobileMenu) { mobileMenu.setAttribute('aria-hidden', 'true'); mobileMenu.classList.remove('is-open'); document.body.classList.remove('menu-open'); } };
  document.addEventListener('click', (e) => {
    const t = e.target.closest('[data-menu-open]');
    if (t) { e.preventDefault(); openMenu(); }
    const c = e.target.closest('[data-menu-close]');
    if (c) { e.preventDefault(); closeMenu(); }
    if (e.target === mobileMenu) closeMenu();
  });
  document.addEventListener('keydown', (e) => { if (e.key === 'Escape') { closeMenu(); closeSearch(); } });

  /* ---------- Search overlay ---------- */
  const searchOverlay = $('#search-overlay');
  const searchInput = $('[data-search-input]');
  const openSearch = () => {
    if (!searchOverlay) return;
    searchOverlay.setAttribute('aria-hidden', 'false');
    searchOverlay.classList.add('is-open');
    document.body.classList.add('search-open');
    setTimeout(() => searchInput && searchInput.focus(), 80);
  };
  const closeSearch = () => {
    if (!searchOverlay) return;
    searchOverlay.setAttribute('aria-hidden', 'true');
    searchOverlay.classList.remove('is-open');
    document.body.classList.remove('search-open');
  };
  document.addEventListener('click', (e) => {
    if (e.target.closest('[data-search-open]')) { e.preventDefault(); openSearch(); }
    if (e.target.closest('[data-search-close]')) { e.preventDefault(); closeSearch(); }
    if (e.target === searchOverlay) closeSearch();
  });

  /* ---------- Toasts ---------- */
  function toastStack() {
    let stack = document.querySelector('.toast-stack--ajax');
    if (!stack) {
      stack = document.createElement('div');
      stack.className = 'toast-stack toast-stack--ajax';
      stack.setAttribute('aria-live', 'polite');
      stack.setAttribute('data-ajax-toast', '');
      document.body.appendChild(stack);
    }
    return stack;
  }
  function showToast(message, type = 'success', icon = 'i-check') {
    const stack = toastStack();
    const toast = document.createElement('div');
    toast.className = `toast toast--${type}`;
    toast.setAttribute('role', 'status');
    toast.innerHTML = `<span class="toast__icon"><svg class="icon"><use href="#${icon}"></use></svg></span><p>${escapeHtml(message)}</p><button type="button" class="toast__close" data-toast-close aria-label="Fechar aviso"><svg class="icon"><use href="#i-close"></use></svg></button>`;
    stack.appendChild(toast);
    requestAnimationFrame(() => toast.classList.add('is-in'));
    const remove = () => {
      toast.classList.add('is-leaving');
      setTimeout(() => toast.remove(), 300);
    };
    toast.querySelector('[data-toast-close]').addEventListener('click', remove);
    setTimeout(remove, 4200);
  }
  function escapeHtml(s) { return String(s).replace(/[&<>"']/g, (c) => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c])); }

  /* auto-dismiss server toasts */
  $$('.toast[data-toast]').forEach((t) => {
    const close = t.querySelector('[data-toast-close]');
    const remove = () => { t.classList.add('is-leaving'); setTimeout(() => t.remove(), 300); };
    if (close) close.addEventListener('click', remove);
    setTimeout(remove, 4500);
  });

  /* ---------- Count badges ---------- */
  function setCount(selector, count) {
    $$(selector).forEach((el) => {
      el.textContent = count;
      el.classList.toggle('is-hidden', !count || count === 0);
    });
  }

  /* ---------- Quantity steppers ---------- */
  function clamp(v, min, max) {
    v = parseInt(v, 10);
    if (isNaN(v)) v = min || 1;
    if (min != null && v < min) v = min;
    if (max != null && v > max) v = max;
    return v;
  }
  document.addEventListener('click', (e) => {
    const minus = e.target.closest('[data-quantity-minus]');
    const plus = e.target.closest('[data-quantity-plus]');
    if (!minus && !plus) return;
    const wrap = (minus || plus).closest('[data-quantity], .quantity');
    if (!wrap) return;
    const input = wrap.querySelector('input[type="number"]');
    if (!input) return;
    const min = input.getAttribute('min') != null ? parseInt(input.getAttribute('min'), 10) : 1;
    const max = input.getAttribute('max') != null ? parseInt(input.getAttribute('max'), 10) : Infinity;
    let val = parseInt(input.value, 10) || min;
    if (minus) val -= 1; else val += 1;
    input.value = clamp(val, min, max);
    input.dispatchEvent(new Event('change', { bubbles: true }));
  });
  document.addEventListener('change', (e) => {
    const input = e.target;
    if (!input.matches('input[type="number"][data-quantity-input], .quantity input[type="number"]')) return;
    const min = input.getAttribute('min') != null ? parseInt(input.getAttribute('min'), 10) : 1;
    const max = input.getAttribute('max') != null ? parseInt(input.getAttribute('max'), 10) : Infinity;
    input.value = clamp(input.value, min, max);
    const form = input.closest('form[data-quantity-form]');
    if (form) {
      // auto submit
      setTimeout(() => form.requestSubmit ? form.requestSubmit() : form.submit(), 60);
    }
  });

  /* ---------- AJAX forms (add to cart / wishlist) ---------- */
  function getAntiForgeryToken(form) {
    const el = form.querySelector('input[name="__RequestVerificationToken"]');
    return el ? el.value : '';
  }

  async function postForm(form) {
    const fd = new FormData(form);
    const token = getAntiForgeryToken(form);
    const res = await fetch(form.action, {
      method: 'POST',
      body: fd,
      headers: {
        'X-Requested-With': 'XMLHttpRequest',
        'Accept': 'application/json',
        ...(token ? { 'RequestVerificationToken': token } : {})
      },
      credentials: 'same-origin'
    });
    let data = null;
    try { data = await res.json(); } catch { data = null; }
    return { res, data };
  }

  document.addEventListener('submit', async (e) => {
    const form = e.target;
    if (!form.matches || !form.matches('form[data-ajax-form]')) return;
    // wishlist
    if (form.matches('[data-wishlist-form]')) {
      e.preventDefault();
      const { res, data } = await postForm(form);
      if (res.ok && data && data.success) {
        setCount('[data-wishlist-count]', data.count);
        const btn = form.querySelector('[data-wishlist-button]');
        if (btn) {
          btn.classList.toggle('is-active', !!data.added);
          const span = btn.querySelector('span');
          if (span) span.textContent = data.added ? 'Nos favoritos' : 'Salvar nos favoritos';
        }
        showToast(data.message, 'success', data.added ? 'i-heart' : 'i-check');
      } else {
        showToast((data && data.message) || 'Não foi possível atualizar os favoritos.', 'error', 'i-alert');
      }
      return;
    }
    // add to cart
    if (form.matches('[data-add-to-cart]') || form.getAttribute('action')?.includes('/carrinho/adicionar')) {
      e.preventDefault();
      const { res, data } = await postForm(form);
      if (res.ok && data && data.success) {
        setCount('[data-cart-count]', data.count);
        showToast(data.message || 'Peça adicionada ao carrinho.', 'success', 'i-bag');
      } else {
        showToast((data && data.message) || 'Não foi possível adicionar ao carrinho.', 'error', 'i-alert');
      }
      return;
    }
    // generic ajax form: let it submit normally unless handled
  });

  /* ---------- Buy now ---------- */
  document.addEventListener('click', async (e) => {
    const btn = e.target.closest('[data-buy-now]');
    if (!btn) return;
    e.preventDefault();
    const form = btn.closest('form') || document.querySelector('[data-product-form]');
    if (!form) { window.location.href = '/checkout'; return; }
    const { res, data } = await postForm(form);
    if (res.ok && data && data.success) {
      setCount('[data-cart-count]', data.count);
      window.location.href = '/checkout';
    } else {
      showToast((data && data.message) || 'Não foi possível comprar agora.', 'error', 'i-alert');
    }
  });

  /* ---------- Confirm dialogs ---------- */
  document.addEventListener('submit', (e) => {
    const form = e.target;
    const msg = form.getAttribute('data-confirm');
    if (msg && !form.dataset.confirmed) {
      if (!window.confirm(msg)) { e.preventDefault(); return; }
      form.dataset.confirmed = '1';
    }
  });

  /* ---------- Password toggle ---------- */
  document.addEventListener('click', (e) => {
    const btn = e.target.closest('[data-password-toggle]');
    if (!btn) return;
    const wrap = btn.closest('.input-password') || btn.parentElement;
    const input = wrap ? wrap.querySelector('input') : null;
    if (!input) return;
    const show = input.type === 'password';
    input.type = show ? 'text' : 'password';
    btn.setAttribute('aria-pressed', show ? 'true' : 'false');
  });

  /* ---------- Gallery (product) ---------- */
  document.addEventListener('click', (e) => {
    const thumb = e.target.closest('[data-gallery-thumb]');
    if (thumb) {
      const url = thumb.getAttribute('data-gallery-thumb');
      const main = thumb.closest('[data-gallery]')?.querySelector('[data-gallery-image]');
      if (main && url) main.src = url;
      thumb.closest('[data-gallery]')?.querySelectorAll('.product-gallery__thumb').forEach((t) => t.classList.toggle('is-active', t === thumb));
      return;
    }
    const zoom = e.target.closest('[data-gallery-zoom]');
    if (zoom) {
      const img = zoom.closest('[data-gallery]')?.querySelector('[data-gallery-image]');
      if (img) window.open(img.src, '_blank');
    }
  });

  /* ---------- Color label update ---------- */
  document.addEventListener('change', (e) => {
    const input = e.target;
    if (!input.matches('input[name="color"]')) return;
    const label = document.querySelector('[data-color-label]');
    if (label) label.textContent = input.value;
  });

  /* ---------- Filters toggle (mobile) ---------- */
  document.addEventListener('click', (e) => {
    if (e.target.closest('[data-filters-toggle]')) {
      const panel = document.querySelector('[data-filters-panel]');
      if (panel) panel.classList.toggle('is-open');
    }
    if (e.target.closest('[data-filters-close]')) {
      const panel = document.querySelector('[data-filters-panel]');
      if (panel) panel.classList.remove('is-open');
    }
  });

  /* ---------- Auto submit selects ---------- */
  document.addEventListener('change', (e) => {
    const el = e.target;
    if (el.matches('[data-auto-submit]')) {
      const form = el.closest('form');
      if (form) form.submit();
    }
  });

  /* ---------- Copy to clipboard ---------- */
  document.addEventListener('click', async (e) => {
    const btn = e.target.closest('[data-copy], [data-copy-link]');
    if (!btn) return;
    e.preventDefault();
    let text = btn.getAttribute('data-copy');
    if (!text) text = window.location.href;
    try {
      await navigator.clipboard.writeText(text);
      showToast('Link copiado.', 'success', 'i-copy');
    } catch {
      showToast('Não foi possível copiar.', 'error', 'i-alert');
    }
  });

  /* ---------- Masks ---------- */
  function applyMask(input, fn) {
    input.addEventListener('input', () => {
      const start = input.selectionStart;
      const before = input.value.length;
      input.value = fn(input.value);
      // crude cursor keep
    });
  }
  const masks = {
    phone: (v) => {
      v = v.replace(/\D/g, '').slice(0, 11);
      if (v.length <= 10) return v.replace(/(\d{2})(\d{4})(\d{0,4})/, '($1) $2-$3').trim();
      return v.replace(/(\d{2})(\d{5})(\d{0,4})/, '($1) $2-$3').trim();
    },
    cpf: (v) => {
      v = v.replace(/\D/g, '').slice(0, 11);
      return v.replace(/(\d{3})(\d{3})(\d{3})(\d{0,2})/, '$1.$2.$3-$4').replace(/[-.]$/, '');
    },
    cep: (v) => {
      v = v.replace(/\D/g, '').slice(0, 8);
      return v.replace(/(\d{5})(\d{0,3})/, '$1-$2').replace(/-$/, '');
    },
    card: (v) => v.replace(/\D/g, '').slice(0, 16).replace(/(.{4})/g, '$1 ').trim(),
    expiry: (v) => {
      v = v.replace(/\D/g, '').slice(0, 4);
      if (v.length >= 3) v = v.slice(0, 2) + '/' + v.slice(2);
      return v;
    }
  };
  $$('[data-mask-phone]').forEach((i) => applyMask(i, masks.phone));
  $$('[data-mask-cpf]').forEach((i) => applyMask(i, masks.cpf));
  $$('[data-mask-cep]').forEach((i) => applyMask(i, masks.cep));
  $$('[data-mask-card]').forEach((i) => applyMask(i, masks.card));
  $$('[data-mask-expiry]').forEach((i) => applyMask(i, masks.expiry));

  /* ---------- CEP lookup ---------- */
  async function lookupCep(input) {
    const cep = input.value.replace(/\D/g, '');
    if (cep.length !== 8) return;
    const status = document.querySelector('[data-cep-status]');
    if (status) status.textContent = 'Buscando...';
    try {
      const res = await fetch(`https://viacep.com.br/ws/${cep}/json/`);
      const data = await res.json();
      if (data.erro) throw new Error('CEP não encontrado');
      const set = (sel, val) => { const el = document.querySelector(sel); if (el && val) el.value = val; };
      set('[data-cep-address]', data.logradouro);
      set('[data-cep-neighborhood]', data.bairro);
      set('[data-cep-city]', data.localidade);
      set('[data-cep-state]', data.uf);
      if (status) status.textContent = 'Endereço encontrado';
    } catch (err) {
      if (status) status.textContent = 'CEP não encontrado';
    }
  }
  document.addEventListener('blur', (e) => {
    if (e.target.matches('[data-cep-input]')) lookupCep(e.target);
  }, true);

  /* ---------- Checkout totals ---------- */
  const subtotalEl = null;
  function parseBrl(s) { return parseFloat(String(s).replace(/[^\d,]/g, '').replace(',', '.')) || 0; }
  function formatBrl(n) { return n.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' }); }
  function updateCheckoutTotals() {
    const form = document.querySelector('[data-checkout-form]');
    if (!form) return;
    const subtotalText = document.querySelector('.checkout-summary__head')?.dataset?.subtotal;
    // subtotal from a data attribute if present; fallback parse from first summary line
    let subtotal = 0;
    const subLine = document.querySelector('.checkout-summary .summary-lines')?.querySelector('div:first-child dd');
    if (subLine) subtotal = parseBrl(subLine.textContent);
    // shipping
    const shipInput = form.querySelector('input[name="ShippingMethod"]:checked') || form.querySelector('input[data-shipping-option]:checked');
    const method = shipInput ? shipInput.value : 'Standard';
    let cost = 0;
    if (method === 'Express') cost = subtotal >= 900 ? 0 : 49.90;
    else cost = subtotal >= 500 ? 0 : 29.90;
    // update radio labels
    form.querySelectorAll('[data-shipping-option]').forEach((inp) => {
      const c = parseFloat(inp.getAttribute('data-cost')) || 0;
      const free = inp.value === 'Express' ? subtotal >= 900 : subtotal >= 500;
      const em = inp.closest('.radio-card')?.querySelector('em');
      if (em) em.textContent = free ? 'Grátis' : formatBrl(c);
    });
    const shipDisplay = document.querySelector('[data-shipping-display]');
    if (shipDisplay) shipDisplay.textContent = cost === 0 ? 'Grátis' : formatBrl(cost);
    const totalDisplay = document.querySelector('[data-total-display]');
    // discount (coupon) parse from existing discount line if present
    let discount = 0;
    const discLine = document.querySelector('.checkout-summary .summary-lines__discount dd');
    if (discLine) discount = parseBrl(discLine.textContent);
    const total = Math.max(0, subtotal + cost - discount);
    if (totalDisplay) totalDisplay.textContent = formatBrl(total);
    const inst = document.querySelector('.checkout-summary .summary-installments strong');
    if (inst) inst.textContent = formatBrl(total / 6);
  }
  document.addEventListener('change', (e) => {
    if (e.target.matches('[data-shipping-option], [data-payment-option]')) {
      updateCheckoutTotals();
      // payment panels
      if (e.target.matches('[data-payment-option]')) {
        const val = e.target.value;
        document.querySelectorAll('[data-payment-panel]').forEach((p) => {
          p.hidden = p.getAttribute('data-payment-panel') !== val;
        });
      }
    }
  });
  // init
  if (document.querySelector('[data-checkout-form]')) {
    updateCheckoutTotals();
    const checked = document.querySelector('[data-payment-option]:checked');
    if (checked) {
      document.querySelectorAll('[data-payment-panel]').forEach((p) => {
        p.hidden = p.getAttribute('data-payment-panel') !== checked.value;
      });
    }
  }

  /* ---------- Reveal on scroll ---------- */
  const io = 'IntersectionObserver' in window ? new IntersectionObserver((entries) => {
    entries.forEach((en) => { if (en.isIntersecting) { en.target.classList.add('is-visible'); io.unobserve(en.target); } });
  }, { threshold: .12 }) : null;
  if (io) $$('.reveal, .product-card, .category-card, .collection-card').forEach((el) => { el.classList.add('reveal'); io.observe(el); });

  /* ---------- Admin sidebar (shared) ---------- */
  document.addEventListener('click', (e) => {
    if (e.target.closest('[data-admin-menu-open]')) {
      document.querySelector('[data-admin-sidebar]')?.classList.add('is-open');
      document.querySelector('[data-admin-overlay]')?.classList.add('is-open');
    }
    if (e.target.closest('[data-admin-menu-close]') || e.target.closest('[data-admin-overlay]')) {
      document.querySelector('[data-admin-sidebar]')?.classList.remove('is-open');
      document.querySelector('[data-admin-overlay]')?.classList.remove('is-open');
    }
  });
})();
