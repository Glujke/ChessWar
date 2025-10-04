import { Routes } from '@angular/router';
import { ConfigListComponent } from './features/config-list/config-list.component';
import { ConfigEditorComponent } from './features/config-editor/config-editor.component';

export const routes: Routes = [
  { path: '', component: ConfigListComponent },
  { path: 'config/:id', component: ConfigEditorComponent },
  { path: 'config/:id/edit', component: ConfigEditorComponent },
  { path: '**', redirectTo: '' }
];
