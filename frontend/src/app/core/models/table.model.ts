export enum TableStatus {
  Available = 1,
  Occupied = 2,
  Reserved = 3
}

export interface Table {
  id: string;
  branchId: string;
  tableNumber: string;
  status: TableStatus;
  statusName: string;
  positionX: number;
  positionY: number;
  width: number;
  height: number;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateTableRequest {
  branchId: string;
  tableNumber: string;
  positionX?: number;
  positionY?: number;
  width?: number;
  height?: number;
}

export interface UpdateTableRequest {
  tableNumber: string;
  positionX: number;
  positionY: number;
  width: number;
  height: number;
}

export interface UpdateTableStatusRequest {
  status: TableStatus;
}

export interface BulkCreateTablesRequest {
  branchId: string;
  count: number;
}

export interface TableCountResponse {
  branchId: string;
  count: number;
}

