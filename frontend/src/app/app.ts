import { Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { EventsRealtimeService } from './core/services/events-realtime.service';

@Component({
  selector: 'app-root',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('EventHub');
  private readonly realtime = inject(EventsRealtimeService);

  constructor() {
    void this.realtime.start();
  }
}
