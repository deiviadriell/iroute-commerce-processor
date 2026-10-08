import { ChangeDetectionStrategy, Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { CommerceApiService } from '../../core/services/commerce-api.service';
import { QuarantinedCommerce } from '../../core/models/commerce.models';
import { ApiErrorMessage, toApiErrorMessage } from '../../core/utils/http-error';

@Component({
  selector: 'app-quarantine-page',
  imports: [DatePipe, FormsModule],
  templateUrl: './quarantine.page.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class QuarantinePage implements OnInit {
  private readonly api = inject(CommerceApiService);

  readonly processDate = input<string>();

  protected readonly dateFilter = signal('');
  protected readonly search = signal('');
  protected readonly records = signal<QuarantinedCommerce[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<ApiErrorMessage | null>(null);

  protected readonly filtered = computed(() => {
    const term = this.search().trim().toLowerCase();
    if (!term) {
      return this.records();
    }
    return this.records().filter(r =>
      [r.codComRed, r.nomComRed, r.numDoc, r.motivo].some(v => v?.toLowerCase().includes(term))
    );
  });

  ngOnInit(): void {
    this.dateFilter.set(this.processDate() ?? '');
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api
      .getQuarantine(this.dateFilter() || null)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: records => this.records.set(records),
        error: error => this.error.set(toApiErrorMessage(error))
      });
  }

  protected clearFilters(): void {
    this.dateFilter.set('');
    this.search.set('');
    this.load();
  }

  protected reasons(record: QuarantinedCommerce): string[] {
    return record.motivo.split(';').map(m => m.trim()).filter(Boolean);
  }
}
