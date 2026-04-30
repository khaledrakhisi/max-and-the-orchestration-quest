const { contextBridge, ipcRenderer } = require('electron');

// Exposes a controlled, safe API to the React renderer process
// via window.electron — React cannot access Node.js directly
contextBridge.exposeInMainWorld('electron', {

  // React calls this to launch Unity .exe and Python WebSocket server
  launchGame: (levelId) => {
    ipcRenderer.send('launch-game', levelId);
  },

  // React calls this when the player navigates back to the menu
  stopGame: () => {
    ipcRenderer.send('stop-game');
  },

  // React calls this to check if Unity/Python are currently running
  getServiceStatus: () => {
    return ipcRenderer.invoke('get-service-status');
  },

  // React registers a callback for when Unity window is closed by the player
  onUnityClosed: (callback) => {
    ipcRenderer.on('unity-closed', () => callback());
  },

  // React registers a callback for when Unity fails to launch
  onUnityError: (callback) => {
    ipcRenderer.on('unity-error', (event, message) => callback(message));
  },

  // Cleanup listeners to avoid memory leaks
  removeAllListeners: (channel) => {
    ipcRenderer.removeAllListeners(channel);
  }
});
