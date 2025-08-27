import { DatePipe } from '@angular/common';
import type { User } from './user.model';
import type { Template } from './template.model';
import type { Skill } from './skill.model';
import type { EducationEntry } from './education-entry.model';
import type { EmploymentEntry } from './employment-entry.model';
import { LanguageEntry } from './language-entry.model';

export interface Cv {
  id: number;

  createdByUser: number;
  user: User;

  cvName: string;

  fullName?: string | null;
  dateOfBirth?: string | null;
  phoneNumber: string;
  email: string;
  aboutMe?: string | null;
  photoUrl?: string | null;
  address?: string | null;
  webPage?: string | null;
  jobTitle?: string | null;

  skills: Skill[];
  education?: EducationEntry[] | null;
  employment?: EmploymentEntry[] | null;
  language?: LanguageEntry[] | null;

  templateId?: number | null;
  template?: Template | null;

  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface CvListItem {
  id: number;
  fullName?: string | null;
  cvName: string;
  templateId?: number | null;
  template?: Pick<Template, 'id' | 'name'> | null;
  createdAtUtc: string;
  modifiedAt?: string;
}

export interface CreateCvDto {
  cvName: string;
  fullName?: string | null;
  /** 'yyyy-MM-dd' */
  dateOfBirth?: string | null;
  phoneNumber: string;
  email: string;
  aboutMe?: string | null;
  photoUrl?: string | null;
  address?: string | null;
  webPage?: string | null;
  jobTitle?: string | null;

  skills: Skill[] | null;
  education?: EducationEntry[] | null;
  employment?: EmploymentEntry[] | null;
  language?: LanguageEntry[] | null;

  templateId?: number | null;
}

export type UpdateCvDto = CreateCvDto;

export interface CvDisplay extends Cv {
  localTime: Date | null;
}
