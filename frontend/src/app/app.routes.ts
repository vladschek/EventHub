import { Routes } from '@angular/router';
import { EventFormComponent } from './features/events/event-form/event-form';
import { EventListComponent } from './features/events/event-list/event-list';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'events' },
  { path: 'events', component: EventListComponent },
  { path: 'events/new', component: EventFormComponent },
];
