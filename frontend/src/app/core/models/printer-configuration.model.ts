export enum PrinterType {
  Kitchen = 1,
  Receipt = 2,
  Label = 3
}

export enum ConnectionType {
  USB = 1,
  Network = 2,
  Serial = 3
}

export interface PrinterConfiguration {
  id: string;
  branchId: string;
  branchName: string;
  name: string;
  type: PrinterType;
  connectionType: ConnectionType;
  printerName: string;
  paperWidth: number;
  isDefault: boolean;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreatePrinterConfigurationRequest {
  branchId: string;
  name: string;
  type: PrinterType;
  connectionType: ConnectionType;
  printerName: string;
  paperWidth: number;
  isDefault: boolean;
}

export interface UpdatePrinterConfigurationRequest {
  name: string;
  type: PrinterType;
  connectionType: ConnectionType;
  printerName: string;
  paperWidth: number;
  isDefault: boolean;
  isActive: boolean;
}

export const PrinterTypeLabels: Record<PrinterType, string> = {
  [PrinterType.Kitchen]: 'Kitchen',
  [PrinterType.Receipt]: 'Receipt',
  [PrinterType.Label]: 'Label'
};

export const ConnectionTypeLabels: Record<ConnectionType, string> = {
  [ConnectionType.USB]: 'USB',
  [ConnectionType.Network]: 'Network',
  [ConnectionType.Serial]: 'Serial'
};

export const PaperWidthOptions = [
  { value: 58, label: '58mm' },
  { value: 80, label: '80mm' }
];

