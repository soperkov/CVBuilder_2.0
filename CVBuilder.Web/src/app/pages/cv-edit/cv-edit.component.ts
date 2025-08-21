import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CVService } from '../../services/cv.service';
import { Cv, CreateCvDto, UpdateCvDto } from '../../models';

@Component({
  selector: 'app-cv-edit',
  standalone: false,
  templateUrl: './cv-edit.component.html',
  styleUrls: ['./cv-edit.component.css'],
})
export class CVEditComponent implements OnInit {
  cvForm!: FormGroup;
  cvId!: number;
  isLoading = true;
  errorMessage = '';
  showSaveModal = false;
  showDeleteModal = false;
  isDeleting = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private cvService: CVService
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    this.cvId = Number(idParam);

    this.initializeForm();

    this.cvService.getCVById(this.cvId).subscribe({
      next: (cv: Cv) => {
        this.patchForm(cv);
        this.cvForm.markAsPristine();
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load CV data.';
        this.isLoading = false;
      },
    });
  }

  get isFormModified(): boolean {
    return this.cvForm.dirty && this.cvForm.valid;
  }

  initializeForm(): void {
    this.cvForm = this.fb.group({
      fullName: ['', Validators.required],
      dateOfBirth: ['', Validators.required], // "yyyy-MM-dd"
      phoneNumber: [''],
      email: ['', [Validators.required, Validators.email]],
      aboutMe: [''],
      photoUrl: [''],
      cvName: [''],
      templateId: [null],
      skills: this.fb.array([]),
      education: this.fb.array([]),
      employment: this.fb.array([]),
    });
  }

  private toDateOnly(value?: string | null): string {
    if (!value) return '';
    return value.length >= 10 ? value.substring(0, 10) : value;
  }

  patchForm(cv: Cv): void {
    this.cvForm.patchValue({
      fullName: cv.fullName ?? '',
      dateOfBirth: this.toDateOnly(cv.dateOfBirth ?? null),
      phoneNumber: cv.phoneNumber ?? '',
      email: cv.email ?? '',
      aboutMe: cv.aboutMe ?? '',
      photoUrl: cv.photoUrl ?? '',
      cvName: cv.cvName ?? '',
      templateId: cv.templateId ?? null,
    });

    const skillsArr = Array.isArray(cv.skills) ? cv.skills : [];
    skillsArr.forEach((s: any) => {
      const name = typeof s === 'string' ? s : s?.name ?? '';
      this.skills.push(this.fb.group({ name: [name, Validators.required] }));
    });

    // Education
    (cv.education || []).forEach((edu) =>
      this.education.push(
        this.fb.group({
          id: [edu.id],
          institutionName: [edu.institutionName, Validators.required],
          description: [edu.description ?? ''],
          from: [this.toDateOnly(edu.from), Validators.required],
          to: [this.toDateOnly(edu.to), Validators.required],
        })
      )
    );

    // Employment
    (cv.employment || []).forEach((emp) =>
      this.employment.push(
        this.fb.group({
          id: [emp.id],
          companyName: [emp.companyName, Validators.required],
          description: [emp.description ?? ''],
          from: [this.toDateOnly(emp.from), Validators.required],
          to: [this.toDateOnly(emp.to), Validators.required],
        })
      )
    );
  }

  // ===== FormArray helpers =====
  get skills(): FormArray {
    return this.cvForm.get('skills') as FormArray;
  }
  get education(): FormArray {
    return this.cvForm.get('education') as FormArray;
  }
  get employment(): FormArray {
    return this.cvForm.get('employment') as FormArray;
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

  // ===== Add / Remove =====
  addSkill(): void {
    if (!this.canAddSkill) return;
    this.skills.push(this.fb.group({ name: ['', Validators.required] }));
  }
  removeSkill(index: number): void {
    this.skills.removeAt(index);
    this.cvForm.markAsDirty();
    this.cvForm.updateValueAndValidity();
  }

  addEducation(): void {
    if (!this.canAddEducation) return;
    this.education.push(
      this.fb.group({
        institutionName: ['', Validators.required],
        description: [''],
        from: ['', Validators.required],
        to: ['', Validators.required],
      })
    );
  }
  removeEducation(index: number): void {
    this.education.removeAt(index);
    this.cvForm.markAsDirty();
    this.cvForm.updateValueAndValidity();
  }

  addEmployment(): void {
    if (!this.canAddEmployment) return;
    this.employment.push(
      this.fb.group({
        companyName: ['', Validators.required],
        description: [''],
        from: ['', Validators.required],
        to: ['', Validators.required],
      })
    );
  }
  removeEmployment(index: number): void {
    this.employment.removeAt(index);
    this.cvForm.markAsDirty();
    this.cvForm.updateValueAndValidity();
  }

  // ===== Build DTOs =====
  private buildUpdateDto(): UpdateCvDto {
    const v = this.cvForm.value;
    const dto: UpdateCvDto = {
      cvName: v.cvName ?? '',
      fullName: v.fullName ?? '',
      dateOfBirth: v.dateOfBirth ?? null, // "yyyy-MM-dd"
      phoneNumber: v.phoneNumber ?? '',
      email: v.email ?? '',
      aboutMe: v.aboutMe ?? '',
      photoUrl: v.photoUrl ?? '',
      templateId: v.templateId ?? null,
      skills: (v.skills || []).map((s: any) => ({ name: s.name })),
      education: (v.education || []).map((e: any) => ({
        id: e.id, // backend može iskoristiti
        institutionName: e.institutionName,
        description: e.description ?? '',
        from: e.from,
        to: e.to,
      })),
      employment: (v.employment || []).map((e: any) => ({
        id: e.id,
        companyName: e.companyName,
        description: e.description ?? '',
        from: e.from,
        to: e.to,
      })),
    };
    return dto;
  }

  private buildCreateDto(withName?: string): CreateCvDto {
    const v = this.cvForm.value;
    const dto: CreateCvDto = {
      cvName: (withName ?? v.cvName) || '',
      fullName: v.fullName ?? '',
      dateOfBirth: v.dateOfBirth ?? null, // "yyyy-MM-dd"
      phoneNumber: v.phoneNumber ?? '',
      email: v.email ?? '',
      aboutMe: v.aboutMe ?? '',
      photoUrl: v.photoUrl ?? '',
      templateId: v.templateId ?? null,
      skills: (v.skills || []).map((s: any) => ({ name: s.name })),
      education: (v.education || []).map((e: any) => ({
        id: e.id,
        institutionName: e.institutionName,
        description: e.description ?? '',
        from: e.from,
        to: e.to,
      })),
      employment: (v.employment || []).map((e: any) => ({
        id: e.id,
        companyName: e.companyName,
        description: e.description ?? '',
        from: e.from,
        to: e.to,
      })),
    };
    return dto;
  }

  // ===== Update CV =====
  onSubmit(): void {
    if (this.cvForm.invalid) return;

    const dto = this.buildUpdateDto();

    this.cvService.updateCV(this.cvId, dto).subscribe({
      next: () => this.router.navigate(['/my-cvs']),
      error: () => (this.errorMessage = 'Failed to update CV.'),
    });
  }

  // ===== Save As (modal) =====
  openSaveModal(): void {
    this.showSaveModal = true;
  }
  closeSaveModal(): void {
    this.showSaveModal = false;
  }

  handleSaveAs(newName: string): void {
    const trimmedName = (newName || '').trim();
    if (!this.cvForm.valid || !trimmedName) return;

    const dto = this.buildCreateDto(trimmedName);

    this.cvService.createCV(dto).subscribe({
      next: (id) => {
        this.router.navigate(['/cv', id]);
        this.closeSaveModal();
      },
      error: () => {
        this.errorMessage = 'Failed to create new CV.';
        this.closeSaveModal();
      },
    });
  }

  // ===== Delete (modal) =====
  openDeleteModal() {
    this.showDeleteModal = true;
  }
  closeDeleteModal() {
    this.showDeleteModal = false;
  }

  deleteThisCv() {
    this.isDeleting = true;
    this.cvService.deleteCV(this.cvId).subscribe({
      next: () => this.router.navigate(['/my-cvs']),
      error: () => {
        this.errorMessage = 'Failed to delete CV.';
        this.isDeleting = false;
        this.closeDeleteModal();
      },
    });
  }
}
