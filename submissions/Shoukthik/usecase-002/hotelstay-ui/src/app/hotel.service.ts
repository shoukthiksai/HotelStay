import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BookingConfirmation, BookingRequest, HotelResult, SearchCriteria } from './models';

const API_BASE = 'http://localhost:5168';

@Injectable({ providedIn: 'root' })
export class HotelService {
  private http = inject(HttpClient);

  search(criteria: SearchCriteria): Observable<HotelResult[]> {
    let params = new HttpParams()
      .set('destination', criteria.destination)
      .set('checkIn', criteria.checkIn)
      .set('checkOut', criteria.checkOut);
    if (criteria.roomType) {
      params = params.set('roomType', criteria.roomType);
    }
    return this.http.get<HotelResult[]>(`${API_BASE}/hotels/search`, { params });
  }

  book(request: BookingRequest): Observable<BookingConfirmation> {
    return this.http.post<BookingConfirmation>(`${API_BASE}/hotels/book`, request);
  }
}
