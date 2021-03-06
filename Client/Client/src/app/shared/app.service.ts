﻿import { Injectable, Inject, forwardRef } from '@angular/core';
import { remote } from 'electron';
import { spawn, spawnSync, ChildProcess } from 'child_process';
import { MdDialog } from '@angular/material';

import { Subject } from 'rxjs/Subject';
import { ToastsManager } from 'ng2-toastr/ng2-toastr';

import { TrackingService, ConfigurationService, ReputationDetailsDialogComponent, ReputationDetailsDialogData } from './';
import { Reputation } from '../app.models';

@Injectable()
export class AppService {
    public stringVersion = "1.4.0";
    public lobbiesPageOpened = false;
    public sessionRunning = false;
    private nethookActive = false;
    private backendProcess: ChildProcess;
    public Events = {
        gameStarted: new Subject<any>()
    }

    constructor(private toastsManager: ToastsManager, private trackingService: TrackingService, private configurationService: ConfigurationService, private dialogController: MdDialog) {
        this.startBackend();
        window.onbeforeunload = (e) => {
            if (this.nethookActive) {
                this.stopNethook(() => { });
            }
            if (this.backendProcess) {
                this.backendProcess.kill();
            }
        }
    }

    public toastInfo(message: string) {
        this.toastsManager.info(message);
    }

    public toastSuccess(message: string) {
        this.toastsManager.success(message);
    }

    public toastError(message: string) {
        this.toastsManager.error(message);
        this.trackingService.logException(message);
    }

    public startNethook(done: (success) => void) {
        try {
            spawn(this.configurationService.configuration.cmdPath, ["/k", "rundll32", "NetHook2.dll,Inject"], {
                cwd: this.getWorkingDir()
            });
            this.nethookActive = true;
            done(true);
        } catch (e) {
            this.toastError("Failed to start NetHook process.");
            console.error("Failed to start NetHook process", e);
            done(false);
        }
    }

    public stopNethook(done: (success) => void) {
        try {
            spawnSync(this.configurationService.configuration.cmdPath, ["/k", "rundll32", "NetHook2.dll,Eject"], {
                cwd: this.getWorkingDir()
            });
            this.nethookActive = false;
            done(true);
        } catch (e) {
            this.toastError("Failed to stop NetHook process.");
            console.error("Failed to stop NetHook process", e);
            done(false);
        }
    }

    //To avoid circular module imports then opening a reputation details dialog is performed here
    public openReputationDetailsDialog(uiSource: string, reputation: Reputation, reputationDeleted: () => void) {
        this.dialogController.open(ReputationDetailsDialogComponent, {
            data: <ReputationDetailsDialogData>{
                reputation: reputation,
                reputationDeleted: reputationDeleted
            },
            width: window.innerWidth * 0.75 + "px"
        });
        this.trackingService.sendEvent(uiSource, "ReputationTypeDetailsOpened");
    }

    private startBackend() {
        try {
            this.backendProcess = spawn(this.getWorkingDir() + "\\Backend\\Backend.exe", [], {
                cwd: this.getWorkingDir() + "\\Backend\\"
            });
        } catch (e) {
            this.toastError("Failed to start backend process.");
            console.error("Failed to start backend process", e);
        }
    }

    private getWorkingDir() {
        return remote.app.getAppPath().replace("\\resources\\app.asar", "");
    }
}
