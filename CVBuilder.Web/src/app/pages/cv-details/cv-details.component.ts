import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize } from 'rxjs';
import { CVService } from '../../services/cv.service';
import { Cv } from '../../models';
import { toLocalDate } from '../../utils/date.utils';
import { TemplateService } from '../../services/template.service';

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
export class CVDetailsComponent implements OnInit, OnDestroy {
  cvId!: number;
  cv!: CvDisplay | null;
  loading = true;
  error = '';

  showDeleteModal = false;
  isDeleting = false;

  private styleEl: HTMLStyleElement | null = null;

  constructor(
    private route: ActivatedRoute,
    private cvService: CVService,
    private templateService: TemplateService,
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
        // skills mogu biti string[] ili { name: string }[]
        const skillObjs = Array.isArray(res.skills)
          ? res.skills.map((s: any) =>
              typeof s === 'string' ? { name: s } : s
            )
          : [];

        // uzmi created/modified iz više mogućih polja
        const createdRaw = res.createdAt ?? res.createdAtUtc ?? null;
        const modifiedRaw =
          res.modifiedAt ?? res.updatedAt ?? res.updatedAtUtc ?? createdRaw;

        this.cv = {
          ...res,
          skills: skillObjs,
          localCreated: toLocalDate(createdRaw),
          localModified: toLocalDate(modifiedRaw),
        } as CvDisplay;

        // ako postoji templateId — učitaj CSS iz backenda i injektaj u <head>
        this.applyTemplateCss(this.cv.templateId ?? null);

        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load CV details.';
        this.loading = false;
      },
    });
  }

  ngOnDestroy(): void {
    this.removeTemplateCss();
  }

  // ===== Template CSS injekcija / čišćenje =====
  private applyTemplateCss(templateId: number | null) {
    this.removeTemplateCss();
    if (!templateId) return;

    this.templateService.getById(templateId).subscribe({
      next: (t) => {
        if (!t?.cssContent) return;
        const el = document.createElement('style');
        el.setAttribute('data-cv-template', String(templateId));
        el.textContent = t.cssContent;
        document.head.appendChild(el);
        this.styleEl = el;
      },
      error: () => {
        // ne ruši UI ako CSS fali
      },
    });
  }

  private removeTemplateCss() {
    if (this.styleEl?.parentNode) {
      this.styleEl.parentNode.removeChild(this.styleEl);
    }
    this.styleEl = null;
  }

  // ===== PDF download =====
  exportPdf() {
    if (!this.cvId) return;

    this.cvService.downloadPdf(this.cvId).subscribe({
      next: (res: any) => {
        // podrži oba scenarija:
        // 1) servis vraća samo Blob
        // 2) servis vraća HttpResponse<Blob> s headerima
        let blob: Blob | null = null;
        let fileName = (this.cv?.cvName?.trim() || `CV_${this.cvId}`) + '.pdf';

        if (res instanceof Blob) {
          blob = res;
        } else if (res?.body instanceof Blob) {
          blob = res.body as Blob;
          const dispo = res.headers?.get?.('content-disposition');
          if (dispo) {
            const m = /filename\*?=(?:UTF-8''|")?([^;"']+)/i.exec(dispo);
            if (m && m[1]) {
              try {
                const decoded = decodeURIComponent(m[1].replace(/"/g, ''));
                fileName = decoded.toLowerCase().endsWith('.pdf')
                  ? decoded
                  : decoded + '.pdf';
              } catch {
                /* ignore decode failure */
              }
            }
          }
        }

        if (!blob) return;

        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        a.click();
        URL.revokeObjectURL(url);
      },
      error: () => {
        this.error = 'Failed to export PDF.';
      },
    });
  }

  // ===== Delete modal =====
  openDeleteModal() {
    this.showDeleteModal = true;
  }
  closeDeleteModal() {
    this.showDeleteModal = false;
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
