import type { Cv } from './cv.model';

export interface User {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  passwordHash: string;

  resetToken?: string | null;
  /** ISO datetime string */
  resetTokenExpires?: string | null;

  cvs?: Cv[];
}
