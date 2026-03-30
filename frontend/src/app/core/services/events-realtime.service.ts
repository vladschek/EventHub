import { Injectable, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { EventDto } from '../models/event.models';

@Injectable({ providedIn: 'root' })
export class EventsRealtimeService {
  private hub?: signalR.HubConnection;
  private readonly createdSubject = new Subject<EventDto>();

  readonly eventCreated$ = this.createdSubject.asObservable();
  /** `true` while the hub is connected and receiving live events. */
  readonly connected = signal(false);

  async start(): Promise<void> {
    const base = environment.apiUrl?.replace(/\/$/, '');
    if (!base) return;

    if (this.hub?.state === signalR.HubConnectionState.Connected) return;

    this.hub = new signalR.HubConnectionBuilder()
      .withUrl(`${base}/hubs/events`, {
        withCredentials: true,
        transport:
          signalR.HttpTransportType.WebSockets |
          signalR.HttpTransportType.ServerSentEvents |
          signalR.HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect()
      .build();

    this.hub.on('EventCreated', (payload: EventDto) => {
      this.createdSubject.next(payload);
    });

    this.hub.onreconnecting(() => this.connected.set(false));
    this.hub.onreconnected(() => this.connected.set(true));
    this.hub.onclose(() => this.connected.set(false));

    try {
      await this.hub.start();
      this.connected.set(true);
    } catch (e) {
      console.warn('[EventHub] SignalR could not start — is the API running?', e);
    }
  }
}
