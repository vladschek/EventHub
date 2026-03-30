import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectionStrategy,
  Component,
  Injector,
  computed,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { startWith } from 'rxjs';
import type { EventType } from '../../../core/models/event.models';

const EVENT_TYPES: EventType[] = ['PageView', 'Click', 'Purchase'];
import { EventApiService } from '../../../core/services/event-api.service';

const descMax = 2000;
const userIdMax = 256;

@Component({
  selector: 'app-event-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './event-form.html',
  styleUrl: './event-form.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EventFormComponent {
  private readonly injector = inject(Injector);
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(EventApiService);

  readonly descMax = descMax;
  readonly userIdMax = userIdMax;
  readonly eventTypes = EVENT_TYPES;

  readonly form = this.fb.nonNullable.group({
    userId: ['', [Validators.required, Validators.maxLength(userIdMax)]],
    type: ['PageView' as EventType, Validators.required],
    description: ['', [Validators.required, Validators.maxLength(descMax)]],
  });

  private readonly formState = toSignal(this.form.valueChanges.pipe(startWith(this.form.value)), {
    initialValue: this.form.value,
    injector: this.injector,
  });

  readonly descriptionLength = computed(
    () => (this.formState()?.description ?? '').length,
  );

  readonly submitting = signal(false);
  readonly submitError = signal<string | null>(null);
  readonly submitSuccess = signal(false);

  readonly descRemaining = computed(() => descMax - this.descriptionLength());

  constructor() {
    this.form.valueChanges.pipe(takeUntilDestroyed()).subscribe(() => this.submitSuccess.set(false));
  }

  onSubmit(): void {
    this.submitError.set(null);
    this.submitSuccess.set(false);
    this.clearServerFieldErrors();

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    const v = this.form.getRawValue();
    const body = {
      userId: v.userId.trim(),
      type: v.type,
      description: v.description.trim(),
    };

    this.api.create(body).subscribe({
      next: () => {
        this.submitting.set(false);
        this.form.reset({
          userId: '',
          type: 'PageView',
          description: '',
        });
        this.form.markAsPristine();
        this.form.markAsUntouched();
        queueMicrotask(() => this.submitSuccess.set(true));
      },
      error: (err: HttpErrorResponse) => {
        this.submitting.set(false);
        const bodyErr = err.error as { title?: string; errors?: Record<string, string[]> } | undefined;
        if (err.status === 400 && bodyErr?.errors) {
          for (const [key, messages] of Object.entries(bodyErr.errors)) {
            const control = this.form.get(key);
            const first = Array.isArray(messages) ? messages[0] : undefined;
            if (control && first) {
              control.setErrors({ server: first });
              control.markAsTouched();
            }
          }
        }
        this.submitError.set(
          bodyErr?.title ?? err.message ?? 'Could not create event. Try again.',
        );
      },
    });
  }

  controlError(key: 'userId' | 'type' | 'description'): string | null {
    const c = this.form.get(key);
    if (!c || !c.touched) return null;
    if (c.hasError('required')) return 'This field is required.';
    if (c.hasError('maxlength')) {
      const max = c.errors?.['maxlength']?.requiredLength;
      return typeof max === 'number' ? `Maximum length is ${max} characters.` : 'Value is too long.';
    }
    if (c.hasError('server')) return String(c.errors?.['server']);
    return null;
  }

  private clearServerFieldErrors(): void {
    for (const name of ['userId', 'type', 'description'] as const) {
      const c = this.form.get(name);
      if (!c?.errors?.['server']) continue;
      const { server: _s, ...rest } = c.errors;
      c.setErrors(Object.keys(rest).length ? rest : null);
    }
  }
}
