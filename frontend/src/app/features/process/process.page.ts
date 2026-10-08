import { ChangeDetectionStrategy, Component, OnInit, inject, input, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { CommerceApiService } from '../../core/services/commerce-api.service';
import { ProcessCommerceResult, ProcessDateSummary } from '../../core/models/commerce.models';
import { ApiErrorMessage, toApiErrorMessage } from '../../core/utils/http-error';

@Component({
  selector: 'app-process-page',
  imports: [DatePipe, FormsModule, RouterLink],
  templateUrl: './process.page.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProcessPage implements OnInit {
  private readonly api = inject(CommerceApiService);

  readonly processDate = input<string>();

  protected readonly selectedDate = signal('');
  protected readonly dates = signal<ProcessDateSummary[]>([]);
  protected readonly loadingDates = signal(false);
  protected readonly processing = signal(false);
  protected readonly result = signal<ProcessCommerceResult | null>(null);
  protected readonly error = signal<ApiErrorMessage | null>(null);

  ngOnInit(): void {
    this.selectedDate.set(this.processDate() ?? '');
    this.loadDates();
  }

  protected select(date: string): void {
    this.selectedDate.set(date);
    this.result.set(null);
  }

  protected process(): void {
    const date = this.selectedDate();
    if (!date || this.processing()) {
      return;
    }

    this.processing.set(true);
    this.error.set(null);
    this.result.set(null);
    this.api
      .process(date)
      .pipe(finalize(() => this.processing.set(false)))
      .subscribe({
        next: result => {
          this.result.set(result);
          this.loadDates();
        },
        error: error => this.error.set(toApiErrorMessage(error))
      });
  }

  private loadDates(): void {
    this.loadingDates.set(true);
    this.api
      .getProcessDates()
      .pipe(finalize(() => this.loadingDates.set(false)))
      .subscribe({
        next: dates => this.dates.set(dates),
        error: error => this.error.set(toApiErrorMessage(error))
      });
  }
}
