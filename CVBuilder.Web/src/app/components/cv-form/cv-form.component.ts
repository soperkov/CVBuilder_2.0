import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CreateCvDto } from '../../models';
import { LanguageLevel } from '../../api-client';
import { FormControl } from '@angular/forms';
import {
  Observable,
  debounceTime,
  distinctUntilChanged,
  map,
  startWith /*, switchMap*/,
} from 'rxjs';
import { LanguageService } from '../../services/language.service';

type LangOption = { id: number; code?: string; name: string };

@Component({
  selector: 'app-cv-form',
  standalone: false,
  templateUrl: './cv-form.component.html',
  styleUrls: ['./cv-form.component.css'],
})
export class CVFormComponent implements OnInit {
  cvForm!: FormGroup;
  languageLevels: LanguageLevel[] = Object.values(
    LanguageLevel
  ) as LanguageLevel[];
  availableLanguages: LangOption[] = [];

  @Output() formStatusChanged = new EventEmitter<boolean>();
  @Output() formSubmitted = new EventEmitter<CreateCvDto>();

  constructor(
    private fb: FormBuilder,
    private languageService: LanguageService
  ) {}

  ngOnInit(): void {
    this.cvForm = this.fb.group({
      fullName: ['', Validators.required],
      dateOfBirth: ['', Validators.required], // "yyyy-MM-dd"
      phoneNumber: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      aboutMe: [''],
      photoUrl: [''],
      address: [''],
      webPage: [''],
      jobTitle: [''],
      templateId: [null],
      skills: this.fb.array([]),
      education: this.fb.array([]),
      employment: this.fb.array([]),
      language: this.fb.array([]),
    });

    this.languageService.getAll().subscribe({
      next: (list) => (this.availableLanguages = list ?? []),
      error: (err) => console.error('[Languages] load failed', err),
    });

    this.cvForm.statusChanges.subscribe(() => {
      this.formStatusChanged.emit(this.cvForm.valid);
    });
    this.formStatusChanged.emit(this.cvForm.valid);
  }

  private attachCurrentToggle(group: FormGroup) {
    const isCurrentCtrl = group.get('isCurrent');
    const toCtrl = group.get('to');
    if (!isCurrentCtrl || !toCtrl) return;

    isCurrentCtrl.valueChanges.subscribe((isCurr: boolean) => {
      if (isCurr) {
        toCtrl.reset(null, { emitEvent: false });
        toCtrl.disable({ emitEvent: false });
      } else {
        toCtrl.enable({ emitEvent: false });
      }
      group.updateValueAndValidity({ onlySelf: true });
    });

    const initial = (isCurrentCtrl as any).value as boolean;
    if (initial) {
      toCtrl.reset(null, { emitEvent: false });
      toCtrl.disable({ emitEvent: false });
    }
  }

  // ===== FormArrays =====
  get skills(): FormArray {
    return this.cvForm.get('skills') as FormArray;
  }
  get education(): FormArray {
    return this.cvForm.get('education') as FormArray;
  }
  get employment(): FormArray {
    return this.cvForm.get('employment') as FormArray;
  }
  get language(): FormArray {
    return this.cvForm.get('language') as FormArray;
  }

  // ===== Can-add getters =====
  get canAddSkill(): boolean {
    const arr = this.skills;
    return arr.length === 0 || arr.at(arr.length - 1).valid;
  }
  get canAddEducation(): boolean {
    const arr = this.education;
    return arr.length === 0 || arr.at(arr.length - 1).valid;
  }
  get canAddEmployment(): boolean {
    const arr = this.employment;
    return arr.length === 0 || arr.at(arr.length - 1).valid;
  }
  get canAddLanguage(): boolean {
    const arr = this.language;
    return arr.length === 0 || arr.at(arr.length - 1).valid;
  }

  // ===== Add / Remove =====
  addSkill(): void {
    if (!this.canAddSkill) return;
    this.skills.push(
      this.fb.group({
        name: ['', [Validators.required, Validators.minLength(1)]],
      })
    );
  }
  removeSkill(index: number): void {
    this.skills.removeAt(index);
    this.cvForm.markAsDirty();
    this.cvForm.updateValueAndValidity();
  }

  addEducation(): void {
    if (!this.canAddEducation) return;
    const group = this.fb.group({
      institutionName: ['', Validators.required],
      description: [''],
      from: ['', Validators.required], // "yyyy-MM-dd"
      to: [{ value: null, disabled: false }],
      isCurrent: this.fb.nonNullable.control(false),
    });
    this.attachCurrentToggle(group);
    this.education.push(group);
  }
  removeEducation(index: number): void {
    this.education.removeAt(index);
    this.cvForm.markAsDirty();
    this.cvForm.updateValueAndValidity();
  }

  addEmployment(): void {
    if (!this.canAddEmployment) return;
    const group = this.fb.group({
      companyName: ['', Validators.required],
      description: [''],
      from: ['', Validators.required],
      to: [{ value: null, disabled: false }],
      isCurrent: this.fb.nonNullable.control(false),
    });
    this.attachCurrentToggle(group);
    this.employment.push(group);
  }
  removeEmployment(index: number): void {
    this.employment.removeAt(index);
    this.cvForm.markAsDirty();
    this.cvForm.updateValueAndValidity();
  }

  addLanguage(): void {
    if (!this.canAddLanguage) return;
    this.language.push(
      this.fb.group({
        languageId: [null, Validators.required],
        level: [null, Validators.required],
      })
    );
  }

  removeLanguage(index: number): void {
    this.language.removeAt(index);
    this.cvForm.markAsDirty();
    this.cvForm.updateValueAndValidity();
  }

  displayLanguageName = (l?: LangOption): string => (l ? l.name : '');

  buildCreateDto(): CreateCvDto {
    const v = this.cvForm.value;
    const dto: CreateCvDto = {
      cvName: v.cvName ?? '',
      fullName: v.fullName ?? '',
      dateOfBirth: v.dateOfBirth ?? null,
      phoneNumber: v.phoneNumber ?? '',
      email: v.email ?? '',
      aboutMe: v.aboutMe ?? '',
      photoUrl: v.photoUrl ?? '',
      address: v.address ?? '',
      webPage: v.webPage ?? '',
      jobTitle: v.jobTitle ?? '',
      templateId: v.templateId ?? null,
      skills: (v.skills || []).map((s: any) => ({ name: s.name })),
      education: (v.education || []).map((e: any) => ({
        id: e.id,
        institutionName: e.institutionName,
        description: e.description ?? '',
        from: e.from,
        to: e.to,
        isCurrent: e.isCurrent,
      })),
      employment: (v.employment || []).map((e: any) => ({
        id: e.id,
        companyName: e.companyName,
        description: e.description ?? '',
        from: e.from,
        to: e.to,
        isCurrent: e.isCurrent,
      })),
      language: Array.isArray((v as any).language)
        ? (v as any).language.map((e: any) => ({
            languageId: e.languageId,
            level: e.level,
          }))
        : [],
    };
    return dto;
  }

  // ===== Submit =====
  onSubmit(): void {
    if (this.cvForm.invalid) return;
    this.formSubmitted.emit(this.buildCreateDto());
  }
}
