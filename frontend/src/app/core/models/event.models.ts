export type EventType = 'PageView' | 'Click' | 'Purchase';

export type EventSortField = 'CreatedAt' | 'UserId' | 'Type';

export interface EventDto {
  id: string;
  userId: string;
  type: EventType;
  description: string;
  createdAt: string;
}

/** Body for POST /api/events (camelCase JSON). */
export interface CreateEventDto {
  userId: string;
  type: EventType;
  description: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ListEventsParams {
  userId?: string;
  type?: EventType;
  fromDate?: string;
  toDate?: string;
  sortBy: EventSortField;
  sortDescending: boolean;
  page: number;
  pageSize: number;
}
