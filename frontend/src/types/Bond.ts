export interface Bond {
  instrumentId: string;
  name: string;
  issuer: string;
  currency: string;
  sector: string;
  maturityDate: string;
  couponRate: number;
  faceValue: number;
  bid: number;
  ask: number;
  spread: number;
  yield: number;
  openingPrice: number;
  closingPrice: number;
  lastPrice: number;
  volume: number;
  updateTime: string;
  rating: string;
  isin: string;
  cusip: string;
  tierId: string;
  isGroup?: boolean;
  key?: string;
  childCount?: number;
}

export interface GroupRow {
  key: string;
  isGroup: true;
  childCount: number;
}

export interface ServerSideRequest {
  startRow: number;
  endRow: number;
  sortModel: SortModel[];
  filterModel: any; // Can be FilterModel or AdvancedFilterModel
  groupKeys: string[];
  groupingCols: string[];
}

export interface SortModel {
  colId: string;
  sort: 'asc' | 'desc';
}

export interface FilterModel {
  filterType: string;
  filter?: any;
  values?: string[];
}

export interface ServerSideResponse {
  rows: (Bond | GroupRow)[];
  lastRow?: number;
}

export interface SubscriptionFilter {
  currencies: string[];
  sectors: string[];
}