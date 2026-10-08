export interface Commerce {
  processDate: string;
  codComRed: string | null;
  nomComRed: string | null;
  tipDoc: string | null;
  numDoc: string | null;
  ciudad: string | null;
  direccion: string | null;
  telefono: string | null;
  email: string | null;
}

export interface QuarantinedCommerce extends Commerce {
  id: number;
  commerceId: number;
  motivo: string;
  quarantinedAt: string;
}

export interface ProcessDateSummary {
  processDate: string;
  total: number;
}

export interface UploadCommerceFileResult {
  fileName: string;
  inserted: number;
  processDates: string[];
}

export interface ProcessCommerceResult {
  processDate: string;
  quarantined: number;
}

export interface CsvPreviewRow {
  line: number;
  values: Record<string, string>;
  issues: string[];
}

export interface CsvPreview {
  columns: string[];
  rows: CsvPreviewRow[];
  missingColumns: string[];
}
