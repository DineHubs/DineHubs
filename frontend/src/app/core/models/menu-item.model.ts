export interface MenuItem {
  id: string;
  branchId: string;
  name: string;
  category: string;
  price: number;
  isAvailable: boolean;
  imageUrl?: string;
}

export interface CreateMenuItemRequest {
  branchId: string;
  name: string;
  category: string;
  price: number;
  isAvailable: boolean;
  imageUrl?: string;
}

export interface UpdateMenuItemRequest {
  name: string;
  category: string;
  price: number;
  isAvailable: boolean;
  imageUrl?: string;
}

