import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CVService } from '../../services/cv.service';

@Component({
  selector: 'app-cv-form',
  standalone: false,
  templateUrl: './cv-form.component.html',
  styleUrl: './cv-form.component.css',
})
export class CvFormComponent implements OnInit {
  cvForm!: FormGroup;

  @Output() formSubmitted = new EventEmitter<number>();

  constructor(private fb: FormBuilder, private cvService: CVService) {}

  ngOnInit(): void {
    this.cvForm = this.fb.group({
      fullName: ['', Validators.required],
      dateOfBirth: ['', Validators.required],
      phoneNumber: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      aboutMe: [''],
      photoUrl: [''],
      templateId: [null],
      skills: this.fb.array([]),
      education: this.fb.array([]),
      employment: this.fb.array([]),
    });
  }

  get skills(): FormArray {
    return this.cvForm.get('skills') as FormArray;
  }

  addSkill(): void {
    this.skills.push(
      this.fb.group({
        name: ['', Validators.required],
      })
    );
  }

  removeSkill(index: number): void {
    this.skills.removeAt(index);
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
  }

  // ========== SUBMIT ==========
  onSubmit(): void {
    if (this.cvForm.invalid) return;

    const formData = this.cvForm.value;

    this.cvService.createCv(formData).subscribe({
      next: (response) => {
        console.log('CV saved!', response);
        // možeš redirectati ili prikazati poruku
      },
      error: (error) => {
        console.error('Failed to save CV', error);
      },
    });
  }
}
