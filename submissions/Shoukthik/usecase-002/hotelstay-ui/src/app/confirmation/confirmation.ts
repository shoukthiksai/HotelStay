import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { BookingFlow } from '../booking-flow';
import { CANCELLATION_LABELS } from '../models';

@Component({
  selector: 'app-confirmation',
  templateUrl: './confirmation.html',
  styleUrl: './confirmation.scss',
})
export class Confirmation {
  private flow = inject(BookingFlow);
  private router = inject(Router);

  readonly confirmation = this.flow.confirmation();
  readonly cancellationLabels = CANCELLATION_LABELS;

  constructor() {
    if (!this.confirmation) {
      this.router.navigate(['/']);
    }
  }

  newSearch(): void {
    this.flow.selection.set(null);
    this.flow.confirmation.set(null);
    this.router.navigate(['/']);
  }
}
