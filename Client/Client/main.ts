import { app, BrowserWindow, screen } from 'electron';
import * as path from 'path';

let win, serve;
let apiProcess = null;
const args = process.argv.slice(1);
serve = args.some(val => val === "--serve");

if (serve) {
    require('electron-reload')(__dirname, {
    });
}

function createWindow() {
    let electronScreen = screen;
    let size = electronScreen.getPrimaryDisplay().workAreaSize;
    win = new BrowserWindow({
        x: size.width / 2 - 1020 / 2,
        y: size.height / 2 - 640 / 2,
        width: 1020,
        height: 640
    });
    win.loadURL('file://' + __dirname + '/index.html');
}

try {
    app.on('ready', createWindow);

    app.on('window-all-closed', () => {
        setTimeout(() => {
            app.quit();
        }, 1000);
    });

    app.on('activate', () => {
        if (win === null) {
            createWindow();
        }
    });
} catch (e) {
    console.error("Error while initializing", e);
}
