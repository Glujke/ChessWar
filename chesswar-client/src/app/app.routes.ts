import { Routes } from '@angular/router';
import { MenuComponent } from './features/menu/menu.component';
import { GameComponent } from './features/game/game.component';

export const routes: Routes = [
  { path: '', component: MenuComponent },
  { path: 'gamesession/:id', component: GameComponent }
];
