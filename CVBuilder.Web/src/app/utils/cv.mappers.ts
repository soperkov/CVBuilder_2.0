import { Cv, CvDisplay } from '../models/cv.model';
import { toLocalDate } from './date.utils';

export function mapCvToDisplay(cv: Cv): CvDisplay {
  return { ...cv, localTime: toLocalDate(cv.createdAtUtc) };
}
