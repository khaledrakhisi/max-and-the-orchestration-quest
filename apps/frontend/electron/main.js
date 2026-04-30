const { app, BrowserWindow, ipcMain } = require('electron');
const path = require('path');
const { spawn } = require('child_process');

console.log("🔥 MAIN PROCESS STARTED");
// ============================================================
// PATHS — Update UNITY_EXE_PATH once you have a Unity build
// ============================================================
const PYTHON_SCRIPT_PATH_DOCKER = path.join(__dirname, '../../backend/docker_server.py');
const PYTHON_SCRIPT_PATH_API = path.join(__dirname, '../../backend/api_server.py');
const UNITY_EXE_PATH = path.join(__dirname, '../../unity/max/build_file/Max.exe');
console.log(UNITY_EXE_PATH);

let mainWindow;
let pythonProcess = null;
let apiProcess = null;
let unityProcess = null;

// ============================================================
// CREATE MAIN WINDOW
// ============================================================
function createWindow() {
  mainWindow = new BrowserWindow({
    width: 1280,
    height: 720,
    minWidth: 960,
    minHeight: 600,
    title: 'Max and the Orchestration Quest',
    icon: path.join(__dirname, '../public/favicon.ico'),
    backgroundColor: '#1a1c2c',
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      nodeIntegration: false,
      contextIsolation: true,
    }
  });

  const isDev = process.env.NODE_ENV === 'development';

  if (isDev) {
    // Development: load from React dev server
    mainWindow.loadURL('http://localhost:3001');
  } else {
    // Production: load built React files
    mainWindow.loadFile(path.join(__dirname, '../build/index.html'));
  }

  mainWindow.on('closed', () => {
    mainWindow = null;
  });
}

// ============================================================
// IPC HANDLERS — React communicates via these
// ============================================================

// Called when "Start Game" or a Level card is clicked
ipcMain.on('launch-game', (event, levelId) => {
  console.log(`[Electron] Launching game for Level ${levelId}...`);
  launchServices(levelId);
});

// Called when player returns to main menu
ipcMain.on('stop-game', () => {
  console.log('[Electron] Game stopped. Cleaning up services...');
  stopServices();
});

// React can check if services are currently running
ipcMain.handle('get-service-status', () => {
  return {
    pythonRunning: pythonProcess !== null,
    unityRunning: unityProcess !== null,
  };
});

// ============================================================
// SERVICE LAUNCHERS
// ============================================================

function launchServices(levelId = 1) {
  // 1. Start Python WebSocket server if not already running
  if (!pythonProcess) {
    console.log('[Electron] Starting Python WebSocket server...');
    pythonProcess = spawn('python', [PYTHON_SCRIPT_PATH_DOCKER], {
      stdio: 'pipe',
      shell: false
    });

    pythonProcess.stdout.on('data', (data) => {
      console.log(`[Python] ${data.toString().trim()}`);
    });

    pythonProcess.stderr.on('data', (data) => {
      console.error(`[Python Error] ${data.toString().trim()}`);
    });

    pythonProcess.on('exit', (code) => {
      console.log(`[Electron] Python exited with code: ${code}`);
      pythonProcess = null;
    });
  }
  // 2. Start Python WebSocket server if not already running
  if (!apiProcess) {
    console.log('[Electron] Starting Python api server...');
    apiProcess = spawn('python', [PYTHON_SCRIPT_PATH_API], {
      stdio: 'pipe',
      shell: false
    });

    apiProcess.stdout.on('data', (data) => {
      console.log(`[Python] ${data.toString().trim()}`);
    });

    apiProcess.stderr.on('data', (data) => {
      console.error(`[Python Error] ${data.toString().trim()}`);
    });

    apiProcess.on('exit', (code) => {
      console.log(`[Electron] Python exited with code: ${code}`);
      apiProcess = null;
    });
  }

  // 3. Start Unity standalone .exe if not already running
  if (!unityProcess) {
    console.log(`[Electron] Starting Unity game for level ${levelId}...`);
    try {
      unityProcess = spawn(UNITY_EXE_PATH, [`-level=${levelId}`], {
        detached: false,
        shell: false
      });

      unityProcess.on('exit', (code) => {
        console.log(`[Electron] Unity exited with code: ${code}`);
        unityProcess = null;
        // Notify React that Unity was closed so it can go back to the menu
        if (mainWindow && !mainWindow.isDestroyed()) {
          mainWindow.webContents.send('unity-closed');
        }
      });

      unityProcess.on('error', (err) => {
        console.error(`[Electron] Failed to launch Unity: ${err.message}`);
        console.error('[Electron] Make sure Unity is built to: ' + UNITY_EXE_PATH);
        if (mainWindow && !mainWindow.isDestroyed()) {
          mainWindow.webContents.send('unity-error', err.message);
        }
      });

    } catch (err) {
      console.error(`[Electron] Unity spawn failed: ${err.message}`);
    }
  }
}

function stopServices() {
  if (pythonProcess) {
    console.log('[Electron] Killing Python process...');
    pythonProcess.kill();
    pythonProcess = null;
  }
  if (unityProcess) {
    console.log('[Electron] Killing Unity process...');
    unityProcess.kill();
    unityProcess = null;
  }
}

// ============================================================
// APP LIFECYCLE
// ============================================================

app.whenReady().then(createWindow);

// Ensure all child processes are killed when app closes
app.on('will-quit', () => {
  console.log('[Electron] App closing — cleaning up all processes...');
  stopServices();
});

app.on('window-all-closed', () => {
  stopServices();
  app.quit();
});

app.on('activate', () => {
  if (BrowserWindow.getAllWindows().length === 0) {
    createWindow();
  }
});
