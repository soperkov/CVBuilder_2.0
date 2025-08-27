export interface EmploymentEntry {
  id?: number;
  companyName: string;
  description?: string;
  from: string;
  to?: string | null;
  isCurrent: boolean;
}
