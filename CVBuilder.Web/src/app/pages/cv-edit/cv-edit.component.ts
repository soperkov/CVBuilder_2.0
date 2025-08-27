import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CVService } from '../../services/cv.service';
import { CVFormComponent } from '../../components/cv-form/cv-form.component';
import { CreateCvDto, Cv, UpdateCvDto } from '../../models';

@Component({
  selector: 'app-cv-edit',
  standalone: false,
  templateUrl: './cv-edit.component.html',
  styleUrls: ['./cv-edit.component.css'],
})
export class CVEditComponent implements OnInit {
  @ViewChild(CVFormComponent) cvFormComponent!: CVFormComponent;

  cvId!: number;
  loadedCv: Cv | null = null;

  isLoading = true;
  loadError = '';
  isFormValid = false;
  isFormDirty = false;

  showSaveAsModal = false;
  saving = false;
  savingAs = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private cvService: CVService
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    this.cvId = Number(idParam);

    this.cvService.getCVById(this.cvId).subscribe({
      next: (cv: Cv) => {
        this.loadedCv = cv;
        this.isLoading = false;
      },
      error: () => {
        this.loadError = 'Failed to load CV data.';
        this.isLoading = false;
      },
    });
  }

  // form events from child
  onFormValidityChanged(isValid: boolean) {
    this.isFormValid = isValid;
    // čitamo dirty sa child forme (može biti undefined dok se ne inicijalizira)
    this.isFormDirty = !!this.cvFormComponent?.cvForm?.dirty;
  }

  // ako netko pritisne submit unutar child forme (Enter u inputu)
  onFormSubmitted(_: CreateCvDto) {
    this.save();
  }

  get canSave(): boolean {
    return (
      this.isFormValid && this.isFormDirty && !this.isLoading && !this.saving
    );
  }
  get canSaveAs(): boolean {
    return this.isFormValid && !this.isLoading && !this.savingAs;
  }

  // --- Save (UPDATE postojeće) ---
  save(): void {
    if (!this.loadedCv || !this.cvFormComponent || !this.canSave) return;

    this.saving = true;

    const formDto = this.cvFormComponent.buildCreateDto();
    const dto: UpdateCvDto = {
      ...formDto,
      // cvName nije u formi: zadržavamo postojeći naziv
      cvName: this.loadedCv.cvName || '',
    };

    this.cvService.updateCV(this.cvId, dto).subscribe({
      next: () => {
        this.router.navigate(['/my-cvs']);
      },
      error: () => {
        this.saving = false;
        alert('Failed to update CV.');
      },
    });
  }

  // --- Save As (CREATE novi) ---
  openSaveAsModal(): void {
    if (!this.canSaveAs) return;
    this.showSaveAsModal = true;
  }
  closeSaveAsModal(): void {
    this.showSaveAsModal = false;
  }

  handleSaveAs(newName: string): void {
    const trimmed = (newName || '').trim();
    if (!this.cvFormComponent || !trimmed) return;

    this.savingAs = true;

    const formDto = this.cvFormComponent.buildCreateDto();
    const create: CreateCvDto = {
      ...formDto,
      cvName: trimmed,
    };

    this.cvService.createCV(create).subscribe({
      next: (newId) => {
        this.savingAs = false;
        this.showSaveAsModal = false;
        this.router.navigate(['/cv', newId]);
      },
      error: () => {
        this.savingAs = false;
        this.showSaveAsModal = false;
        alert('Failed to create new CV.');
      },
    });
  }
}
