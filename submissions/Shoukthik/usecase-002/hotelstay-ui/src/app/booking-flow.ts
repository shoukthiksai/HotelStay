import { Injectable, signal } from '@angular/core';
import { BookingConfirmation, HotelResult } from './models';

export interface Selection {
  offer: HotelResult;
  destination: string;
  checkIn: string;
  checkOut: string;
  nights: number;
  total: number;
}

/// Carries the chosen room from search → booking → confirmation. Search-driven, so it
/// resets naturally when a new search begins.
@Injectable({ providedIn: 'root' })
export class BookingFlow {
  readonly selection = signal<Selection | null>(null);
  readonly confirmation = signal<BookingConfirmation | null>(null);
}
