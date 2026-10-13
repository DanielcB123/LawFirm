import type { Money } from "../../types/money";

// Draft shapes for gap G50. No backend routes exist yet; align with the API contract when Features/Billing ships.

export type TimeEntryStatus = "Draft" | "Submitted" | "Approved" | "Billed";

export type TimeEntry = {
  id: string;
  matterId: string;
  workDate: string;
  minutes: number;
  narrative: string;
  isBillable: boolean;
  status: TimeEntryStatus;
};

export type InvoiceStatus = "Draft" | "Issued" | "PartiallyPaid" | "Paid" | "Void";

export type InvoiceSummary = {
  id: string;
  invoiceNumber: string;
  matterId: string;
  issuedOn: string | null;
  total: Money;
  balanceDue: Money;
  status: InvoiceStatus;
};
