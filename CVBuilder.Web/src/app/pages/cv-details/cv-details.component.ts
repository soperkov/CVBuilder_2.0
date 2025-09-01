import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize } from 'rxjs';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { CVService } from '../../services/cv.service';
import { Cv } from '../../models';
import { toLocalDate } from '../../utils/date.utils';
import { environment } from '../../environments/environment';

type CvDisplay = Cv & {
  localCreated: Date | null;
  localModified: Date | null;
};

@Component({
  selector: 'app-cv-details',
  standalone: false,
  templateUrl: './cv-details.component.html',
  styleUrls: ['./cv-details.component.css'],
})
export class CVDetailsComponent implements OnInit {
  cvId!: number;
  cv!: CvDisplay | null;
  loading = true;
  error = '';

  showDeleteModal = false;
  isDeleting = false;
  pdfLoading = true;

  pdfSafeUrl: SafeResourceUrl | null = null; // koristimo samo ovo u <iframe>

  constructor(
    private route: ActivatedRoute,
    private cvService: CVService,
    private sanitizer: DomSanitizer,
    public router: Router
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    this.cvId = Number(idParam);
    if (!Number.isFinite(this.cvId)) {
      this.error = 'Invalid CV id.';
      this.loading = false;
      return;
    }

    this.cvService.getCVById(this.cvId).subscribe({
      next: (res: any) => {
        const skillObjs = Array.isArray(res.skills)
          ? res.skills.map((s: any) =>
              typeof s === 'string' ? { name: s } : s
            )
          : [];

        const createdRaw = res.createdAt ?? res.createdAtUtc ?? null;
        const modifiedRaw =
          res.modifiedAt ?? res.updatedAt ?? res.updatedAtUtc ?? createdRaw;

        this.cv = {
          ...res,
          skills: skillObjs,
          localCreated: toLocalDate(createdRaw),
          localModified: toLocalDate(modifiedRaw),
        } as CvDisplay;

        // Ticket za PREVIEW (inline)
        this.cvService.getPdfTicket(this.cvId).subscribe({
          next: ({ token }) => {
            const inlineUrl = `${environment.apiBaseUrl}/cv/pdf-inline/${token}`;
            this.pdfSafeUrl =
              this.sanitizer.bypassSecurityTrustResourceUrl(inlineUrl);
            this.loading = false;
          },
          error: () => {
            this.error = 'Failed to get PDF ticket.';
            this.loading = false;
          },
        });
      },
      error: () => {
        this.error = 'Failed to load CV details.';
        this.loading = false;
      },
    });
  }

  exportPdf() {
    this.cvService.getPdfTicket(this.cvId).subscribe({
      next: ({ token }) => {
        const downloadUrl = `${environment.apiBaseUrl}/cv/pdf/${token}`;
        const a = document.createElement('a');
        a.href = downloadUrl;
        a.rel = 'noopener';
        a.target = '_self';
        a.click();
      },
      error: () => {
        this.error = 'Failed to get download link.';
      },
    });
  }

  openDeleteModal() {
    this.showDeleteModal = true;
  }
  closeDeleteModal() {
    this.showDeleteModal = false;
  }

  onPdfLoaded() {
    this.pdfLoading = false;
  }

  confirmDelete() {
    this.isDeleting = true;
    this.cvService
      .deleteCV(this.cvId)
      .pipe(
        finalize(() => {
          this.isDeleting = false;
          this.closeDeleteModal();
        })
      )
      .subscribe({
        next: () => this.router.navigate(['/my-cvs']),
        error: () => {
          this.error = 'Failed to delete CV.';
        },
      });
  }
}
