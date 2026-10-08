import { HttpErrorResponse } from '@angular/common/http';

export interface ApiErrorMessage {
  title: string;
  details: string[];
}

export function toApiErrorMessage(error: unknown): ApiErrorMessage {
  if (!(error instanceof HttpErrorResponse)) {
    return { title: 'Ocurrió un error inesperado.', details: [] };
  }

  if (error.status === 0) {
    return { title: 'No fue posible conectar con el servidor.', details: [] };
  }

  const problem = error.error ?? {};
  const errors = problem.errors;
  const details: string[] = Array.isArray(errors)
    ? errors
    : errors && typeof errors === 'object'
      ? Object.values(errors as Record<string, string[]>).flat()
      : [];

  return { title: problem.title ?? `Error ${error.status}`, details };
}
