import { HttpErrorResponse, HttpEvent, HttpHandlerFn, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, Observable, tap, throwError } from 'rxjs';
import { ProblemDetails } from './problem-details';
import { API_BASE_URL } from '../config/tokens';

export const withBaseUrlInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> => {
  const baseUrl = inject(API_BASE_URL);
  const url = req.url.startsWith('http') ? req.url : `${baseUrl}${req.url}`;
  const cloned = req.clone({ url, setHeaders: { 'X-Requested-With': 'XMLHttpRequest' } });
  return next(cloned);
};

export const withAcceptJsonInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> => {
  const accepts = 'application/json, application/problem+json;q=0.9, */*;q=0.8';
  const hasBody = req.body !== undefined && req.method !== 'GET' && req.method !== 'HEAD';
  const headers: Record<string, string> = { Accept: accepts };
  if (hasBody) {
    headers['Content-Type'] = 'application/json; charset=utf-8';
  }
  return next(req.clone({ setHeaders: headers }));
};

export const withLoggingInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> => {
  const started = performance.now();
  return next(req).pipe(
    tap({
      next: () => void 0,
      error: () => void 0,
      complete: () => {
        const ms = Math.round(performance.now() - started);
        // eslint-disable-next-line no-console
        console.debug(`[HTTP] ${req.method} ${req.url} - ${ms}ms`);
      }
    })
  );
};

export const withProblemDetailsInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> => {
  return next(req).pipe(
    catchError((err: unknown) => {
      if (err instanceof HttpErrorResponse) {
        const contentType = err.headers?.get('Content-Type') ?? '';
        const isStringBody = typeof err.error === 'string' && err.error.trim().length > 0;
        const maybeProblem: ProblemDetails | undefined = typeof err.error === 'object' ? (err.error as ProblemDetails) : undefined;
        const isProblem = contentType.includes('application/problem+json') || (maybeProblem && (maybeProblem.title !== undefined || maybeProblem.type !== undefined)) || isStringBody;
        if (isProblem) {
          const problem = isStringBody
            ? { title: (err.error as string), status: err.status }
            : (maybeProblem ?? { title: err.statusText, status: err.status });
          const enriched = {
            message: problem.title ?? 'Request failed',
            status: problem.status ?? err.status,
            problem
          } as const;
          return throwError(() => enriched);
        }
      }
      return throwError(() => err);
    })
  );
};


