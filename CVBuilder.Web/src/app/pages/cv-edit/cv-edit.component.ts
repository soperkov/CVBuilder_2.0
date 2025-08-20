import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CVService } from '../../services/cv.service';

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
  showModal = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private cvService: CVService
  ) {}

  ngOnInit(): void {
    this.cvId = Number(this.route.snapshot.paramMap.get('id'));
    this.initializeForm();

    this.cvService.getCVById(this.cvId).subscribe({
      next: (cv) => {
        this.patchForm(cv);
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
      dateOfBirth: ['', Validators.required],
      phoneNumber: [''],
      email: ['', [Validators.required, Validators.email]],
      aboutMe: [''],
      photoUrl: [''],
      cvName: [''],
      skills: this.fb.array([]),
      education: this.fb.array([]),
      employment: this.fb.array([]),
    });
  }

  patchForm(cv: any): void {
    this.cvForm.patchValue({
      fullName: cv.fullName,
      dateOfBirth: cv.dateOfBirth,
      phoneNumber: cv.phoneNumber,
      email: cv.email,
      aboutMe: cv.aboutMe,
      photoUrl: cv.photoUrl,
      cvName: cv.cvName || '',
    });

    cv.skills?.forEach((skill: any) =>
      this.skills.push(this.fb.group({ name: [skill, Validators.required] }))
    );

    cv.education?.forEach((edu: any) =>
      this.education.push(
        this.fb.group({
          id: [edu.id],
          institutionName: [edu.institutionName, Validators.required],
          description: [edu.description],
          from: [edu.from, Validators.required],
          to: [edu.to, Validators.required],
        })
      )
    );

    cv.employment?.forEach((emp: any) =>
      this.employment.push(
        this.fb.group({
          id: [emp.id],
          companyName: [emp.companyName, Validators.required],
          description: [emp.description],
          from: [emp.from, Validators.required],
          to: [emp.to, Validators.required],
        })
      )
    );
  }

  // === FormArray helpers ===
  get skills(): FormArray {
    return this.cvForm.get('skills') as FormArray;
  }

  addSkill(): void {
    this.skills.push(this.fb.group({ name: ['', Validators.required] }));
  }

  removeSkill(index: number): void {
    this.skills.removeAt(index);
    this.cvForm.markAsDirty();
    this.cvForm.updateValueAndValidity();
  }

  get education(): FormArray {
    return this.cvForm.get('education') as FormArray;
  }

  addEducation(): void {
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

  get employment(): FormArray {
    return this.cvForm.get('employment') as FormArray;
  }

  addEmployment(): void {
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

  // === Update CV ===
  onSubmit(): void {
    if (this.cvForm.invalid) return;

    this.cvService.updateCV(this.cvId, this.cvForm.value).subscribe({
      next: () => this.router.navigate(['/my-cvs']),
      error: () => (this.errorMessage = 'Failed to update CV.'),
    });
  }

  // === Save As Logic ===
  openModal(): void {
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
  }

  handleSaveAs(newName: string): void {
    const trimmedName = newName.trim();
    if (!this.cvForm.valid || !trimmedName) return;

    this.cvForm.patchValue({ cvName: trimmedName });

    const formData = this.cvForm.value;

    this.cvService.createCV(formData).subscribe({
      next: (id) => {
        this.router.navigate(['/cv', id]);
        this.closeModal();
      },
      error: () => {
        this.errorMessage = 'Failed to create new CV.';
        this.closeModal();
      },
    });
  }
}
