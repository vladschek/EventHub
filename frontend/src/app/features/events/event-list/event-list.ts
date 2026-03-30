import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectionStrategy,
  Component,
  Injector,
  computed,
  effect,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed, toObservable, toSignal } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged, throttleTime } from 'rxjs';
import type { EventDto, EventSortField, EventType, PagedResult } from '../../../core/models/event.models';
import { EventApiService } from '../../../core/services/event-api.service';
import { EventsRealtimeService } from '../../../core/services/events-realtime.service';

@Component({
  selector: 'app-event-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './event-list.html',
  styleUrl: './event-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EventListComponent {
  private readonly injector = inject(Injector);
  private readonly api = inject(EventApiService);
  private readonly realtime = inject(EventsRealtimeService);

  readonly userIdInput = signal('');
  readonly debouncedUserId = toSignal(
    toObservable(this.userIdInput, { injector: this.injector }).pipe(
      debounceTime(400),
      distinctUntilChanged(),
    ),
    { initialValue: '', injector: this.injector },
  );

  readonly typeFilter = signal<EventType | ''>('');
  readonly fromDate = signal('');
  readonly toDate = signal('');
  readonly sortBy = signal<EventSortField>('CreatedAt');
  readonly sortDescending = signal(true);
  readonly page = signal(1);
  readonly pageSize = signal(20);

  readonly loading = signal(false);
  readonly result = signal<PagedResult<EventDto> | null>(null);
  readonly errorMessage = signal<string | null>(null);

  readonly skeletonRows = [0, 1, 2, 3, 4];
  readonly eventTypes: EventType[] = ['PageView', 'Click', 'Purchase'];

  readonly realtimeConnected = this.realtime.connected;

  /** Bumped when the hub raises `EventCreated` so the list reloads for the active filters. */
  readonly refreshTick = signal(0);

  readonly showEmpty = computed(() => {
    const r = this.result();
    return !this.loading() && r !== null && r.items.length === 0;
  });

  constructor() {
    this.realtime.eventCreated$
      .pipe(throttleTime(500, undefined, { leading: true, trailing: true }), takeUntilDestroyed())
      .subscribe(() => {
        this.refreshTick.update((n) => n + 1);
      });

    effect((onCleanup) => {
      this.refreshTick();
      const typeVal = this.typeFilter();
      const params = {
        userId: this.debouncedUserId() || undefined,
        type: typeVal ? typeVal : undefined,
        fromDate: this.toIsoOrUndefined(this.fromDate()),
        toDate: this.toIsoOrUndefined(this.toDate()),
        sortBy: this.sortBy(),
        sortDescending: this.sortDescending(),
        page: this.page(),
        pageSize: this.pageSize(),
      };

      this.loading.set(true);
      this.errorMessage.set(null);

      const sub = this.api.list(params).subscribe({
        next: (r) => {
          this.result.set(r);
          this.loading.set(false);
        },
        error: (err: HttpErrorResponse) => {
          const msg =
            typeof err.error === 'object' && err.error && 'title' in err.error
              ? String((err.error as { title?: string }).title)
              : err.message;
          this.errorMessage.set(msg || 'Could not load events.');
          this.result.set(null);
          this.loading.set(false);
        },
      });

      onCleanup(() => sub.unsubscribe());
    }, { allowSignalWrites: true });
  }

  onUserIdInput(value: unknown): void {
    this.userIdInput.set(value == null ? '' : String(value));
    this.page.set(1);
  }

  onTypeChange(value: unknown): void {
    const v = value == null ? '' : String(value);
    this.typeFilter.set(v as EventType | '');
    this.page.set(1);
  }

  onFromDateChange(value: unknown): void {
    this.fromDate.set(value == null ? '' : String(value));
    this.page.set(1);
  }

  onToDateChange(value: unknown): void {
    this.toDate.set(value == null ? '' : String(value));
    this.page.set(1);
  }

  onSortByChange(value: unknown): void {
    const v = value == null ? '' : String(value);
    this.sortBy.set(v as EventSortField);
    this.page.set(1);
  }

  onSortDirChange(value: unknown): void {
    const v = value == null ? '' : String(value);
    this.sortDescending.set(v === 'desc');
    this.page.set(1);
  }

  onPageSizeChange(value: unknown): void {
    const n = Number(value == null ? '' : String(value));
    if (!Number.isNaN(n) && n > 0) {
      this.pageSize.set(n);
      this.page.set(1);
    }
  }

  prevPage(): void {
    const p = this.page();
    if (p > 1) this.page.set(p - 1);
  }

  nextPage(): void {
    const r = this.result();
    if (!r) return;
    if (this.page() < r.totalPages) this.page.set(this.page() + 1);
  }

  formatTime(iso: string): string {
    const d = new Date(iso);
    return Number.isNaN(d.getTime()) ? iso : d.toLocaleString();
  }

  private toIsoOrUndefined(localDateTime: string): string | undefined {
    if (!localDateTime?.trim()) return undefined;
    const d = new Date(localDateTime);
    return Number.isNaN(d.getTime()) ? undefined : d.toISOString();
  }
}
