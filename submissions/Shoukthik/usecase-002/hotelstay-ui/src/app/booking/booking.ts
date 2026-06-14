import { Component, inject, signal } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { HotelService } from '../hotel.service';
import { BookingFlow } from '../booking-flow';
import { CANCELLATION_LABELS, DocumentType, isInternational } from '../models';

@Component({
  selector: 'app-booking',
  imports: [ReactiveFormsModule],
  templateUrl: './booking.html',
  styleUrl: './booking.scss',
})
export class Booking {
  private hotels = inject(HotelService);
  private flow = inject(BookingFlow);
  private router = inject(Router);

  readonly selection = this.flow.selection();
  readonly cancellationLabels = CANCELLATION_LABELS;
  readonly submitting = signal(false);
  readonly error = signal<string | null>(null);

  readonly form = new FormGroup({
    passengerName: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    documentType: new FormControl<DocumentType>('Passport', { nonNullable: true, validators: [Validators.required] }),
    documentNumber: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
  }, { validators: (group: AbstractControl) => this.documentMatchesDestination(group) });

  constructor() {
    if (!this.selection) {
      this.router.navigate(['/']);
    }
  }

  // Mirrors the server guard: a national ID is not accepted for international destinations.
  private documentMatchesDestination(group: AbstractControl): ValidationErrors | null {
    const documentType = group.get('documentType')?.value;
    const international = !!this.selection && isInternational(this.selection.destination);
    return international && documentType !== 'Passport' ? { documentMismatch: true } : null;
  }

  submit(): void {
    if (!this.selection || this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { offer, destination, checkIn, checkOut } = this.selection;
    const { passengerName, documentType, documentNumber } = this.form.getRawValue();
    this.submitting.set(true);
    this.error.set(null);

    this.hotels.book({
      provider: offer.provider,
      roomType: offer.roomType,
      checkIn, checkOut, destination,
      passengerName, documentType, documentNumber,
    }).subscribe({
      next: (confirmation) => {
        this.flow.confirmation.set(confirmation);
        this.router.navigate(['/confirmation']);
      },
      error: (err) => {
        this.error.set(err?.error?.message ?? 'Booking failed. Please try again.');
        this.submitting.set(false);
      },
    });
  }

  back(): void {
    this.router.navigate(['/']);
  }
}
