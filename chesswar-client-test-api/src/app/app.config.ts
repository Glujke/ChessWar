import { ApplicationConfig, importProvidersFrom, provideBrowserGlobalErrorListeners, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { HttpClientModule, provideHttpClient, withInterceptors } from '@angular/common/http';
import { API_BASE_URL } from './core/config/tokens';
import { ApiClientService, IApiClientService } from './core/api/api-client.service';
import { environment } from '../environments/environment';
import { withAcceptJsonInterceptor, withBaseUrlInterceptor, withLoggingInterceptor, withProblemDetailsInterceptor } from './core/api/interceptors';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    importProvidersFrom(HttpClientModule),
    provideHttpClient(withInterceptors([withAcceptJsonInterceptor, withBaseUrlInterceptor, withProblemDetailsInterceptor, withLoggingInterceptor])),
    // В dev через proxy: оставляем пустым; в prod используем /api
    { provide: API_BASE_URL, useValue: environment.apiBaseUrl },
    { provide: IApiClientService, useExisting: ApiClientService }
  ]
};
