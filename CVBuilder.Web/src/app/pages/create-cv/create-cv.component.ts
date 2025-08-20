import { Component, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { CVService } from '../../services/cv.service';
import { CVFormComponent } from '../../components/cv-form/cv-form.component';

@Component({
  selector: 'app-create-cv',
  standalone: false,
  styleUrls: ['./create-cv.component.css'],
  templateUrl: './create-cv.component.html',
})
export class CreateCVComponent {
  @ViewChild(CVFormComponent) cvFormComponent!: CVFormComponent;
  showModal = false;
  isFormValid = false;
  modalAction: 'save' | 'saveAs' = 'save';
  lastCVData: any = null;

  constructor(private router: Router, private cvService: CVService) {}

  onFormSubmitted(dto: any) {
    this.lastCVData = dto;
    this.openModal('save');
  }

  onFormValidityChanged(isValid: boolean) {
    this.isFormValid = isValid;
  }

  openModal(action: 'save' | 'saveAs') {
    if (!this.cvFormComponent || this.cvFormComponent.cvForm.invalid) {
      console.warn('[CreateCV] Form is invalid or not initialized.');
      return;
    }

    this.lastCVData = this.cvFormComponent.cvForm.value;
    this.modalAction = action;
    this.showModal = true;
  }

  closeModal() {
    this.showModal = false;
  }

  handleModalSave(cvName: string) {
    console.log('[CreateCV] Received cvName:', cvName);

    if (!this.lastCVData) return;

    const data = {
      ...this.lastCVData,
      cvName,
    };

    console.log('[CreateCV] Sending data to backend:', data);

    this.showModal = false;

    this.cvService.createCV(data).subscribe({
      next: (id) => {
        console.log('[CreateCV] CV created with ID:', id);
        this.router.navigate(['/cv', id]);
      },
      error: (err) => {
        console.error('[CreateCV] Error while saving CV:', err);
      },
    });
  }
}
