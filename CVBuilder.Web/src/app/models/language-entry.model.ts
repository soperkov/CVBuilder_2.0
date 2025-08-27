import { LanguageLevel } from '../api-client';

export interface LanguageEntry {
  id?: number;
  languageId?: number;
  level: LanguageLevel;
}
