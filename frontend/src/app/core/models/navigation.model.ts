export interface NavigationMenuItem {
  id: string;
  label: string;
  icon?: string;
  route?: string;
  parentId?: string;
  allowedRoles: string[];
  children?: NavigationMenuItem[];
}

