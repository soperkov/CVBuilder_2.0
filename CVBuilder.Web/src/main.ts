import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { AppModule } from './app/app.module';

platformBrowserDynamic()
  .bootstrapModule(AppModule, {
    ngZoneEventCoalescing: true,
  })
  .catch((err) => console.error(err));

// === Cursor Water Ripple controller =====================================
(() => {
  const prefersReduced = window.matchMedia?.(
    '(prefers-reduced-motion: reduce)'
  ).matches;
  if (prefersReduced) return;

  // Ensure overlay exists
  let overlay = document.getElementById(
    'ripple-overlay'
  ) as HTMLDivElement | null;
  if (!overlay) {
    overlay = document.createElement('div');
    overlay.id = 'ripple-overlay';
    overlay.setAttribute('aria-hidden', 'true');
    overlay.innerHTML = '<div id="ripple-rings"></div>';
    document.body.prepend(overlay);
  }
  const ringsHost = overlay.querySelector('#ripple-rings') as HTMLDivElement;

  // Areas considered "solid UI" where ripple should be hidden / not spawned
  const SOLID_SELECTOR = [
    '[data-no-ripple]',
    '.form-container',
    '.glass-card',
    '.welcome-panel',
    '.auth-fixed',
    '.card',
    '.btn-group',
    '.btn',
    'button',
    'input',
    'select',
    'textarea',
    'label',
    'a',
  ].join(',');

  let raf = 0;
  let lastRingAt = 0;
  const RING_INTERVAL = 50; // ms between spawned rings while moving
  const BASE_SIZE = () => Math.max(window.innerWidth, window.innerHeight) * 0.1; // ring size

  function isOverSolid(x: number, y: number) {
    const el = document.elementFromPoint(x, y);
    return !!el?.closest(SOLID_SELECTOR);
  }

  function setCursorVars(x: number, y: number) {
    // put vars on overlay so CSS can read them
    overlay!.style.setProperty('--rx', `${x}px`);
    overlay!.style.setProperty('--ry', `${y}px`);
  }

  function spawnRing(x: number, y: number, strong = false) {
    const ring = document.createElement('span');
    ring.className = 'ripple-ring' + (strong ? ' is-press' : '');
    ring.style.setProperty('--x', `${x}px`);
    ring.style.setProperty('--y', `${y}px`);
    ring.style.setProperty('--size', `${BASE_SIZE()}px`);
    ringsHost.appendChild(ring);
    ring.addEventListener('animationend', () => ring.remove(), { once: true });
  }

  function onMove(e: PointerEvent | MouseEvent) {
    const x = (e as PointerEvent).clientX ?? (e as MouseEvent).clientX;
    const y = (e as PointerEvent).clientY ?? (e as MouseEvent).clientY;
    cancelAnimationFrame(raf);
    raf = requestAnimationFrame(() => {
      setCursorVars(x, y);

      // Only visible on empty/background areas
      const solid = isOverSolid(x, y);
      overlay!.style.opacity = solid ? '0' : '1';
      if (solid) return;

      // Spawn trailing rings while moving (throttled)
      const now = performance.now();
      if (now - lastRingAt > RING_INTERVAL) {
        lastRingAt = now;
        spawnRing(x, y, false);
      }
    });
  }

  function onPress(e: PointerEvent | MouseEvent) {
    const x = (e as PointerEvent).clientX ?? (e as MouseEvent).clientX;
    const y = (e as PointerEvent).clientY ?? (e as MouseEvent).clientY;
    if (!isOverSolid(x, y)) {
      spawnRing(x, y, true);
      setTimeout(() => spawnRing(x, y, false), 120); // subtle echo ring
    }
  }

  function hide() {
    overlay!.style.opacity = '0';
  }

  // Pointer events
  window.addEventListener('pointermove', onMove as any, { passive: true });
  window.addEventListener('mousemove', onMove as any, { passive: true }); // fallback
  window.addEventListener('pointerdown', onPress as any, { passive: true });
  window.addEventListener('mousedown', onPress as any, { passive: true }); // fallback
  window.addEventListener('pointerleave', hide);
  window.addEventListener('blur', hide);
})();
