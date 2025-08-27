import {
  Component,
  EventEmitter,
  OnInit,
  OnChanges,
  SimpleChanges,
  Input,
  Output,
} from '@angular/core';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CreateCvDto, Cv } from '../../models';
import { LanguageLevel } from '../../api-client';
import { LanguageService } from '../../services/language.service';

type LangOption = { id: number; code?: string; name: string };

@Component({
  selector: 'app-cv-form',
  standalone: false,
  templateUrl: './cv-form.component.html',
  styleUrls: ['./cv-form.component.css'],
})
export class CVFormComponent implements OnInit, OnChanges {
  @Input() mode: 'create' | 'edit' = 'create';
  @Input() value?: Cv | null; // za patch kod edita

  @Output() formStatusChanged = new EventEmitter<boolean>();
  @Output() formSubmitted = new EventEmitter<CreateCvDto>(); // roditelj će dodati cvName i pozvati create/update

  cvForm!: FormGroup;

  languageLevels: LanguageLevel[] = Object.values(
    LanguageLevel
  ) as LanguageLevel[];
  availableLanguages: LangOption[] = [];

  constructor(
    private fb: FormBuilder,
    private languageService: LanguageService
  ) {}

  ngOnInit(): void {
    this.cvForm = this.fb.group({
      // bez cvName — dolazi iz modala u roditelju
      fullName: ['', Validators.required],
      dateOfBirth: [''], // "yyyy-MM-dd"
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

    // ako je value već stigao prije OnInit
    if (this.value) this.applyValue(this.value);
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['value'] && !changes['value'].firstChange) {
      this.applyValue(this.value ?? null);
    }
  }

  // ---------- helpers ----------
  private clearArray(arr: FormArray) {
    while (arr.length) arr.removeAt(0);
  }
  private toDateOnly(v?: string | null): string {
    if (!v) return '';
    return v.length >= 10 ? v.substring(0, 10) : v;
  }
  private attachCurrentToggle(group: FormGroup) {
    const isCurrentCtrl = group.get('isCurrent');
    const toCtrl = group.get('to');
    if (!isCurrentCtrl || !toCtrl) return;

    // initial
    if (isCurrentCtrl.value === true) {
      toCtrl.reset(null, { emitEvent: false });
      toCtrl.disable({ emitEvent: false });
    }

    // changes
    isCurrentCtrl.valueChanges.subscribe((isCurr: boolean) => {
      if (isCurr) {
        toCtrl.reset(null, { emitEvent: false });
        toCtrl.disable({ emitEvent: false });
      } else {
        toCtrl.enable({ emitEvent: false });
      }
      group.updateValueAndValidity({ onlySelf: true });
    });
  }

  private makeSkillGroup(init?: any) {
    const name = typeof init === 'string' ? init : init?.name ?? '';
    return this.fb.group({
      name: [name, [Validators.required, Validators.minLength(1)]],
    });
  }
  private makeEducationGroup(init?: any) {
    const g = this.fb.group({
      id: [init?.id ?? null],
      institutionName: [init?.institutionName ?? '', Validators.required],
      description: [init?.description ?? ''],
      from: [this.toDateOnly(init?.from) ?? '', Validators.required],
      to: [{ value: this.toDateOnly(init?.to) ?? null, disabled: false }],
      isCurrent: this.fb.nonNullable.control(!!init?.isCurrent),
    });
    this.attachCurrentToggle(g);
    return g;
  }
  private makeEmploymentGroup(init?: any) {
    const g = this.fb.group({
      id: [init?.id ?? null],
      companyName: [init?.companyName ?? '', Validators.required],
      description: [init?.description ?? ''],
      from: [this.toDateOnly(init?.from) ?? '', Validators.required],
      to: [{ value: this.toDateOnly(init?.to) ?? null, disabled: false }],
      isCurrent: this.fb.nonNullable.control(!!init?.isCurrent),
    });
    this.attachCurrentToggle(g);
    return g;
  }
  private makeLanguageGroup(init?: any) {
    return this.fb.group({
      id: [init?.id ?? null],
      languageId: [init?.languageId ?? null, Validators.required],
      level: [init?.level ?? null, Validators.required], // broj ili enum vrijednost iz NSwag-a
    });
  }

  private applyValue(cv: Cv | null) {
    if (!cv) {
      this.cvForm.reset();
      this.clearArray(this.skills);
      this.clearArray(this.education);
      this.clearArray(this.employment);
      this.clearArray(this.language);
      return;
    }

    this.cvForm.patchValue({
      fullName: cv.fullName ?? '',
      dateOfBirth: this.toDateOnly(cv.dateOfBirth ?? null),
      phoneNumber: cv.phoneNumber ?? '',
      email: cv.email ?? '',
      aboutMe: cv.aboutMe ?? '',
      photoUrl: cv.photoUrl ?? '',
      address: (cv as any).address ?? '',
      webPage: (cv as any).webPage ?? '',
      jobTitle: (cv as any).jobTitle ?? '',
      templateId: cv.templateId ?? null,
    });

    this.clearArray(this.skills);
    (cv.skills ?? []).forEach((s: any) => {
      this.skills.push(this.makeSkillGroup(s));
    });

    this.clearArray(this.education);
    (cv.education ?? []).forEach((e) =>
      this.education.push(this.makeEducationGroup(e))
    );

    this.clearArray(this.employment);
    (cv.employment ?? []).forEach((e) =>
      this.employment.push(this.makeEmploymentGroup(e))
    );

    this.clearArray(this.language);
    // pozor: u modelu si kolekciju nazvala Language (singular) – ako je plural, promijeni ovdje
    const langs = (cv as any).language ?? (cv as any).languages ?? [];
    langs.forEach((l: any) => this.language.push(this.makeLanguageGroup(l)));

    this.cvForm.markAsPristine();
  }

  // ---------- getters ----------
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

  // can-add
  get canAddSkill() {
    const a = this.skills;
    return a.length === 0 || a.at(a.length - 1).valid;
  }
  get canAddEducation() {
    const a = this.education;
    return a.length === 0 || a.at(a.length - 1).valid;
  }
  get canAddEmployment() {
    const a = this.employment;
    return a.length === 0 || a.at(a.length - 1).valid;
  }
  get canAddLanguage() {
    const a = this.language;
    return a.length === 0 || a.at(a.length - 1).valid;
  }

  // add/remove
  addSkill() {
    if (this.canAddSkill) this.skills.push(this.makeSkillGroup());
  }
  removeSkill(i: number) {
    this.skills.removeAt(i);
    this.cvForm.markAsDirty();
    this.cvForm.updateValueAndValidity();
  }

  addEducation() {
    if (this.canAddEducation) this.education.push(this.makeEducationGroup());
  }
  removeEducation(i: number) {
    this.education.removeAt(i);
    this.cvForm.markAsDirty();
    this.cvForm.updateValueAndValidity();
  }

  addEmployment() {
    if (this.canAddEmployment) this.employment.push(this.makeEmploymentGroup());
  }
  removeEmployment(i: number) {
    this.employment.removeAt(i);
    this.cvForm.markAsDirty();
    this.cvForm.updateValueAndValidity();
  }

  addLanguage() {
    if (this.canAddLanguage) this.language.push(this.makeLanguageGroup());
  }
  removeLanguage(i: number) {
    this.language.removeAt(i);
    this.cvForm.markAsDirty();
    this.cvForm.updateValueAndValidity();
  }

  displayLanguageName = (l?: LangOption): string => (l ? l.name : '');

  // ---------- DTO builder ----------
  // koristimo getRawValue da pokupimo i 'to' kad je disabled
  buildCreateDto(): CreateCvDto {
    const v = this.cvForm.getRawValue() as any;
    const dto: CreateCvDto = {
      cvName: '', // roditelj će postaviti kroz modal
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
        id: e.id ?? 0,
        institutionName: e.institutionName,
        description: e.description ?? '',
        from: e.from,
        to: e.isCurrent ? null : e.to,
        isCurrent: !!e.isCurrent,
      })),

      employment: (v.employment || []).map((e: any) => ({
        id: e.id ?? 0,
        companyName: e.companyName,
        description: e.description ?? '',
        from: e.from,
        to: e.isCurrent ? null : e.to,
        isCurrent: !!e.isCurrent,
      })),

      // u tvom backendu kolekcija je "Language" (singular); DTO ključ držiš kao "language"
      language: (v.language || []).map((l: any) => ({
        id: l.id ?? 0,
        languageId: Number(l.languageId),
        level: l.level as LanguageLevel,
      })),
    };
    return dto;
  }

  // submit — roditelj odlučuje je li create ili update i dodaje cvName
  onSubmit(): void {
    if (this.cvForm.invalid) return;
    this.formSubmitted.emit(this.buildCreateDto());
  }
}
