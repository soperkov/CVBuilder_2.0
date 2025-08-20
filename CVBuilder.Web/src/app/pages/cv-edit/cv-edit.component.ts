import { Component } from '@angular/core';
import { FormGroup, FormBuilder, Validators, FormArray } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CVService } from '../../services/cv.service';

@Component({
  selector: 'app-cv-edit',
  standalone: false,
  templateUrl: './cv-edit.component.html',
  styleUrl: './cv-edit.component.css',
})
export class CVEditComponent {
  cvForm!: FormGroup;
  cvId!: number;
  isLoading = true;
  errorMessage = '';

  constructor(
    private route: ActivatedRoute,
    private fb: FormBuilder,
    private cvService: CVService,
    private router: Router
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

    const skills = this.cvForm.get('skills') as FormArray;
    cv.skills.forEach((skill: any) =>
      skills.push(this.fb.group({ name: [skill] }))
    );

    const education = this.cvForm.get('education') as FormArray;
    cv.education.forEach((edu: any) =>
      education.push(
        this.fb.group({
          id: [edu.id],
          institutionName: [edu.institutionName],
          description: [edu.description],
          from: [edu.from],
          to: [edu.to],
        })
      )
    );

    const employment = this.cvForm.get('employment') as FormArray;
    cv.employment.forEach((emp: any) =>
      employment.push(
        this.fb.group({
          id: [emp.id],
          companyName: [emp.companyName],
          description: [emp.description],
          from: [emp.from],
          to: [emp.to],
        })
      )
    );
  }

  onSubmit(): void {
    if (this.cvForm.invalid) return;

    this.cvService.updateCV(this.cvId, this.cvForm.value).subscribe({
      next: () => {
        this.router.navigate(['/my-cvs']);
      },
      error: () => {
        this.errorMessage = 'Failed to update CV.';
      },
    });
  }

  // Helpers to access FormArrays
  get skills() {
    return this.cvForm.get('skills') as FormArray;
  }

  get education() {
    return this.cvForm.get('education') as FormArray;
  }

  get employment() {
    return this.cvForm.get('employment') as FormArray;
  }
}
