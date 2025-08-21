import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CreateCvDto } from '../../models';

@Component({
  selector: 'app-cv-form',
  standalone: false,
  templateUrl: './cv-form.component.html',
  styleUrls: ['./cv-form.component.css'],
})
export class CVFormComponent implements OnInit {
  cvForm!: FormGroup;

  @Output() formStatusChanged = new EventEmitter<boolean>();
  @Output() formSubmitted = new EventEmitter<CreateCvDto>();

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    this.cvForm = this.fb.group({
      fullName: ['', Validators.required],
      dateOfBirth: ['', Validators.required], // "yyyy-MM-dd"
      phoneNumber: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      aboutMe: [''],
      photoUrl: [''],
      templateId: [null],
      skills: this.fb.array([]),
      education: this.fb.array([]),
      employment: this.fb.array([]),
    });

    this.cvForm.statusChanges.subscribe(() => {
      this.formStatusChanged.emit(this.cvForm.valid);
    });
    this.formStatusChanged.emit(this.cvForm.valid);
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
    this.education.push(
      this.fb.group({
        institutionName: ['', Validators.required],
        description: [''],
        from: ['', Validators.required], // "yyyy-MM-dd"
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

  // ===== Submit =====
  onSubmit(): void {
    if (this.cvForm.invalid) return;
    this.formSubmitted.emit(this.buildCreateDto());
  }
}
