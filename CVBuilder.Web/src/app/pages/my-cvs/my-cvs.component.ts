import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CVService } from '../../services/cv.service';
import { Cv } from '../../models';
import { toLocalDate } from '../../utils/date.utils';

type CvDisplay = Cv & {
  localTime: Date | null;
  templateName?: string;
};

@Component({
  selector: 'app-my-cvs',
  standalone: false,
  templateUrl: './my-cvs.component.html',
  styleUrls: ['./my-cvs.component.css'],
})
export class MyCVsComponent implements OnInit {
  cvs: CvDisplay[] = [];
  loading = true;
  error = '';

  // selection state
  selectionMode = false;
  selectedIds = new Set<number>();
  showDeleteModal = false;

  constructor(private cvService: CVService, private router: Router) {}

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading = true;
    this.cvService.getMyCVs().subscribe({
      next: (res: Cv[]) => {
        this.cvs = (res || [])
          .map((cv: any) => ({
            ...cv,
            localTime: toLocalDate(cv.createdAt),
            templateName: cv.templateName ?? cv.template?.name,
          }))
          .sort(
            (a, b) =>
              (b.localTime?.getTime() ?? 0) - (a.localTime?.getTime() ?? 0)
          );
        this.loading = false;
        this.selectionMode = false;
        this.selectedIds.clear();
      },
      error: () => {
        this.error = 'Failed to load CVs';
        this.loading = false;
      },
    });
  }

  // navigation
  viewDetails(id: number) {
    if (!this.selectionMode) this.router.navigate(['/cv', id]);
  }
  editCV(id: number) {
    if (!this.selectionMode) this.router.navigate(['/cv/edit', id]);
  }

  // selection
  toggleSelectionMode(): void {
    this.selectionMode = !this.selectionMode;
    if (!this.selectionMode) this.selectedIds.clear();
  }
  isSelected(id: number): boolean {
    return this.selectedIds.has(id);
  }
  toggleItem(id: number, ev?: MouseEvent): void {
    ev?.stopPropagation();
    if (!this.selectionMode) return;
    this.selectedIds.has(id)
      ? this.selectedIds.delete(id)
      : this.selectedIds.add(id);
  }

  // delete
  openDeleteModal(): void {
    if (this.selectedIds.size > 0) this.showDeleteModal = true;
  }
  closeDeleteModal(): void {
    this.showDeleteModal = false;
  }
  confirmDelete(): void {
    const ids = Array.from(this.selectedIds);
    this.cvService.deleteMany(ids).subscribe({
      next: () => {
        this.closeDeleteModal();
        this.selectedIds.clear();
        this.selectionMode = false;
        this.load();
      },
      error: () => {
        this.error = 'Failed to delete selected CV(s).';
        this.closeDeleteModal();
      },
    });
  }

  trackById = (_: number, item: CvDisplay) => item.id;
}
