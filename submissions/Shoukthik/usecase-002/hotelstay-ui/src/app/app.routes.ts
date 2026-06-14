import { Routes } from '@angular/router';
import { Search } from './search/search';
import { Booking } from './booking/booking';
import { Confirmation } from './confirmation/confirmation';

export const routes: Routes = [
  { path: '', component: Search },
  { path: 'book', component: Booking },
  { path: 'confirmation', component: Confirmation },
  { path: '**', redirectTo: '' },
];
