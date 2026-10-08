import { ChangeDetectionStrategy, Component, ElementRef, computed, inject, signal, viewChild } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { CommerceApiService } from '../../core/services/commerce-api.service';
import { CsvPreview, UploadCommerceFileResult } from '../../core/models/commerce.models';
import { COMMERCE_COLUMNS, parseCommerceCsv, validateCommerceFileName } from '../../core/utils/commerce-csv';
import { ApiErrorMessage, toApiErrorMessage } from '../../core/utils/http-error';

const PAGE_SIZE = 20;

@Component({
  selector: 'app-upload-page',
  imports: [DatePipe, RouterLink],
  templateUrl: './upload.page.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UploadPage {
  private readonly api = inject(CommerceApiService);
  private readonly fileInput = viewChild.required<ElementRef<HTMLInputElement>>('fileInput');

  protected readonly columns = COMMERCE_COLUMNS;
  protected readonly file = signal<File | null>(null);
  protected readonly preview = signal<CsvPreview | null>(null);
  protected readonly fileError = signal<string | null>(null);
  protected readonly apiError = signal<ApiErrorMessage | null>(null);
  protected readonly result = signal<UploadCommerceFileResult | null>(null);
  protected readonly uploading = signal(false);
  protected readonly page = signal(0);

  protected readonly rowsWithIssues = computed(() => this.preview()?.rows.filter(r => r.issues.length > 0).length ?? 0);
  protected readonly totalPages = computed(() => Math.max(1, Math.ceil((this.preview()?.rows.length ?? 0) / PAGE_SIZE)));
  protected readonly pagedRows = computed(() => {
    const start = this.page() * PAGE_SIZE;
    return this.preview()?.rows.slice(start, start + PAGE_SIZE) ?? [];
  });
  protected readonly canUpload = computed(() => {
    const preview = this.preview();
    return !!preview && !this.fileError() && preview.rows.length > 0 && preview.missingColumns.length === 0 && !this.uploading();
  });

  protected async onFileSelected(event: Event): Promise<void> {
    const file = (event.target as HTMLInputElement).files?.[0];
    this.clear();
    if (!file) {
      return;
    }

    this.file.set(file);

    if (file.size === 0) {
      this.fileError.set('El archivo se encuentra vacío.');
      return;
    }

    this.fileError.set(validateCommerceFileName(file.name));
    const preview = parseCommerceCsv(await file.text());
    this.preview.set(preview);

    if (preview.rows.length === 0) {
      this.fileError.set('El archivo no contiene registros.');
    }
  }

  protected changePage(delta: number): void {
    this.page.update(p => Math.min(Math.max(p + delta, 0), this.totalPages() - 1));
  }

  protected upload(): void {
    const file = this.file();
    if (!file || !this.canUpload()) {
      return;
    }

    this.uploading.set(true);
    this.apiError.set(null);
    this.api
      .upload(file)
      .pipe(finalize(() => this.uploading.set(false)))
      .subscribe({
        next: result => {
          this.cancel();
          this.result.set(result);
        },
        error: error => this.apiError.set(toApiErrorMessage(error))
      });
  }

  protected cancel(): void {
    this.fileInput().nativeElement.value = '';
    this.clear();
  }

  private clear(): void {
    this.file.set(null);
    this.preview.set(null);
    this.fileError.set(null);
    this.apiError.set(null);
    this.result.set(null);
    this.page.set(0);
  }
}
