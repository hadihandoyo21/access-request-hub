import { Routes } from '@angular/router';
import { RequestListComponent } from './components/request-list/request-list.component';
import { CreateRequestComponent } from './components/create-request/create-request.component';

export const routes: Routes = [
  { path: '', redirectTo: '/requests', pathMatch: 'full' },
  { path: 'requests', component: RequestListComponent },
  { path: 'create', component: CreateRequestComponent }
];