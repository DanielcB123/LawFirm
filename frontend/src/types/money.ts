/**
 * Money crosses the API as a decimal string plus ISO 4217 currency code, never a JS number,
 * so amounts stored as DECIMAL(19,2) are not rounded by floating point.
 */
export type Money = {
  amount: string;
  currency: string;
};
