import { Component, computed, inject, signal } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { HotelService } from '../hotel.service';
import { BookingFlow } from '../booking-flow';
import {
  CANCELLATION_LABELS, DESTINATIONS, HotelResult, ROOM_TYPES, RoomType, nightsBetween,
} from '../models';

const checkoutAfterCheckin = (group: AbstractControl): ValidationErrors | null => {
  const checkIn = group.get('checkIn')?.value;
  const checkOut = group.get('checkOut')?.value;
  return checkIn && checkOut && checkOut <= checkIn ? { dateOrder: true } : null;
};

type SortDirection = 'asc' | 'desc';
interface SearchContext { destination: string; checkIn: string; checkOut: string; nights: number; }

@Component({
  selector: 'app-search',
  imports: [ReactiveFormsModule],
  templateUrl: './search.html',
  styleUrl: './search.scss',
})
export class Search {
  private hotels = inject(HotelService);
  private flow = inject(BookingFlow);
  private router = inject(Router);

  readonly destinations = DESTINATIONS;
  readonly roomTypes = ROOM_TYPES;
  readonly cancellationLabels = CANCELLATION_LABELS;

  readonly form = new FormGroup({
    destination: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    checkIn: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    checkOut: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    roomType: new FormControl<RoomType | ''>('', { nonNullable: true }),
  }, { validators: checkoutAfterCheckin });

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly context = signal<SearchContext | null>(null);
  private readonly results = signal<HotelResult[]>([]);
  readonly sort = signal<SortDirection | null>(null);

  readonly sortedResults = computed(() => {
    const list = [...this.results()];
    const direction = this.sort();
    if (direction) {
      list.sort((a, b) => direction === 'asc' ? a.nightlyRate - b.nightlyRate : b.nightlyRate - a.nightlyRate);
    }
    return list;
  });

  total(offer: HotelResult): number {
    return (this.context()?.nights ?? 0) * offer.nightlyRate;
  }

  toggleSort(): void {
    this.sort.update((d) => (d === 'asc' ? 'desc' : 'asc'));
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { destination, checkIn, checkOut, roomType } = this.form.getRawValue();
    this.loading.set(true);
    this.error.set(null);

    this.hotels.search({ destination, checkIn, checkOut, roomType: roomType || undefined }).subscribe({
      next: (results) => {
        this.results.set(results);
        this.context.set({ destination, checkIn, checkOut, nights: nightsBetween(checkIn, checkOut) });
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err?.error?.message ?? 'Search failed. Is the API running?');
        this.loading.set(false);
      },
    });
  }

  book(offer: HotelResult): void {
    const context = this.context()!;
    this.flow.selection.set({
      offer,
      destination: context.destination,
      checkIn: context.checkIn,
      checkOut: context.checkOut,
      nights: context.nights,
      total: this.total(offer),
    });
    this.router.navigate(['/book']);
  }
}
