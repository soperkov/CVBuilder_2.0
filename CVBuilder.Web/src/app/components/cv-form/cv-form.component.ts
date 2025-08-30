import {
  Component,
  EventEmitter,
  OnInit,
  OnChanges,
  OnDestroy,
  SimpleChanges,
  Input,
  Output,
} from '@angular/core';
import {
  FormArray,
  FormBuilder,
  FormGroup,
  Validators,
  AbstractControl,
  ValidationErrors,
  ValidatorFn,
} from '@angular/forms';
import { CreateCvDto, Cv } from '../../models';
import { LanguageLevel } from '../../api-client';
import { LanguageService } from '../../services/language.service';
import { UploadsService } from '../../services/uploads.service';
import {
  TemplateService,
  TemplateOption,
} from '../../services/template.service';

type LangOption = { id: number; code?: string; name: string };

/* ------------ Minimal validators (TS function declarations) ------------ */

// Required but ignores pure whitespace.
function requiredTrimmed(): ValidatorFn {
  return (c: AbstractControl): ValidationErrors | null => {
    const v = (c.value ?? '').toString().trim();
    return v ? null : { required: true };
  };
}

// FormArray must contain at least one child where `childKey` is non-empty (trimmed).
function atLeastOneNonEmpty(childKey: string): ValidatorFn {
  return (fa: AbstractControl): ValidationErrors | null => {
    const arr = fa as FormArray;
    const ok = arr.controls.some(
      (g) =>
        ((g.get(childKey)?.value ?? '') as string).toString().trim().length > 0
    );
    return ok ? null : { minItems: true };
  };
}

@Component({
  selector: 'app-cv-form',
  standalone: false,
  templateUrl: './cv-form.component.html',
  styleUrls: ['./cv-form.component.css'],
})
export class CVFormComponent implements OnInit, OnChanges, OnDestroy {
  @Input() mode: 'create' | 'edit' = 'create';
  @Input() value?: Cv | null;

  @Output() formStatusChanged = new EventEmitter<boolean>();
  @Output() formSubmitted = new EventEmitter<CreateCvDto>();

  cvForm!: FormGroup;

  languageLevels: LanguageLevel[] = Object.values(
    LanguageLevel
  ) as LanguageLevel[];
  availableLanguages: LangOption[] = [];

  // templates from API (id, name, thumbUrl)
  availableTemplates: TemplateOption[] = [];

  photoPreviewUrl: string | null = null;
  uploadingPhoto = false;

  hasUploadedOnce = false;
  get uploadLabel(): string {
    return this.mode === 'edit' || this.hasUploadedOnce ? 'Change' : 'Upload';
  }

  get selectedTemplateId(): number | null {
    return this.cvForm?.get('templateId')?.value ?? null;
  }

  constructor(
    private fb: FormBuilder,
    private languageService: LanguageService,
    private uploads: UploadsService,
    private templatesSvc: TemplateService
  ) {}

  ngOnInit(): void {
    this.cvForm = this.fb.group({
      fullName: ['', requiredTrimmed()], // trim-aware required
      dateOfBirth: [''],
      phoneNumber: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      aboutMe: [''],
      photoUrl: [''],
      address: [''],
      webPage: [''],
      jobTitle: [''],
      templateId: [null, Validators.required], // single-select required

      // Required: at least one with non-empty 'name'
      skills: this.fb.array([], { validators: atLeastOneNonEmpty('name') }),

      // Optional – you already gate these in the UI
      education: this.fb.array([]),
      employment: this.fb.array([]),
      language: this.fb.array([]),
    });

    // load languages
    this.languageService.getAll().subscribe({
      next: (list) => (this.availableLanguages = list ?? []),
      error: (err) => console.error('[Languages] load failed', err),
    });

    // load templates (from API -> returns thumbUrl); default to first if none selected
    this.templatesSvc.getAll().subscribe({
      next: (list) => {
        this.availableTemplates = list ?? [];
        const ctrl = this.cvForm.get('templateId');
        if (!ctrl?.value && this.availableTemplates.length) {
          ctrl?.setValue(this.availableTemplates[0].id);
        }
      },
      error: (err) => console.error('[Templates] load failed', err),
    });

    this.cvForm.statusChanges.subscribe(() => {
      this.formStatusChanged.emit(this.cvForm.valid);
    });
    this.formStatusChanged.emit(this.cvForm.valid);

    if (this.value) this.applyValue(this.value);
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['value'] && !changes['value'].firstChange) {
      this.applyValue(this.value ?? null);
    }
  }

  ngOnDestroy(): void {
    this.revokeBlobUrl();
  }

  // ---------- template picker ----------
  selectTemplate(id: number) {
    if (this.selectedTemplateId !== id) {
      const c = this.cvForm.get('templateId');
      c?.setValue(id);
      c?.markAsDirty();
      c?.markAsTouched();
    }
  }

  trackByTemplateId = (_: number, t: TemplateOption) => t.id;

  onThumbError(ev: Event, t: TemplateOption) {
    const img = ev.target as HTMLImageElement;
    img.style.display = 'none';
    img.insertAdjacentHTML(
      'afterend',
      `<div class="thumb-fallback" title="${t.name}">${(t.name || '?').charAt(
        0
      )}</div>`
    );
  }

  // ---------- upload & preview ----------
  onPhotoSelected(evt: Event) {
    const input = evt.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;
    const file = input.files[0];

    // local preview immediately
    this.revokeBlobUrl();
    this.photoPreviewUrl = URL.createObjectURL(file);

    this.uploadingPhoto = true;
    this.uploads.uploadPhoto(file).subscribe({
      next: (res) => {
        this.cvForm.patchValue({ photoUrl: res.path ?? null });
        this.cvForm.markAsDirty();
        this.hasUploadedOnce = true;
      },
      error: (err) => console.error('[Upload photo] failed', err),
      complete: () => (this.uploadingPhoto = false),
    });
  }

  deletePhoto(): void {
    this.revokeBlobUrl();
    this.photoPreviewUrl = null;
    this.cvForm.get('photoUrl')?.setValue(null);
    this.cvForm.markAsDirty();
    this.cvForm.updateValueAndValidity();
    this.hasUploadedOnce = false;
  }

  private setPreviewFromBlob(blob: Blob) {
    this.revokeBlobUrl();
    this.photoPreviewUrl = URL.createObjectURL(blob);
  }

  private revokeBlobUrl() {
    if (this.photoPreviewUrl && this.photoPreviewUrl.startsWith('blob:')) {
      URL.revokeObjectURL(this.photoPreviewUrl);
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

    if (!!isCurrentCtrl.value) {
      toCtrl.reset(null, { emitEvent: false });
      toCtrl.disable({ emitEvent: false });
    }

    isCurrentCtrl.valueChanges.subscribe((val: boolean | null) => {
      const isCurr = !!val;
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
      // trim-aware required (no minLength needed)
      name: [name, [requiredTrimmed()]],
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
      level: [init?.level ?? null, Validators.required],
    });
  }

  private applyValue(cv: Cv | null) {
    if (!cv) {
      this.cvForm.reset();
      this.clearArray(this.skills);
      this.clearArray(this.education);
      this.clearArray(this.employment);
      this.clearArray(this.language);
      this.deletePhoto();
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
    (cv.skills ?? []).forEach((s: any) =>
      this.skills.push(this.makeSkillGroup(s))
    );

    this.clearArray(this.education);
    (cv.education ?? []).forEach((e) =>
      this.education.push(this.makeEducationGroup(e))
    );

    this.clearArray(this.employment);
    (cv.employment ?? []).forEach((e) =>
      this.employment.push(this.makeEmploymentGroup(e))
    );

    this.clearArray(this.language);
    const langs = (cv as any).language ?? (cv as any).languages ?? [];
    langs.forEach((l: any) => this.language.push(this.makeLanguageGroup(l)));

    if (!cv.photoUrl) {
      this.hasUploadedOnce = false;
      this.revokeBlobUrl();
      this.photoPreviewUrl = null;
    } else {
      const p = cv.photoUrl;
      if (/^https?:\/\//i.test(p)) {
        this.revokeBlobUrl();
        this.photoPreviewUrl = p;
        this.hasUploadedOnce = true;
      } else {
        this.uploads.getPhotoAsBlob(p).subscribe({
          next: (blob) => {
            this.setPreviewFromBlob(blob);
            this.hasUploadedOnce = true;
          },
          error: (err) => {
            console.error('[Photo preview] failed', err);
            this.hasUploadedOnce = false;
            this.revokeBlobUrl();
            this.photoPreviewUrl = null;
          },
        });
      }
    }

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

  buildCreateDto(): CreateCvDto {
    const v = this.cvForm.getRawValue() as any;
    const dto: CreateCvDto = {
      cvName: '',
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

      language: (v.language || []).map((l: any) => ({
        id: l.id ?? 0,
        languageId: Number(l.languageId),
        level: l.level as LanguageLevel,
      })),
    };
    return dto;
  }

  // expose for templates (optional)
  isInvalid(path: string) {
    const c = this.cvForm.get(path);
    return !!(c && c.touched && c.invalid);
  }

  onSubmit(): void {
    if (this.cvForm.invalid) {
      this.cvForm.markAllAsTouched();
      return;
    }
    this.formSubmitted.emit(this.buildCreateDto());
  }
}
