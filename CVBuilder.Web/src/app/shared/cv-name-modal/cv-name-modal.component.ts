import { Component, EventEmitter, Output } from '@angular/core';

@Component({
  selector: 'app-cv-name-modal',
  standalone: false,
  templateUrl: './cv-name-modal.component.html',
  styleUrl: './cv-name-modal.component.css',
})
export class CvNameModalComponent {
  cvName: string = '';

  @Output() onSave = new EventEmitter<string>();
  @Output() onCancel = new EventEmitter<void>();

  confirm() {
    console.log('[Modal] Save clicked. Emitting:', this.cvName);
    if (this.cvName.trim()) {
      this.onSave.emit(this.cvName);
    }
  }

  cancel() {
    this.onCancel.emit();
  }
}
