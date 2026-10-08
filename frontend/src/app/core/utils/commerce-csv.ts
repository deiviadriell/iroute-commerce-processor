import { CsvPreview, CsvPreviewRow } from '../models/commerce.models';

export const COMMERCE_COLUMNS = [
  'pc_processdate',
  'pc_codcomred',
  'pc_nomcomred',
  'pc_tipdoc',
  'pc_numdoc',
  'pc_ciudad',
  'pc_direccion',
  'pc_telefono',
  'pc_email'
] as const;

const FILE_NAME_PATTERN = /^commerce_(\d{2})(\d{2})(\d{4})\.csv$/i;
const DELIMITERS = [',', ';', '|', '\t'];

export function validateCommerceFileName(name: string): string | null {
  const match = FILE_NAME_PATTERN.exec(name);
  if (!match) {
    return 'El archivo debe llamarse commerce_DDMMYYYY.csv';
  }

  const [, dd, mm, yyyy] = match.map(Number);
  const date = new Date(yyyy, mm - 1, dd);
  const valid = date.getFullYear() === yyyy && date.getMonth() === mm - 1 && date.getDate() === dd;
  return valid ? null : 'La fecha del nombre del archivo no es válida';
}

export function parseCommerceCsv(content: string): CsvPreview {
  const lines = content.replace(/^﻿/, '').split(/\r?\n/);
  const headerIndex = lines.findIndex(l => l.trim() !== '');
  if (headerIndex < 0) {
    return { columns: [], rows: [], missingColumns: [] };
  }

  const delimiter = detectDelimiter(lines[headerIndex]);
  const columns = splitLine(lines[headerIndex], delimiter).map(c => c.trim().toLowerCase());
  const missingColumns = COMMERCE_COLUMNS.filter(c => !columns.includes(c));
  const rows: CsvPreviewRow[] = [];

  for (let i = headerIndex + 1; i < lines.length; i++) {
    if (lines[i].trim() === '') {
      continue;
    }

    const cells = splitLine(lines[i], delimiter);
    const values = Object.fromEntries(columns.map((c, idx) => [c, (cells[idx] ?? '').trim()]));
    rows.push({ line: i + 1, values, issues: detectIssues(values) });
  }

  return { columns, rows, missingColumns };
}

function detectIssues(values: Record<string, string>): string[] {
  const issues: string[] = [];
  const numDoc = values['pc_numdoc'] ?? '';

  if (!values['pc_processdate']) {
    issues.push('Fecha de proceso vacía');
  }
  if (!values['pc_nomcomred']) {
    issues.push('Nombre del comercio vacío');
  }
  if (!numDoc) {
    issues.push('Número de documento vacío');
  } else {
    if (/\p{L}/u.test(numDoc)) {
      issues.push('Número de documento con letras');
    }
    if (/[^\p{L}0-9]/u.test(numDoc)) {
      issues.push('Número de documento con caracteres especiales');
    }
  }

  return issues;
}

function detectDelimiter(header: string): string {
  return DELIMITERS.reduce((best, d) => (count(header, d) > count(header, best) ? d : best), DELIMITERS[0]);
}

function count(text: string, char: string): number {
  return text.split(char).length - 1;
}

function splitLine(line: string, delimiter: string): string[] {
  const values: string[] = [];
  let current = '';
  let inQuotes = false;

  for (let i = 0; i < line.length; i++) {
    const c = line[i];
    if (inQuotes) {
      if (c === '"' && line[i + 1] === '"') {
        current += '"';
        i++;
      } else if (c === '"') {
        inQuotes = false;
      } else {
        current += c;
      }
    } else if (c === '"') {
      inQuotes = true;
    } else if (c === delimiter) {
      values.push(current);
      current = '';
    } else {
      current += c;
    }
  }

  values.push(current);
  return values;
}
