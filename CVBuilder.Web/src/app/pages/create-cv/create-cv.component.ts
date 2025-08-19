import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-create-cv',
  standalone: false,
  styleUrl: './create-cv.component.css',
  template: `<app-cv-form
    (formSubmitted)="onFormSubmitted($event)"
  ></app-cv-form>`,
})
export class CreateCVComponent {
  constructor(private router: Router) {}

  onFormSubmitted(cvId: number) {
    this.router.navigate(['/cv', cvId]);
  }
}
