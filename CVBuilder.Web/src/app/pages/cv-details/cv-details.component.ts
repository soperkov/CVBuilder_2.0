import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize } from 'rxjs';
import { CVService } from '../../services/cv.service';
import { Cv } from '../../models';
import { toLocalDate } from '../../utils/date.utils';

type CvDisplay = Cv & { localTime: Date | null };

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

  constructor(
    private route: ActivatedRoute,
    private cvService: CVService,
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

        this.cv = {
          ...res,
          skills: skillObjs,
          localTime: toLocalDate(res.createdAt), // ključ je createdAt
        } as CvDisplay;

        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load CV details.';
        this.loading = false;
      },
    });
  }

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
