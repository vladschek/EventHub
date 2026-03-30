import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { CreateEventDto, EventDto, ListEventsParams, PagedResult } from '../models/event.models';

@Injectable({ providedIn: 'root' })
export class EventApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl.replace(/\/$/, '');

  list(params: ListEventsParams): Observable<PagedResult<EventDto>> {
    let hp = new HttpParams()
      .set('sortBy', params.sortBy)
      .set('sortDescending', String(params.sortDescending))
      .set('page', String(params.page))
      .set('pageSize', String(params.pageSize));

    if (params.userId?.trim())
      hp = hp.set('userId', params.userId.trim());
    if (params.type)
      hp = hp.set('type', params.type);
    if (params.fromDate)
      hp = hp.set('fromDate', params.fromDate);
    if (params.toDate)
      hp = hp.set('toDate', params.toDate);

    return this.http.get<PagedResult<EventDto>>(`${this.baseUrl}/api/events`, { params: hp });
  }

  create(body: CreateEventDto): Observable<EventDto> {
    return this.http.post<EventDto>(`${this.baseUrl}/api/events`, body);
  }
}
