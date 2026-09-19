# Changelog

## 1.2 - 2026-09-19

- Added immediate bright green RUNNING status after START is clicked.
- Prevented startup status from flashing back to STOPPED while npm, Vite, or Node is still binding its port.
- Refreshed port status every 500 ms.
- Displayed the real listener PID when the configured port is listening.
- Enabled OPEN only after the service port is available.
- Added npm and npx parent project-root detection for selected child folders.
- Preserved process-tree shutdown through `taskkill /T /F`.
