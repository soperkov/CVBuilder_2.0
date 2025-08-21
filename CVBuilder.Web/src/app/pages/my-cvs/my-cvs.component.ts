import { Component, OnInit } from '@angular/core';
import { CVService } from '../../services/cv.service';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-my-cvs',
  standalone: false,
  templateUrl: './my-cvs.component.html',
  styleUrls: ['./my-cvs.component.css'],
})
export class MyCVsComponent implements OnInit {
  cvs: any[] = [];
  loading = true;
  error = '';

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
      next: (res) => {
        this.cvs = res;
        this.loading = false;
        this.exitSelectionMode();
      },
      error: () => {
        this.error = 'Failed to load CVs';
        this.loading = false;
      },
    });
  }

  viewDetails(id: number) {
    if (this.selectionMode) return;
    this.router.navigate(['/cv', id]);
  }

  editCV(id: number) {
    if (this.selectionMode) return;
    this.router.navigate(['/cv/edit', id]);
  }

  toggleSelectionMode(): void {
    this.selectionMode = !this.selectionMode;
    if (!this.selectionMode) this.selectedIds.clear();
  }

  exitSelectionMode(): void {
    this.selectionMode = false;
    this.selectedIds.clear();
  }

  isSelected(id: number): boolean {
    return this.selectedIds.has(id);
  }

  toggleItem(id: number, $event?: MouseEvent): void {
    if ($event) $event.stopPropagation();
    if (!this.selectionMode) return;
    this.selectedIds.has(id)
      ? this.selectedIds.delete(id)
      : this.selectedIds.add(id);
  }

  deleteSelected(): void {
    if (!this.selectionMode || this.selectedIds.size === 0) return;

    const ids = Array.from(this.selectedIds);
    const confirmText =
      ids.length === 1 ? 'Delete this CV?' : `Delete ${ids.length} CVs?`;
    if (!window.confirm(confirmText)) return;

    this.cvService.deleteMany(ids).subscribe({
      next: () => this.load(),
      error: () => (this.error = 'Failed to delete selected CV(s).'),
    });
  }

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
        this.ngOnInit(); // reload
      },
      error: () => {
        this.error = 'Failed to delete selected CV(s).';
        this.closeDeleteModal();
      },
    });
  }

  trackById = (_: number, item: any) => item.id;
}
