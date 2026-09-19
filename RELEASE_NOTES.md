# Release Notes

## Local App Control Center v1.2

This release improves startup feedback and service detection for local frontend/backend workflows.

### Highlights

- START now shows a bright green RUNNING state immediately.
- Port checks refresh every 500 ms.
- OPEN becomes available after the configured port is listening.
- Listener PID detection reports the real process bound to the configured port.
- npm and npx commands can recover from a selected child folder by finding the nearest parent folder with `package.json`.
- STOP continues to terminate the launched process tree.

### Upgrade

Replace the previous executable with the new build. Existing `controlcenter.ini` settings remain compatible.
