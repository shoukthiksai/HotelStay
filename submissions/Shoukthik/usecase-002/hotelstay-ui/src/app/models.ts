export type RoomType = 'Standard' | 'Deluxe' | 'Suite';
export type CancellationPolicy = 'FreeCancellation' | 'Flexible' | 'NonRefundable';
export type DocumentType = 'Passport' | 'NationalId';

export interface HotelResult {
  provider: string;
  roomType: RoomType;
  nightlyRate: number;
  cancellationPolicy: CancellationPolicy;
  amenities: string[] | null;
  starRating: number | null;
  available: boolean;
}

export interface SearchCriteria {
  destination: string;
  checkIn: string;
  checkOut: string;
  roomType?: RoomType;
}

export interface BookingRequest {
  provider: string;
  roomType: RoomType;
  checkIn: string;
  checkOut: string;
  destination: string;
  passengerName: string;
  documentType: DocumentType;
  documentNumber: string;
}

export interface BookingConfirmation {
  referenceNumber: string;
  provider: string;
  totalPrice: number;
  cancellationPolicy: CancellationPolicy;
}

// Mirrors the server-side registry; home country is the UK.
export const DESTINATIONS: ReadonlyArray<{ name: string; international: boolean }> = [
  { name: 'London', international: false },
  { name: 'Manchester', international: false },
  { name: 'Paris', international: true },
  { name: 'New York', international: true },
  { name: 'Tokyo', international: true },
  { name: 'Dubai', international: true },
  { name: 'Sydney', international: true },
];

export const ROOM_TYPES: readonly RoomType[] = ['Standard', 'Deluxe', 'Suite'];

export const CANCELLATION_LABELS: Record<CancellationPolicy, string> = {
  FreeCancellation: 'Free cancellation',
  Flexible: 'Flexible',
  NonRefundable: 'Non-refundable',
};

export function isInternational(destination: string): boolean {
  return DESTINATIONS.some((d) => d.name === destination && d.international);
}

export function nightsBetween(checkIn: string, checkOut: string): number {
  const ms = new Date(checkOut).getTime() - new Date(checkIn).getTime();
  return Math.round(ms / 86_400_000);
}
