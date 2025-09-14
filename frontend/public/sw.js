// Empty service worker to override any existing ones
self.addEventListener('install', () => {
  self.skipWaiting();
});

self.addEventListener('activate', () => {
  self.clients.claim();
});

// Don't intercept any requests
self.addEventListener('fetch', (event) => {
  // Let all requests pass through normally
  return;
});